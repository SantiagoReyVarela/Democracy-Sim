using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conecta el estado de la simulación con la interfaz.
/// Muestra los parámetros del país y permite aplicar una política.
/// </summary>
public class CountryUIController : MonoBehaviour
{
    [Header("Simulation")]

    [SerializeField]
    private CountryState countryState;

    [SerializeField]
    private PolicyDefinitionSO selectedPolicy;


    [Header("Header")]

    [SerializeField]
    private TMP_Text countryNameText;

    [SerializeField]
    private TMP_Text turnText;


    [Header("Country parameters")]

    [SerializeField]
    private TMP_Text prosperityText;

    [SerializeField]
    private TMP_Text equalityText;

    [SerializeField]
    private TMP_Text employmentText;

    [SerializeField]
    private TMP_Text educationText;

    [SerializeField]
    private TMP_Text stabilityText;


    [Header("Policy")]

    [SerializeField]
    private TMP_Text policyNameText;

    [SerializeField]
    private TMP_Text policyDescriptionText;

    [SerializeField]
    private Button applyPolicyButton;

    [SerializeField]
    private TMP_Text feedbackText;


    private void Awake()
    {
        if (applyPolicyButton != null)
        {
            applyPolicyButton.onClick.AddListener(ApplySelectedPolicy);
        }
    }


    private void OnEnable()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged += RefreshUI;
        }
    }


    private void Start()
    {
        RefreshUI();
    }


    private void OnDisable()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= RefreshUI;
        }
    }


    private void OnDestroy()
    {
        if (applyPolicyButton != null)
        {
            applyPolicyButton.onClick.RemoveListener(ApplySelectedPolicy);
        }
    }


    /// <summary>
    /// Aplica la política seleccionada y avanza el turno.
    /// </summary>
    public void ApplySelectedPolicy()
    {
        if (countryState == null)
        {
            Debug.LogError(
                "CountryUIController: CountryState no está asignado."
            );

            return;
        }

        if (selectedPolicy == null)
        {
            Debug.LogError(
                "CountryUIController: no hay ninguna política asignada."
            );

            SetFeedback("No hay ninguna política seleccionada.");

            return;
        }

        if (countryState.HasReachedFinalTurn)
        {
            SetFeedback("La simulación ha terminado.");
            RefreshUI();

            return;
        }

        countryState.ApplyPolicy(selectedPolicy);

        bool turnAdvanced = countryState.AdvanceTurn();

        if (turnAdvanced)
        {
            SetFeedback(
                $"Se ha aplicado: {selectedPolicy.PolicyName}."
            );
        }
        else
        {
            SetFeedback(
                "La política se aplicó, pero no se pudo avanzar el turno."
            );
        }

        RefreshUI();
    }


    /// <summary>
    /// Actualiza todos los textos de la interfaz.
    /// </summary>
    public void RefreshUI()
    {
        if (countryState == null)
        {
            return;
        }

        SetText(
            countryNameText,
            countryState.CountryName
        );

        SetText(
            turnText,
            $"Turno {countryState.CurrentTurn} de " +
            $"{countryState.MaximumTurns}"
        );

        SetText(
            prosperityText,
            $"Prosperidad: {countryState.Prosperity:0.#}"
        );

        SetText(
            equalityText,
            $"Igualdad: {countryState.Equality:0.#}"
        );

        SetText(
            employmentText,
            $"Empleo: {countryState.Employment:0.#}"
        );

        SetText(
            educationText,
            $"Educación: {countryState.Education:0.#}"
        );

        SetText(
            stabilityText,
            $"Estabilidad: {countryState.Stability:0.#}"
        );

        RefreshPolicyInformation();
        RefreshButton();
    }


    private void RefreshPolicyInformation()
    {
        if (selectedPolicy == null)
        {
            SetText(
                policyNameText,
                "Ninguna política seleccionada"
            );

            SetText(
                policyDescriptionText,
                string.Empty
            );

            return;
        }

        SetText(
            policyNameText,
            selectedPolicy.PolicyName
        );

        SetText(
            policyDescriptionText,
            selectedPolicy.Description
        );
    }


    private void RefreshButton()
    {
        if (applyPolicyButton == null)
        {
            return;
        }

        bool simulationCanContinue =
            countryState != null &&
            selectedPolicy != null &&
            !countryState.HasReachedFinalTurn;

        applyPolicyButton.interactable = simulationCanContinue;
    }


    private void SetFeedback(string message)
    {
        SetText(feedbackText, message);
    }


    private void SetText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
        {
            textComponent.text = value;
        }
    }
}