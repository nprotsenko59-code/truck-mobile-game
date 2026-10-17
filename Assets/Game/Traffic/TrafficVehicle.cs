using TruckGame.Driving;
using UnityEngine;

namespace TruckGame.Traffic
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class TrafficVehicle : MonoBehaviour
    {
        [SerializeField] private TrafficRoute route;
        [SerializeField] private float laneOffset;
        [SerializeField] private float speedMetresPerSecond = 12f;
        [SerializeField] private bool isOncoming;
        [SerializeField] private int startPointIndex;
        [SerializeField] private float collisionGhostSeconds = 0.65f;

        private Rigidbody body;
        private BoxCollider vehicleCollider;
        private int targetPointIndex;
        private float ghostUntil;

        public bool IsOncoming => isOncoming;
        public float SpeedMetresPerSecond => speedMetresPerSecond;
        public int PlayerCollisionCount { get; private set; }

        public void Configure(
            TrafficRoute trafficRoute,
            float offset,
            float speed,
            bool oncoming,
            int startIndex)
        {
            route = trafficRoute;
            laneOffset = offset;
            speedMetresPerSecond = speed;
            isOncoming = oncoming;
            startPointIndex = startIndex;
            PlaceAtStart();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            vehicleCollider = GetComponent<BoxCollider>();
        }

        private void Start()
        {
            PlaceAtStart();
        }

        private void FixedUpdate()
        {
            if (route == null || route.PointCount < 2)
            {
                return;
            }

            if (!vehicleCollider.enabled && Time.time >= ghostUntil)
            {
                vehicleCollider.enabled = true;
            }

            var target = route.GetLanePoint(targetPointIndex, laneOffset);
            target.y = body.position.y;
            var displacement = target - body.position;
            displacement.y = 0f;

            if (displacement.sqrMagnitude < 1.4f * 1.4f)
            {
                AdvanceTarget();
                target = route.GetLanePoint(targetPointIndex, laneOffset);
                target.y = body.position.y;
                displacement = target - body.position;
                displacement.y = 0f;
            }

            if (displacement.sqrMagnitude < 0.001f)
            {
                return;
            }

            var direction = displacement.normalized;
            var distance = Mathf.Min(speedMetresPerSecond * Time.fixedDeltaTime, displacement.magnitude);
            body.MovePosition(body.position + direction * distance);
            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, 8f * Time.fixedDeltaTime));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody == null ||
                collision.rigidbody.GetComponent<ArcadeTruckController>() == null)
            {
                return;
            }

            PlayerCollisionCount++;
            ghostUntil = Time.time + collisionGhostSeconds;
            vehicleCollider.enabled = false;
        }

        private void PlaceAtStart()
        {
            if (route == null || route.PointCount < 2)
            {
                return;
            }

            body ??= GetComponent<Rigidbody>();
            vehicleCollider ??= GetComponent<BoxCollider>();

            startPointIndex = Mathf.Clamp(startPointIndex, 0, route.PointCount - 1);
            targetPointIndex = isOncoming
                ? Mathf.Max(0, startPointIndex - 1)
                : Mathf.Min(route.PointCount - 1, startPointIndex + 1);

            var position = route.GetLanePoint(startPointIndex, laneOffset);
            position.y = 0.68f;
            transform.position = position;
            body.position = position;

            var target = route.GetLanePoint(targetPointIndex, laneOffset);
            var direction = target - position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                var rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = rotation;
                body.rotation = rotation;
            }
        }

        private void AdvanceTarget()
        {
            targetPointIndex += isOncoming ? -1 : 1;
            if (targetPointIndex >= 0 && targetPointIndex < route.PointCount)
            {
                return;
            }

            var restartIndex = isOncoming ? route.PointCount - 1 : 0;
            var position = route.GetLanePoint(restartIndex, laneOffset);
            position.y = body.position.y;
            body.position = position;
            targetPointIndex = isOncoming ? restartIndex - 1 : restartIndex + 1;
        }
    }
}
