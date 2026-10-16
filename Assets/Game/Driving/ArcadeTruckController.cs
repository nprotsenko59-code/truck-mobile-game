using TruckGame.Input;
using UnityEngine;

namespace TruckGame.Driving
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeTruckController : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private SteeringInput steeringInput;
        [SerializeField] private TruckTuning tuning;
        [SerializeField] private WheelCollider[] frontWheels;
        [SerializeField] private WheelCollider[] rearWheels;

        private float currentSteering;

        public float SpeedMps => body == null ? 0f : body.linearVelocity.magnitude;
        public float CurrentSteering => currentSteering;

        public void Configure(
            Rigidbody truckBody,
            SteeringInput input,
            TruckTuning truckTuning,
            WheelCollider[] front,
            WheelCollider[] rear)
        {
            body = truckBody;
            steeringInput = input;
            tuning = truckTuning;
            frontWheels = front;
            rearWheels = rear;
            ApplyBodySettings();
        }

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            steeringInput ??= GetComponent<SteeringInput>();
            ApplyBodySettings();
        }

        private void FixedUpdate()
        {
            if (body == null || steeringInput == null || tuning == null)
            {
                return;
            }

            UpdateSteering();
            UpdateAutomaticSpeed();
            ApplyAntiRoll(frontWheels);
            ApplyAntiRoll(rearWheels);
            body.AddForce(-transform.up * (tuning.downforcePerMps * SpeedMps), ForceMode.Force);
        }

        private void ApplyBodySettings()
        {
            if (body != null && tuning != null)
            {
                body.centerOfMass = tuning.centerOfMassOffset;
            }
        }

        private void UpdateSteering()
        {
            var target = steeringInput.Value;
            var rate = Mathf.Approximately(target, 0f)
                ? tuning.steeringRecenterPerSecond
                : tuning.steeringResponsePerSecond;

            currentSteering = Mathf.MoveTowards(currentSteering, target, rate * Time.fixedDeltaTime);

            var steerLimit = TruckMath.SteeringLimit(
                SpeedMps,
                tuning.lowSpeedReferenceMps,
                tuning.highSpeedReferenceMps,
                tuning.lowSpeedSteerAngle,
                tuning.highSpeedSteerAngle);

            if (frontWheels == null)
            {
                return;
            }

            foreach (var wheel in frontWheels)
            {
                if (wheel != null)
                {
                    wheel.steerAngle = currentSteering * steerLimit;
                }
            }
        }

        private void UpdateAutomaticSpeed()
        {
            if (rearWheels == null)
            {
                return;
            }

            var forwardSpeed = Vector3.Dot(body.linearVelocity, transform.forward);
            var shouldAccelerate = forwardSpeed < tuning.targetSpeedMps;
            var shouldBrake = forwardSpeed > tuning.targetSpeedMps + 0.75f;

            foreach (var wheel in rearWheels)
            {
                if (wheel == null)
                {
                    continue;
                }

                wheel.motorTorque = shouldAccelerate ? tuning.motorTorque : 0f;
                wheel.brakeTorque = shouldBrake ? tuning.overspeedBrakeTorque : 0f;
            }
        }

        private void ApplyAntiRoll(WheelCollider[] axle)
        {
            if (axle == null || axle.Length < 2 || axle[0] == null || axle[1] == null)
            {
                return;
            }

            var leftGrounded = TryGetSuspensionTravel(axle[0], out var leftTravel);
            var rightGrounded = TryGetSuspensionTravel(axle[1], out var rightTravel);
            var antiRoll = (leftTravel - rightTravel) * tuning.antiRollForce;

            if (leftGrounded)
            {
                body.AddForceAtPosition(axle[0].transform.up * -antiRoll, axle[0].transform.position);
            }

            if (rightGrounded)
            {
                body.AddForceAtPosition(axle[1].transform.up * antiRoll, axle[1].transform.position);
            }
        }

        private static bool TryGetSuspensionTravel(WheelCollider wheel, out float travel)
        {
            if (wheel.GetGroundHit(out var hit))
            {
                var localHit = wheel.transform.InverseTransformPoint(hit.point);
                travel = (-localHit.y - wheel.radius) / wheel.suspensionDistance;
                return true;
            }

            travel = 1f;
            return false;
        }
    }
}

