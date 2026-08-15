using UnityEngine;

[CreateAssetMenu(
    fileName = "PoliticalLeader",
    menuName = "Democracy Simulator/Political Leader"
)]
public class PoliticalLeaderSO : ScriptableObject
{
    [Header("Identity")]
    public string leaderName;

    public Sprite portrait;

    [Header("Party")]
    public PoliticalPartySO party;
}