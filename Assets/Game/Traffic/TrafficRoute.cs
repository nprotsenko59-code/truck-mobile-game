using UnityEngine;

namespace TruckGame.Traffic
{
    public sealed class TrafficRoute : MonoBehaviour
    {
        [SerializeField] private Vector3[] points;

        private float totalLength;

        public int PointCount => points?.Length ?? 0;
        public float TotalLength => totalLength;

        public void Configure(Vector3[] routePoints)
        {
            points = routePoints;
            RecalculateLength();
        }

        private void Awake()
        {
            RecalculateLength();
        }

        public Vector3 GetPoint(int index)
        {
            return PointCount == 0
                ? transform.position
                : points[Mathf.Clamp(index, 0, PointCount - 1)];
        }

        public Vector3 GetLanePoint(int index, float laneOffset)
        {
            if (PointCount < 2)
            {
                return transform.position;
            }

            index = Mathf.Clamp(index, 0, PointCount - 1);
            var previous = points[Mathf.Max(0, index - 1)];
            var next = points[Mathf.Min(PointCount - 1, index + 1)];
            var tangent = (next - previous).normalized;
            var right = Vector3.Cross(Vector3.up, tangent).normalized;
            return points[index] + right * laneOffset;
        }

        public Vector3 GetForward(int index)
        {
            if (PointCount < 2)
            {
                return transform.forward;
            }

            index = Mathf.Clamp(index, 0, PointCount - 1);
            var previous = points[Mathf.Max(0, index - 1)];
            var next = points[Mathf.Min(PointCount - 1, index + 1)];
            return (next - previous).normalized;
        }

        public float GetRemainingDistance(Vector3 worldPosition)
        {
            if (PointCount < 2)
            {
                return 0f;
            }

            var nearestSegment = 0;
            var nearestProgress = 0f;
            var nearestSqrDistance = float.PositiveInfinity;
            for (var index = 0; index < PointCount - 1; index++)
            {
                var start = points[index];
                var end = points[index + 1];
                var segment = end - start;
                var segmentSqrLength = segment.sqrMagnitude;
                if (segmentSqrLength <= 0.0001f)
                {
                    continue;
                }

                var progress = Mathf.Clamp01(Vector3.Dot(worldPosition - start, segment) / segmentSqrLength);
                var nearestPoint = start + segment * progress;
                var sqrDistance = (worldPosition - nearestPoint).sqrMagnitude;
                if (sqrDistance >= nearestSqrDistance)
                {
                    continue;
                }

                nearestSqrDistance = sqrDistance;
                nearestSegment = index;
                nearestProgress = progress;
            }

            var remaining = Vector3.Distance(
                Vector3.Lerp(points[nearestSegment], points[nearestSegment + 1], nearestProgress),
                points[nearestSegment + 1]);
            for (var index = nearestSegment + 1; index < PointCount - 1; index++)
            {
                remaining += Vector3.Distance(points[index], points[index + 1]);
            }

            return remaining;
        }

        public float GetProgress01(Vector3 worldPosition)
        {
            return totalLength <= 0.001f
                ? 0f
                : Mathf.Clamp01(1f - GetRemainingDistance(worldPosition) / totalLength);
        }

        private void RecalculateLength()
        {
            totalLength = 0f;
            for (var index = 0; index < PointCount - 1; index++)
            {
                totalLength += Vector3.Distance(points[index], points[index + 1]);
            }
        }
    }
}
