using UnityEditor.EditorTools;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적의 최대 체력")] public int maxHealth = 100;
    [Header("적의 현재 체력")] public float currentHealth;

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

        ObjectPoolManager.Instance.SpawnEnemyDieParticle(transform.position, transform.rotation);
        ObjectPoolManager.Instance.MonsterRelease(gameObject);
    }
}
