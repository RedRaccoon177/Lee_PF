using UnityEngine;
using UnityEngine.UI;

public class ShooterHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f; // 최대 체력
    private float currentHealth; // 현재 체력

    [SerializeField] private Slider healthSlider; // UI 슬라이더

    void Start()
    {
        currentHealth = maxHealth; // 시작 시 체력을 최대값으로 설정
        UpdateHealthUI();
    }

    // 체력 감소 함수
    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // 체력 감소
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 체력이 0~최대값 범위를 벗어나지 않도록 제한
        UpdateHealthUI();
    }

    // 체력 회복 함수
    public void Heal(float healAmount)
    {
        currentHealth += healAmount; // 체력 회복
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    // 슬라이더 UI 업데이트
    private void UpdateHealthUI()
    {
        healthSlider.value = currentHealth / maxHealth; // 슬라이더 값 (0 ~ 1)
    }
}
