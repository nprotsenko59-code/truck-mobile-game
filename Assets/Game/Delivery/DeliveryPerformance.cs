using UnityEngine;

namespace TruckGame.Delivery
{
    public readonly struct DeliveryScoreBreakdown
    {
        public DeliveryScoreBreakdown(int cargo, int safety, int fuel, int time)
        {
            Cargo = cargo;
            Safety = safety;
            Fuel = fuel;
            Time = time;
        }

        public int Cargo { get; }
        public int Safety { get; }
        public int Fuel { get; }
        public int Time { get; }
        public int Total => Cargo + Safety + Fuel + Time;
    }

    public static class DeliveryPerformance
    {
        public const int MaximumScore = 100;

        public static DeliveryScoreBreakdown Calculate(
            float cargoConditionPercent,
            int collisionCount,
            float fuelConsumed,
            float fuelCapacity,
            float elapsedSeconds)
        {
            var cargo = Mathf.RoundToInt(Mathf.Clamp01(cargoConditionPercent / 100f) * 60f);
            var safety = Mathf.Clamp(20 - Mathf.Max(0, collisionCount) * 5, 0, 20);
            var fuelRatio = fuelCapacity <= 0f ? 1f : fuelConsumed / fuelCapacity;
            var fuel = Mathf.RoundToInt(Mathf.Clamp01(1f - fuelRatio) * 10f);
            var time = Mathf.RoundToInt(Mathf.InverseLerp(180f, 90f, elapsedSeconds) * 10f);
            return new DeliveryScoreBreakdown(cargo, safety, fuel, time);
        }

        public static int CalculatePayout(int basePayout, DeliveryScoreBreakdown score)
        {
            var rawPayout = Mathf.Max(0, basePayout) * Mathf.Clamp01(score.Total / (float)MaximumScore);
            return Mathf.RoundToInt(rawPayout / 10f) * 10;
        }
    }
}
