using UnityEngine;

namespace TruckGame.Economy
{
    public readonly struct DeliverySettlement
    {
        public DeliverySettlement(int grossPayout, int fuelCost, int repairCost, int balanceBefore)
        {
            GrossPayout = Mathf.Max(0, grossPayout);
            FuelCost = Mathf.Max(0, fuelCost);
            RepairCost = Mathf.Max(0, repairCost);
            NetIncome = GrossPayout - FuelCost - RepairCost;
            BalanceAfter = Mathf.Max(0, balanceBefore + NetIncome);
        }

        public int GrossPayout { get; }
        public int FuelCost { get; }
        public int RepairCost { get; }
        public int NetIncome { get; }
        public int BalanceAfter { get; }
    }

    public static class EconomyCalculator
    {
        public const float FuelPricePerUnit = 2f;
        public const float RepairPricePerConditionPoint = 4f;
        public const float EfficiencyUpgradeMultiplier = 0.8f;

        public static DeliverySettlement CalculateSettlement(
            int grossPayout,
            float fuelConsumed,
            float cargoConditionPercent,
            int balanceBefore)
        {
            var fuelCost = Mathf.RoundToInt(Mathf.Max(0f, fuelConsumed) * FuelPricePerUnit);
            var damage = 100f - Mathf.Clamp(cargoConditionPercent, 0f, 100f);
            var repairCost = Mathf.RoundToInt(damage * RepairPricePerConditionPoint);
            return new DeliverySettlement(grossPayout, fuelCost, repairCost, Mathf.Max(0, balanceBefore));
        }

        public static float FuelBurnRate(float baseRate, bool efficiencyUpgradeOwned)
        {
            var safeRate = Mathf.Max(0f, baseRate);
            return efficiencyUpgradeOwned ? safeRate * EfficiencyUpgradeMultiplier : safeRate;
        }
    }
}
