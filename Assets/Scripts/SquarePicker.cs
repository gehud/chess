using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess
{
    public class SquarePicker : MonoBehaviour
    {
        public delegate void SelectedHandler(Square square);
        public static SelectedHandler Selected;

        public delegate void DeselectedHandler();
        public static DeselectedHandler Deselected;

        private Controls controls;

        private void OnPress(InputAction.CallbackContext ctx)
        {
            var ray = Camera.main.ScreenPointToRay(ctx.ReadValue<Vector2>());

            if (!Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Deselected?.Invoke();
                return;
            }

            var point = hitInfo.point;
            point += new Vector3
            {
                x = 0.5f,
                y = 0f,
                z = 0.5f,
            };

            var square = Vector3Int.FloorToInt(point);
            if (square.x < 0 || square.x >= Board.Size || square.z < 0 || square.z >= Board.Size)
            {
                Deselected?.Invoke();
                return;
            }

            Selected?.Invoke(new(square.x, square.z));
        }

        private void Awake()
        {
            controls = new();
            controls.Player.Pick.performed += OnPress;
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }
    }
}
