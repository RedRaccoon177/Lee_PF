using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // 싱글톤 패턴

    private int score = 0; // 현재 점수
    private float elapsedTime = 0f; // 게임 시작 후 경과 시간
    private bool isGameRunning = true; // 게임 진행 여부

    [SerializeField] private Text scoreText; // 점수를 표시할 UI
    [SerializeField] private Text timeText;  // 시간을 표시할 UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // 싱글톤 유지

        DontDestroyOnLoad(gameObject); // 씬이 변경되어도 삭제되지 않도록 설정
    }

    private void Start()
    {
        StartCoroutine(GameTimer()); // 시간 흐름 시작
        UpdateUI(); // 초기 UI 업데이트
    }

    // 적을 처치할 때 점수 증가 함수
    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    // UI 갱신 (점수 & 시간)
    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (timeText != null) timeText.text = $"Time: {elapsedTime:F1}s";
    }

    // 게임 시간 흐름을 처리하는 코루틴
    private IEnumerator GameTimer()
    {
        while (isGameRunning)
        {
            elapsedTime += Time.deltaTime; // 경과 시간 증가
            UpdateUI(); // UI 업데이트
            yield return null; // 다음 프레임까지 대기
        }
    }

    // 게임 종료 함수
    public void EndGame()
    {
        isGameRunning = false;
        Debug.Log($"Game Over! Final Score: {score}, Time: {elapsedTime:F1}s");
    }
}
