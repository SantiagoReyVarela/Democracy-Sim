using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa visualmente una política concreta dentro del catálogo.
/// </summary>
public sealed class PolicyCardUI : MonoBehaviour
{
    [Header("Texts")]

    [SerializeField]
    private TMP_Text policyNameText;

    [SerializeField]
    private TMP_Text primaryAxisText;

    [SerializeField]
    private TMP_Text ideologicalProfileText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text effectsText;


    [Header("Controls")]

    [SerializeField]
    private Button applyButton;


    private PolicyDefinitionSO policy;
    private Action<PolicyDefinitionSO> applyCallback;


    private void Awake()
    {
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(HandleApplyClicked);
        }
    }


    /// <summary>
    /// Asigna a la tarjeta una política del catálogo.
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


    public void SetInteractable(bool interactable)
    {
        if (applyButton != null)
        {
            applyButton.interactable =
                interactable && policy != null;
        }
    }


    private void HandleApplyClicked()
    {
        if (policy == null)
        {
            Debug.LogError(
                $"{name}: no hay una política asignada."
            );

            return;
        }

        applyCallback?.Invoke(policy);
    }


    private void RefreshCard()
    {
        if (policy == null)
        {
            SetText(policyNameText, "Política no asignada");
            SetInteractable(false);
            return;
        }

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
            effectsText,
            BuildEffectsText()
        );

        SetInteractable(true);
    }


    private string BuildIdeologicalProfileText()
    {
        if (
            policy.IdeologicalProfile == null ||
            policy.IdeologicalProfile.Count == 0
        )
        {
            return "Sin perfil ideológico.";
        }

        StringBuilder builder = new StringBuilder();

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
                GetPositionName(axisPosition);

            builder.Append(
                $"{axisName}: " +
                $"{axisPosition.Position:0.#} — " +
                $"{positionName} " +
                $"({axisPosition.Relevance:P0})"
            );

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }


    private string GetPositionName(
        PolicyAxisPosition axisPosition
    )
    {
        float position = axisPosition.Position;

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


    private string BuildEffectsText()
    {
        if (
            policy.Effects == null ||
            policy.Effects.Count == 0
        )
        {
            return "Sin efectos configurados.";
        }

        StringBuilder builder = new StringBuilder();

        foreach (PolicyEffect effect in policy.Effects)
        {
            builder.Append("• ");

            builder.Append(
                GetParameterDisplayName(effect.Parameter)
            );

            builder.Append(": ");

            builder.Append(
                FormatSignedValue(effect.Amount)
            );

            if (effect.DelayInTurns > 0)
            {
                builder.Append(
                    $" · retraso {effect.DelayInTurns}"
                );
            }

            if (effect.DurationInTurns > 0)
            {
                builder.Append(
                    $" · duración {effect.DurationInTurns}"
                );
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
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
        return value > 0f
            ? $"+{value:0.##}"
            : $"{value:0.##}";
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