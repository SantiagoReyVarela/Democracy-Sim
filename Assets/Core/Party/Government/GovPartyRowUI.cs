using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GovPartyRowUI : MonoBehaviour
{
    [SerializeField]
    private Image partyColor;

    [SerializeField]
    private TMP_Text partyNameText;

    [SerializeField]
    private TMP_Text seatsText;


    public void Configure(
        PoliticalPartyState partyState,
        PoliticalPartyManager politicalPartyManager
    )
    {
        if (
            partyState == null ||
            partyState.party == null
        )
        {
            gameObject.SetActive(false);
            return;
        }

        PoliticalPartySO party =
            partyState.party;

        gameObject.SetActive(true);

        if (partyColor != null)
        {
            partyColor.color =
                politicalPartyManager != null
                    ? politicalPartyManager.GetEffectivePartyColor(partyState)
                    : party.partyColor;
        }

        if (partyNameText != null)
        {
            partyNameText.text =
                politicalPartyManager != null
                    ? politicalPartyManager.GetEffectivePartyName(partyState)
                    : party.partyName;
        }

        if (seatsText != null)
        {
            seatsText.text =
                partyState.seats.ToString();
        }
    }
}