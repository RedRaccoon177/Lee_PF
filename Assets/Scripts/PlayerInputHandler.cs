using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        _playerInput.actions["Move"].performed += _playerController.OnMove;
        _playerInput.actions["Stop"].performed += _playerController.OnStop;
        _playerInput.actions["Rush"].performed += _playerController.OnRush;
    }

    private void OnDisable()
    {
        _playerInput.actions["Move"].performed -= _playerController.OnMove;
        _playerInput.actions["Stop"].performed -= _playerController.OnStop;
        _playerInput.actions["Rush"].performed -= _playerController.OnRush;
    }
}
