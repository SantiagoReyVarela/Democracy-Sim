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
    // FILAS CREADAS
    // =========================================================

    private readonly List<ConcernRowUI> createdConcernRows =
        new List<ConcernRowUI>();


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        SubscribeToPollManager();

        RefreshConcerns();
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
        RefreshConcerns();
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
    // BORRAR FILAS
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
}