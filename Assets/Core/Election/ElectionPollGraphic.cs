using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElectionPollGraphic : MaskableGraphic
{
    [Header("Graph")]
    [SerializeField]
    private float lineThickness = 4f;

    [SerializeField]
    private float horizontalPadding = 5f;

    [SerializeField]
    private float verticalPadding = 5f;


    // =========================================================
    // REFRESCAR
    // =========================================================

    public void Refresh()
    {
        SetVerticesDirty();
    }


    // =========================================================
    // AL ACTIVARSE
    // =========================================================

    protected override void OnEnable()
    {
        base.OnEnable();

        Refresh();
    }


    // =========================================================
    // DIBUJAR
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
            rect.xMin + horizontalPadding;

        float right =
            rect.xMax - horizontalPadding;

        float bottom =
            rect.yMin + verticalPadding;

        float top =
            rect.yMax - verticalPadding;


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


        // -----------------------------------------------------
        // SOLO EXISTE EL DATO INICIAL
        // -----------------------------------------------------

        if (history.Count == 1)
        {
            float y =
                Mathf.Lerp(
                    bottom,
                    top,
                    Mathf.Clamp01(
                        history[0] / 100f
                    )
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
        // HISTORIAL CON VARIOS TURNOS
        // -----------------------------------------------------

        for (
            int i = 0;
            i < history.Count - 1;
            i++
        )
        {
            float normalizedX1 =
                (float)i /
                (history.Count - 1);


            float normalizedX2 =
                (float)(i + 1) /
                (history.Count - 1);


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
                Mathf.Lerp(
                    bottom,
                    top,
                    Mathf.Clamp01(
                        history[i] / 100f
                    )
                );


            float y2 =
                Mathf.Lerp(
                    bottom,
                    top,
                    Mathf.Clamp01(
                        history[i + 1] / 100f
                    )
                );


            DrawLineSegment(
                vh,
                new Vector2(x1, y1),
                new Vector2(x2, y2),
                partyColor
            );
        }
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


        if (difference.sqrMagnitude <= 0.0001f)
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
            start - perpendicular;

        Vector2 v1 =
            start + perpendicular;

        Vector2 v2 =
            end + perpendicular;

        Vector2 v3 =
            end - perpendicular;


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