using TMPro;
using UnityEngine;

public class CountryEconomySummaryUI : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("Economy Values")]

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
    // UNITY
    // =========================================================

    private void OnEnable()
    {
        BindCountryState();
        Refresh();
    }


    private void Start()
    {
        BindCountryState();
        Refresh();
    }


    private void OnDisable()
    {
        UnbindCountryState();
    }


    // =========================================================
    // BIND
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


    private void UnbindCountryState()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -=
                Refresh;
        }

        countryState = null;
    }


    // =========================================================
    // REFRESH
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


        // =====================================================
        // RESULTADO FISCAL
        // =====================================================

        if (fiscalResultText != null)
        {
            float value =
                countryState.FiscalResult;

            fiscalResultText.text =
                $"{value:+0.0;-0.0;0.0}";

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


        // =====================================================
        // RESERVAS
        // =====================================================

        if (reservesText != null)
        {
            reservesText.text =
                $"{countryState.Reserves:0.0}";
        }


        // =====================================================
        // DEUDA
        // =====================================================

        if (debtText != null)
        {
            debtText.text =
                $"{countryState.Debt:0.0}";
        }


        // =====================================================
        // INFLACIÓN
        // =====================================================

        if (inflationText != null)
        {
            inflationText.text =
                $"{countryState.Inflation:0.0}%";
        }
    }
}