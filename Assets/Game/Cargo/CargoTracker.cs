using System;
using UnityEngine;

namespace TruckGame.Cargo
{
    public enum CargoConditionState
    {
        Secure,
        Damaged,
        Critical
    }

    [RequireComponent(typeof(Rigidbody))]
    public sealed class CargoTracker : MonoBehaviour
    {
        [SerializeField] private string cargoName = "Hotel Supplies";
        [SerializeField, Range(0f, 100f)] private float conditionPercent = 100f;
        [SerializeField, Min(0f)] private float impactThresholdMps = 4f;
        [SerializeField, Min(0f)] private float damagePerExcessMps = 5f;
        [SerializeField, Range(0f, 90f)] private float roughTiltThreshold = 22f;
        [SerializeField, Min(0f)] private float roughDrivingDamagePerSecond = 2.5f;
        [SerializeField, Min(0f)] private float minimumRoughDrivingSpeed = 6f;

        private Rigidbody body;
        private bool trackingEnabled = true;
        private float ignoreCollisionsUntil;

        public event Action<float, float> DamageTaken;

        public string CargoName => cargoName;
        public float ConditionPercent => conditionPercent;
        public float Condition01 => conditionPercent / 100f;
        public int CollisionCount { get; private set; }
        public float RoughDrivingSeconds { get; private set; }
        public CargoConditionState State => EvaluateState(conditionPercent);

        public void Configure(
            string displayName,
            float impactThreshold,
            float impactDamageRate,
            float tiltThreshold,
            float tiltDamageRate)
        {
            cargoName = string.IsNullOrWhiteSpace(displayName) ? "Cargo" : displayName;
            impactThresholdMps = Mathf.Max(0f, impactThreshold);
            damagePerExcessMps = Mathf.Max(0f, impactDamageRate);
            roughTiltThreshold = Mathf.Clamp(tiltThreshold, 0f, 90f);
            roughDrivingDamagePerSecond = Mathf.Max(0f, tiltDamageRate);
            ResetCargo();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!trackingEnabled || body == null)
            {
                return;
            }

            var tilt = Vector3.Angle(transform.up, Vector3.up);
            TrackRoughDriving(Time.fixedDeltaTime, tilt, body.linearVelocity.magnitude);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!trackingEnabled || Time.time < ignoreCollisionsUntil)
            {
                return;
            }

            var damage = ApplyImpact(collision.relativeVelocity.magnitude);
            if (damage > 0f)
            {
                ignoreCollisionsUntil = Time.time + 0.3f;
            }
        }

        public float ApplyImpact(float impactSpeedMps)
        {
            if (!trackingEnabled || conditionPercent <= 0f || impactSpeedMps <= impactThresholdMps)
            {
                return 0f;
            }

            CollisionCount++;
            var damage = (impactSpeedMps - impactThresholdMps) * damagePerExcessMps;
            return ApplyDamage(damage, impactSpeedMps);
        }

        public float TrackRoughDriving(float deltaTime, float tiltDegrees, float speedMps)
        {
            if (!trackingEnabled || deltaTime <= 0f || speedMps < minimumRoughDrivingSpeed ||
                tiltDegrees <= roughTiltThreshold)
            {
                return 0f;
            }

            RoughDrivingSeconds += deltaTime;
            var severity = Mathf.InverseLerp(roughTiltThreshold, 60f, tiltDegrees);
            return ApplyDamage(roughDrivingDamagePerSecond * Mathf.Max(0.25f, severity) * deltaTime, speedMps);
        }

        public void SetTrackingEnabled(bool enabled)
        {
            trackingEnabled = enabled;
        }

        public void ResetCargo()
        {
            conditionPercent = 100f;
            CollisionCount = 0;
            RoughDrivingSeconds = 0f;
            trackingEnabled = true;
            ignoreCollisionsUntil = 0f;
        }

        public static CargoConditionState EvaluateState(float condition)
        {
            if (condition <= 35f)
            {
                return CargoConditionState.Critical;
            }

            return condition < 75f ? CargoConditionState.Damaged : CargoConditionState.Secure;
        }

        private float ApplyDamage(float requestedDamage, float severity)
        {
            if (requestedDamage <= 0f || conditionPercent <= 0f)
            {
                return 0f;
            }

            var previous = conditionPercent;
            conditionPercent = Mathf.Max(0f, conditionPercent - requestedDamage);
            var appliedDamage = previous - conditionPercent;
            DamageTaken?.Invoke(appliedDamage, severity);
            return appliedDamage;
        }
    }
}
