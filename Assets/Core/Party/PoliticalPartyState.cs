using System;

[Serializable]
public class PoliticalPartyState
{
    public PoliticalPartySO party;

    public float popularity;
    public float satisfaction;

    public int seats;
    public bool isGovernment;
}