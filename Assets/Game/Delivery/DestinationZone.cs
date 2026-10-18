using UnityEngine;

namespace TruckGame.Delivery
{
    [RequireComponent(typeof(Collider))]
    public sealed class DestinationZone : MonoBehaviour
    {
        [SerializeField] private DeliveryRunController deliveryRun;

        public void Configure(DeliveryRunController run)
        {
            deliveryRun = run;
        }

        private void OnTriggerEnter(Collider other)
        {
            var enteringRun = other.GetComponentInParent<DeliveryRunController>();
            if (enteringRun != null && enteringRun == deliveryRun)
            {
                deliveryRun.CompleteDelivery();
            }
        }
    }
}
