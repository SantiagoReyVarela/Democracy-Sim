using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectionPollGraphic : MaskableGraphic
{
    // =========================================================
    // CONFIGURACIÓN GENERAL
    // =========================================================

    [Header("Graph")]

    [SerializeField]
    private float lineThickness = 4f;

    [SerializeField]
    private float horizontalPadding = 5f;

    [SerializeField]
    private float verticalPadding = 5f;


    // =========================================================
    // EJE Y
    // =========================================================

    [Header("Y Axis")]

    [Tooltip("Valor mínimo que puede mostrar el eje Y.")]
    [SerializeField, Range(1f, 100f)]
    private float minimumYAxisMaximum = 10f;

    [Tooltip(
        "Separación entre las marcas del eje Y. " +
        "Por ejemplo, 10 genera 0%, 10%, 20%, etc."
    )]
    [SerializeField, Min(1f)]
    private float yAxisStep = 10f;

    [Tooltip(
        "Contenedor que ocupa exactamente la altura útil " +
        "del gráfico."
    )]
    [SerializeField]
    private RectTransform yAxisLabelsContainer;

    [Tooltip(
        "Prefab TMP utilizado para crear cada valor del eje Y."
    )]
    [SerializeField]
    private TMP_Text yAxisLabelPrefab;


    // =========================================================
    // EJE X
    // =========================================================

    [Header("X Axis")]

    [Tooltip(
        "Contenedor horizontal que ocupa exactamente " +
        "el ancho útil del gráfico."
    )]
    [SerializeField]
    private RectTransform xAxisLabelsContainer;

    [Tooltip(
        "Prefab TMP utilizado para crear cada número del eje X."
    )]
    [SerializeField]
    private TMP_Text xAxisLabelPrefab;

    [Tooltip(
        "Cantidad aproximada de etiquetas que queremos " +
        "mostrar en el eje X, incluyendo el primer " +
        "y el último turno."
    )]
    [SerializeField, Min(1)]
    private int xAxisStep = 5;


    // =========================================================
    // ESTADO DEL EJE Y
    // =========================================================

    [SerializeField]
    private float currentYAxisMaximum = 10f;

    private readonly List<TMP_Text> createdYAxisLabels =
        new List<TMP_Text>();

    private float lastBuiltYAxisMaximum =
        -1f;


    // =========================================================
    // ESTADO DEL EJE X
    // =========================================================

    private readonly List<TMP_Text> createdXAxisLabels =
        new List<TMP_Text>();

    private int lastBuiltMaximumTurns = -1;

    private int lastBuiltXAxisStep = -1;


    // =========================================================
    // ENABLE
    // =========================================================

    protected override void OnEnable()
    {
        base.OnEnable();

        if (!Application.isPlaying)
        {
            return;
        }

        SubscribeToPollManager();
        Refresh();
    }


    // =========================================================
    // DISABLE
    // =========================================================

    protected override void OnDisable()
    {
        UnsubscribeFromPollManager();

        ClearYAxisLabels();
        ClearXAxisLabels();

        base.OnDisable();
    }


    // =========================================================
    // SUSCRIPCIÓN
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


    private void UnsubscribeFromPollManager()
    {
        if (ElectionPollManager.Instance == null)
        {
            return;
        }

        ElectionPollManager.Instance.OnPollUpdated -=
            HandlePollUpdated;
    }


    private void HandlePollUpdated()
    {
        Refresh();
    }


    // =========================================================
    // REFRESCAR
    // =========================================================

    public void Refresh()
    {
        // -----------------------------------------------------
        // EJE Y
        // -----------------------------------------------------

        bool yAxisChanged =
            UpdateYAxisMaximum();

        if (
            yAxisChanged ||
            createdYAxisLabels.Count == 0 ||
            !Mathf.Approximately(
                lastBuiltYAxisMaximum,
                currentYAxisMaximum
            )
        )
        {
            RebuildYAxisLabels();
        }


        // -----------------------------------------------------
        // EJE X
        // -----------------------------------------------------

        int maximumTurns =
            GetMaximumTurns();

        if (
            createdXAxisLabels.Count == 0 ||
            lastBuiltMaximumTurns != maximumTurns ||
            lastBuiltXAxisStep != xAxisStep
        )
        {
            RebuildXAxisLabels();
        }


        // -----------------------------------------------------
        // REDIBUJAR LÍNEAS
        // -----------------------------------------------------

        SetVerticesDirty();
    }


    // =========================================================
    // OBTENER MÁXIMO DE TURNOS
    // =========================================================

    private int GetMaximumTurns()
    {
        if (CountryState.Instance != null)
        {
            return Mathf.Max(
                1,
                CountryState.Instance.MaximumTurns
            );
        }

        return 48;
    }


    // =========================================================
    // ACTUALIZAR MÁXIMO DEL EJE Y
    // =========================================================

    private bool UpdateYAxisMaximum()
    {
        ElectionPollManager pollManager =
            ElectionPollManager.Instance;

        if (pollManager == null)
        {
            return false;
        }


        List<PollPartyData> trackedParties =
            pollManager.GetTrackedParties();

        if (
            trackedParties == null ||
            trackedParties.Count == 0
        )
        {
            return false;
        }


        float highestHistoricalValue =
            0f;


        // -----------------------------------------------------
        // BUSCAR EL MAYOR VALOR HISTÓRICO
        // -----------------------------------------------------

        foreach (
            PollPartyData partyData
            in trackedParties
        )
        {
            if (partyData == null)
            {
                continue;
            }


            if (partyData.history != null)
            {
                foreach (
                    float historicalValue
                    in partyData.history
                )
                {
                    if (
                        historicalValue >
                        highestHistoricalValue
                    )
                    {
                        highestHistoricalValue =
                            historicalValue;
                    }
                }
            }


            // Comprobamos también el valor actual.

            if (
                partyData.currentPolling >
                highestHistoricalValue
            )
            {
                highestHistoricalValue =
                    partyData.currentPolling;
            }
        }


        // -----------------------------------------------------
        // ASEGURAR UN MÍNIMO
        // -----------------------------------------------------

        float valueToCover =
            Mathf.Max(
                highestHistoricalValue,
                minimumYAxisMaximum
            );


        // -----------------------------------------------------
        // REDONDEAR HACIA ARRIBA
        //
        // 37.2 -> 40
        // 49.0 -> 50
        // 50.1 -> 60
        // -----------------------------------------------------

        float requiredMaximum =
            Mathf.Ceil(
                valueToCover /
                yAxisStep
            )
            * yAxisStep;


        requiredMaximum =
            Mathf.Clamp(
                requiredMaximum,
                yAxisStep,
                100f
            );


        // -----------------------------------------------------
        // EL EJE Y SOLO PUEDE EXPANDIRSE
        // -----------------------------------------------------

        if (
            requiredMaximum <=
            currentYAxisMaximum
        )
        {
            return false;
        }


        currentYAxisMaximum =
            requiredMaximum;


        return true;
    }


    // =========================================================
    // RECONSTRUIR EJE Y
    // =========================================================

    private void RebuildYAxisLabels()
    {
        ClearYAxisLabels();


        if (
            yAxisLabelsContainer == null ||
            yAxisLabelPrefab == null
        )
        {
            return;
        }


        if (
            currentYAxisMaximum <= 0f ||
            yAxisStep <= 0f
        )
        {
            return;
        }


        int intervalCount =
            Mathf.RoundToInt(
                currentYAxisMaximum /
                yAxisStep
            );


        for (
            int i = 0;
            i <= intervalCount;
            i++
        )
        {
            float value =
                i * yAxisStep;


            float normalizedY =
                value /
                currentYAxisMaximum;


            TMP_Text label =
                Instantiate(
                    yAxisLabelPrefab,
                    yAxisLabelsContainer
                );


            label.text =
                $"{value:0.#}%";


            RectTransform labelRect =
                label.rectTransform;


            // -------------------------------------------------
            // POSICIÓN VERTICAL
            // -------------------------------------------------

            labelRect.anchorMin =
                new Vector2(
                    1f,
                    normalizedY
                );


            labelRect.anchorMax =
                new Vector2(
                    1f,
                    normalizedY
                );


            // -------------------------------------------------
            // PIVOT
            //
            // 0% queda dentro por encima del límite inferior.
            // Máximo queda dentro por debajo del superior.
            // -------------------------------------------------

            float pivotY =
                0.5f;


            if (i == 0)
            {
                pivotY =
                    0f;
            }
            else if (i == intervalCount)
            {
                pivotY =
                    1f;
            }


            labelRect.pivot =
                new Vector2(
                    1f,
                    pivotY
                );


            labelRect.anchoredPosition =
                Vector2.zero;


            createdYAxisLabels.Add(
                label
            );
        }


        lastBuiltYAxisMaximum =
            currentYAxisMaximum;
    }


    // =========================================================
    // BORRAR EJE Y
    // =========================================================

    private void ClearYAxisLabels()
    {
        if (yAxisLabelsContainer != null)
        {
            for (
                int i = yAxisLabelsContainer.childCount - 1;
                i >= 0;
                i--
            )
            {
                GameObject child =
                    yAxisLabelsContainer.GetChild(i).gameObject;

                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        createdYAxisLabels.Clear();
    }


    // =========================================================
    // RECONSTRUIR EJE X
    // =========================================================

    private void RebuildXAxisLabels()
    {
        ClearXAxisLabels();


        if (
            xAxisLabelsContainer == null ||
            xAxisLabelPrefab == null
        )
        {
            return;
        }


        int maximumTurns =
            GetMaximumTurns();


        // =========================================================
        // TURNO 1
        // =========================================================

        CreateXAxisLabel(
            1,
            0f,
            true,
            maximumTurns == 1
        );


        if (maximumTurns <= 1)
        {
            lastBuiltMaximumTurns =
                maximumTurns;

            lastBuiltXAxisStep =
                xAxisStep;

            return;
        }


        // =========================================================
        // MARCAS CADA X TURNOS
        //
        // Ejemplo con Step = 5:
        //
        // 1, 5, 10, 15, 20...
        // =========================================================

        int safeStep =
            Mathf.Max(
                1,
                xAxisStep
            );


        for (
            int turn = safeStep;
            turn < maximumTurns;
            turn += safeStep
        )
        {
            // Evitar repetir el turno 1
            if (turn <= 1)
            {
                continue;
            }


            float normalizedX =
                (turn - 1f) /
                (maximumTurns - 1f);


            CreateXAxisLabel(
                turn,
                normalizedX,
                false,
                false
            );
        }


        // =========================================================
        // ÚLTIMO TURNO
        //
        // Se añade siempre si no ha aparecido ya como múltiplo
        // del step.
        // =========================================================

        bool maximumAlreadyCreated =
            maximumTurns % safeStep == 0;


        if (!maximumAlreadyCreated)
        {
            CreateXAxisLabel(
                maximumTurns,
                1f,
                false,
                true
            );
        }
        else
        {
            // El último múltiplo no se creó porque el bucle usa
            // turn < maximumTurns, así que lo añadimos igualmente.
            CreateXAxisLabel(
                maximumTurns,
                1f,
                false,
                true
            );
        }


        lastBuiltMaximumTurns =
            maximumTurns;

        lastBuiltXAxisStep =
            xAxisStep;
    }


    // =========================================================
    // CREAR UNA ETIQUETA DEL EJE X
    // =========================================================

    private void CreateXAxisLabel(
        int turn,
        float normalizedX,
        bool isFirst,
        bool isLast
    )
    {
        TMP_Text label =
            Instantiate(
                xAxisLabelPrefab,
                xAxisLabelsContainer
            );


        label.text =
            turn.ToString();


        RectTransform labelRect =
            label.rectTransform;


        // -----------------------------------------------------
        // POSICIÓN HORIZONTAL EXACTA
        // -----------------------------------------------------

        labelRect.anchorMin =
            new Vector2(
                normalizedX,
                0.5f
            );


        labelRect.anchorMax =
            new Vector2(
                normalizedX,
                0.5f
            );


        // -----------------------------------------------------
        // PIVOT
        //
        // Primer turno:
        // texto crece hacia la derecha.
        //
        // Último turno:
        // texto crece hacia la izquierda.
        //
        // Intermedios:
        // centrados.
        // -----------------------------------------------------

        float pivotX =
            0.5f;


        if (isFirst)
        {
            pivotX =
                0f;
        }
        else if (isLast)
        {
            pivotX =
                1f;
        }


        labelRect.pivot =
            new Vector2(
                pivotX,
                0.5f
            );


        labelRect.anchoredPosition =
            Vector2.zero;


        createdXAxisLabels.Add(
            label
        );
    }


    // =========================================================
    // BORRAR EJE X
    // =========================================================

    private void ClearXAxisLabels()
    {
        if (xAxisLabelsContainer != null)
        {
            for (
                int i = xAxisLabelsContainer.childCount - 1;
                i >= 0;
                i--
            )
            {
                GameObject child =
                    xAxisLabelsContainer.GetChild(i).gameObject;

                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        createdXAxisLabels.Clear();
    }


    // =========================================================
    // DIBUJAR GRÁFICO
    // =========================================================

    protected override void OnPopulateMesh(
        VertexHelper vh
    )
    {
        vh.Clear();


        ElectionPollManager pollManager =
            ElectionPollManager.Instance;


        if (pollManager == null)
        {
            return;
        }


        List<PollPartyData> trackedParties =
            pollManager.GetTrackedParties();


        if (
            trackedParties == null ||
            trackedParties.Count == 0
        )
        {
            return;
        }


        Rect rect =
            rectTransform.rect;


        float left =
            rect.xMin +
            horizontalPadding;


        float right =
            rect.xMax -
            horizontalPadding;


        float bottom =
            rect.yMin +
            verticalPadding;


        float top =
            rect.yMax -
            verticalPadding;


        foreach (
            PollPartyData partyData
            in trackedParties
        )
        {
            if (
                partyData == null ||
                partyData.party == null ||
                partyData.history == null ||
                partyData.history.Count == 0
            )
            {
                continue;
            }


            DrawPartyLine(
                vh,
                partyData,
                left,
                right,
                bottom,
                top
            );
        }
    }


    // =========================================================
    // DIBUJAR LÍNEA DE UN PARTIDO
    // =========================================================

    private void DrawPartyLine(
        VertexHelper vh,
        PollPartyData partyData,
        float left,
        float right,
        float bottom,
        float top
    )
    {
        List<float> history =
            partyData.history;


        Color partyColor =
            partyData.party.partyColor;


        int maximumTurns =
            GetMaximumTurns();


        int maximumHistoryIndex =
            Mathf.Max(
                1,
                maximumTurns - 1
            );


        // -----------------------------------------------------
        // SOLO UN DATO
        // -----------------------------------------------------

        if (history.Count == 1)
        {
            float y =
                GetYPosition(
                    history[0],
                    bottom,
                    top
                );


            DrawPoint(
                vh,
                new Vector2(
                    left,
                    y
                ),
                partyColor
            );


            return;
        }


        // -----------------------------------------------------
        // VARIOS TURNOS
        // -----------------------------------------------------

        for (
            int i = 0;
            i < history.Count - 1;
            i++
        )
        {
            // -------------------------------------------------
            // POSICIÓN X SEGÚN EL TURNO REAL
            // -------------------------------------------------

            float normalizedX1 =
                Mathf.Clamp01(
                    (float)i /
                    maximumHistoryIndex
                );


            float normalizedX2 =
                Mathf.Clamp01(
                    (float)(i + 1) /
                    maximumHistoryIndex
                );


            float x1 =
                Mathf.Lerp(
                    left,
                    right,
                    normalizedX1
                );


            float x2 =
                Mathf.Lerp(
                    left,
                    right,
                    normalizedX2
                );


            float y1 =
                GetYPosition(
                    history[i],
                    bottom,
                    top
                );


            float y2 =
                GetYPosition(
                    history[i + 1],
                    bottom,
                    top
                );


            DrawLineSegment(
                vh,
                new Vector2(
                    x1,
                    y1
                ),
                new Vector2(
                    x2,
                    y2
                ),
                partyColor
            );
        }
    }


    // =========================================================
    // CONVERTIR INTENCIÓN DE VOTO EN POSICIÓN Y
    // =========================================================

    private float GetYPosition(
        float pollingValue,
        float bottom,
        float top
    )
    {
        float normalized =
            currentYAxisMaximum > 0f
                ? pollingValue /
                  currentYAxisMaximum
                : 0f;


        normalized =
            Mathf.Clamp01(
                normalized
            );


        return Mathf.Lerp(
            bottom,
            top,
            normalized
        );
    }


    // =========================================================
    // SEGMENTO DE LÍNEA
    // =========================================================

    private void DrawLineSegment(
        VertexHelper vh,
        Vector2 start,
        Vector2 end,
        Color color
    )
    {
        Vector2 difference =
            end - start;


        if (
            difference.sqrMagnitude <=
            0.0001f
        )
        {
            DrawPoint(
                vh,
                start,
                color
            );


            return;
        }


        Vector2 direction =
            difference.normalized;


        Vector2 perpendicular =
            new Vector2(
                -direction.y,
                direction.x
            )
            * lineThickness
            * 0.5f;


        Vector2 v0 =
            start -
            perpendicular;


        Vector2 v1 =
            start +
            perpendicular;


        Vector2 v2 =
            end +
            perpendicular;


        Vector2 v3 =
            end -
            perpendicular;


        int index =
            vh.currentVertCount;


        vh.AddVert(
            v0,
            color,
            Vector2.zero
        );


        vh.AddVert(
            v1,
            color,
            Vector2.zero
        );


        vh.AddVert(
            v2,
            color,
            Vector2.zero
        );


        vh.AddVert(
            v3,
            color,
            Vector2.zero
        );


        vh.AddTriangle(
            index,
            index + 1,
            index + 2
        );


        vh.AddTriangle(
            index,
            index + 2,
            index + 3
        );
    }


    // =========================================================
    // PUNTO
    // =========================================================

    private void DrawPoint(
        VertexHelper vh,
        Vector2 position,
        Color color
    )
    {
        float halfSize =
            lineThickness;


        Vector2 v0 =
            position +
            new Vector2(
                -halfSize,
                -halfSize
            );


        Vector2 v1 =
            position +
            new Vector2(
                -halfSize,
                halfSize
            );


        Vector2 v2 =
            position +
            new Vector2(
                halfSize,
                halfSize
            );


        Vector2 v3 =
            position +
            new Vector2(
                halfSize,
                -halfSize
            );


        int index =
            vh.currentVertCount;


        vh.AddVert(
            v0,
            color,
            Vector2.zero
        );


        vh.AddVert(
            v1,
            color,
            Vector2.zero
        );


        vh.AddVert(
            v2,
            color,
            Vector2.zero
        );


        vh.AddVert(
            v3,
            color,
            Vector2.zero
        );


        vh.AddTriangle(
            index,
            index + 1,
            index + 2
        );


        vh.AddTriangle(
            index,
            index + 2,
            index + 3
        );
    }
}