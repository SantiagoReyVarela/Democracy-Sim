public static class PolicySlotUtility
{
    public static PoliticalAxis GetAxis(PolicySlotId slotId)
    {
        switch (slotId)
        {
            case PolicySlotId.Economy1:
            case PolicySlotId.Economy2:
                return PoliticalAxis.Economy;

            case PolicySlotId.Government1:
            case PolicySlotId.Government2:
                return PoliticalAxis.Government;

            case PolicySlotId.Immigration:
                return PoliticalAxis.Immigration;

            case PolicySlotId.Military:
                return PoliticalAxis.Military;

            case PolicySlotId.Religion:
                return PoliticalAxis.Religion;

            case PolicySlotId.Society:
                return PoliticalAxis.Society;

            case PolicySlotId.Trade:
                return PoliticalAxis.Trade;

            default:
                return PoliticalAxis.Economy;
        }
    }

    public static string GetDisplayName(PolicySlotId slotId)
    {
        switch (slotId)
        {
            case PolicySlotId.Economy1:
                return "Economía 1";

            case PolicySlotId.Economy2:
                return "Economía 2";

            case PolicySlotId.Government1:
                return "Gobierno 1";

            case PolicySlotId.Government2:
                return "Gobierno 2";

            case PolicySlotId.Immigration:
                return "Inmigración";

            case PolicySlotId.Military:
                return "Política militar";

            case PolicySlotId.Religion:
                return "Religión";

            case PolicySlotId.Society:
                return "Política social";

            case PolicySlotId.Trade:
                return "Comercio";

            default:
                return slotId.ToString();
        }
    }
}