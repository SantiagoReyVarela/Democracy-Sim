using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GovernmentPartyRowUI : MonoBehaviour
{
    [SerializeField]
    private Image partyColor;

    [SerializeField]
    private TMP_Text partyNameText;

    [SerializeField]
    private TMP_Text seatsText;


    public void Configure(
        PoliticalPartyState partyState
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
                party.partyColor;
        }

        if (partyNameText != null)
        {
            partyNameText.text =
                party.partyName;
        }

        if (seatsText != null)
        {
            seatsText.text =
                partyState.seats.ToString();
        }
    }
}