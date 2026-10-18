using UnityEngine;
using UnityEngine.InputSystem;

namespace TruckGame.Delivery
{
    public sealed class DeliveryHud : MonoBehaviour
    {
        [SerializeField] private DeliveryRunController deliveryRun;

        private GUIStyle labelStyle;
        private GUIStyle valueStyle;
        private GUIStyle resultTitleStyle;
        private GUIStyle resultBodyStyle;
        private GUIStyle breakdownStyle;
        private GUIStyle buttonStyle;

        public void Configure(DeliveryRunController run)
        {
            deliveryRun = run;
        }

        private void Update()
        {
            if (deliveryRun != null && !deliveryRun.IsRunning &&
                Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                deliveryRun.Restart();
            }
        }

        private void OnGUI()
        {
            if (deliveryRun == null)
            {
                return;
            }

            EnsureStyles();
            var scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.35f);
            DrawRoutePanel(scale);
            if (!deliveryRun.IsRunning)
            {
                DrawResult(scale);
            }
        }

        private void DrawRoutePanel(float scale)
        {
            var panel = new Rect(Screen.width - 354f * scale, 22f * scale, 330f * scale, 94f * scale);
            var previousColor = GUI.color;
            GUI.color = new Color(0.025f, 0.035f, 0.05f, 0.88f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(
                new Rect(panel.x + 16f * scale, panel.y + 8f * scale, 190f * scale, 28f * scale),
                "TO HOTEL",
                labelStyle);
            GUI.Label(
                new Rect(panel.xMax - 112f * scale, panel.y + 8f * scale, 96f * scale, 28f * scale),
                FormatTime(deliveryRun.ElapsedSeconds),
                valueStyle);

            var bar = new Rect(panel.x + 16f * scale, panel.y + 52f * scale, 205f * scale, 22f * scale);
            GUI.color = new Color(0.08f, 0.1f, 0.13f, 1f);
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.68f, 0.12f);
            GUI.DrawTexture(
                new Rect(bar.x + 3f, bar.y + 3f, (bar.width - 6f) * deliveryRun.Progress01, bar.height - 6f),
                Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(
                new Rect(panel.xMax - 100f * scale, panel.y + 48f * scale, 84f * scale, 30f * scale),
                $"{Mathf.CeilToInt(deliveryRun.DistanceRemaining)} m",
                valueStyle);
            GUI.color = previousColor;
        }

        private void DrawResult(float scale)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.58f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

            var panel = new Rect(
                Screen.width * 0.5f - 250f * scale,
                Screen.height * 0.5f - 215f * scale,
                500f * scale,
                430f * scale);
            GUI.color = new Color(0.025f, 0.035f, 0.05f, 0.97f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = Color.white;

            var delivered = deliveryRun.State == DeliveryState.Delivered;
            resultTitleStyle.normal.textColor = delivered
                ? new Color(0.2f, 1f, 0.72f)
                : new Color(1f, 0.25f, 0.08f);
            GUI.Label(
                new Rect(panel.x + 20f * scale, panel.y + 28f * scale, panel.width - 40f * scale, 52f * scale),
                delivered ? "DELIVERY COMPLETE" : "DELIVERY FAILED",
                resultTitleStyle);
            GUI.Label(
                new Rect(panel.x + 20f * scale, panel.y + 92f * scale, panel.width - 40f * scale, 32f * scale),
                delivered ? "Cargo delivered to the hotel" : "Reason: out of fuel",
                resultBodyStyle);
            GUI.Label(
                new Rect(panel.x + 20f * scale, panel.y + 134f * scale, panel.width - 40f * scale, 32f * scale),
                $"CARGO  {Mathf.RoundToInt(deliveryRun.CargoConditionPercent)}%     TIME  {FormatTime(deliveryRun.ElapsedSeconds)}",
                resultBodyStyle);

            var score = deliveryRun.Score;
            GUI.Label(
                new Rect(panel.x + 54f * scale, panel.y + 184f * scale, panel.width - 108f * scale, 30f * scale),
                $"Cargo condition     {score.Cargo}/60",
                breakdownStyle);
            GUI.Label(
                new Rect(panel.x + 54f * scale, panel.y + 218f * scale, panel.width - 108f * scale, 30f * scale),
                $"Safe driving          {score.Safety}/20",
                breakdownStyle);
            GUI.Label(
                new Rect(panel.x + 54f * scale, panel.y + 252f * scale, panel.width - 108f * scale, 30f * scale),
                $"Fuel efficiency       {score.Fuel}/10",
                breakdownStyle);
            GUI.Label(
                new Rect(panel.x + 54f * scale, panel.y + 286f * scale, panel.width - 108f * scale, 30f * scale),
                $"Delivery time         {score.Time}/10",
                breakdownStyle);
            GUI.Label(
                new Rect(panel.x + 20f * scale, panel.y + 326f * scale, panel.width - 40f * scale, 34f * scale),
                $"SCORE  {score.Total}/100     PAYOUT  ${deliveryRun.Payout:N0}",
                resultBodyStyle);

            var button = new Rect(
                panel.x + 110f * scale,
                panel.yMax - 58f * scale,
                panel.width - 220f * scale,
                42f * scale);
            if (GUI.Button(button, "RESTART  [R]", buttonStyle))
            {
                deliveryRun.Restart();
            }

            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            valueStyle ??= new GUIStyle(labelStyle)
            {
                alignment = TextAnchor.MiddleRight
            };
            resultTitleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            resultBodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 19,
                alignment = TextAnchor.MiddleCenter
            };
            breakdownStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static string FormatTime(float seconds)
        {
            var totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
