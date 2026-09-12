using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountryHeaderUI : MonoBehaviour
{
    [Header("Country")]
    [SerializeField] private TMP_Text countryNameText;
    [SerializeField] private TMP_Text stateTypeText;
    [SerializeField] private TMP_Text governmentSystemText;
    [SerializeField] private Image countryFlagImage;
    [SerializeField] private Image countryMapImage;

    [Header("Leader")]
    [SerializeField] private TMP_Text leaderNameText;
    [SerializeField] private TMP_Text leaderPartyText;

    [Header("Date")]
    [SerializeField] private TMP_Text dateText;

    [Header("Turn")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text electionCountdownText;


    private CountryState countryState;
    private Coroutine bindRoutine;


    private void OnEnable()
    {
        TryBindCountryState();

        if (bindRoutine == null)
        {
            bindRoutine =
                StartCoroutine(
                    WaitForCountryState()
                );
        }
    }


    private void Start()
    {
        TryBindCountryState();
        Refresh();

        ValidateReferences();
    }


    private void OnDisable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }

        UnbindCountryState();
    }


    private IEnumerator WaitForCountryState()
    {
        while (countryState == null)
        {
            TryBindCountryState();

            if (countryState != null)
            {
                Refresh();
                break;
            }

            yield return null;
        }

        bindRoutine = null;
    }


    private void TryBindCountryState()
    {
        CountryState instance =
            CountryState.Instance;

        if (instance == null)
        {
            return;
        }

        if (countryState == instance)
        {
            return;
        }

        UnbindCountryState();

        countryState = instance;
        countryState.OnStateChanged += Refresh;
    }


    private void UnbindCountryState()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= Refresh;
        }

        countryState = null;
    }


    private void ValidateReferences()
    {
        if (leaderNameText == null)
        {
            Debug.LogWarning(
                "CountryHeaderUI: Leader Name Text no está asignado en el Inspector.",
                this
            );
        }

        if (dateText == null)
        {
            Debug.LogWarning(
                "CountryHeaderUI: Date Text no está asignado en el Inspector.",
                this
            );
        }

        if (countryFlagImage == null)
        {
            Debug.LogWarning(
                "CountryHeaderUI: Country Flag Image no está asignada en el Inspector.",
                this
            );
        }

        if (countryMapImage == null)
        {
            Debug.LogWarning(
                "CountryHeaderUI: Country Map Image no está asignada en el Inspector.",
                this
            );
        }
    }


    public void Refresh()
    {
        if (countryState == null)
        {
            countryState = CountryState.Instance;
        }

        if (countryState == null)
        {
            return;
        }

        if (countryState.GovernmentLeader != null)
        {
            if (leaderNameText != null)
            {
                leaderNameText.text =
                    countryState.GovernmentLeader.LeaderName;
            }

            ScenarioLeaderRuntime selectedLeader =
                countryState.CurrentScenario != null
                    ? countryState.CurrentScenario.SelectedLeader
                    : null;

            if (leaderPartyText != null)
            {
                if (selectedLeader != null && selectedLeader.Party != null)
                {
                    leaderPartyText.text = selectedLeader.Party.DisplayName;
                    leaderPartyText.color = selectedLeader.Party.DisplayColor;
                }
                else
                {
                    leaderPartyText.text = string.Empty;
                }
            }
        }


        // NOMBRE DEL PAÍS
        if (countryNameText != null)
        {
            countryNameText.text =
                countryState.CountryName.ToUpper();
        }


        // BANDERA DEL PAÍS
        if (
            countryFlagImage != null &&
            countryState.CurrentScenario != null &&
            countryState.CurrentScenario.CountryFlag != null
        )
        {
            countryFlagImage.sprite =
                countryState.CurrentScenario.CountryFlag;

            countryFlagImage.enabled = true;
            countryFlagImage.preserveAspect = true;
        }


        // MAPA DEL PAÍS
        if (
            countryMapImage != null &&
            countryState.CurrentScenario != null &&
            countryState.CurrentScenario.CountryMap != null
        )
        {
            countryMapImage.sprite =
                countryState.CurrentScenario.CountryMap;

            countryMapImage.enabled = true;
            countryMapImage.preserveAspect = true;
        }


        // TIPO DE ESTADO
        if (stateTypeText != null)
        {
            stateTypeText.text =
                GetStateTypeName(
                    countryState.StateType
                );
        }


        // SISTEMA DE GOBIERNO
        if (governmentSystemText != null)
        {
            governmentSystemText.text =
                GetGovernmentSystemName(
                    countryState.GovernmentSystem
                );
        }


        // FECHA
        if (dateText != null)
        {
            int month = countryState.CurrentMonth;
            int year = countryState.CurrentYear;

            if (month >= 1 && month <= 12 && year > 0)
            {
                dateText.text =
                    $"{GetMonthName(month)} de {year}";
            }
            else
            {
                dateText.text =
                    string.Empty;
            }
        }


        // TURNO
        if (turnText != null)
        {
            turnText.text =
                $"Turno {countryState.CurrentTurn} de " +
                $"{countryState.MaximumTurns}";
        }


        // ELECCIONES
        if (electionCountdownText != null)
        {
            int remainingTurns =
                Mathf.Max(
                    0,
                    countryState.MaximumTurns -
                    countryState.CurrentTurn
                );

            if (remainingTurns == 0)
            {
                electionCountdownText.text =
                    "Elecciones ahora";
            }
            else if (remainingTurns == 1)
            {
                electionCountdownText.text =
                    "1 turno";
            }
            else
            {
                electionCountdownText.text =
                    $"{remainingTurns} turnos";
            }
        }
    }


    private string GetMonthName(
        int month
    )
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
            default: return string.Empty;
        }
    }


    private string GetStateTypeName(
        StateType stateType
    )
    {
        switch (stateType)
        {
            case StateType.DemocraticRepublic:
                return "República democrática";

            case StateType.AuthoritarianRepublic:
                return "República autoritaria";

            case StateType.ConstitutionalMonarchy:
                return "Monarquía constitucional";

            case StateType.AbsoluteMonarchy:
                return "Monarquía absoluta";

            case StateType.Dictatorship:
                return "Dictadura";

            default:
                return stateType.ToString();
        }
    }


    private string GetGovernmentSystemName(
        GovernmentSystem system
    )
    {
        switch (system)
        {
            case GovernmentSystem.Presidential:
                return "República presidencial";

            case GovernmentSystem.Parliamentary:
                return "Sistema parlamentario";

            case GovernmentSystem.SemiPresidential:
                return "Sistema semipresidencial";

            default:
                return system.ToString();
        }
    }
}