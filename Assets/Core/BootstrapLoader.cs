using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BootstrapLoader : MonoBehaviour
{
    [SerializeField]
    private string initialScene = "Economy";


    private void Awake()
    {
        // Este script está colocado en SimulationManager.
        // Por tanto hacemos persistente al objeto raíz completo,
        // incluyendo todos sus hijos/managers.

        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        SceneManager.LoadScene(initialScene);
    }
}