using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRuntimeState
{
    // =========================================================
    // SOURCE SCENARIO
    // =========================================================

    public GameScenarioSO SourceScenario { get; private set; }


    // =========================================================
    // BASIC DATA
    // =========================================================

    public string ScenarioId { get; private set; }
    public string ScenarioName { get; private set; }

    public string CountryName { get; private set; }
    public Sprite CountryFlag { get; private set; }
    public Sprite CountryMap { get; private set; }

    public string StateType { get; private set; }
    public string GovernmentType { get; private set; }

    public int CurrentYear { get; private set; }
    public int CurrentMonth { get; private set; }


    // =========================================================
    // HISTORICAL CONTEXT
    // =========================================================

    public string PoliticalSituation { get; private set; }
    public string EconomicSituation { get; private set; }


    // =========================================================
    // COUNTRY PARAMETERS
    // =========================================================

    public float Inflation { get; set; }
    public float FiscalResult { get; set; }
    public float Reserves { get; set; }
    public float Debt { get; set; }

    public float Stability { get; set; }
    public float Education { get; set; }


    // =========================================================
    // PARTIES
    // =========================================================

    public List<ScenarioPartyRuntime> Parties { get; private set; } =
        new List<ScenarioPartyRuntime>();


    // =========================================================
    // LEADERS
    // =========================================================

    public List<ScenarioLeaderRuntime> Leaders { get; private set; } =
        new List<ScenarioLeaderRuntime>();


    // =========================================================
    // POLICY SLOTS
    // =========================================================

    public ScenarioPolicySlotsRuntime PolicySlots { get; private set; }


    // =========================================================
    // SELECTED LEADER
    // =========================================================

    public ScenarioLeaderRuntime SelectedLeader { get; private set; }


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public ScenarioRuntimeState(GameScenarioSO scenario)
    {
        if (scenario == null)
        {
            throw new ArgumentNullException(nameof(scenario));
        }

        SourceScenario = scenario;

        LoadBasicData(scenario);
        LoadInitialParameters(scenario);
        LoadParties(scenario);
        LoadLeaders(scenario);
        LoadPolicySlots(scenario);
    }


    // =========================================================
    // BASIC DATA
    // =========================================================

    private void LoadBasicData(GameScenarioSO scenario)
    {
        ScenarioId = scenario.scenarioId;
        ScenarioName = scenario.scenarioName;

        CountryName = scenario.countryName;
        CountryFlag = scenario.countryFlag;
        CountryMap = scenario.countryMap;

        StateType = scenario.stateType;
        GovernmentType = scenario.governmentType;

        CurrentYear = scenario.startYear;
        CurrentMonth = scenario.startMonth;

        PoliticalSituation = scenario.politicalSituation;
        EconomicSituation = scenario.economicSituation;
    }


    // =========================================================
    // INITIAL PARAMETERS
    // =========================================================

    private void LoadInitialParameters(GameScenarioSO scenario)
    {
        if (scenario.initialParameters == null)
            return;

        Inflation = scenario.initialParameters.inflation;
        FiscalResult = scenario.initialParameters.fiscalResult;
        Reserves = scenario.initialParameters.reserves;
        Debt = scenario.initialParameters.debt;

        Stability = scenario.initialParameters.stability;
        Education = scenario.initialParameters.education;
    }


    // =========================================================
    // PARTIES
    // =========================================================

    private void LoadParties(GameScenarioSO scenario)
    {
        Parties.Clear();

        if (scenario.parties == null)
            return;

        foreach (ScenarioPartyConfig config in scenario.parties)
        {
            if (config == null)
                continue;

            if (!config.available)
                continue;

            ScenarioPartyRuntime runtimeParty =
                new ScenarioPartyRuntime(config);

            Parties.Add(runtimeParty);
        }
    }


    // =========================================================
    // LEADERS
    // =========================================================

    private void LoadLeaders(GameScenarioSO scenario)
    {
        Leaders.Clear();

        if (scenario.availableLeaders == null)
            return;

        foreach (ScenarioLeaderConfig config in scenario.availableLeaders)
        {
            if (config == null)
                continue;

            ScenarioPartyRuntime party =
                GetPartyById(config.scenarioPartyId);

            ScenarioLeaderRuntime runtimeLeader =
                new ScenarioLeaderRuntime(
                    config,
                    party
                );

            Leaders.Add(runtimeLeader);
        }
    }


    // =========================================================
    // POLICY SLOTS
    // =========================================================

    private void LoadPolicySlots(GameScenarioSO scenario)
    {
        if (scenario.policySlots == null)
        {
            PolicySlots = new ScenarioPolicySlotsRuntime();
            return;
        }

        PolicySlots =
            new ScenarioPolicySlotsRuntime(
                scenario.policySlots
            );
    }


    // =========================================================
    // PARTY LOOKUP
    // =========================================================

    public ScenarioPartyRuntime GetPartyById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return Parties.Find(
            p => string.Equals(
                p.ScenarioPartyId,
                id,
                StringComparison.OrdinalIgnoreCase
            )
        );
    }


    // =========================================================
    // LEADER SELECTION
    // =========================================================

    public void SelectLeader(ScenarioLeaderRuntime leader)
    {
        if (leader == null)
        {
            SelectedLeader = null;
            return;
        }

        if (!Leaders.Contains(leader))
        {
            Debug.LogWarning(
                "Attempted to select a leader that does not belong to this scenario."
            );

            return;
        }

        SelectedLeader = leader;
    }


    // =========================================================
    // DATE
    // =========================================================

    public void AdvanceMonth()
    {
        CurrentMonth++;

        if (CurrentMonth > 12)
        {
            CurrentMonth = 1;
            CurrentYear++;
        }
    }
}


// ============================================================================
// POLICY SLOTS RUNTIME
// ============================================================================

[Serializable]
public class ScenarioPolicySlotsRuntime
{
    public int economySlots;
    public int governmentSlots;
    public int immigrationSlots;
    public int militarySlots;
    public int religionSlots;
    public int socialSlots;
    public int tradeSlots;


    public ScenarioPolicySlotsRuntime()
    {
    }


    public ScenarioPolicySlotsRuntime(
        ScenarioPolicySlots source
    )
    {
        economySlots = source.economySlots;
        governmentSlots = source.governmentSlots;
        immigrationSlots = source.immigrationSlots;
        militarySlots = source.militarySlots;
        religionSlots = source.religionSlots;
        socialSlots = source.socialSlots;
        tradeSlots = source.tradeSlots;
    }
}