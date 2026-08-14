using System.Collections.Generic;
using UnityEngine;

public class ElectionScreenController : MonoBehaviour
{
    // =========================================================
    // PREOCUPACIONES
    // =========================================================

    [Header("Public Concerns")]

    [SerializeField]
    private Transform concernsContainer;

    [SerializeField]
    private ConcernRowUI concernRowPrefab;


    // =========================================================
    // LEYENDA DE LA ENCUESTA
    // =========================================================

    [Header("Poll Legend")]

    [SerializeField]
    private Transform pollLegendContainer;

    [SerializeField]
    private PollLegendRowUI pollLegendRowPrefab;


    // =========================================================
    // FILAS CREADAS
    // =========================================================

    private readonly List<ConcernRowUI> createdConcernRows =
        new List<ConcernRowUI>();

    private readonly List<PollLegendRowUI> createdLegendRows =
        new List<PollLegendRowUI>();


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        SubscribeToPollManager();

        RefreshAll();
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Por seguridad, por si ElectionPollManager todavía
        // no estaba disponible durante OnEnable.

        SubscribeToPollManager();

        RefreshAll();
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        UnsubscribeFromPollManager();
    }


    // =========================================================
    // SUSCRIBIRSE
    // =========================================================

    private void SubscribeToPollManager()
    {
        if (ElectionPollManager.Instance == null)
        {
            return;
        }


        ElectionPollManager.Instance.OnPollUpdated -=
            HandlePollUpdated;


        ElectionPollManager.Instance.OnPollUpdated +=
            HandlePollUpdated;
    }


    // =========================================================
    // DESUSCRIBIRSE
    // =========================================================

    private void UnsubscribeFromPollManager()
    {
        if (ElectionPollManager.Instance == null)
        {
            return;
        }


        ElectionPollManager.Instance.OnPollUpdated -=
            HandlePollUpdated;
    }


    // =========================================================
    // ENCUESTA ACTUALIZADA
    // =========================================================

    private void HandlePollUpdated()
    {
        RefreshAll();
    }


    // =========================================================
    // REFRESCAR TODO
    // =========================================================

    public void RefreshAll()
    {
        RefreshConcerns();
        RefreshPollLegend();
    }


    // =========================================================
    // REFRESCAR PREOCUPACIONES
    // =========================================================

    public void RefreshConcerns()
    {
        ClearConcernRows();


        if (PublicConcernManager.Instance == null)
        {
            Debug.LogWarning(
                "ElectionScreenController: " +
                "PublicConcernManager.Instance no existe."
            );

            return;
        }


        if (
            concernsContainer == null ||
            concernRowPrefab == null
        )
        {
            Debug.LogWarning(
                "ElectionScreenController: " +
                "Concerns Container o Concern Row Prefab " +
                "no están asignados."
            );

            return;
        }


        IReadOnlyList<PublicConcernData> concerns =
            PublicConcernManager.Instance.TopConcerns;


        if (concerns == null)
        {
            return;
        }


        foreach (
            PublicConcernData concern
            in concerns
        )
        {
            if (concern == null)
            {
                continue;
            }


            ConcernRowUI newRow =
                Instantiate(
                    concernRowPrefab,
                    concernsContainer
                );


            newRow.Configure(
                concern
            );


            createdConcernRows.Add(
                newRow
            );
        }
    }


    // =========================================================
    // REFRESCAR LEYENDA DE PARTIDOS
    // =========================================================

    public void RefreshPollLegend()
    {
        ClearLegendRows();


        if (ElectionPollManager.Instance == null)
        {
            Debug.LogWarning(
                "ElectionScreenController: " +
                "ElectionPollManager.Instance no existe."
            );

            return;
        }


        if (
            pollLegendContainer == null ||
            pollLegendRowPrefab == null
        )
        {
            Debug.LogWarning(
                "ElectionScreenController: " +
                "Poll Legend Container o Poll Legend Row Prefab " +
                "no están asignados."
            );

            return;
        }


        List<PollPartyData> trackedParties =
            ElectionPollManager.Instance.GetTrackedParties();


        if (
            trackedParties == null ||
            trackedParties.Count == 0
        )
        {
            return;
        }


        foreach (
            PollPartyData partyData
            in trackedParties
        )
        {
            if (
                partyData == null ||
                partyData.party == null
            )
            {
                continue;
            }


            PollLegendRowUI newRow =
                Instantiate(
                    pollLegendRowPrefab,
                    pollLegendContainer
                );


            newRow.Configure(
                partyData
            );


            createdLegendRows.Add(
                newRow
            );
        }
    }


    // =========================================================
    // BORRAR FILAS DE PREOCUPACIONES
    // =========================================================

    private void ClearConcernRows()
    {
        foreach (
            ConcernRowUI row
            in createdConcernRows
        )
        {
            if (row != null)
            {
                Destroy(
                    row.gameObject
                );
            }
        }


        createdConcernRows.Clear();
    }


    // =========================================================
    // BORRAR FILAS DE LEYENDA
    // =========================================================

    private void ClearLegendRows()
    {
        foreach (
            PollLegendRowUI row
            in createdLegendRows
        )
        {
            if (row != null)
            {
                Destroy(
                    row.gameObject
                );
            }
        }


        createdLegendRows.Clear();
    }
}