using UnityEngine;
using UnityEngine.UI;

public class CountryLeaderPortraitUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image portraitImage;

    private CountryState countryState;


    private void Start()
    {
        countryState = CountryState.Instance;

        if (countryState == null)
        {
            Debug.LogError(
                "CountryLeaderPortraitUI: no se encontró CountryState.",
                this
            );

            return;
        }

        countryState.OnStateChanged += Refresh;

        Refresh();
    }


    private void OnDestroy()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= Refresh;
        }
    }


    public void Refresh()
    {
        if (portraitImage == null)
        {
            Debug.LogError(
                "CountryLeaderPortraitUI: Portrait Image no está asignado.",
                this
            );

            return;
        }

        if (countryState == null)
        {
            return;
        }


        PoliticalLeaderSO leader =
            countryState.GovernmentLeader;


        if (leader == null)
        {
            Debug.LogWarning(
                "CountryLeaderPortraitUI: GovernmentLeader es NULL.",
                this
            );

            return;
        }


        Debug.Log(
            $"Líder recibido: {leader.LeaderName} | " +
            $"Portrait: {(leader.Portrait != null ? leader.Portrait.name : "NULL")}",
            this
        );


        if (leader.Portrait == null)
        {
            Debug.LogWarning(
                $"El líder {leader.LeaderName} no tiene Portrait.",
                leader
            );

            return;
        }


        portraitImage.sprite =
            leader.Portrait;

        portraitImage.enabled =
            true;

        portraitImage.preserveAspect =
            true;
    }
}