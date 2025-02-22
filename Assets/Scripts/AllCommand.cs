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
    Animator _animator;
    NavMeshAgent _agent;                    // 내비매쉬
    AnimationManager _animationManager;     // 애니메이션 매니져
    Vector3 _destination;                   // Vector3 목적지
    MonoBehaviour _monoBehaviour;           // 모노비헤이비어 (코루틴을 위해서)
    float _speed;
    bool _isATrue;
    Transform _transform;
    float _detectionRadius = 5f;
    LayerMask _autofindEnemy;

    public MoveCommand(Animator animator, NavMeshAgent agent, AnimationManager animationManager, Vector3 destination, MonoBehaviour monoBehaviour, float speed, bool isATrue, Transform transform, LayerMask autofindEnemy)
    {
        _animator = animator;
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
        _animationManager.PlayWalkAnimation(true);  // 애니메이션 매니저를 통해 실행
        _agent.SetDestination(_destination);    //내비매쉬로 이동
        _monoBehaviour.StartCoroutine(CheckIfReachedDestination()); //목적지까지의 코루틴 확인
        if (_isATrue)
        {
            Debug.Log("실행됨?");
            _monoBehaviour.StartCoroutine(AutoFindEnemy());
        }
    }

    public void StopExecute()
    {
        _agent.ResetPath();
     
        _animationManager.PlayWalkAnimation(false); // 애니메이션 정지
    }

    /// <summary>
    /// 목적지에 도착했는지 확인하는 함수
    /// </summary>
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
        GameObject closestEnemy = null;

        while (closestEnemy == null)
        {
            //적 탐지 시작(0.05초 마다)
            yield return new WaitForSeconds(0.05f);

            Collider[] hitColliders = Physics.OverlapSphere(_transform.position, _detectionRadius, _autofindEnemy);
            float closestDistance = Mathf.Infinity;

            foreach (Collider col in hitColliders)
            {
                float distance = Vector3.Distance(_transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = col.gameObject;
                }
            }
        }

        //_monoBehaviour.StopCoroutine();
        if (closestEnemy != null)
        {
            _agent.SetDestination(closestEnemy.transform.position);

            //목적지로 가다가 만약 또다른 기즈모를 접촉한다면 정지 후 공격 시작
        }

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

