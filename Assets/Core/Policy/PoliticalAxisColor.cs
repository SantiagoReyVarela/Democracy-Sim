using UnityEngine;

/// <summary>
/// Define los colores visuales correspondientes
/// a cada eje de políticas.
/// </summary>
public static class PoliticalAxisColors
{
    public static Color GetBackgroundColor(
        PoliticalAxis axis
    )
    {
        switch (axis)
        {
            case PoliticalAxis.Economy:
                return FromHex("#F3D76A"); // Amarillo

            case PoliticalAxis.Government:
                return FromHex("#F2A65A"); // Naranja

            case PoliticalAxis.Military:
                return FromHex("#7FC486"); // Verde

            case PoliticalAxis.Trade:
                return FromHex("#E59BC8"); // Rosa

            case PoliticalAxis.Society:
                return FromHex("#E98282"); // Rojo claro

            case PoliticalAxis.Immigration:
                return FromHex("#BF9169"); // Marrón claro

            case PoliticalAxis.Religion:
                return FromHex("#94D2F4"); // Azul claro

            default:
                return FromHex("#C4C7CE"); // Gris
        }
    }


    public static Color GetAccentColor(
        PoliticalAxis axis
    )
    {
        switch (axis)
        {
            case PoliticalAxis.Economy:
                return FromHex("#C99E00");

            case PoliticalAxis.Government:
                return FromHex("#D66A00");

            case PoliticalAxis.Military:
                return FromHex("#2E8438");

            case PoliticalAxis.Trade:
                return FromHex("#C33B91");

            case PoliticalAxis.Society:
                return FromHex("#B72C2C");

            case PoliticalAxis.Immigration:
                return FromHex("#79502D");

            case PoliticalAxis.Religion:
                return FromHex("#2A83B8");

            default:
                return FromHex("#70747C");
        }
    }


    private static Color FromHex(string hex)
    {
        if (
            ColorUtility.TryParseHtmlString(
                hex,
                out Color color
            )
        )
        {
            return color;
        }

        return Color.gray;
    }
}