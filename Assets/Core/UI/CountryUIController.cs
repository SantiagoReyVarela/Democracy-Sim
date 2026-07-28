using TMPro;
using UnityEngine;

/// <summary>
/// Conecta el estado del país con la interfaz gráfica.
/// También recibe las políticas seleccionadas por los botones.
/// </summary>
public class CountryUIController : MonoBehaviour
{
    [Header("Simulation")]

    [SerializeField]
    private CountryState countryState;


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



    [Header("Economic parameters")]

    [SerializeField]
    private TMP_Text fiscalResultText;

    [SerializeField]
    private TMP_Text reservesText;

    [SerializeField]
    private TMP_Text debtText;

    [SerializeField]
    private TMP_Text inflationText;

    [Header("Feedback")]

    [SerializeField]
    private TMP_Text feedbackText;


    /// <summary>
    /// Permite que otros componentes consulten el país asociado
    /// sin poder sustituir la referencia.
    /// </summary>
    public CountryState CountryState => countryState;


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

        SetFeedback(
            "Selecciona una política para avanzar el turno."
        );
    }


    private void OnDisable()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= RefreshUI;
        }
    }


    /// <summary>
    /// Aplica la política recibida y avanza un turno.
    /// Este método será llamado por cualquiera de los botones de políticas.
    /// </summary>
    public void ApplyPolicy(PolicyDefinitionSO policy)
    {
        if (countryState == null)
        {
            Debug.LogError(
                "CountryUIController: CountryState no está asignado."
            );

            return;
        }

        if (policy == null)
        {
            Debug.LogError(
                "CountryUIController: la política recibida es nula."
            );

            SetFeedback(
                "El botón no tiene una política asignada."
            );

            return;
        }

        if (countryState.HasReachedFinalTurn)
        {
            SetFeedback(
                "La simulación ha terminado."
            );

            return;
        }

        countryState.ApplyPolicy(policy);

        bool turnAdvanced = countryState.AdvanceTurn();

        if (turnAdvanced)
        {
            SetFeedback(
                $"Se ha aplicado: {policy.PolicyName}."
            );
        }
        else
        {
            SetFeedback(
                "No se ha podido avanzar el turno."
            );
        }

        RefreshUI();
    }


    /// <summary>
    /// Actualiza todos los textos relacionados con el país.
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

        SetText(
            fiscalResultText,
            $"Resultado fiscal: {countryState.FiscalResult:+0.#;-0.#;0}"
        );

        SetText(
            reservesText,
            $"Reservas: {countryState.Reserves:0.#}"
        );

        SetText(
            debtText,
            $"Deuda: {countryState.Debt:0.#}"
        );

        SetText(
            inflationText,
            $"Inflación: {countryState.Inflation:0.#}%"
        );
    }


    private void SetFeedback(string message)
    {
        SetText(feedbackText, message);
    }


    private void SetText(
        TMP_Text textComponent,
        string value
    )
    {
        if (textComponent != null)
        {
            textComponent.text = value;
        }
    }
}