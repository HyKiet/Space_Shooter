using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceGameInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction shootAction;

    public Vector2 Move => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

    public event Action ShootPressed;

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("SpaceGameInput: Missing InputActionAsset reference.", this);
            enabled = false;
            return;
        }

        var playerMap = inputActions.FindActionMap("Player", true);
        moveAction = playerMap.FindAction("Move", true);
        shootAction = playerMap.FindAction("Attack", true);
    }

    private void OnEnable()
    {
        inputActions?.Enable();
        if (shootAction != null)
        {
            shootAction.performed += OnShootPerformed;
        }
    }

    private void OnDisable()
    {
        if (shootAction != null)
        {
            shootAction.performed -= OnShootPerformed;
        }

        inputActions?.Disable();
    }

    private void OnShootPerformed(InputAction.CallbackContext _)
    {
        ShootPressed?.Invoke();
    }
}
