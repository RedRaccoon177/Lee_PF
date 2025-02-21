using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttackRange : MonoBehaviour
{
    public LayerMask targetLayer; // 감지할 대상 (플레이어, 적 등)
    private List<Collider> enemiesInRange = new List<Collider>(); // 현재 감지된 적 목록

    void OnTriggerEnter(Collider other)
    {
        // 감지할 레이어에 속한 오브젝트인지 확인
        // 1 << other.gameObject.layer은 1을 other.gameObject.layer의 값만큼 이동시킨다.
        // (비교한 값) & targetLayer는 비교했을때 
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Debug.Log($"적 감지됨! {other.gameObject.name}");
            enemiesInRange.Add(other);
        }
    }
    void OnTriggerExit(Collider other)
    {
        // 리스트에서 제거
        if (enemiesInRange.Contains(other))
        {
            Debug.Log($"적 범위에서 나감! {other.gameObject.name}");
            enemiesInRange.Remove(other);
        }
    }

    public void AttackNearestEnemy()
    {
        if (enemiesInRange.Count > 0)
        {
            Collider nearestEnemy = enemiesInRange[0]; // 첫 번째 적
            Debug.Log($"가장 가까운 적 공격: {nearestEnemy.gameObject.name}");
        }
    }
}
