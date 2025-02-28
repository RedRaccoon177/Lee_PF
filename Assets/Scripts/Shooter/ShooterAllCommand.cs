using UnityEngine;

// 모든 커맨드 클래스가 구현해야 하는 인터페이스
public interface ShooterICommand
{
    void Execute(); // 커맨드 실행 메서드
}

// 사격 커맨드 클래스
public class ShooterShootCommand : ShooterICommand
{
    ShooterController _shooter;
    ShooterAnimationManager _shooterAnimationManager;
    Transform _muzzleTransform;
    Transform _muzzleTransform1;
    ObjectPoolManager _objectPoolManager;

    float _speed;

    // 생성자: 사격을 실행할 대상(Shooter) 설정
    public ShooterShootCommand(ShooterController shooter, ShooterAnimationManager shooterAnimationManager, Transform muzzleTransform, Transform muzzleTransform1, ObjectPoolManager objectPoolManager, float speed)
    {
        _shooter = shooter;
        _shooterAnimationManager = shooterAnimationManager;
        _muzzleTransform = muzzleTransform;
        _muzzleTransform1 = muzzleTransform1;
        _objectPoolManager = objectPoolManager;
        _speed = speed;
    }

    public void Execute()
    {
        if (_shooter._isReloading) return;

        _objectPoolManager.SpawnMuzzleFlash(_muzzleTransform.position, _muzzleTransform.rotation);

        Vector3 start = _muzzleTransform1.position;
        Vector3 target = new Vector3(_shooter.GetAimTarget().x, _muzzleTransform.position.y, _shooter.GetAimTarget().z);
        _objectPoolManager.SpawnBulletTrail(start, target, _speed);

        _shooterAnimationManager.PlayIsShooting("IsShooting");
    }
}

public class ShooterReloadCommand : ShooterICommand 
{
    ShooterController _shooter;
    ShooterAnimationManager _shooterAnimationManager;
    
    public ShooterReloadCommand(ShooterController shooterController, ShooterAnimationManager shooterAnimationManager)
    {
        _shooter = shooterController;
        _shooterAnimationManager = shooterAnimationManager;
    }

    public void Execute() 
    {
        _shooterAnimationManager.PlayReload(_shooter._isReloading);
    }

}
