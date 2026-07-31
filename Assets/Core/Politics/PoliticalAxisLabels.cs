using UnityEngine;

/// <summary>
/// Proporciona los nombres visibles de los ejes y sus posiciones.
/// </summary>
public static class PoliticalAxisLabels
{
    private static readonly string[] AxisNames =
    {
        "Política",
        "Religión",
        "Economía",
        "Comercio",
        "Sociedad",
        "Inmigración",
        "Política militar"
    };

    private static readonly string[][] PositionNames =
    {
        // Política
        new[]
        {
            "Minarquismo",
            "Estado limitado",
            "Democracia pluralista",
            "Autoritarismo",
            "Totalitarismo"
        },

        // Religión
        new[]
        {
            "Ateísmo",
            "Laicismo",
            "Neutralidad religiosa",
            "Preferencia religiosa",
            "Confesionalismo"
        },

        // Economía
        new[]
        {
            "Laissez-faire",
            "Economía de mercado",
            "Economía mixta",
            "Intervencionismo estatal",
            "Economía planificada"
        },

        // Comercio
        new[]
        {
            "Libre comercio",
            "Apertura comercial",
            "Comercio regulado",
            "Proteccionismo",
            "Autarquía"
        },

        // Sociedad
        new[]
        {
            "Progresismo",
            "Liberalismo social",
            "Moderación",
            "Conservadurismo moderado",
            "Conservadurismo"
        },

        // Inmigración
        new[]
        {
            "Fronteras abiertas",
            "Inmigración flexible",
            "Inmigración regulada",
            "Inmigración restrictiva",
            "Fronteras cerradas"
        },

        // Política militar
        new[]
        {
            "Pacifismo",
            "Defensa limitada",
            "Defensa equilibrada",
            "Intervencionismo",
            "Belicismo"
        }
    };

    public static string GetAxisName(PoliticalAxis axis)
    {
        int index = (int)axis;

        if (index < 0 || index >= AxisNames.Length)
        {
            return axis.ToString();
        }

        return AxisNames[index];
    }

    public static string GetPositionName(
        PoliticalAxis axis,
        int position
    )
    {
        int axisIndex = (int)axis;
        int positionIndex = Mathf.Clamp(position, 1, 5) - 1;

        if (
            axisIndex < 0 ||
            axisIndex >= PositionNames.Length
        )
        {
            return $"Posición {position}";
        }

        return PositionNames[axisIndex][positionIndex];
    }
}