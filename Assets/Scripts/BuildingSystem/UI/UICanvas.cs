using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Test.BuildingSystem
{
    public interface ICursorController
    {
        public void SetCursorColor(StateType type);
    }

    public class UICanvas : MonoBehaviour, ICursorController
    {
        [SerializeField] private Image _cursorImg;
        [SerializeField] private RectTransform _cursorTransform;

        public void SetCursorColor(StateType type)
        {
            switch (type)
            {
                case StateType.Delete: _cursorImg.color = Color.red; break;
                case StateType.Edit: _cursorImg.color = Color.blue; break;
                case StateType.Idle:
                case StateType.Build:
                    _cursorImg.color = Color.white;
                    break;
            }
        }

        public void HandleUpdate()
        {
            var mousePos = Mouse.current.position.value;

            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_cursorTransform.parent, mousePos,
                null, out Vector2 localPoint);

            _cursorTransform.anchoredPosition = localPoint;
        }
    }
}