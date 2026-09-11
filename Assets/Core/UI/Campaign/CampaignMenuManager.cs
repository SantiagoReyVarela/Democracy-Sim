using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampaignMenuManager : MonoBehaviour
{
    [Header("Available Scenarios")]
    [Tooltip(
        "Arrastra aquí todos los GameScenarioSO " +
        "que puedan jugarse desde la campaña."
    )]
    [SerializeField]
    private List<GameScenarioSO> availableScenarios =
        new List<GameScenarioSO>();

    [Header("Selection")]
    [SerializeField]
    private TMP_Dropdown countryDropdown;

    [SerializeField]
    private TMP_Dropdown scenarioDropdown;

    [SerializeField]
    private TMP_Dropdown leaderDropdown;

    [Header("Scenario Information")]
    [SerializeField]
    private TMP_Text scenarioTitleText;

    [SerializeField]
    private TMP_Text countryNameText;

    [SerializeField]
    private TMP_Text dateText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text politicalSituationText;

    [SerializeField]
    private TMP_Text economicSituationText;

    [SerializeField]
    private TMP_Text governmentTypeText;

    [SerializeField]
    private TMP_Text stateTypeText;

    [SerializeField]
    private Image countryFlagImage;

    [Header("Buttons")]
    [SerializeField]
    private Button playButton;

    [Header("Scenes")]
    [Tooltip(
        "Nombre exacto de la escena principal de simulación."
    )]
    [SerializeField]
    private string simulationSceneName = "Simulation Manager";

    // =========================================================
    // STATIC SELECTION
    // =========================================================

    public static GameScenarioSO SelectedScenario
    {
        get;
        private set;
    }

    public static ScenarioLeaderConfig SelectedLeader
    {
        get;
        private set;
    }

    public static bool RandomLeaderSelection
    {
        get;
        private set;
    }

    // =========================================================
    // INTERNAL DATA
    // =========================================================

    private readonly List<string> countries =
        new List<string>();

    private readonly List<GameScenarioSO> filteredScenarios =
        new List<GameScenarioSO>();

    private readonly List<ScenarioLeaderConfig> filteredLeaders =
        new List<ScenarioLeaderConfig>();

    private GameScenarioSO currentScenario;
    private ScenarioLeaderConfig currentLeader;
    private bool randomLeaderSelection = true;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        ConfigureButtons();
        ConfigureDropdowns();
    }

    private void Start()
    {
        BuildCountryList();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    // =========================================================
    // LISTENERS
    // =========================================================

    private void ConfigureButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                PlaySelectedScenario
            );

            playButton.onClick.AddListener(
                PlaySelectedScenario
            );
        }
    }

    private void ConfigureDropdowns()
    {
        if (countryDropdown != null)
        {
            countryDropdown.onValueChanged.RemoveListener(
                OnCountryChanged
            );

            countryDropdown.onValueChanged.AddListener(
                OnCountryChanged
            );
        }

        if (scenarioDropdown != null)
        {
            scenarioDropdown.onValueChanged.RemoveListener(
                OnScenarioChanged
            );

            scenarioDropdown.onValueChanged.AddListener(
                OnScenarioChanged
            );
        }

        if (leaderDropdown != null)
        {
            leaderDropdown.onValueChanged.RemoveListener(
                OnLeaderChanged
            );

            leaderDropdown.onValueChanged.AddListener(
                OnLeaderChanged
            );
        }
    }

    private void RemoveListeners()
    {
        if (countryDropdown != null)
        {
            countryDropdown.onValueChanged.RemoveListener(
                OnCountryChanged
            );
        }

        if (scenarioDropdown != null)
        {
            scenarioDropdown.onValueChanged.RemoveListener(
                OnScenarioChanged
            );
        }

        if (leaderDropdown != null)
        {
            leaderDropdown.onValueChanged.RemoveListener(
                OnLeaderChanged
            );
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                PlaySelectedScenario
            );
        }
    }

    // =========================================================
    // COUNTRY
    // =========================================================

    private void BuildCountryList()
    {
        countries.Clear();

        foreach (GameScenarioSO scenario in availableScenarios)
        {
            if (scenario == null)
                continue;

            if (string.IsNullOrWhiteSpace(scenario.countryName))
                continue;

            if (!countries.Contains(scenario.countryName))
            {
                countries.Add(scenario.countryName);
            }
        }

        countries.Sort();

        if (countryDropdown != null)
        {
            countryDropdown.ClearOptions();
            countryDropdown.AddOptions(countries);
        }

        if (countries.Count > 0)
        {
            if (countryDropdown != null)
            {
                countryDropdown.SetValueWithoutNotify(0);
            }

            SelectCountry(0);
        }
        else
        {
            ClearScenarioInformation();
            ClearLeaderSelection();

            if (playButton != null)
            {
                playButton.interactable = false;
            }

            Debug.LogWarning(
                "[CampaignMenuManager] No hay escenarios disponibles."
            );
        }
    }

    private void OnCountryChanged(int index)
    {
        SelectCountry(index);
    }

    private void SelectCountry(int index)
    {
        if (
            index < 0 ||
            index >= countries.Count
        )
        {
            return;
        }

        string selectedCountry =
            countries[index];

        BuildScenarioList(selectedCountry);
    }

    // =========================================================
    // SCENARIOS
    // =========================================================

    private void BuildScenarioList(string countryName)
    {
        filteredScenarios.Clear();

        foreach (GameScenarioSO scenario in availableScenarios)
        {
            if (scenario == null)
                continue;

            if (scenario.countryName == countryName)
            {
                filteredScenarios.Add(scenario);
            }
        }

        filteredScenarios.Sort(
            (a, b) =>
            {
                int yearComparison =
                    a.startYear.CompareTo(b.startYear);

                if (yearComparison != 0)
                {
                    return yearComparison;
                }

                return a.startMonth.CompareTo(
                    b.startMonth
                );
            }
        );

        if (scenarioDropdown != null)
        {
            scenarioDropdown.ClearOptions();

            List<string> names =
                new List<string>();

            foreach (
                GameScenarioSO scenario
                in filteredScenarios
            )
            {
                names.Add(
                    BuildScenarioDropdownName(scenario)
                );
            }

            scenarioDropdown.AddOptions(names);
        }

        if (filteredScenarios.Count > 0)
        {
            if (scenarioDropdown != null)
            {
                scenarioDropdown.SetValueWithoutNotify(0);
            }

            SelectScenario(0);
        }
        else
        {
            currentScenario = null;

            ClearScenarioInformation();
            ClearLeaderSelection();

            if (playButton != null)
            {
                playButton.interactable = false;
            }
        }
    }

    private string BuildScenarioDropdownName(
        GameScenarioSO scenario
    )
    {
        if (scenario == null)
        {
            return "Escenario";
        }

        string name =
            string.IsNullOrWhiteSpace(
                scenario.scenarioName
            )
                ? "Escenario"
                : scenario.scenarioName;

        return
            $"{name} ({scenario.startYear})";
    }

    private void OnScenarioChanged(int index)
    {
        SelectScenario(index);
    }

    private void SelectScenario(int index)
    {
        if (
            index < 0 ||
            index >= filteredScenarios.Count
        )
        {
            return;
        }

        currentScenario =
            filteredScenarios[index];

        UpdateScenarioInformation();

        BuildLeaderList();

        UpdatePlayButtonState();
    }

    // =========================================================
    // LEADERS
    // =========================================================

    private void BuildLeaderList()
    {
        filteredLeaders.Clear();
        currentLeader = null;
        randomLeaderSelection = true;

        if (leaderDropdown != null)
            leaderDropdown.ClearOptions();

        if (currentScenario == null || currentScenario.availableLeaders == null)
        {
            UpdatePlayButtonState();
            return;
        }

        foreach (ScenarioLeaderConfig leader in currentScenario.availableLeaders)
        {
            if (leader == null || !leader.selectableByPlayer)
                continue;

            filteredLeaders.Add(leader);
        }

        List<string> leaderNames = new List<string>();
        leaderNames.Add("Aleatorio");

        foreach (ScenarioLeaderConfig leader in filteredLeaders)
            leaderNames.Add(leader.GetDisplayName());

        if (leaderDropdown != null)
        {
            leaderDropdown.AddOptions(leaderNames);
            leaderDropdown.interactable = true;
            leaderDropdown.SetValueWithoutNotify(0);
        }

        SelectLeader(0);
    }

    private void OnLeaderChanged(int index)
    {
        SelectLeader(index);
    }

    private void SelectLeader(int index)
    {
        if (index == 0)
        {
            currentLeader = null;
            randomLeaderSelection = true;
            UpdatePlayButtonState();

            Debug.Log(
                "[CampaignMenuManager] Selección de líder: Aleatorio"
            );
            return;
        }

        int leaderIndex = index - 1;

        if (leaderIndex < 0 || leaderIndex >= filteredLeaders.Count)
        {
            currentLeader = null;
            randomLeaderSelection = true;
            UpdatePlayButtonState();
            return;
        }

        currentLeader = filteredLeaders[leaderIndex];
        randomLeaderSelection = false;

        UpdatePlayButtonState();

        Debug.Log(
            "[CampaignMenuManager] " +
            $"Líder seleccionado: {currentLeader.GetDisplayName()}"
        );
    }

    private void ClearLeaderSelection()
    {
        filteredLeaders.Clear();
        currentLeader = null;
        randomLeaderSelection = true;

        if (leaderDropdown != null)
        {
            leaderDropdown.ClearOptions();
            leaderDropdown.interactable = false;
        }
    }

    // =========================================================
    // SCENARIO INFORMATION
    // =========================================================

    private void UpdateScenarioInformation()
    {
        if (currentScenario == null)
        {
            ClearScenarioInformation();
            return;
        }

        if (scenarioTitleText != null)
        {
            scenarioTitleText.text =
                currentScenario.scenarioName;
        }

        if (countryNameText != null)
        {
            countryNameText.text =
                currentScenario.countryName;
        }

        if (dateText != null)
        {
            dateText.text =
                GetMonthName(
                    currentScenario.startMonth
                )
                +
                " "
                +
                currentScenario.startYear;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                currentScenario.description;
        }

        if (politicalSituationText != null)
        {
            politicalSituationText.text =
                currentScenario.politicalSituation;
        }

        if (economicSituationText != null)
        {
            economicSituationText.text =
                currentScenario.economicSituation;
        }

        if (governmentTypeText != null)
        {
            governmentTypeText.text =
                currentScenario.governmentType;
        }

        if (stateTypeText != null)
        {
            stateTypeText.text =
                currentScenario.stateType;
        }

        if (countryFlagImage != null)
        {
            countryFlagImage.sprite =
                currentScenario.countryFlag;

            countryFlagImage.enabled =
                currentScenario.countryFlag != null;
        }
    }

    private void ClearScenarioInformation()
    {
        currentScenario = null;

        if (scenarioTitleText != null)
            scenarioTitleText.text = "";

        if (countryNameText != null)
            countryNameText.text = "";

        if (dateText != null)
            dateText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";

        if (politicalSituationText != null)
            politicalSituationText.text = "";

        if (economicSituationText != null)
            economicSituationText.text = "";

        if (governmentTypeText != null)
            governmentTypeText.text = "";

        if (stateTypeText != null)
            stateTypeText.text = "";

        if (countryFlagImage != null)
        {
            countryFlagImage.sprite = null;
            countryFlagImage.enabled = false;
        }
    }

    // =========================================================
    // PLAY BUTTON
    // =========================================================

    private void UpdatePlayButtonState()
    {
        if (playButton == null)
            return;

        playButton.interactable =
            currentScenario != null &&
            (randomLeaderSelection || currentLeader != null);
    }

    public void PlaySelectedScenario()
    {
        if (currentScenario == null)
        {
            Debug.LogWarning(
                "[CampaignMenuManager] " +
                "No hay ningún escenario seleccionado."
            );

            return;
        }

        SelectedScenario =
            currentScenario;

        RandomLeaderSelection =
            randomLeaderSelection;

        SelectedLeader =
            randomLeaderSelection
                ? null
                : currentLeader;

        Debug.Log(
            "[CampaignMenuManager] " +
            $"Escenario seleccionado: " +
            $"{SelectedScenario.scenarioName}"
        );

        Debug.Log(
            "[CampaignMenuManager] " +
            (RandomLeaderSelection
                ? "Líder seleccionado: Aleatorio"
                : $"Líder seleccionado: {SelectedLeader.GetDisplayName()}")
        );

        if (
            string.IsNullOrWhiteSpace(
                simulationSceneName
            )
        )
        {
            Debug.LogError(
                "[CampaignMenuManager] " +
                "No se ha definido la escena de simulación."
            );

            return;
        }

        SceneManager.LoadScene(
            simulationSceneName
        );
    }

    // =========================================================
    // STATIC RESET
    // =========================================================

    public static void ClearSelection()
    {
        SelectedScenario = null;
        SelectedLeader = null;
        RandomLeaderSelection = false;
    }

    // =========================================================
    // DATE
    // =========================================================

    private string GetMonthName(int month)
    {
        switch (month)
        {
            case 1: return "Enero";
            case 2: return "Febrero";
            case 3: return "Marzo";
            case 4: return "Abril";
            case 5: return "Mayo";
            case 6: return "Junio";
            case 7: return "Julio";
            case 8: return "Agosto";
            case 9: return "Septiembre";
            case 10: return "Octubre";
            case 11: return "Noviembre";
            case 12: return "Diciembre";
            default: return "";
        }
    }
}