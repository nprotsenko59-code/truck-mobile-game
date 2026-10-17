using System.Collections;
using NUnit.Framework;
using TruckGame.Driving;
using TruckGame.Fuel;
using TruckGame.Traffic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TruckGame.Tests
{
    public sealed class PrototypeSmokeTests
    {
        [UnityTest]
        public IEnumerator PrototypeScene_TruckMovesForwardAndRemainsUpright()
        {
            SceneManager.LoadScene("Prototype");
            yield return null;

            var truck = GameObject.Find("Prototype Truck");
            Assert.That(truck, Is.Not.Null, "Prototype Truck is missing from the scene.");
            Assert.That(truck.GetComponent<ArcadeTruckController>(), Is.Not.Null);

            var initialPosition = truck.transform.position;
            yield return new WaitForSeconds(2.5f);

            Assert.That(truck.transform.position.z, Is.GreaterThan(initialPosition.z + 0.5f));
            Assert.That(Vector3.Dot(truck.transform.up, Vector3.up), Is.GreaterThan(0.5f));
        }

        [UnityTest]
        public IEnumerator PrototypeScene_TrafficMovesInBothDirections()
        {
            SceneManager.LoadScene("Prototype");
            yield return null;

            var traffic = Object.FindObjectsByType<TrafficVehicle>(FindObjectsInactive.Exclude);
            Assert.That(traffic.Length, Is.GreaterThanOrEqualTo(4));
            Assert.That(System.Array.Exists(traffic, vehicle => vehicle.IsOncoming), Is.True);
            Assert.That(System.Array.Exists(traffic, vehicle => !vehicle.IsOncoming), Is.True);

            var initialPositions = new Vector3[traffic.Length];
            for (var index = 0; index < traffic.Length; index++)
            {
                initialPositions[index] = traffic[index].transform.position;
            }

            yield return new WaitForSeconds(1.25f);

            var movingVehicles = 0;
            for (var index = 0; index < traffic.Length; index++)
            {
                if (Vector3.Distance(initialPositions[index], traffic[index].transform.position) > 1f)
                {
                    movingVehicles++;
                }
            }

            Assert.That(movingVehicles, Is.EqualTo(traffic.Length));
        }

        [UnityTest]
        public IEnumerator PrototypeScene_FuelConsumesAndStationRefillsTruck()
        {
            SceneManager.LoadScene("Prototype");
            yield return null;

            var truck = GameObject.Find("Prototype Truck");
            var tank = truck.GetComponent<FuelTank>();
            var controller = truck.GetComponent<ArcadeTruckController>();
            var station = Object.FindAnyObjectByType<FuelStation>(FindObjectsInactive.Exclude);
            var hud = Object.FindAnyObjectByType<FuelHud>(FindObjectsInactive.Exclude);
            Assert.That(tank, Is.Not.Null, "FuelTank is missing from the player truck.");
            Assert.That(station, Is.Not.Null, "Fuel station is missing from the prototype scene.");
            Assert.That(hud, Is.Not.Null, "Fuel HUD is missing from the prototype scene.");

            var startingFuel = tank.CurrentFuel;
            yield return new WaitForSeconds(1.25f);
            Assert.That(tank.CurrentFuel, Is.LessThan(startingFuel));

            tank.Consume(30f);
            var addedFuel = station.Refuel(tank);
            Assert.That(addedFuel, Is.GreaterThan(0f));
            Assert.That(tank.CurrentFuel, Is.EqualTo(tank.Capacity).Within(0.001f));
            Assert.That(tank.State, Is.EqualTo(FuelState.Normal));

            tank.Consume(tank.Capacity);
            Assert.That(tank.RunFailed, Is.True);
            Assert.That(controller.HasPower, Is.False);
        }
    }
}
