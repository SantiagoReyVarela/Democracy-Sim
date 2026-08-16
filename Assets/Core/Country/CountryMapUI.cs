using UnityEngine;
using UnityEngine.UI;

public class CountryMapUI : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]

    [SerializeField]
    private Image mapImage;


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
    // DESCONECTAR
    // =========================================================

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

        if (mapImage == null)
        {
            return;
        }


        // =====================================================
        // MAPA
        // =====================================================

        mapImage.sprite =
            countryState.CountryMap;


        // Si no hay mapa asignado,
        // ocultamos la imagen.

        mapImage.enabled =
            countryState.CountryMap != null;


        // Evita que Unity deforme la silueta.

        mapImage.preserveAspect =
            true;
    }
}