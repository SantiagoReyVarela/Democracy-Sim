using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catálogo general de políticas disponibles en la partida.
/// </summary>
[CreateAssetMenu(
    fileName = "PolicyCatalog",
    menuName = "Democracy Simulator/Policy Catalog",
    order = 2
)]
public sealed class PolicyCatalogSO : ScriptableObject
{
    [SerializeField]
    private List<PolicyDefinitionSO> policies =
        new List<PolicyDefinitionSO>();

    public IReadOnlyList<PolicyDefinitionSO> Policies => policies;


    public List<PolicyDefinitionSO> GetPoliciesByAxis(
        PoliticalAxis axis
    )
    {
        List<PolicyDefinitionSO> result =
            new List<PolicyDefinitionSO>();

        foreach (PolicyDefinitionSO policy in policies)
        {
            if (
                policy != null &&
                policy.PrimaryAxis == axis
            )
            {
                result.Add(policy);
            }
        }

        return result;
    }


    public PolicyDefinitionSO GetPolicyById(string policyId)
    {
        if (string.IsNullOrWhiteSpace(policyId))
        {
            return null;
        }

        foreach (PolicyDefinitionSO policy in policies)
        {
            if (
                policy != null &&
                policy.PolicyId == policyId
            )
            {
                return policy;
            }
        }

        return null;
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        // Elimina referencias nulas.
        policies.RemoveAll(policy => policy == null);

        // Elimina políticas repetidas.
        HashSet<PolicyDefinitionSO> uniquePolicies =
            new HashSet<PolicyDefinitionSO>();

        policies.RemoveAll(
            policy => !uniquePolicies.Add(policy)
        );
    }

#endif
}