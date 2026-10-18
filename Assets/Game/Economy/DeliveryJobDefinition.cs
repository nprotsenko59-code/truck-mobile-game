using System;
using UnityEngine;

namespace TruckGame.Economy
{
    [Serializable]
    public sealed class DeliveryJobDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string difficulty;
        [SerializeField] private string cargoRisk;
        [SerializeField] private string fuelPressure;
        [SerializeField, Min(0)] private int payout;
        [SerializeField, Range(1f, 100f)] private float startingFuel;
        [SerializeField, Min(0f)] private float impactDamageRate;
        [SerializeField, Min(0f)] private float roughDrivingDamageRate;

        public DeliveryJobDefinition(
            string jobId,
            string jobTitle,
            string difficultyLabel,
            string cargoRiskLabel,
            string fuelPressureLabel,
            int basePayout,
            float initialFuel,
            float impactDamage,
            float roughDrivingDamage)
        {
            id = jobId;
            title = jobTitle;
            difficulty = difficultyLabel;
            cargoRisk = cargoRiskLabel;
            fuelPressure = fuelPressureLabel;
            payout = Mathf.Max(0, basePayout);
            startingFuel = Mathf.Clamp(initialFuel, 1f, 100f);
            impactDamageRate = Mathf.Max(0f, impactDamage);
            roughDrivingDamageRate = Mathf.Max(0f, roughDrivingDamage);
        }

        public string Id => id;
        public string Title => title;
        public string Difficulty => difficulty;
        public string CargoRisk => cargoRisk;
        public string FuelPressure => fuelPressure;
        public int Payout => payout;
        public float StartingFuel => startingFuel;
        public float ImpactDamageRate => impactDamageRate;
        public float RoughDrivingDamageRate => roughDrivingDamageRate;
    }
}
