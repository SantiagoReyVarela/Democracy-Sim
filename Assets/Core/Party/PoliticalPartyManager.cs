using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class PoliticalPartyManager : MonoBehaviour
{
    // =========================================================
    // PARTIDOS DISPONIBLES
    // =========================================================

    [Header("Partidos disponibles")]

    [SerializeField]
    private List<PoliticalPartySO> availableParties =
        new List<PoliticalPartySO>();


    // =========================================================
    // PARLAMENTO
    // =========================================================

    [Header("Parlamento")]

    [SerializeField, Min(1)]
    private int parliamentSeatCount = 1000;

    [SerializeField]
    private bool generateRandomParliament = true;

    [SerializeField, Min(0)]
    private int minimumSeatsPerParty = 1;


    // =========================================================
    // DISTRIBUCIÓN IDEOLÓGICA DE ESCAÑOS
    // =========================================================

    [Header("Distribución ideológica de escaños")]

    [SerializeField, Range(0f, 100f)]
    private float centerMainstreamWeight = 50f;

    [SerializeField, Range(0f, 100f)]
    private float hardlineWeight = 30f;

    [SerializeField, Range(0f, 100f)]
    private float radicalWeight = 20f;


    // =========================================================
    // ESTADO DE LOS PARTIDOS
    // =========================================================

    private List<PoliticalPartyState> partyStates =
        new List<PoliticalPartyState>();

    public IReadOnlyList<PoliticalPartyState> PartyStates
    {
        get { return partyStates; }
    }


    // =========================================================
    // PARLAMENTO INICIAL
    // =========================================================

    [Header("Parlamento inicial")]

    [SerializeField]
    private List<PartyParliamentSetup> initialParliament =
        new List<PartyParliamentSetup>();


    // =========================================================
    // GOBIERNO
    // =========================================================

    [Header("Gobierno")]

    [SerializeField]
    private GovernmentCoalitionType coalitionType =
        GovernmentCoalitionType.MajorityCoalition;

    public GovernmentCoalitionType CoalitionType =>
        coalitionType;


    // Partido que lidera el gobierno.
    // Siempre será el partido gubernamental
    // con mayor número de escaños.

    private PoliticalPartyState governmentLeader;

    public PoliticalPartyState GovernmentLeader =>
        governmentLeader;


    // =========================================================
    // TOTAL DE ESCAÑOS
    // =========================================================

    public int TotalSeats
    {
        get
        {
            int totalSeats = 0;

            foreach (
                PoliticalPartyState partyState
                in partyStates
            )
            {
                if (partyState != null)
                {
                    totalSeats +=
                        partyState.seats;
                }
            }

            return totalSeats;
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        InitializeParties();
    }


    // =========================================================
    // INICIALIZAR PARTIDOS
    // =========================================================

    public void InitializeParties()
    {
        partyStates.Clear();

        governmentLeader = null;


        if (
            availableParties == null ||
            availableParties.Count == 0
        )
        {
            Debug.LogWarning(
                "PoliticalPartyManager: no hay partidos asignados."
            );

            return;
        }


        // -----------------------------------------------------
        // CONTAR PARTIDOS HABILITADOS
        // -----------------------------------------------------

        int validPartyCount = 0;


        foreach (
            PoliticalPartySO party
            in availableParties
        )
        {
            if (party == null)
            {
                Debug.LogWarning(
                    "PoliticalPartyManager: hay una referencia " +
                    "de partido nula."
                );

                continue;
            }


            if (!party.Enabled)
            {
                Debug.Log(
                    $"{party.partyName} | Enabled: False"
                );

                continue;
            }


            validPartyCount++;


            Debug.Log(
                $"{party.partyName} | Enabled: True"
            );
        }


        if (validPartyCount == 0)
        {
            Debug.LogWarning(
                "PoliticalPartyManager: no hay partidos habilitados."
            );

            return;
        }


        // -----------------------------------------------------
        // POPULARIDAD INICIAL
        // -----------------------------------------------------

        float initialPopularity =
            100f /
            validPartyCount;


        // -----------------------------------------------------
        // CREAR ESTADOS SOLO PARA PARTIDOS HABILITADOS
        // -----------------------------------------------------

        foreach (
            PoliticalPartySO party
            in availableParties
        )
        {
            if (
                party == null ||
                !party.Enabled
            )
            {
                continue;
            }


            PartyParliamentSetup parliamentSetup =
                null;


            if (initialParliament != null)
            {
                parliamentSetup =
                    initialParliament.Find(
                        setup =>
                            setup != null &&
                            setup.Party == party
                    );
            }


            PoliticalPartyState newState =
                new PoliticalPartyState
                {
                    party =
                        party,

                    popularity =
                        initialPopularity,

                    satisfaction =
                        50f,

                    lastSatisfactionChange =
                        0f,

                    seats =
                        parliamentSetup != null
                            ? parliamentSetup.Seats
                            : 0,

                    isGovernment =
                        parliamentSetup != null &&
                        parliamentSetup.IsGovernment
                };


            partyStates.Add(
                newState
            );
        }


        // -----------------------------------------------------
        // GENERAR PARLAMENTO ALEATORIO
        // -----------------------------------------------------

        if (generateRandomParliament)
        {
            GenerateRandomSeats();
        }


        // -----------------------------------------------------
        // FORMAR GOBIERNO
        // -----------------------------------------------------

        FormAutomaticGovernment();


        Debug.Log(
            $"PoliticalPartyManager: " +
            $"{partyStates.Count} partidos inicializados."
        );
    }


    // =========================================================
    // BUSCAR ESTADO DE PARTIDO
    // =========================================================

    public PoliticalPartyState GetPartyState(
        string partyId
    )
    {
        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            if (partyState.party == null)
            {
                continue;
            }


            if (
                partyState.party.partyId ==
                partyId
            )
            {
                return partyState;
            }
        }


        Debug.LogWarning(
            $"PoliticalPartyManager: no existe el partido " +
            $"con ID '{partyId}'."
        );


        return null;
    }


    // =========================================================
    // SATISFACCIÓN
    // =========================================================

    public float CalculatePartySatisfaction(
        PoliticalPartyState partyState
    )
    {
        if (partyState == null)
        {
            Debug.LogWarning(
                "No se puede calcular la satisfacción " +
                "de un partido nulo."
            );

            return 0f;
        }


        partyState.satisfaction =
            Mathf.Clamp(
                partyState.satisfaction,
                0f,
                100f
            );


        return partyState.satisfaction;
    }


    // =========================================================
    // DEBUG PARTIDOS
    // =========================================================

    [ContextMenu("Mostrar partidos inicializados")]
    private void DebugPartyStates()
    {
        if (
            partyStates == null ||
            partyStates.Count == 0
        )
        {
            Debug.LogWarning(
                "PoliticalPartyManager: todavía no hay " +
                "partidos inicializados."
            );

            return;
        }


        Debug.Log(
            $"PARTIDOS INICIALIZADOS: {partyStates.Count}"
        );


        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null
            )
            {
                Debug.LogWarning(
                    "Hay un estado de partido vacío."
                );

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


    // =========================================================
    // DEBUG REACCIÓN ECONÓMICA
    // =========================================================

    [ContextMenu("Probar reacción económica")]
    private void DebugEconomicPolicyReaction()
    {
        if (
            partyStates == null ||
            partyStates.Count == 0
        )
        {
            Debug.LogWarning(
                "No hay partidos inicializados. " +
                "Ejecuta primero la partida."
            );

            return;
        }


        PoliticalAxis testAxis =
            PoliticalAxis.Economy;

        float testPolicyPosition =
            4.5f;

        float testRelevance =
            20f;


        Debug.Log(
            "=== PRUEBA: POLÍTICA ECONÓMICA +4.5 ==="
        );


        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null
            )
            {
                continue;
            }


            float partyPosition =
                partyState.party.GetPosition(
                    testAxis
                );


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


    // =========================================================
    // REACCIÓN A POLÍTICAS
    // =========================================================

    public void ApplyPolicyReaction(
        PoliticalAxis axis,
        float policyPosition,
        float policyRelevance
    )
    {
        if (
            partyStates == null ||
            partyStates.Count == 0
        )
        {
            Debug.LogWarning(
                "PoliticalPartyManager: no hay " +
                "partidos inicializados."
            );

            return;
        }


        Debug.Log(
            $"=== REACCIÓN A POLÍTICA | " +
            $"Eje: {axis} | " +
            $"Posición: {policyPosition:F1} | " +
            $"Relevancia: {policyRelevance:F1} ==="
        );


        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null
            )
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


            float newSatisfaction =
                Mathf.Clamp(
                    previousSatisfaction +
                    impact,
                    0f,
                    100f
                );


            partyState.satisfaction =
                newSatisfaction;


            partyState.lastSatisfactionChange =
                newSatisfaction -
                previousSatisfaction;


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


    // =========================================================
    // DEBUG PARLAMENTO
    // =========================================================

    [ContextMenu("Mostrar parlamento actual")]
    private void DebugCurrentParliament()
    {
        if (
            partyStates == null ||
            partyStates.Count == 0
        )
        {
            Debug.LogWarning(
                "PoliticalPartyManager: no hay " +
                "partidos inicializados."
            );

            return;
        }


        int totalSeats =
            TotalSeats;


        Debug.Log(
            $"=== PARLAMENTO | TOTAL: {totalSeats} ASIENTOS | " +
            $"GOBIERNO: {coalitionType} ==="
        );


        if (
            governmentLeader != null &&
            governmentLeader.party != null
        )
        {
            Debug.Log(
                $"LÍDER DEL GOBIERNO | " +
                $"{governmentLeader.party.partyName} | " +
                $"{governmentLeader.seats} asientos"
            );
        }


        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null
            )
            {
                continue;
            }


            float percentage =
                totalSeats > 0
                    ? partyState.seats /
                      (float)totalSeats *
                      100f
                    : 0f;


            string governmentLabel =
                partyState.isGovernment
                    ? " | GOBIERNO"
                    : string.Empty;


            string leaderLabel =
                partyState == governmentLeader
                    ? " | LÍDER"
                    : string.Empty;


            Debug.Log(
                $"{partyState.party.partyName} | " +
                $"Asientos: {partyState.seats} | " +
                $"Porcentaje: {percentage:F2}%" +
                governmentLabel +
                leaderLabel
            );
        }
    }


    // =========================================================
    // GENERAR ESCAÑOS
    // =========================================================

    private void GenerateRandomSeats()
    {
        if (
            partyStates == null ||
            partyStates.Count == 0
        )
        {
            return;
        }


        int partyCount =
            partyStates.Count;


        int minimumTotal =
            minimumSeatsPerParty *
            partyCount;


        if (minimumTotal > parliamentSeatCount)
        {
            Debug.LogError(
                "El mínimo de asientos por partido supera " +
                "el tamaño total del Parlamento."
            );

            return;
        }


        // -----------------------------------------------------
        // 1. GARANTIZAR MÍNIMO
        // -----------------------------------------------------

        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            partyState.seats =
                minimumSeatsPerParty;
        }


        int remainingSeats =
            parliamentSeatCount -
            minimumTotal;


        // -----------------------------------------------------
        // 2. SEPARAR LOS PARTIDOS POR RADICALIZACIÓN
        // -----------------------------------------------------

        List<PoliticalPartyState> centerMainstreamParties =
            new List<PoliticalPartyState>();


        List<PoliticalPartyState> hardlineParties =
            new List<PoliticalPartyState>();


        List<PoliticalPartyState> radicalParties =
            new List<PoliticalPartyState>();


        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null
            )
            {
                continue;
            }


            switch (
                partyState.party.RadicalizationLayer
            )
            {
                case PoliticalRadicalizationLayer.Center:

                case PoliticalRadicalizationLayer.Mainstream:

                    centerMainstreamParties.Add(
                        partyState
                    );

                    break;


                case PoliticalRadicalizationLayer.Hardline:

                    hardlineParties.Add(
                        partyState
                    );

                    break;


                case PoliticalRadicalizationLayer.Radical:

                    radicalParties.Add(
                        partyState
                    );

                    break;
            }
        }


        // -----------------------------------------------------
        // 3. PESOS ACTIVOS
        // -----------------------------------------------------

        float activeCenterMainstreamWeight =
            centerMainstreamParties.Count > 0
                ? centerMainstreamWeight
                : 0f;


        float activeHardlineWeight =
            hardlineParties.Count > 0
                ? hardlineWeight
                : 0f;


        float activeRadicalWeight =
            radicalParties.Count > 0
                ? radicalWeight
                : 0f;


        float totalActiveWeight =
            activeCenterMainstreamWeight +
            activeHardlineWeight +
            activeRadicalWeight;


        if (totalActiveWeight <= 0f)
        {
            Debug.LogError(
                "PoliticalPartyManager: no existen grupos " +
                "ideológicos válidos para repartir escaños."
            );

            return;
        }


        // -----------------------------------------------------
        // 4. REPARTIR ESCAÑOS ENTRE CAPAS
        // -----------------------------------------------------

        int centerMainstreamSeats =
            Mathf.FloorToInt(
                remainingSeats *
                activeCenterMainstreamWeight /
                totalActiveWeight
            );


        int hardlineSeats =
            Mathf.FloorToInt(
                remainingSeats *
                activeHardlineWeight /
                totalActiveWeight
            );


        int radicalSeats =
            Mathf.FloorToInt(
                remainingSeats *
                activeRadicalWeight /
                totalActiveWeight
            );


        int assignedGroupSeats =
            centerMainstreamSeats +
            hardlineSeats +
            radicalSeats;


        int groupSeatsLeft =
            remainingSeats -
            assignedGroupSeats;


        // -----------------------------------------------------
        // REPARTIR RESTOS
        // -----------------------------------------------------

        while (groupSeatsLeft > 0)
        {
            if (
                centerMainstreamParties.Count > 0 &&
                activeCenterMainstreamWeight >=
                activeHardlineWeight &&
                activeCenterMainstreamWeight >=
                activeRadicalWeight
            )
            {
                centerMainstreamSeats++;
            }
            else if (
                hardlineParties.Count > 0 &&
                activeHardlineWeight >=
                activeRadicalWeight
            )
            {
                hardlineSeats++;
            }
            else if (
                radicalParties.Count > 0
            )
            {
                radicalSeats++;
            }


            groupSeatsLeft--;
        }


        // -----------------------------------------------------
        // 5. REPARTIR DENTRO DE CADA GRUPO
        // -----------------------------------------------------

        DistributeSeatsWithinGroup(
            centerMainstreamParties,
            centerMainstreamSeats
        );


        DistributeSeatsWithinGroup(
            hardlineParties,
            hardlineSeats
        );


        DistributeSeatsWithinGroup(
            radicalParties,
            radicalSeats
        );


        Debug.Log(
            $"=== PARLAMENTO GENERADO: {TotalSeats} ASIENTOS ==="
        );


        Debug.Log(
            $"Center/Mainstream: " +
            $"{GetGroupSeats(centerMainstreamParties)} | " +
            $"Hardline: " +
            $"{GetGroupSeats(hardlineParties)} | " +
            $"Radical: " +
            $"{GetGroupSeats(radicalParties)}"
        );
    }


    // =========================================================
    // REPARTIR ESCAÑOS DENTRO DE UN GRUPO
    // =========================================================

    private void DistributeSeatsWithinGroup(
        List<PoliticalPartyState> group,
        int seatsToDistribute
    )
    {
        if (
            group == null ||
            group.Count == 0 ||
            seatsToDistribute <= 0
        )
        {
            return;
        }


        float[] weights =
            new float[group.Count];


        float totalWeight =
            0f;


        for (
            int i = 0;
            i < group.Count;
            i++
        )
        {
            weights[i] =
                Random.Range(
                    0.25f,
                    1f
                );


            totalWeight +=
                weights[i];
        }


        int distributedSeats =
            0;


        for (
            int i = 0;
            i < group.Count;
            i++
        )
        {
            int seats =
                Mathf.FloorToInt(
                    seatsToDistribute *
                    weights[i] /
                    totalWeight
                );


            group[i].seats +=
                seats;


            distributedSeats +=
                seats;
        }


        int seatsLeft =
            seatsToDistribute -
            distributedSeats;


        while (seatsLeft > 0)
        {
            int randomParty =
                Random.Range(
                    0,
                    group.Count
                );


            group[randomParty].seats++;


            seatsLeft--;
        }
    }


    // =========================================================
    // OBTENER TOTAL DE ESCAÑOS DE UN GRUPO
    // =========================================================

    private int GetGroupSeats(
        List<PoliticalPartyState> group
    )
    {
        if (group == null)
        {
            return 0;
        }


        int total =
            0;


        foreach (
            PoliticalPartyState partyState
            in group
        )
        {
            if (partyState != null)
            {
                total +=
                    partyState.seats;
            }
        }


        return total;
    }


    // =========================================================
    // FORMAR GOBIERNO
    // =========================================================

    private void FormAutomaticGovernment()
    {
        // El líder se vuelve a calcular
        // cada vez que se forma un gobierno.

        governmentLeader =
            null;


        if (
            partyStates == null ||
            partyStates.Count == 0
        )
        {
            coalitionType =
                GovernmentCoalitionType.NoGovernment;

            return;
        }


        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            partyState.isGovernment =
                false;
        }


        int totalSeats =
            TotalSeats;


        if (totalSeats <= 0)
        {
            coalitionType =
                GovernmentCoalitionType.NoGovernment;

            return;
        }


        int majorityThreshold =
            totalSeats / 2 + 1;


        // -----------------------------------------------------
        // 1. MAYORÍA ABSOLUTA DE UN SOLO PARTIDO
        // -----------------------------------------------------

        foreach (
            PoliticalPartyState partyState
            in partyStates
        )
        {
            if (
                partyState.seats >=
                majorityThreshold
            )
            {
                partyState.isGovernment =
                    true;


                governmentLeader =
                    partyState;


                coalitionType =
                    GovernmentCoalitionType.SinglePartyMajority;


                Debug.Log(
                    $"Gobierno formado por " +
                    $"{partyState.party.partyName} " +
                    $"con {partyState.seats} asientos."
                );


                Debug.Log(
                    $"LÍDER DEL GOBIERNO | " +
                    $"{governmentLeader.party.partyName}"
                );


                return;
            }
        }


        // -----------------------------------------------------
        // 2. BUSCAR COALICIÓN
        // -----------------------------------------------------

        List<PoliticalPartyState> bestCoalition =
            FindBestMajorityCoalition(
                majorityThreshold
            );


        if (
            bestCoalition == null ||
            bestCoalition.Count == 0
        )
        {
            coalitionType =
                GovernmentCoalitionType.NoGovernment;


            Debug.LogWarning(
                "No ha sido posible formar gobierno."
            );


            return;
        }


        int governmentSeats =
            0;


        // -----------------------------------------------------
        // MARCAR PARTIDOS DEL GOBIERNO
        // Y ENCONTRAR EL PARTIDO MÁS GRANDE
        // -----------------------------------------------------

        foreach (
            PoliticalPartyState partyState
            in bestCoalition
        )
        {
            partyState.isGovernment =
                true;


            governmentSeats +=
                partyState.seats;


            if (
                governmentLeader == null ||
                partyState.seats >
                governmentLeader.seats
            )
            {
                governmentLeader =
                    partyState;
            }
        }


        coalitionType =
            GovernmentCoalitionType.MajorityCoalition;


        Debug.Log(
            $"Coalición formada con " +
            $"{governmentSeats}/{totalSeats} asientos."
        );


        if (
            governmentLeader != null &&
            governmentLeader.party != null
        )
        {
            Debug.Log(
                $"LÍDER DEL GOBIERNO | " +
                $"{governmentLeader.party.partyName} | " +
                $"{governmentLeader.seats} asientos"
            );
        }


        foreach (
            PoliticalPartyState partyState
            in bestCoalition
        )
        {
            string leaderLabel =
                partyState == governmentLeader
                    ? " | LÍDER"
                    : string.Empty;


            Debug.Log(
                $"GOBIERNO | " +
                $"{partyState.party.partyName} | " +
                $"{partyState.seats} asientos" +
                leaderLabel
            );
        }
    }


    // =========================================================
    // BUSCAR MEJOR COALICIÓN
    // =========================================================

    private List<PoliticalPartyState> FindBestMajorityCoalition(
        int majorityThreshold
    )
    {
        List<PoliticalPartyState> bestCoalition =
            null;


        float bestScore =
            float.MaxValue;


        foreach (
            PoliticalPartyState leader
            in partyStates
        )
        {
            if (
                leader == null ||
                leader.party == null ||
                leader.seats <= 0
            )
            {
                continue;
            }


            List<PoliticalPartyState> possiblePartners =
                new List<PoliticalPartyState>();


            foreach (
                PoliticalPartyState candidate
                in partyStates
            )
            {
                if (
                    candidate == null ||
                    candidate.party == null ||
                    candidate == leader ||
                    candidate.seats <= 0
                )
                {
                    continue;
                }


                // =================================================
                // REGLA DE LIDERAZGO
                //
                // Un partido NO puede liderar una coalición
                // que incluya otro partido con más escaños.
                // =================================================

                if (
                    candidate.seats >
                    leader.seats
                )
                {
                    continue;
                }


                possiblePartners.Add(
                    candidate
                );
            }


            // -------------------------------------------------
            // ORDENAR SOCIOS POR COMPATIBILIDAD
            // -------------------------------------------------

            possiblePartners.Sort(
                (a, b) =>
                {
                    float distanceA =
                        CalculatePartyIdeologicalDistance(
                            leader.party,
                            a.party
                        );


                    float distanceB =
                        CalculatePartyIdeologicalDistance(
                            leader.party,
                            b.party
                        );


                    int comparison =
                        distanceA.CompareTo(
                            distanceB
                        );


                    if (comparison != 0)
                    {
                        return comparison;
                    }


                    // Si tienen exactamente la misma
                    // compatibilidad, preferimos al
                    // partido con más escaños.

                    return b.seats.CompareTo(
                        a.seats
                    );
                }
            );


            // -------------------------------------------------
            // CREAR COALICIÓN
            // -------------------------------------------------

            List<PoliticalPartyState> coalition =
                new List<PoliticalPartyState>();


            coalition.Add(
                leader
            );


            int coalitionSeats =
                leader.seats;


            // -------------------------------------------------
            // AÑADIR SOCIOS HASTA MAYORÍA
            // -------------------------------------------------

            foreach (
                PoliticalPartyState partner
                in possiblePartners
            )
            {
                if (
                    coalitionSeats >=
                    majorityThreshold
                )
                {
                    break;
                }


                coalition.Add(
                    partner
                );


                coalitionSeats +=
                    partner.seats;
            }


            if (
                coalitionSeats <
                majorityThreshold
            )
            {
                continue;
            }


            // -------------------------------------------------
            // PUNTUAR COALICIÓN
            // -------------------------------------------------

            float ideologicalDistance =
                CalculateCoalitionIdeologicalDistance(
                    coalition
                );


            int excessSeats =
                coalitionSeats -
                majorityThreshold;


            float excessSeatPenalty =
                3f /
                Mathf.Max(
                    1,
                    TotalSeats
                );


            float score =
                ideologicalDistance +

                (coalition.Count - 2) *
                0.5f +

                excessSeats *
                excessSeatPenalty;


            if (score < bestScore)
            {
                bestScore =
                    score;


                bestCoalition =
                    new List<PoliticalPartyState>(
                        coalition
                    );
            }
        }


        return bestCoalition;
    }


    // =========================================================
    // DISTANCIA ENTRE DOS PARTIDOS
    // =========================================================

    private float CalculatePartyIdeologicalDistance(
        PoliticalPartySO partyA,
        PoliticalPartySO partyB
    )
    {
        if (
            partyA == null ||
            partyB == null
        )
        {
            return float.MaxValue;
        }


        float totalDistance =
            0f;


        int axisCount =
            0;


        foreach (
            PoliticalAxis axis
            in Enum.GetValues(
                typeof(PoliticalAxis)
            )
        )
        {
            float positionA =
                partyA.GetPosition(
                    axis
                );


            float positionB =
                partyB.GetPosition(
                    axis
                );


            totalDistance +=
                Mathf.Abs(
                    positionA -
                    positionB
                );


            axisCount++;
        }


        if (axisCount == 0)
        {
            return 0f;
        }


        return
            totalDistance /
            axisCount;
    }


    // =========================================================
    // DISTANCIA IDEOLÓGICA DE UNA COALICIÓN
    // =========================================================

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


        float totalDistance =
            0f;


        int comparisons =
            0;


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
                        partyA.GetPosition(
                            axis
                        );


                    float positionB =
                        partyB.GetPosition(
                            axis
                        );


                    totalDistance +=
                        Mathf.Abs(
                            positionA -
                            positionB
                        );


                    comparisons++;
                }
            }
        }


        if (comparisons == 0)
        {
            return 0f;
        }


        return
            totalDistance /
            comparisons;
    }
}