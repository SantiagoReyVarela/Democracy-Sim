using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Genera las tarjetas de la escena Policies utilizando
/// las políticas incluidas en el catálogo.
/// </summary>
public sealed class PoliciesSceneController : MonoBehaviour
{
    [Header("Data")]

    [SerializeField]
    private PolicyCatalogSO policyCatalog;


    [Header("Card generation")]

    [SerializeField]
    private PolicyCardUI policyCardPrefab;

    [SerializeField]
    private Transform policiesContainer;


    [Header("Scene information")]

    [SerializeField]
    private TMP_Text turnText;

    [SerializeField]
    private TMP_Text feedbackText;


    private CountryState countryState;

    private readonly List<PolicyCardUI> generatedCards =
        new List<PolicyCardUI>();


    private void Start()
    {
        countryState = CountryState.Instance;

        if (countryState == null)
        {
            Debug.LogError(
                "PoliciesSceneController: no existe CountryState. " +
                "Inicia la partida desde la escena Economy."
            );

            SetFeedback(
                "No hay ninguna simulación activa. " +
                "Inicia la partida desde Economía."
            );

            return;
        }

        countryState.OnStateChanged += RefreshUI;

        GeneratePolicyCards();
        RefreshUI();
    }


    private void GeneratePolicyCards()
    {
        ClearGeneratedCards();

        if (policyCatalog == null)
        {
            Debug.LogError(
                "PoliciesSceneController: " +
                "PolicyCatalog no está asignado."
            );

            return;
        }

        if (policyCardPrefab == null)
        {
            Debug.LogError(
                "PoliciesSceneController: " +
                "PolicyCardPrefab no está asignado."
            );

            return;
        }

        if (policiesContainer == null)
        {
            Debug.LogError(
                "PoliciesSceneController: " +
                "PoliciesContainer no está asignado."
            );

            return;
        }

        List<PolicyDefinitionSO> sortedPolicies =
            new List<PolicyDefinitionSO>(
                policyCatalog.Policies
            );

        sortedPolicies.Sort(ComparePolicies);

        foreach (
            PolicyDefinitionSO policy
            in sortedPolicies
        )
        {
            if (policy == null)
            {
                continue;
            }

            PolicyCardUI card = Instantiate(
                policyCardPrefab,
                policiesContainer,
                false
            );

            card.name = $"PolicyCard_{policy.PolicyId}";

            card.Initialize(
                policy,
                ApplyPolicy
            );

            generatedCards.Add(card);
        }
    }


    private int ComparePolicies(
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

        int axisComparison =
            first.PrimaryAxis.CompareTo(
                second.PrimaryAxis
            );

        if (axisComparison != 0)
        {
            return axisComparison;
        }

        return string.Compare(
            first.PolicyName,
            second.PolicyName,
            System.StringComparison.OrdinalIgnoreCase
        );
    }


    private void ApplyPolicy(
        PolicyDefinitionSO policy
    )
    {
        if (countryState == null || policy == null)
        {
            return;
        }

        if (countryState.HasReachedFinalTurn)
        {
            SetFeedback(
                "La simulación ha terminado."
            );

            RefreshUI();
            return;
        }

        countryState.ApplyPolicy(policy);

        bool turnAdvanced =
            countryState.AdvanceTurn();

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


    private void RefreshUI()
    {
        if (countryState == null)
        {
            return;
        }

        if (turnText != null)
        {
            turnText.text =
                $"Turno {countryState.CurrentTurn} " +
                $"de {countryState.MaximumTurns}";
        }

        bool canApplyPolicies =
            !countryState.HasReachedFinalTurn;

        foreach (
            PolicyCardUI card
            in generatedCards
        )
        {
            if (card != null)
            {
                card.SetInteractable(
                    canApplyPolicies
                );
            }
        }
    }


    private void ClearGeneratedCards()
    {
        generatedCards.Clear();

        if (policiesContainer == null)
        {
            return;
        }

        for (
            int i = policiesContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                policiesContainer.GetChild(i).gameObject
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
            countryState.OnStateChanged -= RefreshUI;
        }
    }
}