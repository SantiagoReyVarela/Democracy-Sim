using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Representa una modificación concreta sobre un parámetro del país.
///
/// El funcionamiento depende de la lista de PolicyDefinitionSO
/// en la que esté incluido:
///
/// - FirstImpactEffects: se aplica una sola vez.
/// - ActiveEffects: permanece mientras la política esté activa.
/// </summary>
[Serializable]
public sealed class PolicyEffect
{
    [SerializeField]
    private CountryParameter parameter;

    [Tooltip(
        "Cantidad fija que se suma o resta al parámetro. " +
        "Ejemplo: 500000 suma 500.000; -3 resta 3 puntos."
    )]
    [FormerlySerializedAs("amount")]
    [SerializeField]
    private float absoluteAmount;

    [Tooltip(
        "Porcentaje del valor actual que se suma o resta. " +
        "Ejemplo: 5 aumenta un 5%; -10 reduce un 10%. " +
        "Puede combinarse con Absolute Amount."
    )]
    [SerializeField]
    private float percentageModifier;


    public CountryParameter Parameter => parameter;

    public float AbsoluteAmount => absoluteAmount;

    public float PercentageModifier => percentageModifier;

    public bool HasAbsoluteEffect =>
        !Mathf.Approximately(absoluteAmount, 0f);

    public bool HasPercentageEffect =>
        !Mathf.Approximately(percentageModifier, 0f);


    /// <summary>
    /// Calcula cuánto cambia realmente el parámetro partiendo de
    /// su valor actual.
    ///
    /// Ejemplo:
    /// valor actual = 1.000.000
    /// porcentaje = +5
    /// absoluto = +100.000
    /// cambio total = +150.000
    /// resultado = 1.150.000
    /// </summary>
    public float CalculateDelta(float currentValue)
    {
        float percentageDelta =
            currentValue *
            (percentageModifier / 100f);

        return percentageDelta + absoluteAmount;
    }


    /// <summary>
    /// Calcula el valor final después de aplicar simultáneamente
    /// el porcentaje y la cantidad absoluta.
    /// </summary>
    public float CalculateFinalValue(float currentValue)
    {
        return currentValue + CalculateDelta(currentValue);
    }


    /*
     * Compatibilidad temporal con scripts antiguos que todavía
     * consulten effect.Amount. Devuelve únicamente la parte absoluta,
     * porque un porcentaje necesita conocer el valor actual para poder
     * convertirse en una cantidad real.
     */
    [Obsolete(
        "Utiliza AbsoluteAmount, PercentageModifier o CalculateDelta."
    )]
    public float Amount => absoluteAmount;


    /*
     * Propiedades temporales para que el código antiguo
     * pueda continuar compilando durante la migración.
     */

    [Obsolete(
        "El nuevo sistema de políticas no utiliza retrasos."
    )]
    public int DelayInTurns => 0;

    [Obsolete(
        "El nuevo sistema de políticas no utiliza duraciones."
    )]
    public int DurationInTurns => 0;
}


/// <summary>
/// Contiene la definición permanente de una política.
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

    [Header("Political reaction")]

    [Tooltip(
        "Determina cuánto cambia la satisfacción de los partidos " +
        "cuando se introduce esta política."
    )]
    [SerializeField, Range(0f, 20f)]
    private float politicalRelevance = 10f;


    [Header("Ideological classification")]

    [SerializeField]
    private PoliticalAxis primaryAxis =
        PoliticalAxis.Economy;

    [SerializeField]
    private List<PolicyAxisPosition> ideologicalProfile =
        new List<PolicyAxisPosition>();


    [Header("Policy effects")]

    [Tooltip(
        "Efectos aplicados una sola vez cuando " +
        "la política es seleccionada."
    )]
    [FormerlySerializedAs("effects")]
    [SerializeField]
    private List<PolicyEffect> firstImpactEffects =
        new List<PolicyEffect>();

    [Tooltip(
        "Modificadores que existen mientras " +
        "la política permanece activa."
    )]
    [SerializeField]
    private List<PolicyEffect> activeEffects =
        new List<PolicyEffect>();


    public string PolicyId => policyId;

    public string PolicyName => policyName;

    public string Description => description;

    public float PoliticalCost => politicalCost;

    public float PoliticalRelevance => politicalRelevance;

    public PoliticalAxis PrimaryAxis => primaryAxis;

    public IReadOnlyList<PolicyAxisPosition> IdeologicalProfile =>
        ideologicalProfile;

    public IReadOnlyList<PolicyEffect> FirstImpactEffects =>
        firstImpactEffects;

    public IReadOnlyList<PolicyEffect> ActiveEffects =>
        activeEffects;


    /*
     * Propiedad temporal para mantener compatible
     * cualquier script que todavía utilice policy.Effects.
     */
    [Obsolete(
        "Utiliza FirstImpactEffects o ActiveEffects."
    )]
    public IReadOnlyList<PolicyEffect> Effects =>
        firstImpactEffects;


    /// <summary>
    /// Busca la posición de esta política en un eje ideológico.
    /// </summary>
    public bool TryGetAxisPosition(
        PoliticalAxis axis,
        out PolicyAxisPosition axisPosition
    )
    {
        if (ideologicalProfile != null)
        {
            foreach (
                PolicyAxisPosition profileEntry
                in ideologicalProfile
            )
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
        }

        axisPosition = null;
        return false;
    }


    /// <summary>
    /// Método antiguo conservado temporalmente.
    /// </summary>
    [Obsolete(
        "Utiliza GetTotalFirstImpactEffectOnParameter " +
        "o GetTotalActiveEffectOnParameter."
    )]
    public float GetTotalEffectOnParameter(
        CountryParameter parameter
    )
    {
        return GetTotalFirstImpactEffectOnParameter(
            parameter
        );
    }


    /// <summary>
    /// Suma las cantidades absolutas de primer impacto
    /// que afectan al parámetro indicado.
    /// Los porcentajes se consultan por separado porque necesitan
    /// conocer el valor actual del parámetro.
    /// </summary>
    public float GetTotalFirstImpactEffectOnParameter(
        CountryParameter parameter
    )
    {
        if (firstImpactEffects == null)
        {
            return 0f;
        }

        float totalEffect = 0f;

        foreach (
            PolicyEffect effect
            in firstImpactEffects
        )
        {
            if (
                effect != null &&
                effect.Parameter == parameter
            )
            {
                totalEffect += effect.AbsoluteAmount;
            }
        }

        return totalEffect;
    }


    /// <summary>
    /// Suma las cantidades absolutas activas
    /// que afectan al parámetro indicado.
    /// Los porcentajes se consultan por separado porque necesitan
    /// conocer el valor actual del parámetro.
    /// </summary>
    public float GetTotalActiveEffectOnParameter(
        CountryParameter parameter
    )
    {
        if (activeEffects == null)
        {
            return 0f;
        }

        float totalEffect = 0f;

        foreach (
            PolicyEffect effect
            in activeEffects
        )
        {
            if (
                effect != null &&
                effect.Parameter == parameter
            )
            {
                totalEffect += effect.AbsoluteAmount;
            }
        }

        return totalEffect;
    }

    /// <summary>
    /// Suma los porcentajes de primer impacto configurados para
    /// el parámetro indicado. El valor 5 representa +5%.
    /// </summary>
    public float GetTotalFirstImpactPercentageOnParameter(
        CountryParameter parameter
    )
    {
        if (firstImpactEffects == null)
        {
            return 0f;
        }

        float totalPercentage = 0f;

        foreach (
            PolicyEffect effect
            in firstImpactEffects
        )
        {
            if (
                effect != null &&
                effect.Parameter == parameter
            )
            {
                totalPercentage += effect.PercentageModifier;
            }
        }

        return totalPercentage;
    }


    /// <summary>
    /// Suma los porcentajes activos configurados para
    /// el parámetro indicado. El valor 5 representa +5% por turno.
    /// </summary>
    public float GetTotalActivePercentageOnParameter(
        CountryParameter parameter
    )
    {
        if (activeEffects == null)
        {
            return 0f;
        }

        float totalPercentage = 0f;

        foreach (
            PolicyEffect effect
            in activeEffects
        )
        {
            if (
                effect != null &&
                effect.Parameter == parameter
            )
            {
                totalPercentage += effect.PercentageModifier;
            }
        }

        return totalPercentage;
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

        if (ideologicalProfile == null)
        {
            ideologicalProfile =
                new List<PolicyAxisPosition>();
        }

        if (firstImpactEffects == null)
        {
            firstImpactEffects =
                new List<PolicyEffect>();
        }

        if (activeEffects == null)
        {
            activeEffects =
                new List<PolicyEffect>();
        }

        ideologicalProfile.RemoveAll(
            profileEntry => profileEntry == null
        );

        firstImpactEffects.RemoveAll(
            effect => effect == null
        );

        activeEffects.RemoveAll(
            effect => effect == null
        );
    }

#endif
}