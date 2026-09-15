using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPoliticalParty",
    menuName = "Democracy Simulator/Political Party"
)]
public class PoliticalPartySO : ScriptableObject
{
    [SerializeField] private bool enabled = true;
    public bool Enabled => enabled;

    [Header("Identidad")]
    public string partyId;
    public string partyName;
    public string acronym;
    public Sprite logo;
    public Color partyColor;
    [TextArea] public string description;

    [Header("Political identity")]
    [SerializeField] private string ideologicalLine;
    public string IdeologicalLine => ideologicalLine;

    /*
     * Se conserva la lista internamente para no romper el resto del proyecto
     * ni perder los valores de assets existentes. El editor personalizado
     * la muestra como 15 barras fijas, sin botones Add/Remove ni selector de eje.
     */
    [SerializeField, HideInInspector]
    public List<PartyIdeologicalPosition> ideologicalPositions =
        new List<PartyIdeologicalPosition>();

    [Header("Hemicycle")]
    [SerializeField] private HemicycleBloc hemicycleBloc;
    [SerializeField] private PoliticalRadicalizationLayer radicalizationLayer =
        PoliticalRadicalizationLayer.Mainstream;

    public HemicycleBloc HemicycleBloc => hemicycleBloc;
    public PoliticalRadicalizationLayer RadicalizationLayer => radicalizationLayer;

    private void OnEnable() => EnsureIdeologicalPositions();
    private void OnValidate() => EnsureIdeologicalPositions();
    private void Reset() => EnsureIdeologicalPositions();

    public void EnsureIdeologicalPositions()
    {
        Array axes = Enum.GetValues(typeof(PoliticalAxis));
        Dictionary<PoliticalAxis, float> savedValues =
            new Dictionary<PoliticalAxis, float>();

        if (ideologicalPositions != null)
        {
            foreach (PartyIdeologicalPosition entry in ideologicalPositions)
            {
                if (entry == null)
                    continue;

                if (!Enum.IsDefined(typeof(PoliticalAxis), entry.axis))
                    continue;

                if (!savedValues.ContainsKey(entry.axis))
                {
                    savedValues.Add(
                        entry.axis,
                        Mathf.Clamp(entry.position, -5f, 5f)
                    );
                }
            }
        }

        bool needsRebuild =
            ideologicalPositions == null ||
            ideologicalPositions.Count != axes.Length;

        if (!needsRebuild)
        {
            for (int i = 0; i < axes.Length; i++)
            {
                PoliticalAxis expectedAxis =
                    (PoliticalAxis)axes.GetValue(i);

                PartyIdeologicalPosition current =
                    ideologicalPositions[i];

                if (current == null || current.axis != expectedAxis)
                {
                    needsRebuild = true;
                    break;
                }
            }
        }

        if (needsRebuild)
        {
            List<PartyIdeologicalPosition> rebuilt =
                new List<PartyIdeologicalPosition>(axes.Length);

            for (int i = 0; i < axes.Length; i++)
            {
                PoliticalAxis axis =
                    (PoliticalAxis)axes.GetValue(i);

                float value = 0f;
                if (savedValues.TryGetValue(axis, out float saved))
                    value = saved;

                rebuilt.Add(
                    new PartyIdeologicalPosition
                    {
                        axis = axis,
                        position = value
                    }
                );
            }

            ideologicalPositions = rebuilt;
        }
        else
        {
            foreach (PartyIdeologicalPosition entry in ideologicalPositions)
            {
                if (entry != null)
                    entry.position = Mathf.Clamp(entry.position, -5f, 5f);
            }
        }
    }

    public float GetPosition(PoliticalAxis axis)
    {
        EnsureIdeologicalPositions();

        foreach (PartyIdeologicalPosition entry in ideologicalPositions)
        {
            if (entry != null && entry.axis == axis)
                return entry.position;
        }

        return 0f;
    }

    public void SetPosition(PoliticalAxis axis, float value)
    {
        EnsureIdeologicalPositions();

        foreach (PartyIdeologicalPosition entry in ideologicalPositions)
        {
            if (entry != null && entry.axis == axis)
            {
                entry.position = Mathf.Clamp(value, -5f, 5f);
                return;
            }
        }
    }
}

[Serializable]
public class PartyIdeologicalPosition
{
    public PoliticalAxis axis;

    [Range(-5f, 5f)]
    public float position;
}

public static class PoliticalAxisExtremeLabels
{
    public static string GetNegativeLabel(PoliticalAxis axis)
    {
        switch (axis)
        {
            case PoliticalAxis.Government: return "Minarquismo";
            case PoliticalAxis.Religion: return "Ateísmo";
            case PoliticalAxis.Economy: return "Economía planificada";
            case PoliticalAxis.Trade: return "Libre comercio";
            case PoliticalAxis.Society: return "Progresismo";
            case PoliticalAxis.Immigration: return "Fronteras abiertas";
            case PoliticalAxis.Military: return "Pacifismo";
            case PoliticalAxis.NationalIdentity: return "Cosmopolitismo";
            case PoliticalAxis.Environment: return "Ecologismo";
            case PoliticalAxis.Welfare: return "Responsabilidad individual";
            case PoliticalAxis.CriminalJustice: return "Rehabilitación";
            case PoliticalAxis.Taxation: return "Redistribución";
            case PoliticalAxis.Labour: return "Poder sindical";
            case PoliticalAxis.TerritorialOrganization: return "Confederalismo";
            case PoliticalAxis.Radicalization: return "Status quo";
            default: return "Extremo negativo";
        }
    }

    public static string GetPositiveLabel(PoliticalAxis axis)
    {
        switch (axis)
        {
            case PoliticalAxis.Government: return "Totalitarismo";
            case PoliticalAxis.Religion: return "Confesionalismo";
            case PoliticalAxis.Economy: return "Laissez-faire";
            case PoliticalAxis.Trade: return "Proteccionismo";
            case PoliticalAxis.Society: return "Conservadurismo";
            case PoliticalAxis.Immigration: return "Cierre fronterizo";
            case PoliticalAxis.Military: return "Belicismo";
            case PoliticalAxis.NationalIdentity: return "Nacionalismo";
            case PoliticalAxis.Environment: return "Productivismo";
            case PoliticalAxis.Welfare: return "Estado del bienestar";
            case PoliticalAxis.CriminalJustice: return "Punitivismo";
            case PoliticalAxis.Taxation: return "Baja fiscalidad";
            case PoliticalAxis.Labour: return "Flexibilidad laboral";
            case PoliticalAxis.TerritorialOrganization: return "Centralismo";
            case PoliticalAxis.Radicalization: return "Revolución";
            default: return "Extremo positivo";
        }
    }
}