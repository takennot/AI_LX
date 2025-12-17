using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEditor;

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Clear Target", description: "Clears Target and resets LOS memory.", story: "Forget the target and reset perception flags.", category: "Action/Sensing", id: "fe18d4316690a8022fd5d50b93f65a9e")]
    public partial class ClearTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<bool> HasLineOfSight;
        [SerializeReference] public BlackboardVariable<float> TimeSinceLastSeen;
        [SerializeReference] public BehaviorGraphAgent bgAgent;

        protected override Status OnStart()
        {
            bgAgent = GameObject.FindGameObjectWithTag("Guard").GetComponent<BehaviorGraphAgent>();
        
            return Status.Running;
        }
    
        protected override Status OnUpdate()
        {
            if (Target != null)
            {
                Target.Value = null;
                bgAgent.SetVariableValue("Target", Target.Value);
            }

            if (HasLineOfSight != null)
            {
                bgAgent.SetVariableValue("HasLineOfSight", false);
                HasLineOfSight.Value = false;
            }

            if (TimeSinceLastSeen != null)
            {
                bgAgent.SetVariableValue("TimeSinceLastSeen", 9999f);
                TimeSinceLastSeen.Value = 9999f;
            }
            return Status.Success;
        }
    }
