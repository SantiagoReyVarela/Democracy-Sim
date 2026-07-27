using UnityEngine;
using UnityEngine.InputSystem;

public class CountryStateTest : MonoBehaviour
{
    [SerializeField]
    private CountryState countryState;

    [SerializeField]
    private PolicyDefinitionSO testPolicy;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RunTest();
        }
    }

    private void RunTest()
    {
        if (countryState == null)
        {
            Debug.LogError(
                "CountryStateTest: no se ha asignado CountryState."
            );

            return;
        }

        if (testPolicy == null)
        {
            Debug.LogError(
                "CountryStateTest: no se ha asignado la política de prueba."
            );

            return;
        }

        countryState.ApplyPolicy(testPolicy);
        countryState.AdvanceTurn();

        Debug.Log(
            $"Turno: {countryState.CurrentTurn} | " +
            $"Prosperidad: {countryState.Prosperity} | " +
            $"Igualdad: {countryState.Equality} | " +
            $"Empleo: {countryState.Employment} | " +
            $"Educación: {countryState.Education} | " +
            $"Estabilidad: {countryState.Stability}"
        );
    }
}