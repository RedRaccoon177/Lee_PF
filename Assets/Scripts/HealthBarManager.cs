using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance { get; private set; } // 싱글톤 패턴

    [SerializeField] private GameObject healthBarPrefab; // 체력바 프리팹 (Slider UI)
    [SerializeField] private Transform healthBarContainer; // 모든 체력바를 담을 UI 컨테이너 (Canvas 내부)
    
    private Dictionary<EnemyController, RectTransform> healthBars = new Dictionary<EnemyController, RectTransform>(); // 적과 체력바 매핑

    void Awake()
    {
        if (Instance == null) Instance = this; // 싱글톤 인스턴스 설정
        else Destroy(gameObject); // 중복된 매니저 삭제
    }

    // 연산 최적화를 위해 Update 대신 LateUpdate 사용
    void LateUpdate()
    {
        List<EnemyController> toRemove = new List<EnemyController>();

        foreach (var entry in healthBars)
        {
            EnemyController enemy = entry.Key;
            RectTransform healthBar = entry.Value;

            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                toRemove.Add(enemy);
                continue;
            }

            // 체력이 0이면 체력바 숨김
            if (enemy.GetHealthPercentage() <= 0f)
            {
                healthBar.gameObject.SetActive(false);
                continue;
            }

            // 화면 안에 있는 적만 체력바 업데이트
            if (IsEnemyVisible(enemy))
            {
                UpdateHealthBarPosition(enemy);
                healthBar.GetComponent<Slider>().value = enemy.GetHealthPercentage();
            }
        }

        // 사라진 적의 체력바 제거
        foreach (var enemy in toRemove)
        {
            UnregisterEnemy(enemy);
        }
    }

    // 적이 생성되었을 때 체력바를 추가하는 함수
    public void RegisterEnemy(EnemyController enemy)
    {
        if (!healthBars.ContainsKey(enemy)) // 적이 처음 생성된 경우만 체력바 추가
        {
            GameObject newHealthBar = Instantiate(healthBarPrefab, healthBarContainer);
            RectTransform healthBar = newHealthBar.GetComponent<RectTransform>();

            // 처음에는 화면 밖으로 배치 후 비활성화
            healthBar.position = new Vector3(-1000, -1000, 0);
            healthBar.gameObject.SetActive(false);

            healthBars[enemy] = healthBar;
        }

        // 새롭게 생성된 적은 체력도 초기화해야 함
        ResetHealthBar(enemy);

        // 적이 처음 등장할 때 강제로 체력바 업데이트 실행
        UpdateHealthBarPosition(enemy);

        // 강제로 체력바 활성화 (첫 번째 적을 위해 추가)
        if (IsEnemyVisible(enemy))
        {
            healthBars[enemy].gameObject.SetActive(true);
        }
    }

    private void ResetHealthBar(EnemyController enemy)
    {
        if (healthBars.TryGetValue(enemy, out RectTransform healthBar))
        {
            healthBar.GetComponent<Slider>().value = 1f; // 체력 100%로 초기화
            healthBar.position = new Vector3(-1000, -1000, 0); // 화면 밖으로 배치
            healthBar.gameObject.SetActive(false); // 비활성화
        }
    }

    // 적이 제거될 때 체력바도 삭제하는 함수
    public void UnregisterEnemy(EnemyController enemy)
    {
        if (healthBars.ContainsKey(enemy))
        {
            healthBars[enemy].gameObject.SetActive(false); // 체력바 즉시 숨기기
            healthBars[enemy].position = new Vector3(-1000, -1000, 0); // 화면 밖으로 이동
            healthBars.Remove(enemy); // Dictionary에서 제거
        }
    }

    // 적의 위치에 따라 체력바를 UI에 갱신하는 함수
    public void UpdateHealthBarPosition(EnemyController enemy)
    {
        if (enemy == null || !healthBars.ContainsKey(enemy)) return;

        RectTransform healthBar = healthBars[enemy];

        // 월드 좌표 → 스크린 좌표 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up * 4f);

        // 체력 확인 (체력이 0이면 숨김)
        float healthValue = enemy.GetHealthPercentage();
        if (healthValue <= 0f)
        {
            healthBar.gameObject.SetActive(false);
            return;
        }

        // 강제 활성화 조건 추가 (첫 번째 적도 제대로 표시되도록)
        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
        {
            healthBar.position = screenPos;
            if (!healthBar.gameObject.activeSelf)
            {
                healthBar.gameObject.SetActive(true);
            }
        }
        else
        {
            healthBar.position = new Vector3(-1000, -1000, 0);
            healthBar.gameObject.SetActive(false);
        }
    }

    // 카메라 안에 있는 적인지 확인하는 함수 (카메라에 보이는 적만 연산)
    private bool IsEnemyVisible(EnemyController enemy)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
        return screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height;
    }
}
