using UnityEngine;
using UnityEngine.AI;

public class GuardPatrol : MonoBehaviour
{
    // if guard has navmeshagent and no baked navmesh - it wont run + error of active navmeshagent
    
    // path is the set of coordinates along which we will move
    // movement along the path is action of continuously changing your position alongside said path
    
    
    // no idea why do we check for !agent.pathfinding
    // no % waypoints.length is probably index out of bounds, no?
    
    // without enum code will look like dogwater and no one wants to maintain that
    public enum GuardState
    {
        Patrolling,
        Chasing,
        ReturningToPatrol
    }
    
    public GuardState currentState = GuardState.Patrolling;
    public Transform player;
    public float chaseRange = 20f;
    public float loseRange = 40f;
    public Transform[] waypoints;
    public float waypointTolerance = 0.5f;

    private int _currentIndex = 0;
    public NavMeshAgent _agent;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (waypoints.Length > 0)
        {
            _agent.SetDestination(waypoints[_currentIndex].position);
        }
    }

    // Update is called once per frame
    void Update()
    {

        switch (currentState)
        {
            case  GuardState.Patrolling:
                UpdatePatrol();
                break;
            case  GuardState.Chasing:
                UpdateChase();
                break;
            case  GuardState.ReturningToPatrol:
                UpdateReturning();
                break;
        }
    }

    void UpdatePatrol()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= waypointTolerance)
        {
            _currentIndex = (_currentIndex + 1) % waypoints.Length;
            _agent.SetDestination(waypoints[_currentIndex].position);
        }

        if (Vector3.Distance(player.position, transform.position) <= chaseRange)
        {
            currentState = GuardState.Chasing;
        }
    }

    void UpdateChase()
    {
        _agent.SetDestination(player.position);
        if (Vector3.Distance(player.position, transform.position) >= loseRange)
        {
            // I dont see any point in finding closest waypoint tbh, guard should continue from where they stopped imo
            
            currentState = GuardState.ReturningToPatrol;
        }
    }

    void UpdateReturning()
    {
        _agent.SetDestination(waypoints[_currentIndex].position);
        currentState = GuardState.Patrolling;
    }
}
