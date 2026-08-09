using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParliamentHemicycleGraphic : MaskableGraphic
{
    [Header("Geometry")]

    [SerializeField, Range(0.2f, 0.9f)]
    private float innerRadiusRatio = 0.58f;

    [SerializeField, Range(0f, 4f)]
    private float gapDegrees = 0.8f;

    [SerializeField, Min(1f)]
    private float degreesPerStep = 3f;


    private sealed class Segment
    {
        public PoliticalPartyState partyState;
        public Color color;
        public int seats;
    }


    private readonly List<Segment> segments =
        new List<Segment>();


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

            parties.Add(partyState);
        }


        parties.Sort(CompareParties);


        foreach (
            PoliticalPartyState partyState
            in parties
        )
        {
            segments.Add(
                new Segment
                {
                    partyState = partyState,
                    color = partyState.party.partyColor,
                    seats = partyState.seats
                }
            );
        }


        SetVerticesDirty();
    }


    private int CompareParties(
        PoliticalPartyState a,
        PoliticalPartyState b
    )
    {
        HemicycleBloc blocA =
            a.party.HemicycleBloc;

        HemicycleBloc blocB =
            b.party.HemicycleBloc;


        // Primero izquierda, después centro,
        // finalmente derecha.

        if (blocA != blocB)
        {
            return blocA.CompareTo(blocB);
        }


        // En la izquierda:
        // más escaños = más hacia el extremo izquierdo.

        if (blocA == HemicycleBloc.Left)
        {
            return b.seats.CompareTo(a.seats);
        }


        // En el centro utilizamos una posición manual.
        // Esto permite colocar al Centrista exactamente
        // entre Conservador y Liberal.

        if (blocA == HemicycleBloc.Center)
        {
            return
                a.party.HemicycleCenterPosition
                .CompareTo(
                    b.party.HemicycleCenterPosition
                );
        }


        // En la derecha:
        // menos escaños primero, cerca del centro.
        // El partido con más escaños termina
        // en el extremo derecho.

        return a.seats.CompareTo(b.seats);
    }


    protected override void OnPopulateMesh(
        VertexHelper vertexHelper
    )
    {
        vertexHelper.Clear();

        if (segments.Count == 0)
        {
            return;
        }


        int totalSeats = 0;

        foreach (Segment segment in segments)
        {
            totalSeats += segment.seats;
        }

        if (totalSeats <= 0)
        {
            return;
        }


        Rect rect =
            rectTransform.rect;


        // El radio máximo está limitado:
        // horizontalmente por la mitad del ancho,
        // verticalmente por la altura.

        float outerRadius =
            Mathf.Min(
                rect.width * 0.5f,
                rect.height
            );

        outerRadius *= 0.95f;


        float innerRadius =
            outerRadius *
            innerRadiusRatio;


        // Centro del círculo:
        // centro horizontal y parte inferior del RectTransform.

        Vector2 center =
            new Vector2(
                rect.center.x,
                rect.yMin
            );


        // 180º = izquierda.
        //   0º = derecha.

        float currentAngle = 180f;


        foreach (Segment segment in segments)
        {
            float percentage =
                segment.seats /
                (float)totalSeats;


            float segmentAngle =
                180f * percentage;


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


            // Evita que un segmento muy pequeño
            // desaparezca debido al espacio entre sectores.

            if (visualStart < visualEnd)
            {
                float middle =
                    (rawStart + rawEnd) * 0.5f;

                visualStart = middle;
                visualEnd = middle;
            }


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
                startAngle - endAngle
            );


        int steps =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    angularSize /
                    degreesPerStep
                )
            );


        for (int i = 0; i < steps; i++)
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


            int index =
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
                index,
                index + 1,
                index + 2
            );

            vertexHelper.AddTriangle(
                index,
                index + 2,
                index + 3
            );
        }
    }


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
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            ) *
            radius;
    }


#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();

        innerRadiusRatio =
            Mathf.Clamp(
                innerRadiusRatio,
                0.2f,
                0.9f
            );

        degreesPerStep =
            Mathf.Max(
                1f,
                degreesPerStep
            );

        SetVerticesDirty();
    }

#endif
}