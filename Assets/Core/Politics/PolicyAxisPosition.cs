using System;
using UnityEngine;

/// <summary>
/// Posición de una política dentro de un eje ideológico.
/// </summary>
[Serializable]
public sealed class PolicyAxisPosition
{
    [SerializeField]
    private PoliticalAxis axis;

    [Tooltip("Posición ideológica dentro del eje, entre 1 y 5.")]
    [SerializeField, Range(1, 5)]
    private int position = 3;

    [Tooltip(
        "Indica cuánto representa esta política al eje. " +
        "1 significa que es completamente relevante."
    )]
    [SerializeField, Range(0f, 1f)]
    private float relevance = 1f;

    public PoliticalAxis Axis => axis;

    public int Position => position;

    public float Relevance => relevance;
}