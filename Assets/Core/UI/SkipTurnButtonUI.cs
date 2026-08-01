using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Permite avanzar un turno sin aprobar una nueva política.
/// </summary>
[RequireComponent(typeof(Button))]
public sealed class SkipTurnButtonUI : MonoBehaviour
{
    private Button skipTurnButton;
    private CountryState countryState;

    private void Awake()
    {
        skipTurnButton = GetComponent<Button>();
        skipTurnButton.onClick.AddListener(SkipTurn);
    }

    private void OnEnable()
    {
        BindCountryState();
        RefreshInteractivity();
    }

    private void BindCountryState()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= RefreshInteractivity;
        }

        countryState = CountryState.Instance;

        if (countryState == null)
        {
            Debug.LogError(
                "SkipTurnButtonUI: no existe CountryState.Instance."
            );

            return;
        }

        countryState.OnStateChanged += RefreshInteractivity;
    }

    private void SkipTurn()
    {
        if (countryState == null)
        {
            BindCountryState();
        }

        if (
            countryState == null ||
            countryState.HasReachedFinalTurn
        )
        {
            RefreshInteractivity();
            return;
        }

        countryState.AdvanceTurn();
    }

    private void RefreshInteractivity()
    {
        if (skipTurnButton == null)
        {
            return;
        }

        skipTurnButton.interactable =
            countryState != null &&
            !countryState.HasReachedFinalTurn;
    }

    private void OnDisable()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= RefreshInteractivity;
        }
    }

    private void OnDestroy()
    {
        if (skipTurnButton != null)
        {
            skipTurnButton.onClick.RemoveListener(SkipTurn);
        }
    }
}