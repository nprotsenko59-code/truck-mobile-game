using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TruckGame.Input
{
    public sealed class SteeringInput : MonoBehaviour
    {
        public float Value { get; private set; }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            Value = 0f;
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (Touch.activeTouches.Count > 0)
            {
                Value = Touch.activeTouches[0].screenPosition.x < Screen.width * 0.5f ? -1f : 1f;
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Value = Mouse.current.position.ReadValue().x < Screen.width * 0.5f ? -1f : 1f;
                return;
            }

            var keyboardValue = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                {
                    keyboardValue -= 1f;
                }

                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                {
                    keyboardValue += 1f;
                }
            }

            Value = Mathf.Clamp(keyboardValue, -1f, 1f);
        }
    }
}

