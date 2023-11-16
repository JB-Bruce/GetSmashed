using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    public int lifes;
    
    public Rigidbody2D rb;

    [SerializeField] List<Attack> attackList;

    [SerializeField] LayerMask swordMask;

    [SerializeField] Transform feetPoint;
    [SerializeField] float inputsMinima;

    [SerializeField] Animator bottomAnimator;
    [SerializeField] Animator UpAnimator;

    [SerializeField] BoxCollider2D swordCollider;

    [SerializeField] Transform collidersParent;

    [SerializeField] GameObject hitParticlesPrefab;
    [SerializeField] GameObject wallHitParticlesPrefab;
    [SerializeField] GameObject shieldHitParticlesPrefab;

    [SerializeField] List<SpriteRenderer> tShirtParts;

    public UnityEvent takeDamageEvent;
    public UnityEvent dieEvent;

    public float damageTaken { get; private set; } = 0f;

    bool invincible;
    [SerializeField] float invincibleTime;

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

    public bool isShielding { get; private set; } = false;
    float shieldLife;
    [SerializeField] float shieldMaxLife;
    [SerializeField] float shieldRegenSpeed;
    [SerializeField] float shieldLostOnActivate;
    [SerializeField] float breakShieldStunTime;
    bool pressingShieldBtn = false;

    [SerializeField] GameObject shieldGO;
    [SerializeField] SpriteRenderer shieldSprite;

    [SerializeField] GameObject stunGO;

    public PlayerPlatformeManager plat;


    private void Start()
    {
        invincible = true;
        Invoke("EndInvincibility", invincibleTime);

        rb.gravityScale = jumpingGravity;
        swordCollider.enabled = false;

        Color c = PlayerManager.instance.AddPlayer(this);

        shieldLife = shieldMaxLife;

        isShielding = false;
        shieldGO.SetActive(false);

        stunGO.SetActive(false);

        SetColor(c);
    }

    private void SetColor(Color c)
    {
        foreach (var item in tShirtParts)
        {
            item.color = c;
        }

        shieldSprite.color = new(c.r, c.g, c.b, 72f/255f);
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(feetPoint.position, Vector2.down, .1f);

        float velX = rb.velocity.x;

        bottomAnimator.SetFloat("Speed", Mathf.Abs(velX));

        if (pressingShieldBtn)
            Shield(true);

        if(isShielding)
        {
            shieldLife -= shieldLostOnActivate * Time.deltaTime;
            if( shieldLife <= 0 )
            {
                isShielding = false;
                shieldGO.SetActive(false);
            }
        }
        else
        {
            shieldLife += shieldRegenSpeed * Time.deltaTime;
        }
        shieldLife = Mathf.Clamp(shieldLife, 0f, shieldMaxLife);
        float ratio = shieldLife / shieldMaxLife;
        shieldGO.transform.localScale = Vector2.one * ratio;


        if(isHit)
        {
            if(!stunGO.activeInHierarchy)
                stunGO.SetActive(true); 

            hitTimer -= Time.deltaTime;
            if(hitTimer < 0f)
            {
                stunGO.SetActive(false);
                isHit = false;
                hitTimer = 0f;
            }
        }


        if (isStunByAttack)
        {
            currentAttackDelay -= Time.deltaTime;
            if(currentAttackDelay < 0f)
                isStunByAttack = false;
        }


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
        if (isHit || isShielding)
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
        if (invincible)
            return;

        if(isShielding)
        {
            if (ReduceShield(damage))
                return;

            hitTime += breakShieldStunTime;
        }

        isHit = true;
        hitTimer += hitTime;

        rb.velocity = Vector2.zero;

        rb.gravityScale = baseGravity;
        dashing = false;
        dashTimer = 0;

        damageTaken += damage;
        rb.AddForce((direction * (baseKnockback + ((float)(1f / 10f) * damageTaken) * newKnockback)) + Vector2.up * baseUpKnockback, ForceMode2D.Impulse);

        takeDamageEvent.Invoke();
    }

    public void Jump(bool startJump)
    {
        if (dashing || isStunByAttack || isHit || isShielding)
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
        if (movements.magnitude < inputsMinima || dashed || isStunByAttack || isHit || isShielding)
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
            Physics2D.IgnoreCollision(coll, item);
        }
    }

    public bool ReduceShield(float damage)
    {
        shieldLife -= damage;
        if(shieldLife <= 0)
        {
            shieldLife = 0;
            isShielding = false;
            shieldGO.SetActive(false);

            return false;
        }
        return true;
    }

    public void SwordHit(Collider2D col, Vector2 nearHit)
    {
        PlayerController enemyController = col.GetComponentInParent<PlayerController>();

        if (playersHitted.Contains(enemyController) || col.gameObject.layer == swordMask)
            return;

        if (enemyController != null && enemyController != this)
        {
            playersHitted.Add(enemyController);

            rb.gravityScale = baseGravity;
            dashing = false;
            dashTimer = 0;

            if(!enemyController.isShielding) 
            {
                GameObject particle = Instantiate(hitParticlesPrefab, nearHit, Quaternion.identity);
                particle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            else
            {
                GameObject particle = Instantiate(shieldHitParticlesPrefab, nearHit, Quaternion.identity);
                particle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            

            enemyController.TakeDamage(currentAttack.damage, (enemyController.transform.position - transform.position).normalized, currentAttack.knockback, currentAttack.stunTime);
        }
        else
        {
            Instantiate(wallHitParticlesPrefab, nearHit, Quaternion.identity);
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
        if (invincible)
            return;

        invincible = true;
        Invoke("EndInvincibility", invincibleTime);

        rb.velocity = Vector2.zero;
        transform.position = Vector2.zero;

        lifes = Math.Clamp(lifes - 1, 0, 99);
        damageTaken = 0;
        

        dieEvent.Invoke();

        if (lifes == 0)
            Die();
    }

    private void Die()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        this.enabled = false;
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

    private void Shield(bool start)
    {
        if (attacking || isStunByAttack)
            return;

        if(start)
        {
            if(shieldLife > 10f && !isShielding)
            {
                isShielding = true;
                shieldGO.SetActive(true);
            }
        }
        else
        {
            isShielding = false;
            shieldGO.SetActive(false);
        }
    }

    public void Forward()
    {
        if (attacking || isStunByAttack || isShielding)
            return;

        attacking = true;
        SetCurrentAttack(AttackEnum.Forward);
        UpAnimator.Play("ForwardAttack", 0, 0);
    }

    public void Smash()
    {
        if (attacking || isStunByAttack || !grounded || jumping || isShielding)
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

    private void EndInvincibility()
    {
        invincible = false;
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
        if(ctx.performed)
            Dash();
    }

    public void OnForward(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
            Forward();
    }

    public void OnSmash(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
            Smash();
    }

    public void OnShield(InputAction.CallbackContext ctx)
    {
        pressingShieldBtn = ctx.ReadValueAsButton();
        Shield(ctx.ReadValueAsButton());
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
