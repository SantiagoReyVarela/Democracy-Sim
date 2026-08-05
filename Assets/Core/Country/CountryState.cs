using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Almacena el estado actual del país durante una partida.
///
/// Funcionamiento de las políticas:
/// - FirstImpactEffects: se aplican una sola vez al colocar la política.
/// - ActiveEffects: se aplican una vez en cada turno mientras la política
///   continúe ocupando un slot.
/// </summary>
public class CountryState : MonoBehaviour
{
    [Serializable]
    public sealed class PolicySlotChangeRecord
    {
        [SerializeField]
        private int turn;

        [SerializeField]
        private string slotId;

        [SerializeField]
        private string slotDisplayName;

        [SerializeField]
        private PolicyDefinitionSO previousPolicy;

        [SerializeField]
        private PolicyDefinitionSO newPolicy;

        public int Turn => turn;
        public string SlotId => slotId;
        public string SlotDisplayName => slotDisplayName;
        public PolicyDefinitionSO PreviousPolicy => previousPolicy;
        public PolicyDefinitionSO NewPolicy => newPolicy;

        public PolicySlotChangeRecord(
            int turn,
            PolicySlotState slot,
            PolicyDefinitionSO previousPolicy,
            PolicyDefinitionSO newPolicy
        )
        {
            this.turn = turn;
            slotId = slot != null ? slot.SlotId : string.Empty;
            slotDisplayName = slot != null ? slot.DisplayName : string.Empty;
            this.previousPolicy = previousPolicy;
            this.newPolicy = newPolicy;
        }
    }


    public static CountryState Instance { get; private set; }


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


    [Header("Permanent Policy Slots")]

    [SerializeField]
    private List<PolicySlotState> policySlots =
        new List<PolicySlotState>();


    [Header("Fiscal economy")]

    [SerializeField, Range(-100f, 100f)]
    private float fiscalResult = -10f;

    [SerializeField, Min(0f)]
    private float reserves = 20f;

    [SerializeField, Min(0f)]
    private float debt = 100f;

    [SerializeField, Range(-10f, 100f)]
    private float inflation = 2f;

    [SerializeField]
    private float minimumInflation = -10f;

    [SerializeField, Min(0f)]
    private float disinflationPerReserveUnit = 0.03f;

    [SerializeField, Min(0f)]
    private float highInflationMultiplier = 2f;

    [SerializeField, Min(0f)]
    private float highInflationThreshold = 5f;

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


    [Header("History")]

    [SerializeField]
    private List<CountrySnapshot> history =
        new List<CountrySnapshot>();


    [Header("Applied policy history")]

    [SerializeField]
    private List<AppliedPolicyRecord> appliedPolicyHistory =
        new List<AppliedPolicyRecord>();

    [SerializeField]
    private List<PolicySlotChangeRecord> policySlotChangeHistory =
        new List<PolicySlotChangeRecord>();


    [Header("Parameter relations")]

    [SerializeField]
    private List<ParameterRelationSO> parameterRelations =
        new List<ParameterRelationSO>();


    /// <summary>
    /// Se ejecuta siempre que cambia el estado del país.
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

    public DeficitFinancingMode DeficitFinancingMode =>
        deficitFinancingMode;

    public float LastReservesUsed => lastReservesUsed;
    public float LastDebtPayment => lastDebtPayment;
    public float LastDebtIssued => lastDebtIssued;
    public float LastMoneyPrinted => lastMoneyPrinted;

    public IReadOnlyList<CountrySnapshot> History => history;

    public IReadOnlyList<AppliedPolicyRecord> AppliedPolicyHistory =>
        appliedPolicyHistory;

    public IReadOnlyList<PolicySlotChangeRecord> PolicySlotChangeHistory =>
        policySlotChangeHistory;

    public IReadOnlyList<PolicySlotState> PolicySlots => policySlots;


    /// <summary>
    /// Devuelve el valor actual almacenado del parámetro.
    /// Los efectos activos ya se incorporan al valor cuando avanza el turno.
    /// </summary>
    public float GetBaseParameterValue(
        CountryParameter parameter
    )
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

            case CountryParameter.FiscalResult:
                return fiscalResult;

            case CountryParameter.Reserves:
                return reserves;

            case CountryParameter.Debt:
                return debt;

            case CountryParameter.Inflation:
                return inflation;

            default:
                Debug.LogError(
                    $"Unknown country parameter: {parameter}"
                );
                return 0f;
        }
    }


    /// <summary>
    /// Método conservado por compatibilidad con la interfaz existente.
    /// Ya no añade modificadores temporales: devuelve el valor almacenado.
    /// </summary>
    public float GetEffectiveParameterValue(
        CountryParameter parameter
    )
    {
        return GetBaseParameterValue(parameter);
    }


    public float GetParameter(
        CountryParameter parameter
    )
    {
        return GetBaseParameterValue(parameter);
    }


    public void SetParameter(
        CountryParameter parameter,
        float newValue
    )
    {
        SetParameterWithoutNotification(
            parameter,
            newValue
        );

        NotifyStateChanged();
    }


    public void ModifyParameter(
        CountryParameter parameter,
        float amount
    )
    {
        ModifyParameterWithoutNotification(
            parameter,
            amount
        );

        NotifyStateChanged();
    }


    /// <summary>
    /// Método de compatibilidad con interfaces antiguas.
    /// Asigna la política al primer slot compatible vacío.
    /// </summary>
    public void ApplyPolicy(
        PolicyDefinitionSO policy
    )
    {
        if (policy == null)
        {
            Debug.LogError(
                "Cannot apply a null policy."
            );
            return;
        }

        PolicySlotState compatibleSlot =
            FindCompatiblePolicySlot(
                policy,
                preferEmptySlot: true
            );

        if (compatibleSlot == null)
        {
            Debug.LogError(
                $"No existe ningún espacio compatible con " +
                $"{policy.PolicyName} ({policy.PrimaryAxis})."
            );
            return;
        }

        AssignPolicyToSlot(
            compatibleSlot.SlotId,
            policy
        );
    }


    /// <summary>
    /// Avanza un turno.
    ///
    /// Orden:
    /// 1. Se aplican una vez los ActiveEffects de cada política activa.
    /// 2. Se procesa el presupuesto con el resultado fiscal actualizado.
    /// 3. Se procesa la inflación.
    /// 4. Se aplican las relaciones configuradas.
    /// 5. Se guarda el historial del turno.
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

        ProcessActivePoliciesPerTurn();
        ProcessFiscalEconomy();
        ProcessInflation();
        ApplyConfiguredRelations();

        RecordSnapshot();
        NotifyStateChanged();

        return true;
    }


    /// <summary>
    /// Aplica una vez por turno los ActiveEffects de todas las
    /// políticas que continúan dentro de un slot.
    /// </summary>
    private void ProcessActivePoliciesPerTurn()
    {
        if (policySlots == null)
        {
            return;
        }

        foreach (PolicySlotState slot in policySlots)
        {
            if (
                slot == null ||
                slot.ActivePolicy == null ||
                slot.ActivePolicy.ActiveEffects == null
            )
            {
                continue;
            }

            foreach (
                PolicyEffect effect
                in slot.ActivePolicy.ActiveEffects
            )
            {
                if (effect == null)
                {
                    continue;
                }

                ModifyParameterWithoutNotification(
                    effect.Parameter,
                    effect.Amount
                );

                Debug.Log(
                    $"Active policy effect | " +
                    $"Policy: {slot.ActivePolicy.PolicyName} | " +
                    $"Parameter: {effect.Parameter} | " +
                    $"Amount: {effect.Amount:+0.##;-0.##;0}"
                );
            }
        }
    }


    /// <summary>
    /// Procesa el presupuesto público del turno utilizando el
    /// resultado fiscal que ya incluye los efectos activos aplicados.
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

            lastDebtPayment = Mathf.Min(
                debt,
                surplus * debtPaymentPercentage / 100f
            );

            debt = Mathf.Max(
                0f,
                debt - lastDebtPayment
            );

            float amountAddedToReserves =
                surplus - lastDebtPayment;

            reserves += amountAddedToReserves;
        }
        else
        {
            float deficit = -fiscalResult;

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


    private void RegisterMoneyPrintingInflation(
        float moneyPrinted
    )
    {
        if (moneyPrinted <= 0f)
        {
            return;
        }

        pendingInflation +=
            moneyPrinted * inflationPerPrintedUnit;

        inflationPressureTurnsRemaining =
            inflationEffectDuration;

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

        float reservesAddedThisTurn = 0f;

        if (fiscalResult > 0f)
        {
            reservesAddedThisTurn = Mathf.Max(
                0f,
                fiscalResult - lastDebtPayment
            );
        }

        if (reservesAddedThisTurn > 0f)
        {
            float multiplier =
                inflation >= highInflationThreshold
                    ? highInflationMultiplier
                    : 1f;

            float fiscalCooling =
                reservesAddedThisTurn *
                disinflationPerReserveUnit *
                multiplier;

            inflation -= fiscalCooling;
        }

        inflation = Mathf.Clamp(
            inflation,
            minimumInflation,
            100f
        );
    }


    private void ModifyParameterWithoutNotification(
        CountryParameter parameter,
        float amount
    )
    {
        float currentValue =
            GetBaseParameterValue(parameter);

        SetParameterWithoutNotification(
            parameter,
            currentValue + amount
        );
    }


    private void SetParameterWithoutNotification(
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

            case CountryParameter.FiscalResult:
                fiscalResult = Mathf.Clamp(
                    newValue,
                    -100f,
                    100f
                );
                break;

            case CountryParameter.Reserves:
                reserves = Mathf.Max(
                    0f,
                    newValue
                );
                break;

            case CountryParameter.Debt:
                debt = Mathf.Max(
                    0f,
                    newValue
                );
                break;

            case CountryParameter.Inflation:
                inflation = Mathf.Clamp(
                    newValue,
                    minimumInflation,
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


    public void SetDebtPaymentPercentage(
        float percentage
    )
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


    public void ResetState(
        float initialProsperity = 50f,
        float initialEquality = 50f,
        float initialEmployment = 50f,
        float initialEducation = 50f,
        float initialStability = 50f,
        float initialFiscalResult = -10f,
        float initialReserves = 20f,
        float initialDebt = 100f,
        float initialInflation = 2f
    )
    {
        currentTurn = 1;

        prosperity = ClampParameter(initialProsperity);
        equality = ClampParameter(initialEquality);
        employment = ClampParameter(initialEmployment);
        education = ClampParameter(initialEducation);
        stability = ClampParameter(initialStability);

        fiscalResult = Mathf.Clamp(
            initialFiscalResult,
            -100f,
            100f
        );

        reserves = Mathf.Max(
            0f,
            initialReserves
        );

        debt = Mathf.Max(
            0f,
            initialDebt
        );

        inflation = Mathf.Clamp(
            initialInflation,
            minimumInflation,
            100f
        );

        lastReservesUsed = 0f;
        lastDebtPayment = 0f;
        lastDebtIssued = 0f;
        lastMoneyPrinted = 0f;

        pendingInflation = 0f;
        inflationPressureTurnsRemaining = 0;
        inflationRecoveryTurnsRemaining = 0;

        ClearActivePolicies();

        appliedPolicyHistory.Clear();
        policySlotChangeHistory.Clear();

        history.Clear();
        RecordSnapshot();

        NotifyStateChanged();
    }


    public void SetCountryName(
        string newCountryName
    )
    {
        if (string.IsNullOrWhiteSpace(newCountryName))
        {
            Debug.LogWarning(
                "The country name cannot be empty."
            );
            return;
        }

        countryName = newCountryName.Trim();
        NotifyStateChanged();
    }


    private float ClampParameter(
        float value
    )
    {
        return Mathf.Clamp(
            value,
            0f,
            100f
        );
    }


    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ClearActivePolicies();

        appliedPolicyHistory.Clear();
        policySlotChangeHistory.Clear();

        pendingInflation = 0f;
        inflationPressureTurnsRemaining = 0;
        inflationRecoveryTurnsRemaining = 0;

        history.Clear();
        RecordSnapshot();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    /// <summary>
    /// Guarda los valores actuales después de procesar el turno.
    /// </summary>
    private void RecordSnapshot()
    {
        CountrySnapshot snapshot =
            new CountrySnapshot(
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


    /// <summary>
    /// Las relaciones leen los valores actuales y modifican el estado
    /// una vez por turno.
    /// </summary>
    private void ApplyConfiguredRelations()
    {
        Dictionary<CountryParameter, float> sourceValues =
            new Dictionary<CountryParameter, float>();

        foreach (ParameterRelationSO relation in parameterRelations)
        {
            if (relation == null || !relation.IsEnabled)
            {
                continue;
            }

            CountryParameter source =
                relation.SourceParameter;

            if (!sourceValues.ContainsKey(source))
            {
                sourceValues.Add(
                    source,
                    GetParameter(source)
                );
            }
        }

        foreach (ParameterRelationSO relation in parameterRelations)
        {
            if (relation == null || !relation.IsEnabled)
            {
                continue;
            }

            float sourceValue =
                sourceValues[relation.SourceParameter];

            float impact =
                relation.Evaluate(sourceValue);

            if (Mathf.Approximately(impact, 0f))
            {
                continue;
            }

            ModifyParameterWithoutNotification(
                relation.TargetParameter,
                impact
            );
        }
    }


    private void RecordAppliedPolicy(
        PolicyDefinitionSO policy
    )
    {
        appliedPolicyHistory.Add(
            new AppliedPolicyRecord(
                policy,
                currentTurn
            )
        );
    }


    private void RecordPolicySlotChange(
        PolicySlotState slot,
        PolicyDefinitionSO previousPolicy,
        PolicyDefinitionSO newPolicy
    )
    {
        policySlotChangeHistory.Add(
            new PolicySlotChangeRecord(
                currentTurn,
                slot,
                previousPolicy,
                newPolicy
            )
        );
    }


    private void ClearActivePolicies()
    {
        if (policySlots == null)
        {
            return;
        }

        foreach (PolicySlotState slot in policySlots)
        {
            slot?.Clear();
        }
    }


    public PolicySlotState GetPolicySlot(
        string slotId
    )
    {
        if (
            string.IsNullOrWhiteSpace(slotId) ||
            policySlots == null
        )
        {
            return null;
        }

        foreach (PolicySlotState slot in policySlots)
        {
            if (
                slot != null &&
                string.Equals(
                    slot.SlotId,
                    slotId,
                    StringComparison.Ordinal
                )
            )
            {
                return slot;
            }
        }

        return null;
    }


    public bool IsPolicyActive(
        PolicyDefinitionSO policy
    )
    {
        if (policy == null || policySlots == null)
        {
            return false;
        }

        foreach (PolicySlotState slot in policySlots)
        {
            if (
                slot != null &&
                slot.ActivePolicy == policy
            )
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Coloca o sustituye una política sin avanzar el turno.
    /// Solo aplica en ese momento sus FirstImpactEffects.
    /// </summary>
    public bool AssignPolicyToSlot(
        string slotId,
        PolicyDefinitionSO policy
    )
    {
        if (policy == null)
        {
            Debug.LogError(
                "No se puede asignar una política nula."
            );
            return false;
        }

        PolicySlotState targetSlot =
            GetPolicySlot(slotId);

        if (targetSlot == null)
        {
            Debug.LogError(
                $"No existe el espacio de política '{slotId}'."
            );
            return false;
        }

        if (policy.PrimaryAxis != targetSlot.Axis)
        {
            Debug.LogWarning(
                $"{policy.PolicyName} pertenece al eje " +
                $"{policy.PrimaryAxis}, pero el espacio " +
                $"{targetSlot.DisplayName} pertenece a " +
                $"{targetSlot.Axis}."
            );
            return false;
        }

        foreach (PolicySlotState slot in policySlots)
        {
            if (
                slot != null &&
                slot != targetSlot &&
                slot.ActivePolicy == policy
            )
            {
                Debug.LogWarning(
                    $"{policy.PolicyName} ya está activa en " +
                    $"{slot.DisplayName}."
                );
                return false;
            }
        }

        if (targetSlot.ActivePolicy == policy)
        {
            return true;
        }

        PolicyDefinitionSO previousPolicy =
            targetSlot.ActivePolicy;

        targetSlot.SetPolicy(policy);

        ApplyFirstImpactEffects(policy);
        RecordAppliedPolicy(policy);

        RecordPolicySlotChange(
            targetSlot,
            previousPolicy,
            policy
        );

        Debug.Log(
            $"Policy selected | Slot: {targetSlot.DisplayName} | " +
            $"Previous: " +
            $"{(previousPolicy != null ? previousPolicy.PolicyName : "None")} | " +
            $"New: {policy.PolicyName}"
        );

        NotifyStateChanged();

        return true;
    }


    /// <summary>
    /// Retira una política. Desde el siguiente turno sus ActiveEffects
    /// dejan de ejecutarse.
    /// </summary>
    public bool RemovePolicyFromSlot(
        string slotId
    )
    {
        PolicySlotState slot =
            GetPolicySlot(slotId);

        if (slot == null || slot.IsEmpty)
        {
            return false;
        }

        PolicyDefinitionSO previousPolicy =
            slot.ActivePolicy;

        slot.Clear();

        RecordPolicySlotChange(
            slot,
            previousPolicy,
            null
        );

        Debug.Log(
            $"Policy removed | Slot: {slot.DisplayName} | " +
            $"Policy: {previousPolicy.PolicyName}"
        );

        NotifyStateChanged();

        return true;
    }


    private void ApplyFirstImpactEffects(
        PolicyDefinitionSO policy
    )
    {
        if (
            policy == null ||
            policy.FirstImpactEffects == null
        )
        {
            return;
        }

        foreach (
            PolicyEffect effect
            in policy.FirstImpactEffects
        )
        {
            if (effect == null)
            {
                continue;
            }

            ModifyParameterWithoutNotification(
                effect.Parameter,
                effect.Amount
            );
        }
    }


    /// <summary>
    /// Devuelve la cantidad total que las políticas activas aplicarán
    /// al parámetro en el siguiente turno.
    ///
    /// Se conserva el nombre por compatibilidad con código anterior.
    /// Ya no es un modificador visual permanente.
    /// </summary>
    public float GetActivePolicyModifier(
        CountryParameter parameter
    )
    {
        if (policySlots == null)
        {
            return 0f;
        }

        float totalPerTurnEffect = 0f;

        foreach (PolicySlotState slot in policySlots)
        {
            if (
                slot == null ||
                slot.ActivePolicy == null
            )
            {
                continue;
            }

            totalPerTurnEffect +=
                slot.ActivePolicy
                    .GetTotalActiveEffectOnParameter(
                        parameter
                    );
        }

        return totalPerTurnEffect;
    }


    private PolicySlotState FindCompatiblePolicySlot(
        PolicyDefinitionSO policy,
        bool preferEmptySlot
    )
    {
        if (policy == null || policySlots == null)
        {
            return null;
        }

        PolicySlotState firstCompatibleSlot = null;

        foreach (PolicySlotState slot in policySlots)
        {
            if (
                slot == null ||
                slot.Axis != policy.PrimaryAxis
            )
            {
                continue;
            }

            if (slot.ActivePolicy == policy)
            {
                return slot;
            }

            if (firstCompatibleSlot == null)
            {
                firstCompatibleSlot = slot;
            }

            if (preferEmptySlot && slot.IsEmpty)
            {
                return slot;
            }
        }

        return firstCompatibleSlot;
    }


    [ContextMenu("Debug Active Policy Effects Per Turn")]
    private void DebugActivePolicyEffectsPerTurn()
    {
        foreach (
            CountryParameter parameter
            in Enum.GetValues(typeof(CountryParameter))
        )
        {
            float perTurnEffect =
                GetActivePolicyModifier(parameter);

            if (!Mathf.Approximately(perTurnEffect, 0f))
            {
                Debug.Log(
                    $"{parameter}: efecto activo por turno " +
                    $"{perTurnEffect:+0.##;-0.##;0}"
                );
            }
        }
    }


    [ContextMenu("Debug Current Values And Policy Effects")]
    private void DebugCurrentValuesAndPolicyEffects()
    {
        foreach (
            CountryParameter parameter
            in Enum.GetValues(typeof(CountryParameter))
        )
        {
            float currentValue =
                GetParameter(parameter);

            float perTurnEffect =
                GetActivePolicyModifier(parameter);

            Debug.Log(
                $"{parameter} | " +
                $"Current: {currentValue:0.##} | " +
                $"Active next turn: " +
                $"{perTurnEffect:+0.##;-0.##;0}"
            );
        }
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        maximumTurns = Mathf.Max(
            1,
            maximumTurns
        );

        currentTurn = Mathf.Clamp(
            currentTurn,
            1,
            maximumTurns
        );

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

        reserves = Mathf.Max(
            0f,
            reserves
        );

        debt = Mathf.Max(
            0f,
            debt
        );

        inflation = Mathf.Clamp(
            inflation,
            minimumInflation,
            100f
        );

        debtPaymentPercentage = Mathf.Clamp(
            debtPaymentPercentage,
            0f,
            100f
        );

        ValidatePolicySlots();
    }


    private void ValidatePolicySlots()
    {
        if (policySlots == null)
        {
            return;
        }

        HashSet<string> usedIds =
            new HashSet<string>();

        foreach (PolicySlotState slot in policySlots)
        {
            if (slot == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(slot.SlotId))
            {
                Debug.LogWarning(
                    "Hay un espacio de política sin Slot ID.",
                    this
                );
                continue;
            }

            if (!usedIds.Add(slot.SlotId))
            {
                Debug.LogWarning(
                    $"El Slot ID '{slot.SlotId}' está repetido.",
                    this
                );
            }
        }
    }

#endif
}