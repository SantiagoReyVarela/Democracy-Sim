using System;
using System.Collections.Generic;
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
    private int maximumTurns = 48;


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




    [Header("Fiscal economy")]

    [SerializeField, Range(-100f, 100f)]
    private float fiscalResult = -10f;

    [SerializeField, Min(0f)]
    private float reserves = 20f;

    [SerializeField, Min(0f)]
    private float debt = 100f;

    [SerializeField, Range(0f, 100f)]
    private float inflation = 2f;

    [SerializeField, Range(0f, 100f)]
    private float debtPaymentPercentage = 50f;

    [SerializeField]
    private DeficitFinancingMode deficitFinancingMode =
        DeficitFinancingMode.Debt;

    [SerializeField, Min(0f)]
    private float inflationPerPrintedUnit = 0.1f;



    [Header("Last fiscal result")]

    [SerializeField]
    private float lastReservesUsed;

    [SerializeField]
    private float lastDebtPayment;

    [SerializeField]
    private float lastDebtIssued;

    [SerializeField]
    private float lastMoneyPrinted;

    [SerializeField, Min(1)]
    private int inflationEffectDuration = 10;

    [SerializeField, Min(1)]
    private int inflationRecoveryDuration = 10;

    [SerializeField, Range(0f, 100f)]
    private float normalInflation = 2f;

    [SerializeField]
    private float pendingInflation;

    [SerializeField]
    private int inflationPressureTurnsRemaining;

    [SerializeField]
    private int inflationRecoveryTurnsRemaining;


    [Header("History")] // historial de datos para la grafica

    [SerializeField]
    private List<CountrySnapshot> history = new List<CountrySnapshot>();
    public IReadOnlyList<CountrySnapshot> History => history;



    [Header("Active policy effects")]

    [SerializeField]
    private List<ActivePolicyEffect> activePolicyEffects = new List<ActivePolicyEffect>();
    public IReadOnlyList<ActivePolicyEffect> ActivePolicyEffects => activePolicyEffects;

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

    public float FiscalResult => fiscalResult;

    public float Reserves => reserves;

    public float Debt => debt;

    public float Inflation => inflation;

    public float DebtPaymentPercentage => debtPaymentPercentage;

    public DeficitFinancingMode DeficitFinancingMode => deficitFinancingMode;

    public float LastReservesUsed => lastReservesUsed;

    public float LastDebtPayment => lastDebtPayment;


    public float LastDebtIssued => lastDebtIssued;

    public float LastMoneyPrinted => lastMoneyPrinted;

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

            // FISCAL 

            case CountryParameter.FiscalResult:
                return fiscalResult;

            case CountryParameter.Reserves:
                return reserves;

            case CountryParameter.Debt:
                return debt;

            case CountryParameter.Inflation:
                return inflation;

            default:
                Debug.LogError($"Unknown country parameter: {parameter}");
                return 0f;
        }
    }


    /// <summary>
    /// Sustituye directamente el valor de un parámetro.
    /// El valor siempre se limita entre 0 y 100.
    /// </summary>
    public void SetParameter(
        CountryParameter parameter,
        float newValue
    )
    {
        switch (parameter)
        {
            case CountryParameter.Prosperity:
                prosperity = ClampParameter(newValue);
                break;

            case CountryParameter.Equality:
                equality = ClampParameter(newValue);
                break;

            case CountryParameter.Employment:
                employment = ClampParameter(newValue);
                break;

            case CountryParameter.Education:
                education = ClampParameter(newValue);
                break;

            case CountryParameter.Stability:
                stability = ClampParameter(newValue);
                break;

            // FISCAL

            case CountryParameter.FiscalResult:
            fiscalResult = Mathf.Clamp(newValue, -100f, 100f);
                break;

            case CountryParameter.Reserves:
                reserves = Mathf.Max(0f, newValue);
                break;

            case CountryParameter.Debt:
                debt = Mathf.Max(0f, newValue);
                break;

            case CountryParameter.Inflation:
                inflation = Mathf.Clamp(newValue, 0f, 100f);
                break;
            default:
                Debug.LogError(
                    $"Unknown country parameter: {parameter}"
                );
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
    /// Aplica los efectos inmediatos y registra los efectos
    /// retrasados o recurrentes de una política.
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
            /*
            * Duration 0 representa un efecto único.
            * Duration superior a 0 indica el número total de aplicaciones.
            */
            int totalApplications =
                effect.DurationInTurns > 0
                    ? effect.DurationInTurns
                    : 1;

            if (effect.DelayInTurns == 0)
            {
                // Primera aplicación inmediata.
                ModifyParameterWithoutNotification(
                    effect.Parameter,
                    effect.Amount
                );

                totalApplications--;

                /*
                * Si todavía quedan aplicaciones, guardamos el efecto.
                * skipNextProcessing evita aplicarlo otra vez durante
                * el AdvanceTurn que ocurre inmediatamente después.
                */
                if (totalApplications > 0)
                {
                    ActivePolicyEffect activeEffect =
                        new ActivePolicyEffect(
                            policy.PolicyName,
                            effect.Parameter,
                            effect.Amount,
                            remainingDelay: 0,
                            remainingApplications: totalApplications,
                            skipNextProcessing: true
                        );

                    activePolicyEffects.Add(activeEffect);
                }
            }
            else
            {
                // El efecto completo queda pendiente.
                ActivePolicyEffect delayedEffect =
                    new ActivePolicyEffect(
                        policy.PolicyName,
                        effect.Parameter,
                        effect.Amount,
                        effect.DelayInTurns,
                        totalApplications,
                        skipNextProcessing: true //si el efecto tiene delay, lo saltamos hasta que toque
                    );

                activePolicyEffects.Add(delayedEffect);
            }
        }

        Debug.Log($"Policy applied: {policy.PolicyName}");

        NotifyStateChanged();
    }


    /// <summary>
    /// Avanza un turno, siempre que no se haya alcanzado el límite.
    /// </summary>
    public bool AdvanceTurn()
    {
        if (HasReachedFinalTurn)
        {
            Debug.LogWarning(
                "The simulation has already reached its final turn."
            );

            return false;
        }

        currentTurn++;

        ProcessActivePolicyEffects();
        ApplyAutomaticRelations();
        ProcessFiscalEconomy();

        ProcessFiscalEconomy();
        ProcessInflation();

        RecordSnapshot();   
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
    /// Procesa todos los efectos retrasados y recurrentes
    /// correspondientes al nuevo turno.
    /// </summary>
    private void ProcessActivePolicyEffects()
    {
        for (int i = activePolicyEffects.Count - 1; i >= 0; i--)
        {
            ActivePolicyEffect activeEffect =
                activePolicyEffects[i];

            bool shouldApply = activeEffect.ProcessTurn(
                out CountryParameter parameter,
                out float amount
            );

            if (shouldApply)
            {
                ModifyParameterWithoutNotification(
                    parameter,
                    amount
                );

                Debug.Log(
                    $"Active effect applied | " +
                    $"Policy: {activeEffect.SourcePolicyName} | " +
                    $"Parameter: {parameter} | " +
                    $"Amount: {amount}"
                );
            }

            if (activeEffect.IsCompleted)
            {
                activePolicyEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Procesa el presupuesto público correspondiente al turno.
    /// </summary>
    private void ProcessFiscalEconomy()
    {
        lastReservesUsed = 0f;
        lastDebtPayment = 0f;
        lastDebtIssued = 0f;
        lastMoneyPrinted = 0f;

        if (fiscalResult >= 0f)
        {
            float surplus = fiscalResult;

            // Parte del superávit se utiliza para amortizar deuda.
            lastDebtPayment = Mathf.Min(
                debt,
                surplus * debtPaymentPercentage / 100f
            );

            debt = Mathf.Max(
                0f,
                debt - lastDebtPayment
            );

            // El superávit restante aumenta las reservas.
            float amountAddedToReserves =
                surplus - lastDebtPayment;

            reserves += amountAddedToReserves;
        }
        else
        {
            float deficit = -fiscalResult;

            // Primero se utilizan las reservas disponibles.
            lastReservesUsed = Mathf.Min(
                reserves,
                deficit
            );

            reserves -= lastReservesUsed;

            float remainingDeficit =
                deficit - lastReservesUsed;

            if (remainingDeficit > 0f)
            {
                switch (deficitFinancingMode)
                {
                    case DeficitFinancingMode.Debt:

                        lastDebtIssued = remainingDeficit;
                        debt += remainingDeficit;

                        break;

                    case DeficitFinancingMode.MoneyPrinting:

                        lastMoneyPrinted = remainingDeficit;

                        RegisterMoneyPrintingInflation(
                            lastMoneyPrinted
                        );

                        break;
                }
            }
        }

        Debug.Log(
            $"Fiscal result: {fiscalResult:0.##} | " +
            $"Reserves used: {lastReservesUsed:0.##} | " +
            $"Debt payment: {lastDebtPayment:0.##} | " +
            $"Debt issued: {lastDebtIssued:0.##} | " +
            $"Money printed: {lastMoneyPrinted:0.##} | " +
            $"Reserves: {reserves:0.##} | " +
            $"Debt: {debt:0.##} | " +
            $"Inflation: {inflation:0.##}%"
        );
    }

    private void RegisterMoneyPrintingInflation(float moneyPrinted)
    {
        if (moneyPrinted <= 0f)
        {
            return;
        }

        pendingInflation +=
            moneyPrinted * inflationPerPrintedUnit;

        // La presión se reparte durante los próximos 10 turnos.
        inflationPressureTurnsRemaining =
            inflationEffectDuration;

        // Una nueva emisión interrumpe la recuperación.
        inflationRecoveryTurnsRemaining = 0;
    }
    private void ProcessInflation()
    {
        if (
            inflationPressureTurnsRemaining > 0 &&
            pendingInflation > 0f
        )
        {
            float increase =
                pendingInflation /
                inflationPressureTurnsRemaining;

            inflation += increase;
            pendingInflation -= increase;
            inflationPressureTurnsRemaining--;

            inflationRecoveryTurnsRemaining = 0;
        }
        else if (inflation > normalInflation)
        {
            if (inflationRecoveryTurnsRemaining <= 0)
            {
                inflationRecoveryTurnsRemaining =
                    inflationRecoveryDuration;
            }

            float decrease =
                (inflation - normalInflation) /
                inflationRecoveryTurnsRemaining;

            inflation -= decrease;
            inflationRecoveryTurnsRemaining--;
        }

        inflation = Mathf.Clamp(
            inflation,
            normalInflation,
            100f
        );
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
        switch (parameter)
        {
            case CountryParameter.Prosperity:
                prosperity = ClampParameter(prosperity + amount);
                break;

            case CountryParameter.Equality:
                equality = ClampParameter(equality + amount);
                break;

            case CountryParameter.Employment:
                employment = ClampParameter(employment + amount);
                break;

            case CountryParameter.Education:
                education = ClampParameter(education + amount);
                break;

            case CountryParameter.Stability:
                stability = ClampParameter(stability + amount);
                break;

            case CountryParameter.FiscalResult:
                fiscalResult = Mathf.Clamp(
                    fiscalResult + amount,
                    -100f,
                    100f
                );
                break;

            case CountryParameter.Reserves:
                reserves = Mathf.Max(0f, reserves + amount);
                break;

            case CountryParameter.Debt:
                debt = Mathf.Max(0f, debt + amount);
                break;

            case CountryParameter.Inflation:
                inflation = Mathf.Clamp(
                    inflation + amount,
                    0f,
                    100f
                );
                break;

            default:
                Debug.LogError(
                    $"Unknown country parameter: {parameter}"
                );
                break;
        }
    }

    public void SetDebtPaymentPercentage(float percentage)
    {
        debtPaymentPercentage = Mathf.Clamp(
            percentage,
            0f,
            100f
        );

        NotifyStateChanged();
    }
    public void SetDeficitFinancingMode(
        DeficitFinancingMode financingMode
    )
    {
        deficitFinancingMode = financingMode;

        NotifyStateChanged();
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
        
        pendingInflation = 0f;
        inflationPressureTurnsRemaining = 0;
        inflationRecoveryTurnsRemaining = 0;

        activePolicyEffects.Clear();
        history.Clear();
        RecordSnapshot();

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

    private void Awake()
    {
        pendingInflation = 0f;
        inflationPressureTurnsRemaining = 0;
        inflationRecoveryTurnsRemaining = 0;

        activePolicyEffects.Clear();
        history.Clear();
        RecordSnapshot();
    }

    /// <summary>
    /// Guarda una copia del estado actual del país.
    /// </summary>
    private void RecordSnapshot()
    {
        CountrySnapshot snapshot = new CountrySnapshot(
            currentTurn,
            prosperity,
            equality,
            employment,
            education,
            stability,
            fiscalResult,
            reserves,
            debt,
            inflation
        );

        if (
            history.Count > 0 &&
            history[history.Count - 1].Turn == currentTurn
        )
        {
            history[history.Count - 1] = snapshot;
        }
        else
        {
            history.Add(snapshot);
        }
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

        fiscalResult = Mathf.Clamp(
            fiscalResult,
            -100f,
            100f
        );

        reserves = Mathf.Max(0f, reserves);
        debt = Mathf.Max(0f, debt);
        inflation = Mathf.Clamp(inflation, 0f, 100f);

        debtPaymentPercentage = Mathf.Clamp(
            debtPaymentPercentage,
            0f,
            100f
        );
    }

#endif
}