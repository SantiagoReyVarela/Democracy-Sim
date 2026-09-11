using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class ParliamentHemicycleGraphic : MaskableGraphic
{
    // =========================================================
    // GEOMETRÍA DEL HEMICICLO
    // =========================================================

    [Header("Geometry")]

    [SerializeField, Range(0.2f, 0.9f)]
    private float innerRadiusRatio = 0.58f;

    [SerializeField, Range(0f, 4f)]
    private float gapDegrees = 0.8f;

    [SerializeField, Min(1f)]
    private float degreesPerStep = 3f;


    // =========================================================
    // INDICADOR DEL GOBIERNO
    // =========================================================

    [Header("Government Ring")]

    [SerializeField, Min(0f)]
    private float governmentRingGap = 3f;

    [SerializeField, Min(1f)]
    private float governmentRingThickness = 9f;

    [SerializeField]
    private Color governmentRingColor = Color.white;

    [SerializeField]
    private bool usePartyColorForGovernmentRing = false;


    // =========================================================
    // ANILLO IDEOLÓGICO INTERIOR
    // =========================================================

    [Header("Ideological Inner Ring")]

    [SerializeField, Min(0f)]
    private float ideologicalRingGap = 2f;

    [SerializeField, Min(1f)]
    private float ideologicalRingThickness = 10f;

    [Tooltip(
        "Separación angular entre grupos ideológicos. " +
        "No separa partidos individuales."
    )]
    [SerializeField, Range(0f, 4f)]
    private float ideologicalGroupGapDegrees = 0.8f;


    // =========================================================
    // COLORES IZQUIERDA
    // =========================================================

    [Header("Left Ideology Colors")]

    [SerializeField]
    private Color leftMainstreamColor =
        new Color(1f, 0.65f, 0.65f);

    [SerializeField]
    private Color leftHardlineColor =
        new Color(0.9f, 0.15f, 0.15f);

    [SerializeField]
    private Color leftRadicalColor =
        new Color(0.45f, 0.05f, 0.05f);


    // =========================================================
    // COLOR CENTRO
    // =========================================================

    [Header("Center Ideology Color")]

    [SerializeField]
    private Color centerIdeologyColor =
        new Color(0.75f, 0.75f, 0.75f);


    // =========================================================
    // COLORES DERECHA
    // =========================================================

    [Header("Right Ideology Colors")]

    [SerializeField]
    private Color rightMainstreamColor =
        new Color(0.65f, 0.78f, 1f);

    [SerializeField]
    private Color rightHardlineColor =
        new Color(0.2f, 0.4f, 0.9f);

    [SerializeField]
    private Color rightRadicalColor =
        new Color(0.05f, 0.12f, 0.35f);


    // =========================================================
    // SEGMENTO DE PARTIDO
    // =========================================================

    private sealed class Segment
    {
        public PoliticalPartyState partyState;

        public Color color;

        public int seats;

        public bool isGovernment;
    }


    private readonly List<Segment> segments =
        new List<Segment>();


    // =========================================================
    // REFRESH
    // =========================================================

    public void Refresh(
        PoliticalPartyManager manager
    )
    {
        segments.Clear();


        if (manager == null)
        {
            SetVerticesDirty();
            return;
        }


        List<PoliticalPartyState> parties =
            new List<PoliticalPartyState>();


        foreach (
            PoliticalPartyState partyState
            in manager.PartyStates
        )
        {
            if (
                partyState == null ||
                partyState.party == null ||
                partyState.seats <= 0
            )
            {
                continue;
            }


            parties.Add(
                partyState
            );
        }


        // Orden:
        //
        // Izquierda radical
        // Izquierda hardline
        // Izquierda mainstream
        // Centro
        // Derecha mainstream
        // Derecha hardline
        // Derecha radical

        parties.Sort(
            CompareParties
        );


        foreach (
            PoliticalPartyState partyState
            in parties
        )
        {
            segments.Add(
                new Segment
                {
                    partyState =
                        partyState,

                    color =
                        manager.GetEffectivePartyColor(partyState),

                    seats =
                        partyState.seats,

                    isGovernment =
                        partyState.isGovernment
                }
            );
        }


        SetVerticesDirty();
        SetMaterialDirty();
    }


    // =========================================================
    // ORDEN IDEOLÓGICO
    // =========================================================

    private int CompareParties(
        PoliticalPartyState a,
        PoliticalPartyState b
    )
    {
        PoliticalPartySO partyA =
            a.party;

        PoliticalPartySO partyB =
            b.party;


        HemicycleBloc blocA =
            partyA.HemicycleBloc;

        HemicycleBloc blocB =
            partyB.HemicycleBloc;


        // =========================================================
        // 1. BLOQUES
        //
        // LEFT -> CENTER -> RIGHT
        // =========================================================

        if (blocA != blocB)
        {
            return blocA.CompareTo(
                blocB
            );
        }


        int layerA =
            (int)partyA.RadicalizationLayer;

        int layerB =
            (int)partyB.RadicalizationLayer;


        // =========================================================
        // 2. IZQUIERDA
        //
        // Orden de capas:
        //
        // Radical -> Hardline -> Mainstream -> Centro
        //
        // Dentro de una misma capa:
        //
        // MENOS escaños -> más hacia el extremo
        // MÁS escaños   -> más cerca del centro
        // =========================================================

        if (blocA == HemicycleBloc.Left)
        {
            if (layerA != layerB)
            {
                return layerB.CompareTo(
                    layerA
                );
            }


            int seatsComparison =
                a.seats.CompareTo(
                    b.seats
                );


            if (seatsComparison != 0)
            {
                return seatsComparison;
            }


            return string.CompareOrdinal(
                partyA.partyName,
                partyB.partyName
            );
        }


        // =========================================================
        // 3. CENTRO
        // =========================================================

        if (blocA == HemicycleBloc.Center)
        {
            if (layerA != layerB)
            {
                return layerA.CompareTo(
                    layerB
                );
            }


            // Si algún día tenemos varios partidos
            // exactamente centrales, dejamos primero
            // los de mayor representación.

            int seatsComparison =
                b.seats.CompareTo(
                    a.seats
                );


            if (seatsComparison != 0)
            {
                return seatsComparison;
            }


            return string.CompareOrdinal(
                partyA.partyName,
                partyB.partyName
            );
        }


        // =========================================================
        // 4. DERECHA
        //
        // Orden de capas:
        //
        // Centro -> Mainstream -> Hardline -> Radical
        //
        // Dentro de una misma capa:
        //
        // MÁS escaños   -> más cerca del centro
        // MENOS escaños -> más hacia el extremo
        //
        // Como la derecha avanza desde el centro hacia
        // el extremo, los grandes tienen que aparecer primero.
        // =========================================================

        if (layerA != layerB)
        {
            return layerA.CompareTo(
                layerB
            );
        }


        int rightSeatsComparison =
            b.seats.CompareTo(
                a.seats
            );


        if (rightSeatsComparison != 0)
        {
            return rightSeatsComparison;
        }


        return string.CompareOrdinal(
            partyA.partyName,
            partyB.partyName
        );
    }


    // =========================================================
    // DIBUJAR TODO
    // =========================================================

    protected override void OnPopulateMesh(
        VertexHelper vertexHelper
    )
    {
        vertexHelper.Clear();


        if (segments.Count == 0)
        {
            return;
        }


        // =====================================================
        // TOTAL DE ESCAÑOS
        // =====================================================

        int totalSeats =
            0;


        foreach (
            Segment segment
            in segments
        )
        {
            totalSeats +=
                segment.seats;
        }


        if (totalSeats <= 0)
        {
            return;
        }


        // =====================================================
        // RECT
        // =====================================================

        Rect rect =
            rectTransform.rect;


        if (
            rect.width <= 1f ||
            rect.height <= 1f
        )
        {
            return;
        }


        // =====================================================
        // RADIO MÁXIMO
        // =====================================================

        float maxRadius =
            Mathf.Min(
                rect.width * 0.5f,
                rect.height
            );


        maxRadius *=
            0.95f;


        // =====================================================
        // RADIO DEL HEMICICLO PRINCIPAL
        // =====================================================

        float outerRadius =
            maxRadius -
            governmentRingGap -
            governmentRingThickness;


        if (outerRadius <= 0f)
        {
            return;
        }


        float innerRadius =
            outerRadius *
            innerRadiusRatio;


        // =====================================================
        // RADIOS DEL ANILLO IDEOLÓGICO INTERIOR
        // =====================================================

        float ideologyOuterRadius =
            innerRadius -
            ideologicalRingGap;


        float ideologyInnerRadius =
            ideologyOuterRadius -
            ideologicalRingThickness;


        ideologyInnerRadius =
            Mathf.Max(
                0f,
                ideologyInnerRadius
            );


        // =====================================================
        // CENTRO
        // =====================================================

        Vector2 center =
            new Vector2(
                rect.center.x,
                rect.yMin
            );


        // =====================================================
        // CAPA 1:
        // ANILLO IDEOLÓGICO INTERIOR
        // =====================================================

        if (
            ideologyOuterRadius > 0f &&
            ideologyOuterRadius >
            ideologyInnerRadius
        )
        {
            DrawIdeologicalInnerRing(
                vertexHelper,
                center,
                ideologyInnerRadius,
                ideologyOuterRadius,
                totalSeats
            );
        }


        // =====================================================
        // CAPA 2:
        // HEMICICLO PRINCIPAL DE PARTIDOS
        // =====================================================

        DrawMainParliament(
            vertexHelper,
            center,
            innerRadius,
            outerRadius,
            totalSeats
        );


        // =====================================================
        // CAPA 3:
        // ANILLO EXTERIOR DEL GOBIERNO
        // =====================================================

        float governmentInnerRadius =
            outerRadius +
            governmentRingGap;


        float governmentOuterRadius =
            governmentInnerRadius +
            governmentRingThickness;


        DrawGovernmentRing(
            vertexHelper,
            center,
            governmentInnerRadius,
            governmentOuterRadius,
            totalSeats
        );
    }


    // =========================================================
    // HEMICICLO PRINCIPAL
    // =========================================================

    private void DrawMainParliament(
        VertexHelper vertexHelper,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        int totalSeats
    )
    {
        float currentAngle =
            180f;


        foreach (
            Segment segment
            in segments
        )
        {
            float segmentAngle =
                180f *
                segment.seats /
                (float)totalSeats;


            float rawStart =
                currentAngle;


            float rawEnd =
                currentAngle -
                segmentAngle;


            float visualStart =
                rawStart -
                gapDegrees * 0.5f;


            float visualEnd =
                rawEnd +
                gapDegrees * 0.5f;


            FixSmallSegment(
                rawStart,
                rawEnd,
                ref visualStart,
                ref visualEnd
            );


            AddSegment(
                vertexHelper,
                center,
                innerRadius,
                outerRadius,
                visualStart,
                visualEnd,
                segment.color
            );


            currentAngle =
                rawEnd;
        }
    }


    // =========================================================
    // ANILLO EXTERIOR DEL GOBIERNO
    // =========================================================

    private void DrawGovernmentRing(
        VertexHelper vertexHelper,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        int totalSeats
    )
    {
        float currentAngle =
            180f;


        foreach (
            Segment segment
            in segments
        )
        {
            float segmentAngle =
                180f *
                segment.seats /
                (float)totalSeats;


            float rawStart =
                currentAngle;


            float rawEnd =
                currentAngle -
                segmentAngle;


            float visualStart =
                rawStart -
                gapDegrees * 0.5f;


            float visualEnd =
                rawEnd +
                gapDegrees * 0.5f;


            FixSmallSegment(
                rawStart,
                rawEnd,
                ref visualStart,
                ref visualEnd
            );


            if (segment.isGovernment)
            {
                AddSegment(
                    vertexHelper,
                    center,
                    innerRadius,
                    outerRadius,
                    visualStart,
                    visualEnd,
                    GetGovernmentRingColor(
                        segment
                    )
                );
            }


            currentAngle =
                rawEnd;
        }
    }


    // =========================================================
    // ANILLO IDEOLÓGICO INTERIOR
    //
    // IMPORTANTE:
    // NO DIBUJA UNA FRANJA POR PARTIDO.
    //
    // AGRUPA TODOS LOS PARTIDOS CON:
    //
    // MISMO HemicycleBloc
    // +
    // MISMO RadicalizationLayer
    //
    // EN UNA SOLA FRANJA.
    // =========================================================

    private void DrawIdeologicalInnerRing(
        VertexHelper vertexHelper,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        int totalSeats
    )
    {
        if (
            segments == null ||
            segments.Count == 0
        )
        {
            return;
        }


        float currentAngle =
            180f;


        int index =
            0;


        while (index < segments.Count)
        {
            Segment firstSegment =
                segments[index];


            PoliticalPartySO firstParty =
                firstSegment.partyState.party;


            HemicycleBloc groupBloc =
                firstParty.HemicycleBloc;


            PoliticalRadicalizationLayer groupLayer =
                firstParty.RadicalizationLayer;


            int groupSeats =
                0;


            int currentIndex =
                index;


            // -------------------------------------------------
            // SUMAMOS TODOS LOS PARTIDOS CONSECUTIVOS
            // DEL MISMO BLOQUE Y CAPA
            // -------------------------------------------------

            while (
                currentIndex <
                segments.Count
            )
            {
                Segment candidate =
                    segments[currentIndex];


                PoliticalPartySO candidateParty =
                    candidate.partyState.party;


                if (
                    candidateParty.HemicycleBloc !=
                    groupBloc ||
                    candidateParty.RadicalizationLayer !=
                    groupLayer
                )
                {
                    break;
                }


                groupSeats +=
                    candidate.seats;


                currentIndex++;
            }


            // -------------------------------------------------
            // ÁNGULO TOTAL DEL GRUPO
            // -------------------------------------------------

            float groupAngle =
                180f *
                groupSeats /
                (float)totalSeats;


            float rawStart =
                currentAngle;


            float rawEnd =
                currentAngle -
                groupAngle;


            // -------------------------------------------------
            // EL GAP SOLO SE APLICA ENTRE GRUPOS.
            //
            // NO entre partidos individuales.
            // -------------------------------------------------

            float visualStart =
                rawStart -
                ideologicalGroupGapDegrees *
                0.5f;


            float visualEnd =
                rawEnd +
                ideologicalGroupGapDegrees *
                0.5f;


            FixSmallSegment(
                rawStart,
                rawEnd,
                ref visualStart,
                ref visualEnd
            );


            Color groupColor =
                GetIdeologicalRingColor(
                    groupBloc,
                    groupLayer
                );


            // -------------------------------------------------
            // UNA ÚNICA FRANJA PARA TODO EL GRUPO
            // -------------------------------------------------

            AddSegment(
                vertexHelper,
                center,
                innerRadius,
                outerRadius,
                visualStart,
                visualEnd,
                groupColor
            );


            currentAngle =
                rawEnd;


            index =
                currentIndex;
        }
    }


    // =========================================================
    // COLOR DEL ANILLO IDEOLÓGICO
    // =========================================================

    private Color GetIdeologicalRingColor(
        HemicycleBloc bloc,
        PoliticalRadicalizationLayer layer
    )
    {
        // -----------------------------------------------------
        // IZQUIERDA
        // -----------------------------------------------------

        if (bloc == HemicycleBloc.Left)
        {
            switch (layer)
            {
                case PoliticalRadicalizationLayer.Radical:

                    return
                        leftRadicalColor;


                case PoliticalRadicalizationLayer.Hardline:

                    return
                        leftHardlineColor;


                case PoliticalRadicalizationLayer.Mainstream:

                    return
                        leftMainstreamColor;


                case PoliticalRadicalizationLayer.Center:

                    return
                        leftMainstreamColor;
            }
        }


        // -----------------------------------------------------
        // CENTRO
        // -----------------------------------------------------

        if (bloc == HemicycleBloc.Center)
        {
            return
                centerIdeologyColor;
        }


        // -----------------------------------------------------
        // DERECHA
        // -----------------------------------------------------

        switch (layer)
        {
            case PoliticalRadicalizationLayer.Radical:

                return
                    rightRadicalColor;


            case PoliticalRadicalizationLayer.Hardline:

                return
                    rightHardlineColor;


            case PoliticalRadicalizationLayer.Mainstream:

                return
                    rightMainstreamColor;


            case PoliticalRadicalizationLayer.Center:

                return
                    rightMainstreamColor;
        }


        return
            centerIdeologyColor;
    }


    // =========================================================
    // COLOR DEL ANILLO DEL GOBIERNO
    // =========================================================

    private Color GetGovernmentRingColor(
        Segment segment
    )
    {
        if (
            usePartyColorForGovernmentRing
        )
        {
            return
                segment.color;
        }


        return
            governmentRingColor;
    }


    // =========================================================
    // CORREGIR SEGMENTOS MUY PEQUEÑOS
    // =========================================================

    private void FixSmallSegment(
        float rawStart,
        float rawEnd,
        ref float visualStart,
        ref float visualEnd
    )
    {
        if (
            visualStart >=
            visualEnd
        )
        {
            return;
        }


        float middle =
            (
                rawStart +
                rawEnd
            ) *
            0.5f;


        visualStart =
            middle;


        visualEnd =
            middle;
    }


    // =========================================================
    // CREAR SEGMENTO
    // =========================================================

    private void AddSegment(
        VertexHelper vertexHelper,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color segmentColor
    )
    {
        float angularSize =
            Mathf.Abs(
                startAngle -
                endAngle
            );


        if (angularSize <= 0f)
        {
            return;
        }


        int steps =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    angularSize /
                    degreesPerStep
                )
            );


        for (
            int i = 0;
            i < steps;
            i++
        )
        {
            float t0 =
                i /
                (float)steps;


            float t1 =
                (i + 1) /
                (float)steps;


            float angle0 =
                Mathf.Lerp(
                    startAngle,
                    endAngle,
                    t0
                );


            float angle1 =
                Mathf.Lerp(
                    startAngle,
                    endAngle,
                    t1
                );


            Vector2 inner0 =
                GetPoint(
                    center,
                    innerRadius,
                    angle0
                );


            Vector2 outer0 =
                GetPoint(
                    center,
                    outerRadius,
                    angle0
                );


            Vector2 inner1 =
                GetPoint(
                    center,
                    innerRadius,
                    angle1
                );


            Vector2 outer1 =
                GetPoint(
                    center,
                    outerRadius,
                    angle1
                );


            int vertexIndex =
                vertexHelper.currentVertCount;


            vertexHelper.AddVert(
                inner0,
                segmentColor,
                Vector2.zero
            );


            vertexHelper.AddVert(
                outer0,
                segmentColor,
                Vector2.zero
            );


            vertexHelper.AddVert(
                outer1,
                segmentColor,
                Vector2.zero
            );


            vertexHelper.AddVert(
                inner1,
                segmentColor,
                Vector2.zero
            );


            vertexHelper.AddTriangle(
                vertexIndex,
                vertexIndex + 1,
                vertexIndex + 2
            );


            vertexHelper.AddTriangle(
                vertexIndex,
                vertexIndex + 2,
                vertexIndex + 3
            );
        }
    }


    // =========================================================
    // PUNTO DEL CÍRCULO
    // =========================================================

    private Vector2 GetPoint(
        Vector2 center,
        float radius,
        float angleDegrees
    )
    {
        float radians =
            angleDegrees *
            Mathf.Deg2Rad;


        return center +
            new Vector2(
                Mathf.Cos(
                    radians
                ),
                Mathf.Sin(
                    radians
                )
            ) *
            radius;
    }


#if UNITY_EDITOR

    // =========================================================
    // VALIDACIÓN
    // =========================================================

    protected override void OnValidate()
    {
        base.OnValidate();


        innerRadiusRatio =
            Mathf.Clamp(
                innerRadiusRatio,
                0.2f,
                0.9f
            );


        gapDegrees =
            Mathf.Max(
                0f,
                gapDegrees
            );


        degreesPerStep =
            Mathf.Max(
                1f,
                degreesPerStep
            );


        governmentRingGap =
            Mathf.Max(
                0f,
                governmentRingGap
            );


        governmentRingThickness =
            Mathf.Max(
                1f,
                governmentRingThickness
            );


        ideologicalRingGap =
            Mathf.Max(
                0f,
                ideologicalRingGap
            );


        ideologicalRingThickness =
            Mathf.Max(
                1f,
                ideologicalRingThickness
            );


        ideologicalGroupGapDegrees =
            Mathf.Max(
                0f,
                ideologicalGroupGapDegrees
            );


        SetVerticesDirty();
    }

#endif
}