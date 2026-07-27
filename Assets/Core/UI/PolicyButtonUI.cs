using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa un botón asociado a una política concreta.
/// El mismo componente sirve para cualquier PolicyDefinitionSO.
/// </summary>
[RequireComponent(typeof(Button))]
public class PolicyButtonUI : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private CountryUIController countryUIController;

    [SerializeField]
    private PolicyDefinitionSO policy;

    [SerializeField]
    private TMP_Text buttonLabel;


    private Button button;
    private CountryState countryState;


    private void Awake()
    {
        button = GetComponent<Button>();

        if (buttonLabel == null)
        {
            buttonLabel = GetComponentInChildren<TMP_Text>();
        }
    }


    private void OnEnable()
    {
        button.onClick.AddListener(HandleButtonClicked);

        if (countryUIController != null)
        {
            countryState = countryUIController.CountryState;
        }

        if (countryState != null)
        {
            countryState.OnStateChanged += RefreshInteractivity;
        }

        RefreshButton();
    }


    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleButtonClicked);
        }

        if (countryState != null)
        {
            countryState.OnStateChanged -= RefreshInteractivity;
        }
    }


    private void HandleButtonClicked()
    {
        if (countryUIController == null)
        {
            Debug.LogError(
                $"{name}: CountryUIController no está asignado."
            );

            return;
        }

        if (policy == null)
        {
            Debug.LogError(
                $"{name}: no hay ninguna política asignada."
            );

            return;
        }

        countryUIController.ApplyPolicy(policy);
    }


    private void RefreshButton()
    {
        if (buttonLabel != null)
        {
            buttonLabel.text = policy != null
                ? policy.PolicyName
                : "Política sin asignar";
        }

        RefreshInteractivity();
    }


    private void RefreshInteractivity()
    {
        if (button == null)
        {
            return;
        }

        button.interactable =
            countryUIController != null &&
            countryState != null &&
            policy != null &&
            !countryState.HasReachedFinalTurn;
    }
}