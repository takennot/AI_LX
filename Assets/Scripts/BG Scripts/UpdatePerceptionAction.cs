using System;
using L4;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Perception", description: "Updates Target/LOS/LastKnownPosition from GuardSensors.", story: "Update perception and write to the blackboard.", category: "Action/Sensing", id: "87087d7c1d8dc466b5dcb9cb05a17294")]
public class UpdatePerceptionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPosition;
    [SerializeReference] public BlackboardVariable<float> TimeSinceLastSeen;
    [SerializeReference] public BehaviorGraphAgent bgAgent;
    private Vector3 cachedLastKnownPosition;
    
    protected override Status OnStart()
    {
        bgAgent = GameObject.FindGameObjectWithTag("Guard").GetComponent<BehaviorGraphAgent>();
        if (TimeSinceLastSeen != null && TimeSinceLastSeen.Value < 0f)
        {
            bgAgent.SetVariableValue("TimeSinceLastSeen", 9999f);
            TimeSinceLastSeen.Value = 9999f;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        
        var sensors = GameObject != null ? GameObject.GetComponent<GuardSensors>() : null;
        if (sensors == null)
        {
            if (HasLineOfSight != null)
            {
                bgAgent.SetVariableValue("HasLineOfSight", false);
                HasLineOfSight.Value = false;
            }

            if (TimeSinceLastSeen != null)
            {
                TimeSinceLastSeen.Value += Time.deltaTime;
                bgAgent.SetVariableValue("TimeSinceLastSeen", TimeSinceLastSeen.Value);
            }
            return Status.Failure;
        }
        
        bool previouslHadLOS = HasLineOfSight.Value;
        bool sensed = sensors.TrySenseTarget(out GameObject sensedTarget, out Vector3 sensedPos, out bool hasLOS);

        if (sensed && hasLOS)
        {
            cachedLastKnownPosition = sensedPos;
            if (Target != null)
            {
                bgAgent.SetVariableValue("Target", sensedTarget);
                Target.Value = sensedTarget;
            }

            if (HasLineOfSight != null)
            {
                bgAgent.SetVariableValue("HasLineOfSight", true); // this actually changes variable
                HasLineOfSight.Value = true; // this one doesn't????????????????
            }

            // if (LastKnownPosition != null)
            // {
            //     bgAgent.SetVariableValue("LastKnownPosition", sensedPos);
            //     LastKnownPosition.Value = sensedPos;
            // }

            if (TimeSinceLastSeen != null)
            {
                bgAgent.SetVariableValue("TimeSinceLastSeen", 0f);
                TimeSinceLastSeen.Value = 0f;
            }
        }
        else
        {
            if (previouslHadLOS)
            {
                if (LastKnownPosition != null)
                {
                    LastKnownPosition.Value = cachedLastKnownPosition;
                    bgAgent.SetVariableValue("LastKnownPosition", cachedLastKnownPosition);
                }
            }
            
            if (HasLineOfSight != null)
            {
                bgAgent.SetVariableValue("HasLineOfSight", false);
                HasLineOfSight.Value = false;
            }

            if (TimeSinceLastSeen != null)
            {
                TimeSinceLastSeen.Value += Time.deltaTime;
                bgAgent.SetVariableValue("TimeSinceLastSeen", TimeSinceLastSeen.Value);
            }

            return Status.Failure;
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


