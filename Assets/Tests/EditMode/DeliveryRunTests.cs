using NUnit.Framework;
using TruckGame.Cargo;
using TruckGame.Delivery;
using TruckGame.Fuel;
using TruckGame.Traffic;
using UnityEngine;

namespace TruckGame.Tests
{
    public sealed class DeliveryRunTests
    {
        private GameObject routeObject;
        private GameObject truck;
        private TrafficRoute route;
        private FuelTank tank;
        private CargoTracker cargo;
        private DeliveryRunController deliveryRun;

        [SetUp]
        public void SetUp()
        {
            routeObject = new GameObject("Test Route");
            route = routeObject.AddComponent<TrafficRoute>();
            route.Configure(new[]
            {
                Vector3.zero,
                new Vector3(0f, 0f, 50f),
                new Vector3(0f, 0f, 100f)
            });

            truck = new GameObject("Delivery Test Truck");
            truck.AddComponent<Rigidbody>();
            tank = truck.AddComponent<FuelTank>();
            tank.Configure(100f, 50f, 1f, 0.35f, 0.15f);
            cargo = truck.AddComponent<CargoTracker>();
            cargo.Configure("Hotel Supplies", 4f, 5f, 22f, 2.5f);
            deliveryRun = truck.AddComponent<DeliveryRunController>();
            deliveryRun.Configure(truck.transform, tank, cargo, route, null, null, 1250);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(truck);
            Object.DestroyImmediate(routeObject);
        }

        [Test]
        public void Route_ReportsRemainingDistanceAndProgress()
        {
            truck.transform.position = new Vector3(0f, 0f, 25f);

            Assert.That(route.TotalLength, Is.EqualTo(100f).Within(0.001f));
            Assert.That(route.GetRemainingDistance(truck.transform.position), Is.EqualTo(75f).Within(0.001f));
            Assert.That(route.GetProgress01(truck.transform.position), Is.EqualTo(0.25f).Within(0.001f));
        }

        [Test]
        public void CompleteDelivery_AwardsPayoutOnlyOnce()
        {
            deliveryRun.CompleteDelivery();
            deliveryRun.FailFromFuel();

            Assert.That(deliveryRun.State, Is.EqualTo(DeliveryState.Delivered));
            Assert.That(deliveryRun.Payout, Is.EqualTo(1250));
        }

        [Test]
        public void EmptyFuel_FailsDeliveryAndForfeitsPayout()
        {
            tank.Consume(tank.Capacity);

            Assert.That(deliveryRun.State, Is.EqualTo(DeliveryState.FailedFuel));
            Assert.That(deliveryRun.Payout, Is.Zero);
        }

        [Test]
        public void DamagedCargo_ReducesScoreAndPayout()
        {
            cargo.ApplyImpact(14f);

            deliveryRun.CompleteDelivery();

            Assert.That(deliveryRun.Score.Cargo, Is.EqualTo(30));
            Assert.That(deliveryRun.Score.Safety, Is.EqualTo(15));
            Assert.That(deliveryRun.Score.Total, Is.LessThan(100));
            Assert.That(deliveryRun.Payout, Is.LessThan(1250));
        }
    }
}
