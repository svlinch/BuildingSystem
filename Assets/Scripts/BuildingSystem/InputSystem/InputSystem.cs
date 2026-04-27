using UnityEngine;
using UnityEngine.InputSystem;

namespace Test.BuildingSystem
{
    public struct InputData
    {
        public Vector3 StartPosition;
        public Vector3 CurrentPosition;

        public bool ShiftPressed;
        public bool MouseHold;
        public bool MouseDown;
        public bool MouseUp;
    }

    public class InputSystem
    {
        private Vector3 _startPosition;
        private bool _isDragging;

        public InputData HandleUpdate()
        {
            var mouse = Mouse.current;
            if (mouse == null) return default;

            var result = new InputData();

            var mouseWorldPos = GetMouseWorldPosition();

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                result.ShiftPressed = keyboard.shiftKey.isPressed;
            }

            result.CurrentPosition = mouseWorldPos;
            result.StartPosition = _startPosition;

            if (!_isDragging && mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _startPosition = mouseWorldPos;
                result.MouseDown = true;
                result.StartPosition = _startPosition;
            }

            if (_isDragging)
            {
                result.MouseHold = true;

                if (!mouse.leftButton.isPressed)
                {
                    _isDragging = false;
                    result.MouseHold = false;
                    result.MouseUp = true;
                }
            }

            return result;
        }

        private Vector3 GetMouseWorldPosition()
        {
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            var hPlane = new Plane(Vector3.up, Vector3.zero);

            if (hPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            return Vector3.zero;
        }
    }
}