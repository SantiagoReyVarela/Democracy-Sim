using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class PoliticalPartyManager : MonoBehaviour
{
    [Header("Partidos disponibles")]
    [SerializeField]
    private List<PoliticalPartySO> availableParties =
        new List<PoliticalPartySO>();

    [Header("Parlamento")]

    [SerializeField, Min(1)]
    private int parliamentSeatCount = 100;

    [SerializeField]
    private bool generateRandomParliament = true;

    [SerializeField, Min(0)]
    private int minimumSeatsPerParty = 1;

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

        if (generateRandomParliament)
        {
            GenerateRandomSeats();
        }

        FormAutomaticGovernment();

        Debug.Log(
            $"PoliticalPartyManager: " +
            $"{partyStates.Count} partidos inicializados."
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

    private void GenerateRandomSeats()
    {
        if (partyStates == null || partyStates.Count == 0)
        {
            return;
        }

        int partyCount = partyStates.Count;

        int minimumTotal =
            minimumSeatsPerParty * partyCount;

        if (minimumTotal > parliamentSeatCount)
        {
            Debug.LogError(
                "El mínimo de asientos por partido supera " +
                "el tamaño total del Parlamento."
            );

            return;
        }

        // Primero garantizamos el mínimo.
        foreach (PoliticalPartyState partyState in partyStates)
        {
            partyState.seats =
                minimumSeatsPerParty;
        }

        int remainingSeats =
            parliamentSeatCount - minimumTotal;


        // Creamos pesos aleatorios para cada partido.

        float[] weights =
            new float[partyCount];

        float totalWeight = 0f;

        for (int i = 0; i < partyCount; i++)
        {
            weights[i] =
                Random.Range(0.25f, 1f);

            totalWeight += weights[i];
        }


        // Primera distribución proporcional.

        int distributedSeats = 0;

        for (int i = 0; i < partyCount; i++)
        {
            int seatsToAdd =
                Mathf.FloorToInt(
                    remainingSeats *
                    (weights[i] / totalWeight)
                );

            partyStates[i].seats +=
                seatsToAdd;

            distributedSeats +=
                seatsToAdd;
        }


        // FloorToInt puede dejar algunos asientos sin repartir.
        // Los distribuimos aleatoriamente.

        int seatsLeft =
            remainingSeats - distributedSeats;

        while (seatsLeft > 0)
        {
            int randomParty =
                Random.Range(
                    0,
                    partyStates.Count
                );

            partyStates[randomParty].seats++;

            seatsLeft--;
        }


        Debug.Log(
            $"Parlamento generado aleatoriamente: " +
            $"{TotalSeats} asientos."
        );
    }

    private void FormAutomaticGovernment()
    {
        if (partyStates == null || partyStates.Count == 0)
        {
            return;
        }

        foreach (PoliticalPartyState partyState in partyStates)
        {
            partyState.isGovernment = false;
        }

        int totalSeats = TotalSeats;

        if (totalSeats <= 0)
        {
            coalitionType =
                GovernmentCoalitionType.NoGovernment;

            return;
        }

        int majorityThreshold =
            totalSeats / 2 + 1;


        // 1. Comprobar si existe mayoría absoluta
        // de un único partido.

        foreach (PoliticalPartyState partyState in partyStates)
        {
            if (partyState.seats >= majorityThreshold)
            {
                partyState.isGovernment = true;

                coalitionType =
                    GovernmentCoalitionType.SinglePartyMajority;

                Debug.Log(
                    $"Gobierno formado por " +
                    $"{partyState.party.partyName} " +
                    $"con {partyState.seats} asientos."
                );

                return;
            }
        }


        // 2. Buscar la mejor coalición posible.

        List<PoliticalPartyState> bestCoalition =
            FindBestMajorityCoalition(
                majorityThreshold
            );

        if (bestCoalition == null ||
            bestCoalition.Count == 0)
        {
            coalitionType =
                GovernmentCoalitionType.NoGovernment;

            Debug.LogWarning(
                "No ha sido posible formar gobierno."
            );

            return;
        }


        int governmentSeats = 0;

        foreach (
            PoliticalPartyState partyState
            in bestCoalition
        )
        {
            partyState.isGovernment = true;
            governmentSeats += partyState.seats;
        }

        coalitionType =
            GovernmentCoalitionType.MajorityCoalition;

        Debug.Log(
            $"Coalición formada con " +
            $"{governmentSeats}/{totalSeats} asientos."
        );

        foreach (
            PoliticalPartyState partyState
            in bestCoalition
        )
        {
            Debug.Log(
                $"GOBIERNO | " +
                $"{partyState.party.partyName} | " +
                $"{partyState.seats} asientos"
            );
        }
    }

    private List<PoliticalPartyState> FindBestMajorityCoalition(int majorityThreshold)
    {
        List<PoliticalPartyState> bestCoalition =
            null;

        float bestScore =
            float.MaxValue;

        int partyCount =
            partyStates.Count;

        int combinationCount =
            1 << partyCount;


        for (
            int mask = 1;
            mask < combinationCount;
            mask++
        )
        {
            List<PoliticalPartyState> coalition =
                new List<PoliticalPartyState>();

            int coalitionSeats = 0;


            for (
                int i = 0;
                i < partyCount;
                i++
            )
            {
                if ((mask & (1 << i)) != 0)
                {
                    PoliticalPartyState partyState =
                        partyStates[i];

                    coalition.Add(partyState);

                    coalitionSeats +=
                        partyState.seats;
                }
            }


            // Ya hemos comprobado gobiernos
            // de un solo partido.

            if (coalition.Count < 2)
            {
                continue;
            }


            if (coalitionSeats < majorityThreshold)
            {
                continue;
            }


            float ideologicalDistance =
                CalculateCoalitionIdeologicalDistance(
                    coalition
                );


            // Penalizamos ligeramente:
            // - tener demasiados partidos
            // - tener muchos asientos innecesarios

            int excessSeats =
                coalitionSeats -
                majorityThreshold;

            float score =
                ideologicalDistance +
                (coalition.Count - 2) * 0.5f +
                excessSeats * 0.03f;


            if (score < bestScore)
            {
                bestScore = score;
                bestCoalition = coalition;
            }
        }


        return bestCoalition;
    }
    private float CalculateCoalitionIdeologicalDistance(
        List<PoliticalPartyState> coalition
    )
    {
        if (
            coalition == null ||
            coalition.Count <= 1
        )
        {
            return 0f;
        }

        float totalDistance = 0f;
        int comparisons = 0;


        for (
            int i = 0;
            i < coalition.Count;
            i++
        )
        {
            for (
                int j = i + 1;
                j < coalition.Count;
                j++
            )
            {
                PoliticalPartySO partyA =
                    coalition[i].party;

                PoliticalPartySO partyB =
                    coalition[j].party;


                foreach (
                    PoliticalAxis axis
                    in Enum.GetValues(
                        typeof(PoliticalAxis)
                    )
                )
                {
                    float positionA =
                        partyA.GetPosition(axis);

                    float positionB =
                        partyB.GetPosition(axis);

                    totalDistance +=
                        Mathf.Abs(
                            positionA - positionB
                        );

                    comparisons++;
                }
            }
        }


        if (comparisons == 0)
        {
            return 0f;
        }

        return totalDistance /
            comparisons;
    }
}