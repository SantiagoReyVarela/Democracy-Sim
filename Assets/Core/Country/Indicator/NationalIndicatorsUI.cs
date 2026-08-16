using UnityEngine;

public class NationalIndicatorsUI : MonoBehaviour
{
    // =========================================================
    // INDICADORES
    // =========================================================

    [Header("Indicators")]

    [SerializeField]
    private CountryIndicatorUI prosperityIndicator;

    [SerializeField]
    private CountryIndicatorUI equalityIndicator;

    [SerializeField]
    private CountryIndicatorUI employmentIndicator;

    [SerializeField]
    private CountryIndicatorUI educationIndicator;

    [SerializeField]
    private CountryIndicatorUI stabilityIndicator;


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
        // Segunda comprobación por seguridad.
        // Si CountryState todavía no estaba preparado
        // durante OnEnable, lo encontrará aquí.

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
                HandleCountryStateChanged;
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
                HandleCountryStateChanged;
        }


        countryState =
            null;
    }


    // =========================================================
    // CAMBIO EN EL ESTADO DEL PAÍS
    // =========================================================

    private void HandleCountryStateChanged()
    {
        Refresh();
    }


    // =========================================================
    // REFRESCAR TODOS LOS INDICADORES
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
        // TENDENCIAS
        // =====================================================

        float prosperityChange =
            0f;

        float equalityChange =
            0f;

        float employmentChange =
            0f;

        float educationChange =
            0f;

        float stabilityChange =
            0f;


        // -----------------------------------------------------
        // COUNTRY STATE GUARDA UN SNAPSHOT POR TURNO.
        //
        // Ejemplo:
        //
        // History[0] → Turno 1
        // History[1] → Turno 2
        // History[2] → Turno 3
        //
        // Si estamos en el turno 3:
        //
        // valor actual - History[1]
        //
        // nos da el cambio producido durante el último turno.
        // -----------------------------------------------------

        if (
            countryState.History != null &&
            countryState.History.Count >= 2
        )
        {
            CountrySnapshot previousSnapshot =
                countryState.History[
                    countryState.History.Count - 2
                ];


            prosperityChange =
                countryState.Prosperity -
                previousSnapshot.Prosperity;


            equalityChange =
                countryState.Equality -
                previousSnapshot.Equality;


            employmentChange =
                countryState.Employment -
                previousSnapshot.Employment;


            educationChange =
                countryState.Education -
                previousSnapshot.Education;


            stabilityChange =
                countryState.Stability -
                previousSnapshot.Stability;
        }


        // =====================================================
        // PROSPERIDAD
        // =====================================================

        if (prosperityIndicator != null)
        {
            prosperityIndicator.Configure(
                "Prosperidad",
                countryState.Prosperity,
                prosperityChange
            );
        }


        // =====================================================
        // IGUALDAD
        // =====================================================

        if (equalityIndicator != null)
        {
            equalityIndicator.Configure(
                "Igualdad",
                countryState.Equality,
                equalityChange
            );
        }


        // =====================================================
        // EMPLEO
        // =====================================================

        if (employmentIndicator != null)
        {
            employmentIndicator.Configure(
                "Empleo",
                countryState.Employment,
                employmentChange
            );
        }


        // =====================================================
        // EDUCACIÓN
        // =====================================================

        if (educationIndicator != null)
        {
            educationIndicator.Configure(
                "Educación",
                countryState.Education,
                educationChange
            );
        }


        // =====================================================
        // ESTABILIDAD
        // =====================================================

        if (stabilityIndicator != null)
        {
            stabilityIndicator.Configure(
                "Estabilidad",
                countryState.Stability,
                stabilityChange
            );
        }
    }
}