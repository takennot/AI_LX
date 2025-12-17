using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace L4
{
    public class GuardSensors : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetTag = "Player";
    
    [Header("View")]
    [SerializeField] private float viewDistance = 10f;
    [Range(1f, 180f)]
    [SerializeField] private float viewAngleDegrees = 90f;
    
    [Header("Line of Sight")]
    [SerializeField] private Transform eyes;
    [SerializeField] private LayerMask occlusionMask = ~0;
    //[SerializeField] private LayerMask occlusionMask;

    private Transform cachedTarget;
    
    public float ViewDistance => viewDistance;
    public float ViewAngleDegrees => viewAngleDegrees;
    
    private Transform EyesTransform => eyes != null ? eyes : transform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag(targetTag);
        cachedTarget = go != null ? go.transform : null;
        Debug.Log(cachedTarget);
    }

    public bool TrySenseTarget(out GameObject target, out Vector3 lastKnownPosition, out bool hasLineofSight)
    {
        target = null;
        lastKnownPosition = default;
        hasLineofSight = false;

        if (cachedTarget == null)
        {
            return false;
        }

        Vector3 eyePos = EyesTransform.position;
        Vector3 toTarget = cachedTarget.position - transform.position;
        
        float dist = toTarget.magnitude;
        if (dist > viewDistance)
        {
            return false;
        }

        Vector3 toTargetDir = toTarget / Mathf.Max(dist, 0.0001f);
        
        float halfAnlge = viewAngleDegrees * 0.5f;
        float angle = Vector3.Angle(transform.forward, toTargetDir);
        if (angle > halfAnlge)
        {
            return false;
        }
        
        if (Physics.Raycast(transform.position, toTargetDir, out RaycastHit hit, dist, occlusionMask))
        {
            if (hit.transform != cachedTarget)
            {
                return false;
            }
        }

        target = cachedTarget.gameObject;
        lastKnownPosition = cachedTarget.position;
        hasLineofSight = true;
        Debug.DrawRay(eyePos, toTargetDir * dist, hasLineofSight ? Color.green : Color.red);
        return true;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
}
