using UnityEngine;

[CreateAssetMenu(
    fileName = "PoliticalLeader",
    menuName = "Democracy Simulator/Political Leader"
)]
public class PoliticalLeaderSO : ScriptableObject
{
    // =========================================================
    // IDENTIDAD
    // =========================================================

    [Header("Identity")]

    [SerializeField]
    private string leaderName;


    // =========================================================
    // PARTIDO
    // =========================================================

    [Header("Party")]

    [SerializeField]
    private PoliticalPartySO party;


    // =========================================================
    // ACCESO PÚBLICO
    // =========================================================

    public string LeaderName =>
        leaderName;

    public PoliticalPartySO Party =>
        party;
}