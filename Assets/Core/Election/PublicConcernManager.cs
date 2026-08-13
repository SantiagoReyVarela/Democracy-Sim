using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// =============================================================
// TIPOS DE PREOCUPACIÓN
// =============================================================

public enum PublicConcernType
{
    Economy,
    Education,
    Unemployment,
    Inequality,
    Inflation,
    Stability,
    Debt,
    FiscalDeficit,
    Reserves
}


// =============================================================
// DATOS DE UNA PREOCUPACIÓN
// =============================================================

[Serializable]
public class PublicConcernData
{
    public PublicConcernType type;

    [Range(0f, 100f)]
    public float rawConcern;

    [Range(0f, 100f)]
    public float importance;


    public string DisplayName
    {
        get
        {
            switch (type)
            {
                case PublicConcernType.Economy:
                    return "Economía";

                case PublicConcernType.Education:
                    return "Educación";

                case PublicConcernType.Unemployment:
                    return "Desempleo";

                case PublicConcernType.Inequality:
                    return "Desigualdad";

                case PublicConcernType.Inflation:
                    return "Inflación";

                case PublicConcernType.Stability:
                    return "Estabilidad";

                case PublicConcernType.Debt:
                    return "Deuda";

                case PublicConcernType.FiscalDeficit:
                    return "Déficit público";

                case PublicConcernType.Reserves:
                    return "Reservas";

                default:
                    return type.ToString();
            }
        }
    }
}


// =============================================================
// MANAGER
// =============================================================

public class PublicConcernManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static PublicConcernManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // CONFIGURACIÓN
    // =========================================================

    [Header("Configuration")]

    [SerializeField, Range(1, 10)]
    private int visibleConcernCount = 5;


    // =========================================================
    // ESTADO
    // =========================================================

    [Header("Current Concerns")]

    [SerializeField]
    private List<PublicConcernData> allConcerns =
        new List<PublicConcernData>();

    [SerializeField]
    private List<PublicConcernData> topConcerns =
        new List<PublicConcernData>();


    // =========================================================
    // ACCESO PÚBLICO
    // =========================================================

    public IReadOnlyList<PublicConcernData> AllConcerns =>
        allConcerns;

    public IReadOnlyList<PublicConcernData> TopConcerns =>
        topConcerns;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(
            gameObject
        );
    }


    // =========================================================
    // START
    // =========================================================

    private IEnumerator Start()
    {
        // Esperamos un frame para asegurarnos de que
        // CountryState ya existe.
        yield return null;


        RefreshConcerns();
    }


    // =========================================================
    // ACTUALIZAR PREOCUPACIONES
    // =========================================================

    public void RefreshConcerns()
    {
        CountryState countryState =
            CountryState.Instance;


        if (countryState == null)
        {
            Debug.LogWarning(
                "PublicConcernManager: CountryState.Instance no existe."
            );

            return;
        }


        allConcerns.Clear();


        // -----------------------------------------------------
        // ECONOMÍA
        //
        // Cuanto menor es la prosperidad,
        // mayor preocupación económica.
        // -----------------------------------------------------

        AddConcern(
            PublicConcernType.Economy,
            100f - countryState.Prosperity
        );


        // -----------------------------------------------------
        // EDUCACIÓN
        // -----------------------------------------------------

        AddConcern(
            PublicConcernType.Education,
            100f - countryState.Education
        );


        // -----------------------------------------------------
        // DESEMPLEO
        //
        // Employment = 100 significa empleo excelente.
        // -----------------------------------------------------

        AddConcern(
            PublicConcernType.Unemployment,
            100f - countryState.Employment
        );


        // -----------------------------------------------------
        // DESIGUALDAD
        // -----------------------------------------------------

        AddConcern(
            PublicConcernType.Inequality,
            100f - countryState.Equality
        );


        // -----------------------------------------------------
        // INFLACIÓN
        //
        // Consideramos aproximadamente 2% como normal.
        // Tanto inflación elevada como deflación pueden
        // convertirse en preocupación.
        // -----------------------------------------------------

        float inflationConcern =
            Mathf.Abs(
                countryState.Inflation - 2f
            )
            * 10f;


        AddConcern(
            PublicConcernType.Inflation,
            inflationConcern
        );


        // -----------------------------------------------------
        // ESTABILIDAD
        // -----------------------------------------------------

        AddConcern(
            PublicConcernType.Stability,
            100f - countryState.Stability
        );


        // -----------------------------------------------------
        // DEUDA
        //
        // Fórmula suavizada para permitir deuda > 100.
        // -----------------------------------------------------

        float debtConcern =
            countryState.Debt /
            (
                countryState.Debt +
                100f
            )
            * 100f;


        AddConcern(
            PublicConcernType.Debt,
            debtConcern
        );


        // -----------------------------------------------------
        // DÉFICIT
        //
        // Solo genera preocupación si el resultado fiscal
        // es negativo.
        // -----------------------------------------------------

        float deficitConcern =
            Mathf.Max(
                0f,
                -countryState.FiscalResult
            );


        AddConcern(
            PublicConcernType.FiscalDeficit,
            deficitConcern
        );


        // -----------------------------------------------------
        // FALTA DE RESERVAS
        // -----------------------------------------------------

        float reserveConcern =
            100f -
            Mathf.Clamp(
                countryState.Reserves,
                0f,
                100f
            );


        AddConcern(
            PublicConcernType.Reserves,
            reserveConcern
        );


        CalculateTopConcerns();
    }


    // =========================================================
    // AÑADIR PREOCUPACIÓN
    // =========================================================

    private void AddConcern(
        PublicConcernType type,
        float rawConcern
    )
    {
        PublicConcernData data =
            new PublicConcernData();


        data.type =
            type;

        data.rawConcern =
            Mathf.Clamp(
                rawConcern,
                0f,
                100f
            );

        data.importance =
            0f;


        allConcerns.Add(
            data
        );
    }


    // =========================================================
    // SELECCIONAR TOP 5 Y NORMALIZAR A 100%
    // =========================================================

    private void CalculateTopConcerns()
    {
        topConcerns =
            allConcerns
            .OrderByDescending(
                concern =>
                    concern.rawConcern
            )
            .Take(
                visibleConcernCount
            )
            .ToList();


        float totalConcern =
            topConcerns.Sum(
                concern =>
                    concern.rawConcern
            );


        // -----------------------------------------------------
        // CASO EXTREMO:
        // TODO FUNCIONA PERFECTAMENTE
        // -----------------------------------------------------

        if (
            totalConcern <=
            Mathf.Epsilon
        )
        {
            float equalWeight =
                100f /
                Mathf.Max(
                    1,
                    topConcerns.Count
                );


            foreach (
                PublicConcernData concern
                in topConcerns
            )
            {
                concern.importance =
                    equalWeight;
            }


            return;
        }


        // -----------------------------------------------------
        // NORMALIZAR
        //
        // LOS CINCO PRINCIPALES SUMAN 100%
        // -----------------------------------------------------

        foreach (
            PublicConcernData concern
            in topConcerns
        )
        {
            concern.importance =
                concern.rawConcern /
                totalConcern *
                100f;
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("Refresh Concerns")]
    private void DebugRefreshConcerns()
    {
        RefreshConcerns();


        Debug.Log(
            "===== PRINCIPALES PREOCUPACIONES ====="
        );


        foreach (
            PublicConcernData concern
            in topConcerns
        )
        {
            Debug.Log(
                $"{concern.DisplayName}: " +
                $"{concern.importance:F1}% " +
                $"(problema bruto: {concern.rawConcern:F1})"
            );
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}