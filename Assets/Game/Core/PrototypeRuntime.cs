using UnityEngine;

namespace TruckGame.Core
{
    public sealed class PrototypeRuntime : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}

