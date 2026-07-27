using System;
using UnityEngine;

/// <summary>
/// Almacena el estado actual del país durante una partida.
///
/// Esta clase contiene datos de ejecución, por lo que se implementa como
/// MonoBehaviour. Las definiciones permanentes de políticas se almacenan
/// mediante ScriptableObjects.
/// </summary>
public class CountryState : MonoBehaviour
{
    [Header("Country information")]

    [SerializeField]
    private string countryName = "New Country";

    [SerializeField, Min(1)]
    private int currentTurn = 1;

    [SerializeField, Min(1)]
    private int maximumTurns = 12;


    [Header("Country parameters")]

    [SerializeField, Range(0f, 100f)]
    private float prosperity = 50f;

    [SerializeField, Range(0f, 100f)]
    private float equality = 50f;

    [SerializeField, Range(0f, 100f)]
    private float employment = 50f;

    [SerializeField, Range(0f, 100f)]
    private float education = 50f;

    [SerializeField, Range(0f, 100f)]
    private float stability = 50f;


    /// <summary>
    /// Se ejecuta siempre que cambia el estado del país.
    /// La interfaz puede suscribirse a este evento para actualizar los textos
    /// y barras sin consultar constantemente el estado.
    /// </summary>
    public event Action OnStateChanged;


    public string CountryName => countryName;

    public int CurrentTurn => currentTurn;

    public int MaximumTurns => maximumTurns;

    public bool HasReachedFinalTurn => currentTurn >= maximumTurns;

    public float Prosperity => prosperity;

    public float Equality => equality;

    public float Employment => employment;

    public float Education => education;

    public float Stability => stability;


    /// <summary>
    /// Devuelve el valor actual de un parámetro.
    /// </summary>
    public float GetParameter(CountryParameter parameter)
    {
        switch (parameter)
        {
            case CountryParameter.Prosperity:
                return prosperity;

            case CountryParameter.Equality:
                return equality;

            case CountryParameter.Employment:
                return employment;

            case CountryParameter.Education:
                return education;

            case CountryParameter.Stability:
                return stability;

            default:
                Debug.LogError($"Unknown country parameter: {parameter}");
                return 0f;
        }
    }


    /// <summary>
    /// Sustituye directamente el valor de un parámetro.
    /// El valor siempre se limita entre 0 y 100.
    /// </summary>
    public void SetParameter(CountryParameter parameter, float newValue)
    {
        float clampedValue = ClampParameter(newValue);

        switch (parameter)
        {
            case CountryParameter.Prosperity:
                prosperity = clampedValue;
                break;

            case CountryParameter.Equality:
                equality = clampedValue;
                break;

            case CountryParameter.Employment:
                employment = clampedValue;
                break;

            case CountryParameter.Education:
                education = clampedValue;
                break;

            case CountryParameter.Stability:
                stability = clampedValue;
                break;

            default:
                Debug.LogError($"Unknown country parameter: {parameter}");
                return;
        }

        NotifyStateChanged();
    }


    /// <summary>
    /// Suma o resta una cantidad al parámetro indicado.
    ///
    /// Ejemplos:
    /// ModifyParameter(CountryParameter.Education, 5);
    /// ModifyParameter(CountryParameter.Stability, -3);
    /// </summary>
    public void ModifyParameter(CountryParameter parameter, float amount)
    {
        float currentValue = GetParameter(parameter);
        SetParameter(parameter, currentValue + amount);
    }


    /// <summary>
    /// Aplica todos los efectos inmediatos de una política.
    /// </summary>
    public void ApplyPolicy(PolicyDefinitionSO policy)
    {
        if (policy == null)
        {
            Debug.LogError("Cannot apply a null policy.");
            return;
        }

        foreach (PolicyEffect effect in policy.Effects)
        {
            ModifyParameter(effect.Parameter, effect.Amount);
        }

        Debug.Log($"Policy applied: {policy.PolicyName}");
    }


    /// <summary>
    /// Avanza un turno, siempre que no se haya alcanzado el límite.
    /// </summary>
    public bool AdvanceTurn()
    {
        if (HasReachedFinalTurn)
        {
            Debug.LogWarning("The simulation has already reached its final turn.");
            return false;
        }

        currentTurn++;

        ApplyAutomaticRelations();
        NotifyStateChanged();

        return true;
    }


    /// <summary>
    /// Relaciones básicas que se procesan automáticamente al avanzar de turno.
    ///
    /// Son reglas provisionales para el primer prototipo. Más adelante deberían
    /// trasladarse a un sistema de reglas independiente.
    /// </summary>
    private void ApplyAutomaticRelations()
    {
        // Un empleo elevado impulsa gradualmente la prosperidad.
        if (employment >= 70f)
        {
            ModifyParameterWithoutNotification(
                CountryParameter.Prosperity,
                1f
            );
        }
        else if (employment <= 30f)
        {
            ModifyParameterWithoutNotification(
                CountryParameter.Prosperity,
                -1f
            );
        }

        // Una educación elevada mejora lentamente el empleo.
        if (education >= 70f)
        {
            ModifyParameterWithoutNotification(
                CountryParameter.Employment,
                0.5f
            );
        }

        // Una desigualdad elevada perjudica la estabilidad.
        if (equality <= 30f)
        {
            ModifyParameterWithoutNotification(
                CountryParameter.Stability,
                -2f
            );
        }

        // Una prosperidad muy baja perjudica la estabilidad.
        if (prosperity <= 30f)
        {
            ModifyParameterWithoutNotification(
                CountryParameter.Stability,
                -2f
            );
        }

        // Una prosperidad elevada beneficia ligeramente la estabilidad.
        if (prosperity >= 75f)
        {
            ModifyParameterWithoutNotification(
                CountryParameter.Stability,
                1f
            );
        }
    }


    /// <summary>
    /// Versión interna que modifica un valor sin ejecutar el evento.
    /// Evita actualizar la interfaz varias veces durante un mismo cálculo.
    /// </summary>
    private void ModifyParameterWithoutNotification(
        CountryParameter parameter,
        float amount
    )
    {
        float newValue = GetParameter(parameter) + amount;
        newValue = ClampParameter(newValue);

        switch (parameter)
        {
            case CountryParameter.Prosperity:
                prosperity = newValue;
                break;

            case CountryParameter.Equality:
                equality = newValue;
                break;

            case CountryParameter.Employment:
                employment = newValue;
                break;

            case CountryParameter.Education:
                education = newValue;
                break;

            case CountryParameter.Stability:
                stability = newValue;
                break;

            default:
                Debug.LogError($"Unknown country parameter: {parameter}");
                break;
        }
    }


    /// <summary>
    /// Reinicia el país utilizando los valores indicados.
    /// </summary>
    public void ResetState(
        float initialProsperity = 50f,
        float initialEquality = 50f,
        float initialEmployment = 50f,
        float initialEducation = 50f,
        float initialStability = 50f
    )
    {
        currentTurn = 1;

        prosperity = ClampParameter(initialProsperity);
        equality = ClampParameter(initialEquality);
        employment = ClampParameter(initialEmployment);
        education = ClampParameter(initialEducation);
        stability = ClampParameter(initialStability);

        NotifyStateChanged();
    }


    public void SetCountryName(string newCountryName)
    {
        if (string.IsNullOrWhiteSpace(newCountryName))
        {
            Debug.LogWarning("The country name cannot be empty.");
            return;
        }

        countryName = newCountryName.Trim();
        NotifyStateChanged();
    }


    private float ClampParameter(float value)
    {
        return Mathf.Clamp(value, 0f, 100f);
    }


    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }


#if UNITY_EDITOR

    /// <summary>
    /// Mantiene los valores válidos cuando se modifican desde el Inspector.
    /// </summary>
    private void OnValidate()
    {
        currentTurn = Mathf.Clamp(currentTurn, 1, maximumTurns);

        prosperity = ClampParameter(prosperity);
        equality = ClampParameter(equality);
        employment = ClampParameter(employment);
        education = ClampParameter(education);
        stability = ClampParameter(stability);
    }

#endif
}