using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    [Header("PLAYERS UI")]
    public GameObject velocityText;
    public GameObject speedText;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        PlayerDebuggerUI();
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded || OnSlope())
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // Mode - Crouching
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            //manually applied gravity downwards force on slope when moving up slope
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 30f, ForceMode.Force);
            else if (rb.linearVelocity.y < 0 && state == MovementState.crouching)
            {
                Debug.Log("moving down slope while crouching");
            }
            //if crouching while on slope i can turn off the friction or linear damping, and add some velocity.
        }

        // on ground
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // in air
        else if (!grounded)
        {
            Air_Accelerate();

            //air control
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    //private void Air_Accelerate()
    //{
    //    Vector3 wish_dir = moveDirection.normalized; // Normalize to get direction only
    //    float wish_speed = moveSpeed * airMultiplier; // Set air movement speed

    //    // Project velocity onto wish direction
    //    float projSpeed = Vector3.Dot(rb.linearVelocity, wish_dir);

    //    // Calculate how much speed needs to be added
    //    float add_speed = wish_speed - projSpeed;

    //    if (add_speed <= 0)
    //    {
    //        return; // Already at or above max desired speed
    //    }

    //    // Acceleration formula
    //    float accelAmount = 300.0f; // Similar to sv_accelerate
    //    float accelspeed = accelAmount * wish_speed * Time.deltaTime;

    //    // Cap acceleration to not exceed needed speed
    //    if (accelspeed > add_speed)
    //    {
    //        accelspeed = add_speed;
    //    }

    //    // Apply acceleration
    //    rb.linearVelocity += wish_dir * accelspeed;
    //}


    private void Air_Accelerate()
    {
        //Put acceleration code here.

        //0. normalize the wish_velcity so the player does not gain speed past a certain amount
        //1. so we use the velocity, and wish_dir vectors dot product to find the limit when we increase the players speed
        //2. then we subtract our wish_dir - proj_speed = add_speed
        //2. if add_speed <=0 then our dot product puts us beyond the acceleration limit
        //3. else
        //
        // acceleration = dv / dt =
        // _dv = acceleration * dt;
        // check if _dv > projSpeed
        // _dv = projSpeed

        // player.linearVelocity = player.linearVelocity + wish_dir * _dv

        Vector3 wish_dir = moveDirection.normalized * moveSpeed;

        float projSpeed = Vector3.Dot(rb.linearVelocity, wish_dir);//returns 0 when 2nd vector is at 90 degrees

        float wish_speed = 30;
        float add_speed = wish_speed - projSpeed;

        if (add_speed < 0)
        {
            return;
        }

        float accelerationAmount = 300;
        float _dv = accelerationAmount * Time.deltaTime;

        if (_dv > projSpeed)
        {
            _dv = projSpeed;
        }


        rb.linearVelocity = rb.linearVelocity + wish_dir * _dv;
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit linearVelocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y linearVelocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public void PlayerDebuggerUI()
    {
        //need to show the players current velocity
        TextMeshProUGUI textMesh1 = velocityText.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI textMesh2 = speedText.GetComponent<TextMeshProUGUI>();
        textMesh1.text = "Velocity:" + rb.linearVelocity;
        textMesh2.text = "Speed:" + moveDirection.normalized * moveSpeed * 10f;
    }
}
