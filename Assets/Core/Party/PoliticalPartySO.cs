using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPoliticalParty",
    menuName = "Democracy Simulator/Political Party")]
public class PoliticalPartySO : ScriptableObject
{
    [Header("Identity")]
    public string partyId;
    public string partyName;
    public string acronym;
    public Sprite logo;
    public Color partyColor;

    [TextArea]
    public string description;

    [Header("Ideology")]
    public List<PartyIdeologicalPosition> ideologicalPositions;
}

[Serializable]
public class PartyIdeologicalPosition
{
    public IdeologicalAxis axis;

    [Range(-5f, 5f)]
    public float position;
}