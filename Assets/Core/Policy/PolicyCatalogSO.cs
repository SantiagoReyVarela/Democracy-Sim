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
    [Header("Políticas disponibles")]

    [Tooltip(
        "Lista completa de políticas disponibles. " +
        "Puede ampliarse modificando Size en el Inspector."
    )]
    [SerializeField]
    private List<PolicyDefinitionSO> policies =
        new List<PolicyDefinitionSO>();


    /// <summary>
    /// Lista de políticas en modo de solo lectura
    /// para el resto del juego.
    /// </summary>
    public IReadOnlyList<PolicyDefinitionSO> Policies =>
        policies;


    /// <summary>
    /// Número de espacios que tiene actualmente el catálogo.
    /// Puede incluir temporalmente elementos vacíos.
    /// </summary>
    public int Count =>
        policies != null
            ? policies.Count
            : 0;


    /// <summary>
    /// Devuelve todas las políticas pertenecientes
    /// a un eje político concreto.
    /// </summary>
    public List<PolicyDefinitionSO> GetPoliciesByAxis(
        PoliticalAxis axis
    )
    {
        List<PolicyDefinitionSO> result =
            new List<PolicyDefinitionSO>();

        if (policies == null)
        {
            return result;
        }

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


    /// <summary>
    /// Busca una política mediante su identificador interno.
    /// </summary>
    public PolicyDefinitionSO GetPolicyById(
        string policyId
    )
    {
        if (
            string.IsNullOrWhiteSpace(policyId) ||
            policies == null
        )
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


    /// <summary>
    /// Comprueba si una política ya está incluida
    /// en el catálogo.
    /// </summary>
    public bool ContainsPolicy(
        PolicyDefinitionSO policy
    )
    {
        return
            policy != null &&
            policies != null &&
            policies.Contains(policy);
    }


    /// <summary>
    /// Elimina manualmente los elementos vacíos
    /// y las políticas repetidas.
    ///
    /// Se ejecuta desde el menú contextual del componente:
    /// tres puntos → Limpiar catálogo.
    /// </summary>
    [ContextMenu("Limpiar catálogo")]
    private void CleanCatalog()
    {
        if (policies == null)
        {
            policies =
                new List<PolicyDefinitionSO>();

            MarkAsModified();

            return;
        }

        // Esta limpieza solo se ejecuta manualmente.
        policies.RemoveAll(
            policy => policy == null
        );

        HashSet<PolicyDefinitionSO> uniquePolicies =
            new HashSet<PolicyDefinitionSO>();

        for (
            int index = policies.Count - 1;
            index >= 0;
            index--
        )
        {
            PolicyDefinitionSO policy =
                policies[index];

            if (!uniquePolicies.Add(policy))
            {
                policies.RemoveAt(index);
            }
        }

        MarkAsModified();

        Debug.Log(
            $"Catálogo limpiado. " +
            $"Políticas restantes: {policies.Count}.",
            this
        );
    }


    /// <summary>
    /// Garantiza que la lista exista, pero no elimina
    /// los espacios vacíos creados al modificar Size.
    /// </summary>
    private void OnValidate()
    {
        if (policies == null)
        {
            policies =
                new List<PolicyDefinitionSO>();
        }
    }


    /// <summary>
    /// Marca el asset como modificado dentro del editor
    /// para que Unity guarde los cambios.
    /// </summary>
    private void MarkAsModified()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}