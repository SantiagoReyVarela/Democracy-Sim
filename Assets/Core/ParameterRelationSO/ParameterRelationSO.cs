using System;
using System.Collections.Generic;
using UnityEngine;

public enum RelationInterpolationMode
{
    // Mantiene el impacto del punto anterior hasta alcanzar el siguiente.
    Step = 0,

    // Calcula una transición proporcional y recta entre dos puntos.
    Linear = 1,

    // Genera una curva suave que pasa por los puntos definidos.
    Spline = 2
}

[Serializable]
public class RelationPoint
{
    [SerializeField]
    private float sourceValue;

    [SerializeField]
    private float impactPerTurn;

    public float SourceValue => sourceValue;
    public float ImpactPerTurn => impactPerTurn;
}

/// <summary>
/// Define cómo el valor de un parámetro provoca cambios por turno
/// sobre otro parámetro.
/// </summary>
[CreateAssetMenu(
    fileName = "Relation_New",
    menuName = "Democracy Simulator/Parameter Relation",
    order = 10
)]
public sealed class ParameterRelationSO : ScriptableObject
{
    [Header("Identification")]

    [SerializeField]
    private string relationId;


    [Header("Parameters")]

    [SerializeField]
    private CountryParameter sourceParameter;

    [SerializeField]
    private CountryParameter targetParameter;


    [Header("Evaluation")]

    public IReadOnlyList<RelationPoint> Points => points;

    [SerializeField]
    private RelationInterpolationMode interpolationMode =
        RelationInterpolationMode.Linear;

    [SerializeField]
    private List<RelationPoint> points =
        new List<RelationPoint>();


    [Header("Spline settings")]

    [Tooltip(
        "Evita que la spline produzca valores superiores o inferiores " +
        "a los dos puntos que delimitan cada tramo."
    )]
    [SerializeField]
    private bool clampSplineToSegment = true;

    [Tooltip(
    "Continúa la tendencia fuera del rango usando la pendiente " +
    "de los dos primeros o de los dos últimos puntos."
    )]
    [SerializeField]
    private bool extrapolateBeyondRange = true;


    [Header("General settings")]

    [SerializeField]
    private float multiplier = 1f;

    [SerializeField]
    private bool isEnabled = true;


    public string RelationId => relationId;

    public CountryParameter SourceParameter => sourceParameter;

    public CountryParameter TargetParameter => targetParameter;

    public RelationInterpolationMode InterpolationMode =>
        interpolationMode;

    public bool IsEnabled => isEnabled;


    /// <summary>
    /// Calcula el impacto correspondiente al valor del parámetro de origen.
    /// </summary>
    public float Evaluate(float sourceValue)
    {
        if (!isEnabled || points == null || points.Count == 0)
        {
            return 0f;
        }

        if (points.Count == 1)
        {
            return points[0].ImpactPerTurn * multiplier;
        }

        int lastIndex = points.Count - 1;

        if (sourceValue < points[0].SourceValue)
        {
            float impact = extrapolateBeyondRange
                ? EvaluateExtrapolation(
                    points[0],
                    points[1],
                    sourceValue
                )
                : points[0].ImpactPerTurn;

            return impact * multiplier;
        }

        if (sourceValue > points[lastIndex].SourceValue)
        {
            float impact = extrapolateBeyondRange
                ? EvaluateExtrapolation(
                    points[lastIndex - 1],
                    points[lastIndex],
                    sourceValue
                )
                : points[lastIndex].ImpactPerTurn;

            return impact * multiplier;
        }

        for (int i = 1; i < points.Count; i++)
        {
            RelationPoint previousPoint = points[i - 1];
            RelationPoint nextPoint = points[i];

            if (Mathf.Approximately(
                sourceValue,
                nextPoint.SourceValue
            ))
            {
                return nextPoint.ImpactPerTurn * multiplier;
            }

            if (sourceValue < nextPoint.SourceValue)
            {
                float impact = EvaluateSegment(
                    segmentStartIndex: i - 1,
                    sourceValue
                );

                return impact * multiplier;
            }
        }

        return points[lastIndex].ImpactPerTurn * multiplier;
    }


    /// <summary>
    /// Evalúa el tramo comprendido entre un punto y el siguiente.
    /// </summary>
    private float EvaluateSegment(
        int segmentStartIndex,
        float sourceValue
    )
    {
        RelationPoint startPoint =
            points[segmentStartIndex];

        RelationPoint endPoint =
            points[segmentStartIndex + 1];

        switch (interpolationMode)
        {
            case RelationInterpolationMode.Step:
                return startPoint.ImpactPerTurn;

            case RelationInterpolationMode.Linear:
                return EvaluateLinear(
                    startPoint,
                    endPoint,
                    sourceValue
                );

            case RelationInterpolationMode.Spline:
                return EvaluateSpline(
                    segmentStartIndex,
                    sourceValue
                );

            default:
                return startPoint.ImpactPerTurn;
        }
    }


    private float EvaluateLinear(
        RelationPoint startPoint,
        RelationPoint endPoint,
        float sourceValue
    )
    {
        float interval =
            endPoint.SourceValue -
            startPoint.SourceValue;

        if (Mathf.Approximately(interval, 0f))
        {
            return endPoint.ImpactPerTurn;
        }

        float interpolation = Mathf.Clamp01(
            (sourceValue - startPoint.SourceValue) /
            interval
        );

        return Mathf.Lerp(
            startPoint.ImpactPerTurn,
            endPoint.ImpactPerTurn,
            interpolation
        );
    }


    /// <summary>
    /// Interpolación cúbica suave que pasa exactamente por los puntos.
    /// Utiliza tangentes calculadas a partir de los puntos vecinos.
    /// </summary>
    private float EvaluateSpline(
        int segmentStartIndex,
        float sourceValue
    )
    {
        /*
         * Con solo dos puntos no es posible calcular correctamente
         * la curvatura, por lo que usamos interpolación lineal.
         */
        if (points.Count < 3)
        {
            return EvaluateLinear(
                points[segmentStartIndex],
                points[segmentStartIndex + 1],
                sourceValue
            );
        }

        int startIndex = segmentStartIndex;
        int endIndex = segmentStartIndex + 1;

        RelationPoint startPoint = points[startIndex];
        RelationPoint endPoint = points[endIndex];

        float interval =
            endPoint.SourceValue -
            startPoint.SourceValue;

        if (Mathf.Approximately(interval, 0f))
        {
            return endPoint.ImpactPerTurn;
        }

        float t = Mathf.Clamp01(
            (sourceValue - startPoint.SourceValue) /
            interval
        );

        float startTangent =
            CalculateTangent(startIndex);

        float endTangent =
            CalculateTangent(endIndex);

        float tSquared = t * t;
        float tCubed = tSquared * t;

        // Funciones base de Hermite.
        float startValueWeight =
            2f * tCubed -
            3f * tSquared +
            1f;

        float startTangentWeight =
            tCubed -
            2f * tSquared +
            t;

        float endValueWeight =
            -2f * tCubed +
            3f * tSquared;

        float endTangentWeight =
            tCubed -
            tSquared;

        float result =
            startValueWeight *
            startPoint.ImpactPerTurn +

            startTangentWeight *
            interval *
            startTangent +

            endValueWeight *
            endPoint.ImpactPerTurn +

            endTangentWeight *
            interval *
            endTangent;

        if (clampSplineToSegment)
        {
            float minimumImpact = Mathf.Min(
                startPoint.ImpactPerTurn,
                endPoint.ImpactPerTurn
            );

            float maximumImpact = Mathf.Max(
                startPoint.ImpactPerTurn,
                endPoint.ImpactPerTurn
            );

            result = Mathf.Clamp(
                result,
                minimumImpact,
                maximumImpact
            );
        }

        return result;
    }


    /// <summary>
    /// Calcula la pendiente de la curva en un punto.
    /// En los extremos utiliza una diferencia lateral y en los puntos
    /// interiores utiliza el punto anterior y el siguiente.
    /// </summary>
    private float CalculateTangent(int pointIndex)
    {
        int lastIndex = points.Count - 1;

        if (pointIndex <= 0)
        {
            return CalculateSlope(
                points[0],
                points[1]
            );
        }

        if (pointIndex >= lastIndex)
        {
            return CalculateSlope(
                points[lastIndex - 1],
                points[lastIndex]
            );
        }

        RelationPoint previousPoint =
            points[pointIndex - 1];

        RelationPoint nextPoint =
            points[pointIndex + 1];

        return CalculateSlope(
            previousPoint,
            nextPoint
        );
    }


    private float CalculateSlope(
        RelationPoint firstPoint,
        RelationPoint secondPoint
    )
    {
        float sourceDifference =
            secondPoint.SourceValue -
            firstPoint.SourceValue;

        if (Mathf.Approximately(sourceDifference, 0f))
        {
            return 0f;
        }

        return (
            secondPoint.ImpactPerTurn -
            firstPoint.ImpactPerTurn
        ) / sourceDifference;
    }

    /// <summary>
    /// Continúa linealmente la pendiente definida por dos puntos,
    /// tanto hacia valores positivos como negativos infinitos.
    /// </summary>
    private float EvaluateExtrapolation(
        RelationPoint firstPoint,
        RelationPoint secondPoint,
        float sourceValue
    )
    {
        float sourceDifference =
            secondPoint.SourceValue -
            firstPoint.SourceValue;

        if (Mathf.Approximately(sourceDifference, 0f))
        {
            return firstPoint.ImpactPerTurn;
        }

        float slope =
            (
                secondPoint.ImpactPerTurn -
                firstPoint.ImpactPerTurn
            )
            / sourceDifference;

        return firstPoint.ImpactPerTurn +
            slope *
            (
                sourceValue -
                firstPoint.SourceValue
            );
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        relationId = relationId?.Trim();

        if (points == null)
        {
            points = new List<RelationPoint>();
            return;
        }

        points.Sort(
            (first, second) =>
                first.SourceValue.CompareTo(
                    second.SourceValue
                )
        );

        for (int i = 1; i < points.Count; i++)
        {
            if (Mathf.Approximately(
                points[i - 1].SourceValue,
                points[i].SourceValue
            ))
            {
                Debug.LogWarning(
                    $"{name}: existen dos puntos con el mismo " +
                    $"Source Value: {points[i].SourceValue}.",
                    this
                );
            }
        }
    }

#endif
}