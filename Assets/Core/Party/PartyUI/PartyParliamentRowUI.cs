using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyParliamentRowUI : MonoBehaviour
{
    [Header("Party")]
    [SerializeField]
    private Image partyLogo;

    [SerializeField]
    private Image partyColorStripe;

    [SerializeField]
    private TMP_Text partyNameText;

    [SerializeField]
    private TMP_Text ideologicalLineText;


    [Header("Parliament")]
    [SerializeField]
    private TMP_Text seatsText;

    [SerializeField]
    private TMP_Text percentageText;

    [SerializeField]
    private GameObject governmentIndicator;


    public void Configure(
        PoliticalPartyState partyState,
        int totalSeats
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


        // Nombre

        if (partyNameText != null)
        {
            partyNameText.text =
                party.partyName;
        }


        // Línea ideológica

        if (ideologicalLineText != null)
        {
            ideologicalLineText.text =
                party.IdeologicalLine;
        }


        // Color

        if (partyColorStripe != null)
        {
            partyColorStripe.color =
                party.partyColor;
        }


        // Logo

        if (partyLogo != null)
        {
            partyLogo.sprite =
                party.logo;

            partyLogo.enabled =
                party.logo != null;
        }


        // Asientos

        if (seatsText != null)
        {
            seatsText.text =
                partyState.seats.ToString();
        }


        // Porcentaje parlamentario

        float percentage = 0f;

        if (totalSeats > 0)
        {
            percentage =
                partyState.seats /
                (float)totalSeats *
                100f;
        }

        if (percentageText != null)
        {
            percentageText.text =
                $"{percentage:0.0}%";
        }


        // Partido gubernamental

        if (governmentIndicator != null)
        {
            governmentIndicator.SetActive(
                partyState.isGovernment
            );
        }
    }
}