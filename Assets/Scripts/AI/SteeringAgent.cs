using System;
using System.Collections.Generic;
using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    [Header("Movement")] 
    public float maxSpeed = 5f;
    public float maxForce = 10f; // turning radius
    
    [Header("Arrive")]
    public float slowingRadius = 3f;
    
    [Header("Separation")]
    public float separationRadius = 1.5f;
    public float separationStrength = 5f;

    [Header("Weights")] 
    public float arriveWeight = 1f;
    public float separationWeight = 1f;
    
    [Header("Debug")]
    public bool drawDebug = true;
    
    private Vector3 velocity = Vector3.zero;
    
    public Transform target;
    
    public static List<SteeringAgent> allAgents = new List<SteeringAgent>();

    private void OnEnable()
    {
        allAgents.Add(this);
    }

    private void OnDisable()
    {
        allAgents.Remove(this);
    }
    
    void Start()
    {
        target = GameObject.Find("Target").transform;
    }
    
    void Update()
    {
        // Steering force
        Vector3 totalSteerng = Vector3.zero;

        if (target != null)
        {
            totalSteerng += Arrive(target.position, slowingRadius) * arriveWeight;
        }

        if (allAgents.Count > 1)
        {
            totalSteerng += Separation(separationRadius, separationStrength) * separationWeight;
        }
        
        // 2. Limit Steering (Truncate)
        // This prevents the agent from turning instantly.
        totalSteerng = Vector3.ClampMagnitude(totalSteerng, maxSpeed);
        
        // 3. Apply Steering to Velocity (Integration)
        // Acceleration = Force / Mass. (We assume Mass = 1).
        // Velocity Change = Acceleration * Time.
        velocity += totalSteerng  * Time.deltaTime;
        
        // 4. Limit Velocity
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        
        // 5. Move Agent
        transform.position += velocity * Time.deltaTime;
        
        // 6. Face Movement Direction
        if (velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = velocity.normalized;
        }
    }
    
    public Vector3 Arrive(Vector3 targetPos, float slowRadius) 
    {
        
        // The core idea (Reynolds’ steering formula):
        // Steering Force = Desired Velocity – Current Velocity
        //     ● Desired Velocity:
        //     ○ A vector pointing from you to the target,
        //     ○ Normalized (direction only),
        //     ○ Then multiplied by maxSpeed.
        //     ● Steering:
        //     ○ The difference between where you want to be heading (desired) and where
        // you are currently heading (velocity).
        
        Vector3 toTarget = targetPos - transform.position;
        float distance = toTarget.magnitude;
        
        //if we are already there, stop steering
        if (distance < 0.0001f)
        {
            return Vector3.zero;
        }

        float desiredSpeed = maxSpeed;
        
        // ramp down speed if within radius
        if (distance < slowRadius)
        {
            desiredSpeed = maxSpeed * (distance / slowRadius);
        }

        Vector3 desiredVelocity = toTarget.normalized * desiredSpeed;
        return desiredVelocity - velocity;
    }

    public Vector3 Separation(float radius, float strength)
    {
        Vector3 force = Vector3.zero;
        int neighbourCount = 0;
        foreach (SteeringAgent other in allAgents)
        {
            if (other == this)
            {
                continue;
            }
            
            Vector3 toMe = transform.position - other.transform.position;
            float distance = toMe.magnitude;
            
            // if they are within my personal space
            if (distance > 0f && distance < radius)
            {
                // Weight 1/dist means closer neighbours push MUCH harder
                force += toMe.normalized / distance;
                neighbourCount++;
            }
        }

        if (neighbourCount > 0)
        {
            force /= neighbourCount; // average direction
            
            // convert "move away" direction into a steering force
            force = force.normalized * maxSpeed;
            force -= velocity;
            force *= strength;
        }
        return force;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebug)
        {
            return;
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
    }
}
