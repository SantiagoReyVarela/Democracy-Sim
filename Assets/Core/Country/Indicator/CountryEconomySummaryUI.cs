using TMPro;
using UnityEngine;

public class CountryEconomySummaryUI : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("Economy UI")]

    [SerializeField]
    private TMP_Text fiscalResultText;

    [SerializeField]
    private TMP_Text reservesText;

    [SerializeField]
    private TMP_Text debtText;

    [SerializeField]
    private TMP_Text inflationText;


    // =========================================================
    // COUNTRY STATE
    // =========================================================

    private CountryState countryState;


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        BindCountryState();

        Refresh();
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        BindCountryState();

        Refresh();
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        UnbindCountryState();
    }


    // =========================================================
    // CONECTAR COUNTRY STATE
    // =========================================================

    private void BindCountryState()
    {
        if (countryState == CountryState.Instance)
        {
            return;
        }


        UnbindCountryState();


        countryState =
            CountryState.Instance;


        if (countryState != null)
        {
            countryState.OnStateChanged +=
                Refresh;
        }
    }


    // =========================================================
    // DESCONECTAR COUNTRY STATE
    // =========================================================

    private void UnbindCountryState()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -=
                Refresh;
        }


        countryState =
            null;
    }


    // =========================================================
    // REFRESCAR
    // =========================================================

    public void Refresh()
    {
        if (countryState == null)
        {
            countryState =
                CountryState.Instance;
        }


        if (countryState == null)
        {
            return;
        }


        RefreshFiscalResult();
        RefreshReserves();
        RefreshDebt();
        RefreshInflation();
    }


    // =========================================================
    // RESULTADO FISCAL
    // =========================================================

    private void RefreshFiscalResult()
    {
        if (fiscalResultText == null)
        {
            return;
        }


        float value =
            countryState.FiscalResult;


        fiscalResultText.text =
            value.ToString(
                "+0.0;-0.0;0.0"
            );


        // -----------------------------------------------------
        // COLOR
        //
        // Positivo = superávit
        // Negativo = déficit
        // -----------------------------------------------------

        if (value > 0.01f)
        {
            fiscalResultText.color =
                new Color(
                    0.35f,
                    0.85f,
                    0.35f
                );
        }
        else if (value < -0.01f)
        {
            fiscalResultText.color =
                new Color(
                    0.9f,
                    0.3f,
                    0.3f
                );
        }
        else
        {
            fiscalResultText.color =
                Color.white;
        }
    }


    // =========================================================
    // RESERVAS
    // =========================================================

    private void RefreshReserves()
    {
        if (reservesText == null)
        {
            return;
        }


        reservesText.text =
            countryState.Reserves.ToString(
                "0.0"
            );
    }


    // =========================================================
    // DEUDA
    // =========================================================

    private void RefreshDebt()
    {
        if (debtText == null)
        {
            return;
        }


        debtText.text =
            countryState.Debt.ToString(
                "0.0"
            );
    }


    // =========================================================
    // INFLACIÓN
    // =========================================================

    private void RefreshInflation()
    {
        if (inflationText == null)
        {
            return;
        }


        inflationText.text =
            $"{countryState.Inflation:0.0}%";
    }
}