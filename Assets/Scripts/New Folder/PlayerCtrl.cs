using UnityEngine;
using UnityEngine.AI;

public class PlayerCtrl : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent가 없습니다!");
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        Debug.Log("플레이어 이동: " + targetPosition);
    }

    public void Attack()
    {
        Debug.Log("플레이어 공격 실행!");
        // 공격 애니메이션 추가 가능
    }
}
