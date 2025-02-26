using UnityEngine;

// 모든 커맨드 클래스가 구현해야 하는 인터페이스
public interface ShooterICommand
{
    void Execute(); // 커맨드 실행 메서드
}

// 사격 커맨드 클래스
public class ShooterShootCommand : ShooterICommand
{
    private ShooterController _Shooter;

    // 생성자: 사격을 실행할 대상(Shooter) 설정
    public ShooterShootCommand(ShooterController shooter)
    {
        _Shooter = shooter;
    }

    public void Execute()
    {
        Debug.Log("사격 실행!"); // 콘솔에 사격 실행 로그 출력
    }
}
