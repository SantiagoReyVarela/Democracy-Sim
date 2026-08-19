using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewGameScenario",
    menuName = "Democracy Simulator/Scenarios/Game Scenario"
)]
public class GameScenarioSO : ScriptableObject
{
    // =========================================================
    // IDENTIFICATION
    // =========================================================

    [Header("Identification")]

    [Tooltip("Internal unique ID used to identify this scenario.")]
    public string scenarioId;

    [Tooltip("Name displayed to the player.")]
    public string scenarioName;

    [TextArea(2, 5)]
    [Tooltip("Optional general description of the scenario.")]
    public string description;


    // =========================================================
    // COUNTRY
    // =========================================================

    [Header("Country")]

    [Tooltip("Name of the country.")]
    public string countryName;

    [Tooltip("Flag displayed for this scenario.")]
    public Sprite countryFlag;

    [Tooltip("Example: Federal State, Unitary State, Confederation...")]
    public string stateType;

    [Tooltip("Example: Presidential Republic, Parliamentary Republic...")]
    public string governmentType;


    // =========================================================
    // STARTING DATE
    // =========================================================

    [Header("Starting Date")]

    [Range(1, 3000)]
    [Tooltip("Year in which the scenario begins.")]
    public int startYear = 1930;

    [Range(1, 12)]
    [Tooltip("Month in which the scenario begins.")]
    public int startMonth = 1;


    // =========================================================
    // HISTORICAL CONTEXT
    // =========================================================

    [Header("Historical Context")]

    [TextArea(3, 8)]
    [Tooltip("Brief description of the political situation at the start of the scenario.")]
    public string politicalSituation;

    [TextArea(3, 8)]
    [Tooltip("Brief description of the economic situation at the start of the scenario.")]
    public string economicSituation;


    // =========================================================
    // INITIAL COUNTRY PARAMETERS
    // =========================================================

    [Header("Initial Country Parameters")]

    [Tooltip("Initial economic, political and social values of the country.")]
    public ScenarioInitialParameters initialParameters;


    // =========================================================
    // PARTIES
    // =========================================================

    [Header("Parties")]

    [Tooltip("Configuration of the political parties for this scenario.")]
    public List<ScenarioPartyConfig> parties = new List<ScenarioPartyConfig>();


    // =========================================================
    // LEADERS
    // =========================================================

    [Header("Available Leaders")]

    [Tooltip("Political leaders that can be selected at the beginning of the scenario.")]
    public List<ScenarioLeaderConfig> availableLeaders = new List<ScenarioLeaderConfig>();


    // =========================================================
    // POLICY SLOTS
    // =========================================================

    [Header("Policy Slots")]

    [Tooltip("Number of policy slots available for each policy category.")]
    public ScenarioPolicySlots policySlots;


    // =========================================================
    // HELPERS
    // =========================================================

    /// <summary>
    /// Returns the configuration of a specific party in this scenario.
    /// </summary>
    public ScenarioPartyConfig GetPartyConfig(PoliticalPartySO party)
    {
        if (party == null || parties == null)
            return null;

        return parties.Find(p => p.party == party);
    }


    /// <summary>
    /// Returns whether a political party is available in this scenario.
    /// </summary>
    public bool IsPartyAvailable(PoliticalPartySO party)
    {
        ScenarioPartyConfig config = GetPartyConfig(party);

        return config != null && config.available;
    }
}


// ============================================================================
// INITIAL COUNTRY PARAMETERS
// ============================================================================

[Serializable]
public class ScenarioInitialParameters
{
    // ---------------------------------------------------------
    // ECONOMY
    // ---------------------------------------------------------

    [Header("Economy")]

    [Tooltip("Initial inflation value.")]
    public float inflation;

    [Tooltip("Initial fiscal result / budget balance.")]
    public float fiscalResult;

    [Tooltip("Initial government reserves.")]
    public float reserves;

    [Tooltip("Initial public debt.")]
    public float debt;


    // ---------------------------------------------------------
    // POLITICAL & SOCIAL
    // ---------------------------------------------------------

    [Header("Political & Social")]

    [Tooltip("Initial political stability.")]
    public float stability;

    [Tooltip("Initial education level.")]
    public float education;
}


// ============================================================================
// PARTY CONFIGURATION
// ============================================================================

[Serializable]
public class ScenarioPartyConfig
{
    // ---------------------------------------------------------
    // PARTY
    // ---------------------------------------------------------

    [Header("Party")]

    [Tooltip("Political party represented by this configuration.")]
    public PoliticalPartySO party;

    [Tooltip("Whether this party exists and can participate in this scenario.")]
    public bool available = true;


    // ---------------------------------------------------------
    // INITIAL ELECTORAL SITUATION
    // ---------------------------------------------------------

    [Header("Initial Electoral Situation")]

    [Range(0f, 100f)]
    [Tooltip("Default vote intention at the beginning of the scenario.")]
    public float initialVoteShare;


    // ---------------------------------------------------------
    // HISTORICAL TREND
    // ---------------------------------------------------------

    [Header("Historical Trend")]

    [Tooltip("Enables the historical electoral tendency for this party.")]
    public bool useHistoricalTrend = false;

    [Tooltip(
        "Natural electoral tendency per turn. " +
        "Positive values make the party grow and negative values make it decline."
    )]
    public float historicalTrendPerTurn;


    // ---------------------------------------------------------
    // HISTORICAL LIMITS
    // ---------------------------------------------------------

    [Header("Historical Limits")]

    [Range(0f, 100f)]
    [Tooltip("Minimum support allowed by the historical tendency system.")]
    public float minimumHistoricalSupport = 0f;

    [Range(0f, 100f)]
    [Tooltip("Maximum support allowed by the historical tendency system.")]
    public float maximumHistoricalSupport = 100f;


    // ---------------------------------------------------------
    // COUNTRY SITUATION SENSITIVITY
    // ---------------------------------------------------------

    [Header("Situation Sensitivity")]

    [Tooltip("How strongly an economic crisis affects this party.")]
    public float economicCrisisSensitivity;

    [Tooltip("How strongly political instability affects this party.")]
    public float politicalInstabilitySensitivity;

    [Tooltip("How strongly inflation affects this party.")]
    public float inflationSensitivity;

    [Tooltip("How strongly unemployment affects this party.")]
    public float unemploymentSensitivity;
}


// ============================================================================
// LEADER CONFIGURATION
// ============================================================================

[Serializable]
public class ScenarioLeaderConfig
{
    // ---------------------------------------------------------
    // LEADER
    // ---------------------------------------------------------

    [Header("Leader")]

    [Tooltip("Name of the political leader.")]
    public string leaderName;

    [Tooltip("Portrait displayed for this leader.")]
    public Sprite portrait;

    [Tooltip("Political party represented by this leader.")]
    public PoliticalPartySO party;


    // ---------------------------------------------------------
    // PLAYER AVAILABILITY
    // ---------------------------------------------------------

    [Header("Player Availability")]

    [Tooltip("Whether the player can select this leader at the beginning.")]
    public bool selectableByPlayer = true;


    // ---------------------------------------------------------
    // ELECTORAL EFFECT
    // ---------------------------------------------------------

    [Header("Electoral Effect")]

    [Tooltip(
        "General vote bonus or penalty applied to the leader's own party " +
        "when this leader is selected."
    )]
    public float ownPartyVoteModifier;

    [Tooltip(
        "Changes to the initial vote intention of individual parties " +
        "when this leader is selected."
    )]
    public List<LeaderVoteModifier> voteModifiers = new List<LeaderVoteModifier>();
}


// ============================================================================
// LEADER-SPECIFIC VOTE MODIFIERS
// ============================================================================

[Serializable]
public class LeaderVoteModifier
{
    [Tooltip("Party affected by this modifier.")]
    public PoliticalPartySO party;

    [Tooltip(
        "Percentage points added or subtracted from this party's initial vote share."
    )]
    public float voteChange;
}


// ============================================================================
// POLICY SLOT CONFIGURATION
// ============================================================================

[Serializable]
public class ScenarioPolicySlots
{
    [Header("Policy Slots")]

    [Min(0)]
    public int economySlots = 2;

    [Min(0)]
    public int governmentSlots = 2;

    [Min(0)]
    public int immigrationSlots = 1;

    [Min(0)]
    public int militarySlots = 1;

    [Min(0)]
    public int religionSlots = 1;

    [Min(0)]
    public int socialSlots = 1;

    [Min(0)]
    public int tradeSlots = 1;
}