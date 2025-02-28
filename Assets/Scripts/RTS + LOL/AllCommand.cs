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

    bool _isSearching = false;

    public Coroutine IsAttackCoroutine;
    public Coroutine AutoFindEnemyCoroutine;
    public Coroutine CheckIfReachedDestinationCoroutine;

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

    public void SetDestination(NavMeshAgent agent, AnimationManager animationManager, Vector3 destination, float speed, bool isATrue, Transform transform, LayerMask autofindEnemy)
    {
        StopExecute();
        _agent = agent;
        _animationManager = animationManager;
        _destination = destination;
        _speed = speed;
        _isATrue = isATrue;
        _transform = transform;
        _autofindEnemy = autofindEnemy;
    }

    public void Execute()
    {
        StopExecute();
        _animationManager.PlayWalkAnimation(true);

        _agent.SetDestination(_destination);
        CheckIfReachedDestinationCoroutine = _monoBehaviour.StartCoroutine(CheckIfReachedDestination());

        if (_isATrue)
        {
            _isSearching = true;
            IsAttackCoroutine = _monoBehaviour.StartCoroutine(IsAttack());
            AutoFindEnemyCoroutine = _monoBehaviour.StartCoroutine(AutoFindEnemy());
        }
    }

    public void StopExecute()
    {
        _isSearching = false; // 모든 코루틴이 즉시 탈출할 수 있도록 설정
        _agent.ResetPath();

        _animationManager.PlayWalkAnimation(false);
        _animationManager.PlayRushAnimation(false);
        _animationManager.PlayIdleAnimation(true);

        if (IsAttackCoroutine != null)
        {
            _monoBehaviour.StopCoroutine(IsAttackCoroutine);
            IsAttackCoroutine = null;
        }
        if (AutoFindEnemyCoroutine != null)
        {
            _monoBehaviour.StopCoroutine(AutoFindEnemyCoroutine);
            AutoFindEnemyCoroutine = null;
        }
        if (CheckIfReachedDestinationCoroutine != null)
        {
            _monoBehaviour.StopCoroutine(CheckIfReachedDestinationCoroutine);
            CheckIfReachedDestinationCoroutine = null;
        }
    }

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
        closestEnemy = null;
        targetEnemy = null;

        float closestDistance = Mathf.Infinity;
        while (_isSearching == true)
        {
            if (!_isSearching) yield break;

            Collider[] outerCircle = Physics.OverlapSphere(_transform.position, _detectionRadius, _autofindEnemy);
            foreach (Collider col in outerCircle)
            {
                float distance = Vector3.Distance(_transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = col.gameObject;
                }
            }

            Collider[] isEnemycontactAttackzone = Physics.OverlapSphere(_transform.position, _attackdetection, _autofindEnemy);
            foreach (Collider col in isEnemycontactAttackzone)
            {
                if (closestEnemy == col.gameObject)
                {
                    targetEnemy = closestEnemy;
                }
            }

            Debug.Log(closestEnemy+ "  " + targetEnemy);

            if (closestEnemy != null && targetEnemy == null)
            {
                _agent.SetDestination(closestEnemy.transform.position);
            }
            else if (closestEnemy != null && targetEnemy != null && closestEnemy == targetEnemy)
            {
                _agent.ResetPath(); // 경로 초기화
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }
    
    IEnumerator IsAttack()
    {
        closestEnemy = null;
        targetEnemy = null;

        while (_isSearching)
        {
            if (!_isSearching) yield break;

            while (closestEnemy != null && targetEnemy != null && targetEnemy == closestEnemy)
            {
                if (!_isSearching) yield break;

                _transform.LookAt(targetEnemy.transform.position);

                _animationManager.PlayIsAttack("IsAttack");

                //"IsAttack"의 애니메이션이 끝나면 다시
                yield return new WaitForSeconds(1f);

                
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }

    public GameObject GetTargetEnemy()
    {
        return targetEnemy;
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

    public StopCommand(NavMeshAgent agent, AnimationManager animationManager, MonoBehaviour monoBehaviour)
    {
        _agent = agent;
        _animationManager = animationManager;
    }

    //TODO: ▼ 모든 커맨드 정지
    public void Execute()
    {
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

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
        Collider[] enemycol = Physics.OverlapBox(_playerSkillQPosition, _skillQRange / 2, _transform.rotation, _targetLayer);

        foreach (Collider col in enemycol)
        {
            col.GetComponent<EnemyHealth>().TakeDamage(40);
        }
    }

    public void StopExecute()
    {

    }
}

