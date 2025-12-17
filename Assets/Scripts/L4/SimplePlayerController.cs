using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool faceMoveDirection = true;
    
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 input = new Vector3(x, 0, z);
        if (input.sqrMagnitude < 0.0001f)
        {
            return;
        }
        
        Vector3 dir = input.normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);

        if (faceMoveDirection)
        {
            transform.forward = dir;
        }
    }
}
