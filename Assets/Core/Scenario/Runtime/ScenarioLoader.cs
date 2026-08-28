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
        if (loadOnStart)
        {
            LoadScenario(scenarioToLoad);
        }
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

        // Seleccionar automáticamente el primer líder jugable
        ScenarioLeaderRuntime defaultLeader =
            CurrentScenario.Leaders.Find(
                l => l.SelectableByPlayer
            );

        if (defaultLeader != null)
        {
            CurrentScenario.SelectLeader(defaultLeader);
        }


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
    // INITIALIZE MANAGERS
    // =========================================================

    private void InitializeManagers()
    {
        InitializeCountryState();
        InitializePoliticalPartyManager();
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