using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartiesScreenController : MonoBehaviour
{
    [Header("Parliament UI")]

    [SerializeField]
    private Transform partyListContent;

    [SerializeField]
    private PartyParliamentRowUI partyRowPrefab;

    [SerializeField]
    private TMP_Text parliamentSummaryText;


    private PoliticalPartyManager politicalPartyManager;

    private readonly List<PartyParliamentRowUI> createdRows =
        new List<PartyParliamentRowUI>();


    private void Start()
    {
        FindPoliticalPartyManager();

        RefreshParliament();
    }


    private void FindPoliticalPartyManager()
    {
        politicalPartyManager =
            FindObjectOfType<PoliticalPartyManager>();

        if (politicalPartyManager == null)
        {
            Debug.LogError(
                "PartiesScreenController: no se encontró " +
                "PoliticalPartyManager. " +
                "Inicia la simulación desde la escena inicial."
            );
        }
    }


    public void RefreshParliament()
    {
        if (politicalPartyManager == null)
        {
            FindPoliticalPartyManager();

            if (politicalPartyManager == null)
            {
                return;
            }
        }

        ClearRows();

        int totalSeats =
            politicalPartyManager.TotalSeats;


        // Actualizar el resumen del Parlamento.

        if (parliamentSummaryText != null)
        {
            parliamentSummaryText.text =
                $"{totalSeats} ASIENTOS";
        }


        // Copiamos los estados para poder ordenarlos
        // sin modificar la lista original del manager.

        List<PoliticalPartyState> orderedParties =
            new List<PoliticalPartyState>();

        foreach (
            PoliticalPartyState partyState
            in politicalPartyManager.PartyStates
        )
        {
            if (
                partyState != null &&
                partyState.party != null
            )
            {
                orderedParties.Add(partyState);
            }
        }


        // Mayor número de asientos primero.

        orderedParties.Sort(
            (a, b) =>
                b.seats.CompareTo(a.seats)
        );


        // Crear una fila para cada partido.

        foreach (
            PoliticalPartyState partyState
            in orderedParties
        )
        {
            PartyParliamentRowUI newRow =
                Instantiate(
                    partyRowPrefab,
                    partyListContent
                );

            newRow.Configure(
                partyState,
                totalSeats
            );

            createdRows.Add(newRow);
        }

        Debug.Log(
            $"PartiesScreenController: " +
            $"{createdRows.Count} partidos mostrados."
        );
    }


    private void ClearRows()
    {
        foreach (
            PartyParliamentRowUI row
            in createdRows
        )
        {
            if (row != null)
            {
                Destroy(row.gameObject);
            }
        }

        createdRows.Clear();
    }
}