using UnityEngine;

public class CountryStateTest : MonoBehaviour
{
    [SerializeField]
    private CountryState countryState;

    [SerializeField]
    private PolicyDefinitionSO testPolicy;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            countryState.ApplyPolicy(testPolicy);
            countryState.AdvanceTurn();

            Debug.Log(
                $"Turn: {countryState.CurrentTurn} | " +
                $"Prosperity: {countryState.Prosperity} | " +
                $"Equality: {countryState.Equality} | " +
                $"Employment: {countryState.Employment} | " +
                $"Education: {countryState.Education} | " +
                $"Stability: {countryState.Stability}"
            );
        }
    }
}