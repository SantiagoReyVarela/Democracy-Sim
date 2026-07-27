using System;
using UnityEngine;

/// <summary>
/// Representa un efecto de política pendiente o recurrente durante la partida.
/// Es una copia independiente de los datos del ScriptableObject.
/// </summary>
[Serializable]
public sealed class ActivePolicyEffect
{
    [SerializeField]
    private string sourcePolicyName;

    [SerializeField]
    private CountryParameter parameter;

    [SerializeField]
    private float amount;

    [SerializeField, Min(0)]
    private int remainingDelay;

    [SerializeField, Min(0)]
    private int remainingApplications;

    [SerializeField]
    private bool skipNextProcessing;


    public string SourcePolicyName => sourcePolicyName;

    public CountryParameter Parameter => parameter;

    public float Amount => amount;

    public int RemainingDelay => remainingDelay;

    public int RemainingApplications => remainingApplications;

    public bool IsCompleted => remainingApplications <= 0;


    public ActivePolicyEffect(
        string sourcePolicyName,
        CountryParameter parameter,
        float amount,
        int remainingDelay,
        int remainingApplications,
        bool skipNextProcessing
    )
    {
        this.sourcePolicyName = sourcePolicyName;
        this.parameter = parameter;
        this.amount = amount;
        this.remainingDelay = Mathf.Max(0, remainingDelay);
        this.remainingApplications = Mathf.Max(0, remainingApplications);
        this.skipNextProcessing = skipNextProcessing;
    }


    /// <summary>
    /// Procesa un turno del efecto.
    /// Devuelve true cuando el efecto debe aplicarse en este turno.
    /// </summary>
    public bool ProcessTurn(
        out CountryParameter affectedParameter,
        out float appliedAmount
    )
    {
        affectedParameter = parameter;
        appliedAmount = amount;

        if (IsCompleted)
        {
            return false;
        }

        /*
         * Evita que un efecto inmediato y recurrente se aplique dos veces
         * durante el mismo clic: una en ApplyPolicy y otra en AdvanceTurn.
         */
        if (skipNextProcessing)
        {
            skipNextProcessing = false;
            return false;
        }

        if (remainingDelay > 0)
        {
            remainingDelay--;

            if (remainingDelay > 0)
            {
                return false;
            }
        }

        remainingApplications--;

        return true;
    }
}