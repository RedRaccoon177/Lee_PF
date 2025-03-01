using UnityEditor.EditorTools;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적의 최대 체력")]
    public int maxHealth = 100;
    public float currentHealth;

    [Header("적 사망 이펙트")]
    public GameObject deathEffect; // 죽을 때 이펙트 (선택)

    void Start()
    {
        currentHealth = maxHealth; // 시작 체력 설정
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 피격! 남은 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        EnemyController enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.Die(); // EnemyController의 Die()를 호출
        }

        // 사망 이펙트 생성 (선택 사항)
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        ObjectPoolManager.Instance.MonsterRelease(gameObject);
    }
}
