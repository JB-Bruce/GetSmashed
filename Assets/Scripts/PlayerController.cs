using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public struct Attack
    {
        public AttackEnum AttackEnum;
        public float damage;
        public float knockback;
        public float stunTime;
        public float attackDelay;
        public bool slowMovementsOnAttack;
    }

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

    [SerializeField] float knockbackMultiplier;

    [SerializeField] float baseKnockback;
    [SerializeField] float baseUpKnockback;



    [Header("Others")]

    

    public Rigidbody2D rb;

    [SerializeField] List<Attack> attackList;

    [SerializeField] LayerMask swordMask;

    [SerializeField] Transform feetPoint;
    [SerializeField] float inputsMinima;

    [SerializeField] Animator bottomAnimator;
    [SerializeField] Animator UpAnimator;

    [SerializeField] BoxCollider2D swordCollider;

    [SerializeField] Transform collidersParent;

    float damageTaken = 0.0f;


    Vector2 movements;

    bool grounded;

    float lastJump = -10;

    Attack currentAttack;

    bool attacking;

    bool jumping;
    List<PlayerController> playersHitted = new();

    bool isStunByAttack = false;
    float currentAttackDelay = 0f;

    bool isHit = false;
    float hitTimer = 0f;

    public PlayerPlatformeManager plat;


    private void Start()
    {
        rb.gravityScale = jumpingGravity;
        swordCollider.enabled = false;

        PlayerManager.instance.AddPlayer(this);
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(feetPoint.position, Vector2.down, .1f);

        float velX = rb.velocity.x;

        bottomAnimator.SetFloat("Speed", Mathf.Abs(velX));

        if(isHit)
        {
            hitTimer -= Time.deltaTime;
            if(hitTimer < 0f)
                isHit = false;
        }

        if (isStunByAttack)
        {
            currentAttackDelay -= Time.deltaTime;
            if(currentAttackDelay < 0f)
                isStunByAttack = false;
        }

        /*if(Mathf.Abs(velX) > 0f)
        {
            Vector3 newScale = new(velX > 0 ? 1f : -1f, 1f, 1f);
        }*/

        if (Mathf.Abs(movements.x) > inputsMinima && velX != 0)
        {
            Vector3 newScale = new(movements.x > 0 ? 1f : -1f, 1f, 1f);
            UpAnimator.transform.localScale = newScale;
            bottomAnimator.transform.localScale = newScale;
        }
            

        if(hit.collider != null)
        {
            if(!grounded)
                bottomAnimator.SetBool("StoppedJump", true);

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
        if (isHit)
        {
            if(grounded)
                rb.velocity = new(rb.velocity.x * horizontalDeceleration, rb.velocity.y);
            return;
        }
        

        if (movements.magnitude <= inputsMinima)
        {
            rb.velocity = new(rb.velocity.x * horizontalDeceleration, rb.velocity.y);
        }


        rb.velocity = new(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -maxFallSpeed, maxFallSpeed));

        if (isStunByAttack && currentAttack.slowMovementsOnAttack)
        {
            rb.velocity = new(rb.velocity.x * 0.95f, rb.velocity.y);
            return;
        }
            

        if (dashing)
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

        if(jumping && rb.velocity.y < 0f)
        {
            Jump(false);
        }

        if (movements.magnitude > inputsMinima)
        {
            rb.velocity = new(Mathf.Clamp(rb.velocity.x + movements.x * acceleration * Time.fixedDeltaTime, -maxSpeed, maxSpeed), rb.velocity.y);
        }

    }


    public void TakeDamage(float damage, Vector2 direction, float newKnockback, float hitTime)
    {
        isHit = true;
        hitTimer = hitTime;

        rb.velocity = Vector2.zero;

        rb.gravityScale = baseGravity;
        dashing = false;
        dashTimer = 0;

        damageTaken += damage;
        rb.AddForce((direction * (baseKnockback + ((float)(1f / 10f) * damageTaken) * newKnockback)) + Vector2.up * baseUpKnockback, ForceMode2D.Impulse);
    }

    public void Jump(bool startJump)
    {
        if (dashing || isStunByAttack)
            return;

        if(startJump)
        {
            if ((!grounded && Time.time - lastJump < doubleJumpIntervalMinima) || currentDoubleJumpAmount >= doubleJumpAmount)
                return;

            if(!grounded)
                currentDoubleJumpAmount++;

            bottomAnimator.SetBool("StoppedJump", false);
            bottomAnimator.Play("Jump", 0, 0);

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
        if (movements.magnitude < inputsMinima || dashed || isStunByAttack || isHit)
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

    public void EnableCollisions(Collider2D coll)
    {
        foreach (Collider2D item in collidersParent.GetComponents<Collider2D>())
        {
            Debug.Log(item.gameObject.name);
            Debug.Log(coll.gameObject.name);
            Debug.Log("ff    ");
            Physics2D.IgnoreCollision(coll, item);
        }
    }

    public void SwordHit(Collider2D col)
    {
        PlayerController enemyController = col.GetComponentInParent<PlayerController>();

        if (playersHitted.Contains(enemyController) || col.gameObject.layer == swordMask)
            return;

        

        if (enemyController != null && enemyController != this)
        {
            rb.gravityScale = baseGravity;
            dashing = false;
            dashTimer = 0;

            playersHitted.Add(enemyController);
            enemyController.TakeDamage(currentAttack.damage, (enemyController.transform.position - transform.position).normalized, currentAttack.knockback, currentAttack.stunTime);
        }
    }

    public Attack GetAttackFromEnum(AttackEnum newEnum)
    {
        foreach (var item in attackList)
        {
            if(item.AttackEnum == newEnum)
            {
                return item;
            }
        }
        return attackList[0];
    }

    public void Respawn()
    {
        damageTaken = 0;
        rb.velocity = Vector2.zero;
        transform.position = Vector2.zero;
    }

    public void StartAttack()
    {
        rb.gravityScale = baseGravity;
        dashing = false;
        dashTimer = 0;

        swordCollider.enabled = true;
    }

    public void EndAttack()
    {
        attacking = false;
        swordCollider.enabled = false;
        playersHitted.Clear();
    }

    public void Forward()
    {
        if (attacking || isStunByAttack)
            return;

        attacking = true;
        SetCurrentAttack(AttackEnum.Forward);
        UpAnimator.Play("ForwardAttack", 0, 0);
    }

    public void Smash()
    {
        if (attacking || isStunByAttack || !grounded || jumping)
            return;

        attacking = true;
        SetCurrentAttack(AttackEnum.UpSmash);
        UpAnimator.Play("UpSmash", 0, 0);
    }

    private void SetCurrentAttack(AttackEnum newEnum)
    {
        currentAttack = GetAttackFromEnum(newEnum);
        isStunByAttack = true;
        currentAttackDelay = currentAttack.attackDelay;
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

    public void OnForward(InputAction.CallbackContext ctx)
    {
        Forward();
    }

    public void OnSmash(InputAction.CallbackContext ctx)
    {
        Smash();
    }
}

[System.Serializable]
public enum AttackEnum : uint
{
    None,
    Forward,
    UpSmash,
    DownSmash,
    ForwardSmash
}
