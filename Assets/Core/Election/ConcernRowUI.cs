using TMPro;
using UnityEngine;

public class ConcernRowUI : MonoBehaviour
{
    [Header("Texts")]

    [SerializeField]
    private TMP_Text concernNameText;

    [SerializeField]
    private TMP_Text importanceText;


    [Header("Bar")]

    [SerializeField]
    private RectTransform importanceFill;


    private float currentNormalizedValue = 1f;


    // =========================================================
    // CONFIGURAR
    // =========================================================

    public void Configure(
        PublicConcernData concern
    )
    {
        if (concern == null)
        {
            return;
        }


        if (concernNameText != null)
        {
            concernNameText.text =
                concern.DisplayName;
        }


        if (importanceText != null)
        {
            importanceText.text =
                $"{concern.importance:F1}%";
        }


        currentNormalizedValue =
            Mathf.Clamp01(
                concern.importance / 100f
            );


        UpdateBar();
    }


    // =========================================================
    // ACTUALIZAR BARRA
    // =========================================================

    private void UpdateBar()
    {
        if (importanceFill == null)
        {
            return;
        }


        importanceFill.anchorMin =
            new Vector2(
                0f,
                0f
            );


        importanceFill.anchorMax =
            new Vector2(
                currentNormalizedValue,
                1f
            );


        importanceFill.offsetMin =
            Vector2.zero;


        importanceFill.offsetMax =
            Vector2.zero;
    }


    // =========================================================
    // SI UNITY RECALCULA EL LAYOUT
    // VOLVEMOS A APLICAR EL PORCENTAJE
    // =========================================================

    protected void OnRectTransformDimensionsChange()
    {
        UpdateBar();
    }
}