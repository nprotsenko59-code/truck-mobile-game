using UnityEngine;

namespace TruckGame.Economy
{
    public sealed class JobBoardHud : MonoBehaviour
    {
        [SerializeField] private EconomyGameController economy;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle payoutStyle;
        private GUIStyle buttonStyle;
        private GUIStyle garageTitleStyle;

        public void Configure(EconomyGameController controller)
        {
            economy = controller;
        }

        private void OnGUI()
        {
            if (economy == null || !economy.IsChoosingJob)
            {
                return;
            }

            EnsureStyles();
            var scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.25f);
            GUI.color = new Color(0.01f, 0.015f, 0.025f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

            var panel = new Rect(
                Screen.width * 0.5f - 510f * scale,
                Screen.height * 0.5f - 310f * scale,
                1020f * scale,
                620f * scale);
            GUI.color = new Color(0.025f, 0.035f, 0.05f, 0.98f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(
                new Rect(panel.x + 30f * scale, panel.y + 20f * scale, 550f * scale, 48f * scale),
                "CHOOSE A DELIVERY",
                titleStyle);
            GUI.Label(
                new Rect(panel.xMax - 280f * scale, panel.y + 24f * scale, 250f * scale, 42f * scale),
                $"BALANCE  ${economy.Balance:N0}",
                subtitleStyle);

            const float cardTop = 88f;
            const float cardHeight = 300f;
            var gap = 18f * scale;
            var cardWidth = (panel.width - 60f * scale - gap * 2f) / 3f;
            for (var index = 0; index < economy.JobCount; index++)
            {
                var job = economy.GetJob(index);
                var card = new Rect(
                    panel.x + 30f * scale + index * (cardWidth + gap),
                    panel.y + cardTop * scale,
                    cardWidth,
                    cardHeight * scale);
                DrawJobCard(card, job, index, scale);
            }

            DrawGarage(panel, scale);
            GUI.color = Color.white;
            GUI.enabled = true;
        }

        private void DrawJobCard(Rect card, DeliveryJobDefinition job, int index, float scale)
        {
            GUI.color = new Color(0.065f, 0.085f, 0.115f, 1f);
            GUI.DrawTexture(card, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(card.x + 18f * scale, card.y + 16f * scale, card.width - 36f * scale, 36f * scale), job.Title, cardTitleStyle);
            GUI.Label(new Rect(card.x + 18f * scale, card.y + 62f * scale, card.width - 36f * scale, 28f * scale), "CARGO  Hotel Supplies", bodyStyle);
            GUI.Label(new Rect(card.x + 18f * scale, card.y + 96f * scale, card.width - 36f * scale, 28f * scale), $"DIFFICULTY  {job.Difficulty}", bodyStyle);
            GUI.Label(new Rect(card.x + 18f * scale, card.y + 130f * scale, card.width - 36f * scale, 28f * scale), $"CARGO RISK  {job.CargoRisk}", bodyStyle);
            GUI.Label(new Rect(card.x + 18f * scale, card.y + 164f * scale, card.width - 36f * scale, 28f * scale), $"FUEL  {job.FuelPressure}", bodyStyle);
            GUI.Label(new Rect(card.x + 18f * scale, card.y + 202f * scale, card.width - 36f * scale, 36f * scale), $"${job.Payout:N0}", payoutStyle);

            var button = new Rect(card.x + 18f * scale, card.yMax - 52f * scale, card.width - 36f * scale, 36f * scale);
            if (GUI.Button(button, "TAKE JOB", buttonStyle))
            {
                economy.SelectJob(index);
            }
        }

        private void DrawGarage(Rect panel, float scale)
        {
            var garage = new Rect(
                panel.x + 30f * scale,
                panel.y + 414f * scale,
                panel.width - 60f * scale,
                170f * scale);
            GUI.color = new Color(0.045f, 0.06f, 0.08f, 1f);
            GUI.DrawTexture(garage, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(
                new Rect(garage.x + 20f * scale, garage.y + 14f * scale, 420f * scale, 34f * scale),
                "GARAGE  •  FUEL EFFICIENCY",
                garageTitleStyle);
            GUI.Label(
                new Rect(garage.x + 20f * scale, garage.y + 55f * scale, 560f * scale, 54f * scale),
                economy.EfficiencyUpgradeOwned
                    ? "INSTALLED — 20% LESS FUEL USED"
                    : "UPGRADE THE CURRENT TRUCK TO USE 20% LESS FUEL",
                bodyStyle);

            var button = new Rect(garage.xMax - 250f * scale, garage.y + 50f * scale, 220f * scale, 48f * scale);
            if (economy.EfficiencyUpgradeOwned)
            {
                GUI.enabled = false;
                GUI.Button(button, "INSTALLED", buttonStyle);
            }
            else
            {
                GUI.enabled = economy.Balance >= economy.EfficiencyUpgradeCost;
                if (GUI.Button(button, $"BUY  ${economy.EfficiencyUpgradeCost:N0}", buttonStyle))
                {
                    economy.PurchaseEfficiencyUpgrade();
                }
            }

            GUI.enabled = true;
            if (!economy.EfficiencyUpgradeOwned && economy.Balance < economy.EfficiencyUpgradeCost)
            {
                GUI.Label(
                    new Rect(garage.xMax - 300f * scale, garage.y + 108f * scale, 270f * scale, 28f * scale),
                    "EARN CASH TO UNLOCK",
                    bodyStyle);
            }
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            subtitleStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
            cardTitleStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            bodyStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleLeft, wordWrap = false, clipping = TextClipping.Clip };
            payoutStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(0.2f, 1f, 0.72f) } };
            buttonStyle ??= new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            garageTitleStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(1f, 0.68f, 0.12f) } };
        }
    }
}
