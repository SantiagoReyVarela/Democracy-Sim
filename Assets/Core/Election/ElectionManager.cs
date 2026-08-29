using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ElectionManager : MonoBehaviour
{
    public static ElectionManager Instance { get; private set; }

    [Header("Election Settings")]
    [SerializeField, Min(1)]
    private int turnsPerElection = 100;

    [SerializeField, Range(0f, 20f)]
    private float electionMarginOfError = 5f;

    [SerializeField, Min(1)]
    private int randomTransferPasses = 24;

    [Header("UI")]
    [SerializeField]
    private ElectionResultPanelUI electionResultPanel;

    private bool electionOpen;
    private float previousTimeScale = 1f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    /// <summary>
    /// Comprueba si debe celebrarse una elección.
    /// CountryState llama a este método después
    /// de registrar el último turno y la última encuesta.
    /// </summary>
    public void TryTriggerElection()
    {
        if (electionOpen)
        {
            return;
        }

        if (CountryState.Instance == null)
        {
            return;
        }

        if (!CountryState.Instance.HasReachedFinalTurn)
        {
            return;
        }

        HoldElection();
    }


    /// <summary>
    /// Celebra las elecciones usando la encuesta
    /// actual como base.
    /// </summary>
    private void HoldElection()
    {
        CountryState countryState =
            CountryState.Instance;

        ElectionPollManager pollManager =
            ElectionPollManager.Instance;

        if (countryState == null)
        {
            Debug.LogError(
                "[ElectionManager] No existe CountryState."
            );

            return;
        }

        if (pollManager == null)
        {
            Debug.LogError(
                "[ElectionManager] No existe ElectionPollManager."
            );

            return;
        }

        List<ElectionPartyResult> results =
            BuildElectionResults(pollManager);

        if (results == null || results.Count == 0)
        {
            Debug.LogWarning(
                "[ElectionManager] No hay partidos disponibles " +
                "para celebrar elecciones."
            );

            return;
        }

        ElectionPartyResult winner =
            DetermineWinner(results);

        PoliticalPartySO playerParty =
            GetPlayerParty();

        bool playerWon =
            winner != null &&
            winner.Party != null &&
            winner.Party == playerParty;


        // ---------------------------------------------------------
        // PAUSAR JUEGO
        // ---------------------------------------------------------

        electionOpen = true;

        previousTimeScale = Time.timeScale;

        Time.timeScale = 0f;


        // ---------------------------------------------------------
        // MOSTRAR RESULTADOS
        // ---------------------------------------------------------

        if (electionResultPanel != null)
        {
            electionResultPanel.Show(
                results,
                winner != null ? winner.Party : null,
                playerParty,
                playerWon,
                this
            );
        }
        else
        {
            Debug.LogError(
                "[ElectionManager] No se ha asignado " +
                "ElectionResultPanelUI en el Inspector."
            );
        }
    }


    /// <summary>
    /// Construye los resultados electorales.
    ///
    /// Cada partido parte exactamente de su
    /// última encuesta y puede desviarse hasta
    /// ± electionMarginOfError puntos porcentuales.
    /// </summary>
    private List<ElectionPartyResult> BuildElectionResults(
        ElectionPollManager pollManager)
    {
        List<ElectionPartyResult> results =
            new List<ElectionPartyResult>();

        IReadOnlyList<PollPartyData> pollData =
            pollManager.PartyPollingData;

        if (pollData == null)
        {
            return results;
        }


        // ---------------------------------------------------------
        // COPIAR ENCUESTAS
        // ---------------------------------------------------------

        foreach (PollPartyData data in pollData)
        {
            if (data == null || data.party == null)
            {
                continue;
            }

            float polling =
                Mathf.Clamp(
                    data.currentPolling,
                    0f,
                    100f
                );

            ElectionPartyResult result =
                new ElectionPartyResult(
                    data.party,
                    polling,
                    polling
                );

            results.Add(result);
        }


        if (results.Count <= 1)
        {
            return results;
        }


        // ---------------------------------------------------------
        // TRANSFERENCIAS ALEATORIAS DE VOTO
        //
        // Movemos votos entre partidos en vez de
        // modificar porcentajes independientemente.
        //
        // Así el total permanece igual.
        // ---------------------------------------------------------

        for (int pass = 0;
             pass < randomTransferPasses;
             pass++)
        {
            ElectionPartyResult donor =
                results[
                    Random.Range(
                        0,
                        results.Count
                    )
                ];

            ElectionPartyResult receiver =
                results[
                    Random.Range(
                        0,
                        results.Count
                    )
                ];


            if (donor == receiver)
            {
                continue;
            }


            float donorMinimum =
                Mathf.Max(
                    0f,
                    donor.PollPercentage -
                    electionMarginOfError
                );

            float receiverMaximum =
                Mathf.Min(
                    100f,
                    receiver.PollPercentage +
                    electionMarginOfError
                );


            float donorAvailable =
                donor.ResultPercentage -
                donorMinimum;

            float receiverCapacity =
                receiverMaximum -
                receiver.ResultPercentage;


            float maximumTransfer =
                Mathf.Min(
                    donorAvailable,
                    receiverCapacity
                );


            if (maximumTransfer <= 0f)
            {
                continue;
            }


            float transfer =
                Random.Range(
                    0f,
                    maximumTransfer
                );


            donor.ResultPercentage -= transfer;
            receiver.ResultPercentage += transfer;
        }


        // ---------------------------------------------------------
        // ORDENAR DE MAYOR A MENOR
        // ---------------------------------------------------------

        results.Sort(
            (a, b) =>
                b.ResultPercentage.CompareTo(
                    a.ResultPercentage
                )
        );


        return results;
    }


    /// <summary>
    /// Devuelve el ganador.
    ///
    /// Si hay empate exacto, selecciona
    /// aleatoriamente uno de los empatados.
    /// </summary>
    private ElectionPartyResult DetermineWinner(
        List<ElectionPartyResult> results)
    {
        if (results == null || results.Count == 0)
        {
            return null;
        }


        float highestResult =
            results[0].ResultPercentage;

        List<ElectionPartyResult> tied =
            new List<ElectionPartyResult>();


        foreach (ElectionPartyResult result in results)
        {
            if (result == null)
            {
                continue;
            }

            if (
                Mathf.Abs(
                    result.ResultPercentage -
                    highestResult
                )
                <= 0.0001f
            )
            {
                tied.Add(result);
            }
        }


        if (tied.Count == 0)
        {
            return results[0];
        }


        if (tied.Count == 1)
        {
            return tied[0];
        }


        return tied[
            Random.Range(
                0,
                tied.Count
            )
        ];
    }


    /// <summary>
    /// Obtiene el partido actual del jugador
    /// a partir del líder seleccionado.
    /// </summary>
    private PoliticalPartySO GetPlayerParty()
    {
        CountryState countryState =
            CountryState.Instance;

        if (countryState == null)
        {
            return null;
        }


        if (countryState.CurrentScenario == null)
        {
            return null;
        }


        ScenarioLeaderRuntime selectedLeader =
            countryState
                .CurrentScenario
                .SelectedLeader;


        if (selectedLeader == null)
        {
            return null;
        }


        if (selectedLeader.Party == null)
        {
            return null;
        }


        return selectedLeader.Party.BaseParty;
    }


    /// <summary>
    /// Se llama desde el botón Continuar
    /// solamente si el jugador ha ganado.
    /// </summary>
    public void ContinueAfterVictory()
    {
        if (!electionOpen)
        {
            return;
        }


        CountryState countryState =
            CountryState.Instance;


        if (countryState != null)
        {
            /*
             * Reiniciamos únicamente el ciclo
             * electoral y el historial gráfico.
             *
             * La fecha y el estado actual del país
             * permanecen intactos.
             */
            countryState.ResetElectionCycle(
                turnsPerElection
            );
        }


        if (ElectionPollManager.Instance != null)
        {
            /*
             * La última encuesta pasa a convertirse
             * en el primer punto de la nueva gráfica.
             */
            ElectionPollManager.Instance
                .ResetHistoryKeepingLatest();
        }


        electionOpen = false;


        if (electionResultPanel != null)
        {
            electionResultPanel.HideImmediate();
        }


        /*
         * Restauramos la velocidad anterior.
         *
         * Si por alguna razón era 0,
         * volvemos a 1 para evitar dejar
         * la simulación congelada.
         */
        Time.timeScale =
            previousTimeScale <= 0f
                ? 1f
                : previousTimeScale;
    }
}



/// <summary>
/// Resultado electoral individual de un partido.
/// </summary>
[System.Serializable]
public class ElectionPartyResult
{
    public PoliticalPartySO Party { get; private set; }

    public float PollPercentage { get; private set; }

    public float ResultPercentage { get; set; }


    public ElectionPartyResult(
        PoliticalPartySO party,
        float pollPercentage,
        float resultPercentage)
    {
        Party = party;
        PollPercentage = pollPercentage;
        ResultPercentage = resultPercentage;
    }
}