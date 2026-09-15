using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoliticalPartyProfileSO))]
public class PoliticalPartyProfileSOEditor : Editor
{
    private SerializedProperty basePartyProperty;
    private SerializedProperty partyIdProperty;
    private SerializedProperty partyNameProperty;
    private SerializedProperty acronymProperty;
    private SerializedProperty countryProperty;
    private SerializedProperty logoProperty;
    private SerializedProperty partyColorProperty;
    private SerializedProperty descriptionProperty;
    private SerializedProperty ideologicalLineProperty;
    private SerializedProperty ideologicalOverridesProperty;
    private SerializedProperty overrideHemicycleBlocProperty;
    private SerializedProperty hemicycleBlocProperty;
    private SerializedProperty overrideRadicalizationLayerProperty;
    private SerializedProperty radicalizationLayerProperty;

    private GUIStyle valueStyle;
    private GUIStyle baseStyle;
    private GUIStyle overrideStyle;
    private GUIStyle tendencyStyle;

    private void OnEnable()
    {
        basePartyProperty = serializedObject.FindProperty("baseParty");
        partyIdProperty = serializedObject.FindProperty("partyId");
        partyNameProperty = serializedObject.FindProperty("partyName");
        acronymProperty = serializedObject.FindProperty("acronym");
        countryProperty = serializedObject.FindProperty("country");
        logoProperty = serializedObject.FindProperty("logo");
        partyColorProperty = serializedObject.FindProperty("partyColor");
        descriptionProperty = serializedObject.FindProperty("description");
        ideologicalLineProperty = serializedObject.FindProperty("ideologicalLine");
        ideologicalOverridesProperty = serializedObject.FindProperty("ideologicalOverrides");
        overrideHemicycleBlocProperty = serializedObject.FindProperty("overrideHemicycleBloc");
        hemicycleBlocProperty = serializedObject.FindProperty("hemicycleBloc");
        overrideRadicalizationLayerProperty = serializedObject.FindProperty("overrideRadicalizationLayer");
        radicalizationLayerProperty = serializedObject.FindProperty("radicalizationLayer");

        valueStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        baseStyle = new GUIStyle(EditorStyles.miniLabel);

        overrideStyle = new GUIStyle(EditorStyles.miniBoldLabel);

        tendencyStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField(
                "Script",
                MonoScript.FromScriptableObject((PoliticalPartyProfileSO)target),
                typeof(PoliticalPartyProfileSO),
                false
            );
        }

        EditorGUILayout.Space(6f);

        // =====================================================
        // PARTIDO BASE
        // =====================================================

        DrawSectionTitle("Base Party");
        EditorGUILayout.PropertyField(basePartyProperty);

        EditorGUILayout.Space(10f);

        // =====================================================
        // IDENTIDAD HISTÓRICA
        // =====================================================

        DrawSectionTitle("Historical Identity");

        EditorGUILayout.PropertyField(partyIdProperty);
        EditorGUILayout.PropertyField(partyNameProperty);
        EditorGUILayout.PropertyField(acronymProperty);
        EditorGUILayout.PropertyField(countryProperty);
        EditorGUILayout.PropertyField(logoProperty);
        EditorGUILayout.PropertyField(partyColorProperty);
        EditorGUILayout.PropertyField(descriptionProperty);
        EditorGUILayout.PropertyField(ideologicalLineProperty);

        EditorGUILayout.Space(12f);

        // =====================================================
        // OVERRIDES IDEOLÓGICOS
        // =====================================================

        DrawSectionTitle("Ideological Overrides");

        EditorGUILayout.HelpBox(
            "Añade únicamente los ejes que quieras cambiar respecto al partido base. " +
            "Los demás se heredan automáticamente.",
            MessageType.Info
        );

        EditorGUILayout.PropertyField(
            ideologicalOverridesProperty,
            true
        );

        /*
         * Aplicamos aquí los cambios para que el resumen inferior
         * refleje inmediatamente cualquier override modificado.
         */
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(12f);

        // =====================================================
        // RESUMEN IDEOLÓGICO EFECTIVO
        // =====================================================

        DrawEffectiveIdeologicalSummary(
            (PoliticalPartyProfileSO)target
        );

        EditorGUILayout.Space(12f);

        // =====================================================
        // HEMICICLO
        // =====================================================

        serializedObject.Update();

        DrawSectionTitle("Hemicycle Overrides");

        EditorGUILayout.PropertyField(
            overrideHemicycleBlocProperty
        );

        if (overrideHemicycleBlocProperty.boolValue)
        {
            EditorGUILayout.PropertyField(
                hemicycleBlocProperty
            );
        }

        EditorGUILayout.PropertyField(
            overrideRadicalizationLayerProperty
        );

        if (overrideRadicalizationLayerProperty.boolValue)
        {
            EditorGUILayout.PropertyField(
                radicalizationLayerProperty
            );
        }

        serializedObject.ApplyModifiedProperties();
    }

    // =========================================================
    // RESUMEN IDEOLÓGICO
    // =========================================================

    private void DrawEffectiveIdeologicalSummary(
        PoliticalPartyProfileSO profile
    )
    {
        DrawSectionTitle(
            "Effective Ideological Summary"
        );

        if (
            profile == null ||
            profile.BaseParty == null
        )
        {
            EditorGUILayout.HelpBox(
                "Asigna un Base Party para calcular el resumen ideológico.",
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        EditorGUILayout.LabelField(
            profile.PartyName,
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            $"Base: {profile.BaseParty.partyName}",
            EditorStyles.miniLabel
        );

        EditorGUILayout.Space(6f);

        // Cabecera de columnas.
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            "Eje",
            EditorStyles.miniBoldLabel,
            GUILayout.MinWidth(145f)
        );

        EditorGUILayout.LabelField(
            "Valor",
            EditorStyles.miniBoldLabel,
            GUILayout.Width(42f)
        );

        EditorGUILayout.LabelField(
            "Extremo más cercano",
            EditorStyles.miniBoldLabel,
            GUILayout.MinWidth(145f)
        );

        EditorGUILayout.LabelField(
            "Origen",
            EditorStyles.miniBoldLabel,
            GUILayout.Width(68f)
        );

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2f);

        System.Array axes =
            System.Enum.GetValues(
                typeof(PoliticalAxis)
            );

        foreach (
            PoliticalAxis axis
            in axes
        )
        {
            float value =
                profile.GetPosition(
                    axis
                );

            bool overridden =
                profile.HasIdeologicalOverride(
                    axis
                );

            DrawSummaryRow(
                axis,
                value,
                overridden
            );
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSummaryRow(
        PoliticalAxis axis,
        float value,
        bool overridden
    )
    {
        EditorGUILayout.BeginHorizontal();

        // EJE
        EditorGUILayout.LabelField(
            GetAxisDisplayName(axis),
            GUILayout.MinWidth(145f)
        );

        // VALOR
        EditorGUILayout.LabelField(
            FormatValue(value),
            valueStyle,
            GUILayout.Width(42f)
        );

        // EXTREMO MÁS CERCANO
        EditorGUILayout.LabelField(
            GetNearestExtremeLabel(
                axis,
                value
            ),
            tendencyStyle,
            GUILayout.MinWidth(145f)
        );

        // ORIGEN
        EditorGUILayout.LabelField(
            overridden
                ? "OVERRIDE"
                : "BASE",
            overridden
                ? overrideStyle
                : baseStyle,
            GUILayout.Width(68f)
        );

        EditorGUILayout.EndHorizontal();
    }

    // =========================================================
    // EXTREMO MÁS CERCANO
    // =========================================================

    private string GetNearestExtremeLabel(
        PoliticalAxis axis,
        float value
    )
    {
        /*
         * La lógica que queremos es deliberadamente simple:
         *
         * valor < 0  -> extremo negativo
         * valor > 0  -> extremo positivo
         * valor == 0 -> equilibrado
         *
         * Ejemplos:
         * Economy -1 -> Economía planificada
         * Economy +1 -> Laissez-faire
         * Economy  0 -> Equilibrado
         */

        if (
            Mathf.Approximately(
                value,
                0f
            )
        )
        {
            return "Equilibrado";
        }

        if (value < 0f)
        {
            return
                PoliticalAxisExtremeLabels
                    .GetNegativeLabel(axis);
        }

        return
            PoliticalAxisExtremeLabels
                .GetPositiveLabel(axis);
    }

    private string FormatValue(
        float value
    )
    {
        if (
            Mathf.Approximately(
                value,
                Mathf.Round(value)
            )
        )
        {
            return
                Mathf.RoundToInt(value)
                    .ToString("+0;-0;0");
        }

        return
            value.ToString("+0.0;-0.0;0.0");
    }

    // =========================================================
    // NOMBRES DE LOS EJES
    // =========================================================

    private string GetAxisDisplayName(
        PoliticalAxis axis
    )
    {
        switch (axis)
        {
            case PoliticalAxis.Government:
                return "Government";

            case PoliticalAxis.Religion:
                return "Religion";

            case PoliticalAxis.Economy:
                return "Economy";

            case PoliticalAxis.Trade:
                return "Trade";

            case PoliticalAxis.Society:
                return "Society";

            case PoliticalAxis.Immigration:
                return "Immigration";

            case PoliticalAxis.Military:
                return "Military";

            case PoliticalAxis.NationalIdentity:
                return "National Identity";

            case PoliticalAxis.Environment:
                return "Environment";

            case PoliticalAxis.Welfare:
                return "Welfare";

            case PoliticalAxis.CriminalJustice:
                return "Criminal Justice";

            case PoliticalAxis.Taxation:
                return "Taxation";

            case PoliticalAxis.Labour:
                return "Labour";

            case PoliticalAxis.TerritorialOrganization:
                return "Territorial Organization";

            case PoliticalAxis.Radicalization:
                return "Radicalization";

            default:
                return axis.ToString();
        }
    }

    private void DrawSectionTitle(
        string title
    )
    {
        EditorGUILayout.LabelField(
            title,
            EditorStyles.boldLabel
        );
    }
}