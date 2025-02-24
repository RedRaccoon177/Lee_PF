using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적의 최대 체력")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("적 사망 이펙트")]
    public GameObject deathEffect; // 죽을 때 이펙트 (선택)

    private void Start()
    {
        currentHealth = maxHealth; // 시작 체력 설정
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 피격! 남은 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " 사망!");

        // 사망 이펙트 생성 (선택 사항)
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 적 삭제
        Destroy(gameObject);
    }
}
