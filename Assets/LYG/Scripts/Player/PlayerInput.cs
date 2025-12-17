using Unity.Android.Gradle;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] Vector2 moveVec;
    [SerializeField] Vector2 mouseInputVec;

    public Vector2 MoveVec => moveVec;
    public Vector2 MouseInputVec => mouseInputVec;


    private void Awake()
    {
        player = GetComponent<Player>();
    }

    #region Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        if (context.performed)
        {
            moveVec = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            moveVec = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        player.Jump(context);
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        mouseInputVec = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        player.InitiateAttack(context);
    }

    public void OnSubAttack(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        player.Parrying(context);
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        player.Dodge(context);
    }

    public void OnSwitch(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        player.SwitchWeapon(context);
    }

    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        player.Skill1(context);
    }

    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        player.Skill2(context);
    }

    public void OverFlowSkill(InputAction.CallbackContext context)
    {
        if (player.CrSt is PlayerCrowdControlState)
            return;

        player.OverFlowSkill(context);
    }

    #endregion
}
