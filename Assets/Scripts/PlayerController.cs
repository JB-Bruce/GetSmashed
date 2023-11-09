using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Assignation")]

    [SerializeField] float maxSpeed;
    [SerializeField] float maxFallSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float horizontalDeceleration;

    [SerializeField] float baseGravity;
    [SerializeField] float jumpingGravity;

    [SerializeField] float jumpForce;

    [SerializeField] int doubleJumpAmount;
    int currentDoubleJumpAmount;
    [SerializeField] float doubleJumpIntervalMinima;

    bool dashed;
    bool dashing;
    float dashTimer = 0f;
    float lastDashed = -10;
    Vector2 dashDirection;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float minimumDashInterval;
    


    [Header("Other")]

    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform feetPoint;
    [SerializeField] float inputsMinima;


    Vector2 movements;

    bool grounded;

    float lastJump = -10;

    bool jumping;

    private void Start()
    {
        rb.gravityScale = jumpingGravity;
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(feetPoint.position, Vector2.down, .1f);

        if(hit.collider != null)
        {
            if(dashed && !dashing)
                dashed = false;

            if (!grounded)
            {
                grounded = true;
                currentDoubleJumpAmount = 0;
                rb.gravityScale = jumpingGravity;
            }
        }
        else
        {
            grounded = false;
        }
    }

    private void FixedUpdate()
    {
        if(dashing)
        {
            rb.velocity = dashDirection * dashSpeed * Time.fixedDeltaTime;
            dashTimer += Time.fixedDeltaTime;

            if(dashTimer >= dashDuration)
            {
                rb.gravityScale = baseGravity;
                dashing = false;
                dashTimer = 0;
            }

            return;
        }

        if(movements.magnitude > inputsMinima)
        {
            rb.velocity = new(Mathf.Clamp(rb.velocity.x + movements.x * acceleration * Time.fixedDeltaTime, -maxSpeed, maxSpeed), rb.velocity.y);
        }
        else
        {
            rb.velocity = new(rb.velocity.x * horizontalDeceleration, rb.velocity.y);
        }
        rb.velocity = new(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -maxFallSpeed, maxFallSpeed));

    }

    public void Jump(bool startJump)
    {
        if (dashing)
            return;

        if(startJump)
        {
            if ((!grounded && Time.time - lastJump < doubleJumpIntervalMinima) || currentDoubleJumpAmount >= doubleJumpAmount)
                return;

            if(!grounded)
                currentDoubleJumpAmount++;

            lastJump = Time.time;
            jumping = true;
            rb.gravityScale = jumpingGravity;
            rb.velocity = new(rb.velocity.x, 0f);
            rb.AddForce(new(0, jumpForce), ForceMode2D.Impulse);
            return;
        }
        else
        {
            rb.gravityScale = baseGravity;
            jumping = false;
            return;
        }
    }

    public void Dash()
    {
        if (movements.magnitude < inputsMinima || dashed)
            return;
        if(Time.time - lastDashed > minimumDashInterval)
        {
            dashed = true;
            dashing = true;
            rb.gravityScale = 0f;
            dashDirection = movements.normalized;
            lastDashed = Time.time;
        }
        
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        movements = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        Jump(ctx.ReadValueAsButton());
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        Dash();
    }
}
