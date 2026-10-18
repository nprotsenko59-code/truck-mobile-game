using NUnit.Framework;
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
            deliveryRun = truck.AddComponent<DeliveryRunController>();
            deliveryRun.Configure(truck.transform, tank, route, null, null, 1250);
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
    }
}
