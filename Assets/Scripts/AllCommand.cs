using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public interface ICommand
{
    void Execute();         // 명령실행
    void StopExecute();     // 실행취소
}

public class MoveCommand : ICommand
{
    NavMeshAgent _agent;
    AnimationManager _animationManager;
    Vector3 _destination;
    MonoBehaviour _monoBehaviour;
    float _speed;
    bool _isATrue;
    Transform _transform;
    float _detectionRadius = 5f;
    LayerMask _autofindEnemy;
    float _attackdetection = 2.5f;

    GameObject closestEnemy = null;
    GameObject targetEnemy = null;

    public MoveCommand(NavMeshAgent agent, AnimationManager animationManager, Vector3 destination, MonoBehaviour monoBehaviour, float speed, bool isATrue, Transform transform, LayerMask autofindEnemy)
    {
        _agent = agent;
        _animationManager = animationManager;
        _destination = destination;
        _monoBehaviour = monoBehaviour;
        _speed = speed;
        _isATrue = isATrue;
        _transform = transform;
        _autofindEnemy = autofindEnemy;
    }

    public void Execute()
    {
        _monoBehaviour.StopAllCoroutines();

        _animationManager.PlayWalkAnimation(true);
        _agent.SetDestination(_destination);
        _monoBehaviour.StartCoroutine(CheckIfReachedDestination());

        if (_isATrue)
        {
            _monoBehaviour.StartCoroutine(AutoFindEnemy());
        }
    }

    public void StopExecute()
    {
        _agent.ResetPath();
        _animationManager.PlayWalkAnimation(false);
        _animationManager.PlayRushAnimation(false);
    }

    // 목적지에 도착했는지 확인하는 코루틴
    IEnumerator CheckIfReachedDestination()
    {
        yield return new WaitForSeconds(0.1f);

        while (!_agent.isStopped && _agent.remainingDistance > _agent.stoppingDistance)
        {
            yield return null;
        }

        _agent.speed = _speed;

        //이동 완료 후 애니메이션 정지
        _animationManager.PlayWalkAnimation(false);
        _animationManager.PlayRushAnimation(false);
    }

    IEnumerator AutoFindEnemy()
    {
        float closestDistance = Mathf.Infinity;

        while (closestEnemy == null)
        {
            Collider[] findEnemySphere = Physics.OverlapSphere(_transform.position, _detectionRadius, _autofindEnemy);

            foreach (Collider col in findEnemySphere)
            {
                float distance = Vector3.Distance(_transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = col.gameObject;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        if(closestEnemy != null)
        {
            _monoBehaviour.StartCoroutine(GoToEnemy());
        }
    }

    IEnumerator GoToEnemy()
    {
        _agent.SetDestination(closestEnemy.transform.position);

        while (targetEnemy != closestEnemy)
        {
            Collider[] isEnemycontactAttackzone = Physics.OverlapSphere(_transform.position, _attackdetection, _autofindEnemy);

            foreach (Collider col in isEnemycontactAttackzone)
            {
                if (closestEnemy == col.gameObject)
                {
                    targetEnemy = closestEnemy;
                }
            }
            yield return new WaitForSeconds(0.05f);
        }

        if (closestEnemy != null && targetEnemy != null && closestEnemy == targetEnemy)
        {
            _monoBehaviour.StartCoroutine(AttackEnemy(targetEnemy));
        }
    }

    IEnumerator AttackEnemy(GameObject targetEnemy)
    {
        GameObject enemy = targetEnemy.GetComponent<EnemyHealth>().gameObject;
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

        while (enemyHealth.currentHealth > 0)
        {
            _transform.LookAt(enemy.transform.position);
            _agent.ResetPath();
            _animationManager.PlayIsAttack("IsAttack");

            enemyHealth.TakeDamage(20);

            yield return new WaitForSeconds(1f);
        }

        _monoBehaviour.StartCoroutine(AutoFindEnemy());
    }
}


public class RushCommand : ICommand
{
    NavMeshAgent _agent;                    // 내비매쉬
    AnimationManager _animationManager;     // 애니메이션 매니져
    float _originalSpeed;
    float _rushSpeed;

    public RushCommand(NavMeshAgent agent, AnimationManager animationManager, float originalSpeed, float rushSpeed)
    {
        _agent = agent;
        _animationManager = animationManager;
        _originalSpeed = originalSpeed;
        _rushSpeed = rushSpeed;
    }

    public void Execute()
    {
        if (!_agent.isStopped && _agent.hasPath)
        {
            _agent.speed = _originalSpeed * _rushSpeed;
            _animationManager.PlayRushAnimation(true); // 달리기 애니메이션
        }
    }

    public void StopExecute()
    {
        _animationManager.PlayRushAnimation(false);
        _agent.speed = _originalSpeed;
    }
}

public class StopCommand : ICommand
{
    NavMeshAgent _agent;
    AnimationManager _animationManager;

    public StopCommand(NavMeshAgent agent, AnimationManager animationManager)
    {
        _agent = agent;
        _animationManager = animationManager;
    }

    //TODO: ▼ 모든 커맨드 정지
    public void Execute()
    {
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        //_agent.isStopped = true;

        // TODO: 모든 상태 및 애니메이션 정지

        //_animationManager.PlayIdleAnimation(true);
        _animationManager.PlayWalkAnimation(false);
        _animationManager.PlayRushAnimation(false);
    }

    public void StopExecute()
    {

    }
}

public class SkillQCommand : ICommand
{
    AnimationManager _animationManager;
    Transform _transform;
    Vector3 _skillQRange; // 박스 크기
    Vector3 _playerSkillQPosition; // 박스 위치 오프셋
    LayerMask _targetLayer; // 감지할 레이어
    NavMeshAgent _agent;

    public SkillQCommand(AnimationManager animationManager, Transform transform, Vector3 skillQRange, Vector3 playerSkillQPosition, LayerMask targetLayer, NavMeshAgent agent) 
    {
        _animationManager = animationManager;
        _transform = transform;
        _skillQRange = skillQRange;
        _playerSkillQPosition = playerSkillQPosition;
        _targetLayer = targetLayer;
        _agent = agent;
    }

    public void Execute()
    {
        _agent.ResetPath();
        _animationManager.PlaySkillAnimation("IsSkillQ"); // Q 스킬 애니메이션 실행

        // OverlapBox를 사용하여 박스 내에 존재하는 모든 Collider 가져오기
        Collider[] hitColliders = Physics.OverlapBox(_playerSkillQPosition, _skillQRange / 2, _transform.rotation, _targetLayer);

        foreach (Collider col in hitColliders)
        {
            Debug.Log($"감지된 오브젝트: {col.gameObject.name}");
        }
    }

    public void StopExecute()
    {

    }
}

