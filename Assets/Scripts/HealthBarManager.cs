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

    // 적이 생성되었을 때 체력바를 추가하는 함수
    public void RegisterEnemy(EnemyController enemy)
    {
        if (!healthBars.ContainsKey(enemy)) // 이미 등록된 적이 아니라면
        {
            GameObject newHealthBar = Instantiate(healthBarPrefab, healthBarContainer); // 새로운 체력바 생성
            RectTransform healthBar = newHealthBar.GetComponent<RectTransform>(); // RectTransform 참조 가져오기
            healthBars[enemy] = healthBar; // Dictionary에 저장
        }

        // 풀링된 몬스터가 다시 활성화될 때 체력바 위치 초기화
        UpdateHealthBarPosition(enemy);
    }

    // 적이 제거될 때 체력바도 삭제하는 함수
    public void UnregisterEnemy(EnemyController enemy)
    {
        if (healthBars.ContainsKey(enemy)) // 적이 존재하면
        {
            Destroy(healthBars[enemy].gameObject); // 체력바 UI 삭제
            healthBars.Remove(enemy); // Dictionary에서 제거
        }
    }

    // 적의 위치에 따라 체력바를 UI에 갱신하는 함수
    public void UpdateHealthBarPosition(EnemyController enemy)
    {
        if (enemy == null || !healthBars.ContainsKey(enemy)) return; // 적이 없거나 체력바가 없으면 무시

        RectTransform healthBar = healthBars[enemy]; // 해당 적의 체력바 가져오기

        // 월드 좌표 → 스크린 좌표 변환 (적 머리 위로 이동)
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up * 4f);

        // 카메라 뒤쪽에 있을 경우 체력바를 숨김
        if (screenPos.z > 0)
        {
            healthBar.position = screenPos; // 체력바 위치 업데이트
            healthBar.gameObject.SetActive(true); // 체력바 활성화
        }
        else
        {
            healthBar.gameObject.SetActive(false); // 체력바 숨김
        }
    }

    // 연산 최적화를 위해 `Update()` 대신 `LateUpdate()` 사용
    void LateUpdate()
    {
        List<EnemyController> toRemove = new List<EnemyController>(); // 삭제할 적 목록

        foreach (var entry in healthBars)
        {
            EnemyController enemy = entry.Key;
            RectTransform healthBar = entry.Value;

            if (enemy == null || !enemy.gameObject.activeInHierarchy) // 적이 사라졌거나 비활성화되었을 경우
            {
                toRemove.Add(enemy); // 삭제 리스트에 추가
                continue;
            }

            // 카메라에 보이는 적만 체력바 갱신
            if (IsEnemyVisible(enemy))
            {
                UpdateHealthBarPosition(enemy); // 적의 위치에 맞게 체력바 갱신
                healthBar.GetComponent<Slider>().value = enemy.GetHealthPercentage(); // 체력 퍼센트 UI 업데이트
            }
        }

        // 비활성화된 적들의 체력바 제거
        foreach (var enemy in toRemove)
        {
            UnregisterEnemy(enemy);
        }
    }

    // 카메라 안에 있는 적인지 확인하는 함수 (카메라에 보이는 적만 연산)
    private bool IsEnemyVisible(EnemyController enemy)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
        return screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height;
    }
}
