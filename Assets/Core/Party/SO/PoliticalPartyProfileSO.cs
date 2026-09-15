using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPoliticalPartyProfile",
    menuName = "Democracy Simulator/Political Party Profile"
)]
public class PoliticalPartyProfileSO : ScriptableObject
{
    [Header("Base Party")]
    [SerializeField] private PoliticalPartySO baseParty;
    public PoliticalPartySO BaseParty => baseParty;

    [Header("Historical Identity")]
    [SerializeField] private string partyId;
    [SerializeField] private string partyName;
    [SerializeField] private string acronym;
    [SerializeField] private string country;
    [SerializeField] private Sprite logo;
    [SerializeField] private Color partyColor = Color.white;
    [TextArea(2, 6)] [SerializeField] private string description;
    [SerializeField] private string ideologicalLine;

    public string PartyId => partyId;
    public string PartyName =>
        !string.IsNullOrWhiteSpace(partyName)
            ? partyName
            : baseParty != null ? baseParty.partyName : "Unknown Party";
    public string Acronym =>
        !string.IsNullOrWhiteSpace(acronym)
            ? acronym
            : baseParty != null ? baseParty.acronym : string.Empty;
    public string Country => country;
    public Sprite Logo =>
        logo != null ? logo : baseParty != null ? baseParty.logo : null;
    public Color PartyColor => partyColor;
    public string Description =>
        !string.IsNullOrWhiteSpace(description)
            ? description
            : baseParty != null ? baseParty.description : string.Empty;
    public string IdeologicalLine =>
        !string.IsNullOrWhiteSpace(ideologicalLine)
            ? ideologicalLine
            : baseParty != null ? baseParty.IdeologicalLine : string.Empty;

    [Header("Ideological Overrides")]
    [Tooltip("Añade únicamente los ejes en los que este partido histórico se aparta de su partido base.")]
    [SerializeField] private List<PartyProfileIdeologicalOverride> ideologicalOverrides =
        new List<PartyProfileIdeologicalOverride>();

    public IReadOnlyList<PartyProfileIdeologicalOverride> IdeologicalOverrides =>
        ideologicalOverrides;

    [Header("Hemicycle Overrides")]
    [SerializeField] private bool overrideHemicycleBloc = false;
    [SerializeField] private HemicycleBloc hemicycleBloc;
    [SerializeField] private bool overrideRadicalizationLayer = false;
    [SerializeField] private PoliticalRadicalizationLayer radicalizationLayer =
        PoliticalRadicalizationLayer.Mainstream;

    public HemicycleBloc HemicycleBloc =>
        overrideHemicycleBloc
            ? hemicycleBloc
            : baseParty != null ? baseParty.HemicycleBloc : hemicycleBloc;

    public PoliticalRadicalizationLayer RadicalizationLayer =>
        overrideRadicalizationLayer
            ? radicalizationLayer
            : baseParty != null ? baseParty.RadicalizationLayer : radicalizationLayer;

    public float GetPosition(PoliticalAxis axis)
    {
        if (TryGetIdeologicalOverride(axis, out float overrideValue))
            return overrideValue;

        if (baseParty != null)
            return baseParty.GetPosition(axis);

        return 0f;
    }

    public bool TryGetIdeologicalOverride(PoliticalAxis axis, out float value)
    {
        if (ideologicalOverrides != null)
        {
            for (int i = ideologicalOverrides.Count - 1; i >= 0; i--)
            {
                PartyProfileIdeologicalOverride entry = ideologicalOverrides[i];

                if (entry != null && entry.axis == axis)
                {
                    value = Mathf.Clamp(entry.position, -5f, 5f);
                    return true;
                }
            }
        }

        value = 0f;
        return false;
    }

    public bool HasIdeologicalOverride(PoliticalAxis axis)
    {
        return TryGetIdeologicalOverride(axis, out _);
    }
}

[Serializable]
public class PartyProfileIdeologicalOverride
{
    public PoliticalAxis axis;

    [Range(-5f, 5f)]
    public float position;
}
