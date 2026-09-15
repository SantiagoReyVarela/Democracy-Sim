using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoliticalPartySO))]
public class PoliticalPartySOEditor : Editor
{
    private SerializedProperty enabledProperty;
    private SerializedProperty partyIdProperty;
    private SerializedProperty partyNameProperty;
    private SerializedProperty acronymProperty;
    private SerializedProperty logoProperty;
    private SerializedProperty partyColorProperty;
    private SerializedProperty descriptionProperty;
    private SerializedProperty ideologicalLineProperty;
    private SerializedProperty ideologicalPositionsProperty;
    private SerializedProperty hemicycleBlocProperty;
    private SerializedProperty radicalizationLayerProperty;

    private GUIStyle leftExtremeStyle;
    private GUIStyle rightExtremeStyle;

    private void OnEnable()
    {
        PoliticalPartySO party = target as PoliticalPartySO;
        if (party != null)
            party.EnsureIdeologicalPositions();

        enabledProperty = serializedObject.FindProperty("enabled");
        partyIdProperty = serializedObject.FindProperty("partyId");
        partyNameProperty = serializedObject.FindProperty("partyName");
        acronymProperty = serializedObject.FindProperty("acronym");
        logoProperty = serializedObject.FindProperty("logo");
        partyColorProperty = serializedObject.FindProperty("partyColor");
        descriptionProperty = serializedObject.FindProperty("description");
        ideologicalLineProperty = serializedObject.FindProperty("ideologicalLine");
        ideologicalPositionsProperty = serializedObject.FindProperty("ideologicalPositions");
        hemicycleBlocProperty = serializedObject.FindProperty("hemicycleBloc");
        radicalizationLayerProperty = serializedObject.FindProperty("radicalizationLayer");

        leftExtremeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft
        };

        rightExtremeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField(
                "Script",
                MonoScript.FromScriptableObject((PoliticalPartySO)target),
                typeof(PoliticalPartySO),
                false
            );
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.PropertyField(enabledProperty);
        EditorGUILayout.Space(8f);

        DrawSectionTitle("Identidad");
        EditorGUILayout.PropertyField(partyIdProperty);
        EditorGUILayout.PropertyField(partyNameProperty);
        EditorGUILayout.PropertyField(acronymProperty);
        EditorGUILayout.PropertyField(logoProperty);
        EditorGUILayout.PropertyField(partyColorProperty);
        EditorGUILayout.PropertyField(descriptionProperty);

        EditorGUILayout.Space(10f);

        DrawSectionTitle("Political identity");
        EditorGUILayout.PropertyField(ideologicalLineProperty);

        EditorGUILayout.Space(12f);

        DrawSectionTitle("Ideología");
        EditorGUILayout.HelpBox(
            "Los ejes del partido base son fijos. No es necesario añadir ni eliminar posiciones: solo mueve cada barra entre -5 y +5.",
            MessageType.Info
        );

        EditorGUILayout.Space(4f);
        DrawAllIdeologicalAxes();

        EditorGUILayout.Space(12f);

        DrawSectionTitle("Hemicycle");
        EditorGUILayout.PropertyField(hemicycleBlocProperty);
        EditorGUILayout.PropertyField(radicalizationLayerProperty);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAllIdeologicalAxes()
    {
        Array axes = Enum.GetValues(typeof(PoliticalAxis));

        if (
            ideologicalPositionsProperty == null ||
            !ideologicalPositionsProperty.isArray
        )
        {
            EditorGUILayout.HelpBox(
                "No se ha encontrado la lista interna de posiciones ideológicas.",
                MessageType.Error
            );
            return;
        }

        if (ideologicalPositionsProperty.arraySize != axes.Length)
        {
            serializedObject.ApplyModifiedProperties();

            PoliticalPartySO party = target as PoliticalPartySO;
            if (party != null)
            {
                Undo.RecordObject(party, "Repair ideological axes");
                party.EnsureIdeologicalPositions();
                EditorUtility.SetDirty(party);
            }

            serializedObject.Update();
        }

        int count = Mathf.Min(
            ideologicalPositionsProperty.arraySize,
            axes.Length
        );

        for (int i = 0; i < count; i++)
        {
            PoliticalAxis axis =
                (PoliticalAxis)axes.GetValue(i);

            SerializedProperty element =
                ideologicalPositionsProperty.GetArrayElementAtIndex(i);

            SerializedProperty positionProperty =
                element.FindPropertyRelative("position");

            DrawIdeologicalAxis(axis, positionProperty);

            if (i < count - 1)
                EditorGUILayout.Space(5f);
        }
    }

    private void DrawIdeologicalAxis(
        PoliticalAxis axis,
        SerializedProperty positionProperty
    )
    {
        if (positionProperty == null)
            return;

        EditorGUILayout.LabelField(
            GetAxisDisplayName(axis),
            EditorStyles.boldLabel
        );

        Rect sliderRect =
            EditorGUILayout.GetControlRect(
                false,
                EditorGUIUtility.singleLineHeight
            );

        float newValue = EditorGUI.Slider(
            sliderRect,
            GUIContent.none,
            positionProperty.floatValue,
            -5f,
            5f
        );

        positionProperty.floatValue =
            Mathf.Clamp(newValue, -5f, 5f);

        Rect labelsRect =
            EditorGUILayout.GetControlRect(
                false,
                EditorGUIUtility.singleLineHeight
            );

        // El Slider reserva una pequeña zona a la derecha para el valor numérico.
        const float numericFieldWidth = 52f;
        const float gap = 4f;

        float ideologicalBarWidth = Mathf.Max(
            40f,
            labelsRect.width - numericFieldWidth - gap
        );

        Rect barLabelsRect = new Rect(
            labelsRect.x,
            labelsRect.y,
            ideologicalBarWidth,
            labelsRect.height
        );

        float halfWidth = barLabelsRect.width * 0.5f;

        Rect leftRect = new Rect(
            barLabelsRect.x,
            barLabelsRect.y,
            halfWidth,
            barLabelsRect.height
        );

        Rect rightRect = new Rect(
            barLabelsRect.x + halfWidth,
            barLabelsRect.y,
            halfWidth,
            barLabelsRect.height
        );

        EditorGUI.LabelField(
            leftRect,
            PoliticalAxisExtremeLabels.GetNegativeLabel(axis),
            leftExtremeStyle
        );

        EditorGUI.LabelField(
            rightRect,
            PoliticalAxisExtremeLabels.GetPositiveLabel(axis),
            rightExtremeStyle
        );
    }

    private string GetAxisDisplayName(PoliticalAxis axis)
    {
        switch (axis)
        {
            case PoliticalAxis.Government: return "Government";
            case PoliticalAxis.Religion: return "Religion";
            case PoliticalAxis.Economy: return "Economy";
            case PoliticalAxis.Trade: return "Trade";
            case PoliticalAxis.Society: return "Society";
            case PoliticalAxis.Immigration: return "Immigration";
            case PoliticalAxis.Military: return "Military";
            case PoliticalAxis.NationalIdentity: return "National Identity";
            case PoliticalAxis.Environment: return "Environment";
            case PoliticalAxis.Welfare: return "Welfare";
            case PoliticalAxis.CriminalJustice: return "Criminal Justice";
            case PoliticalAxis.Taxation: return "Taxation";
            case PoliticalAxis.Labour: return "Labour";
            case PoliticalAxis.TerritorialOrganization: return "Territorial Organization";
            case PoliticalAxis.Radicalization: return "Radicalization";
            default: return axis.ToString();
        }
    }

    private void DrawSectionTitle(string title)
    {
        EditorGUILayout.LabelField(
            title,
            EditorStyles.boldLabel
        );
    }
}
