using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }

    [Header("UI 설정")]
    [SerializeField] TextMeshProUGUI _scoreText; // 스코어 UI
    [SerializeField] TextMeshProUGUI _timeText;  // 생존 시간 UI
    [SerializeField] private PlayerInput playerInput; // PlayerInput 연결

    [SerializeField] private GameObject pauseMenuUIOn;
    [SerializeField] private GameObject pauseMenuUIOff;
    [SerializeField] private GameObject pauseMenuSettingUI;

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverTimeText;

    bool isPaused = false; // 게임이 일시정지 상태인지 확인

    int _score = 0; // 현재 스코어
    float _startTime; // 게임 시작 시간

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;
        _startTime = Time.time; // 게임 시작 시간 저장
        UpdateScoreUI(); // 초기 스코어 설정
        UpdateTimeUI(); // 초기 시간 설정
    }

    void Update()
    {
        UpdateTimeUI(); // 매 프레임마다 생존 시간 업데이트

        if (Input.GetKeyDown(KeyCode.Escape)) // ESC 키로 토글
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // 스코어 추가 함수 (ShooterController나 EnemyController에서 호출)
    public void AddScore(int amount)
    {
        _score += amount;
        UpdateScoreUI();
    }
    // 스코어 UI 업데이트
    void UpdateScoreUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"KILL Count: {_score}";
        }
    }
    // 생존 시간 UI 업데이트
    void UpdateTimeUI()
    {
        if (_timeText != null)
        {
            float elapsedTime = Time.time - _startTime; // 현재 경과 시간
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            _timeText.text = $"Time: {minutes:00}:{seconds:00}"; // 00:00 형식
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // 게임 정지
        isPaused = true;

        if (playerInput != null)
        {
            playerInput.enabled = false; // 입력 차단
        }
        pauseMenuUIOn.SetActive(false);
        pauseMenuUIOff.SetActive(true);
        pauseMenuSettingUI.SetActive(true);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f; // 게임 속도 정상화
        isPaused = false;

        if (playerInput != null)
        {
            playerInput.enabled = true; // 입력 다시 활성화
        }
        pauseMenuUIOn.SetActive(true);
        pauseMenuUIOff.SetActive(false);
        pauseMenuSettingUI.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0f; // 게임 정지
        if (playerInput != null)
        {
            playerInput.enabled = false; // 입력 차단
        }

        // 생존 시간 가져오기
        float elapsedTime = Time.time - _startTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        // UI 업데이트
        gameOverScoreText.text = $"KILL Count: {_score}";
        gameOverTimeText.text = $"Time Survived: {minutes:00}:{seconds:00}";

        // Game Over UI 활성화
        gameOverUI.SetActive(true);
    }
}
