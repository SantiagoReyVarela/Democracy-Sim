using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectionPollManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static ElectionPollManager Instance { get; private set; }


    // =========================================================
    // CONFIGURACIÓN
    // =========================================================

    [Header("Polling Configuration")]

    [SerializeField]
    private int trackedPartyCount = 5;


    [Header("Voting Intention")]

    [Tooltip(
        "Cuánto reaccionan las encuestas a las preocupaciones " +
        "de la población en cada turno."
    )]
    [SerializeField, Range(0f, 1f)]
    private float concernResponsiveness = 0.35f;


    [Tooltip(
        "Máximo cambio permitido para un partido en un solo turno, " +
        "medido en puntos porcentuales."
    )]
    [SerializeField, Range(0.1f, 10f)]
    private float maxChangePerTurn = 2f;


    [Tooltip(
        "Apoyo mínimo que conserva cualquier partido."
    )]
    [SerializeField, Range(0.01f, 5f)]
    private float minimumPolling = 0.10f;


    // =========================================================
    // ESTADO
    // =========================================================

    [Header("Polling State")]

    [SerializeField]
    private int currentTurn = 0;

    [SerializeField]
    private List<PollPartyData> partyPollingData =
        new List<PollPartyData>();


    // =========================================================
    // EVENTOS
    // =========================================================

    public event Action OnPollUpdated;


    // =========================================================
    // ACCESO PÚBLICO
    // =========================================================

    public int CurrentTurn => currentTurn;

    public IReadOnlyList<PollPartyData> PartyPollingData =>
        partyPollingData;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(
            gameObject
        );
    }


    // =========================================================
    // START
    // =========================================================

    private IEnumerator Start()
    {
        yield return null;

        InitializeFromCurrentParliament();
    }


    // =========================================================
    // INICIALIZAR ENCUESTAS
    // =========================================================

    public void InitializeFromCurrentParliament()
    {
        PoliticalPartyManager politicalPartyManager =
            FindObjectOfType<PoliticalPartyManager>();


        if (politicalPartyManager == null)
        {
            Debug.LogError(
                "ElectionPollManager: no se encontró PoliticalPartyManager."
            );

            return;
        }


        int totalSeats =
            politicalPartyManager.TotalSeats;


        if (totalSeats <= 0)
        {
            Debug.LogWarning(
                "ElectionPollManager: el parlamento no tiene escaños."
            );

            return;
        }


        currentTurn = 0;

        partyPollingData.Clear();


        foreach (
            PoliticalPartyState partyState
            in politicalPartyManager.PartyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null
            )
            {
                continue;
            }


            float initialPolling =
                (
                    (float)partyState.seats /
                    totalSeats
                )
                * 100f;


            PollPartyData pollingData =
                new PollPartyData();


            pollingData.party =
                partyState.party;

            pollingData.currentPolling =
                initialPolling;

            pollingData.isTracked =
                false;


            pollingData.history.Add(
                initialPolling
            );


            partyPollingData.Add(
                pollingData
            );
        }


        // Los cinco partidos visibles se eligen aquí.
        // NO se vuelven a elegir en cada turno.
        SelectTopTrackedParties();


        OnPollUpdated?.Invoke();


        Debug.Log(
            $"ElectionPollManager: inicializados " +
            $"{partyPollingData.Count} partidos."
        );
    }


    // =========================================================
    // REGISTRAR NUEVO TURNO
    // =========================================================

    public void RegisterTurn()
    {
        currentTurn++;


        // -----------------------------------------------------
        // 1. ACTUALIZAR PREOCUPACIONES
        // -----------------------------------------------------

        if (PublicConcernManager.Instance != null)
        {
            PublicConcernManager.Instance.RefreshConcerns();
        }


        // -----------------------------------------------------
        // 2. CAMBIAR INTENCIÓN DE VOTO
        // -----------------------------------------------------

        UpdateVotingIntentions();


        // -----------------------------------------------------
        // 3. GUARDAR HISTORIAL
        // -----------------------------------------------------

        foreach (
            PollPartyData data
            in partyPollingData
        )
        {
            if (data == null)
            {
                continue;
            }


            data.history.Add(
                data.currentPolling
            );
        }


        // -----------------------------------------------------
        // 4. AVISAR AL GRÁFICO
        // -----------------------------------------------------

        OnPollUpdated?.Invoke();
    }


    // =========================================================
    // ACTUALIZAR INTENCIÓN DE VOTO
    // =========================================================

    private void UpdateVotingIntentions()
    {
        if (
            partyPollingData == null ||
            partyPollingData.Count == 0
        )
        {
            return;
        }


        if (
            PublicConcernManager.Instance == null ||
            PublicConcernManager.Instance.TopConcerns == null ||
            PublicConcernManager.Instance.TopConcerns.Count == 0
        )
        {
            return;
        }


        Dictionary<PollPartyData, float> partyScores =
            new Dictionary<PollPartyData, float>();


        // -----------------------------------------------------
        // CALCULAR PUNTUACIÓN DE CADA PARTIDO
        // -----------------------------------------------------

        foreach (
            PollPartyData data
            in partyPollingData
        )
        {
            if (
                data == null ||
                data.party == null
            )
            {
                continue;
            }


            float score =
                CalculatePartyConcernScore(
                    data.party
                );


            partyScores[data] =
                score;
        }


        if (partyScores.Count == 0)
        {
            return;
        }


        // -----------------------------------------------------
        // PUNTUACIÓN MEDIA DEL SISTEMA
        //
        // PONDERADA POR LA INTENCIÓN DE VOTO ACTUAL.
        // -----------------------------------------------------

        float weightedAverageScore =
            0f;

        float totalCurrentPolling =
            0f;


        foreach (
            KeyValuePair<PollPartyData, float> pair
            in partyScores
        )
        {
            weightedAverageScore +=
                pair.Value *
                pair.Key.currentPolling;


            totalCurrentPolling +=
                pair.Key.currentPolling;
        }


        if (totalCurrentPolling > 0f)
        {
            weightedAverageScore /=
                totalCurrentPolling;
        }


        // -----------------------------------------------------
        // CALCULAR NUEVOS PORCENTAJES PROVISIONALES
        // -----------------------------------------------------

        Dictionary<PollPartyData, float> newValues =
            new Dictionary<PollPartyData, float>();


        foreach (
            KeyValuePair<PollPartyData, float> pair
            in partyScores
        )
        {
            PollPartyData data =
                pair.Key;


            float score =
                pair.Value;


            // > 0 = responde mejor que la media
            // < 0 = responde peor que la media
            float relativeScore =
                score -
                weightedAverageScore;


            // Convertimos esa diferencia en presión electoral.
            float proportionalChange =
                relativeScore *
                concernResponsiveness;


            float targetPolling =
                data.currentPolling *
                (
                    1f +
                    proportionalChange
                );


            float delta =
                targetPolling -
                data.currentPolling;


            delta =
                Mathf.Clamp(
                    delta,
                    -maxChangePerTurn,
                    maxChangePerTurn
                );


            float newPolling =
                Mathf.Max(
                    minimumPolling,
                    data.currentPolling +
                    delta
                );


            newValues[data] =
                newPolling;
        }


        // -----------------------------------------------------
        // NORMALIZAR PARA QUE TODOS LOS PARTIDOS SUMEN 100%
        // -----------------------------------------------------

        float totalNewPolling =
            newValues.Values.Sum();


        if (totalNewPolling <= 0f)
        {
            return;
        }


        foreach (
            KeyValuePair<PollPartyData, float> pair
            in newValues
        )
        {
            pair.Key.currentPolling =
                pair.Value /
                totalNewPolling *
                100f;
        }
    }


    // =========================================================
    // PUNTUACIÓN POLÍTICA SEGÚN PREOCUPACIONES
    // =========================================================

    private float CalculatePartyConcernScore(
        PoliticalPartySO party
    )
    {
        if (
            party == null ||
            PublicConcernManager.Instance == null
        )
        {
            return 0f;
        }


        float totalScore =
            0f;


        foreach (
            PublicConcernData concern
            in PublicConcernManager.Instance.TopConcerns
        )
        {
            if (concern == null)
            {
                continue;
            }


            PoliticalAxis axis;

            float desiredPosition;


            GetPoliticalDemandForConcern(
                concern.type,
                out axis,
                out desiredPosition
            );


            float partyPosition =
                party.GetPosition(
                    axis
                );


            float distance =
                Mathf.Abs(
                    partyPosition -
                    desiredPosition
                );


            // Rango completo -5 ... +5
            // Distancia máxima = 10.
            float affinity =
                1f -
                Mathf.Clamp01(
                    distance / 10f
                );


            float concernWeight =
                concern.importance /
                100f;


            totalScore +=
                affinity *
                concernWeight;
        }


        return totalScore;
    }


    // =========================================================
    // QUÉ RESPUESTA POLÍTICA DEMANDA CADA PROBLEMA
    //
    // ESTOS VALORES SON DE BALANCE Y PODREMOS AJUSTARLOS.
    // =========================================================

    private void GetPoliticalDemandForConcern(
        PublicConcernType concern,
        out PoliticalAxis axis,
        out float desiredPosition
    )
    {
        switch (concern)
        {
            // -------------------------------------------------
            // ECONOMÍA GENERAL
            //
            // No favorece un extremo concreto.
            // -------------------------------------------------

            case PublicConcernType.Economy:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    0f;

                break;


            // -------------------------------------------------
            // EDUCACIÓN
            //
            // Mayor intervención/inversión pública.
            // -------------------------------------------------

            case PublicConcernType.Education:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    -2f;

                break;


            // -------------------------------------------------
            // DESEMPLEO
            // -------------------------------------------------

            case PublicConcernType.Unemployment:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    -2.5f;

                break;


            // -------------------------------------------------
            // DESIGUALDAD
            // -------------------------------------------------

            case PublicConcernType.Inequality:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    -3.5f;

                break;


            // -------------------------------------------------
            // INFLACIÓN
            //
            // Favorece posiciones de mayor disciplina económica.
            // -------------------------------------------------

            case PublicConcernType.Inflation:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    3f;

                break;


            // -------------------------------------------------
            // INESTABILIDAD
            // -------------------------------------------------

            case PublicConcernType.Stability:

                axis =
                    PoliticalAxis.Government;

                desiredPosition =
                    2.5f;

                break;


            // -------------------------------------------------
            // DEUDA
            // -------------------------------------------------

            case PublicConcernType.Debt:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    3f;

                break;


            // -------------------------------------------------
            // DÉFICIT
            // -------------------------------------------------

            case PublicConcernType.FiscalDeficit:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    3.5f;

                break;


            // -------------------------------------------------
            // FALTA DE RESERVAS
            // -------------------------------------------------

            case PublicConcernType.Reserves:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    2f;

                break;


            default:

                axis =
                    PoliticalAxis.Economy;

                desiredPosition =
                    0f;

                break;
        }
    }


    // =========================================================
    // ELEGIR PARTIDOS VISIBLES
    // =========================================================

    public void SelectTopTrackedParties()
    {
        foreach (
            PollPartyData data
            in partyPollingData
        )
        {
            data.isTracked =
                false;
        }


        List<PollPartyData> ordered =
            partyPollingData
            .OrderByDescending(
                data =>
                    data.currentPolling
            )
            .Take(
                trackedPartyCount
            )
            .ToList();


        foreach (
            PollPartyData data
            in ordered
        )
        {
            data.isTracked =
                true;
        }
    }


    // =========================================================
    // OBTENER LOS PARTIDOS DEL GRÁFICO
    // =========================================================

    public List<PollPartyData> GetTrackedParties()
    {
        return
            partyPollingData
            .Where(
                data =>
                    data.isTracked
            )
            .OrderByDescending(
                data =>
                    data.currentPolling
            )
            .ToList();
    }


    // =========================================================
    // OBTENER DATOS DE UN PARTIDO
    // =========================================================

    public PollPartyData GetPartyData(
        PoliticalPartySO party
    )
    {
        if (party == null)
        {
            return null;
        }


        foreach (
            PollPartyData data
            in partyPollingData
        )
        {
            if (data.party == party)
            {
                return data;
            }
        }


        return null;
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}


// =============================================================
// DATOS ELECTORALES DE UN PARTIDO
// =============================================================

[Serializable]
public class PollPartyData
{
    public PoliticalPartySO party;


    [Range(0f, 100f)]
    public float currentPolling;


    public bool isTracked;


    public List<float> history =
        new List<float>();
}