using NUnit.Framework;
using TruckGame.Fuel;
using UnityEngine;

namespace TruckGame.Tests
{
    public sealed class FuelTankTests
    {
        private GameObject truck;
        private FuelTank tank;

        [SetUp]
        public void SetUp()
        {
            truck = new GameObject("Fuel Test Truck");
            truck.AddComponent<Rigidbody>();
            tank = truck.AddComponent<FuelTank>();
            tank.Configure(100f, 50f, 4f, 0.35f, 0.15f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(truck);
        }

        [Test]
        public void Consume_TransitionsThroughWarningsAndEmptyFailure()
        {
            tank.Consume(20f);
            Assert.That(tank.State, Is.EqualTo(FuelState.Low));

            tank.Consume(20f);
            Assert.That(tank.State, Is.EqualTo(FuelState.Critical));

            tank.Consume(20f);
            Assert.That(tank.State, Is.EqualTo(FuelState.Empty));
            Assert.That(tank.RunFailed, Is.True);
        }

        [Test]
        public void RefuelToFull_RestoresCapacityAndNormalState()
        {
            tank.Consume(42f);
            var added = tank.RefuelToFull();

            Assert.That(added, Is.EqualTo(92f).Within(0.001f));
            Assert.That(tank.CurrentFuel, Is.EqualTo(100f).Within(0.001f));
            Assert.That(tank.State, Is.EqualTo(FuelState.Normal));
        }

        [Test]
        public void ConsumeFor_IgnoresStoppedTruckAndBurnsWhileMoving()
        {
            tank.ConsumeFor(1f, 0.1f);
            Assert.That(tank.CurrentFuel, Is.EqualTo(50f).Within(0.001f));

            tank.ConsumeFor(1f, 16f);
            Assert.That(tank.CurrentFuel, Is.EqualTo(46f).Within(0.001f));
        }
    }
}
