using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartiesScreenController : MonoBehaviour
{
    // =========================================================
    // PARLAMENTO
    // =========================================================

    [Header("Parliament UI")]

    [SerializeField]
    private Transform partyListContent;

    [SerializeField]
    private PartyParliamentRowUI partyRowPrefab;

    [SerializeField]
    private TMP_Text parliamentSummaryText;


    // =========================================================
    // HEMICICLO
    // =========================================================

    [Header("Hemicycle")]

    [SerializeField]
    private ParliamentHemicycleGraphic parliamentHemicycle;


    // =========================================================
    // GOBIERNO
    // =========================================================

    [Header("Government UI")]

    [SerializeField]
    private Transform governmentPartiesContainer;

    [SerializeField]
    private GovPartyRowUI governmentPartyRowPrefab;

    [SerializeField]
    private TMP_Text governmentSeatsText;

    [SerializeField]
    private TMP_Text coalitionTypeText;


    // =========================================================
    // MANAGER
    // =========================================================

    private PoliticalPartyManager politicalPartyManager;


    // =========================================================
    // FILAS CREADAS
    // =========================================================

    private readonly List<PartyParliamentRowUI> createdRows =
        new List<PartyParliamentRowUI>();

    private readonly List<GovPartyRowUI> createdGovernmentRows =
        new List<GovPartyRowUI>();


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPoliticalPartyManager();

        if (politicalPartyManager == null)
        {
            return;
        }

        RefreshParliament();
        RefreshGovernment();
    }


    // =========================================================
    // BUSCAR MANAGER
    // =========================================================

    private void FindPoliticalPartyManager()
    {
        politicalPartyManager =
            FindObjectOfType<PoliticalPartyManager>();

        if (politicalPartyManager == null)
        {
            Debug.LogError(
                "PartiesScreenController: no se encontró " +
                "PoliticalPartyManager."
            );
        }
    }


    // =========================================================
    // REFRESCAR TODO
    // =========================================================

    public void RefreshAll()
    {
        if (politicalPartyManager == null)
        {
            FindPoliticalPartyManager();

            if (politicalPartyManager == null)
            {
                return;
            }
        }

        RefreshParliament();
        RefreshGovernment();
    }


    // =========================================================
    // PARLAMENTO
    // =========================================================

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

        ClearParliamentRows();

        int totalSeats =
            politicalPartyManager.TotalSeats;


        // -----------------------------------------------------
        // TOTAL DE ASIENTOS
        // -----------------------------------------------------

        if (parliamentSummaryText != null)
        {
            parliamentSummaryText.text =
                $"{totalSeats} ASIENTOS";
        }


        // -----------------------------------------------------
        // OBTENER PARTIDOS
        // -----------------------------------------------------

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
                orderedParties.Add(
                    partyState
                );
            }
        }


        // -----------------------------------------------------
        // ORDENAR POR ASIENTOS
        // -----------------------------------------------------

        orderedParties.Sort(
            (a, b) =>
                b.seats.CompareTo(a.seats)
        );


        // -----------------------------------------------------
        // CREAR FILAS
        // -----------------------------------------------------

        if (
            partyRowPrefab != null &&
            partyListContent != null
        )
        {
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

                createdRows.Add(
                    newRow
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "PartiesScreenController: Party Row Prefab " +
                "o Party List Content no están asignados."
            );
        }


        // -----------------------------------------------------
        // HEMICICLO
        // -----------------------------------------------------

        Debug.Log(
            "PartiesScreenController: intentando refrescar el hemiciclo."
        );

        if (parliamentHemicycle != null)
        {
            Debug.Log(
                "PartiesScreenController: Parliament Hemicycle encontrado."
            );

            parliamentHemicycle.Refresh(
                politicalPartyManager
            );
        }
        else
        {
            Debug.LogError(
                "PartiesScreenController: Parliament Hemicycle es NULL."
            );
        }


        Debug.Log(
            $"PartiesScreenController: " +
            $"{createdRows.Count} partidos mostrados."
        );
    }


    // =========================================================
    // GOBIERNO
    // =========================================================

    public void RefreshGovernment()
    {
        if (politicalPartyManager == null)
        {
            FindPoliticalPartyManager();

            if (politicalPartyManager == null)
            {
                return;
            }
        }

        ClearGovernmentRows();

        List<PoliticalPartyState> governmentParties =
            new List<PoliticalPartyState>();

        int governmentSeats = 0;


        foreach (
            PoliticalPartyState partyState
            in politicalPartyManager.PartyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null ||
                !partyState.isGovernment
            )
            {
                continue;
            }

            governmentParties.Add(
                partyState
            );

            governmentSeats +=
                partyState.seats;
        }


        // Ordenar partidos gubernamentales por asientos.

        governmentParties.Sort(
            (a, b) =>
                b.seats.CompareTo(a.seats)
        );


        // Crear sus filas.

        if (
            governmentPartyRowPrefab != null &&
            governmentPartiesContainer != null
        )
        {
            foreach (
                PoliticalPartyState partyState
                in governmentParties
            )
            {
                GovPartyRowUI newRow =
                    Instantiate(
                        governmentPartyRowPrefab,
                        governmentPartiesContainer
                    );

                newRow.Configure(
                    partyState
                );

                createdGovernmentRows.Add(
                    newRow
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "PartiesScreenController: Gov Party Row Prefab " +
                "o Government Parties Container no están asignados."
            );
        }


        // Asientos gubernamentales.

        if (governmentSeatsText != null)
        {
            governmentSeatsText.text =
                $"{governmentSeats} / " +
                $"{politicalPartyManager.TotalSeats} ASIENTOS";
        }


        // Tipo de coalición.

        if (coalitionTypeText != null)
        {
            coalitionTypeText.text =
                GetCoalitionTypeDisplayName(
                    politicalPartyManager.CoalitionType
                );
        }
    }


    // =========================================================
    // BORRAR FILAS PARLAMENTO
    // =========================================================

    private void ClearParliamentRows()
    {
        foreach (
            PartyParliamentRowUI row
            in createdRows
        )
        {
            if (row != null)
            {
                Destroy(
                    row.gameObject
                );
            }
        }

        createdRows.Clear();
    }


    // =========================================================
    // BORRAR FILAS GOBIERNO
    // =========================================================

    private void ClearGovernmentRows()
    {
        foreach (
            GovPartyRowUI row
            in createdGovernmentRows
        )
        {
            if (row != null)
            {
                Destroy(
                    row.gameObject
                );
            }
        }

        createdGovernmentRows.Clear();
    }


    // =========================================================
    // NOMBRE DEL TIPO DE COALICIÓN
    // =========================================================

    private string GetCoalitionTypeDisplayName(
        GovernmentCoalitionType type
    )
    {
        switch (type)
        {
            case GovernmentCoalitionType.NoGovernment:
                return "Sin gobierno";

            case GovernmentCoalitionType.SinglePartyMajority:
                return "Gobierno de mayoría";

            case GovernmentCoalitionType.SinglePartyMinority:
                return "Gobierno en minoría";

            case GovernmentCoalitionType.MajorityCoalition:
                return "Coalición mayoritaria";

            case GovernmentCoalitionType.MinorityCoalition:
                return "Coalición minoritaria";

            case GovernmentCoalitionType.GrandCoalition:
                return "Gran coalición";

            case GovernmentCoalitionType.CaretakerGovernment:
                return "Gobierno en funciones";

            default:
                return type.ToString();
        }
    }
}