using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Scene Names")]
    [SerializeField] private string campaignSceneName = "Campaign";
    [SerializeField] private string customScenariosSceneName = "CustomScenarios";


    private void Start()
    {
        ShowMainMenu();
    }


    // =========================================================
    // JUGAR CAMPAÑA
    // =========================================================

    public void PlayCampaign()
    {
        if (string.IsNullOrWhiteSpace(campaignSceneName))
        {
            Debug.LogError(
                "[MainMenuManager] No se ha definido la escena de campaña."
            );

            return;
        }

        SceneManager.LoadScene(campaignSceneName);
    }


    // =========================================================
    // ESCENARIOS PERSONALIZADOS
    // =========================================================

    public void OpenCustomScenarios()
    {
        if (string.IsNullOrWhiteSpace(customScenariosSceneName))
        {
            Debug.LogError(
                "[MainMenuManager] No se ha definido la escena de escenarios personalizados."
            );

            return;
        }

        SceneManager.LoadScene(customScenariosSceneName);
    }


    // =========================================================
    // OPCIONES
    // =========================================================

    public void OpenOptions()
    {
        SetAllPanelsInactive();

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }


    // =========================================================
    // CRÉDITOS
    // =========================================================

    public void OpenCredits()
    {
        SetAllPanelsInactive();

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }


    // =========================================================
    // VOLVER AL MENÚ PRINCIPAL
    // =========================================================

    public void ShowMainMenu()
    {
        SetAllPanelsInactive();

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }


    // =========================================================
    // SALIR
    // =========================================================

    public void ExitGame()
    {
        Debug.Log("[MainMenuManager] Cerrando juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    // =========================================================
    // UTILIDAD
    // =========================================================

    private void SetAllPanelsInactive()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }
}