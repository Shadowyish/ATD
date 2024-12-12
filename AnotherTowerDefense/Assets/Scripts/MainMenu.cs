using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void StartGame() {
        SceneManager.LoadScene("Gameplay");
    }
    public void QuitGame() {
        Application.Quit();
    }
}