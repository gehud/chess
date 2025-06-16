using UnityEngine;

namespace Chess
{
    public class CameraRotation : MonoBehaviour
    {
        [SerializeField]
        private float sencitivity = 2.0f;
        [SerializeField]
        private float smoothTime = 0.1f;

        private bool isDragging = false;
        private Vector3 delta = Vector3.zero;
        private Vector3 targetDelta = Vector3.zero;
        private Vector3 deltaVelocity = Vector3.zero;

        private Controls controls;

        private void Awake()
        {
            controls = new();
            controls.Player.Hold.started += (ctx) => isDragging = true;
            controls.Player.Hold.canceled += (ctx) => isDragging = false;

            transform.LookAt(Vector3.zero);
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }

        private void Update()
        {
            if (isDragging)
            {
                targetDelta = controls.Player.Drag.ReadValue<Vector2>();
            }
            else
            {
                targetDelta = Vector3.zero;
            }

            delta = Vector3.SmoothDamp(delta, targetDelta, ref deltaVelocity, smoothTime);
            transform.RotateAround(Vector3.zero, Vector3.up, delta.x * sencitivity);
            transform.RotateAround(Vector3.zero, transform.right, -delta.y * sencitivity);
        }
    }
}
