using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushInputDefault : RushInput
{

    public string lookAxisX = "Mouse X";
    public string lookAxisY = "Mouse Y";
    [Space]
    public string movementStrafe = "Horizontal";
    public string movementWalk = "Vertical";
    [Space]
    public KeyCode keyCrouch = KeyCode.LeftControl;
    public KeyCode keyJump = KeyCode.Space;
    public KeyCode keySprint = KeyCode.LeftShift;
    [Space]
    public RushMouseSetting zoomButton = RushMouseSetting.rightMouse;

    public override void CameraControls()
    {
        mouseInput = new Vector2(Input.GetAxis(lookAxisX), Input.GetAxis(lookAxisY));

        zoomPress = Input.GetMouseButtonDown((int)zoomButton);
        zoomHold = Input.GetMouseButton((int)zoomButton);
        zoomRelease = Input.GetMouseButtonUp((int)zoomButton);
    }

    public override void MovementControls()
    {
        movementInput = new Vector3(Input.GetAxisRaw(movementStrafe), 0f, Input.GetAxisRaw(movementWalk));

        crouchPress = Input.GetKeyDown(keyCrouch);
        crouchHold = Input.GetKey(keyCrouch);
        crouchRelease = Input.GetKeyUp(keyCrouch);

        jumpPress = Input.GetKeyDown(keyJump);
        jumpHold = Input.GetKey(keyJump);
        jumpRelease = Input.GetKeyUp(keyJump);

        sprintPress = Input.GetKeyDown(keySprint);
        sprintHold = Input.GetKey(keySprint);
        sprintRelease = Input.GetKeyUp(keySprint);
    }


    public enum RushMouseSetting
    { //Enumerator for mouse buttons.

        leftMouse = 0,
        rightMouse = 1,
        middleMouse = 2,
        extraMouse1 = 3,
        extraMouse2 = 4,
        none = 5
    }
}
