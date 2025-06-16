using UnityEngine;
using UnityEngine.InputSystem;

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

        private void Update()
        {
            targetDelta = Mouse.current.scroll.y.ReadValue();
            delta = Mathf.SmoothDamp(delta, targetDelta, ref deltaVelocity, smoothTime);
            transform.Translate(delta * sensitivity * Vector3.forward);
        }
    }
}
