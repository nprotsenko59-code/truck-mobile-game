using UnityEngine;

namespace TruckGame.Driving
{
    public static class TruckMath
    {
        public static float SpeedFactor(float speedMps, float lowSpeedMps, float highSpeedMps)
        {
            if (highSpeedMps <= lowSpeedMps)
            {
                return speedMps >= highSpeedMps ? 1f : 0f;
            }

            return Mathf.InverseLerp(lowSpeedMps, highSpeedMps, Mathf.Max(0f, speedMps));
        }

        public static float SteeringLimit(
            float speedMps,
            float lowSpeedMps,
            float highSpeedMps,
            float lowSpeedAngle,
            float highSpeedAngle)
        {
            return Mathf.Lerp(
                lowSpeedAngle,
                highSpeedAngle,
                SpeedFactor(speedMps, lowSpeedMps, highSpeedMps));
        }
    }
}

