using UnityEngine;

public class ScenarioLoader : MonoBehaviour
{
    // =========================================================
    // SCENARIO SOURCE
    // =========================================================

    [Header("Scenario")]

    [Tooltip("Scenario that will be loaded when the game starts.")]
    [SerializeField]
    private GameScenarioSO scenarioToLoad;


    // =========================================================
    // MANAGERS
    // =========================================================

    [Header("Managers")]

    [Tooltip("Political party manager that will receive the scenario parties.")]
    [SerializeField]
    private PoliticalPartyManager politicalPartyManager;

    [Tooltip(
        "CountryState that will receive the country data, date and initial parameters."
    )]
    [SerializeField]
    private CountryState countryState;


    // =========================================================
    // CURRENT RUNTIME STATE
    // =========================================================

    public ScenarioRuntimeState CurrentScenario { get; private set; }


    // =========================================================
    // AUTO LOAD
    // =========================================================

    [Header("Loading")]

    [Tooltip("If enabled, the scenario is loaded automatically on Start.")]
    [SerializeField]
    private bool loadOnStart = true;


    private void Start()
    {
        if (!loadOnStart)
        {
            return;
        }

        GameScenarioSO scenario =
            CampaignMenuManager.SelectedScenario != null
                ? CampaignMenuManager.SelectedScenario
                : scenarioToLoad;

        LoadScenario(scenario);
    }


    // =========================================================
    // LOAD SCENARIO
    // =========================================================

    public void LoadScenario(GameScenarioSO scenario)
    {
        if (scenario == null)
        {
            Debug.LogError(
                "ScenarioLoader: Cannot load a null GameScenarioSO."
            );

            return;
        }


        // -----------------------------------------------------
        // 1. CREATE RUNTIME STATE
        // -----------------------------------------------------

        CurrentScenario =
            new ScenarioRuntimeState(
                scenario
            );

        // Seleccionar el líder elegido en el menú de campaña.
        // Si la escena se ejecuta directamente desde el Editor,
        // se mantiene como respaldo el primer líder jugable.
        SelectInitialLeader(scenario);


        Debug.Log(
            $"Scenario loaded: {CurrentScenario.ScenarioName}"
        );

        Debug.Log(
            $"Country: {CurrentScenario.CountryName}"
        );

        Debug.Log(
            $"Date: {CurrentScenario.CurrentMonth}/{CurrentScenario.CurrentYear}"
        );

        Debug.Log(
            $"Parties loaded: {CurrentScenario.Parties.Count}"
        );

        Debug.Log(
            $"Leaders loaded: {CurrentScenario.Leaders.Count}"
        );


        // -----------------------------------------------------
        // 2. VALIDATE SCENARIO
        // -----------------------------------------------------

        ValidateScenario();


        // -----------------------------------------------------
        // 3. INITIALIZE MANAGERS
        // -----------------------------------------------------

        InitializeManagers();
    }


    // =========================================================
    // INITIAL LEADER SELECTION
    // =========================================================

    private void SelectInitialLeader(GameScenarioSO scenario)
    {
        if (CurrentScenario == null)
        {
            return;
        }

        bool cameFromCampaign =
            CampaignMenuManager.SelectedScenario == scenario;

        // -----------------------------------------------------
        // CAMPAIGN: RANDOM MODE
        // -----------------------------------------------------
        // En modo Aleatorio NO elegimos todavía ningún líder.
        // El PoliticalPartyManager decidirá primero qué partido
        // comienza gobernando y después vincularemos al jugador
        // con un líder seleccionable de ese partido.
        if (
            cameFromCampaign &&
            CampaignMenuManager.RandomLeaderSelection
        )
        {
            CurrentScenario.SelectLeader(null);

            Debug.Log(
                "ScenarioLoader: Random leader mode selected. " +
                "Leader selection is deferred until the initial " +
                "government has been determined."
            );

            return;
        }

        ScenarioLeaderRuntime leaderToSelect = null;

        ScenarioLeaderConfig selectedConfig =
            CampaignMenuManager.SelectedLeader;

        // -----------------------------------------------------
        // CAMPAIGN: SPECIFIC LEADER
        // -----------------------------------------------------
        if (
            cameFromCampaign &&
            selectedConfig != null &&
            scenario.availableLeaders != null
        )
        {
            int runtimeIndex = 0;

            foreach (
                ScenarioLeaderConfig config
                in scenario.availableLeaders
            )
            {
                if (config == null)
                {
                    continue;
                }

                if (config == selectedConfig)
                {
                    if (
                        runtimeIndex >= 0 &&
                        runtimeIndex < CurrentScenario.Leaders.Count
                    )
                    {
                        ScenarioLeaderRuntime candidate =
                            CurrentScenario.Leaders[runtimeIndex];

                        if (
                            candidate != null &&
                            candidate.SelectableByPlayer
                        )
                        {
                            leaderToSelect = candidate;
                        }
                    }

                    break;
                }

                runtimeIndex++;
            }
        }

        // -----------------------------------------------------
        // EDITOR / DIRECT SCENE FALLBACK
        // -----------------------------------------------------
        // Si Simulation Manager se ejecuta directamente sin pasar
        // por Campaign, conservamos el comportamiento anterior.
        if (!cameFromCampaign && leaderToSelect == null)
        {
            leaderToSelect =
                CurrentScenario.Leaders.Find(
                    l =>
                        l != null &&
                        l.SelectableByPlayer
                );
        }

        if (leaderToSelect == null)
        {
            Debug.LogWarning(
                cameFromCampaign
                    ? "ScenarioLoader: The selected campaign leader " +
                      "could not be mapped to a selectable runtime leader."
                    : "ScenarioLoader: No selectable leader was found."
            );

            return;
        }

        // Esta selección ocurre antes de InitializeManagers(),
        // por lo que hacemos aquí la parte del método SelectLeader()
        // que no depende todavía de CountryState.
        CurrentScenario.SelectLeader(
            leaderToSelect
        );

        leaderToSelect.ApplyVoteModifiers(
            CurrentScenario
        );

        Debug.Log(
            $"ScenarioLoader: Initial leader selected: " +
            $"{leaderToSelect.DisplayName}"
        );

        if (leaderToSelect.Party != null)
        {
            Debug.Log(
                $"ScenarioLoader: Player party: " +
                $"{leaderToSelect.Party.DisplayName}"
            );
        }
    }


    // =========================================================
    // INITIALIZE MANAGERS
    // =========================================================

    private void InitializeManagers()
    {
        InitializeCountryState();
        InitializePoliticalPartyManager();
        ResolveRandomLeaderAfterGovernmentFormation();
    }


    // =========================================================
    // RANDOM LEADER AFTER GOVERNMENT FORMATION
    // =========================================================

    private void ResolveRandomLeaderAfterGovernmentFormation()
    {
        if (
            CurrentScenario == null ||
            !CampaignMenuManager.RandomLeaderSelection
        )
        {
            return;
        }

        if (politicalPartyManager == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: Cannot resolve random leader because " +
                "PoliticalPartyManager is not assigned."
            );

            return;
        }

        PoliticalPartyState governmentPartyState =
            politicalPartyManager.GovernmentLeader;

        if (governmentPartyState == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: Random leader mode could not find " +
                "a government-leading party."
            );

            return;
        }

        ScenarioPartyRuntime governmentRuntimeParty =
            politicalPartyManager.GetRuntimeParty(
                governmentPartyState
            );

        if (governmentRuntimeParty == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: The government-leading party does not " +
                "have a valid ScenarioPartyRuntime."
            );

            return;
        }

        ScenarioLeaderRuntime leaderToSelect = null;

        foreach (
            ScenarioLeaderRuntime leader
            in CurrentScenario.Leaders
        )
        {
            if (
                leader == null ||
                !leader.SelectableByPlayer ||
                leader.Party == null
            )
            {
                continue;
            }

            if (
                leader.Party.ScenarioPartyId ==
                governmentRuntimeParty.ScenarioPartyId
            )
            {
                leaderToSelect = leader;
                break;
            }
        }

        if (leaderToSelect == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: No selectable leader was found for " +
                $"government party '{governmentRuntimeParty.DisplayName}'."
            );

            return;
        }

        // No aplicamos aquí modificadores de voto: el parlamento ya ha sido
        // generado precisamente para conservar el resultado natural de
        // la opción Aleatorio.
        CurrentScenario.SelectLeader(
            leaderToSelect
        );

        if (countryState == null)
        {
            countryState = CountryState.Instance;
        }

        if (countryState != null)
        {
            countryState.SyncLeaderFromScenario();
        }

        Debug.Log(
            "ScenarioLoader: Random leader resolved after government " +
            $"formation: {leaderToSelect.DisplayName} | " +
            $"Party: {governmentRuntimeParty.DisplayName}"
        );
    }


    // =========================================================
    // COUNTRY STATE
    // =========================================================

    private void InitializeCountryState()
    {
        if (countryState == null)
        {
            countryState = CountryState.Instance;
        }


        if (countryState == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: CountryState is not assigned and " +
                "CountryState.Instance could not be found."
            );

            return;
        }


        countryState.InitializeFromScenario(
            CurrentScenario
        );


        Debug.Log(
            $"ScenarioLoader: CountryState initialized | " +
            $"Country: {CurrentScenario.CountryName} | " +
            $"Date: {CurrentScenario.CurrentMonth}/{CurrentScenario.CurrentYear}"
        );
    }


    // =========================================================
    // POLITICAL PARTY MANAGER
    // =========================================================

    private void InitializePoliticalPartyManager()
    {
        if (politicalPartyManager == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: PoliticalPartyManager is not assigned."
            );

            return;
        }


        politicalPartyManager.InitializeFromScenario(
            CurrentScenario
        );


        Debug.Log(
            $"ScenarioLoader: PoliticalPartyManager initialized " +
            $"with {CurrentScenario.Parties.Count} scenario parties."
        );
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private void ValidateScenario()
    {
        if (CurrentScenario == null)
            return;


        // -----------------------------------------------------
        // PARTIES
        // -----------------------------------------------------

        foreach (
            ScenarioPartyRuntime party
            in CurrentScenario.Parties
        )
        {
            if (party.BaseParty == null)
            {
                Debug.LogWarning(
                    $"Scenario party '{party.DisplayName}' " +
                    $"has no base PoliticalPartySO."
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    party.ScenarioPartyId
                )
            )
            {
                Debug.LogWarning(
                    $"Scenario party '{party.DisplayName}' " +
                    $"has no Scenario Party ID."
                );
            }
        }


        // -----------------------------------------------------
        // LEADERS
        // -----------------------------------------------------

        foreach (
            ScenarioLeaderRuntime leader
            in CurrentScenario.Leaders
        )
        {
            if (leader.BaseLeader == null)
            {
                Debug.LogWarning(
                    $"Leader '{leader.DisplayName}' " +
                    $"has no base PoliticalLeaderSO."
                );
            }


            if (leader.Party == null)
            {
                Debug.LogWarning(
                    $"Leader '{leader.DisplayName}' " +
                    $"is not associated with a valid scenario party."
                );
            }
        }
    }


    // =========================================================
    // LEADER SELECTION
    // =========================================================

    public void SelectLeader(
        ScenarioLeaderRuntime leader
    )
    {
        if (CurrentScenario == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: No scenario is currently loaded."
            );

            return;
        }


        if (leader == null)
        {
            Debug.LogWarning(
                "ScenarioLoader: Cannot select a null leader."
            );

            return;
        }


        if (!leader.SelectableByPlayer)
        {
            Debug.LogWarning(
                $"Leader '{leader.DisplayName}' " +
                $"is not selectable by the player."
            );

            return;
        }


        CurrentScenario.SelectLeader(
            leader
        );


        leader.ApplyVoteModifiers(
            CurrentScenario
        );


        if (countryState == null)
        {
            countryState = CountryState.Instance;
        }


        if (countryState != null)
        {
            countryState.SyncLeaderFromScenario();
        }


        Debug.Log(
            $"Selected leader: {leader.DisplayName}"
        );


        if (leader.Party != null)
        {
            Debug.Log(
                $"Party: {leader.Party.DisplayName}"
            );
        }
    }


    // =========================================================
    // GET LEADER BY INDEX
    // =========================================================

    public ScenarioLeaderRuntime GetLeader(
        int index
    )
    {
        if (CurrentScenario == null)
            return null;


        if (
            index < 0 ||
            index >=
            CurrentScenario.Leaders.Count
        )
        {
            return null;
        }


        return
            CurrentScenario.Leaders[index];
    }


    // =========================================================
    // GET PARTY BY SCENARIO ID
    // =========================================================

    public ScenarioPartyRuntime GetParty(
        string scenarioPartyId
    )
    {
        if (CurrentScenario == null)
            return null;


        return
            CurrentScenario.GetPartyById(
                scenarioPartyId
            );
    }
    
}