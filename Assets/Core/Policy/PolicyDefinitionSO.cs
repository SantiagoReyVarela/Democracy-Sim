using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PolicyEffect
{
    [SerializeField]
    private CountryParameter parameter;

    [SerializeField]
    private float amount;

    [SerializeField, Min(0)]
    private int delayInTurns;

    [SerializeField, Min(0)]
    private int durationInTurns;

    public CountryParameter Parameter => parameter;
    public float Amount => amount;
    public int DelayInTurns => delayInTurns;
    public int DurationInTurns => durationInTurns;
}

[CreateAssetMenu(
    fileName = "NewPolicy",
    menuName = "Democracy Simulator/Policy Definition",
    order = 1
)]
public class PolicyDefinitionSO : ScriptableObject
{
    [Header("Identification")]

    [SerializeField]
    private string policyId;

    [SerializeField]
    private string policyName;

    [SerializeField, TextArea(3, 8)]
    private string description;


    [Header("Political cost")]

    [SerializeField, Min(0f)]
    private float politicalCost;


    [Header("Ideological classification")]

    [SerializeField]
    private PoliticalAxis primaryAxis = PoliticalAxis.Economy;

    [SerializeField]
    private List<PolicyAxisPosition> ideologicalProfile =
        new List<PolicyAxisPosition>();


    [Header("Effects")]

    [SerializeField]
    private List<PolicyEffect> effects =
        new List<PolicyEffect>();


    public string PolicyId => policyId;
    public string PolicyName => policyName;
    public string Description => description;
    public float PoliticalCost => politicalCost;

    public PoliticalAxis PrimaryAxis => primaryAxis;

    public IReadOnlyList<PolicyAxisPosition> IdeologicalProfile =>
        ideologicalProfile;

    public IReadOnlyList<PolicyEffect> Effects => effects;


    public bool TryGetAxisPosition(
        PoliticalAxis axis,
        out PolicyAxisPosition axisPosition
    )
    {
        foreach (PolicyAxisPosition profileEntry in ideologicalProfile)
        {
            if (
                profileEntry != null &&
                profileEntry.Axis == axis
            )
            {
                axisPosition = profileEntry;
                return true;
            }
        }

        axisPosition = null;
        return false;
    }


    public float GetTotalEffectOnParameter(
        CountryParameter parameter
    )
    {
        float totalEffect = 0f;

        foreach (PolicyEffect effect in effects)
        {
            if (effect.Parameter == parameter)
            {
                totalEffect += effect.Amount;
            }
        }

        return totalEffect;
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        policyId = policyId?.Trim();
        policyName = policyName?.Trim();

        if (string.IsNullOrWhiteSpace(policyId))
        {
            policyId = name
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "_");
        }
    }

#endif
}