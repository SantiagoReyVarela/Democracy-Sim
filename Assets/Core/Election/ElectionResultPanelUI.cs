using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ElectionResultPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    private ElectionManager electionManager;

    private readonly CultureInfo spanishCulture =
        CultureInfo.GetCultureInfo("es-ES");


    private void Awake()
    {
        // Conectamos el botón una sola vez.
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinuePressed);
            continueButton.onClick.AddListener(OnContinuePressed);
        }
    }


    private void OnEnable()
    {
        /*
         * Cada vez que este objeto se activa,
         * lo mandamos al final de la jerarquía UI.
         *
         * En un Canvas de Unity, el último sibling
         * se dibuja por encima de los anteriores.
         */
        BringToFront();
    }


    /// <summary>
    /// Muestra el panel con los resultados electorales.
    /// </summary>
    public void Show(
        List<ElectionPartyResult> results,
        PoliticalPartySO winner,
        PoliticalPartySO playerParty,
        bool playerWon,
        ElectionManager manager)
    {
        electionManager = manager;

        // Primero activamos el panel.
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Después nos aseguramos de que está por encima de todo.
        BringToFront();


        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

        if (titleText != null)
        {
            titleText.text = "RESULTADOS ELECTORALES";
        }


        // ---------------------------------------------------------
        // RESULTADOS
        // ---------------------------------------------------------

        if (resultText != null)
        {
            resultText.text = BuildResultsText(
                results,
                winner,
                playerParty
            );
        }


        // ---------------------------------------------------------
        // RESULTADO DEL JUGADOR
        // ---------------------------------------------------------

        if (statusText != null)
        {
            if (playerWon)
            {
                statusText.text =
                    "HAS GANADO LAS ELECCIONES";
            }
            else
            {
                statusText.text =
                    "HAS PERDIDO";
            }
        }


        // ---------------------------------------------------------
        // BOTÓN CONTINUAR
        // ---------------------------------------------------------

        if (continueButton != null)
        {
            /*
             * Si gana:
             *     aparece el botón Continuar.
             *
             * Si pierde:
             *     desaparece y la partida queda detenida.
             */
            continueButton.gameObject.SetActive(playerWon);
            continueButton.interactable = playerWon;
        }
    }


    /// <summary>
    /// Construye el texto con todos los resultados.
    /// </summary>
    private string BuildResultsText(
        List<ElectionPartyResult> results,
        PoliticalPartySO winner,
        PoliticalPartySO playerParty)
    {
        if (results == null || results.Count == 0)
        {
            return "No hay resultados disponibles.";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < results.Count; i++)
        {
            ElectionPartyResult result = results[i];

            if (result == null || result.Party == null)
            {
                continue;
            }

            bool isWinner =
                result.Party == winner;

            bool isPlayer =
                result.Party == playerParty;


            // Estrella para el ganador.
            if (isWinner)
            {
                builder.Append("★ ");
            }


            // Nombre del partido.
            builder.Append(result.Party.partyName);


            // Indicamos cuál es el partido del jugador.
            if (isPlayer)
            {
                builder.Append(" (TÚ)");
            }


            builder.Append("   ");


            // Resultado electoral.
            builder.Append(
                result.ResultPercentage.ToString(
                    "0.00",
                    spanishCulture
                )
            );

            builder.Append("%");


            // Encuesta previa.
            builder.Append("   [Encuesta: ");

            builder.Append(
                result.PollPercentage.ToString(
                    "0.00",
                    spanishCulture
                )
            );

            builder.Append("%]");


            // Salto de línea excepto después del último.
            if (i < results.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }


    /// <summary>
    /// Coloca el panel electoral por encima
    /// de cualquier otro elemento UI hermano.
    /// </summary>
    private void BringToFront()
    {
        if (panelRoot == null)
        {
            return;
        }

        Transform panelTransform =
            panelRoot.transform;

        /*
         * SetAsLastSibling hace que el panel sea
         * el último elemento de su padre.
         *
         * En UI esto significa que se renderizará
         * por encima de los elementos anteriores.
         */
        panelTransform.SetAsLastSibling();
    }


    /// <summary>
    /// Botón Continuar después de ganar.
    /// </summary>
    private void OnContinuePressed()
    {
        if (electionManager == null)
        {
            Debug.LogWarning(
                "[ElectionResultPanelUI] " +
                "No hay ElectionManager asignado."
            );

            return;
        }

        electionManager.ContinueAfterVictory();
    }


    /// <summary>
    /// Oculta inmediatamente el panel.
    /// Lo utiliza ElectionManager después
    /// de continuar una partida ganada.
    /// </summary>
    public void HideImmediate()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}