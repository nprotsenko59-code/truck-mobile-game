using System;
using TruckGame.Cargo;
using TruckGame.Delivery;
using TruckGame.Driving;
using TruckGame.Fuel;
using UnityEngine;

namespace TruckGame.Economy
{
    public sealed class EconomyGameController : MonoBehaviour
    {
        private const string BalanceKey = "TruckGame.Economy.Balance.v1";
        private const string EfficiencyUpgradeKey = "TruckGame.Economy.FuelEfficiency.v1";
        private const float BaseFuelBurnRate = 0.65f;

        [SerializeField] private DeliveryRunController deliveryRun;
        [SerializeField] private FuelTank fuelTank;
        [SerializeField] private CargoTracker cargoTracker;
        [SerializeField] private ArcadeTruckController truckController;
        [SerializeField] private DeliveryJobDefinition[] jobs;
        [SerializeField, Min(0)] private int startingBalance = 500;
        [SerializeField, Min(0)] private int efficiencyUpgradeCost = 1200;
        [SerializeField] private bool persistenceEnabled = true;

        private bool settledThisRun;

        public event Action ProfileChanged;

        public int Balance { get; private set; }
        public bool EfficiencyUpgradeOwned { get; private set; }
        public int EfficiencyUpgradeCost => efficiencyUpgradeCost;
        public bool IsChoosingJob { get; private set; }
        public bool HasSettlement { get; private set; }
        public DeliverySettlement LastSettlement { get; private set; }
        public DeliveryJobDefinition SelectedJob { get; private set; }
        public int JobCount => jobs?.Length ?? 0;

        public void Configure(
            DeliveryRunController run,
            FuelTank tank,
            CargoTracker cargo,
            ArcadeTruckController controller,
            DeliveryJobDefinition[] jobDefinitions)
        {
            deliveryRun = run;
            fuelTank = tank;
            cargoTracker = cargo;
            truckController = controller;
            jobs = jobDefinitions;
            deliveryRun?.PrepareForJobSelection();
        }

        private void Awake()
        {
            LoadProfile();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            OpenJobBoard();
        }

        private void OnDisable()
        {
            Unsubscribe();
            Time.timeScale = 1f;
        }

        public DeliveryJobDefinition GetJob(int index)
        {
            return jobs != null && index >= 0 && index < jobs.Length ? jobs[index] : null;
        }

        public bool SelectJob(int index)
        {
            var job = GetJob(index);
            if (job == null)
            {
                return false;
            }

            SelectedJob = job;
            settledThisRun = false;
            HasSettlement = false;
            IsChoosingJob = false;

            fuelTank?.Configure(
                100f,
                job.StartingFuel,
                EconomyCalculator.FuelBurnRate(BaseFuelBurnRate, EfficiencyUpgradeOwned),
                0.35f,
                0.15f);
            cargoTracker?.Configure(
                "Hotel Supplies",
                4f,
                job.ImpactDamageRate,
                22f,
                job.RoughDrivingDamageRate);

            Time.timeScale = 1f;
            deliveryRun?.BeginJob(job.Title, job.Payout);
            return true;
        }

        public bool PurchaseEfficiencyUpgrade()
        {
            if (EfficiencyUpgradeOwned || Balance < efficiencyUpgradeCost)
            {
                return false;
            }

            Balance -= efficiencyUpgradeCost;
            EfficiencyUpgradeOwned = true;
            SaveProfile();
            ProfileChanged?.Invoke();
            return true;
        }

        public void SetPersistenceEnabled(bool enabled)
        {
            persistenceEnabled = enabled;
        }

        public void OpenJobBoard()
        {
            IsChoosingJob = true;
            SelectedJob = null;
            HasSettlement = false;
            settledThisRun = false;
            deliveryRun?.PrepareForJobSelection();
            Time.timeScale = 0f;
        }

        private void HandleDeliveryStateChanged(DeliveryState state)
        {
            if (settledThisRun || state == DeliveryState.Running || state == DeliveryState.AwaitingJob)
            {
                return;
            }

            LastSettlement = EconomyCalculator.CalculateSettlement(
                deliveryRun == null ? 0 : deliveryRun.Payout,
                fuelTank == null ? 0f : fuelTank.TotalConsumed,
                cargoTracker == null ? 100f : cargoTracker.ConditionPercent,
                Balance);
            Balance = LastSettlement.BalanceAfter;
            HasSettlement = true;
            settledThisRun = true;
            SaveProfile();
            ProfileChanged?.Invoke();
            Time.timeScale = 0f;
        }

        private void LoadProfile()
        {
            Balance = persistenceEnabled ? PlayerPrefs.GetInt(BalanceKey, startingBalance) : startingBalance;
            EfficiencyUpgradeOwned = persistenceEnabled && PlayerPrefs.GetInt(EfficiencyUpgradeKey, 0) == 1;
        }

        private void SaveProfile()
        {
            if (!persistenceEnabled)
            {
                return;
            }

            PlayerPrefs.SetInt(BalanceKey, Balance);
            PlayerPrefs.SetInt(EfficiencyUpgradeKey, EfficiencyUpgradeOwned ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void Subscribe()
        {
            if (deliveryRun != null)
            {
                deliveryRun.StateChanged -= HandleDeliveryStateChanged;
                deliveryRun.StateChanged += HandleDeliveryStateChanged;
            }
        }

        private void Unsubscribe()
        {
            if (deliveryRun != null)
            {
                deliveryRun.StateChanged -= HandleDeliveryStateChanged;
            }
        }
    }
}
