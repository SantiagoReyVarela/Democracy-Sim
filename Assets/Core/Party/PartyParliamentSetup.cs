using System;
using UnityEngine;

[Serializable]
public sealed class PartyParliamentSetup
{
    [SerializeField]
    private PoliticalPartySO party;

    [SerializeField, Min(0)]
    private int seats;

    [SerializeField]
    private bool isGovernment;

    public PoliticalPartySO Party => party;

    public int Seats => seats;

    public bool IsGovernment => isGovernment;
}