using UnityEngine;

namespace TruckGame.Fuel
{
    [RequireComponent(typeof(Collider), typeof(AudioSource))]
    public sealed class FuelStation : MonoBehaviour
    {
        [SerializeField] private Transform feedbackMarker;
        [SerializeField, Min(0f)] private float retriggerDelay = 1.5f;

        private AudioSource audioSource;
        private AudioClip refuelChime;
        private Vector3 markerScale;
        private float feedbackUntil;
        private float lastRefuelTime = float.NegativeInfinity;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.35f;
            refuelChime = CreateRefuelChime();
            if (feedbackMarker != null)
            {
                markerScale = feedbackMarker.localScale;
            }
        }

        public void Configure(Transform marker)
        {
            feedbackMarker = marker;
            markerScale = marker == null ? Vector3.one : marker.localScale;
        }

        private void Update()
        {
            if (feedbackMarker == null)
            {
                return;
            }

            var targetScale = Time.time < feedbackUntil ? markerScale * 1.22f : markerScale;
            feedbackMarker.localScale = Vector3.Lerp(
                feedbackMarker.localScale,
                targetScale,
                10f * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            var tank = other.GetComponentInParent<FuelTank>();
            if (tank != null)
            {
                Refuel(tank);
            }
        }

        public float Refuel(FuelTank tank)
        {
            if (tank == null || Time.time < lastRefuelTime + retriggerDelay)
            {
                return 0f;
            }

            var addedFuel = tank.RefuelToFull();
            if (addedFuel <= 0f)
            {
                return 0f;
            }

            lastRefuelTime = Time.time;
            feedbackUntil = Time.time + 0.8f;
            if (audioSource != null && refuelChime != null)
            {
                audioSource.PlayOneShot(refuelChime, 0.35f);
            }

            return addedFuel;
        }

        private static AudioClip CreateRefuelChime()
        {
            const int sampleRate = 22050;
            const float duration = 0.18f;
            var samples = new float[Mathf.CeilToInt(sampleRate * duration)];
            for (var index = 0; index < samples.Length; index++)
            {
                var progress = index / (float)samples.Length;
                var frequency = Mathf.Lerp(620f, 920f, progress);
                var envelope = 1f - progress;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * index / sampleRate) * envelope * 0.25f;
            }

            var clip = AudioClip.Create("Refuel Chime", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
