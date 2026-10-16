using System.Collections;
using NUnit.Framework;
using TruckGame.Driving;
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
    }
}
