using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPoliticalParty",
    menuName = "Democracy Simulator/Political Party"
)]
public class PoliticalPartySO : ScriptableObject
{
    [SerializeField]
    private bool enabled = true;

    public bool Enabled => enabled;
    // =========================================================
    // IDENTIDAD
    // =========================================================

    [Header("Identidad")]

    public string partyId;

    public string partyName;

    public string acronym;

    public Sprite logo;

    public Color partyColor;

    [TextArea]
    public string description;


    // =========================================================
    // IDENTIDAD POLÍTICA
    // =========================================================

    [Header("Political identity")]

    [SerializeField]
    private string ideologicalLine;

    public string IdeologicalLine =>
        ideologicalLine;


    // =========================================================
    // IDEOLOGÍA
    // =========================================================

    [Header("Ideología")]

    public List<PartyIdeologicalPosition> ideologicalPositions =
        new List<PartyIdeologicalPosition>();


    // =========================================================
    // HEMICICLO
    // =========================================================

    [Header("Hemicycle")]

    [SerializeField]
    private HemicycleBloc hemicycleBloc;

    [SerializeField]
    private PoliticalRadicalizationLayer radicalizationLayer =
        PoliticalRadicalizationLayer.Mainstream;


    public HemicycleBloc HemicycleBloc =>
        hemicycleBloc;

    public PoliticalRadicalizationLayer RadicalizationLayer =>
        radicalizationLayer;


    // =========================================================
    // OBTENER POSICIÓN EN UN EJE IDEOLÓGICO
    // =========================================================

    public float GetPosition(
        PoliticalAxis axis
    )
    {
        if (ideologicalPositions == null)
        {
            Debug.LogWarning(
                $"El partido '{partyName}' " +
                $"no tiene posiciones ideológicas."
            );

            return 0f;
        }


        foreach (
            PartyIdeologicalPosition ideologicalPosition
            in ideologicalPositions
        )
        {
            if (
                ideologicalPosition.axis ==
                axis
            )
            {
                return
                    ideologicalPosition.position;
            }
        }


        Debug.LogWarning(
            $"El partido '{partyName}' " +
            $"no tiene definido el eje '{axis}'."
        );

        return 0f;
    }
}


// =============================================================
// POSICIÓN IDEOLÓGICA DEL PARTIDO
// =============================================================

[Serializable]
public class PartyIdeologicalPosition
{
    public PoliticalAxis axis;

    [Range(-5f, 5f)]
    public float position;
}