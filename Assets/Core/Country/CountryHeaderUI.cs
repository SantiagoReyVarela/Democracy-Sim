using TMPro;
using UnityEngine;

public class CountryHeaderUI : MonoBehaviour
{
    [Header("Country")]
    [SerializeField] private TMP_Text countryNameText;
    [SerializeField] private TMP_Text stateTypeText;
    [SerializeField] private TMP_Text governmentSystemText;

    [Header("Turn")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text electionCountdownText;

    private CountryState countryState;


    private void OnEnable()
    {
        BindCountryState();
        Refresh();
    }


    private void Start()
    {
        BindCountryState();
        Refresh();
    }


    private void OnDisable()
    {
        UnbindCountryState();
    }


    private void BindCountryState()
    {
        if (countryState == CountryState.Instance)
        {
            return;
        }

        UnbindCountryState();

        countryState = CountryState.Instance;

        if (countryState != null)
        {
            countryState.OnStateChanged += Refresh;
        }
    }


    private void UnbindCountryState()
    {
        if (countryState != null)
        {
            countryState.OnStateChanged -= Refresh;
        }

        countryState = null;
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


        // NOMBRE DEL PAÍS
        if (countryNameText != null)
        {
            countryNameText.text =
                countryState.CountryName.ToUpper();
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