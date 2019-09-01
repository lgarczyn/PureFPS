using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : AWeaponController
{
    [SerializeField]
    private float walkSpeed = 8f;
    public float speedMultiplier = 1f;
    [SerializeField]
    private float jumpVelocity = 8f;
    [SerializeField]
    private float stickToGroundForce = 0.1f;
    [SerializeField]
    private float gravityMultiplier = 1f;
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private bool constantFire = false;
    [SerializeField]
    private float XSensitivity = 2f;
    [SerializeField]
    private float YSensitivity = 2f;

    enum State
    {
        Grounded,
        Jumping,
        Falling,
    }

    private State m_State;
    //private bool m_PreviouslyGrounded;
    private Vector2 m_TargetMovement;
    private Vector2 m_TargetRotation;
    private Vector2 m_CurrentRotation;
    private Vector2 m_CurrentVelocity;


    private Vector3 m_MoveDir = Vector3.zero;
    private bool m_cursorIsLocked = true;

    private CollisionFlags m_CollisionFlags;
    private CharacterController m_CharacterController;
    private AudioSource m_AudioSource;

    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_AudioSource = GetComponent<AudioSource>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        m_State = State.Grounded;

        InternalLockUpdate();
    }

    private void Update()
    {
        float hRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
        float vRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

        m_TargetRotation += new Vector2(hRot, vRot);


        // the jump state needs to read here to make sure it is not missed
        if (CrossPlatformInputManager.GetButtonDown("Jump") && m_State == State.Grounded)
        {
            m_State = State.Jumping;
        }

        //if (m_State == State.Grounded)
        ////    m_MoveDir.y = 0f;
        //}

        InternalLockUpdate();
    }

    private void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_cursorIsLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_cursorIsLocked = true;
        }

        if (m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    private void FixedUpdate()
    {
        float speed = GetInput();

        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_TargetMovement.y + transform.right * m_TargetMovement.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -stickToGroundForce;

            if (m_State == State.Jumping)
            {
                m_MoveDir.y = jumpVelocity * speedMultiplier;
                m_State = State.Falling;
            }
            else
            {
                m_State = State.Grounded;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
    }


    private float GetInput()
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        float speed = walkSpeed * speedMultiplier;
        m_TargetMovement = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_TargetMovement.sqrMagnitude > 1)
        {
            m_TargetMovement.Normalize();
        }

        return speed;
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    [SerializeField]
    private float m_LastMovementUpdate;

    private void LateUpdate()
    {
        UpdateRotation(Time.time);
        transform.localRotation = Quaternion.AngleAxis(m_CurrentRotation.x, Vector3.up);
        playerCamera.transform.localRotation = Quaternion.AngleAxis(-m_CurrentRotation.y, Vector3.right);

    }

    private void UpdateRotation(float time)
    {
        float delta = time - m_LastMovementUpdate;

        if (delta == 0)
            return;
        if (delta < 0)
            return;
        //    throw new System.Exception("DeltaTime is negative");

        m_LastMovementUpdate = time;

        //float factor = Mathf.Max(0.01f, Vector2.Distance(m_CurrentRotation, m_TargetRotation) / 100f);
        //m_MovementTime, m_MaxSpeed
        m_CurrentRotation = Vector2.SmoothDamp(m_CurrentRotation, m_TargetRotation, ref m_CurrentVelocity, 0.1f, float.PositiveInfinity, delta);
    }


    public override WeaponIntentions GetIntentions()
    {
        Quaternion rotation = playerCamera.transform.rotation;
        return new WeaponIntentions(Input.GetMouseButton(0) || constantFire, Input.GetMouseButton(1));
    }

    public override void HandleRecoil(Vector2 recoil)
    {
        m_CurrentVelocity += recoil;
        m_TargetRotation += recoil;
    }

    public override Quaternion GetRotation(float time)
    {
        UpdateRotation(time);
        return Quaternion.Euler(-m_CurrentRotation.y, m_CurrentRotation.x, 0f);
    }
}

[Serializable]
public class MouseLook
{
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;
    public bool lockCursor = true;


    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;
    private bool m_cursorIsLocked = true;

    public void Init(Transform character, Transform camera)
    {
        m_CharacterTargetRot = character.localRotation;
        m_CameraTargetRot = camera.localRotation;
    }


    public void LookRotation(Transform character, Transform camera)
    {
        float vRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
        float hRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

        m_CharacterTargetRot *= Quaternion.Euler(0f, vRot, 0f);
        m_CameraTargetRot *= Quaternion.Euler(-hRot, 0f, 0f);

        if (clampVerticalRotation)
            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

        if (smooth)
        {
            character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                smoothTime * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                smoothTime * Time.deltaTime);
        }
        else
        {
            character.localRotation = m_CharacterTargetRot;
            camera.localRotation = m_CameraTargetRot;
        }

        UpdateCursorLock();
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {//we force unlock the cursor if the user disable the cursor locking helper
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        //if the user set "lockCursor" we check & properly lock the cursos
        if (lockCursor)
            InternalLockUpdate();
    }

    private void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_cursorIsLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_cursorIsLocked = true;
        }

        if (m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

}
