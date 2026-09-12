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

    public Sprite Portrait =>
        portrait;
}