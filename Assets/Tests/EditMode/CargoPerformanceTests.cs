using NUnit.Framework;
using TruckGame.Cargo;
using TruckGame.Delivery;
using UnityEngine;

namespace TruckGame.Tests
{
    public sealed class CargoPerformanceTests
    {
        private GameObject truck;
        private CargoTracker cargo;

        [SetUp]
        public void SetUp()
        {
            truck = new GameObject("Cargo Test Truck");
            truck.AddComponent<Rigidbody>();
            cargo = truck.AddComponent<CargoTracker>();
            cargo.Configure("Hotel Supplies", 4f, 5f, 22f, 2.5f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(truck);
        }

        [Test]
        public void Impact_OnlyDamagesCargoAboveThreshold()
        {
            Assert.That(cargo.ApplyImpact(3.9f), Is.Zero);
            Assert.That(cargo.ApplyImpact(8f), Is.EqualTo(20f).Within(0.001f));
            Assert.That(cargo.ConditionPercent, Is.EqualTo(80f).Within(0.001f));
            Assert.That(cargo.CollisionCount, Is.EqualTo(1));
            Assert.That(cargo.State, Is.EqualTo(CargoConditionState.Secure));
        }

        [Test]
        public void RoughDriving_DamagesCargoOnlyWhenMovingAndTilted()
        {
            Assert.That(cargo.TrackRoughDriving(1f, 30f, 2f), Is.Zero);
            Assert.That(cargo.TrackRoughDriving(1f, 10f, 12f), Is.Zero);

            var damage = cargo.TrackRoughDriving(2f, 45f, 12f);

            Assert.That(damage, Is.GreaterThan(0f));
            Assert.That(cargo.RoughDrivingSeconds, Is.EqualTo(2f).Within(0.001f));
            Assert.That(cargo.ConditionPercent, Is.LessThan(100f));
        }

        [Test]
        public void PerformanceScore_WeightsAllFourDrivingFactors()
        {
            var perfect = DeliveryPerformance.Calculate(100f, 0, 0f, 100f, 60f);
            var damaged = DeliveryPerformance.Calculate(50f, 2, 50f, 100f, 135f);

            Assert.That(perfect.Total, Is.EqualTo(100));
            Assert.That(damaged.Cargo, Is.EqualTo(30));
            Assert.That(damaged.Safety, Is.EqualTo(10));
            Assert.That(damaged.Fuel, Is.EqualTo(5));
            Assert.That(damaged.Time, Is.EqualTo(5));
            Assert.That(damaged.Total, Is.EqualTo(50));
            Assert.That(DeliveryPerformance.CalculatePayout(1000, damaged), Is.EqualTo(500));
        }
    }
}
