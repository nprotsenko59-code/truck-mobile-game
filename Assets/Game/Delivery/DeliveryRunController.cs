using System;
using TruckGame.Driving;
using TruckGame.Fuel;
using TruckGame.Traffic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TruckGame.Delivery
{
    public enum DeliveryState
    {
        Running,
        Delivered,
        FailedFuel
    }

    public sealed class DeliveryRunController : MonoBehaviour
    {
        [SerializeField] private Transform truck;
        [SerializeField] private FuelTank fuelTank;
        [SerializeField] private TrafficRoute route;
        [SerializeField] private Transform destination;
        [SerializeField] private ArcadeTruckController truckController;
        [SerializeField, Min(0)] private int basePayout = 1250;
        [SerializeField] private DeliveryState state;

        private float startedAt;
        private float finishedAt;

        public event Action<DeliveryState> StateChanged;

        public DeliveryState State => state;
        public bool IsRunning => state == DeliveryState.Running;
        public int Payout => state == DeliveryState.Delivered ? basePayout : 0;
        public float ElapsedSeconds => (IsRunning ? Time.time : finishedAt) - startedAt;
        public float DistanceRemaining => route == null || truck == null
            ? 0f
            : route.GetRemainingDistance(truck.position);
        public float Progress01 => route == null || truck == null
            ? 0f
            : route.GetProgress01(truck.position);

        public void Configure(
            Transform playerTruck,
            FuelTank tank,
            TrafficRoute trafficRoute,
            Transform destinationTransform,
            ArcadeTruckController controller,
            int payout)
        {
            Unsubscribe();
            truck = playerTruck;
            fuelTank = tank;
            route = trafficRoute;
            destination = destinationTransform;
            truckController = controller;
            basePayout = Mathf.Max(0, payout);
            state = DeliveryState.Running;
            Subscribe();
        }

        private void Awake()
        {
            truck ??= transform;
            fuelTank ??= GetComponent<FuelTank>();
            truckController ??= GetComponent<ArcadeTruckController>();
        }

        private void OnEnable()
        {
            state = DeliveryState.Running;
            startedAt = Time.time;
            finishedAt = startedAt;
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (IsRunning && destination != null && truck != null &&
                Vector3.Distance(truck.position, destination.position) <= 5f)
            {
                CompleteDelivery();
            }
        }

        public void CompleteDelivery()
        {
            Finish(DeliveryState.Delivered);
        }

        public void FailFromFuel()
        {
            Finish(DeliveryState.FailedFuel);
        }

        public void Restart()
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        private void HandleFuelStateChanged(FuelState fuelState)
        {
            if (fuelState == FuelState.Empty)
            {
                FailFromFuel();
            }
        }

        private void Finish(DeliveryState result)
        {
            if (!IsRunning || result == DeliveryState.Running)
            {
                return;
            }

            state = result;
            finishedAt = Time.time;
            truckController?.SetDriveEnabled(false);
            StateChanged?.Invoke(state);
        }

        private void Subscribe()
        {
            if (fuelTank == null)
            {
                return;
            }

            fuelTank.StateChanged -= HandleFuelStateChanged;
            fuelTank.StateChanged += HandleFuelStateChanged;
        }

        private void Unsubscribe()
        {
            if (fuelTank != null)
            {
                fuelTank.StateChanged -= HandleFuelStateChanged;
            }
        }
    }
}
