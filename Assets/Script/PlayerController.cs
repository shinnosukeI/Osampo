using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FreeMoveInputSystem : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float accel = 12f;
    public float jumpPower = 5.5f;
    public float gravity = -9.81f;
    public float groundSnap = -2f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask = ~0;

    // PlayerInput 経由で参照
    public InputActionReference moveAction;
    public InputActionReference lookAction;   // 使うのはカメラ側
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;

    private CharacterController cc;
    private float yVel;
    private float currentSpeed;
    private bool isGrounded;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (!groundCheck)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -cc.height * 0.5f + cc.skinWidth + 0.05f, 0);
            groundCheck = gc.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        moveAction?.action?.Enable();
        lookAction?.action?.Enable();
        jumpAction?.action?.Enable();
        sprintAction?.action?.Enable();
    }

    void OnDisable()
    {
        moveAction?.action?.Disable();
        lookAction?.action?.Disable();
        jumpAction?.action?.Disable();
        sprintAction?.action?.Disable();
    }

    void Update()
    {
        // === 止める方式：Wキーでアニメ再生/停止（パラメータ不要） ===
        bool isPressW = Keyboard.current != null && Keyboard.current.wKey.isPressed;
        if (animator != null)
        {
            animator.speed = isPressW ? 1f : 0f;
        }

        // === 移動処理（今まで通り InputAction を使う） ===
        Vector2 mv = (moveAction != null) ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        Vector3 inputDir = new Vector3(mv.x, 0f, mv.y);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        // Ground
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        // 移動方向（プレイヤーの向き基準）
        Vector3 moveDir = transform.TransformDirection(inputDir);

        bool sprint = (sprintAction != null) && sprintAction.action.IsPressed();
        float targetSpeed = (sprint ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.deltaTime);

        // 重力/ジャンプ
        if (isGrounded)
        {
            if (yVel < 0f) yVel = groundSnap;
            if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
                yVel = jumpPower;
        }
        else
        {
            yVel += gravity * Time.deltaTime;
        }

        Vector3 vel = new Vector3(moveDir.x * currentSpeed, yVel, moveDir.z * currentSpeed);
        cc.Move(vel * Time.deltaTime);

        // ESCでマウス解放
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            else
            { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}