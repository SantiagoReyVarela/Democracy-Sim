using System;
using UnityEngine;

/// <summary>
/// Copia del estado completo del país en un turno concreto.
/// </summary>
[Serializable]
public sealed class CountrySnapshot
{
    [Header("Turn")]

    [SerializeField]
    private int turn;


    [Header("General parameters")]

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


    [Header("Economic parameters")]

    [SerializeField]
    private float fiscalResult;

    [SerializeField]
    private float reserves;

    [SerializeField]
    private float debt;

    [SerializeField]
    private float inflation;


    public int Turn => turn;

    public float Prosperity => prosperity;
    public float Equality => equality;
    public float Employment => employment;
    public float Education => education;
    public float Stability => stability;

    public float FiscalResult => fiscalResult;
    public float Reserves => reserves;
    public float Debt => debt;
    public float Inflation => inflation;


    public CountrySnapshot(
        int turn,
        float prosperity,
        float equality,
        float employment,
        float education,
        float stability,
        float fiscalResult,
        float reserves,
        float debt,
        float inflation
    )
    {
        this.turn = turn;

        this.prosperity = prosperity;
        this.equality = equality;
        this.employment = employment;
        this.education = education;
        this.stability = stability;

        this.fiscalResult = fiscalResult;
        this.reserves = reserves;
        this.debt = debt;
        this.inflation = inflation;
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

            case CountryParameter.FiscalResult:
                return fiscalResult;

            case CountryParameter.Reserves:
                return reserves;

            case CountryParameter.Debt:
                return debt;

            case CountryParameter.Inflation:
                return inflation;

            default:
                Debug.LogError(
                    $"CountrySnapshot: parámetro desconocido: {parameter}"
                );

                return 0f;
        }
    }
}