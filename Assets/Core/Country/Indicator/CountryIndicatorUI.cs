using TMPro;
using UnityEngine;

public class CountryIndicatorUI : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text valueText;

    [SerializeField]
    private TMP_Text trendText;


    // =========================================================
    // CONFIGURAR
    // =========================================================

    public void Configure(
        string indicatorName,
        float currentValue,
        float change
    )
    {
        // -----------------------------------------------------
        // NOMBRE
        // -----------------------------------------------------

        if (nameText != null)
        {
            nameText.text =
                indicatorName.ToUpper();
        }


        // -----------------------------------------------------
        // VALOR ACTUAL
        // -----------------------------------------------------

        if (valueText != null)
        {
            valueText.text =
                $"{currentValue:0.#}";
        }


        // -----------------------------------------------------
        // TENDENCIA
        // -----------------------------------------------------

        RefreshTrend(change);
    }


    // =========================================================
    // TENDENCIA
    // =========================================================

    private void RefreshTrend(
        float change
    )
    {
        if (trendText == null)
        {
            return;
        }


        // Pequeñísimo cambio = lo consideramos estable.

        if (Mathf.Abs(change) < 0.01f)
        {
            trendText.text =
                "→ 0";

            trendText.color =
                Color.white;

            return;
        }


        // -----------------------------------------------------
        // SUBIDA
        // -----------------------------------------------------

        if (change > 0f)
        {
            trendText.text =
                $"▲ +{change:0.#}";

            trendText.color =
                new Color(
                    0.35f,
                    0.85f,
                    0.35f
                );

            return;
        }


        // -----------------------------------------------------
        // BAJADA
        // -----------------------------------------------------

        trendText.text =
            $"▼ {change:0.#}";

        trendText.color =
            new Color(
                0.9f,
                0.3f,
                0.3f
            );
    }
}