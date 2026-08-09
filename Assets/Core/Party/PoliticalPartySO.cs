using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPoliticalParty",
    menuName = "Democracy Simulator/Political Party")]

public class PoliticalPartySO : ScriptableObject
{
    [Header("Identidad")]
    public string partyId;
    public string partyName;
    public string acronym;
    public Sprite logo;
    public Color partyColor;
    [TextArea]
    public string description;

    [Header("Political identity")]

    [SerializeField]
    private string ideologicalLine;

    public string IdeologicalLine => ideologicalLine;
    
    [Header("Ideología")]
    public List<PartyIdeologicalPosition> ideologicalPositions =
        new List<PartyIdeologicalPosition>();

    [Header("Parliament position")]

    [SerializeField, Range(-5f, 5f)]
    private float parliamentPosition;

    public float ParliamentPosition => parliamentPosition;

    [Header("Hemicycle")]

    [SerializeField]
    private HemicycleBloc hemicycleBloc;

    [SerializeField, Range(-5f, 5f)]
    private float hemicycleCenterPosition;

    public HemicycleBloc HemicycleBloc => hemicycleBloc;

    public float HemicycleCenterPosition => hemicycleCenterPosition;


    public float GetPosition(PoliticalAxis axis)
    {
        if (ideologicalPositions == null)
        {
            Debug.LogWarning(
                $"El partido '{partyName}' no tiene posiciones ideológicas."
            );

            return 0f;
        }

        foreach (PartyIdeologicalPosition ideologicalPosition
                 in ideologicalPositions)
        {
            if (ideologicalPosition.axis == axis)
            {
                return ideologicalPosition.position;
            }
        }

        Debug.LogWarning(
            $"El partido '{partyName}' no tiene definido el eje '{axis}'."
        );

        return 0f;
    }
}

[Serializable]
public class PartyIdeologicalPosition
{
    public PoliticalAxis  axis;

    [Range(-5f, 5f)]
    public float position;
}