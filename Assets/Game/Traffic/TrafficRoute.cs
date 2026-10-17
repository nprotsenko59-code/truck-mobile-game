using UnityEngine;

namespace TruckGame.Traffic
{
    public sealed class TrafficRoute : MonoBehaviour
    {
        [SerializeField] private Vector3[] points;

        public int PointCount => points?.Length ?? 0;

        public void Configure(Vector3[] routePoints)
        {
            points = routePoints;
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
    }
}
