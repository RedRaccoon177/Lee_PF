using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    Animator _animator;
    NavMeshAgent _agent;                    // 플레이어 이동을 담당하는 NavMeshAgent 컴포넌트
    Camera _camera;                         // 마우스 클릭한 위치를 가져오기 위한 카메라
    PlayerInput _playerInput;               // 플레이어 입력을 감지하는 PlayerInput 컴포넌트
    AnimationManager _animationManager;     // 애니메이션을 관리하는 AnimationManager
    Rigidbody _rigidbody;      
    Transform _transform;
    public bool _isPlaying = false;

    public LayerMask _targetLayer;
    Vector3 _skillQRange = new Vector3(4f, 2f, 3f); // 박스 크기
    Vector3 _skillQCenter = new Vector3(0f, 1.2f, 2f); // 박스 위치 오프셋
    Quaternion _skillQRotation;

    Ray _mouseTransform;

    Queue<ICommand> _commandHistory = new Queue<ICommand>(); // 실행한 명령을 저장하는 큐 ( REC용 )
    Stack<ICommand> _AvoidRedundantExecution = new Stack<ICommand>();   //중복 실행을 방지하는

    float _originalSpeed;   //원래 스피드
    float _rushSpeed;   //원래 스피드

    /// <summary>
    /// 오브젝트가 생성될 때 실행되는 초기화 함수.
    /// 필요한 컴포넌트들을 가져온다.
    /// </summary>
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();  // NavMeshAgent 가져오기 (이동 처리)
        _camera = Camera.main;                  // 메인 카메라 가져오기 (마우스 클릭 감지용)
        _playerInput = GetComponent<PlayerInput>();  // PlayerInput 가져오기 (입력 처리)
        _animationManager = GetComponent<AnimationManager>();  // AnimationManager 가져오기 (애니메이션 제어)
        _rigidbody = GetComponent<Rigidbody>();
        _transform = GetComponent<Transform>();

        _originalSpeed = 4.0f;
        _rushSpeed = 2;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + _skillQCenter, transform.rotation, _skillQRange);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
    /// <summary>
    /// 게임 오브젝트가 활성화될 때 실행되는 함수.
    /// 입력 시스템에 이벤트를 등록하여 플레이어 입력을 감지한다.
    /// </summary>
    private void OnEnable()
    {
        _playerInput.actions["Move"].performed += OnMove;   // 이동 입력 등록
        
        _playerInput.actions["Stop"].performed += OnStop;   // 실행 취소 입력 등록
        
        _playerInput.actions["Rush"].performed += OnRush;   // 점프 입력 등록
        
        _playerInput.actions["SkillQ"].performed += OnSkillQ; // 스킬 Q 입력 등록
    }

    /// <summary>
    /// 게임 오브젝트가 비활성화될 때 실행되는 함수.
    /// 등록된 입력 이벤트를 제거하여 메모리 누수를 방지한다.
    /// </summary>
    void OnDisable()
    {
        _playerInput.actions["Move"].performed -= OnMove;

        _playerInput.actions["Stop"].performed -= OnStop;
        
        _playerInput.actions["Rush"].performed -= OnRush;
        
        _playerInput.actions["SkillQ"].performed -= OnSkillQ;
    }


    /// <summary>
    /// 마우스 클릭 시 해당 위치로 플레이어를 이동시키는 함수.
    /// </summary>
    /// <param name="ctx">입력 시스템에서 전달하는 CallbackContext (입력 정보 포함)</param>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        // 마우스 클릭한 위치를 가져오기
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        //  TODO: ▼ 만약 바닥이면 이동, 바닥이 아니라면 공격 진행
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // MoveCommand 객체를 생성하여 플레이어 이동 실행
            ICommand moveCommand = new MoveCommand(_animator, _agent, _animationManager, hit.point, this, _originalSpeed);
            moveCommand.Execute(); // 이동 실행
            
            _commandHistory.Enqueue(moveCommand); // 실행한 명령을 스택에 저장 (Undo 기능을 위해)
        }
    }

    /// <summary>
    /// 실행 취소 (Stop) 기능을 수행하는 함수.
    /// </summary>
    /// <param name="ctx">입력 시스템에서 전달하는 CallbackContext (입력 정보 포함)</param>
    public void OnStop(InputAction.CallbackContext ctx)
    {
        if (ctx.performed == true && _commandHistory.Count > 0) // 실행 취소할 명령이 있는지 확인
        {
            ICommand stopCommand = new StopCommand(_agent, _animationManager);
            stopCommand.Execute(); // 해당 명령 실행 취소
        }
    }

    /// <summary>
    /// 가속 애니메이션을 실행하는 함수.
    /// </summary>
    /// <param name="ctx">입력 시스템에서 전달하는 CallbackContext (입력 정보 포함)</param>
    public void OnRush(InputAction.CallbackContext ctx)
    {
        ICommand rushCommand = new RushCommand(_agent, _animationManager, _originalSpeed, _rushSpeed);
        rushCommand.Execute();

        _AvoidRedundantExecution.Push(rushCommand);
    }

    /// <summary>
    /// 스킬 Q를 사용하는 함수.
    /// </summary>
    /// <param name="ctx">입력 시스템에서 전달하는 CallbackContext (입력 정보 포함)</param>
    public void OnSkillQ(InputAction.CallbackContext ctx)
    {
        _mouseTransform = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(_mouseTransform, out hit))
        {
            transform.LookAt(hit.point);
            _skillQRotation = transform.rotation;
            _skillQCenter = transform.position + transform.forward;
        }

        ICommand skillQCommand = new SkillQCommand(_animationManager, _transform, _skillQRange, _skillQCenter, _targetLayer);
        skillQCommand.Execute();
    }


    //TODO: 만약 Stack(_AvoidRedundantExecution)에 중복 되는 것이 존재한다면 그것의 실행을 막자.
    public void ExecuteCommand(ICommand command)
    {
        if (_AvoidRedundantExecution.Contains(command)) return;
    }

}
