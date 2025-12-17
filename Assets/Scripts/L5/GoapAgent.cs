using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace L5
{
    public class GoapAgent : MonoBehaviour
    {
        [Header("Scene refs")]
        public GuardSensors sensors;
        public Transform player;
        public Transform weaponPickup;
        public Transform[] patrolWaypoints;

        [Header("Debug")] 
        public bool logPlans = true;
        
        [Header("Planning")]
        [Tooltip("Minimum seconds between replans (prevents spam when facts flicker)")]
        public float minSecondsBetweenReplans = 0.20f;

        private float _nextAllowedReplanTime = 0f;
        
        private NavMeshAgent _agent;
        private GoapContext _ctx;

        private List<GoapActionBase> _allActions;
        private Queue<GoapActionBase> _plan;
        private GoapActionBase _currentAction;
        
        // “Owned” facts: memory/execution facts (e.g., HasWeapon, AtWeapon, AtPlayer, PatrolStepDone, PlayerTagged)
        // Sensor/world facts (SeesPlayer, WeaponExists) are refreshed each tick.
        private ulong _ownedFactsBits = 0;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            _ctx = new GoapContext
            {
                Agent = _agent,
                Player = player,
                Weapon = weaponPickup,
                PatrolWaypoints = patrolWaypoints,
                Sensors = sensors,
                PatrolIndex = 0
            };

            _allActions = new List<GoapActionBase>(GetComponents<GoapActionBase>());
        }

        // Update is called once per frame
        void Update()
        {
            GoapState current = BuildCurrentState();
            ulong goalMask = SelectGoalMask(current);

            if ((_plan == null || _plan.Count == 0) && Time.time >= _nextAllowedReplanTime)
            {
                MakePlan(current, goalMask);
            }

            if (_plan == null || _plan.Count == 0)
            {
                return;
            }

            if (_currentAction == null)
            {
                _currentAction = _plan.Dequeue();

                if (!_currentAction.CheckProcedural(_ctx))
                {
                    InvalidatePlan(throttle: true);
                    return;
                }
                _currentAction.OnEnter(_ctx);
            }
            var status = _currentAction.Tick(_ctx);
            switch (status)
            {
                case GoapStatus.Running:
                    return;
                case GoapStatus.Success:
                    ApplyActionEffectsToOwnedFacts(_currentAction);
                    _currentAction.OnExit(_ctx);
                    _currentAction = null;
                    return;
            }
            _currentAction.OnExit(_ctx);
            _currentAction = null;
            InvalidatePlan(throttle: true);
        }

        private GoapState BuildCurrentState()
        {
            ulong bits = _ownedFactsBits;
            
            bool hasWeapon = (bits & GoapBits.Mask(GoapFact.HasWeapon)) != 0;

            if (sensors != null && sensors.SeesPlayer)
            {
                bits |= GoapBits.Mask(GoapFact.SeesPlayer);
            }
            else
            {
                bits &= ~GoapBits.Mask(GoapFact.SeesPlayer);
            }
            
            bool pickupActive = weaponPickup != null && weaponPickup.gameObject.activeInHierarchy;
            bool weaponAvailable = pickupActive && !hasWeapon;

            if (weaponAvailable)
            {
                bits |= GoapBits.Mask(GoapFact.WeaponExists);
            }
            else
            {
                bits &= ~GoapBits.Mask(GoapFact.WeaponExists);
            }

            return new GoapState(bits);
        }

        private ulong SelectGoalMask(GoapState current)
        {
            if (current.Has(GoapFact.SeesPlayer))
            {
                return GoapBits.Mask(GoapFact.PlayerTagged);
            }

            return GoapBits.Mask(GoapFact.PatrolStepDone);
        }

        private void MakePlan(GoapState current, ulong goalMask)
        {
            var res = GoapPlanner.Plan(current, goalMask, _allActions);
            if (res == null)
            {
                if (logPlans)
                {
                    Debug.LogWarning($"Plan {goalMask} returned null plan");
                }

                _plan = null;
                return;
            }

            _plan = new Queue<GoapActionBase>(res.Actions);

            if (logPlans)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"GOAP Plan (cost {res.TotalCost:0.0}):");
                foreach (var a in res.Actions)
                {
                    sb.AppendLine($"-{a.actionName} (cost {a.cost:0.0})");
                    Debug.Log(sb.ToString());
                }
            }
        }

        private void InvalidatePlan(bool throttle)
        {
            _plan = null;
            _currentAction = null;

            if (throttle)
            {
                _nextAllowedReplanTime = Time.time + minSecondsBetweenReplans;
            }
        }

        private void ApplyActionEffectsToOwnedFacts(GoapActionBase a)
        {
            _ownedFactsBits &= ~a.delMask;
            _ownedFactsBits |= a.addMask;
        }
    }
}