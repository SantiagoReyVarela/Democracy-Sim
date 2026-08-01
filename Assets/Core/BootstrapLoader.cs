using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BootstrapLoader : MonoBehaviour
{
    [SerializeField]
    private string initialScene = "Economy";

    private void Start()
    {
        SceneManager.LoadScene(initialScene);
    }
}