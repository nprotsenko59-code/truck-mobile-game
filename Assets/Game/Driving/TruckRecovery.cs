using UnityEngine;
using UnityEngine.InputSystem;

namespace TruckGame.Driving
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TruckRecovery : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float upsideDownDelay = 2.5f;
        [SerializeField, Min(0.1f)] private float liftDistance = 1.5f;

        private Rigidbody body;
        private float upsideDownTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var manuallyRequested = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            var isUpsideDown = Vector3.Dot(transform.up, Vector3.up) < 0.25f;

            upsideDownTime = isUpsideDown ? upsideDownTime + Time.deltaTime : 0f;
            if (manuallyRequested || upsideDownTime >= upsideDownDelay)
            {
                Recover();
            }
        }

        public void Recover()
        {
            var yaw = transform.eulerAngles.y;
            body.position += Vector3.up * liftDistance;
            body.rotation = Quaternion.Euler(0f, yaw, 0f);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            upsideDownTime = 0f;
        }
    }
}

