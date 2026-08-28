using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioLeaderRuntime
{
    // =========================================================
    // SOURCE DATA
    // =========================================================

    public ScenarioLeaderConfig SourceConfig { get; private set; }

    public PoliticalLeaderSO BaseLeader { get; private set; }


    // =========================================================
    // DISPLAY DATA
    // =========================================================

    public string DisplayName { get; private set; }

    public Sprite DisplayPortrait { get; private set; }


    // =========================================================
    // PARTY
    // =========================================================

    public ScenarioPartyRuntime Party { get; private set; }


    // =========================================================
    // PLAYER AVAILABILITY
    // =========================================================

    public bool SelectableByPlayer { get; private set; }


    // =========================================================
    // ELECTORAL EFFECT
    // =========================================================

    public float OwnPartyVoteModifier { get; private set; }

    public List<LeaderVoteModifier> VoteModifiers { get; private set; } =
        new List<LeaderVoteModifier>();


    // =========================================================
    // PARAMETER OVERRIDES
    // =========================================================

    private Dictionary<string, float> parameterOverrides =
        new Dictionary<string, float>();


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public ScenarioLeaderRuntime(
        ScenarioLeaderConfig config,
        ScenarioPartyRuntime party
    )
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        SourceConfig = config;
        BaseLeader = config.leader;

        Party = party;

        SelectableByPlayer =
            config.selectableByPlayer;

        LoadDisplayData(config);
        LoadParameterOverrides(config);
        LoadElectoralEffects(config);
    }


    // =========================================================
    // DISPLAY DATA
    // =========================================================

    private void LoadDisplayData(
        ScenarioLeaderConfig config
    )
    {
        DisplayName =
            config.GetDisplayName();

        DisplayPortrait =
            config.GetDisplayPortrait();
    }


    // =========================================================
    // PARAMETER OVERRIDES
    // =========================================================

    private void LoadParameterOverrides(
        ScenarioLeaderConfig config
    )
    {
        parameterOverrides.Clear();

        if (config.parameterOverrides == null)
            return;

        foreach (
            ScenarioLeaderParameterOverride parameter
            in config.parameterOverrides
        )
        {
            if (parameter == null)
                continue;

            if (string.IsNullOrWhiteSpace(parameter.parameterId))
                continue;

            parameterOverrides[
                parameter.parameterId
            ] = parameter.value;
        }
    }


    public bool HasParameterOverride(
        string parameterId
    )
    {
        if (string.IsNullOrWhiteSpace(parameterId))
            return false;

        return parameterOverrides.ContainsKey(
            parameterId
        );
    }


    public bool TryGetParameterOverride(
        string parameterId,
        out float value
    )
    {
        if (string.IsNullOrWhiteSpace(parameterId))
        {
            value = 0f;
            return false;
        }

        return parameterOverrides.TryGetValue(
            parameterId,
            out value
        );
    }


    // =========================================================
    // ELECTORAL EFFECTS
    // =========================================================

    private void LoadElectoralEffects(
        ScenarioLeaderConfig config
    )
    {
        OwnPartyVoteModifier =
            config.ownPartyVoteModifier;

        VoteModifiers.Clear();

        if (config.voteModifiers == null)
            return;

        VoteModifiers.AddRange(
            config.voteModifiers
        );
    }


    // =========================================================
    // APPLY LEADER EFFECTS
    // =========================================================

    public void ApplyVoteModifiers(
        ScenarioRuntimeState scenario
    )
    {
        if (scenario == null)
            return;

        // Bonus o penalización al partido propio.
        if (Party != null)
        {
            Party.AddVoteShare(
                OwnPartyVoteModifier
            );
        }

        // Modificadores sobre otros partidos.
        foreach (
            LeaderVoteModifier modifier
            in VoteModifiers
        )
        {
            if (modifier == null)
                continue;

            ScenarioPartyRuntime targetParty =
                modifier.GetScenarioParty(
                    scenario.SourceScenario
                ) != null
                ? scenario.GetPartyById(
                    modifier.scenarioPartyId
                )
                : null;

            if (targetParty == null)
                continue;

            targetParty.AddVoteShare(
                modifier.voteChange
            );
        }
    }
}