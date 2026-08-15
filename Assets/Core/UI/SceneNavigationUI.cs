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
    private Button countryButton;

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
        if (countryButton != null)
        {
            countryButton.onClick.AddListener(
                OpenCountry
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


    public void OpenCountry()
    {
        LoadScene("Country");
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
        if (countryButton != null)
        {
            countryButton.onClick.RemoveListener(
                OpenCountry
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