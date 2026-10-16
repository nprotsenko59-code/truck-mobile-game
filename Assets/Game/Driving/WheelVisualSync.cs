using System;
using UnityEngine;

namespace TruckGame.Driving
{
    public sealed class WheelVisualSync : MonoBehaviour
    {
        [Serializable]
        public struct WheelBinding
        {
            public WheelCollider collider;
            public Transform visual;
        }

        [SerializeField] private WheelBinding[] wheels;

        public void Configure(WheelBinding[] bindings)
        {
            wheels = bindings;
        }

        private void LateUpdate()
        {
            if (wheels == null)
            {
                return;
            }

            foreach (var wheel in wheels)
            {
                if (wheel.collider == null || wheel.visual == null)
                {
                    continue;
                }

                wheel.collider.GetWorldPose(out var position, out var rotation);
                wheel.visual.SetPositionAndRotation(position, rotation * Quaternion.Euler(0f, 0f, 90f));
            }
        }
    }
}

