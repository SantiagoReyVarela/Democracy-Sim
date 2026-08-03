using System;
using UnityEngine;

/// <summary>
/// Espacio configurable en el que puede mantenerse activa una política.
/// </summary>
[Serializable]
public sealed class PolicySlotState
{
    [SerializeField]
    private string slotId;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private PoliticalAxis axis;

    [SerializeField]
    private PolicyDefinitionSO activePolicy;

    public string SlotId => slotId;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName)
            ? slotId
            : displayName;

    public PoliticalAxis Axis => axis;

    public PolicyDefinitionSO ActivePolicy => activePolicy;

    public bool IsEmpty => activePolicy == null;

    public void SetPolicy(PolicyDefinitionSO policy)
    {
        activePolicy = policy;
    }

    public void Clear()
    {
        activePolicy = null;
    }
}