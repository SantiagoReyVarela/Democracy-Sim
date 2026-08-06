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

            PoliticalPartyState newState = new PoliticalPartyState
            {
                party = party,
                popularity = initialPopularity,
                satisfaction = 50f,
                seats = 0,
                isGovernment = false
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
}