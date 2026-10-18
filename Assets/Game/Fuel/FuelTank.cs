using System;
using UnityEngine;

namespace TruckGame.Fuel
{
    public enum FuelState
    {
        Normal,
        Low,
        Critical,
        Empty
    }

    [RequireComponent(typeof(Rigidbody))]
    public sealed class FuelTank : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float capacity = 100f;
        [SerializeField, Min(0f)] private float startingFuel = 52f;
        [SerializeField, Min(0f)] private float consumptionPerSecond = 3.2f;
        [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.35f;
        [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.15f;
        [SerializeField] private float currentFuel;

        private Rigidbody body;
        private FuelState previousState;

        public event Action<float> FuelChanged;
        public event Action<FuelState> StateChanged;
        public event Action<float> Refueled;

        public float Capacity => capacity;
        public float CurrentFuel => currentFuel;
        public float NormalizedFuel => capacity <= 0f ? 0f : currentFuel / capacity;
        public float TotalConsumed { get; private set; }
        public float TotalRefueled { get; private set; }
        public bool HasFuel => currentFuel > 0.001f;
        public bool RunFailed => State == FuelState.Empty;
        public FuelState State => EvaluateState(NormalizedFuel, lowThreshold, criticalThreshold);

        public void Configure(
            float fuelCapacity,
            float initialFuel,
            float burnRate,
            float lowFuelThreshold,
            float criticalFuelThreshold)
        {
            capacity = Mathf.Max(1f, fuelCapacity);
            startingFuel = Mathf.Clamp(initialFuel, 0f, capacity);
            consumptionPerSecond = Mathf.Max(0f, burnRate);
            lowThreshold = Mathf.Clamp01(lowFuelThreshold);
            criticalThreshold = Mathf.Clamp(criticalFuelThreshold, 0f, lowThreshold);
            currentFuel = startingFuel;
            TotalConsumed = 0f;
            TotalRefueled = 0f;
            previousState = State;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            currentFuel = Mathf.Clamp(startingFuel, 0f, capacity);
            previousState = State;
        }

        private void FixedUpdate()
        {
            ConsumeFor(Time.fixedDeltaTime, body == null ? 0f : body.linearVelocity.magnitude);
        }

        public void ConsumeFor(float deltaTime, float speedMetresPerSecond)
        {
            if (!HasFuel || deltaTime <= 0f || speedMetresPerSecond < 0.5f)
            {
                return;
            }

            var speedFactor = Mathf.Clamp(speedMetresPerSecond / 16f, 0.25f, 1.25f);
            Consume(consumptionPerSecond * speedFactor * deltaTime);
        }

        public void Consume(float amount)
        {
            if (amount <= 0f || !HasFuel)
            {
                return;
            }

            var consumed = Mathf.Min(amount, currentFuel);
            TotalConsumed += consumed;
            SetFuel(currentFuel - consumed);
        }

        public float RefuelToFull()
        {
            var addedFuel = capacity - currentFuel;
            if (addedFuel <= 0.001f)
            {
                return 0f;
            }

            SetFuel(capacity);
            TotalRefueled += addedFuel;
            Refueled?.Invoke(addedFuel);
            return addedFuel;
        }

        public static FuelState EvaluateState(float normalizedFuel, float low, float critical)
        {
            if (normalizedFuel <= 0.001f)
            {
                return FuelState.Empty;
            }

            if (normalizedFuel <= critical)
            {
                return FuelState.Critical;
            }

            return normalizedFuel <= low ? FuelState.Low : FuelState.Normal;
        }

        private void SetFuel(float value)
        {
            currentFuel = Mathf.Clamp(value, 0f, capacity);
            FuelChanged?.Invoke(currentFuel);

            var newState = State;
            if (newState == previousState)
            {
                return;
            }

            previousState = newState;
            StateChanged?.Invoke(newState);
        }
    }
}
