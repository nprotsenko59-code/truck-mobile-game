using UnityEngine;

namespace TruckGame.Fuel
{
    public sealed class FuelHud : MonoBehaviour
    {
        [SerializeField] private FuelTank tank;

        private GUIStyle titleStyle;
        private GUIStyle valueStyle;
        private GUIStyle warningStyle;
        private float refuelMessageUntil;
        private float lastRefuelAmount;

        public void Configure(FuelTank fuelTank)
        {
            Unsubscribe();
            tank = fuelTank;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnGUI()
        {
            if (tank == null)
            {
                return;
            }

            EnsureStyles();
            var scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.35f);
            var panel = new Rect(24f * scale, 22f * scale, 310f * scale, 94f * scale);
            var previousColor = GUI.color;

            GUI.color = new Color(0.025f, 0.035f, 0.05f, 0.88f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 16f * scale, panel.y + 9f * scale, 130f * scale, 28f * scale), "FUEL", titleStyle);
            GUI.Label(
                new Rect(panel.xMax - 104f * scale, panel.y + 9f * scale, 88f * scale, 28f * scale),
                $"{Mathf.CeilToInt(tank.NormalizedFuel * 100f)}%",
                valueStyle);

            var bar = new Rect(panel.x + 16f * scale, panel.y + 52f * scale, panel.width - 32f * scale, 22f * scale);
            GUI.color = new Color(0.08f, 0.1f, 0.13f, 1f);
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = FuelColor(tank.State);
            GUI.DrawTexture(new Rect(bar.x + 3f, bar.y + 3f, (bar.width - 6f) * tank.NormalizedFuel, bar.height - 6f), Texture2D.whiteTexture);
            GUI.color = previousColor;

            var message = WarningText(tank.State);
            if (!string.IsNullOrEmpty(message))
            {
                warningStyle.normal.textColor = FuelColor(tank.State);
                GUI.Label(new Rect(Screen.width * 0.5f - 260f * scale, 24f * scale, 520f * scale, 54f * scale), message, warningStyle);
            }
            else if (Time.unscaledTime < refuelMessageUntil)
            {
                warningStyle.normal.textColor = new Color(0.2f, 1f, 0.72f);
                GUI.Label(
                    new Rect(Screen.width * 0.5f - 260f * scale, 24f * scale, 520f * scale, 54f * scale),
                    $"REFUELED  +{Mathf.RoundToInt(lastRefuelAmount)}",
                    warningStyle);
            }
        }

        private void HandleRefueled(float amount)
        {
            lastRefuelAmount = amount;
            refuelMessageUntil = Time.unscaledTime + 1.5f;
        }

        private void Subscribe()
        {
            if (tank != null)
            {
                tank.Refueled -= HandleRefueled;
                tank.Refueled += HandleRefueled;
            }
        }

        private void Unsubscribe()
        {
            if (tank != null)
            {
                tank.Refueled -= HandleRefueled;
            }
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            valueStyle ??= new GUIStyle(titleStyle)
            {
                alignment = TextAnchor.MiddleRight
            };
            warningStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static Color FuelColor(FuelState state)
        {
            return state switch
            {
                FuelState.Low => new Color(1f, 0.72f, 0.12f),
                FuelState.Critical => new Color(1f, 0.25f, 0.08f),
                FuelState.Empty => new Color(0.95f, 0.08f, 0.08f),
                _ => new Color(0.12f, 0.86f, 0.58f)
            };
        }

        private static string WarningText(FuelState state)
        {
            return state switch
            {
                FuelState.Low => "LOW FUEL — STATION AHEAD",
                FuelState.Critical => "CRITICAL FUEL",
                FuelState.Empty => "OUT OF FUEL — RUN FAILED",
                _ => string.Empty
            };
        }
    }
}
