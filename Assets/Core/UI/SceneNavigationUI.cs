using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla la navegación entre las diferentes pantallas
/// del simulador.
/// </summary>
public class SceneNavigationUI : MonoBehaviour
{
    [Header("Navigation buttons")]

    [SerializeField]
    private Button countryDataButton;

    [SerializeField]
    private Button economyButton;

    [SerializeField]
    private Button policiesButton;

    [SerializeField]
    private Button partiesButton;

    [SerializeField]
    private Button electionButton;


    private void Awake()
    {
        if (countryDataButton != null)
        {
            countryDataButton.onClick.AddListener(
                OpenCountryData
            );
        }

        if (economyButton != null)
        {
            economyButton.onClick.AddListener(
                OpenEconomy
            );
        }

        if (policiesButton != null)
        {
            policiesButton.onClick.AddListener(
                OpenPolicies
            );
        }

        if (electionButton != null)
        {
            electionButton.onClick.AddListener(
                OpenElection
            );
        }

        if (partiesButton != null)
        {
            partiesButton.onClick.AddListener(
                OpenParties
            );
        }
    }


    public void OpenCountryData()
    {
        LoadScene("CountryData");
    }


    public void OpenEconomy()
    {
        LoadScene("Economy");
    }


    public void OpenPolicies()
    {
        LoadScene("Policies");
    }


    public void OpenParties()
    {
        LoadScene("Parties");
    }

    public void OpenElection()
    {
        LoadScene("Election");
    }


    private void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }


    private void OnDestroy()
    {
        if (countryDataButton != null)
        {
            countryDataButton.onClick.RemoveListener(
                OpenCountryData
            );
        }

        if (economyButton != null)
        {
            economyButton.onClick.RemoveListener(
                OpenEconomy
            );
        }

        if (policiesButton != null)
        {
            policiesButton.onClick.RemoveListener(
                OpenPolicies
            );
        }

        if (electionButton != null)
        {
            electionButton.onClick.RemoveListener(
                OpenElection
            );
        }

        if (partiesButton != null)
        {
            partiesButton.onClick.RemoveListener(
                OpenParties
            );
        }
    }
}