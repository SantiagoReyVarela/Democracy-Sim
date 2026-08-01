using System;
using UnityEngine;

/// <summary>
/// Registra la aprobación de una política durante la partida.
/// </summary>
[Serializable]
public sealed class AppliedPolicyRecord
{
    [SerializeField]
    private PolicyDefinitionSO policy;

    [SerializeField]
    private string policyId;

    [SerializeField]
    private string policyName;

    [SerializeField]
    private int appliedTurn;


    public PolicyDefinitionSO Policy => policy;

    public string PolicyId => policyId;

    public string PolicyName => policyName;

    public int AppliedTurn => appliedTurn;


    public AppliedPolicyRecord(
        PolicyDefinitionSO policy,
        int appliedTurn
    )
    {
        this.policy = policy;

        policyId = policy != null
            ? policy.PolicyId
            : string.Empty;

        policyName = policy != null
            ? policy.PolicyName
            : "Unknown policy";

        this.appliedTurn = appliedTurn;
    }
}