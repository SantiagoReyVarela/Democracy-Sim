using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define un efecto individual provocado por una política.
/// </summary>
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

    public bool IsImmediate => delayInTurns == 0;

    public bool IsPermanent => durationInTurns == 0;
}


/// <summary>
/// Contiene los datos permanentes de una política.
///
/// Al ser un ScriptableObject, permite crear políticas desde el menú
/// de Assets sin escribir una clase nueva para cada una.
/// </summary>
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


    [Header("Effects")]

    [SerializeField]
    private List<PolicyEffect> effects = new List<PolicyEffect>();


    public string PolicyId => policyId;

    public string PolicyName => policyName;

    public string Description => description;

    public float PoliticalCost => politicalCost;

    public IReadOnlyList<PolicyEffect> Effects => effects;


    /// <summary>
    /// Busca el efecto de esta política sobre un parámetro concreto.
    /// Devuelve cero si la política no afecta a dicho parámetro.
    /// </summary>
    public float GetTotalEffectOnParameter(CountryParameter parameter)
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