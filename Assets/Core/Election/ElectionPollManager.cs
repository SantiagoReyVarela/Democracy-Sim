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
        // Esperamos un frame para dar tiempo a que
        // PoliticalPartyManager inicialice el parlamento.
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
                "ElectionPollManager: no se encontró " +
                "PoliticalPartyManager."
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


        SelectTopTrackedParties();


        Debug.Log(
            $"ElectionPollManager: inicializados " +
            $"{partyPollingData.Count} partidos."
        );
    }


    // =========================================================
    // ELEGIR LOS 5 PARTIDOS VISIBLES
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
                data => data.currentPolling
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
    // OBTENER LOS 5 PARTIDOS DEL GRÁFICO
    // =========================================================

    public List<PollPartyData> GetTrackedParties()
    {
        return
            partyPollingData
            .Where(
                data => data.isTracked
            )
            .OrderByDescending(
                data => data.currentPolling
            )
            .ToList();
    }


    // =========================================================
    // OBTENER UN PARTIDO CONCRETO
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
    // REGISTRAR UN NUEVO TURNO
    //
    // POR AHORA NO CAMBIA LOS PORCENTAJES.
    // SOLO GUARDA EL HISTORIAL.
    // =========================================================

    public void RegisterTurn()
    {
        currentTurn++;


        foreach (
            PollPartyData data
            in partyPollingData
        )
        {
            data.history.Add(
                data.currentPolling
            );
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