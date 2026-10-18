using UnityEngine;

namespace TruckGame.Cargo
{
    public sealed class CargoHud : MonoBehaviour
    {
        [SerializeField] private CargoTracker cargo;

        private GUIStyle titleStyle;
        private GUIStyle valueStyle;
        private GUIStyle hintStyle;
        private float damageFlashUntil;
        private float lastDamage;

        public void Configure(CargoTracker tracker)
        {
            Unsubscribe();
            cargo = tracker;
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
            if (cargo == null)
            {
                return;
            }

            EnsureStyles();
            var scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.35f);
            var panel = new Rect(24f * scale, 128f * scale, 310f * scale, 104f * scale);
            var previousColor = GUI.color;

            GUI.color = new Color(0.025f, 0.035f, 0.05f, 0.88f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(
                new Rect(panel.x + 16f * scale, panel.y + 8f * scale, 210f * scale, 28f * scale),
                cargo.CargoName.ToUpperInvariant(),
                titleStyle);
            GUI.Label(
                new Rect(panel.xMax - 82f * scale, panel.y + 8f * scale, 66f * scale, 28f * scale),
                $"{Mathf.CeilToInt(cargo.ConditionPercent)}%",
                valueStyle);

            var bar = new Rect(panel.x + 16f * scale, panel.y + 43f * scale, panel.width - 32f * scale, 20f * scale);
            GUI.color = new Color(0.08f, 0.1f, 0.13f, 1f);
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = CargoColor(cargo.State);
            GUI.DrawTexture(new Rect(bar.x + 3f, bar.y + 3f, (bar.width - 6f) * cargo.Condition01, bar.height - 6f), Texture2D.whiteTexture);

            GUI.color = Color.white;
            hintStyle.normal.textColor = Time.unscaledTime < damageFlashUntil
                ? new Color(1f, 0.3f, 0.1f)
                : new Color(0.72f, 0.76f, 0.8f);
            GUI.Label(
                new Rect(panel.x + 16f * scale, panel.y + 69f * scale, panel.width - 32f * scale, 24f * scale),
                Time.unscaledTime < damageFlashUntil
                    ? $"IMPACT  -{Mathf.CeilToInt(lastDamage)}%"
                    : "PROTECT CARGO",
                hintStyle);
            GUI.color = previousColor;
        }

        private void HandleDamage(float amount, float severity)
        {
            lastDamage = amount;
            damageFlashUntil = Time.unscaledTime + 1.5f;
        }

        private void Subscribe()
        {
            if (cargo != null)
            {
                cargo.DamageTaken -= HandleDamage;
                cargo.DamageTaken += HandleDamage;
            }
        }

        private void Unsubscribe()
        {
            if (cargo != null)
            {
                cargo.DamageTaken -= HandleDamage;
            }
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            valueStyle ??= new GUIStyle(titleStyle)
            {
                alignment = TextAnchor.MiddleRight
            };
            hintStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
        }

        private static Color CargoColor(CargoConditionState state)
        {
            return state switch
            {
                CargoConditionState.Damaged => new Color(1f, 0.68f, 0.12f),
                CargoConditionState.Critical => new Color(1f, 0.24f, 0.08f),
                _ => new Color(0.12f, 0.86f, 0.58f)
            };
        }
    }
}
