using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla la escena de políticas permanentes.
///
/// La columna izquierda muestra los espacios disponibles.
/// La columna derecha muestra las políticas compatibles
/// con el espacio seleccionado.
/// </summary>
public sealed class PoliciesSceneController : MonoBehaviour
{
    [Header("Data")]

    [SerializeField]
    private PolicyCatalogSO policyCatalog;


    [Header("Policy slots")]

    [SerializeField]
    private PolicySlotButtonUI policySlotButtonPrefab;

    [SerializeField]
    private Transform policySlotsContainer;


    [Header("Policy cards")]

    [SerializeField]
    private PolicyCardUI policyCardPrefab;

    [SerializeField]
    private Transform policiesContainer;


    [Header("Selected slot information")]

    [SerializeField]
    private TMP_Text selectedSlotText;

    [SerializeField]
    private TMP_Text catalogTitleText;


    [Header("Scene information")]

    [SerializeField]
    private TMP_Text turnText;

    [SerializeField]
    private TMP_Text feedbackText;


    private CountryState countryState;

    private PolicySlotState selectedSlot;


    private readonly List<PolicySlotButtonUI>
        generatedSlotButtons =
            new List<PolicySlotButtonUI>();


    private readonly Dictionary<
        PolicyCardUI,
        PolicyDefinitionSO
    > generatedPolicyCards =
        new Dictionary<
            PolicyCardUI,
            PolicyDefinitionSO
        >();


    private void Start()
    {
        countryState = CountryState.Instance;

        if (countryState == null)
        {
            Debug.LogError(
                "PoliciesSceneController: " +
                "no existe CountryState.Instance."
            );

            SetFeedback(
                "No hay ninguna simulación activa."
            );

            return;
        }

        countryState.OnStateChanged +=
            HandleStateChanged;

        GeneratePolicySlotButtons();

        SelectFirstAvailableSlot();

        RefreshUI();
    }


    /// <summary>
    /// Genera una fila por cada espacio configurado
    /// dentro de CountryState.
    /// </summary>
    private void GeneratePolicySlotButtons()
    {
        ClearPolicySlotButtons();

        if (policySlotButtonPrefab == null)
        {
            Debug.LogError(
                "Policy Slot Button Prefab no está asignado."
            );

            return;
        }

        if (policySlotsContainer == null)
        {
            Debug.LogError(
                "Policy Slots Container no está asignado."
            );

            return;
        }

        foreach (
            PolicySlotState slot
            in countryState.PolicySlots
        )
        {
            if (slot == null)
            {
                continue;
            }

            PolicySlotButtonUI slotButton =
                Instantiate(
                    policySlotButtonPrefab,
                    policySlotsContainer,
                    false
                );

            slotButton.name =
                $"PolicySlotRow_{slot.SlotId}";

            slotButton.Initialize(
                slot,
                SelectPolicySlot,
                RemovePolicyFromSlot
            );

            generatedSlotButtons.Add(
                slotButton
            );
        }
    }


    /// <summary>
    /// Selecciona automáticamente el primer espacio
    /// disponible al abrir la escena.
    /// </summary>
    private void SelectFirstAvailableSlot()
    {
        if (countryState.PolicySlots == null)
        {
            return;
        }

        foreach (
            PolicySlotState slot
            in countryState.PolicySlots
        )
        {
            if (slot == null)
            {
                continue;
            }

            SelectPolicySlot(slot);
            return;
        }
    }


    /// <summary>
    /// Selecciona un espacio y genera las políticas
    /// compatibles en la columna derecha.
    /// </summary>
    private void SelectPolicySlot(
        PolicySlotState slot
    )
    {
        if (slot == null)
        {
            return;
        }

        selectedSlot = slot;

        RefreshSlotSelectionIndicators();
        GenerateCompatiblePolicyCards();
        RefreshSelectedSlotInformation();

        SetFeedback(
            $"Espacio seleccionado: {slot.DisplayName}."
        );
    }


    /// <summary>
    /// Genera solamente las políticas cuyo eje principal
    /// coincide con el espacio seleccionado.
    /// </summary>
    private void GenerateCompatiblePolicyCards()
    {
        ClearPolicyCards();

        if (
            selectedSlot == null ||
            policyCatalog == null ||
            policyCardPrefab == null ||
            policiesContainer == null
        )
        {
            return;
        }

        List<PolicyDefinitionSO> compatiblePolicies =
            new List<PolicyDefinitionSO>();

        foreach (
            PolicyDefinitionSO policy
            in policyCatalog.Policies
        )
        {
            if (
                policy != null &&
                policy.PrimaryAxis == selectedSlot.Axis
            )
            {
                compatiblePolicies.Add(policy);
            }
        }

        compatiblePolicies.Sort(
            ComparePoliciesByName
        );

        foreach (
            PolicyDefinitionSO policy
            in compatiblePolicies
        )
        {
            PolicyCardUI policyCard =
                Instantiate(
                    policyCardPrefab,
                    policiesContainer,
                    false
                );

            policyCard.name =
                $"PolicyCard_{policy.PolicyId}";

            policyCard.Initialize(
                policy,
                AssignPolicyToSelectedSlot
            );

            generatedPolicyCards.Add(
                policyCard,
                policy
            );
        }

        RefreshPolicyCardInteractivity();
    }


    private int ComparePoliciesByName(
        PolicyDefinitionSO first,
        PolicyDefinitionSO second
    )
    {
        if (first == null && second == null)
        {
            return 0;
        }

        if (first == null)
        {
            return 1;
        }

        if (second == null)
        {
            return -1;
        }

        return string.Compare(
            first.PolicyName,
            second.PolicyName,
            StringComparison.OrdinalIgnoreCase
        );
    }


    /// <summary>
    /// Coloca o sustituye la política del espacio seleccionado.
    /// No avanza el turno.
    /// </summary>
    private void AssignPolicyToSelectedSlot(
        PolicyDefinitionSO policy
    )
    {
        if (
            countryState == null ||
            selectedSlot == null ||
            policy == null
        )
        {
            return;
        }

        PolicyDefinitionSO previousPolicy =
            selectedSlot.ActivePolicy;

        bool assigned =
            countryState.AssignPolicyToSlot(
                selectedSlot.SlotId,
                policy
            );

        if (!assigned)
        {
            SetFeedback(
                "No se ha podido seleccionar la política."
            );

            RefreshUI();
            return;
        }

        if (previousPolicy == null)
        {
            SetFeedback(
                $"{policy.PolicyName} ha sido asignada " +
                $"a {selectedSlot.DisplayName}."
            );
        }
        else if (previousPolicy != policy)
        {
            SetFeedback(
                $"{previousPolicy.PolicyName} ha sido sustituida " +
                $"por {policy.PolicyName}."
            );
        }
        else
        {
            SetFeedback(
                $"{policy.PolicyName} ya estaba activa " +
                $"en {selectedSlot.DisplayName}."
            );
        }

        RefreshUI();
    }


    /// <summary>
    /// Retira la política del espacio cuyo botón
    /// Retirar ha sido pulsado.
    /// </summary>
    private void RemovePolicyFromSlot(
        PolicySlotState slot
    )
    {
        if (
            countryState == null ||
            slot == null
        )
        {
            return;
        }

        if (slot.ActivePolicy == null)
        {
            SetFeedback(
                $"{slot.DisplayName} ya está vacío."
            );

            RefreshUI();
            return;
        }

        string removedPolicyName =
            slot.ActivePolicy.PolicyName;

        bool removed =
            countryState.RemovePolicyFromSlot(
                slot.SlotId
            );

        if (removed)
        {
            SetFeedback(
                $"{removedPolicyName} ha sido retirada " +
                $"de {slot.DisplayName}."
            );
        }
        else
        {
            SetFeedback(
                "No se ha podido retirar la política."
            );
        }

        RefreshUI();
    }


    private void HandleStateChanged()
    {
        RefreshUI();
    }


    private void RefreshUI()
    {
        RefreshSlotButtons();
        RefreshSlotSelectionIndicators();
        RefreshSelectedSlotInformation();
        RefreshPolicyCardInteractivity();
        RefreshTurnText();
    }


    private void RefreshSlotButtons()
    {
        foreach (
            PolicySlotButtonUI slotButton
            in generatedSlotButtons
        )
        {
            if (slotButton != null)
            {
                slotButton.Refresh();
            }
        }
    }


    private void RefreshSlotSelectionIndicators()
    {
        foreach (
            PolicySlotButtonUI slotButton
            in generatedSlotButtons
        )
        {
            if (slotButton == null)
            {
                continue;
            }

            bool isSelected =
                slotButton.PolicySlot == selectedSlot;

            slotButton.SetSelected(
                isSelected
            );
        }
    }


    private void RefreshSelectedSlotInformation()
    {
        if (selectedSlotText != null)
        {
            if (selectedSlot == null)
            {
                selectedSlotText.text =
                    "Ningún espacio seleccionado";
            }
            else
            {
                string activePolicyName =
                    selectedSlot.ActivePolicy != null
                        ? selectedSlot.ActivePolicy.PolicyName
                        : "Espacio vacío";

                selectedSlotText.text =
                    $"Seleccionando para: " +
                    $"{selectedSlot.DisplayName}\n" +
                    $"Política actual: {activePolicyName}";
            }
        }

        if (catalogTitleText != null)
        {
            if (selectedSlot == null)
            {
                catalogTitleText.text =
                    "Políticas disponibles";
            }
            else
            {
                string axisName =
                    PoliticalAxisLabels.GetAxisName(
                        selectedSlot.Axis
                    );

                catalogTitleText.text =
                    $"Políticas disponibles: {axisName}";
            }
        }
    }


    /// <summary>
    /// Desactiva una política si ya está ocupando
    /// otro espacio diferente.
    /// </summary>
    private void RefreshPolicyCardInteractivity()
    {
        if (countryState == null)
        {
            return;
        }

        foreach (
            KeyValuePair<
                PolicyCardUI,
                PolicyDefinitionSO
            > entry
            in generatedPolicyCards
        )
        {
            PolicyCardUI card = entry.Key;
            PolicyDefinitionSO policy = entry.Value;

            if (
                card == null ||
                policy == null
            )
            {
                continue;
            }

            bool activeInSelectedSlot =
                selectedSlot != null &&
                selectedSlot.ActivePolicy == policy;

            bool activeInAnotherSlot =
                countryState.IsPolicyActive(policy) &&
                !activeInSelectedSlot;

            card.SetInteractable(
                !activeInAnotherSlot
            );
        }
    }


    private void RefreshTurnText()
    {
        if (
            turnText == null ||
            countryState == null
        )
        {
            return;
        }

        turnText.text =
            $"Turno {countryState.CurrentTurn} " +
            $"de {countryState.MaximumTurns}";
    }


    private void ClearPolicySlotButtons()
    {
        generatedSlotButtons.Clear();

        if (policySlotsContainer == null)
        {
            return;
        }

        for (
            int i =
                policySlotsContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                policySlotsContainer
                    .GetChild(i)
                    .gameObject
            );
        }
    }


    private void ClearPolicyCards()
    {
        generatedPolicyCards.Clear();

        if (policiesContainer == null)
        {
            return;
        }

        for (
            int i =
                policiesContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                policiesContainer
                    .GetChild(i)
                    .gameObject
            );
        }
    }


    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }


    private void OnDestroy()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -=
                HandleStateChanged;
        }
    }
}