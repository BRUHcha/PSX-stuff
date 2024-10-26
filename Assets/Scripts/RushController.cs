using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace JTools
{
    public class RushController : NetworkBehaviour
    {

        public static RushController current; //Just call RushController.current to access the current player's variables from any other script!

        /*
            NOTICE:
            - Player rotation is divided between two separate parts! The camera deals with pitch, and the body deals with yaw. Remember this!
            - Tooltips are there for a reason! Mouse over a variable in the inspector for more information on how to use it!
            - Bugs suck! Report anything you run into and I will get onto it as soon as possible!
        */

        [Header("Rush - General")]
        [Tooltip("Switch between first and third person! Made this an enum in case other types of 3D player are implemented into the tool...")] public RushMode playerMode = RushMode.firstPerson;
        [Min(0f)] public float playerRadius = 0.3f;
        [Min(0f)] public float playerHeight = 1.64f;

        [Header("Rush - Controls")]
        [Tooltip("Rush uses a different component to manage input, that way you can slot in whatever modules you need as you please.")] public RushInput inputModule;
        public RushInputData inputData = new RushInputData(Vector3.zero);
        [Tooltip("Changes how jumping will behave.\n\nNone: Disables jumping entirely.\n\nNormal: Jumping is unaffected by anything else. Just pure jump power, baby.\n\nEnhanced: If you're sprinting, your jump power is scaled to be 115% of the original. If you aren't sprinting, it's just normal.\n\nLeaping: Suggested by Aaron. Leaping will make it so that if you're sprinting, you'll leap in the direction you're running, scaled by 2x the current sprint speed. Good for games focused on mobility. If you aren't sprinting, things are normal.")] public RushJump jumpMode = RushJump.enhanced;
        [Tooltip("Changes how sprinting will behave.\n\nNone: Disables sprinting entirely.\n\nNormal: Sprinting engages when the sprint key is held down, and stops when you release.\n\nClassic: If you're walking, pressing the sprint key engages sprintng. Releasing it won't stop sprinting, it's only when you stop walking that sprinting will stop.")] public RushSprintSetting sprintMode = RushSprintSetting.normal;
        [Tooltip("Changes how crouching will behave.\n\nNone: Disables crouching entirely.\n\nNormal: Crouching won't affect how sprinting behaves.\n\nNo Sprint: Sprinting cannot be enabled while you're crouching.")] public RushCrouchSetting crouchMode = RushCrouchSetting.normal;
        [Tooltip("How sensitive the player's mouse is.")] public float mouseSensitivity = 1.5f;


        [Header("Rush - Movement")]
        [Tooltip("This defines the tallest step a player can climb when dealing with stair-related stuff.")] public float stepHeight = 0.5f;
        [Tooltip("Enables smooth stepping, allowing the player to climb stairs with very little visual jittering.")] public bool smoothStepping = true;
        [Tooltip("Sometimes you don't want to be able to walk on anything! You can use this to disable grounding on different layers if you need to.")] public LayerMask groundingLayers = ~0;
        [Tooltip("Whether or not the player can walk around, sprint or jump.")] public bool lockMovement = false;
        [Space]
        [Tooltip("The amount of gravity the player experiences per frame.")] public float gravity = 30f;
        [Tooltip("The maximum fall speed possible.")] public float gravityCap = -100f;
        [Tooltip("0.01 means that the player will control like they're on ice, and 1 means the player moves almost instantaneously in the direction you choose. Your input settings account for friction as well, so check Edit>Project Settings>Input for more information.")] [Range(0.01f, 1f)] public float movementShiftRate = 0.2f;
        [Tooltip("The amount of control a player gets in the air. 0 means absolutely none, 1 means same amount as on the ground.")] [Range(0f, 1f)] public float airControl = 1f;
        [Space]
        [Tooltip("How fast the player is when walking normally.")] public float moveSpeed = 5f;
        [Tooltip("How fast the player is when crouch-walking like a creepy little crab.")] public float crouchSpeed = 3f;
        [Tooltip("How fast the player moves when sprinting.")] public float sprintSpeed = 8f;
        [Space]
        [Tooltip("The amount of force the player jumps with.")] public float jumpPower = 4f;
        [Space]
        [Tooltip("Whether or not the player is affected by slopes.")] public bool slidingOnSlopes = true;
        [Tooltip("Determines at what normal value the player will slide down a sloped surface (assuming slidingOnSlopes is enabled). The higher this is, the steeper a surface a player can walk up.")] [Range(0f, 1f)] public float slopeBias = 0.85f;
        [Tooltip("A precentage of the player's current height that represents how low they can crouch. This specfically affects the player's collider height.")] [Range(0.4f, 1f)] public float crouchPercent = 0.4f;
        [Range(0.1f, 1f)]
        [Tooltip("The lower this value is, the further down towards the ground the camera will drop when crouching. This specifically affects the player's camera height, and isn't related to collider height.")] public float cameraCrouchDrop = 1f;

        [HideInInspector]
        public Vector3 movement; //The vector3 representing player motion.

        [Header("Rush - Camera")]
        [Tooltip("Whether or not the player can look around or zoom in.")] public bool lockCamera = false;
        [Space]
        [Tooltip("A direct reference to the player's camera.")] public Camera playerCamera;
        [Tooltip("If this is enabled: Left-Clicking on the screen will lock your cursor in. Pressing Escape unlocks the cursor.")] public bool cursorManagement = true;
        [Tooltip("The angles at which the player's camera can look up and down. For technical reasons, third person mode completely ignores this and uses its own restraints.")] public Vector2 verticalRestraint = new Vector2(-90f, 90f);
        [Space]
        [Tooltip("Whether or not the camera bobs at all.")] public bool enableViewbob;
        [Tooltip("The rate at which the player's camera bobs.")] public float viewBobRate = 1f;
        [Tooltip("The intensity at which the player's camera bobs.")] public float viewBobPower = 1f;
        [Space]
        [Tooltip("Whether or not the camera should respond to landing.")] public bool landingEffects;
        [Space]
        [Tooltip("Whether or not the player camera should tilt when you move. Used commonly in fancy-shmancy FPS games. Camera tilting won't take effect in third person mode for obvious reasons.")] public bool cameraTilting = false;
        [Tooltip("How much tilt we should apply when the player is moving. This variable affects how much the camera rolls.")]public float cameraTiltRollPower = 10f;
        [Tooltip("How much tilt we should apply when the player is moving. This variable affects how much the camera pitches. Commonly disabled because most camera tilting is based on strafe movement.")] public float cameraTiltPitchPower = 10f;
        [Space]
        [Tooltip("If this is enabled, then mouse movement will be accounted for when tilting the player's camera.")] public bool cameraMouseTilting = false;
        [Tooltip("How much tilt we should apply when the player is turning around. This variable affects how much the camera rolls.")] public float cameraMouseTiltRollPower = 10f;
        [Space]
        [Tooltip("How quickly the camera lerps to different tilting angles. 0.01 means it's incredibly slow, while 1 makes it instant.")][Range(0.01f, 1f)] public float cameraTiltSpeed = 0.1f;
        

        Vector2 m_cameraTilt;

        [Header("Rush - Third Person Camera")]
        [Tooltip("If true, the system will always bring the camera close to the player if something is blocking it.\n\nIf false, the system won't bring the camera in if a non-kinematic rigidbody is between the player and the camera.")] public bool rigidbodyOcclusion = true;
        [Tooltip("How far the third person camera should be from the player while in third person mode.")] public float thirdPersonOrbitDistance;
   
        [Header("Rush - Zoom")]
        [Tooltip("How much the player's FOV changes while sprinting.")] public float sprintIntensity = 15f;
        [Tooltip("This value represents how many degrees the zoom changes. The higher this value is, the further in the camera will zoom.")] public float zoomIntensity = 30f;

        [Header("Rush - Sounds")]
        [Tooltip("Whether or not sounds will play from the player.")] public bool enableSounds = true;
        [Space]
        [Tooltip("How loud the player's sounds are.")] [Range(0f, 1f)] public float soundVolume = 1f;
        [Tooltip("How long the player will wait before allowing the landing sound to play again. Check m_landingTimer in the code for more details.")] public float landingSoundTimer = 1f;
        [Tooltip("The sound that plays whenever the player walks. The rate this plays at is scaled based on speed.")] public AudioClip[] walkSounds;
        [Tooltip("The sound that plays whenever the player jumps.")] public AudioClip jumpingSound;
        [Tooltip("The sound that plays whenever the player lands on the ground.")] public AudioClip landingSound;

        [Header("Rush - Animation")]
        [Tooltip("^ Leave this unassigned to ignore animations. ^\n\nExplanation of used animator parameters below:")] public Animator playerAnimator; //This is important, please read the tooltip for a comprehensive list of the animator parameters used.
        [SerializeField] private GameObject playerModel;
        [Space]
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nWalking: Boolean, whether or not the player is moving.")] public string walkingParameter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nSprinting: Boolean, whether or not sprinting is active.")] public string sprintingParameter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nCrouching: Boolean, whether or not the player is crouching.")] public string crouchingParameter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nGrounded: Boolean, whether or not the player is on the ground.")] public string groundedParamter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nRelativeSpeed: Float, a percentage comparing the player's current speed to the normal walk speed. Used either for smooth animation blending or directly adjusting the speed of a walking animation.")] public string relativeSpeedParameter = string.Empty;
        [Space]
        [Tooltip("^ Leave this empty to ignore weight.^\nQuick explanation: This uses a layer name instead of a parameter. Please be careful!\nCrouching Percent: Float, scales from 0 to 1, representing how crouched the player is.")] public string crouchingWeight = string.Empty;
        [Space]
        [Tooltip("In circumstances where you would rather have your animator manage step sounds via animation events, you can mark this true.")] public bool overrideFootsteps = false;

        [Header("Rush - Misc.")]
        [Tooltip("Whether or not the game should have its framerate locked.")] public bool lockFramerate = true;
        [Tooltip("The framerate the game will be locked at whenever a player is spawned in. Requires lockFramerate to be active first.")] [Range(1, 60)] public int frameRate = 60;

        [HideInInspector]
        public bool isGrounded = false; //Internal boolean to tell whether or not the player is on the ground. Depends on several conditions seen in the code below.
        [HideInInspector]
        public bool isCrouching = false; //Internal boolean, name is self explanatory.
        [HideInInspector]
        public bool isSprinting; //Whether or not the player is sprinting. 

        /// Private variables.
        private float FOV = 60f; //The default FOV value the camera will be set to. 
        private bool m_stepped = false; //Used per-frame to play walking sounds. Prevents rapid-fire sound being played.
        private bool m_canJump; //Decided based on the angle of the ground below the player.
        private float m_topSpeed; //The maximum amount of speed the player may move at any time. Doesn't include vertical speed.
        private float m_lastGrav; //The gravity as of last frame. Used for a handful of calculations.

        private float m_zoomAdditive; //Added onto the current FOV value to decide how far the camera is zoomed in/out.
        private float m_zoomGoal; //The total amount of zoom m_zoomAdditive is trying to reach.
        private Quaternion m_thirdPersonRotation; //A new interpretation on the original system.
        private Quaternion m_thirdPersonMoveRotation; //This has to be separate as the controller needs to move on one axis.
        private Vector3 m_thirdPersonCamOutput; //The direction the camera faces in.
        private Vector3 m_thirdPersonMotion;
        private Vector3 m_camAngles; //The true angles of the camera. It has to be represented in eulerangles because otherwise clamping is challening.
        private Vector3 m_camOrigin; //The original local position of the player's camera.
        private Vector3 m_camPosTracer; //Internal vector3 to lerp the camera's position with. Designed to smooth landing effects.
        private float m_landingTimer; //Internal timer to prevent the landing sound from playing 1000 times every time a series of landing events are registered. Comparable to a debouncing script.
        private float m_camOriginBaseHeight; //Essentially, the best method of keeping the camera from messing up while crouching is to modify the camOrigin's y value if we're crouched or not. This is the base.

        private float m_jumpTimer; //Prevents key bouncing issues;

        private bool m_sliding; //Used when blocking player motion up a slope.
        private Vector3 m_slidingNormal; //The normal of the slope the player is touching.
        private float m_slideHolder; //Prevents infinite sliding when falling off slopes into air, just ignore it.

        private float m_crouchTime; //The transition for crouching, used for calculating curves.
        private float crouchRate = 0.2f; //The speed the crouch transtition occurs at. Measured in seconds, so the smaller this is the faster the transition.
        private float m_walkTime = 0f; //The value used for managing view bobbing.

        RaycastHit m_hit; //A cached raycast for performance. The script recycles this often to avoid allocation issues.

        private Rigidbody m_rig; //Reference to the rigidbody component attached to the player object;
        private CapsuleCollider m_capsule; //Reference to the capsule collider component, cached for performance reasons.
        private AudioSource m_sound; //Reference to the player's audiosource component.

        Vector3 m_lastPosition;

        float m_slideTimer = 0f; //Used to analyze how long a player has been sliding, if it's too short the system won't attempt to run landing effects, that way you don't get effect spam when sliding down a wall.

        ///

        public override void OnNetworkSpawn()
        {
            if(IsOwner)
            {
                playerModel.gameObject.SetActive(false);
            }
            if(!IsOwner)
            {
                playerCamera.enabled = false;
                enabled = false;
                return;
            }
        }
        void Start()
        {
            ///Singleton Access
            current = this; //The current player controller is assigned so you can access it whenever you need to.


            ///Input Module initialization. Make sure an input module is attached to your controller before you go about using it!
            if (inputModule == null)
                inputModule = GetComponent<RushInput>();

            if (inputModule != null)
            {
                inputModule.Initialize();
                inputModule.owner = this;
            }
            else
                Debug.LogWarning("RUSH - Looks like you forgot to assign an input module to your controller! This is really important, so make sure to fix this!");


            /// Initialization details.
            FOV = playerCamera.fieldOfView;

            m_topSpeed = moveSpeed; //The top speed is set to the default moveSpeed.

            m_camAngles = new Vector3(playerCamera.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,0f); //Goal angles are based on the player camera's rotation.
            m_camOrigin = playerCamera.transform.localPosition; //The camera's origin is cached so bobbing and landing effects can be applied without the camera losing its default position.
            m_camOriginBaseHeight = m_camOrigin.y;
            m_camPosTracer = m_camOrigin; //This is where the camera truly lies in a given frame.
            m_thirdPersonRotation = Quaternion.Euler(m_camAngles);

            //The controller's sound component is setup here. This allows it to actually make nosies when moving around. You can also just add an audiosource yourself if you need to configure the settings in any special way!
            m_sound = (GetComponent<AudioSource>() != null) ? GetComponent<AudioSource>() : gameObject.AddComponent<AudioSource>();
            m_sound.spatialBlend = 0f;

            if (cursorManagement)
            {
                Cursor.lockState = CursorLockMode.Locked; //Cursor is locked.
                Cursor.visible = false; //Cursor is hidden.
            }

            switch (playerMode) //This is where we set the camera up for third person, popping it off of the player and getting the position set correctly before the game starts.
            {
                case (RushMode.thirdPerson):
                    playerCamera.transform.SetParent(null);

                    playerCamera.transform.position = transform.position + ((m_thirdPersonRotation * -Vector3.forward) * thirdPersonOrbitDistance) + movement * Time.deltaTime;
                    break;
            }

            //Capsule is added manually, this way the settings can be perfect every time without relying on the user.
            m_capsule = gameObject.AddComponent<CapsuleCollider>();
            m_capsule.radius = playerRadius;
            m_capsule.height = playerHeight;
            m_capsule.center = Vector3.up * playerHeight * 0.5f;

            PhysicMaterial m_phy = new PhysicMaterial
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum

            };

            m_capsule.material = m_phy;

            //Same for the rigidbody, manually set here.
            m_rig = gameObject.AddComponent<Rigidbody>();
            m_rig.freezeRotation = true;
            m_rig.useGravity = false;
            m_rig.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            ///Framerate locking, if you so please. Vsync must be disabled for this feature to work.
            if (lockFramerate)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = frameRate;
            }
            ///

        }

        void Update()
        {

            /// INPUT PROCESSING
            if(inputModule != null)
                inputModule.ProcessInput(ref inputData);


            /// THIRD PERSON CAMERA CONFIGURATION
            if (playerMode == RushMode.thirdPerson && !lockCamera && !lockMovement)
            {
                m_thirdPersonRotation = Quaternion.Euler(m_camAngles.x, -m_camAngles.y, 0f);
                m_thirdPersonMoveRotation = Quaternion.Euler(0f, -m_camAngles.y, 0f);

                m_thirdPersonCamOutput = m_thirdPersonRotation * new Vector3(0f, 0f, -thirdPersonOrbitDistance);
                m_thirdPersonMotion = m_thirdPersonMoveRotation * new Vector3(inputData.motionInput.x * m_topSpeed, 0f, inputData.motionInput.z * m_topSpeed);
            }


            /// MOVEMENT MANAGEMENT
            //Crouching control/logic (check just a bit further down for the part where the height is actually affected!)
            if (crouchMode != RushCrouchSetting.none) //Assuming we're actually letting the player crouch
            {
                if (!Physics.SphereCast(new Ray(transform.position + (Vector3.up * m_capsule.height * 0.5f), Vector3.up), playerRadius * 0.9f, playerHeight * 0.52f, groundingLayers, QueryTriggerInteraction.Ignore)) //We fire a check above the player to make sure they're not trying to stand up while a ceiling is present.
                    isCrouching = ((!lockMovement) ? inputData.holdingCrouch : false);
                else
                    isCrouching = ((inputData.pressedCrouch) || (m_crouchTime >= 0.1f));
            }
            else
                isCrouching = false;


            //Slide Timer
            if (m_sliding)
                m_slideTimer = 1f;
            else
                m_slideTimer = Mathf.Max(m_slideTimer - Time.deltaTime, 0f); //If you're not sliding, a small debouncing value is used to prevent issues with sliding/unsliding over and over, which can cause the landing effects to fire off constantly.

            //The actual physical crouching.
            m_crouchTime = Mathf.Clamp01(m_crouchTime + ((isCrouching) ? 1f : -1f) * (Time.deltaTime * ((crouchRate > 0f) ? (1f / crouchRate) : 1f / 0.01f)));

            //Collider adjustments based on the crouching.
            m_capsule.height = playerHeight + ((playerHeight * crouchPercent) - playerHeight) * SmoothUp(m_crouchTime);
            m_capsule.center = Vector3.up * (m_capsule.height * 0.5f);

            /// Sprinting. Code for managing the camera's FOV, the ways sprinting can be enabled, and the player's top speed.
            switch (sprintMode)
            {
                case (RushSprintSetting.normal):
                    isSprinting = inputData.holdingSprint;
                    break;

                case (RushSprintSetting.classic):
                    //Player's intended movement is averaged on intensity and analyzed. If it falls below a threshold, sprinting turns off.
                    isSprinting = ((Mathf.Abs(inputData.motionInput.x) + Mathf.Abs(inputData.motionInput.z)) * 0.5f < 0.5f) ? false : inputData.holdingSprint;
                    break;
            }

            if (crouchMode == RushCrouchSetting.noSprint && isCrouching)
                isSprinting = false;

            playerCamera.fieldOfView = RLerp(playerCamera.fieldOfView, (FOV + m_zoomAdditive), 0.2f);

            if (!isCrouching)
                m_topSpeed = (isSprinting) ? sprintSpeed : moveSpeed;
            else
                m_topSpeed = (isSprinting) ? (crouchSpeed * (sprintSpeed / moveSpeed)) : crouchSpeed;
            ///

            /// General Motion.
            if (!lockMovement) //If the movement isn't locked, manage player controls to figure out where they want to go.
            {
                if (!isGrounded)
                {
                    if (m_slideHolder > 0f)
                        m_slideHolder -= Time.deltaTime;
                    else
                        m_sliding = false;
                }

                if (m_sliding)
                    m_canJump = false;

                switch (playerMode)
                {
                    case (RushMode.firstPerson):
                        movement = RLerp(movement, transform.TransformDirection( Vector3.ClampMagnitude(new Vector3(inputData.motionInput.x * m_topSpeed, 0, inputData.motionInput.z * m_topSpeed), m_topSpeed) + new Vector3 (0, movement.y, 0)), movementShiftRate * ((!isGrounded) ? airControl : 1f));
                        break;

                    case (RushMode.thirdPerson):
                        movement = RLerp(movement, new Vector3(m_thirdPersonMotion.x, movement.y, m_thirdPersonMotion.z), movementShiftRate * ((!isGrounded) ? airControl : 1f));
                        break;
                }

            }
            else
                movement = RLerp(movement, Vector3.zero, movementShiftRate); //Assuming movement's locked, the player is recursively slowed down to zero.

            ///



            bool m_gqueue = false; //A queued boolean so the grounding code can run first, but the snapping code is guaranteed a chance to check things out.

            //Checking for ground.
            if (Physics.SphereCast(transform.position + Vector3.up * m_capsule.height * 0.5f, playerRadius * 0.99f, Vector3.down, out m_hit, (m_capsule.height * 0.51f) - (playerRadius * 0.8f), groundingLayers, QueryTriggerInteraction.Ignore))
            {
                if (slidingOnSlopes)
                {
                    if (m_hit.normal.y > slopeBias)
                        m_gqueue = true;
                    else
                        isGrounded = false;
                }
                else
                {
                    m_gqueue = true;
                }

                Vector3 normal = m_hit.normal;

                if (Vector3.Dot(normal, Vector3.up) > 0.1f) //We only need to make the following decisions if the object we're colliding with is beneath us.
                {
                    if (slidingOnSlopes)
                    {
                        if (movement.y < 0f)
                        {
                            if (Vector3.Dot(normal, Vector3.up) < slopeBias) //If sliding on slopes is enabled, we check to see if the current surface is too steep. If so, the player can no longer jump, and they are shunted down the slope. If the slope isn't too steep, the player is then allowed to jump.
                            {
                                m_sliding = true;
                                m_slideHolder = 0.1f;
                                m_slidingNormal = normal;
                            }
                            else
                            {
                                m_sliding = false;
                                m_slidingNormal = Vector3.zero;
                                m_canJump = true;
                            }
                        }
                    }
                    else
                        m_canJump = true;
                }
            }
            else
                isGrounded = false;

            /// This snaps the player down to a surface if the conditions are just right. Needed on slopes to prevent the player from sliding off of a surface and floating down to the ground instead of, you know, walking down the slope like a normal human.
            float snapDistance = (playerHeight * 0.5f) + Mathf.Clamp(m_rig.velocity.y, -1f, 0f); //How far down the player will search for snappable terrain.
            if (!isGrounded && movement.y < 0f) //If we're midair and we're also falling down.
            {
                if (Physics.Raycast(transform.position + Vector3.up * m_capsule.height * 0.5f, Vector3.down, out m_hit, snapDistance, groundingLayers, QueryTriggerInteraction.Ignore)) //A raycast is fired below the player.
                {
                    if (slidingOnSlopes)
                    {
                        if (m_hit.normal.y > slopeBias)
                        {
                            if ((1f - m_hit.normal.y) < slopeBias) //If the surface is flat enough, 
                                transform.position = m_hit.point; //The player is shifted down to the surface for landing.

                            isGrounded = true;
                        }
                    }
                    else
                    {
                        isGrounded = true;
                    }
                }
            }

            if (m_gqueue)
                isGrounded = true;

            /// STAIR STEPPING

            if (!Physics.Raycast(transform.position + stepHeight * Vector3.up, movement.normalized, playerRadius * 1.2f, groundingLayers))
            {
                Vector3 mx = transform.position + ((movement * Time.deltaTime).normalized * playerRadius * 1.2f) + stepHeight * Vector3.up;
                Debug.DrawLine(mx, mx + Vector3.down * stepHeight, Color.blue);
                if (Physics.Raycast(transform.position + ((movement * Time.deltaTime).normalized * playerRadius * 1.2f) + stepHeight * Vector3.up, Vector3.down, out RaycastHit m_step, stepHeight, groundingLayers))
                {
                    if (Vector3.Dot(m_step.normal, Vector3.up) > 0.98f && m_step.point.y > transform.position.y)
                    {
                        Vector3 cameraPos = playerCamera.transform.position;
                        Debug.DrawLine(m_step.point, m_step.point + Vector3.down * 0.4f, Color.green);
                        transform.position = new Vector3(transform.position.x, m_step.point.y, transform.position.z);

                        if (smoothStepping)
                        {
                            m_camPosTracer += (cameraPos - playerCamera.transform.position);
                            playerCamera.transform.localPosition += (cameraPos - playerCamera.transform.position);
                        }
                    }
                }
            }

            Debug.DrawLine(transform.position + stepHeight * Vector3.up, (transform.position + stepHeight * Vector3.up) + movement.normalized * playerRadius * 1.2f, Color.red);


            ///


            if (m_sliding)
                isGrounded = false;


            /// Ceiling bumping. Prevents that obnoxious "sticky" feel you get whenever you jump into something above you.

            if (Physics.Raycast(transform.position + Vector3.up * m_capsule.height * 0.5f, Vector3.up, out m_hit, m_capsule.height * 0.55f, groundingLayers, QueryTriggerInteraction.Ignore))
            {
                if (m_hit.collider.GetComponent<Rigidbody>() != null)
                {
                    if (m_hit.collider.GetComponent<Rigidbody>().isKinematic)
                        if (movement.y > 0f) movement.y = 0f;
                }
                else
                    if (movement.y > 0f) movement.y = 0f;
            }

            ///


            /*
             * Gravity is affected here. We clamp it so if there's a bug, the gravity won't grow so great that players just fly through geometry.
             * Think of it like terminal velocity, if you drop an object it doesn't just keep accelerating until it punches a hole through the Earth, right?
            */

            movement.y = Mathf.Clamp(movement.y - gravity * Time.deltaTime, gravityCap, Mathf.Infinity);

            /*
             * We use this in case jump controls get rapid inputs, whether this is a hardware fault or a deliberate action.
             * If this weren't here, it'd be possible to chain enough jumps in the frames before the player is no longer considered "grounded" to fly into the air.
            */
            if (m_jumpTimer > 0f)
                m_jumpTimer -= Time.deltaTime;

            /// Jumping management.
            if (!isGrounded)
            {
                if (!lockMovement)
                {
                    if (movement.y > 0f && inputData.releasingJump) //Whenever the player is in midair and going up, we allow them to halve their vertical speed by releasing the jump button.
                        movement.y -= movement.y * 0.5f;
                }

            }
            else
            {
                if (Physics.Raycast(transform.position + Vector3.up * m_capsule.height * 0.5f, Vector3.down, out m_hit, playerHeight * 0.66f, groundingLayers, QueryTriggerInteraction.Ignore))
                {
                    if (new Vector3(inputData.motionInput.x, 0f, inputData.motionInput.z).magnitude > 0f && !m_sliding && Vector3.Dot(m_hit.normal, movement) > 0f)
                    {
                        if (m_hit.normal.y > slopeBias && movement.y > -2f && movement.y < 0f && m_hit.normal.y < 0.99f) //If we're not set to slide down the slope normally
                            transform.position = new Vector3(transform.position.x, m_hit.point.y, transform.position.z);
                    }
                }


                if (!lockMovement)
                {
                    if (jumpMode != RushJump.none)
                    {
                        if (m_canJump && !isCrouching)
                        {
                            if (inputData.pressedJump)
                            {
                                if (m_jumpTimer <= 0f)
                                {
                                    if (enableSounds)
                                        m_sound.PlayOneShot(jumpingSound, soundVolume);

                                    switch (jumpMode)
                                    {
                                        case (RushJump.normal):
                                            movement.y = jumpPower; //No matter what, you will always jump with consistent power.
                                            break;

                                        case (RushJump.enhanced):
                                            movement.y = jumpPower + ((isSprinting) ? jumpPower * 0.15f : 0f); //Jumppower is scaled up if enhanced jumping is enabled.
                                            break;

                                        case (RushJump.leaping):
                                            movement.y = jumpPower; //Jumping is normal here, but the next line flings the player if they're sprinting.

                                            if (isSprinting)
                                            {
                                                Vector3 m_applied = Vector3.zero;
                                                switch (playerMode)
                                                {
                                                    case (RushMode.firstPerson):
                                                        m_applied = new Vector3(inputData.motionInput.x * sprintSpeed * 2f, 0f, inputData.motionInput.z * sprintSpeed * 2f);
                                                        m_applied.y = jumpPower * 0.5f;

                                                        movement += transform.TransformDirection(m_applied);
                                                        break;

                                                    case (RushMode.thirdPerson):
                                                        m_applied = new Vector3(m_thirdPersonMotion.x, 0f, m_thirdPersonMotion.z) * 2f;
                                                        m_applied.y = jumpPower * 0.5f;
                                                        movement += m_applied;
                                                        break;
                                                }
                                            }
                                            break;
                                    }

                                    m_jumpTimer = 0.05f;
                                }
                            }
                        }
                    }
                }


                /*
                 * If the surface below us is flat enough to count as ground, we clamp the player's gravity to prevent it 
                 * from stacking up and making the player fall through terrain!
                 * 
                 * We leave some gravity so when the player walks down a slope, they move to meet the terrain. Otherwise they'd walk off slopes like they were flat
                 * ground, and this looks extremely ugly.
                */

                movement.y = Mathf.Clamp(movement.y, (new Vector3(inputData.motionInput.x, 0f, inputData.motionInput.z).magnitude > 0f) ? -0.5f : 0f, Mathf.Infinity);
            }
            ///

            // CAMERA MANAGEMENT

            m_camOrigin.y = m_camOriginBaseHeight * (m_capsule.height / playerHeight) * ((isCrouching) ? cameraCrouchDrop : 1f);


            if (!lockCamera)
            {

                /// Controls landing effects (if enabled). Uses m_lastGrav to compare against the player's previous gravity and figure out if they've landed.
                if (m_landingTimer > 0f)
                {
                    if (!isGrounded)
                        m_landingTimer -= Time.deltaTime; //If we're not on the ground, we count the timer down for landing effects! This makes it so that small drops don't cause the landing sound to play.
                    else
                        m_landingTimer = landingSoundTimer; //The timer is reset if we hit the ground.
                }

                if (movement.y - m_lastGrav > 5f && m_lastGrav < -5f && isGrounded) //This checks to make sure the player is falling a certain speed before using landing effects.
                {
                    if (enableSounds)
                    {
                        if (m_landingTimer <= 0f && m_slideTimer <= 0f)
                        {
                            m_sound.PlayOneShot(landingSound, soundVolume); //If we're allowed to play sounds on landing, we do it here. The timer is reset to prevent spamming.
                            m_landingTimer = landingSoundTimer;
                        }
                    }

                    if (landingEffects && m_slideTimer <= 0f)
                    {
                        m_camPosTracer -= Vector3.up * -m_lastGrav * 0.3f; //If effects are enabled, the camera is bowed down for the landing.
                        m_camPosTracer.y = Mathf.Clamp(m_camPosTracer.y, -4f, 4f); //This effect is clamped to prevent the camera from bowing too low.
                    }
                }
                ///

                /// Dedicated to controlling viewbobbing and walkSounds.
                if (!lockMovement)
                {
                    if (isGrounded)
                    {
                        if (new Vector3(inputData.motionInput.x, 0f, inputData.motionInput.z).magnitude > 0.3f)
                        {
                            /// Viewbob effects and management for the m_walkTime value. Scales relative to your current speed and however fast you normally move, meaning a low moveSpeed and high sprintSpeed result in quick footsteps.
                            m_walkTime += viewBobRate * Time.deltaTime * ((isSprinting) ? (10f + (sprintSpeed / moveSpeed) * 2f) : 10f); //m_walkTime controls the viewbob's rate! It's scaled depending on speed.

                            if (enableViewbob)
                                m_camPosTracer = RLerp(m_camPosTracer, m_camOrigin + (Vector3.up * viewBobPower * Mathf.Sin(m_walkTime) * ((isSprinting) ? 0.15f : 0.1f)), 0.4f); //Bobbing effects.
                            else
                                m_camPosTracer = RLerp(m_camPosTracer, m_camOrigin, 0.4f); //The camera is lerped to its default position if viewbobbing is disabled.
                                                                                           ///

                            /// This section is dedicated to step sounds, which play on the lowest part of the camera's viewbob curve. Disabling viewbob doesn't affect the m_walkTime value!
                            if (enableSounds && !overrideFootsteps)
                            {
                                if (Mathf.Sin(m_walkTime) < -0.8f)
                                {
                                    if (!m_stepped)
                                    {
                                        m_sound.PlayOneShot(walkSounds[Random.Range(0, walkSounds.Length)], soundVolume);
                                        m_stepped = true;
                                    }
                                }
                                else
                                    m_stepped = false;
                            }
                            ///
                        }
                        else
                            m_camPosTracer = RLerp(m_camPosTracer, m_camOrigin, 0.4f); //If the player isn't moving fast enough to be constituted as "walking", the camera returns to normal.
                    }
                    else
                        m_camPosTracer = RLerp(m_camPosTracer, m_camOrigin, 0.4f); //If the player isn't on the ground, the camera goes back to its default position.
                }
            }


            /// Zoom Feature. Fairly straightforward. Controlled with a lil' recursive linear interpolation formula.

            m_zoomGoal = 0f; //This is reset per-frame.

            if (!lockCamera) //We first determine if we're zooming with the zoom button.
            {
                
                if (inputData.holdingZoom && zoomIntensity != 0f)
                {
                    m_zoomGoal -= zoomIntensity;
                }
                
            }

            if (isSprinting && new Vector3(movement.x, movement.z).magnitude > 0.05f)
                m_zoomGoal += sprintIntensity;

            m_zoomAdditive = RLerp(m_zoomAdditive, m_zoomGoal, 0.2f);

            ///


            //ANIMATIONS

            /// Just assigning all of the parameters here if they've been marked for use. If you need a better explanation, mouse over the parameters in the inspector.

            if (playerAnimator != null)
            {
                if (crouchMode != RushCrouchSetting.none && playerAnimator != null && crouchingWeight != string.Empty)
                    playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex(crouchingWeight), m_crouchTime);
                if (walkingParameter != string.Empty)
                    playerAnimator.SetBool(walkingParameter, Vector3.ProjectOnPlane(inputData.motionInput, Vector3.up).magnitude > 0.2f);
                if (sprintingParameter != string.Empty)
                    playerAnimator.SetBool(sprintingParameter, isSprinting);
                if (crouchingParameter != string.Empty)
                    playerAnimator.SetBool(crouchingParameter, isCrouching);
                if (groundedParamter != string.Empty)
                    playerAnimator.SetBool(groundedParamter, isGrounded);
                if (relativeSpeedParameter != string.Empty && moveSpeed != 0f)
                    playerAnimator.SetFloat(relativeSpeedParameter, m_topSpeed / moveSpeed);
            }

            ///


            m_lastGrav = movement.y; //The gravity from the last frame is set here. This is mostly used to compare against prior frame motion.

        }

        private void LateUpdate()
        {
            
            switch (playerMode)
            {
                case RushMode.firstPerson:
 
                    if(!lockCamera)
                    m_camAngles += new Vector3(-inputData.mouseInput.y * mouseSensitivity, inputData.mouseInput.x * mouseSensitivity); //The current motion of the mouse is taken in, multiplied by the mouse sensitivity, and then added onto the goal camera angles.

                    m_camAngles.x = Mathf.Clamp(m_camAngles.x, verticalRestraint.x, verticalRestraint.y); //The camera angles are restricted here, so the player can't flip their head completely down and snap their neck.

                    gameObject.transform.rotation = Quaternion.Euler(0f, m_camAngles.y, 0f); //The horizontal rotation is applied to the player's body.

                    Vector2 m_goalTilt;
                    m_goalTilt = new Vector2(((cameraTilting) ? inputData.motionInput.x * cameraTiltRollPower : 0f) + ((cameraMouseTilting) ? inputData.mouseInput.x * cameraMouseTiltRollPower : 0f)
                        , ((cameraTilting) ? inputData.motionInput.z * cameraTiltPitchPower : 0f));

                    if (cameraTilting || cameraMouseTilting)
                        m_cameraTilt = RLerp(m_cameraTilt, m_goalTilt, cameraTiltSpeed);
                    else
                        m_cameraTilt = RLerp(m_cameraTilt, Vector2.zero, cameraTiltSpeed);

                    playerCamera.transform.rotation = Quaternion.Euler(m_camAngles) * Quaternion.Euler(m_cameraTilt.y, 0f, -m_cameraTilt.x); //The vertical rotation is applied to the player's head.
                    break;

                case RushMode.thirdPerson:

                    if (!lockCamera)
                        m_camAngles += new Vector3(-inputData.mouseInput.y * mouseSensitivity, -inputData.mouseInput.x * mouseSensitivity); //The current motion of the mouse is taken in, multiplied by the mouse sensitivity, and then added onto the goal camera angles.

                    m_camAngles.x = Mathf.Clamp(m_camAngles.x, -50f, 85f); //The camera angles are restricted here, so the player can't flip their head completely down and snap their neck.

                    break;

            }


            m_lastPosition = transform.position;
        }

        void FixedUpdate()
        {

            if (slidingOnSlopes)
            {
                if (m_sliding)
                {
                    m_canJump = false;
                    Vector3 m_pos = m_slidingNormal;

                    movement.x += (1f - m_slidingNormal.y) * m_slidingNormal.x * 0.5f;
                    movement.z += (1f - m_slidingNormal.y) * m_slidingNormal.z * 0.5f;
                }
            }

            m_rig.velocity = movement * Time.fixedDeltaTime * 60f; //Movement is applied here. Motion is scaled on framerate to smooth things out.

            switch (playerMode)
            {
                case RushMode.firstPerson:
                    playerCamera.transform.localPosition += (m_camPosTracer - playerCamera.transform.localPosition) * 0.1f; //The position of the camera is shifted as needed.
                    break;

                case (RushMode.thirdPerson):
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, Mathf.Rad2Deg * Mathf.Atan2(movement.x, movement.z), 0f), 0.2f * (new Vector3(movement.x, 0f, movement.z).magnitude / moveSpeed));

                    Vector3 m_camPos = Vector3.zero;
                  
                    m_camPos = (transform.position + Vector3.up * playerHeight * 0.5f) + (m_thirdPersonRotation * new Vector3(0f, 0f, -thirdPersonOrbitDistance));

                    if (Physics.SphereCast(transform.position + Vector3.up * m_capsule.height * 0.5f, 0.1f, m_thirdPersonRotation * -Vector3.forward, out m_hit, thirdPersonOrbitDistance, groundingLayers, QueryTriggerInteraction.Ignore))
                    {

                        if (rigidbodyOcclusion)
                        {
                            m_camPos = m_hit.point + m_hit.normal * 0.1f;
                        }
                        else
                        {
                            if (m_hit.collider.GetComponent<Rigidbody>() != null)
                            {
                                if (m_hit.collider.GetComponent<Rigidbody>().isKinematic)
                                    m_camPos = m_hit.point + m_hit.normal * 0.1f;
                            }
                            else
                                m_camPos = m_hit.point + m_hit.normal * 0.1f;
                        }

                    }

                    playerCamera.transform.position = m_camPos;

                    playerCamera.transform.rotation = Quaternion.LookRotation(((transform.position + Vector3.up * playerHeight * 0.5f) - playerCamera.transform.position).normalized);
                    break;
            }
            ///

        }

        public static void RushStepEventHandler(AudioClip sound)
        {
            current.m_sound.PlayOneShot(sound, current.soundVolume);
        }

        private void OnCollisionStay(Collision collision)
        {

            //If the object we're standing on happens to be a rigidbody that's active, sliding calculations will be inaccurate. As a result, we stop here if it's a moving rigidbody.
            if (collision.collider.GetComponent<Rigidbody>() != null)
                if (!collision.collider.GetComponent<Rigidbody>().isKinematic)
                    return;

            /// This entire section focuses on preventing the player from climbing or jumping up slopes. While sliding can be disabled, for the sake of gameplay, you cannot change settings that prevent the player from jumping up hills. If it's too steep, it ain't happening.
            Vector3 normal = Vector3.zero;

            if (Physics.Raycast(transform.position + Vector3.up * m_capsule.height * 0.5f, Vector3.down, out m_hit))
                normal = m_hit.normal;

            if (Vector3.Dot(normal, Vector3.up) > 0.1f) //We only need to make the following decisions if the object we're colliding with is beneath us.
            {
                if (slidingOnSlopes)
                {
                    if (movement.y < 0f)
                    {
                        if (Vector3.Dot(m_hit.normal, Vector3.up) < slopeBias) //If sliding on slopes is enabled, we check to see if the current surface is too steep. If so, the player can no longer jump, and they are shunted down the slope. If the slope isn't too steep, the player is then allowed to jump.
                        {
                            m_sliding = true;
                            m_slideHolder = 0.1f;
                            m_slidingNormal = normal;
                        }
                        else
                        {
                            m_sliding = false;
                            m_slidingNormal = Vector3.zero;
                            m_canJump = true;
                        }
                    }
                }
                else
                    m_canJump = true;
            }
            ///

        }

        public static float SmoothUp(float evaluate)
        {
            return (evaluate < 1f) ? Mathf.Log10((Mathf.Clamp01(evaluate) + 0.1f) * 10f) * 0.96f : 1f;
        }

        public static Vector3 RLerp(Vector3 a, Vector3 b, float t)
        {
            t = 1f - Mathf.Pow(1f - t, Time.deltaTime * 60f);

            return (Vector3.Distance(a, b) > 0.02f) ? (a + (b - a) * t) : b;
        }

        public static float RLerp(float a, float b, float t)
        {
            t = 1f - Mathf.Pow(1f - t, Time.deltaTime * 60f);

            return (Mathf.Abs(a - b) > 0.02f) ? (a + (b - a) * t) : b;
        }

        public void Teleport(Vector3 position)
        {
            transform.position = position;

            switch (playerMode)
            {
                case (RushMode.thirdPerson):
                    Vector3 m_camPos = Vector3.zero;

                    m_camPos = transform.position + ((m_thirdPersonRotation * -Vector3.forward) * thirdPersonOrbitDistance) + movement * Time.fixedDeltaTime;

                    if (Physics.SphereCast(transform.position, 0.5f, (m_camPos - transform.position).normalized, out m_hit, thirdPersonOrbitDistance, groundingLayers, QueryTriggerInteraction.Ignore))
                    {

                        if (rigidbodyOcclusion)
                        {
                            m_camPos = m_hit.point + m_hit.normal * 0.1f;
                        }
                        else
                        {
                            if (m_hit.collider.GetComponent<Rigidbody>() != null)
                            {
                                if (m_hit.collider.GetComponent<Rigidbody>().isKinematic)
                                    m_camPos = m_hit.point + m_hit.normal * 0.1f;
                            }
                            else
                                m_camPos = m_hit.point + m_hit.normal * 0.1f;
                        }

                    }

                    playerCamera.transform.position = m_camPos;

                    break;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * playerHeight * 0.5f, new Vector3(playerRadius * 2f, playerHeight, playerRadius * 2f));
            Gizmos.DrawLine(transform.position + Vector3.up * playerHeight * 0.5f, (transform.position + Vector3.up * playerHeight * 0.5f) - Vector3.up * playerHeight * 0.66f);
        }

        public enum RushSprintSetting
        { //Changes how sprint behaves.

            none = 0, //Self explanatory.
            normal = 1, //Hold sprint to sprint. When released, you will return to normal speed.
            classic = 2 //While you're running, pressing the sprint key will engage sprinting. Sprinting will only stop when you do.
        }

        public enum RushCrouchSetting
        { //Changes how crouching behaves.
            none = 0,
            normal = 1, //Crouching doesn't affect whether or not the player can sprint. Get a move on!
            noSprint = 2 //Crouching disables the ability to sprint. Good if you're practical or whatever.
        }

        public enum RushMode
        { //Determines which player type we're working with. Important!
            firstPerson = 0,
            thirdPerson = 1
        }

        public enum RushJump
        { //Determines how jumping is calculated.
            none = 0,
            normal = 1,
            enhanced = 2,
            leaping = 3
        }

    }


    public struct RushInputData
    {
        public Vector3 motionInput;
        public Vector2 mouseInput;

        public bool pressedJump;
        public bool holdingJump;
        public bool releasingJump;

        public bool pressedZoom;
        public bool holdingZoom;
        public bool releasingZoom;

        public bool pressedSprint;
        public bool holdingSprint;
        public bool releasingSprint;

        public bool pressedCrouch;
        public bool holdingCrouch;
        public bool releasingCrouch;

        public RushInputData(Vector3 initialMotion)
        {
            motionInput = initialMotion;
            mouseInput = Vector2.zero;

            pressedJump = false;
            holdingJump = false;
            releasingJump = false;

            pressedZoom = false;
            holdingZoom = false;
            releasingZoom = false;

            pressedSprint = false;
            holdingSprint = false;
            releasingSprint = false;

            pressedCrouch = false;
            holdingCrouch = false;
            releasingCrouch = false;
        }
    }
}