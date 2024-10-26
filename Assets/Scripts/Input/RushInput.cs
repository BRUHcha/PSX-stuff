using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JTools;

public abstract class RushInput : MonoBehaviour
{
    //This is an abstract class you can use to implement your own input systems for Rush.
    //It explicitly exists because of how many different input solutions exist for Unity, so I wanted to create a more universal approach for Rush so you can have whatever input manager you need to operate the controller.
    //The Rush Input Modules pack is designed to give you more options, such as using Unity's default input systems or InControl.

    [HideInInspector]
    public RushController owner; //We need this to actually operate back and forth. Rush will automatically take ownership of an input component through its own code.

    [HideInInspector]
    public bool lockMovement; //When this is true, the system will call a different method that corresponds to what inputs need to be set when the movement is locked.
    [HideInInspector]
    public bool lockCamera; //When this is true, the system will call a different method that corresponds to what inputs need to be set when the mouse is locked.

    [HideInInspector]
    public bool jumpPress;
    [HideInInspector]
    public bool jumpHold;
    [HideInInspector]
    public bool jumpRelease;

    [HideInInspector]
    public bool sprintPress;
    [HideInInspector]
    public bool sprintHold;
    [HideInInspector]
    public bool sprintRelease;

    [HideInInspector]
    public bool zoomPress;
    [HideInInspector]
    public bool zoomHold;
    [HideInInspector]
    public bool zoomRelease;

    [HideInInspector]
    public bool crouchPress;
    [HideInInspector]
    public bool crouchHold;
    [HideInInspector]
    public bool crouchRelease;

    [HideInInspector]
    public Vector3 movementInput; //Movement input detected for a given frame.
    [HideInInspector]
    public Vector2 mouseInput; //Mouse input detected for a given frame.

    public virtual void Initialize()
    {
        //This is typically unused, but if you're implementing your own special behavior you might want to override it.
        //Rush calls Initialize when the start callback is called.
    }

    public virtual void ProcessInput(ref RushInputData inputData) //We use a ref both here and in WriteOutputs, that way we only need to pass our inputdata struct to ProcessInput, and everything else is handled there.
    {
        //This virtual should only be used to read inputs. You can then store them into the above variables to process into inputs for the Rush controller.
        //Typically, you shouldn't have to actually edit this virtual. If you need to, then feel free to override it. Normally you're expeced to use the MovementControls and CameraControls virtuals to actually modify the input system.

        if (!lockMovement)
            MovementControls();
        else
            MovementControlsLocked();


        if (!lockCamera)
            CameraControls();
        else
            CameraControlsLocked();


        WriteOutputs(ref inputData);
    }

    //This is used to read the inputs for all controls related to movement.
    public virtual void MovementControls()
    {
        //You can write whatever you need in here, if you're implementing a new input layout.
    }

    //This is commonly used to determine what the locking values should be when the player's movement is locked.
    public virtual void MovementControlsLocked()
    {
        movementInput = Vector3.zero;

        crouchPress = false;
        if (crouchHold)
            crouchRelease = true;
        else
            crouchRelease = false;
        crouchHold = false;

        sprintPress = false;
        if (sprintHold)
            sprintRelease = true;
        else
            sprintRelease = false;
        sprintHold = false;

        jumpPress = false;
        if (jumpHold)
            jumpRelease = true;
        else
            jumpRelease = false;
        jumpHold = false;
    }

    //This is used to read inputs for the player's camera.
    public virtual void CameraControls()
    {
        //You can write whatever you need in here, if you're implementing a new input layout.
    }

    //This is commonly used to determine what the locking values should be when the player's camera is locked.
    public virtual void CameraControlsLocked()
    {
        mouseInput = Vector2.zero;
        
        zoomPress = false;
        if (zoomHold) //For 1 frame we perform a release event, if we were holding before the camera was locked.
            zoomRelease = true;
        else
            zoomRelease = false;
        zoomHold = false;
    }

    public virtual void WriteOutputs(ref RushInputData inputData)
    {
        //This virtual actually applies the inputs to the Rush Controller.
        //You can override this if you need to do anything special when applying these controls to the player controller.
        //You can also override this if you implement your own custom systems to Rush that need their inputs processed here.

        inputData.pressedJump = jumpPress;
        inputData.holdingJump = jumpHold;
        inputData.releasingJump = jumpRelease;

        inputData.pressedSprint = sprintPress;
        inputData.holdingSprint = sprintHold;
        inputData.releasingSprint = sprintRelease;

        inputData.pressedZoom = zoomPress;
        inputData.holdingZoom = zoomHold;
        inputData.releasingZoom = zoomRelease;

        inputData.pressedCrouch = crouchPress;
        inputData.holdingCrouch = crouchHold;
        inputData.releasingCrouch = crouchRelease;
        
        inputData.motionInput = movementInput;
        inputData.mouseInput = mouseInput;

    }
}
