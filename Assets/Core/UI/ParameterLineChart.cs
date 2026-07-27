using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dibuja dentro de un Canvas la evolución histórica
/// de un parámetro del país.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class ParameterLineChart : MaskableGraphic
{
    [Header("Data")]

    [SerializeField]
    private CountryState countryState;

    [SerializeField]
    private CountryParameter parameter;


    [Header("Chart range")]

    [SerializeField]
    private float minimumValue = 0f;

    [SerializeField]
    private float maximumValue = 100f;


    [Header("Appearance")]

    [SerializeField, Min(0f)]
    private float padding = 12f;

    [SerializeField, Min(1f)]
    private float lineThickness = 4f;

    [SerializeField]
    private bool showPoints = true;

    [SerializeField, Min(1f)]
    private float pointSize = 8f;


    protected override void Awake()
    {
        base.Awake();

        ConfigureRectTransform();
        
        raycastTarget = false;
    }


    protected override void OnEnable()
    {
        base.OnEnable();

        if (countryState != null)
        {
            countryState.OnStateChanged += HandleStateChanged;
        }

        SetVerticesDirty();
    }


    protected override void OnDisable()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= HandleStateChanged;
        }

        base.OnDisable();
    }


    private void HandleStateChanged()
    {
        SetVerticesDirty();
    }


    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Debug.Log(
            $"{parameter} | Rect: {rectTransform.rect.size} | " +
            $"Historial: {countryState?.History.Count}"
        );

        if (countryState == null)
        {
            return;
        }

        if (countryState.History == null)
        {
            return;
        }

        if (countryState.History.Count == 0)
        {
            return;
        }

        if (maximumValue <= minimumValue)
        {
            return;
        }

        Rect chartRect = rectTransform.rect;

        float chartWidth = chartRect.width - padding * 2f;
        float chartHeight = chartRect.height - padding * 2f;

        if (chartWidth <= 0f || chartHeight <= 0f)
        {
            return;
        }

        Vector2 previousPoint = Vector2.zero;
        bool hasPreviousPoint = false;

        for (int i = 0; i < countryState.History.Count; i++)
        {
            CountrySnapshot snapshot =
                countryState.History[i];

            float horizontalPosition;

            if (countryState.MaximumTurns <= 1)
            {
                horizontalPosition = 0f;
            }
            else
            {
                horizontalPosition = Mathf.InverseLerp(
                    1f,
                    countryState.MaximumTurns,
                    snapshot.Turn
                );
            }

            float parameterValue =
                snapshot.GetParameter(parameter);

            float verticalPosition = Mathf.InverseLerp(
                minimumValue,
                maximumValue,
                parameterValue
            );

            Vector2 currentPoint = new Vector2(
                chartRect.xMin +
                padding +
                horizontalPosition * chartWidth,

                chartRect.yMin +
                padding +
                verticalPosition * chartHeight
            );

            if (hasPreviousPoint)
            {
                AddLine(
                    vertexHelper,
                    previousPoint,
                    currentPoint
                );
            }

            if (showPoints)
            {
                AddPoint(
                    vertexHelper,
                    currentPoint
                );
            }

            previousPoint = currentPoint;
            hasPreviousPoint = true;
        }
    }


    private void AddLine(
        VertexHelper vertexHelper,
        Vector2 start,
        Vector2 end
    )
    {
        Vector2 direction = end - start;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        direction.Normalize();

        Vector2 normal = new Vector2(
            -direction.y,
            direction.x
        );

        normal *= lineThickness * 0.5f;

        Vector2 bottomLeft = start - normal;
        Vector2 topLeft = start + normal;
        Vector2 topRight = end + normal;
        Vector2 bottomRight = end - normal;

        AddQuad(
            vertexHelper,
            bottomLeft,
            topLeft,
            topRight,
            bottomRight
        );
    }


    private void AddPoint(
        VertexHelper vertexHelper,
        Vector2 position
    )
    {
        float halfSize = pointSize * 0.5f;

        Vector2 bottomLeft = new Vector2(
            position.x - halfSize,
            position.y - halfSize
        );

        Vector2 topLeft = new Vector2(
            position.x - halfSize,
            position.y + halfSize
        );

        Vector2 topRight = new Vector2(
            position.x + halfSize,
            position.y + halfSize
        );

        Vector2 bottomRight = new Vector2(
            position.x + halfSize,
            position.y - halfSize
        );

        AddQuad(
            vertexHelper,
            bottomLeft,
            topLeft,
            topRight,
            bottomRight
        );
    }


    private void AddQuad(
        VertexHelper vertexHelper,
        Vector2 bottomLeft,
        Vector2 topLeft,
        Vector2 topRight,
        Vector2 bottomRight
    )
    {
        int startingIndex =
            vertexHelper.currentVertCount;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.uv0 = Vector2.zero;

        vertex.position = bottomLeft;
        vertexHelper.AddVert(vertex);

        vertex.position = topLeft;
        vertexHelper.AddVert(vertex);

        vertex.position = topRight;
        vertexHelper.AddVert(vertex);

        vertex.position = bottomRight;
        vertexHelper.AddVert(vertex);

        vertexHelper.AddTriangle(
            startingIndex,
            startingIndex + 1,
            startingIndex + 2
        );

        vertexHelper.AddTriangle(
            startingIndex,
            startingIndex + 2,
            startingIndex + 3
        );
    }


    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        SetVerticesDirty();
    }

    private void ConfigureRectTransform()
    {
        RectTransform rect = rectTransform;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        rect.localScale = Vector3.one;
    }


#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();

        padding = Mathf.Max(0f, padding);
        lineThickness = Mathf.Max(1f, lineThickness);
        pointSize = Mathf.Max(1f, pointSize);

        if (maximumValue <= minimumValue)
        {
            maximumValue = minimumValue + 1f;
        }

        SetVerticesDirty();
    }

#endif
}