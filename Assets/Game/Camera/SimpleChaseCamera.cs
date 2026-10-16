using UnityEngine;

namespace TruckGame.CameraSystem
{
    public sealed class SimpleChaseCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new(0f, 6.5f, -10.5f);
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.2f;
        [SerializeField, Min(0.1f)] private float rotationSharpness = 7f;
        [SerializeField, Min(0f)] private float lookAhead = 5f;

        private Vector3 velocity;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desiredPosition = target.TransformPoint(localOffset);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                positionSmoothTime);

            var lookPoint = target.position + target.forward * lookAhead + Vector3.up;
            var desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }
    }
}

