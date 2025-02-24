using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    NavMeshAgent _agent;                    // 플레이어 이동을 담당하는 NavMeshAgent 컴포넌트
    Camera _camera;                         // 마우스 클릭한 위치를 가져오기 위한 카메라
    PlayerInput _playerInput;               // 플레이어 입력을 감지하는 PlayerInput 컴포넌트
    AnimationManager _animationManager;     // 애니메이션을 관리하는 AnimationManager
    Transform _transform;

    public LayerMask _autofindEnemy;
    [Header("스킬에 데미지 받는 레이어")] public LayerMask _skillGetDamageLayer;
    Vector3 _skillQRange = new Vector3(3f, 2f, 3f); // 박스 크기
    Vector3 _skillQCenter = new Vector3(0f, 1.5f, 2f); // 박스 위치 오프셋
    Vector3 _playerSkillQPosition;

    Ray _mouseTransform;

    Stack<ICommand> _AvoidRedundantExecution = new Stack<ICommand>();   //중복 실행을 방지하는
    
    //스피드 관련 필드
    float _originalSpeed;   //원래 스피드
    float _rushSpeed;   //원래 스피드

    bool _isSkillQ = false;

    //커서 이미지
    public Texture2D _attackCursor;
    public Vector2 hotSpot = Vector2.zero; // 기준점

    //A를 누르고 좌클릭 참/거짓 값
    bool _isATrue = false;

    public GameObject[] _iconArrows;

    public bool _IsPlayerContactMoveIcon = false;
    public bool _IsPlayerContactAttackIcon = false;


    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();  // NavMeshAgent 가져오기 (이동 처리)
        _camera = Camera.main;                  // 메인 카메라 가져오기 (마우스 클릭 감지용)
        _playerInput = GetComponent<PlayerInput>();  // PlayerInput 가져오기 (입력 처리)
        _animationManager = GetComponent<AnimationManager>();  // AnimationManager 가져오기 (애니메이션 제어)
        _transform = GetComponent<Transform>();

        _originalSpeed = 4.0f;
        _rushSpeed = 2;
    }

    void OnDrawGizmos()
    {
        //스킬 Q 범위
        Gizmos.color = Color.red;
        // 기즈모가 그려질 위치 (플레이어 기준 항상 앞으로!)
        Vector3 _playerSkillQPositionGizmo = transform.position
                                + transform.right * _skillQCenter.x  // 좌우 이동
                                + transform.up * _skillQCenter.y     // 높이 이동
                                + transform.forward * _skillQCenter.z; // 항상 플레이어 앞을 기준

        // 회전 적용 (항상 플레이어 회전 유지)
        Gizmos.matrix = Matrix4x4.TRS(_playerSkillQPositionGizmo, transform.rotation, _skillQRange);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);


        //Auto공격 탐지 범위
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5);

        //Auto공격 실제 범위
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
    }
    
    void OnEnable()
    {
        _playerInput.actions["Move"].performed += OnMove;   // 이동 입력 등록
        
        _playerInput.actions["Stop"].performed += OnStop;   // 실행 취소 입력 등록
        
        _playerInput.actions["Rush"].performed += OnRush;   // 점프 입력 등록

        _playerInput.actions["AttackMove"].performed += OnAtttackMove;
        
        _playerInput.actions["SkillQ"].performed += OnSkillQ; // 스킬 Q 입력 등록
    }

    void OnDisable()
    {
        _playerInput.actions["Move"].performed -= OnMove;

        _playerInput.actions["Stop"].performed -= OnStop;
        
        _playerInput.actions["Rush"].performed -= OnRush;

        _playerInput.actions["AttackMove"].performed += OnAtttackMove;

        _playerInput.actions["SkillQ"].performed -= OnSkillQ;
    }
    public void OnMove(InputAction.CallbackContext ctx)
    {
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        int _layerMasks = LayerMask.GetMask("Ground");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMasks))
        {
            ICommand moveCommand = new MoveCommand(_agent, _animationManager, hit.point, this, _originalSpeed, _isATrue, _transform, _autofindEnemy);

            if (ctx.control.path == "/Mouse/rightButton")
            {
                _IsPlayerContactMoveIcon = false;
                _IsPlayerContactAttackIcon = true;
                _iconArrows[0].transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);

                moveCommand.Execute();
            }
            else if (ctx.control.path == "/Mouse/leftButton" && _isATrue == true)
            {
                _IsPlayerContactMoveIcon = true;
                _IsPlayerContactAttackIcon = false;
                _iconArrows[1].transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);

                moveCommand.Execute();
            }
            _isATrue = false;
        }
    }

    public void OnStop(InputAction.CallbackContext ctx)
    {
        _IsPlayerContactMoveIcon = true;
        _IsPlayerContactAttackIcon = true;

        if (ctx.performed == true)
        {
            ICommand stopCommand = new StopCommand(_agent, _animationManager);
            stopCommand.Execute(); // 해당 명령 실행 취소
        }
    }

    public void OnRush(InputAction.CallbackContext ctx)
    {
        ICommand rushCommand = new RushCommand(_agent, _animationManager, _originalSpeed, _rushSpeed);
        rushCommand.Execute();

        _AvoidRedundantExecution.Push(rushCommand);
    }

    public void OnAtttackMove(InputAction.CallbackContext ctx)
    {
        UnityEngine.Cursor.SetCursor(_attackCursor, hotSpot, CursorMode.Auto);
        _isATrue = true;
    }

    public void OnSkillQ(InputAction.CallbackContext ctx)
    {
        if (_isSkillQ) return;
        StartCoroutine(SkillQCoolTime());

        _IsPlayerContactMoveIcon = true;
        _IsPlayerContactAttackIcon = true;

        _mouseTransform = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(_mouseTransform, out hit))
        {
            transform.LookAt(hit.point);
        }
        _playerSkillQPosition = transform.position + transform.right * _skillQCenter.x + transform.up * _skillQCenter.y + transform.forward * _skillQCenter.z;

        ICommand skillQCommand = new SkillQCommand(_animationManager, _transform, _skillQRange, _playerSkillQPosition, _skillGetDamageLayer, _agent);
        skillQCommand.Execute();
    }
    IEnumerator SkillQCoolTime()
    {
        _isSkillQ = true;
        yield return new WaitForSeconds(1);
        _isSkillQ = false;
    }

}


