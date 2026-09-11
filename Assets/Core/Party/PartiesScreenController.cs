using System;
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

    [SerializeField]
    private TMP_Text governmentLeaderText;


    // =========================================================
    // ESTADO DEL GOBIERNO
    // =========================================================

    [Header("Government Status")]

    [SerializeField]
    private TMP_Text satisfactionText;

    [SerializeField]
    private TMP_Text trendText;


    // =========================================================
    // PERFIL IDEOLÓGICO DEL GOBIERNO
    // =========================================================

    [Header("Government Ideology")]

    [SerializeField]
    private Transform ideologyRowsContainer;

    [SerializeField]
    private GovIdeologyRowUI ideologyRowPrefab;


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

    private readonly List<GovIdeologyRowUI> createdIdeologyRows =
        new List<GovIdeologyRowUI>();


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
                b.seats.CompareTo(
                    a.seats
                )
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
                    totalSeats,
                    politicalPartyManager
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

        if (parliamentHemicycle != null)
        {
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


        // -----------------------------------------------------
        // OBTENER PARTIDOS DEL GOBIERNO
        // -----------------------------------------------------

        List<PoliticalPartyState> governmentParties =
            new List<PoliticalPartyState>();


        int governmentSeats =
            0;


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


        // -----------------------------------------------------
        // ORDENAR PARTIDOS DEL GOBIERNO
        // -----------------------------------------------------

        governmentParties.Sort(
            (a, b) =>
                b.seats.CompareTo(
                    a.seats
                )
        );


        // -----------------------------------------------------
        // CREAR FILAS DEL GOBIERNO
        // -----------------------------------------------------

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
                    partyState,
                    politicalPartyManager
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


        // -----------------------------------------------------
        // ASIENTOS DEL GOBIERNO
        // -----------------------------------------------------

        if (governmentSeatsText != null)
        {
            governmentSeatsText.text =
                $"{governmentSeats} / " +
                $"{politicalPartyManager.TotalSeats} ASIENTOS";
        }


        // -----------------------------------------------------
        // TIPO DE COALICIÓN
        // -----------------------------------------------------

        if (coalitionTypeText != null)
        {
            coalitionTypeText.text =
                GetCoalitionTypeDisplayName(
                    politicalPartyManager.CoalitionType
                );
        }


        // -----------------------------------------------------
        // PARTIDO LÍDER
        // -----------------------------------------------------

        if (governmentLeaderText != null)
        {
            PoliticalPartyState leader =
                politicalPartyManager.GovernmentLeader;


            if (
                leader != null &&
                leader.party != null
            )
            {
                governmentLeaderText.text =
                    $"Leading Party: " +
                    $"{politicalPartyManager.GetEffectivePartyName(leader)}";
            }
            else
            {
                governmentLeaderText.text =
                    "Leading Party: -";
            }
        }


        // -----------------------------------------------------
        // SATISFACCIÓN DEL GOBIERNO
        // -----------------------------------------------------

        float governmentSatisfaction =
            CalculateGovernmentSatisfaction(
                governmentParties
            );


        if (satisfactionText != null)
        {
            if (governmentParties.Count > 0)
            {
                satisfactionText.text =
                    $"Satisfaction: " +
                    $"{governmentSatisfaction:F1}%";
            }
            else
            {
                satisfactionText.text =
                    "Satisfaction: -";
            }
        }


        // -----------------------------------------------------
        // TENDENCIA DEL GOBIERNO
        // -----------------------------------------------------

        float governmentTrend =
            CalculateGovernmentTrend(
                governmentParties
            );


        if (trendText != null)
        {
            trendText.text =
                GetGovernmentTrendDisplay(
                    governmentTrend,
                    governmentParties.Count > 0
                );
        }


        // -----------------------------------------------------
        // PERFIL IDEOLÓGICO DEL GOBIERNO
        // -----------------------------------------------------

        RefreshGovernmentIdeology(
            governmentParties
        );
    }


    // =========================================================
    // PERFIL IDEOLÓGICO DEL GOBIERNO
    // =========================================================

    private void RefreshGovernmentIdeology(
        List<PoliticalPartyState> governmentParties
    )
    {
        ClearIdeologyRows();


        if (
            governmentParties == null ||
            governmentParties.Count == 0
        )
        {
            return;
        }


        if (
            ideologyRowsContainer == null ||
            ideologyRowPrefab == null
        )
        {
            Debug.LogWarning(
                "PartiesScreenController: Ideology Rows Container " +
                "o Ideology Row Prefab no están asignados."
            );

            return;
        }


        // -----------------------------------------------------
        // CREAR UNA FILA POR CADA EJE POLÍTICO
        // -----------------------------------------------------

        foreach (
            PoliticalAxis axis
            in Enum.GetValues(
                typeof(PoliticalAxis)
            )
        )
        {
            float averagePosition =
                CalculateGovernmentAxisPosition(
                    governmentParties,
                    axis
                );


            GovIdeologyRowUI newRow =
                Instantiate(
                    ideologyRowPrefab,
                    ideologyRowsContainer
                );


            newRow.Configure(
                axis,
                averagePosition
            );


            createdIdeologyRows.Add(
                newRow
            );
        }
    }


    // =========================================================
    // POSICIÓN MEDIA DEL GOBIERNO EN UN EJE
    //
    // MEDIA PONDERADA POR ASIENTOS
    // =========================================================

    private float CalculateGovernmentAxisPosition(
        List<PoliticalPartyState> governmentParties,
        PoliticalAxis axis
    )
    {
        if (
            governmentParties == null ||
            governmentParties.Count == 0
        )
        {
            return 0f;
        }


        float weightedPosition =
            0f;


        int totalGovernmentSeats =
            0;


        foreach (
            PoliticalPartyState partyState
            in governmentParties
        )
        {
            if (
                partyState == null ||
                partyState.party == null ||
                partyState.seats <= 0
            )
            {
                continue;
            }


            float partyPosition =
                politicalPartyManager.GetEffectivePartyPosition(
                    partyState,
                    axis
                );


            weightedPosition +=
                partyPosition *
                partyState.seats;


            totalGovernmentSeats +=
                partyState.seats;
        }


        if (totalGovernmentSeats <= 0)
        {
            return 0f;
        }


        return
            weightedPosition /
            totalGovernmentSeats;
    }


    // =========================================================
    // CALCULAR SATISFACCIÓN DEL GOBIERNO
    //
    // MEDIA PONDERADA POR ASIENTOS
    // =========================================================

    private float CalculateGovernmentSatisfaction(
        List<PoliticalPartyState> governmentParties
    )
    {
        if (
            governmentParties == null ||
            governmentParties.Count == 0
        )
        {
            return 0f;
        }


        float weightedSatisfaction =
            0f;


        int totalGovernmentSeats =
            0;


        foreach (
            PoliticalPartyState partyState
            in governmentParties
        )
        {
            if (
                partyState == null ||
                partyState.party == null ||
                partyState.seats <= 0
            )
            {
                continue;
            }


            weightedSatisfaction +=
                partyState.satisfaction *
                partyState.seats;


            totalGovernmentSeats +=
                partyState.seats;
        }


        if (totalGovernmentSeats <= 0)
        {
            return 0f;
        }


        return
            weightedSatisfaction /
            totalGovernmentSeats;
    }


    // =========================================================
    // CALCULAR TENDENCIA DEL GOBIERNO
    //
    // MEDIA PONDERADA POR ASIENTOS
    // =========================================================

    private float CalculateGovernmentTrend(
        List<PoliticalPartyState> governmentParties
    )
    {
        if (
            governmentParties == null ||
            governmentParties.Count == 0
        )
        {
            return 0f;
        }


        float weightedTrend =
            0f;


        int totalGovernmentSeats =
            0;


        foreach (
            PoliticalPartyState partyState
            in governmentParties
        )
        {
            if (
                partyState == null ||
                partyState.party == null ||
                partyState.seats <= 0
            )
            {
                continue;
            }


            weightedTrend +=
                partyState.lastSatisfactionChange *
                partyState.seats;


            totalGovernmentSeats +=
                partyState.seats;
        }


        if (totalGovernmentSeats <= 0)
        {
            return 0f;
        }


        return
            weightedTrend /
            totalGovernmentSeats;
    }


    // =========================================================
    // TEXTO DE TENDENCIA
    // =========================================================

    private string GetGovernmentTrendDisplay(
        float trend,
        bool hasGovernment
    )
    {
        if (!hasGovernment)
        {
            return "Trend: -";
        }


        const float stableThreshold =
            0.05f;


        if (trend > stableThreshold)
        {
            return
                $"Trend: ▲ +{trend:F2}";
        }


        if (trend < -stableThreshold)
        {
            return
                $"Trend: ▼ {trend:F2}";
        }


        return
            "Trend: — Stable";
    }


    // =========================================================
    // BORRAR FILAS DEL PARLAMENTO
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
    // BORRAR FILAS DEL GOBIERNO
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
    // BORRAR FILAS IDEOLÓGICAS
    // =========================================================

    private void ClearIdeologyRows()
    {
        foreach (
            GovIdeologyRowUI row
            in createdIdeologyRows
        )
        {
            if (row != null)
            {
                Destroy(
                    row.gameObject
                );
            }
        }


        createdIdeologyRows.Clear();
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