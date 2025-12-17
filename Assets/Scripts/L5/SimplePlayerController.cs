using UnityEngine;
using UnityEngine.InputSystem;

namespace L5
{
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 5f;

        private Vector2 _moveInput;

        // Update is called once per frame
        void Update()
        {
            Vector3 dir = new Vector3(_moveInput.x, 0f, _moveInput.y);

            if (dir.sqrMagnitude > 1)
            {
                dir.Normalize();
            }
            
            transform.position += dir * (speed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.001f)
            {
                transform.forward = dir;
            }
        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }
    }
}

