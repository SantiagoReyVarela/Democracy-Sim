using TMPro;
using UnityEngine;

public class CountryConcernRowUI : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]

    [SerializeField]
    private TMP_Text concernNameText;

    [SerializeField]
    private TMP_Text importanceText;

    [SerializeField]
    private RectTransform barFill;


    // =========================================================
    // CONFIGURAR
    // =========================================================

    public void Configure(
        string concernName,
        float importance
    )
    {
        // -----------------------------------------------------
        // NOMBRE
        // -----------------------------------------------------

        if (concernNameText != null)
        {
            concernNameText.text =
                concernName;
        }


        // -----------------------------------------------------
        // PORCENTAJE
        // -----------------------------------------------------

        if (importanceText != null)
        {
            importanceText.text =
                $"{importance:0.#}%";
        }


        // -----------------------------------------------------
        // BARRA
        // -----------------------------------------------------

        if (barFill != null)
        {
            float normalized =
                Mathf.Clamp01(
                    importance / 100f
                );


            Vector2 anchorMax =
                barFill.anchorMax;


            anchorMax.x =
                normalized;


            barFill.anchorMax =
                anchorMax;


            barFill.offsetMin =
                Vector2.zero;

            barFill.offsetMax =
                Vector2.zero;
        }
    }
}