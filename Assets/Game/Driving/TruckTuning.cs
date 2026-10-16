using UnityEngine;

namespace TruckGame.Driving
{
    [CreateAssetMenu(menuName = "Truck Game/Truck Tuning", fileName = "TruckTuning")]
    public sealed class TruckTuning : ScriptableObject
    {
        [Header("Speed")]
        [Min(1f)] public float targetSpeedMps = 16f;
        [Min(0f)] public float motorTorque = 2400f;
        [Min(0f)] public float overspeedBrakeTorque = 3500f;

        [Header("Steering")]
        [Min(0f)] public float lowSpeedReferenceMps = 5f;
        [Min(0.1f)] public float highSpeedReferenceMps = 22f;
        [Range(1f, 45f)] public float lowSpeedSteerAngle = 28f;
        [Range(1f, 45f)] public float highSpeedSteerAngle = 9f;
        [Min(0.1f)] public float steeringResponsePerSecond = 2.6f;
        [Min(0.1f)] public float steeringRecenterPerSecond = 3.8f;

        [Header("Stability")]
        [Min(0f)] public float antiRollForce = 11000f;
        [Min(0f)] public float downforcePerMps = 180f;
        public Vector3 centerOfMassOffset = new(0f, -0.7f, 0.1f);
    }
}

