using UnityEngine;
using UnityEngine.InputSystem; // 인풋 시스템 사용

public class PlayerBaseMove : MonoBehaviour
{
    Transform tr;
    Vector3 moveDir;

    PlayerInput playerInput;
    InputActionMap mainActionMap;
    InputAction moveAction;
    InputAction attackAction;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        mainActionMap = playerInput.actions.FindActionMap("PlayerActions");
        moveAction = mainActionMap.FindAction("Move");
        attackAction = mainActionMap.FindAction("Attack");

        moveAction.performed += ctx =>
        {
            Vector2 dir = ctx.ReadValue<Vector2>();
            moveDir = new Vector3(dir.x, 0, dir.y);
        };

        moveAction.canceled += ctx =>
        {
            moveDir = Vector3.zero;
        };

        attackAction.performed += ctx =>
        {
            //공격메서드
        };

    }

    void Update()
    {
        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
            transform.Translate(Vector3.forward * Time.deltaTime * 4.0f);
        }
    }


    #region Unity Event 버전
    public void OnMove(InputAction.CallbackContext ctx) //오버로드 사용
    {
        Vector2 dir = ctx.ReadValue<Vector2>();
        moveDir = new Vector3(dir.x, 0, dir.y);

    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        Debug.Log("공격 성공");
        //공격메서드
    }
    #endregion
}

