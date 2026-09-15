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

    [Tooltip("Map image displayed for this scenario.")]
    public Sprite countryMap;

    [Tooltip("Example: Federal State, Unitary State, Confederation...")]
    public string stateType;

    [Tooltip("Example: Presidential Republic, Parliamentary Republic...")]
    public string governmentType;


    // =========================================================
    // STARTING DATE
    // =========================================================

    [Header("Starting Date")]

    [HistoricalYearSlider(
        1700,
        2100,

        1700,
        1800,
        1900,
        2000,
        2100
    )]
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

    [Tooltip("Political parties configured for this scenario.")]
    public List<ScenarioPartyConfig> parties =
        new List<ScenarioPartyConfig>();


    // =========================================================
    // AVAILABLE LEADERS
    // =========================================================

    [Header("Available Leaders")]

    [Tooltip("Political leaders selectable at the beginning of the scenario.")]
    public List<ScenarioLeaderConfig> availableLeaders =
        new List<ScenarioLeaderConfig>();


    // =========================================================
    // POLICY SLOTS
    // =========================================================

    [Header("Policy Slots")]

    [Tooltip("Policy slots available in this scenario.")]
    public ScenarioPolicySlots policySlots;


    // =========================================================
    // HELPERS
    // =========================================================

    public ScenarioPartyConfig GetPartyConfig(PoliticalPartySO party)
    {
        if (party == null || parties == null)
            return null;

        return parties.Find(
            p => p != null &&
                 p.GetResolvedBaseParty() == party
        );
    }

    public bool IsPartyAvailable(PoliticalPartySO party)
    {
        ScenarioPartyConfig config = GetPartyConfig(party);

        return config != null && config.available;
    }

    public ScenarioPartyConfig GetScenarioParty(string scenarioPartyId)
    {
        if (string.IsNullOrWhiteSpace(scenarioPartyId) || parties == null)
            return null;

        return parties.Find(
            p => p != null &&
                 string.Equals(
                     p.scenarioPartyId,
                     scenarioPartyId,
                     StringComparison.OrdinalIgnoreCase
                 )
        );
    }

    public bool HasScenarioParty(string scenarioPartyId)
    {
        return GetScenarioParty(scenarioPartyId) != null;
    }

    public ScenarioPartyConfig GetLeaderParty(ScenarioLeaderConfig leaderConfig)
    {
        if (leaderConfig == null)
            return null;

        return GetScenarioParty(leaderConfig.scenarioPartyId);
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

    [Tooltip("Initial monthly inflation value.")]
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
    // HISTORICAL PARTY
    // ---------------------------------------------------------

    [Header("Historical Party")]

    [Tooltip(
        "Historical political party represented by this scenario entry. " +
        "Example: PSOE, Republican Party, Communist Party of China."
    )]
    public PoliticalPartyProfileSO partyProfile;

    [Tooltip(
        "Legacy PoliticalPartySO reference. " +
        "Kept temporarily so existing scenarios do not break."
    )]
    public PoliticalPartySO party;

    [Tooltip(
        "Unique ID of this party configuration inside the scenario. " +
        "Leaders and vote modifiers reference this ID instead of the base asset."
    )]
    public string scenarioPartyId;

    [Tooltip("Whether this party exists and participates in this scenario.")]
    public bool available = true;


    // ---------------------------------------------------------
    // SCENARIO OVERRIDES
    // ---------------------------------------------------------

    [Header("Scenario Overrides")]

    [Tooltip(
        "If enabled, this scenario can use custom data instead of " +
        "the values defined in the base PoliticalPartySO."
    )]
    public bool overridePartyData = false;

    [Tooltip("Custom party name used only in this scenario.")]
    public string customName;

    [Tooltip("Custom party color used only in this scenario.")]
    public Color customColor = Color.white;

    [Tooltip("Custom ideology used only in this scenario.")]
    public string customIdeology;

    [Tooltip(
        "Only add entries for ideological axes that deviate from the base PoliticalPartySO. " +
        "Any axis not listed here keeps the original value from the base party asset."
    )]
    public ScenarioIdeologicalPositions ideologicalPositions =
        new ScenarioIdeologicalPositions();


    // ---------------------------------------------------------
    // INITIAL ELECTORAL SITUATION
    // ---------------------------------------------------------

    [Header("Initial Electoral Situation")]

    [Range(0f, 100f)]
    [Tooltip("Default initial vote intention.")]
    public float initialVoteShare;


    // ---------------------------------------------------------
    // HISTORICAL TREND
    // ---------------------------------------------------------

    [Header("Historical Trend")]

    [Tooltip("Enables the historical tendency of this party.")]
    public bool useHistoricalTrend = false;

    [Tooltip(
        "Natural electoral change per turn. " +
        "Positive values increase support and negative values reduce it."
    )]
    public float historicalTrendPerTurn;


    // ---------------------------------------------------------
    // HISTORICAL LIMITS
    // ---------------------------------------------------------

    [Header("Historical Limits")]

    [Range(0f, 100f)]
    [Tooltip("Minimum support allowed by the historical tendency.")]
    public float minimumHistoricalSupport = 0f;

    [Range(0f, 100f)]
    [Tooltip("Maximum support allowed by the historical tendency.")]
    public float maximumHistoricalSupport = 100f;


    // ---------------------------------------------------------
    // SITUATION SENSITIVITY
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


    // =========================================================
    // HELPERS
    // =========================================================

    public PoliticalPartySO GetResolvedBaseParty()
    {
        if (
            partyProfile != null &&
            partyProfile.BaseParty != null
        )
        {
            return partyProfile.BaseParty;
        }

        return party;
    }

    public string GetDisplayName()
    {
        if (
            overridePartyData &&
            !string.IsNullOrWhiteSpace(customName)
        )
        {
            return customName;
        }

        if (
            partyProfile != null &&
            !string.IsNullOrWhiteSpace(
                partyProfile.PartyName
            )
        )
        {
            return partyProfile.PartyName;
        }

        PoliticalPartySO baseParty =
            GetResolvedBaseParty();

        if (baseParty != null)
        {
            return baseParty.partyName;
        }

        return "Unknown Party";
    }

    public Color GetDisplayColor()
    {
        if (overridePartyData)
            return customColor;

        if (partyProfile != null)
            return partyProfile.PartyColor;

        PoliticalPartySO baseParty =
            GetResolvedBaseParty();

        if (baseParty != null)
            return baseParty.partyColor;

        return Color.white;
    }

    public string GetDisplayIdeology()
    {
        if (
            overridePartyData &&
            !string.IsNullOrWhiteSpace(customIdeology)
        )
        {
            return customIdeology;
        }

        if (
            partyProfile != null &&
            !string.IsNullOrWhiteSpace(
                partyProfile.IdeologicalLine
            )
        )
        {
            return partyProfile.IdeologicalLine;
        }

        PoliticalPartySO baseParty =
            GetResolvedBaseParty();

        if (baseParty != null)
            return baseParty.IdeologicalLine;

        return string.Empty;
    }

    public bool HasIdeologicalOverrides()
    {
        return
            ideologicalPositions != null &&
            ideologicalPositions.HasOverrides;
    }

    public bool HasIdeologicalOverride(PoliticalAxis axis)
    {
        return
            ideologicalPositions != null &&
            ideologicalPositions.HasOverride(axis);
    }

    public bool TryGetIdeologicalOverride(
        PoliticalAxis axis,
        out float position
    )
    {
        if (ideologicalPositions != null)
            return ideologicalPositions.TryGetOverride(axis, out position);

        position = 0f;
        return false;
    }
}


// ============================================================================
// IDEOLOGICAL POSITIONS
// ============================================================================


[Serializable]
public class ScenarioIdeologicalPosition
{
    [Tooltip(
        "Ideological axis that differs from the base party. " +
        "Do not add an entry if the scenario should keep the original value."
    )]
    public PoliticalAxis axis;

    [Range(-5f, 5f)]
    [Tooltip(
        "Scenario-specific position for this axis, from -5 to +5. " +
        "This value replaces only this axis; all other axes remain inherited from the base party."
    )]
    public float position;
}


[Serializable]
public class ScenarioIdeologicalPositions
{
    [Tooltip(
        "List only the ideological deviations from the base PoliticalPartySO. " +
        "For example, if a communist party adopts free-market economics, add only Economy here. " +
        "Government, Religion, Trade, Society, Immigration and Military continue using the base asset values."
    )]
    public List<ScenarioIdeologicalPosition> overrides =
        new List<ScenarioIdeologicalPosition>();


    public bool HasOverrides =>
        overrides != null &&
        overrides.Count > 0;


    public bool HasOverride(PoliticalAxis axis)
    {
        if (overrides == null)
            return false;

        return overrides.Exists(
            p => p != null && p.axis == axis
        );
    }


    public bool TryGetOverride(
        PoliticalAxis axis,
        out float position
    )
    {
        if (overrides != null)
        {
            ScenarioIdeologicalPosition entry =
                overrides.Find(
                    p => p != null && p.axis == axis
                );

            if (entry != null)
            {
                position = entry.position;
                return true;
            }
        }

        position = 0f;
        return false;
    }


    public void SetOverride(
        PoliticalAxis axis,
        float value
    )
    {
        if (overrides == null)
            overrides = new List<ScenarioIdeologicalPosition>();

        ScenarioIdeologicalPosition entry =
            overrides.Find(
                p => p != null && p.axis == axis
            );

        if (entry == null)
        {
            entry = new ScenarioIdeologicalPosition
            {
                axis = axis
            };

            overrides.Add(entry);
        }

        entry.position = Mathf.Clamp(value, -5f, 5f);
    }


    public bool RemoveOverride(PoliticalAxis axis)
    {
        if (overrides == null)
            return false;

        int removed = overrides.RemoveAll(
            p => p != null && p.axis == axis
        );

        return removed > 0;
    }


    public void ClearOverrides()
    {
        if (overrides == null)
            overrides = new List<ScenarioIdeologicalPosition>();
        else
            overrides.Clear();
    }
}


// ============================================================================
// LEADER CONFIGURATION
// ============================================================================

[Serializable]
public class ScenarioLeaderConfig
{
    // ---------------------------------------------------------
    // BASE LEADER
    // ---------------------------------------------------------

    [Header("Base Leader")]

    [Tooltip(
        "Base PoliticalLeaderSO used as a template. " +
        "The original asset is never modified by this scenario."
    )]
    public PoliticalLeaderSO leader;

    [Tooltip("Whether the player can select this leader at the beginning of the scenario.")]
    public bool selectableByPlayer = true;


    // ---------------------------------------------------------
    // SCENARIO PARTY
    // ---------------------------------------------------------

    [Header("Scenario Party")]

    [Tooltip(
        "ID of the party configuration inside this GameScenarioSO. " +
        "This links the leader to the scenario-specific version of the party, " +
        "including its overridden name, color, ideology and positions."
    )]
    public string scenarioPartyId;


    // ---------------------------------------------------------
    // SCENARIO OVERRIDES
    // ---------------------------------------------------------

    [Header("Scenario Overrides")]

    [Tooltip(
        "If enabled, this scenario can override data from the base leader asset " +
        "without modifying the PoliticalLeaderSO itself."
    )]
    public bool overrideLeaderData = false;

    [Tooltip("Custom leader name used only in this scenario.")]
    public string customName;

    [Tooltip("Custom portrait used only in this scenario.")]
    public Sprite customPortrait;


    // ---------------------------------------------------------
    // SCENARIO-SPECIFIC LEADER PARAMETERS
    // ---------------------------------------------------------

    [Header("Scenario-Specific Leader Parameters")]

    [Tooltip(
        "Optional numeric parameters overridden only for this scenario. " +
        "Use the same parameter IDs/names used by your leader system."
    )]
    public List<ScenarioLeaderParameterOverride> parameterOverrides =
        new List<ScenarioLeaderParameterOverride>();


    // ---------------------------------------------------------
    // ELECTORAL EFFECT
    // ---------------------------------------------------------

    [Header("Electoral Effect")]

    [Tooltip(
        "General vote bonus or penalty applied to this leader's scenario party."
    )]
    public float ownPartyVoteModifier;

    [Tooltip(
        "Changes applied to the initial vote intention of scenario parties " +
        "when this leader is selected."
    )]
    public List<LeaderVoteModifier> voteModifiers =
        new List<LeaderVoteModifier>();


    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    public string GetDisplayName()
    {
        if (
            overrideLeaderData &&
            !string.IsNullOrWhiteSpace(customName)
        )
        {
            return customName;
        }

        if (leader != null)
            return leader.name;

        return "Unknown Leader";
    }

    public Sprite GetDisplayPortrait()
    {
        if (
            overrideLeaderData &&
            customPortrait != null
        )
        {
            return customPortrait;
        }

        return null;
    }

    public ScenarioPartyConfig GetScenarioParty(GameScenarioSO scenario)
    {
        if (scenario == null)
            return null;

        return scenario.GetScenarioParty(scenarioPartyId);
    }
}


// ============================================================================
// LEADER PARAMETER OVERRIDE
// ============================================================================

[Serializable]
public class ScenarioLeaderParameterOverride
{
    [Tooltip(
        "Identifier/name of the leader parameter to override in this scenario."
    )]
    public string parameterId;

    [Tooltip("Value used only in this scenario.")]
    public float value;
}


// ============================================================================
// LEADER VOTE MODIFIER
// ============================================================================

[Serializable]
public class LeaderVoteModifier
{
    [Tooltip(
        "ID of the scenario-specific party affected by this modifier."
    )]
    public string scenarioPartyId;

    [Tooltip(
        "Percentage points added or subtracted from this scenario party."
    )]
    public float voteChange;

    public ScenarioPartyConfig GetScenarioParty(GameScenarioSO scenario)
    {
        if (scenario == null)
            return null;

        return scenario.GetScenarioParty(scenarioPartyId);
    }
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


// ============================================================================
// HISTORICAL YEAR SLIDER ATTRIBUTE
// ============================================================================

public class HistoricalYearSliderAttribute : PropertyAttribute
{
    public int minYear;
    public int maxYear;

    public int[] markerYears;

    public HistoricalYearSliderAttribute(
        int minYear,
        int maxYear,
        params int[] markerYears
    )
    {
        this.minYear = minYear;
        this.maxYear = maxYear;
        this.markerYears = markerYears;
    }
}