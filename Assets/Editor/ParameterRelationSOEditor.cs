using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParameterRelationSO))]
public sealed class ParameterRelationSOEditor : Editor
{
    private const float GraphHeight = 240f;
    private const float GraphPadding = 28f;
    private const int SampleCount = 150;

    private SerializedProperty relationIdProperty;
    private SerializedProperty sourceParameterProperty;
    private SerializedProperty targetParameterProperty;
    private SerializedProperty interpolationModeProperty;
    private SerializedProperty pointsProperty;
    private SerializedProperty clampSplineProperty;
    private SerializedProperty multiplierProperty;
    private SerializedProperty isEnabledProperty;

    private float graphZoom = 1f;
    private Vector2 graphPan = Vector2.zero;

    private const float MinimumZoom = 1f;
    private const float MaximumZoom = 20f;

    private void OnEnable()
    {
        relationIdProperty =
            serializedObject.FindProperty("relationId");

        sourceParameterProperty =
            serializedObject.FindProperty("sourceParameter");

        targetParameterProperty =
            serializedObject.FindProperty("targetParameter");

        interpolationModeProperty =
            serializedObject.FindProperty("interpolationMode");

        pointsProperty =
            serializedObject.FindProperty("points");

        clampSplineProperty =
            serializedObject.FindProperty("clampSplineToSegment");

        multiplierProperty =
            serializedObject.FindProperty("multiplier");

        isEnabledProperty =
            serializedObject.FindProperty("isEnabled");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField(
            "Identification",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(relationIdProperty);

        EditorGUILayout.Space(8f);

        EditorGUILayout.LabelField(
            "Parameters",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(sourceParameterProperty);
        EditorGUILayout.PropertyField(targetParameterProperty);

        EditorGUILayout.Space(8f);

        EditorGUILayout.LabelField(
            "Interpolation",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(interpolationModeProperty);

        EditorGUILayout.Space(4f);

        /*
         * Aquí se dibuja la lista de puntos.
         */
        EditorGUILayout.PropertyField(
            pointsProperty,
            new GUIContent("Points"),
            includeChildren: true
        );

        bool changed = serializedObject.ApplyModifiedProperties();

        if (changed)
        {
            EditorUtility.SetDirty(target);
            Repaint();
        }

        /*
         * La gráfica aparece inmediatamente debajo de Points.
         */
        EditorGUILayout.Space(8f);

        EditorGUILayout.LabelField(
            "Curve Preview",
            EditorStyles.boldLabel
        );

        DrawCurvePreview((ParameterRelationSO)target);

        EditorGUILayout.Space(8f);

        RelationInterpolationMode mode =
            (RelationInterpolationMode)
            interpolationModeProperty.enumValueIndex;

        if (mode == RelationInterpolationMode.Spline)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(clampSplineProperty);
            serializedObject.ApplyModifiedProperties();
        }

        serializedObject.Update();

        EditorGUILayout.Space(8f);

        EditorGUILayout.LabelField(
            "General Settings",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(multiplierProperty);
        EditorGUILayout.PropertyField(isEnabledProperty);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCurvePreview(
        ParameterRelationSO relation
    )
    {
        if (
            relation.Points == null ||
            relation.Points.Count == 0
        )
        {
            EditorGUILayout.HelpBox(
                "Añade al menos un punto para mostrar la curva.",
                MessageType.Info
            );

            return;
        }

        Rect outerRect = GUILayoutUtility.GetRect(
            100f,
            GraphHeight,
            GUILayout.ExpandWidth(true)
        );

        EditorGUI.DrawRect(
            outerRect,
            new Color(0.10f, 0.10f, 0.10f)
        );

        Rect graphRect = new Rect(
            outerRect.x + GraphPadding,
            outerRect.y + GraphPadding,
            outerRect.width - GraphPadding * 2f,
            outerRect.height - GraphPadding * 2f
        );

        CalculateRanges(
            relation,
            out float minimumX,
            out float maximumX,
            out float minimumY,
            out float maximumY
        );

        HandleGraphNavigation(
            graphRect,
            minimumX,
            maximumX,
            minimumY,
            maximumY
        );

        ApplyViewTransform(
            ref minimumX,
            ref maximumX,
            ref minimumY,
            ref maximumY
        );

        DrawGrid(graphRect);
        DrawZeroAxes(
            graphRect,
            minimumX,
            maximumX,
            minimumY,
            maximumY
        );

        DrawCurve(
            relation,
            graphRect,
            minimumX,
            maximumX,
            minimumY,
            maximumY
        );

        DrawKeyPoints(
            relation,
            graphRect,
            minimumX,
            maximumX,
            minimumY,
            maximumY
        );

        DrawRangeLabels(
            outerRect,
            minimumX,
            maximumX,
            minimumY,
            maximumY
        );
    }

    private void CalculateRanges(
        ParameterRelationSO relation,
        out float minimumX,
        out float maximumX,
        out float minimumY,
        out float maximumY
    )
    {
        minimumX = relation.Points[0].SourceValue;
        maximumX =
            relation.Points[
                relation.Points.Count - 1
            ].SourceValue;

        if (Mathf.Approximately(minimumX, maximumX))
        {
            maximumX = minimumX + 1f;
        }

        minimumY = float.PositiveInfinity;
        maximumY = float.NegativeInfinity;

        for (int i = 0; i < SampleCount; i++)
        {
            float interpolation =
                i / (float)(SampleCount - 1);

            float sourceValue = Mathf.Lerp(
                minimumX,
                maximumX,
                interpolation
            );

            float impact = relation.Evaluate(sourceValue);

            minimumY = Mathf.Min(minimumY, impact);
            maximumY = Mathf.Max(maximumY, impact);
        }

        if (Mathf.Approximately(minimumY, maximumY))
        {
            minimumY -= 1f;
            maximumY += 1f;
        }
        else
        {
            float verticalMargin =
                (maximumY - minimumY) * 0.15f;

            minimumY -= verticalMargin;
            maximumY += verticalMargin;
        }
    }

    private void DrawGrid(Rect rect)
    {
        Handles.BeginGUI();

        Handles.color =
            new Color(1f, 1f, 1f, 0.10f);

        const int divisions = 5;

        for (int i = 0; i <= divisions; i++)
        {
            float fraction = i / (float)divisions;

            float x = Mathf.Lerp(
                rect.xMin,
                rect.xMax,
                fraction
            );

            float y = Mathf.Lerp(
                rect.yMin,
                rect.yMax,
                fraction
            );

            Handles.DrawLine(
                new Vector3(x, rect.yMin),
                new Vector3(x, rect.yMax)
            );

            Handles.DrawLine(
                new Vector3(rect.xMin, y),
                new Vector3(rect.xMax, y)
            );
        }

        Handles.EndGUI();
    }

    private void DrawZeroAxes(
        Rect rect,
        float minimumX,
        float maximumX,
        float minimumY,
        float maximumY
    )
    {
        Handles.BeginGUI();

        Handles.color =
            new Color(1f, 1f, 1f, 0.45f);

        if (minimumY <= 0f && maximumY >= 0f)
        {
            float normalizedY =
                Mathf.InverseLerp(
                    minimumY,
                    maximumY,
                    0f
                );

            float screenY = Mathf.Lerp(
                rect.yMax,
                rect.yMin,
                normalizedY
            );

            Handles.DrawLine(
                new Vector3(rect.xMin, screenY),
                new Vector3(rect.xMax, screenY)
            );
        }

        if (minimumX <= 0f && maximumX >= 0f)
        {
            float normalizedX =
                Mathf.InverseLerp(
                    minimumX,
                    maximumX,
                    0f
                );

            float screenX = Mathf.Lerp(
                rect.xMin,
                rect.xMax,
                normalizedX
            );

            Handles.DrawLine(
                new Vector3(screenX, rect.yMin),
                new Vector3(screenX, rect.yMax)
            );
        }

        Handles.EndGUI();
    }

    private void DrawCurve(
        ParameterRelationSO relation,
        Rect rect,
        float minimumX,
        float maximumX,
        float minimumY,
        float maximumY
    )
    {
        Vector3[] curvePoints =
            new Vector3[SampleCount];

        for (int i = 0; i < SampleCount; i++)
        {
            float interpolation =
                i / (float)(SampleCount - 1);

            float sourceValue = Mathf.Lerp(
                minimumX,
                maximumX,
                interpolation
            );

            float impact =
                relation.Evaluate(sourceValue);

            curvePoints[i] = ConvertToScreenPosition(
                rect,
                sourceValue,
                impact,
                minimumX,
                maximumX,
                minimumY,
                maximumY
            );
        }

        Handles.BeginGUI();

        Handles.color = Color.cyan;

        Handles.DrawAAPolyLine(
            3f,
            curvePoints
        );

        Handles.EndGUI();
    }

    private void DrawKeyPoints(
        ParameterRelationSO relation,
        Rect rect,
        float minimumX,
        float maximumX,
        float minimumY,
        float maximumY
    )
    {
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 10
        };

        labelStyle.normal.textColor = Color.white;

        foreach (RelationPoint point in relation.Points)
        {
            Vector3 position = ConvertToScreenPosition(
                rect,
                point.SourceValue,
                point.ImpactPerTurn,
                minimumX,
                maximumX,
                minimumY,
                maximumY
            );

            // Marcador del punto.
            Rect markerRect = new Rect(
                position.x - 4f,
                position.y - 4f,
                8f,
                8f
            );

            EditorGUI.DrawRect(markerRect, Color.yellow);

            string label =
                $"({point.SourceValue:0.##}, " +
                $"{point.ImpactPerTurn:0.##})";

            // Texto situado a la derecha y ligeramente por encima.
            Rect labelRect = new Rect(
                position.x + 7f,
                position.y - 18f,
                100f,
                18f
            );

            // Fondo para mejorar la legibilidad.
            EditorGUI.DrawRect(
                labelRect,
                new Color(0f, 0f, 0f, 0.65f)
            );

            GUI.Label(
                labelRect,
                label,
                labelStyle
            );
        }
    }

    private Vector3 ConvertToScreenPosition(
        Rect rect,
        float sourceValue,
        float impact,
        float minimumX,
        float maximumX,
        float minimumY,
        float maximumY
    )
    {
        float normalizedX = Mathf.InverseLerp(
            minimumX,
            maximumX,
            sourceValue
        );

        float normalizedY = Mathf.InverseLerp(
            minimumY,
            maximumY,
            impact
        );

        return new Vector3(
            Mathf.Lerp(
                rect.xMin,
                rect.xMax,
                normalizedX
            ),
            Mathf.Lerp(
                rect.yMax,
                rect.yMin,
                normalizedY
            ),
            0f
        );
    }

    private void DrawRangeLabels(
        Rect rect,
        float minimumX,
        float maximumX,
        float minimumY,
        float maximumY
    )
    {
        GUI.Label(
            new Rect(
                rect.x + 6f,
                rect.y + 4f,
                rect.width - 12f,
                20f
            ),
            $"X: {minimumX:0.##} → {maximumX:0.##}    " +
            $"Y: {minimumY:0.##} → {maximumY:0.##}",
            EditorStyles.miniLabel
        );
    }

    private void HandleMouseZoom(Rect graphRect)
    {
        Event currentEvent = Event.current;

        if (
            currentEvent.type != EventType.ScrollWheel ||
            !graphRect.Contains(currentEvent.mousePosition)
        )
        {
            return;
        }

        float zoomMultiplier = Mathf.Pow(
            1.15f,
            -currentEvent.delta.y
        );

        graphZoom = Mathf.Clamp(
            graphZoom * zoomMultiplier,
            MinimumZoom,
            MaximumZoom
        );

        currentEvent.Use();
        Repaint();
    }

    private void ApplyViewTransform(
        ref float minimumX,
        ref float maximumX,
        ref float minimumY,
        ref float maximumY
    )
    {
        float centerX =
            (minimumX + maximumX) * 0.5f +
            graphPan.x;

        float centerY =
            (minimumY + maximumY) * 0.5f +
            graphPan.y;

        float halfWidth =
            (maximumX - minimumX) *
            0.5f /
            graphZoom;

        float halfHeight =
            (maximumY - minimumY) *
            0.5f /
            graphZoom;

        minimumX = centerX - halfWidth;
        maximumX = centerX + halfWidth;

        minimumY = centerY - halfHeight;
        maximumY = centerY + halfHeight;
    }

    private void HandleGraphNavigation(
        Rect graphRect,
        float baseMinimumX,
        float baseMaximumX,
        float baseMinimumY,
        float baseMaximumY
    )
    {
        Event currentEvent = Event.current;

        int controlId = GUIUtility.GetControlID(
            FocusType.Passive,
            graphRect
        );

        EditorGUIUtility.AddCursorRect(
            graphRect,
            MouseCursor.Pan,
            controlId
        );

        // Zoom con la rueda.
        if (
            currentEvent.type == EventType.ScrollWheel &&
            graphRect.Contains(currentEvent.mousePosition)
        )
        {
            float zoomMultiplier = Mathf.Pow(
                1.15f,
                -currentEvent.delta.y
            );

            graphZoom = Mathf.Clamp(
                graphZoom * zoomMultiplier,
                MinimumZoom,
                MaximumZoom
            );

            currentEvent.Use();
            Repaint();
            return;
        }

        // Empieza el desplazamiento.
        if (
            currentEvent.type == EventType.MouseDown &&
            currentEvent.button == 0 &&
            graphRect.Contains(currentEvent.mousePosition)
        )
        {
            GUIUtility.hotControl = controlId;

            currentEvent.Use();
            Repaint();
            return;
        }

        // Desplaza la vista mientras se mantiene pulsado.
        if (
            currentEvent.type == EventType.MouseDrag &&
            currentEvent.button == 0 &&
            GUIUtility.hotControl == controlId
        )
        {
            float visibleWidth =
                (baseMaximumX - baseMinimumX) / graphZoom;

            float visibleHeight =
                (baseMaximumY - baseMinimumY) / graphZoom;

            graphPan.x -=
                currentEvent.delta.x /
                graphRect.width *
                visibleWidth;

            graphPan.y +=
                currentEvent.delta.y /
                graphRect.height *
                visibleHeight;

            currentEvent.Use();
            Repaint();
            return;
        }

        // Termina el desplazamiento.
        if (
            currentEvent.type == EventType.MouseUp &&
            currentEvent.button == 0 &&
            GUIUtility.hotControl == controlId
        )
        {
            GUIUtility.hotControl = 0;

            currentEvent.Use();
            Repaint();
        }
    }
}