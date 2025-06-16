using UnityEngine;

namespace Chess
{
    public class CameraZoom : MonoBehaviour
    {
        [SerializeField]
        private float sensitivity = 5.0f;
        [SerializeField]
        private float smoothTime = 0.1f;

        private float delta = 0.0f;
        private float targetDelta = 0.0f;
        private float deltaVelocity = 0.0f;

        private Controls controls;

        private void Awake()
        {
            controls = new();
            controls.Player.Zoom.performed += (ctx) => targetDelta = ctx.ReadValue<float>();
            controls.Player.Zoom.canceled += (ctx) => targetDelta = 0f;
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
            delta = Mathf.SmoothDamp(delta, targetDelta, ref deltaVelocity, smoothTime);
            transform.Translate(delta * sensitivity * Vector3.forward);
        }
    }
}
