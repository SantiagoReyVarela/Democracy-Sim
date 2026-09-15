using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PartyProfileIdeologicalOverride))]
public class PartyProfileIdeologicalOverrideDrawer : PropertyDrawer
{
    private const float Gap = 2f;
    private const float LabelsHeight = 16f;

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label
    )
    {
        float line = EditorGUIUtility.singleLineHeight;
        return line + Gap + line + Gap + LabelsHeight;
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    )
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty axisProperty =
            property.FindPropertyRelative("axis");

        SerializedProperty positionProperty =
            property.FindPropertyRelative("position");

        float line = EditorGUIUtility.singleLineHeight;
        float y = position.y;

        Rect axisRect = new Rect(
            position.x,
            y,
            position.width,
            line
        );

        EditorGUI.PropertyField(
            axisRect,
            axisProperty,
            new GUIContent("Axis")
        );

        y += line + Gap;

        Rect sliderRect = new Rect(
            position.x,
            y,
            position.width,
            line
        );

        positionProperty.floatValue =
            EditorGUI.Slider(
                sliderRect,
                "Position",
                positionProperty.floatValue,
                -5f,
                5f
            );

        y += line + Gap;

        PoliticalAxis axis =
            (PoliticalAxis)axisProperty.enumValueIndex;

        float controlX =
            position.x + EditorGUIUtility.labelWidth;

        float controlWidth =
            position.width - EditorGUIUtility.labelWidth;

        const float numericFieldWidth = 52f;

        float barWidth = Mathf.Max(
            40f,
            controlWidth - numericFieldWidth
        );

        Rect labelsRect = new Rect(
            controlX,
            y,
            barWidth,
            LabelsHeight
        );

        float half = labelsRect.width * 0.5f;

        GUIStyle leftStyle =
            new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

        GUIStyle rightStyle =
            new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };

        EditorGUI.LabelField(
            new Rect(
                labelsRect.x,
                labelsRect.y,
                half,
                labelsRect.height
            ),
            PoliticalAxisExtremeLabels.GetNegativeLabel(axis),
            leftStyle
        );

        EditorGUI.LabelField(
            new Rect(
                labelsRect.x + half,
                labelsRect.y,
                half,
                labelsRect.height
            ),
            PoliticalAxisExtremeLabels.GetPositiveLabel(axis),
            rightStyle
        );

        EditorGUI.EndProperty();
    }
}
