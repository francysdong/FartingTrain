using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float mediumSpeedMultiplier = 0.7f;  // 中等状态速度倍率
    public float hardSpeedMultiplier = 0.4f;  // 憋不住速度倍率

    [Header("放屁参数")]
    public GameObject fartPrefab;
    public Vector2 fartOffset = new Vector2(-0.5f, 0f);

    [Header("蓄气参数")]
    public float maxChargeTime = 2f;

    [Header("气量阈值")]
    public float mediumThreshold = 0.4f;  // 超过此值进入 Medium
    public float hardThreshold = 0.7f;  // 超过此值进入 Hard

    private float chargeTimer = 0f;
    private bool isCharging = false;
    private bool isLocked = false;
    public float ChargeRatio => chargeTimer / maxChargeTime;
    public bool IsCharging => isCharging;

    [Header("震动")]
    public Transform spriteRoot;
    public float shakeIntensity = 0.05f;

    [Header("放屁跳跃")]
    public float fartJumpForce = 5f;
    public float fartJumpMaxForce = 15f;

    [Header("落地检测")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.1f;

    private bool isGrounded = false;
    private bool waitingToLand = false;
    private bool isFarting = false;

    private bool fartButtonDown = false;
    private bool fartButtonUp = false;

    // ─── 状态枚举 ────────────────────────────────────────────
    enum GasState { Normal, Medium, Hard }
    GasState currentGasState = GasState.Normal;

    // ─── 动画名常量 ───────────────────────────────────────────
    // Normal 用原有 Animator 参数控制，不需要常量
    const string ANIM_NORMAL_IDLE = "shizu_idle";
    const string ANIM_NORMAL_WALK = "shizu_walk";
    const string ANIM_NORMAL_FART = "shizu_fart";
    const string ANIM_NORMAL_HOLD = "shizu_hold";

    const string ANIM_MEDIUM_IDLE = "shizu_idle_medium";
    const string ANIM_MEDIUM_WALK = "shizu_walk_medium";
    const string ANIM_MEDIUM_FART = "shizu_fart_medium";

    const string ANIM_HARD_IDLE = "shizu_idle_hard";
    const string ANIM_HARD_WALK = "shizu_walk_hard";
    const string ANIM_HARD_FART = "shizu_fart_hard";

    public static PlayerController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        UpdateGasState();

        if (Input.GetKeyDown(KeyCode.Space) && !isCharging)
        {
            isCharging = true;
            chargeTimer = 0f;
            animator.Play(ANIM_NORMAL_HOLD, 0, 0f);
        }

        HandleMovement();
        HandleFart();
        HandleShake();
    }

    void UpdateGasState()
    {
        if (GasManager.Instance == null) return;

        float gasRatio = GasManager.Instance.currentGas / GasManager.Instance.maxGas;
        GasState newState;

        if (gasRatio >= hardThreshold) newState = GasState.Hard;
        else if (gasRatio >= mediumThreshold) newState = GasState.Medium;
        else newState = GasState.Normal;

        if (newState != currentGasState)
        {
            currentGasState = newState;
            animator.SetInteger("GasState", (int)newState); // 同步给 Animator
            if (!isFarting && !isCharging)
                PlayIdleForCurrentState();
        }
    }

    void HandleMovement()
    {
        float input = Input.GetAxisRaw("Horizontal");

        // 朝向
        if (input > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (input < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // 速度
        if (isFarting)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            float speed = currentGasState switch
            {
                GasState.Medium => moveSpeed * mediumSpeedMultiplier,
                GasState.Hard => moveSpeed * hardSpeedMultiplier,
                _ => moveSpeed
            };
            rb.linearVelocity = new Vector2(input * speed, rb.linearVelocity.y);
        }

        // 动画（蓄气和放屁时不切换）
        if (isCharging || isFarting) return;

        if (input != 0)
            PlayWalkForCurrentState();
        else
            PlayIdleForCurrentState();
    }

    void HandleFart()
    {
        bool pressDown = Input.GetKeyDown(KeyCode.Space) || fartButtonDown;
        bool pressUp = Input.GetKeyUp(KeyCode.Space) || fartButtonUp;

        fartButtonDown = false;
        fartButtonUp = false;

        if (pressDown && !isCharging)
        {
            isCharging = true;
            chargeTimer = 0f;
            animator.Play(ANIM_NORMAL_HOLD, 0, 0f);
        }

        if (isCharging)
            chargeTimer = Mathf.Clamp(chargeTimer + Time.deltaTime, 0f, maxChargeTime);

        if (pressUp && isCharging)
        {
            float ratio = ChargeRatio;
            PlayFartForCurrentState();
            SpawnFart(ratio);
            FartSoundManager.Instance?.PlayFartSound(ratio);

            if (currentGasState != GasState.Hard)
            {
                float jumpForce = Mathf.Lerp(fartJumpForce, fartJumpMaxForce, ratio);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                isCharging = false;
                chargeTimer = 0f;
                isFarting = true;
                waitingToLand = true;
            }
            else
            {
                isCharging = false;
                chargeTimer = 0f;
                isFarting = true;
                StartCoroutine(HardFartFinish());
            }
        }
    }

    void HandleShake()
    {
        if (spriteRoot == null) return;
        if (!isCharging) { spriteRoot.localPosition = Vector3.zero; return; }

        float intensity = shakeIntensity * ChargeRatio;
        spriteRoot.localPosition = new Vector3(
            Random.Range(-intensity, intensity),
            Random.Range(-intensity, intensity),
            0f
        );
    }

    void SpawnFart(float chargeRatio)
    {
        if (fartPrefab == null) return;

        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(fartOffset.x * direction, fartOffset.y);

        GameObject fart = Instantiate(fartPrefab, spawnPos, Quaternion.identity);
        FartEffect effect = fart.GetComponent<FartEffect>();
        if (effect != null) effect.ApplyCharge(chargeRatio, direction);

        float deduction = GasManager.Instance.maxGas * chargeRatio * GasManager.Instance.maxFartDeductionRatio;
        GasManager.Instance.DeductGas(deduction);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!waitingToLand) return;
        if (((1 << collision.gameObject.layer) & groundLayer) == 0) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                waitingToLand = false;
                isFarting = false;

                if (isCharging)
                    animator.Play(ANIM_NORMAL_HOLD, 0, 0f);
                else
                    PlayIdleForCurrentState();

                return;
            }
        }
    }

    IEnumerator HardFartFinish()
    {
        yield return null;  // 等 Play 指令生效
        yield return null;  // 再等一帧确保已切换到 fart 动画
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);
        isFarting = false;
        PlayIdleForCurrentState();
    }

    public void OnFartButtonDown() => fartButtonDown = true;
    public void OnFartButtonUp() => fartButtonUp = true;

    // ─── 动画工具方法 ─────────────────────────────────────────
    void PlayIdleForCurrentState()
    {
        switch (currentGasState)
        {
            
            case GasState.Normal:
                if (!IsPlaying(ANIM_NORMAL_IDLE)) animator.Play(ANIM_NORMAL_IDLE, 0, 0f);
                break;
            case GasState.Medium:
                if (!IsPlaying(ANIM_MEDIUM_IDLE)) animator.Play(ANIM_MEDIUM_IDLE, 0, 0f);
                break;
            case GasState.Hard:
                if (!IsPlaying(ANIM_HARD_IDLE)) animator.Play(ANIM_HARD_IDLE, 0, 0f);
                break;
        }
    }

    void PlayWalkForCurrentState()
    {
        switch (currentGasState)
        {
            case GasState.Normal:
                if (!IsPlaying(ANIM_NORMAL_WALK)) animator.Play(ANIM_NORMAL_WALK, 0, 0f);
                break;
            case GasState.Medium:
                if (!IsPlaying(ANIM_MEDIUM_WALK)) animator.Play(ANIM_MEDIUM_WALK, 0, 0f);
                break;
            case GasState.Hard:
                if (!IsPlaying(ANIM_HARD_WALK)) animator.Play(ANIM_HARD_WALK, 0, 0f);
                break;
        }
    }

    void PlayFartForCurrentState()
    {
        switch (currentGasState)
        {
            case GasState.Normal:
                if (!IsPlaying(ANIM_NORMAL_FART)) animator.Play(ANIM_NORMAL_FART, 0, 0f); 
                break;
            case GasState.Medium:
                if (!IsPlaying(ANIM_MEDIUM_FART)) animator.Play(ANIM_MEDIUM_FART, 0, 0f);
                break;
            case GasState.Hard:
                if (!IsPlaying(ANIM_HARD_FART)) animator.Play(ANIM_HARD_FART, 0, 0f);
                break;
        }
    }

    bool IsPlaying(string animName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animName);
    }
}