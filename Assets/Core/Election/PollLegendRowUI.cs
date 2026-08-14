using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PollLegendRowUI : MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    private Image partyColor;

    [SerializeField]
    private TMP_Text partyNameText;

    [SerializeField]
    private TMP_Text pollingText;


    // =========================================================
    // CONFIGURAR
    // =========================================================

    public void Configure(
        PollPartyData partyData
    )
    {
        if (
            partyData == null ||
            partyData.party == null
        )
        {
            return;
        }


        if (partyColor != null)
        {
            partyColor.color =
                partyData.party.partyColor;
        }


        if (partyNameText != null)
        {
            partyNameText.text =
                partyData.party.partyName;
        }


        if (pollingText != null)
        {
            pollingText.text =
                $"{partyData.currentPolling:F1}%";
        }
    }
}