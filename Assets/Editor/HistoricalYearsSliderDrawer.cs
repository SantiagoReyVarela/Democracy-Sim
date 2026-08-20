using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HistoricalYearSliderAttribute))]
public class HistoricalYearSliderDrawer : PropertyDrawer
{
    private const float ValueFieldWidth = 55f;
    private const float Spacing = 5f;
    private const float MarkerHeight = 20f;

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    )
    {
        HistoricalYearSliderAttribute slider =
            (HistoricalYearSliderAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;

        // =====================================================
        // LABEL
        // =====================================================

        Rect labelRect = new Rect(
            position.x,
            position.y,
            EditorGUIUtility.labelWidth,
            lineHeight
        );

        EditorGUI.LabelField(labelRect, label);


        // =====================================================
        // VALUE FIELD
        // =====================================================

        Rect valueRect = new Rect(
            position.xMax - ValueFieldWidth,
            position.y,
            ValueFieldWidth,
            lineHeight
        );


        // =====================================================
        // SLIDER
        // =====================================================

        Rect sliderRect = new Rect(
            position.x + EditorGUIUtility.labelWidth,
            position.y,
            position.width
                - EditorGUIUtility.labelWidth
                - ValueFieldWidth
                - Spacing,
            lineHeight
        );


        // Clamp por seguridad.
        int currentValue = Mathf.Clamp(
            property.intValue,
            slider.minYear,
            slider.maxYear
        );

        // Dibujamos el slider manualmente.
        float sliderValue = GUI.HorizontalSlider(
            sliderRect,
            currentValue,
            slider.minYear,
            slider.maxYear
        );

        currentValue = Mathf.RoundToInt(sliderValue);


        // Campo numérico a la derecha.
        int typedValue = EditorGUI.IntField(
            valueRect,
            currentValue
        );

        currentValue = Mathf.Clamp(
            typedValue,
            slider.minYear,
            slider.maxYear
        );

        property.intValue = currentValue;


        // =====================================================
        // MARKERS
        // =====================================================

        Rect markerRect = new Rect(
            sliderRect.x,
            position.y + lineHeight + 2f,
            sliderRect.width,
            MarkerHeight
        );

        if (slider.markerYears != null)
        {
            foreach (int year in slider.markerYears)
            {
                DrawMarker(
                    markerRect,
                    year,
                    slider.minYear,
                    slider.maxYear
                );
            }
        }

        EditorGUI.EndProperty();
    }


    private void DrawMarker(
        Rect rect,
        int year,
        int minYear,
        int maxYear
    )
    {
        // No dibujar años fuera del rango.
        if (year < minYear || year > maxYear)
            return;

        float normalizedPosition =
            Mathf.InverseLerp(
                minYear,
                maxYear,
                year
            );

        float x =
            Mathf.Lerp(
                rect.x,
                rect.xMax,
                normalizedPosition
            );


        // =====================================================
        // VERTICAL MARKER
        // =====================================================

        Rect lineRect = new Rect(
            x - 0.5f,
            rect.y,
            1f,
            5f
        );

        EditorGUI.DrawRect(
            lineRect,
            new Color(
                0.6f,
                0.6f,
                0.6f,
                1f
            )
        );


        // =====================================================
        // YEAR TEXT
        // =====================================================

        GUIStyle style =
            new GUIStyle(EditorStyles.miniLabel);

        style.alignment =
            TextAnchor.UpperCenter;

        Rect yearRect = new Rect(
            x - 25f,
            rect.y + 5f,
            50f,
            15f
        );

        GUI.Label(
            yearRect,
            year.ToString(),
            style
        );
    }


    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label
    )
    {
        return
            EditorGUIUtility.singleLineHeight
            + MarkerHeight
            + 2f;
    }
}