using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public interface ICommand
{
    void Execute();         // 명령실행
    void StopExecute();     // 실행취소
    
    //void Redo();    // 재실행

}
public class MoveCommand : ICommand
{
    Animator _animator;
    NavMeshAgent _agent;                    // 내비매쉬
    AnimationManager _animationManager;     // 애니메이션 매니져
    Vector3 _destination;                   // Vector3 목적지
    MonoBehaviour _monoBehaviour;           // 모노비헤이비어 (코루틴을 위해서)
    float _speed;

    public MoveCommand(Animator animator, NavMeshAgent agent, AnimationManager animationManager, Vector3 destination, MonoBehaviour monoBehaviour, float speed)
    {
        _animator = animator;
        _agent = agent;
        _animationManager = animationManager;
        _destination = destination;
        _monoBehaviour = monoBehaviour;
        _speed = speed;
    }

    public void Execute()
    {
        _animationManager.PlayWalkAnimation(true);  // 애니메이션 매니저를 통해 실행

        _agent.SetDestination(_destination);    //내비매쉬로 이동

        _monoBehaviour.StartCoroutine(CheckIfReachedDestination()); //목적지까지의 코루틴 확인
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
    PlayerController _playerController;
    AnimationManager _animationManager;
    Transform _transform;
    Vector3 _skillQRange; // 박스 크기
    Vector3 _skillQCenter; // 박스 위치 오프셋
    LayerMask _targetLayer; // 감지할 레이어

    public SkillQCommand(AnimationManager animationManager, Transform transform, Vector3 skillQRange, Vector3 skillQCenter, LayerMask targetLayer) 
    {
        _animationManager = animationManager;
        _transform = transform;
        _skillQRange = skillQRange;
        _skillQCenter = skillQCenter;
        _targetLayer = targetLayer;

    }

    public void Execute()
    {
        if(_playerController._isPlaying) return;
        
        _playerController._isPlaying = true;

        _animationManager.PlaySkillAnimation("IsSkillQ"); // Q 스킬 애니메이션 실행

        Vector3 boxCenter = _transform.position + _skillQCenter; // 박스의 중심

        // OverlapBox를 사용하여 박스 내에 존재하는 모든 Collider 가져오기
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, _skillQRange / 2, _transform.rotation, _targetLayer);

        foreach (Collider col in hitColliders)
        {
            Debug.Log($"감지된 오브젝트: {col.gameObject.name}");
        }

        _playerController._isPlaying = false;
    }

    public void StopExecute()
    {

    }


}



