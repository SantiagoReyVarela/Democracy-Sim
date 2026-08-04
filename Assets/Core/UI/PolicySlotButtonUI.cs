using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa visualmente un espacio permanente de políticas.
///
/// Permite:
/// - Seleccionar el espacio.
/// - Ver la política activa.
/// - Retirar la política directamente desde el espacio.
/// </summary>
public sealed class PolicySlotButtonUI : MonoBehaviour
{
    [Header("Texts")]

    [SerializeField]
    private TMP_Text slotNameText;

    [SerializeField]
    private TMP_Text activePolicyText;


    [Header("Buttons")]

    [SerializeField]
    private Button selectSlotButton;

    [SerializeField]
    private Button removePolicyButton;


    [Header("Selection")]

    [SerializeField]
    private GameObject selectedIndicator;


    private PolicySlotState policySlot;

    private Action<PolicySlotState> selectionCallback;

    private Action<PolicySlotState> removalCallback;


    public PolicySlotState PolicySlot => policySlot;


    private void Awake()
    {
        if (selectSlotButton != null)
        {
            selectSlotButton.onClick.AddListener(
                HandleSelectClicked
            );
        }

        if (removePolicyButton != null)
        {
            removePolicyButton.onClick.AddListener(
                HandleRemoveClicked
            );
        }
    }


    /// <summary>
    /// Asigna a la fila uno de los espacios configurados
    /// dentro de CountryState.
    /// </summary>
    public void Initialize(
        PolicySlotState slot,
        Action<PolicySlotState> onSelected,
        Action<PolicySlotState> onRemoved
    )
    {
        policySlot = slot;

        selectionCallback = onSelected;
        removalCallback = onRemoved;

        Refresh();
        SetSelected(false);
    }


    /// <summary>
    /// Actualiza los textos y el estado de los botones.
    /// </summary>
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

            if (selectSlotButton != null)
            {
                selectSlotButton.interactable = false;
            }

            if (removePolicyButton != null)
            {
                removePolicyButton.interactable = false;
            }

            return;
        }

        SetText(
            slotNameText,
            policySlot.DisplayName
        );

        string activePolicyName =
            policySlot.ActivePolicy != null
                ? policySlot.ActivePolicy.PolicyName
                : "Espacio vacío";

        SetText(
            activePolicyText,
            activePolicyName
        );

        if (selectSlotButton != null)
        {
            selectSlotButton.interactable = true;
        }

        if (removePolicyButton != null)
        {
            removePolicyButton.interactable =
                policySlot.ActivePolicy != null;
        }
    }


    /// <summary>
    /// Muestra u oculta el indicador visual de selección.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }


    private void HandleSelectClicked()
    {
        if (policySlot == null)
        {
            return;
        }

        selectionCallback?.Invoke(policySlot);
    }


    private void HandleRemoveClicked()
    {
        if (
            policySlot == null ||
            policySlot.ActivePolicy == null
        )
        {
            return;
        }

        removalCallback?.Invoke(policySlot);
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
        if (selectSlotButton != null)
        {
            selectSlotButton.onClick.RemoveListener(
                HandleSelectClicked
            );
        }

        if (removePolicyButton != null)
        {
            removePolicyButton.onClick.RemoveListener(
                HandleRemoveClicked
            );
        }
    }
}