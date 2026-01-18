using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("General")]
    public PlayerInput _playerInput;

    [Header("Cursor")]
    public Vector2 CursorInput;


    [Header("Current Device Settings")]
    public InputDevice CurrentDevice;
    public enum InputDevice { K_M, GAMEPAD };



    private void Start()
    {
        CursorInput = new Vector2(0, 0);
        _playerInput = GetComponent<PlayerInput>();
    }



    void Update()
    {
        GetCurrentDevice();
    }



    public void Cursor(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CursorInput = context.ReadValue<Vector2>();
        }
    }



    public void GetCurrentDevice()
    {
        if (_playerInput.currentControlScheme == "M&K")
        {
            CurrentDevice = InputDevice.K_M;
        }
        else if (_playerInput.currentControlScheme == "Gamepad")
        {
            CurrentDevice = InputDevice.GAMEPAD;
        }
    }


    public void Confirm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (GameManager.Instance.GamePaused) return;
            // Triggered in SkillCaster
            // Debug.Log("Confirm Pressed");
            GlobalDataStore.Instance.SkillCaster.UseActiveManualSkill();
        }
    }



    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (GameManager.Instance.GamePaused) return;
            // Triggered in SkillCaster
            // Debug.Log("Cancel Pressed");
            GlobalDataStore.Instance.SkillCaster.ClearActiveManualSkill();
        }
    }
}
