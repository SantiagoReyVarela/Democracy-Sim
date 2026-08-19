using System.Collections.Generic;
using UnityEngine;

public class CountryPoliticalSituationUI : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("Concerns")]

    [SerializeField]
    private Transform concernsContainer;

    [SerializeField]
    private CountryConcernRowUI concernRowPrefab;

    [SerializeField, Min(1)]
    private int visibleConcernCount = 3;


    // =========================================================
    // COUNTRY STATE
    // =========================================================

    private CountryState countryState;


    // =========================================================
    // FILAS CREADAS
    // =========================================================

    private readonly List<CountryConcernRowUI>
        createdRows =
            new List<CountryConcernRowUI>();


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

        ClearRows();
    }


    // =========================================================
    // COUNTRY STATE
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


        countryState =
            null;
    }


    // =========================================================
    // REFRESCAR
    // =========================================================

    public void Refresh()
    {
        ClearRows();


        if (
            concernsContainer == null ||
            concernRowPrefab == null
        )
        {
            return;
        }


        PublicConcernManager manager =
            PublicConcernManager.Instance;


        if (manager == null)
        {
            Debug.LogWarning(
                "CountryPoliticalSituationUI: " +
                "no existe PublicConcernManager."
            );

            return;
        }


        if (
            manager.TopConcerns == null ||
            manager.TopConcerns.Count == 0
        )
        {
            return;
        }


        // =====================================================
        // COPIAR Y ORDENAR
        // =====================================================

        List<PublicConcernData> concerns =
            new List<PublicConcernData>();


        foreach (
            PublicConcernData concern
            in manager.TopConcerns
        )
        {
            if (concern != null)
            {
                concerns.Add(
                    concern
                );
            }
        }


        concerns.Sort(
            (a, b) =>
                b.importance.CompareTo(
                    a.importance
                )
        );


        // =====================================================
        // MOSTRAR SOLO LAS PRINCIPALES
        // =====================================================

        int amountToShow =
            Mathf.Min(
                visibleConcernCount,
                concerns.Count
            );


        for (
            int i = 0;
            i < amountToShow;
            i++
        )
        {
            PublicConcernData concern =
                concerns[i];


            CountryConcernRowUI row =
                Instantiate(
                    concernRowPrefab,
                    concernsContainer
                );


            row.Configure(
                GetConcernDisplayName(
                    concern.type
                ),
                concern.importance
            );


            createdRows.Add(
                row
            );
        }
    }


    // =========================================================
    // LIMPIAR
    // =========================================================

    private void ClearRows()
    {
        createdRows.Clear();


        if (concernsContainer == null)
        {
            return;
        }


        for (
            int i =
                concernsContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                concernsContainer
                    .GetChild(i)
                    .gameObject;


            child.SetActive(false);


            if (Application.isPlaying)
            {
                Destroy(
                    child
                );
            }
            else
            {
                DestroyImmediate(
                    child
                );
            }
        }
    }


    // =========================================================
    // NOMBRES
    // =========================================================

    private string GetConcernDisplayName(
        PublicConcernType type
    )
    {
        switch (type)
        {
            case PublicConcernType.Economy:

                return
                    "Prosperidad";


            case PublicConcernType.Education:

                return
                    "Educación";


            case PublicConcernType.Unemployment:

                return
                    "Desempleo";


            case PublicConcernType.Inequality:

                return
                    "Desigualdad";


            case PublicConcernType.Inflation:

                return
                    "Inflación";


            case PublicConcernType.Stability:

                return
                    "Inestabilidad";


            case PublicConcernType.Debt:

                return
                    "Deuda";


            case PublicConcernType.FiscalDeficit:

                return
                    "Déficit fiscal";


            case PublicConcernType.Reserves:

                return
                    "Falta de reservas";


            default:

                return
                    type.ToString();
        }
    }
}