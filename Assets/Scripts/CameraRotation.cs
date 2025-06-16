using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess
{
    public class CameraRotation : MonoBehaviour
    {
        [SerializeField]
        private float sencitivity = 2.0f;
        [SerializeField]
        private float smoothTime = 0.1f;

        private Vector3 mouseDelta = Vector3.zero;
        private Vector3 targetDelta = Vector3.zero;
        private Vector3 deltaVelocity = Vector3.zero;

        private void Awake()
        {
            transform.LookAt(Vector3.zero);
        }

        private void Update()
        {
            if (Mouse.current.rightButton.isPressed)
            {
                targetDelta = new Vector3
                {
                    x = Mouse.current.delta.x.ReadValue(),
                    y = Mouse.current.delta.y.ReadValue(),
                };
            }
            else
            {
                targetDelta = Vector3.zero;
            }

            mouseDelta = Vector3.SmoothDamp(mouseDelta, targetDelta, ref deltaVelocity, smoothTime);

            transform.RotateAround(Vector3.zero, Vector3.up, mouseDelta.x * sencitivity);
            transform.RotateAround(Vector3.zero, transform.right, -mouseDelta.y * sencitivity);
        }
    }
}
