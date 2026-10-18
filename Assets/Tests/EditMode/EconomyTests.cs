using NUnit.Framework;
using TruckGame.Economy;

namespace TruckGame.Tests
{
    public sealed class EconomyTests
    {
        [Test]
        public void Settlement_DeductsFuelAndRepairCostsFromGrossPayout()
        {
            var settlement = EconomyCalculator.CalculateSettlement(1000, 40f, 75f, 500);

            Assert.That(settlement.GrossPayout, Is.EqualTo(1000));
            Assert.That(settlement.FuelCost, Is.EqualTo(80));
            Assert.That(settlement.RepairCost, Is.EqualTo(100));
            Assert.That(settlement.NetIncome, Is.EqualTo(820));
            Assert.That(settlement.BalanceAfter, Is.EqualTo(1320));
        }

        [Test]
        public void FailedJob_CannotMakeBalanceNegative()
        {
            var settlement = EconomyCalculator.CalculateSettlement(0, 100f, 0f, 100);

            Assert.That(settlement.NetIncome, Is.EqualTo(-600));
            Assert.That(settlement.BalanceAfter, Is.Zero);
        }

        [Test]
        public void FuelEfficiencyUpgrade_ReducesConsumptionRateByTwentyPercent()
        {
            Assert.That(EconomyCalculator.FuelBurnRate(0.65f, false), Is.EqualTo(0.65f).Within(0.001f));
            Assert.That(EconomyCalculator.FuelBurnRate(0.65f, true), Is.EqualTo(0.52f).Within(0.001f));
        }
    }
}
