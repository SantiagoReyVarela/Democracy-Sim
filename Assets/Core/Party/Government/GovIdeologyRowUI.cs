using TMPro;
using UnityEngine;

public class GovIdeologyRowUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text axisNameText;

    [SerializeField]
    private RectTransform positionMarker;

    [SerializeField]
    private TMP_Text positionText;


    public void Configure(
        PoliticalAxis axis,
        float position
    )
    {
        position =
            Mathf.Clamp(
                position,
                -5f,
                5f
            );


        if (axisNameText != null)
        {
            axisNameText.text =
                axis.ToString();
        }


        if (positionText != null)
        {
            positionText.text =
                position.ToString(
                    "+0.0;-0.0;0.0"
                );
        }


        if (positionMarker != null)
        {
            float normalized =
                Mathf.InverseLerp(
                    -5f,
                    5f,
                    position
                );


            positionMarker.anchorMin =
                new Vector2(
                    normalized,
                    0.5f
                );

            positionMarker.anchorMax =
                new Vector2(
                    normalized,
                    0.5f
                );


            positionMarker.anchoredPosition =
                Vector2.zero;
        }
    }


    [ContextMenu("Test -5")]
    private void TestMinus5()
    {
        Configure(
            PoliticalAxis.Economy,
            -5f
        );
    }


    [ContextMenu("Test 0")]
    private void TestZero()
    {
        Configure(
            PoliticalAxis.Economy,
            0f
        );
    }


    [ContextMenu("Test +5")]
    private void TestPlus5()
    {
        Configure(
            PoliticalAxis.Economy,
            5f
        );
    }
}