using System.Collections.Generic;
using UnityEngine;

public class PoliticalPartyManager : MonoBehaviour
{
    [Header("Partidos disponibles")]
    [SerializeField]
    private List<PoliticalPartySO> availableParties =
        new List<PoliticalPartySO>();

    private List<PoliticalPartyState> partyStates =
        new List<PoliticalPartyState>();

    public IReadOnlyList<PoliticalPartyState> PartyStates
    {
        get { return partyStates; }
    }

    [Header("Parlamento inicial")]
    [SerializeField]
    private List<PartyParliamentSetup> initialParliament =
        new List<PartyParliamentSetup>();

    [Header("Gobierno")]
    [SerializeField]
    private GovernmentCoalitionType coalitionType =
        GovernmentCoalitionType.MajorityCoalition;

    public GovernmentCoalitionType CoalitionType =>
      coalitionType;

    public int TotalSeats
    {
        get
        {
            int totalSeats = 0;

            foreach (PoliticalPartyState partyState in partyStates)
            {
                if (partyState != null)
                {
                    totalSeats += partyState.seats;
                }
            }

            return totalSeats;
        }
    }

    private void Awake()
    {
        InitializeParties();
    }

    public void InitializeParties()
    {
        partyStates.Clear();

        if (availableParties == null || availableParties.Count == 0)
        {
            Debug.LogWarning(
                "PoliticalPartyManager: no hay partidos asignados."
            );

            return;
        }

        int validPartyCount = 0;

        foreach (PoliticalPartySO party in availableParties)
        {
            if (party != null)
            {
                validPartyCount++;
            }
        }

        if (validPartyCount == 0)
        {
            Debug.LogWarning(
                "PoliticalPartyManager: todos los partidos asignados son nulos."
            );

            return;
        }

        float initialPopularity = 100f / validPartyCount;

        foreach (PoliticalPartySO party in availableParties)
        {
            if (party == null)
            {
                continue;
            }

            PartyParliamentSetup parliamentSetup = null;

            if (initialParliament != null)
            {
                parliamentSetup = initialParliament.Find(
                    setup =>
                        setup != null &&
                        setup.Party == party
                );
            }

            PoliticalPartyState newState = new PoliticalPartyState
            {
                party = party,
                popularity = initialPopularity,
                satisfaction = 50f,
                lastSatisfactionChange = 0f,

                seats = parliamentSetup != null
                    ? parliamentSetup.Seats
                    : 0,

                isGovernment =
                    parliamentSetup != null &&
                    parliamentSetup.IsGovernment
            };

            partyStates.Add(newState);
        }

        Debug.Log(
            $"PoliticalPartyManager: {partyStates.Count} partidos inicializados."
        );
    }

    public PoliticalPartyState GetPartyState(string partyId)
    {
        foreach (PoliticalPartyState partyState in partyStates)
        {
            if (partyState.party == null)
            {
                continue;
            }

            if (partyState.party.partyId == partyId)
            {
                return partyState;
            }
        }

        Debug.LogWarning(
            $"PoliticalPartyManager: no existe el partido con ID '{partyId}'."
        );

        return null;
    }

    public float CalculatePartySatisfaction(
        PoliticalPartyState partyState
    )
    {
        if (partyState == null)
        {
            Debug.LogWarning(
                "No se puede calcular la satisfacción de un partido nulo."
            );

            return 0f;
        }

        // Provisional. Más adelante se calculará comparando
        // la ideología del partido con las políticas activas.
        partyState.satisfaction = Mathf.Clamp(
            partyState.satisfaction,
            0f,
            100f
        );

        return partyState.satisfaction;
    }

    [ContextMenu("Mostrar partidos inicializados")]
    private void DebugPartyStates()
    {
        if (partyStates == null || partyStates.Count == 0)
        {
            Debug.LogWarning(
                "PoliticalPartyManager: todavía no hay partidos inicializados."
            );

            return;
        }

        Debug.Log($"PARTIDOS INICIALIZADOS: {partyStates.Count}");

        foreach (PoliticalPartyState partyState in partyStates)
        {
            if (partyState == null || partyState.party == null)
            {
                Debug.LogWarning("Hay un estado de partido vacío.");
                continue;
            }

            Debug.Log(
                $"Partido: {partyState.party.partyName} | " +
                $"ID: {partyState.party.partyId} | " +
                $"Popularidad: {partyState.popularity:F2}% | " +
                $"Satisfacción: {partyState.satisfaction:F2} | " +
                $"Escaños: {partyState.seats}"
            );
        }
    }

    [ContextMenu("Probar reacción económica")]
    private void DebugEconomicPolicyReaction()
    {
        if (partyStates == null || partyStates.Count == 0)
        {
            Debug.LogWarning(
                "No hay partidos inicializados. Ejecuta primero la partida."
            );

            return;
        }

        PoliticalAxis testAxis = PoliticalAxis.Economy;
        float testPolicyPosition = 4.5f;
        float testRelevance = 20f;

        Debug.Log(
            "=== PRUEBA: POLÍTICA ECONÓMICA +4.5 ==="
        );

        foreach (PoliticalPartyState partyState in partyStates)
        {
            if (partyState == null || partyState.party == null)
            {
                continue;
            }

            float partyPosition =
                partyState.party.GetPosition(testAxis);

            float opinion =
                PartyPolicyEvaluator.EvaluateOpinion(
                    partyState.party,
                    testAxis,
                    testPolicyPosition
                );

            float impact =
                PartyPolicyEvaluator.CalculateSatisfactionImpact(
                    partyState.party,
                    testAxis,
                    testPolicyPosition,
                    testRelevance
                );

            Debug.Log(
                $"{partyState.party.partyName} | " +
                $"Posición: {partyPosition:F1} | " +
                $"Opinión: {opinion:F2} | " +
                $"Impacto: {impact:+0.00;-0.00;0.00}"
            );
        }
    }

    public void ApplyPolicyReaction(
        PoliticalAxis axis,
        float policyPosition,
        float policyRelevance
    )
    {
        if (partyStates == null || partyStates.Count == 0)
        {
            Debug.LogWarning(
                "PoliticalPartyManager: no hay partidos inicializados."
            );

            return;
        }

        Debug.Log(
            $"=== REACCIÓN A POLÍTICA | " +
            $"Eje: {axis} | " +
            $"Posición: {policyPosition:F1} | " +
            $"Relevancia: {policyRelevance:F1} ==="
        );

        foreach (PoliticalPartyState partyState in partyStates)
        {
            if (partyState == null || partyState.party == null)
            {
                continue;
            }

            float previousSatisfaction =
                partyState.satisfaction;

            float impact =
                PartyPolicyEvaluator.CalculateSatisfactionImpact(
                    partyState.party,
                    axis,
                    policyPosition,
                    policyRelevance
                );

            float newSatisfaction = Mathf.Clamp(
                previousSatisfaction + impact,
                0f,
                100f
            );

            partyState.satisfaction =
                newSatisfaction;

            partyState.lastSatisfactionChange =
                newSatisfaction - previousSatisfaction;

            Debug.Log(
                $"{partyState.party.partyName} | " +
                $"Impacto calculado: " +
                $"{impact:+0.00;-0.00;0.00} | " +
                $"Cambio real: " +
                $"{partyState.lastSatisfactionChange:+0.00;-0.00;0.00} | " +
                $"Satisfacción: {previousSatisfaction:F2} " +
                $"→ {newSatisfaction:F2}"
            );
        }
    }

    [ContextMenu("Aplicar política económica de prueba")]
    private void DebugApplyEconomicPolicy()
    {
        ApplyPolicyReaction(
            PoliticalAxis.Economy,
            4.5f,
            20f
        );
    }

    [ContextMenu("Mostrar parlamento actual")]
    private void DebugCurrentParliament()
    {
        if (partyStates == null || partyStates.Count == 0)
        {
            Debug.LogWarning(
                "PoliticalPartyManager: no hay partidos inicializados."
            );

            return;
        }

        int totalSeats = TotalSeats;

        Debug.Log(
            $"=== PARLAMENTO | TOTAL: {totalSeats} ASIENTOS | " +
            $"GOBIERNO: {coalitionType} ==="
        );

        foreach (PoliticalPartyState partyState in partyStates)
        {
            if (partyState == null || partyState.party == null)
            {
                continue;
            }

            float percentage = totalSeats > 0
                ? partyState.seats / (float)totalSeats * 100f
                : 0f;

            string governmentLabel =
                partyState.isGovernment
                    ? " | GOBIERNO"
                    : string.Empty;

            Debug.Log(
                $"{partyState.party.partyName} | " +
                $"Asientos: {partyState.seats} | " +
                $"Porcentaje: {percentage:F2}%" +
                governmentLabel
            );
        }
    }
}