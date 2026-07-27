using System;
using UnityEngine;

/// <summary>
/// Copia inmutable del estado del país en un turno concreto.
/// Se utiliza para estadísticas, gráficas y resultados finales.
/// </summary>
[Serializable]
public sealed class CountrySnapshot
{
    [SerializeField]
    private int turn;

    [SerializeField]
    private float prosperity;

    [SerializeField]
    private float equality;

    [SerializeField]
    private float employment;

    [SerializeField]
    private float education;

    [SerializeField]
    private float stability;


    public int Turn => turn;

    public float Prosperity => prosperity;

    public float Equality => equality;

    public float Employment => employment;

    public float Education => education;

    public float Stability => stability;


    public CountrySnapshot(
        int turn,
        float prosperity,
        float equality,
        float employment,
        float education,
        float stability
    )
    {
        this.turn = turn;
        this.prosperity = prosperity;
        this.equality = equality;
        this.employment = employment;
        this.education = education;
        this.stability = stability;
    }


    public float GetParameter(CountryParameter parameter)
    {
        switch (parameter)
        {
            case CountryParameter.Prosperity:
                return prosperity;

            case CountryParameter.Equality:
                return equality;

            case CountryParameter.Employment:
                return employment;

            case CountryParameter.Education:
                return education;

            case CountryParameter.Stability:
                return stability;

            default:
                Debug.LogError(
                    $"CountrySnapshot: parámetro desconocido: {parameter}"
                );

                return 0f;
        }
    }
}