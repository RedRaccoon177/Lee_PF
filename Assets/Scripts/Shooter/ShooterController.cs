using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class ShooterController : MonoBehaviour
{
    //싱글톤 선언
    public static ShooterController _instance { get; private set; }

    ShooterAnimationManager _shooterAnimationManager;
    public Rigidbody _rb;

    // 이동 입력값을 저장할 변수 (입력된 키 값에 따라 이동 방향을 결정)
    Vector3 _moveInput;
    public Vector3 moveInput { get => _moveInput; }

    // 현재 달리기 상태를 저장하는 변수 (true면 달리는 중, false면 걷는 중)
    bool _isRunning = false;
    public bool isRunning { get => _isRunning; }

    // 걷기 속도 설정
    public float WalkSpeed = 4f;
    // 달리기 속도 설정
    public float RunSpeed = 8f;

    // 입력 시스템 관련 변수
    PlayerInput _playerInput;
    InputAction _moveAction;
    InputAction _runAction;
    InputAction _shootAction;
    InputAction _reloadAction;

    // 메인 카메라 (마우스 위치 계산에 사용)
    public Camera mainCamera;

    //커맨드 패턴 활용
    ShooterShootCommand _shooterShootCommand;
    ShooterReloadCommand _shooterReloadCommand;

    //총구 화염 오브젝트 풀
    public ObjectPoolManager _muzzlePoolManager;
    public Transform _muzzleTransform;
    public Transform _muzzleTransform1;

    Vector3 _targetEemeyPosition = new Vector3 (0, 0, 0);

    //마우스 좌표 확인용
    Ray _ray;
    Plane _groundPlane;

    [Header("총알 궤적 속도")] [SerializeField] float _speed = 200f; // 총알 속도

    [Header("플레이어 최대 체력")] public int _maxHealth = 100;
    [Header("플레이어 현재 체력")] public int _currentHealth;
    [Header("체력 UI")][SerializeField] Slider _healthSlider; 


    public event Action<Vector3> OnShooterPositionChanged; // 적들에게 전달할 옵저버 이벤트
    private Vector3 _lastPosition;   //전에 위치한 위치
    [SerializeField] private float _positionThreshold = 0.5f; // 위치 변화 감지 기준

    [Header("총알 설정")]
    [SerializeField] int _magazineCapacity = 30; // 탄창 크기
    [SerializeField] int _maxReserveAmmo = 999; // 최대 보유 가능한 총알
    [SerializeField] int _startAmmo = 270;
    int _currentAmmoInMagazine; // 현재 탄창 총알
    int _currentReserveAmmo; // 현재 예비 탄약

    [Header("총알 UI")]
    [SerializeField] private TextMeshProUGUI _ammoText; // 탄창 UI

    [Header("재장전 설정")]
    [SerializeField] private float _reloadTime = 2f; // 장전 시간
    public bool _isReloading = false; // 장전 중인지 체크

    SoundManager soundManager;


    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        _shooterAnimationManager = GetComponent<ShooterAnimationManager>();
        _rb = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 카메라 자동 할당
        }

        //커맨드 패턴 생성
        _shooterShootCommand = new ShooterShootCommand(this, _shooterAnimationManager, _muzzleTransform, _muzzleTransform1, _muzzlePoolManager, _speed);
        _shooterReloadCommand = new ShooterReloadCommand(this, _shooterAnimationManager);

        _currentHealth = _maxHealth;
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = _maxHealth; // 최대 체력 설정
            _healthSlider.value = _currentHealth; // 현재 체력 설정
        }

        // 초기 탄약 설정
        _currentAmmoInMagazine = _magazineCapacity; // 탄창 가득 채우기
        _currentReserveAmmo = _startAmmo; // 예비 총알 지정

        UpdateAmmoUI(); // 초기 UI 설정

        soundManager = FindObjectOfType<SoundManager>();
    }

    void OnDrawGizmos()
    {
        // 기즈모 색상 설정 (빨간색)
        Gizmos.color = Color.red;
        // 플레이어의 위치 (시작점)
        Vector3 startPos = _muzzleTransform.transform.position;

        // 플레이어의 전방 방향
        Vector3 endPos = startPos + transform.forward;

        Gizmos.DrawLine(startPos, _targetEemeyPosition);
    }
    void OnEnable()
    {
        // 입력 액션 매핑
        _moveAction = _playerInput.actions["Move"];
        _runAction = _playerInput.actions["Run"];
        _shootAction = _playerInput.actions["Shoot"];
        _reloadAction = _playerInput.actions["Reload"];

        // 입력 이벤트에 함수 등록
        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;

        _runAction.performed += OnRun;
        _runAction.canceled += OnRun;

        _shootAction.performed += OnShoot;

        _reloadAction.performed += OnReload;

        // 입력 활성화
        _moveAction.Enable();
        _runAction.Enable();
        _shootAction.Enable();
        _reloadAction.Enable();
        
    }
    void OnDisable()
    {
        // 입력 이벤트에서 함수 제거
        _moveAction.performed -= OnMove;
        _moveAction.canceled -= OnMove;

        _runAction.performed -= OnRun;
        _runAction.canceled -= OnRun;

        _shootAction.performed -= OnShoot;

        _reloadAction.performed -= OnReload;

        // 입력 비활성화
        _moveAction.Disable();
        _runAction.Disable();
        _shootAction.Disable();
        _reloadAction.Disable();
    }

    void Update()
    {
        // 마우스 방향을 향해 캐릭터 회전
        RotateToMouse();

        // 애니메이션 업데이트
        UpdateAnimation();

        // 플레이어 위치 옵저버 패턴으로
        ShooterTransformObserver();
    }

    void FixedUpdate()
    {
        // 물리 이동 처리
        Move();
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        // 입력값을 읽어서 이동 방향을 설정
        Vector2 input = ctx.ReadValue<Vector2>();
        _moveInput = new Vector3(input.x, 0, input.y);
    }
    void Move()
    {
        if (moveInput.magnitude == 0) return; // 입력값이 없으면 이동하지 않음

        float speed = isRunning ? RunSpeed : WalkSpeed; // 달리기 상태에 따라 이동 속도 결정

        //이동하는 방향
        Vector3 moveDirection = ConvertInputToWorldDirection(_moveInput);
        Vector3 moveVelocity = moveDirection * speed * Time.fixedDeltaTime;

        _rb.MovePosition(_rb.position + moveVelocity); // Rigidbody를 사용하여 이동
    }

    void RotateToMouse()
    {
        // 마우스 위치로부터 레이를 발사하여 캐릭터의 회전 방향을 설정
        _ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        _groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (_groundPlane.Raycast(_ray, out float hitDistance))
        {
            Vector3 mouseWorldPosition = _ray.GetPoint(hitDistance);

            _targetEemeyPosition = mouseWorldPosition + new Vector3(0, 2, 0);

            Vector3 directionToMouse = (mouseWorldPosition - transform.position).normalized;
            directionToMouse.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(directionToMouse, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    Vector3 ConvertInputToWorldDirection(Vector3 inputDirection)
    {
        return new Vector3(inputDirection.x, 0, inputDirection.z).normalized;
    }

    void UpdateAnimation()
    {
        // 이동 속도를 계산 (걷기 또는 달리기 속도 적용)
        float speedFactor = isRunning ? 1.0f : 0.5f; // Shift를 누르면 1.0 (달리기), 걷기는 0.5
        float targetSpeed = Mathf.Clamp(moveInput.magnitude * speedFactor, 0.0f, 1.0f); // 0~1 사이로 제한

        // 이동 방향 벡터를 구함 (월드 좌표 기준)
        Vector3 moveDirection = ConvertInputToWorldDirection(moveInput);
        Vector3 forwardDirection = transform.forward; // 캐릭터가 바라보는 방향

        // 이동 방향과 캐릭터 바라보는 방향 간의 상대 각도를 계산
        float angle = Vector3.SignedAngle(forwardDirection, moveDirection, Vector3.up);

        float directionX = 0;
        float directionY = 0;

        if (moveInput.magnitude > 0) // 이동 중일 때만 방향 계산
        {
            directionX = Mathf.Sin(angle * Mathf.Deg2Rad);
            directionY = Mathf.Cos(angle * Mathf.Deg2Rad);
        }

        // 애니메이션 속도 및 방향 설정
        _shooterAnimationManager.SetMoveSpeed(targetSpeed);
        _shooterAnimationManager.SetDirection(directionX, directionY);
    }

    void OnRun(InputAction.CallbackContext ctx)
    {
        // 달리기 상태를 설정 (버튼을 누르면 true, 떼면 false)
        _isRunning = ctx.performed;
    }

    void OnShoot(InputAction.CallbackContext ctx)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (_currentAmmoInMagazine > 0 && !_isReloading) // 탄창에 총알이 있으면 발사 가능
        {
            soundManager.PlayShootSound();
            _currentAmmoInMagazine--; // 한 발 소비
            _shooterShootCommand.Execute();
            UpdateAmmoUI(); // 총기 발사 후 UI 갱신
        }
        else
        {
            Debug.Log("탄창이 비었습니다! R키를 눌러 장전하세요.");
        }
    }
    public Vector3 GetAimTarget()
    {
        if (_groundPlane.Raycast(_ray, out float hitDistance))
        {
            return _ray.GetPoint(hitDistance);
        }

        return transform.position + transform.forward * 1f; // 기본 사거리 50
    }

    void OnReload(InputAction.CallbackContext ctx)
    {
        //_shooterReloadCommand.Execute();
        soundManager.PlayReloadSound();
        if (_currentReserveAmmo <= 0 || _isReloading) return;
        _isReloading = true;
        _shooterAnimationManager.PlayReload(_isReloading);
        StartCoroutine(IsReloading());
    }
    IEnumerator IsReloading()
    {
        yield return new WaitForSeconds(_reloadTime);

        // 필요한 만큼 탄창 채우기
        int ammoNeeded = _magazineCapacity - _currentAmmoInMagazine; // 탄창이 채워져야 할 개수
        int ammoToLoad = Mathf.Min(ammoNeeded, _currentReserveAmmo); // 예비 총알에서 빼올 수 있는 개수

        _currentAmmoInMagazine += ammoToLoad; // 탄창에 추가
        _currentReserveAmmo -= ammoToLoad; // 예비 탄약에서 차감

        UpdateAmmoUI(); // 장전 후 UI 갱신

        _isReloading = false;
        _shooterAnimationManager.PlayReload(_isReloading);
    }

    public void AddAmmo(int amount) // 총알 획득 시 호출
    {
        _currentReserveAmmo = Mathf.Min(_currentReserveAmmo + amount, _maxReserveAmmo);
        UpdateAmmoUI(); // 총알 획득 후 UI 갱신
        Debug.Log($" 총알 {amount}개 획득! 현재 예비 탄약: {_currentReserveAmmo}");
    }
    private void UpdateAmmoUI()
    {
        if (_ammoText != null)
        {
            _ammoText.text = $"{_currentAmmoInMagazine} / {_currentReserveAmmo}";
        }
    }

    // Shooter의 위치를 옵저버 패턴으로 뿌림
    void ShooterTransformObserver()
    {
        if (Vector3.Distance(transform.position, _lastPosition) > _positionThreshold)
        {
            _lastPosition = transform.position;
            OnShooterPositionChanged?.Invoke(transform.position);
        }
    }

    public void TakeDamage(int damage)
    {
        if(_currentHealth  > 0) _currentHealth -= damage;

        UpdateHealthUI(); // 슬라이더 UI 업데이트

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
    }
    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        UpdateHealthUI(); // 슬라이더 UI 업데이트
    }
    private void UpdateHealthUI()
    {
        if (_healthSlider != null)
        {
            _healthSlider.value = _currentHealth; // 체력 UI 업데이트
        }
        Debug.Log(_currentHealth);
    }

    void Die()
    {
        _shooterAnimationManager.PlayDeathAnimation();
        GameManager._instance.GameOver();
    }
}