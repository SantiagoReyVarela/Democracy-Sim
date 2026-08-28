using UnityEngine;

public static class PartyPolicyEvaluator
{
    /// <summary>
    /// Devuelve la opinión del partido entre -1 y 1 usando directamente
    /// un PoliticalPartySO.
    ///
    /// Se mantiene para compatibilidad con sistemas que todavía no
    /// utilizan escenarios.
    ///
    ///  1 = apoyo máximo
    ///  0 = indiferencia
    /// -1 = rechazo máximo
    /// </summary>
    public static float EvaluateOpinion(
        PoliticalPartySO party,
        PoliticalAxis axis,
        float policyPosition)
    {
        if (party == null)
        {
            Debug.LogWarning(
                "PartyPolicyEvaluator: el partido recibido es nulo."
            );

            return 0f;
        }

        float partyPosition =
            party.GetPosition(axis);

        return EvaluateOpinionFromPosition(
            partyPosition,
            policyPosition
        );
    }


    /// <summary>
    /// Devuelve la opinión de un PoliticalPartyState.
    ///
    /// Si el partido pertenece a un escenario, PoliticalPartyManager
    /// resolverá primero cualquier override ideológico del escenario.
    /// Si ese eje no tiene override, se heredará la posición del
    /// PoliticalPartySO base.
    /// </summary>
    public static float EvaluateOpinion(
        PoliticalPartyState partyState,
        PoliticalPartyManager partyManager,
        PoliticalAxis axis,
        float policyPosition)
    {
        if (partyState == null)
        {
            Debug.LogWarning(
                "PartyPolicyEvaluator: el PoliticalPartyState recibido es nulo."
            );

            return 0f;
        }

        float partyPosition;

        if (partyManager != null)
        {
            partyPosition =
                partyManager.GetEffectivePartyPosition(
                    partyState,
                    axis
                );
        }
        else
        {
            if (partyState.party == null)
            {
                Debug.LogWarning(
                    "PartyPolicyEvaluator: el partido base del estado es nulo."
                );

                return 0f;
            }

            partyPosition =
                partyState.party.GetPosition(axis);
        }

        return EvaluateOpinionFromPosition(
            partyPosition,
            policyPosition
        );
    }


    /// <summary>
    /// Calcula la opinión a partir de una posición ideológica ya resuelta.
    /// </summary>
    public static float EvaluateOpinionFromPosition(
        float partyPosition,
        float policyPosition)
    {
        partyPosition =
            Mathf.Clamp(
                partyPosition,
                -5f,
                5f
            );

        policyPosition =
            Mathf.Clamp(
                policyPosition,
                -5f,
                5f
            );

        float ideologicalDistance =
            Mathf.Abs(
                partyPosition -
                policyPosition
            );

        float opinion =
            1f -
            ideologicalDistance / 5f;

        return Mathf.Clamp(
            opinion,
            -1f,
            1f
        );
    }


    /// <summary>
    /// Convierte la opinión ideológica en un impacto de satisfacción.
    ///
    /// Versión compatible con PoliticalPartySO.
    /// </summary>
    public static float CalculateSatisfactionImpact(
        PoliticalPartySO party,
        PoliticalAxis axis,
        float policyPosition,
        float policyRelevance)
    {
        float opinion =
            EvaluateOpinion(
                party,
                axis,
                policyPosition
            );

        return
            opinion *
            Mathf.Max(
                0f,
                policyRelevance
            );
    }


    /// <summary>
    /// Convierte la opinión ideológica efectiva del partido en un impacto
    /// de satisfacción.
    ///
    /// Esta es la versión que debe utilizar PoliticalPartyManager cuando
    /// la partida ha sido creada desde un GameScenarioSO.
    /// </summary>
    public static float CalculateSatisfactionImpact(
        PoliticalPartyState partyState,
        PoliticalPartyManager partyManager,
        PoliticalAxis axis,
        float policyPosition,
        float policyRelevance)
    {
        float opinion =
            EvaluateOpinion(
                partyState,
                partyManager,
                axis,
                policyPosition
            );

        return
            opinion *
            Mathf.Max(
                0f,
                policyRelevance
            );
    }
}