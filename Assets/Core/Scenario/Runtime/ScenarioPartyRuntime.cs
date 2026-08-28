using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioPartyRuntime
{
    // =========================================================
    // SOURCE DATA
    // =========================================================

    public ScenarioPartyConfig SourceConfig { get; private set; }

    public PoliticalPartySO BaseParty { get; private set; }


    // =========================================================
    // IDENTIFICATION
    // =========================================================

    public string ScenarioPartyId { get; private set; }

    public string DisplayName { get; private set; }

    public Color DisplayColor { get; private set; }

    public string DisplayIdeology { get; private set; }


    // =========================================================
    // ELECTORAL DATA
    // =========================================================

    public float VoteShare { get; set; }

    public bool UseHistoricalTrend { get; private set; }

    public float HistoricalTrendPerTurn { get; private set; }

    public float MinimumHistoricalSupport { get; private set; }

    public float MaximumHistoricalSupport { get; private set; }


    // =========================================================
    // SITUATION SENSITIVITY
    // =========================================================

    public float EconomicCrisisSensitivity { get; private set; }

    public float PoliticalInstabilitySensitivity { get; private set; }

    public float InflationSensitivity { get; private set; }

    public float UnemploymentSensitivity { get; private set; }


    // =========================================================
    // IDEOLOGICAL OVERRIDES
    // =========================================================

    private Dictionary<ScenarioIdeologicalAxis, float>
        ideologicalOverrides =
            new Dictionary<ScenarioIdeologicalAxis, float>();


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public ScenarioPartyRuntime(
        ScenarioPartyConfig config
    )
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        SourceConfig = config;
        BaseParty = config.party;

        ScenarioPartyId = config.scenarioPartyId;

        LoadDisplayData(config);
        LoadElectoralData(config);
        LoadSensitivities(config);
        LoadIdeologicalOverrides(config);
    }


    // =========================================================
    // DISPLAY DATA
    // =========================================================

    private void LoadDisplayData(
        ScenarioPartyConfig config
    )
    {
        // -----------------------------------------
        // NAME
        // -----------------------------------------

        DisplayName = config.GetDisplayName();


        // -----------------------------------------
        // COLOR
        // -----------------------------------------

        DisplayColor = config.GetDisplayColor();


        // -----------------------------------------
        // IDEOLOGY
        // -----------------------------------------

        DisplayIdeology = config.GetDisplayIdeology();
    }


    // =========================================================
    // ELECTORAL DATA
    // =========================================================

    private void LoadElectoralData(
        ScenarioPartyConfig config
    )
    {
        VoteShare = config.initialVoteShare;

        UseHistoricalTrend =
            config.useHistoricalTrend;

        HistoricalTrendPerTurn =
            config.historicalTrendPerTurn;

        MinimumHistoricalSupport =
            config.minimumHistoricalSupport;

        MaximumHistoricalSupport =
            config.maximumHistoricalSupport;
    }


    // =========================================================
    // SENSITIVITY
    // =========================================================

    private void LoadSensitivities(
        ScenarioPartyConfig config
    )
    {
        EconomicCrisisSensitivity =
            config.economicCrisisSensitivity;

        PoliticalInstabilitySensitivity =
            config.politicalInstabilitySensitivity;

        InflationSensitivity =
            config.inflationSensitivity;

        UnemploymentSensitivity =
            config.unemploymentSensitivity;
    }


    // =========================================================
    // IDEOLOGICAL OVERRIDES
    // =========================================================

    private void LoadIdeologicalOverrides(
        ScenarioPartyConfig config
    )
    {
        ideologicalOverrides.Clear();

        if (config.ideologicalPositions == null)
            return;

        if (config.ideologicalPositions.overrides == null)
            return;

        foreach (
            ScenarioIdeologicalPosition position
            in config.ideologicalPositions.overrides
        )
        {
            if (position == null)
                continue;

            ideologicalOverrides[position.axis] =
                Mathf.Clamp(
                    position.position,
                    -5f,
                    5f
                );
        }
    }


    // =========================================================
    // IDEOLOGY LOOKUP
    // =========================================================

    public bool HasIdeologicalOverride(
        ScenarioIdeologicalAxis axis
    )
    {
        return ideologicalOverrides.ContainsKey(axis);
    }


    public bool TryGetIdeologicalOverride(
        ScenarioIdeologicalAxis axis,
        out float value
    )
    {
        return ideologicalOverrides.TryGetValue(
            axis,
            out value
        );
    }


    // =========================================================
    // VOTE MANAGEMENT
    // =========================================================

    public void AddVoteShare(float amount)
    {
        VoteShare += amount;

        VoteShare =
            Mathf.Clamp(
                VoteShare,
                0f,
                100f
            );
    }


    public void SetVoteShare(float value)
    {
        VoteShare =
            Mathf.Clamp(
                value,
                0f,
                100f
            );
    }


    // =========================================================
    // HISTORICAL TREND
    // =========================================================

    public void ApplyHistoricalTrend()
    {
        if (!UseHistoricalTrend)
            return;

        VoteShare += HistoricalTrendPerTurn;

        VoteShare =
            Mathf.Clamp(
                VoteShare,
                MinimumHistoricalSupport,
                MaximumHistoricalSupport
            );
    }
}