public enum GovernmentCoalitionType
{
    NoGovernment,           // aun no se ha formado gobierno
    SinglePartyMajority,    // un solo partido tiene mayoria absoluta
    SinglePartyMinority,    // un solo partido gobierno sin mayoria absoluta (tiene que pactar con otros)
    MajorityCoalition,      // varios partidos gobiernan juntos y suman mayoria absoluta
    MinorityCoalition,      // varios partidos gobiernan juntos sin mayoria abosluta
    GrandCoalition,         // coalicion entre grandes partidos ideologicamente alejados
    CaretakerGovernment     // gobierno provisional o en funciones
}