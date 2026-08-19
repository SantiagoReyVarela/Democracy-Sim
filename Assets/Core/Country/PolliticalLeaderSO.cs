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
    // IMAGEN
    // =========================================================

    [Header("Visuals")]

    [SerializeField]
    private Sprite portrait;


    // =========================================================
    // ACCESO PÚBLICO
    // =========================================================

    public string LeaderName =>
        leaderName;

    public PoliticalPartySO Party =>
        party;

    public Sprite Portrait =>
        portrait;
}