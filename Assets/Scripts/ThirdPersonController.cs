using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class ThirdPersonController : MonoBehaviour
{
    private const string speedParamName = "Speed";
    private const string groundedParamName = "Grounded";
    private const string jumpParamName = "Jump"; 
    private const float lookTheshold = 0.01f;


    [Header("Cinemachine")]
    [SerializeField]
    private Transform cameraTarget;

    [SerializeField]
    private float topClamp = 70f;

    [SerializeField]
    private float bottomClamp = -30f;

    [Header("Speed")]
    [SerializeField]
    private float movementSpeed = 3f; 
    
    [SerializeField]
    private float lookSpeed = 10f; 

    [Header("Jump")]
    [SerializeField]
    private float jumpStrength = 5f;

    [SerializeField]
    private float jumpDowntime = 1f; 

    [Header("Grounded")]
    [SerializeField]
    private Transform groundedCheckPoint;

    [SerializeField]
    private float groundedCheckRadius = 0.2f; 

    [SerializeField]
    private LayerMask groundLayer;

    private Rigidbody body; 
    private Animator animator;
    private Vector2 move; 
    private Vector2 look;
    private float currentSpeed;
    private float yaw; 
    private float pitch;
    private bool isGrounded = true; 
    private bool isRunning; 
    private bool canJump = true;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        GroundedCheck();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float targetSpeed = (isRunning ? movementSpeed * 2f : movementSpeed) * move.magnitude;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * 8f);

        Vector3 forward = cameraTarget.forward;
        Vector3 right = cameraTarget.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * move.y + right * move.x).normalized; 

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);

            Vector3 currentVelocity = body.linearVelocity; 
            body.linearVelocity = new Vector3(moveDirection.x * currentSpeed, currentVelocity.y, moveDirection.z * currentSpeed);

        }
        else
        {
            Vector3 currentVelocity = body.linearVelocity; 
            body.linearVelocity = new Vector3(0f, currentVelocity.y, 0f);
        }
        
        float normalizeAninSpeed = currentSpeed / (movementSpeed * 2f);
        animator.SetFloat(speedParamName, normalizeAninSpeed); 
    }

    private void Jump()
    {
        // Implement jump logic here
        if (isGrounded && canJump)
        {
            return; 
            
        }
        body.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
        canJump = false;
        StartCoroutine(JumpCooldownCoroutine());
        
        animator.SetTrigger(jumpParamName);
    }

    private IEnumerator JumpCooldownCoroutine()
    {
        yield return new WaitForSeconds(0.25f);

        var waitForGrounded = new WaitUntil(() => isGrounded);
        yield return waitForGrounded;

        yield return new WaitForSeconds(jumpDowntime);
        canJump = true;
    }

    private void Look()
    {
        if (look.sqrMagnitude >= lookTheshold)
        {
            float deltaTimeMultiplier = Time.deltaTime * lookSpeed;
            yaw += look.x * deltaTimeMultiplier;
            pitch -= look.y * deltaTimeMultiplier;
        }
        yaw = ClampAngle(yaw, float.MinValue, float.MaxValue);
        pitch = ClampAngle(pitch, bottomClamp, topClamp);

        cameraTarget.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private void GroundedCheck()
    {
        // Implement grounded check logic here
        isGrounded = Physics.CheckSphere(groundedCheckPoint.position, groundedCheckRadius, groundLayer);
        animator.SetBool(groundedParamName, isGrounded);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundedCheckPoint == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundedCheckPoint.position, groundedCheckRadius);
    }

    private void OnMove(InputValue inputValue)
    {
        move = inputValue.Get<Vector2>();
    }

    private void OnJump()
    {
        // Implement jump logic here
        Jump();
    }

    private void OnRun(InputValue inputValue)
    {
        isRunning = inputValue.isPressed;
    }

    private void OnLook(InputValue inputValue)
    {
        look = inputValue.Get<Vector2>();
        // Implement look logic here
    }
}
