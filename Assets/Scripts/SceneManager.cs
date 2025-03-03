using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리 기능

public class SceneManager : MonoBehaviour
{
    public static SceneManager _instance { get; private set; }

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    // 인게임 씬 이동
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGame"); // 인게임 씬 로드
    }

    // 게임 오버 → 게임 오버 씬 이동
    public void GameOver()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // 게임 오버 씬 로드
    }

    // 메인 메뉴로 이동
    public void ReturnToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // 메인 메뉴 씬 로드
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
