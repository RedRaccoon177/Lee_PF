using UnityEngine;

// 캐릭터의 애니메이션을 관리하는 클래스
public class ShooterAnimationManager : MonoBehaviour
{
    private Animator _animator; // Unity의 애니메이션 시스템을 제어하는 Animator 컴포넌트

    // 애니메이션 방향 전환을 위한 현재 방향 값 저장 (부드러운 전환을 위해 사용)
    private float currentDirectionX = 0f;
    private float currentDirectionY = 0f;

    // 애니메이션 전환을 부드럽게 하기 위한 속도 변수 (값이 작을수록 전환이 느려짐)
    private float smoothTime = 0.1f;

    private void Awake()
    {
        // 이 스크립트가 부착된 게임 오브젝트에서 Animator 컴포넌트를 가져옴
        _animator = GetComponent<Animator>();
    }

    // 이동 속도를 설정하는 함수 (애니메이터의 MoveSpeed 값을 변경)
    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat("MoveSpeed", speed); // MoveSpeed 값을 애니메이터에 전달하여 애니메이션 조정
    }

    // 방향을 설정하는 함수 (부드러운 전환을 위해 Lerp 사용)
    public void SetDirection(float targetX, float targetY)
    {
        // Mathf.Lerp()를 사용하여 방향 값을 천천히 목표 값으로 변경 (애니메이션 전환이 급격하지 않도록 함)
        currentDirectionX = Mathf.Lerp(currentDirectionX, targetX, Time.deltaTime / smoothTime);
        currentDirectionY = Mathf.Lerp(currentDirectionY, targetY, Time.deltaTime / smoothTime);

        // 변경된 방향 값을 애니메이터에 설정 (Blend Tree에서 사용됨)
        _animator.SetFloat("DirectionX", currentDirectionX);
        _animator.SetFloat("DirectionY", currentDirectionY);
    }

    // 공격 애니메이션 실행 함수 (애니메이터의 트리거 사용)
    public void PlayIsShooting(string IsShooting)
    {
        _animator.SetTrigger(IsShooting); // 공격 애니메이션 트리거 실행
    }

    public void PlayReload(bool IsReload)
    {
        _animator.SetBool("IsReload", IsReload);
    }

    // 스킬 애니메이션 실행 함수 (애니메이터의 트리거 사용)
    public void PlaySkillAnimation(string skillName)
    {
        _animator.SetTrigger(skillName); // 특정 스킬 애니메이션 트리거 실행
    }
}
