using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa visualmente una política dentro del catálogo.
///
/// Muestra:
/// - Nombre y descripción.
/// - Clasificación ideológica.
/// - Efectos inmediatos.
/// - Efectos a largo plazo.
/// - Botón para seleccionar la política.
/// </summary>
public sealed class PolicyCardUI : MonoBehaviour
{
    [Header("General texts")]

    [SerializeField]
    private TMP_Text policyNameText;

    [SerializeField]
    private TMP_Text primaryAxisText;

    [SerializeField]
    private TMP_Text ideologicalProfileText;

    [SerializeField]
    private TMP_Text descriptionText;


    [Header("Effects")]

    [SerializeField]
    private TMP_Text immediateEffectsText;

    [SerializeField]
    private TMP_Text longTermEffectsText;


    [Header("Controls")]

    [SerializeField]
    private Button applyButton;


    [Header("Visual appearance")]

    [Tooltip("Imagen de fondo de la raíz PolicyCard.")]
    [SerializeField]
    private Image cardBackground;


    private PolicyDefinitionSO policy;

    private Action<PolicyDefinitionSO> applyCallback;


    private void Awake()
    {
        /*
         * Si no se ha asignado manualmente el fondo,
         * intentamos obtener la Image de la raíz.
         */
        if (cardBackground == null)
        {
            cardBackground = GetComponent<Image>();
        }

        if (applyButton == null)
        {
            Debug.LogError(
                $"{name}: Apply Button no está asignado.",
                this
            );

            return;
        }

        applyButton.onClick.AddListener(
            HandleApplyClicked
        );
    }


    /// <summary>
    /// Asigna una política concreta a la tarjeta.
    /// </summary>
    public void Initialize(
        PolicyDefinitionSO policyDefinition,
        Action<PolicyDefinitionSO> onApply
    )
    {
        policy = policyDefinition;
        applyCallback = onApply;

        RefreshCard();
    }


    /// <summary>
    /// Activa o desactiva el botón de selección.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (applyButton != null)
        {
            applyButton.interactable =
                interactable &&
                policy != null;
        }
    }


    private void HandleApplyClicked()
    {
        if (policy == null)
        {
            Debug.LogError(
                $"{name}: no hay una política asignada.",
                this
            );

            return;
        }

        if (applyCallback == null)
        {
            Debug.LogError(
                $"{name}: el callback de selección " +
                $"no está asignado.",
                this
            );

            return;
        }

        applyCallback.Invoke(policy);
    }


    /// <summary>
    /// Actualiza todos los elementos visuales de la tarjeta.
    /// </summary>
    private void RefreshCard()
    {
        if (policy == null)
        {
            SetText(
                policyNameText,
                "Política no asignada"
            );

            SetText(
                primaryAxisText,
                string.Empty
            );

            SetText(
                ideologicalProfileText,
                string.Empty
            );

            SetText(
                descriptionText,
                string.Empty
            );

            SetText(
                immediateEffectsText,
                string.Empty
            );

            SetText(
                longTermEffectsText,
                string.Empty
            );

            SetInteractable(false);

            return;
        }

        ApplyPolicyColor();

        SetText(
            policyNameText,
            policy.PolicyName
        );

        SetText(
            primaryAxisText,
            $"Tema principal: " +
            $"{PoliticalAxisLabels.GetAxisName(policy.PrimaryAxis)}"
        );

        SetText(
            ideologicalProfileText,
            BuildIdeologicalProfileText()
        );

        SetText(
            descriptionText,
            policy.Description
        );

        SetText(
            immediateEffectsText,
            BuildEffectsColumn(
                "EFECTO INMEDIATO:",
                policy.FirstImpactEffects
            )
        );

        SetText(
            longTermEffectsText,
            BuildEffectsColumn(
                "EFECTOS A LARGO PLAZO:",
                policy.ActiveEffects
            )
        );

        SetInteractable(true);
    }


    /// <summary>
    /// Colorea automáticamente la tarjeta según
    /// el eje principal de la política.
    /// </summary>
    private void ApplyPolicyColor()
    {
        if (
            policy == null ||
            cardBackground == null
        )
        {
            return;
        }

        cardBackground.color =
            PoliticalAxisColors.GetBackgroundColor(
                policy.PrimaryAxis
            );
    }


    /// <summary>
    /// Construye el texto correspondiente a una columna
    /// de efectos.
    /// </summary>
    private string BuildEffectsColumn(
        string title,
        IReadOnlyList<PolicyEffect> effects
    )
    {
        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine(
            $"<b>{title}</b>"
        );

        if (
            effects == null ||
            effects.Count == 0
        )
        {
            builder.Append("- Ninguno");

            return builder.ToString();
        }

        bool hasValidEffects = false;

        foreach (PolicyEffect effect in effects)
        {
            if (effect == null)
            {
                continue;
            }

            hasValidEffects = true;

            builder.Append("- ");

            builder.Append(
                GetParameterDisplayName(
                    effect.Parameter
                )
            );

            builder.Append(": ");

            builder.Append(
                FormatSignedValue(
                    effect.Amount
                )
            );

            builder.AppendLine();
        }

        if (!hasValidEffects)
        {
            builder.Append("- Ninguno");
        }

        return builder.ToString().TrimEnd();
    }


    /// <summary>
    /// Construye la clasificación ideológica completa
    /// de la política.
    /// </summary>
    private string BuildIdeologicalProfileText()
    {
        if (
            policy.IdeologicalProfile == null ||
            policy.IdeologicalProfile.Count == 0
        )
        {
            return "Sin perfil ideológico.";
        }

        StringBuilder builder =
            new StringBuilder();

        foreach (
            PolicyAxisPosition axisPosition
            in policy.IdeologicalProfile
        )
        {
            if (axisPosition == null)
            {
                continue;
            }

            string axisName =
                PoliticalAxisLabels.GetAxisName(
                    axisPosition.Axis
                );

            string positionName =
                GetPositionName(
                    axisPosition
                );

            builder.Append(
                $"{axisName}: " +
                $"{axisPosition.Position:0.#} — " +
                $"{positionName} " +
                $"({axisPosition.Relevance:P0})"
            );

            builder.AppendLine();
        }

        string result =
            builder.ToString().TrimEnd();

        return string.IsNullOrWhiteSpace(result)
            ? "Sin perfil ideológico."
            : result;
    }


    /// <summary>
    /// Devuelve el nombre de una posición ideológica,
    /// incluyendo posiciones intermedias decimales.
    /// </summary>
    private string GetPositionName(
        PolicyAxisPosition axisPosition
    )
    {
        float position =
            Mathf.Clamp(
                axisPosition.Position,
                1f,
                5f
            );

        int lowerPosition =
            Mathf.FloorToInt(position);

        int upperPosition =
            Mathf.CeilToInt(position);

        if (lowerPosition == upperPosition)
        {
            return PoliticalAxisLabels.GetPositionName(
                axisPosition.Axis,
                lowerPosition
            );
        }

        string lowerName =
            PoliticalAxisLabels.GetPositionName(
                axisPosition.Axis,
                lowerPosition
            );

        string upperName =
            PoliticalAxisLabels.GetPositionName(
                axisPosition.Axis,
                upperPosition
            );

        return $"Entre {lowerName} y {upperName}";
    }


    private string GetParameterDisplayName(
        CountryParameter parameter
    )
    {
        switch (parameter)
        {
            case CountryParameter.Prosperity:
                return "Prosperidad";

            case CountryParameter.Equality:
                return "Igualdad";

            case CountryParameter.Employment:
                return "Empleo";

            case CountryParameter.Education:
                return "Educación";

            case CountryParameter.Stability:
                return "Estabilidad";

            case CountryParameter.FiscalResult:
                return "Resultado fiscal";

            case CountryParameter.Reserves:
                return "Reservas";

            case CountryParameter.Debt:
                return "Deuda";

            case CountryParameter.Inflation:
                return "Inflación";

            default:
                return parameter.ToString();
        }
    }


    private string FormatSignedValue(float value)
    {
        if (value > 0f)
        {
            return $"+{value:0.##}";
        }

        return $"{value:0.##}";
    }


    private void SetText(
        TMP_Text textComponent,
        string value
    )
    {
        if (textComponent != null)
        {
            textComponent.text =
                value ?? string.Empty;
        }
    }


    private void OnDestroy()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(
                HandleApplyClicked
            );
        }
    }
}