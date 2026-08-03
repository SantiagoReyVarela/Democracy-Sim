using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa visualmente un espacio de política permanente.
/// </summary>
[RequireComponent(typeof(Button))]
public sealed class PolicySlotButtonUI : MonoBehaviour
{
    [Header("Texts")]

    [SerializeField]
    private TMP_Text slotNameText;

    [SerializeField]
    private TMP_Text activePolicyText;


    [Header("Selection indicator")]

    [SerializeField]
    private GameObject selectedIndicator;


    private Button button;
    private PolicySlotState policySlot;
    private Action<PolicySlotState> selectionCallback;


    public PolicySlotState PolicySlot => policySlot;


    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClicked);
    }


    /// <summary>
    /// Asigna a este botón uno de los espacios configurados
    /// en CountryState.
    /// </summary>
    public void Initialize(
        PolicySlotState slot,
        Action<PolicySlotState> onSelected
    )
    {
        policySlot = slot;
        selectionCallback = onSelected;

        Refresh();
        SetSelected(false);
    }


    public void Refresh()
    {
        if (policySlot == null)
        {
            SetText(
                slotNameText,
                "Espacio no asignado"
            );

            SetText(
                activePolicyText,
                string.Empty
            );

            if (button != null)
            {
                button.interactable = false;
            }

            return;
        }

        SetText(
            slotNameText,
            policySlot.DisplayName
        );

        string policyName =
            policySlot.ActivePolicy != null
                ? policySlot.ActivePolicy.PolicyName
                : "Espacio vacío";

        SetText(
            activePolicyText,
            policyName
        );

        if (button != null)
        {
            button.interactable = true;
        }
    }


    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }


    private void HandleClicked()
    {
        if (policySlot == null)
        {
            return;
        }

        selectionCallback?.Invoke(policySlot);
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
        if (button != null)
        {
            button.onClick.RemoveListener(
                HandleClicked
            );
        }
    }
}