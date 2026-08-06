using UnityEngine;

public static class PartyPolicyEvaluator
{
    /// <summary>
    /// Devuelve la opinión del partido entre -1 y 1.
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

        policyPosition = Mathf.Clamp(policyPosition, -5f, 5f);

        float partyPosition = party.GetPosition(axis);

        float ideologicalDistance = Mathf.Abs(
            partyPosition - policyPosition
        );

        float opinion = 1f - ideologicalDistance / 5f;

        return Mathf.Clamp(opinion, -1f, 1f);
    }

    /// <summary>
    /// Convierte la opinión ideológica en un impacto de satisfacción.
    /// </summary>
    public static float CalculateSatisfactionImpact(
        PoliticalPartySO party,
        PoliticalAxis axis,
        float policyPosition,
        float policyRelevance)
    {
        float opinion = EvaluateOpinion(
            party,
            axis,
            policyPosition
        );

        return opinion * Mathf.Max(0f, policyRelevance);
    }
}