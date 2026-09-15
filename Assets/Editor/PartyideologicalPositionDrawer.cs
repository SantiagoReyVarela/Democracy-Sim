using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PartyIdeologicalPosition))]
public class PartyIdeologicalPositionDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 2f;

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label
    )
    {
        float line =
            EditorGUIUtility.singleLineHeight;

        return
            (line * 3f) +
            (VerticalSpacing * 4f);
    }


    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    )
    {
        SerializedProperty axisProperty =
            property.FindPropertyRelative("axis");

        SerializedProperty positionProperty =
            property.FindPropertyRelative("position");


        EditorGUI.BeginProperty(
            position,
            label,
            property
        );


        float lineHeight =
            EditorGUIUtility.singleLineHeight;

        float y =
            position.y +
            VerticalSpacing;


        // =====================================================
        // AXIS
        // =====================================================

        Rect axisRect =
            new Rect(
                position.x,
                y,
                position.width,
                lineHeight
            );

        EditorGUI.PropertyField(
            axisRect,
            axisProperty
        );


        y +=
            lineHeight +
            VerticalSpacing;


        // =====================================================
        // POSITION
        // =====================================================

        Rect sliderRect =
            new Rect(
                position.x,
                y,
                position.width,
                lineHeight
            );


        float currentValue =
            positionProperty.floatValue;


        currentValue =
            EditorGUI.Slider(
                sliderRect,
                "Position",
                currentValue,
                -5f,
                5f
            );


        positionProperty.floatValue =
            currentValue;


        y +=
            lineHeight +
            VerticalSpacing;


        // =====================================================
        // EXTREMOS IDEOLÓGICOS
        // =====================================================

        PoliticalAxis axis =
            (PoliticalAxis)
            axisProperty.enumValueIndex;


        string negativeLabel =
            PoliticalAxisExtremeLabels
                .GetNegativeLabel(axis);

        string positiveLabel =
            PoliticalAxisExtremeLabels
                .GetPositiveLabel(axis);


        // -----------------------------------------------------
        // MUY IMPORTANTE:
        //
        // Las etiquetas empiezan donde empieza realmente
        // el control del slider, no al principio del Inspector.
        // -----------------------------------------------------

        float controlX =
            position.x +
            EditorGUIUtility.labelWidth;

        float controlWidth =
            position.width -
            EditorGUIUtility.labelWidth;


        // Dejamos espacio a la derecha para el campo numérico
        // que Unity dibuja junto al slider.
        const float numericFieldWidth = 52f;

        float ideologicalBarWidth =
            controlWidth -
            numericFieldWidth;


        Rect labelsArea =
            new Rect(
                controlX,
                y,
                ideologicalBarWidth,
                lineHeight
            );


        float halfWidth =
            labelsArea.width / 2f;


        Rect negativeRect =
            new Rect(
                labelsArea.x,
                labelsArea.y,
                halfWidth,
                lineHeight
            );


        Rect positiveRect =
            new Rect(
                labelsArea.x + halfWidth,
                labelsArea.y,
                halfWidth,
                lineHeight
            );


        // =====================================================
        // ESTILOS
        // =====================================================

        GUIStyle negativeStyle =
            new GUIStyle(
                EditorStyles.miniLabel
            );

        negativeStyle.alignment =
            TextAnchor.MiddleLeft;


        GUIStyle positiveStyle =
            new GUIStyle(
                EditorStyles.miniLabel
            );

        positiveStyle.alignment =
            TextAnchor.MiddleRight;


        // =====================================================
        // DIBUJAR
        // =====================================================

        EditorGUI.LabelField(
            negativeRect,
            negativeLabel,
            negativeStyle
        );


        EditorGUI.LabelField(
            positiveRect,
            positiveLabel,
            positiveStyle
        );


        EditorGUI.EndProperty();
    }
}