using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa visualmente un espacio permanente de políticas.
/// Su color depende automáticamente del eje configurado
/// en PolicySlotState.
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


    [Header("Colors")]

    [Tooltip("Imagen de fondo de la raíz PolicySlotRow.")]
    [SerializeField]
    private Image rowBackground;

    [Tooltip("Franja lateral que muestra el color intenso del eje.")]
    [SerializeField]
    private Image axisColorBar;


    [Header("Selection")]

    [SerializeField]
    private GameObject selectedIndicator;


    private PolicySlotState policySlot;

    private Action<PolicySlotState> selectionCallback;

    private Action<PolicySlotState> removalCallback;


    public PolicySlotState PolicySlot => policySlot;


    private void Awake()
    {
        /*
         * Si no se asignó manualmente, intentamos utilizar
         * la imagen de la raíz como fondo.
         */
        if (rowBackground == null)
        {
            rowBackground = GetComponent<Image>();
        }

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

        ApplyAxisColor();

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


    private void ApplyAxisColor()
    {
        if (policySlot == null)
        {
            return;
        }

        Color backgroundColor =
            PoliticalAxisColors.GetBackgroundColor(
                policySlot.Axis
            );

        Color accentColor =
            PoliticalAxisColors.GetAccentColor(
                policySlot.Axis
            );

        if (rowBackground != null)
        {
            rowBackground.color = backgroundColor;
        }

        if (axisColorBar != null)
        {
            axisColorBar.color = accentColor;
        }

        if (selectedIndicator != null)
        {
            Image indicatorImage =
                selectedIndicator.GetComponent<Image>();

            if (indicatorImage != null)
            {
                indicatorImage.color = accentColor;
            }
        }
    }


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