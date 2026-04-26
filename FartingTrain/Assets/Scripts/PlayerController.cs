using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [Header("移动设置")]
    public float moveSpeed = 5f;

    [Header("放屁设置")]
    public GameObject fartPrefab;
    public Vector2 fartOffset = new Vector2(-0.5f, 0f);

    [Header("蓄力设置")]
    public float maxChargeTime = 2f;

    private float chargeTimer = 0f;
    private bool isCharging = false;
    private bool isLocked = false;          // 移动锁定标志

    public float ChargeRatio => chargeTimer / maxChargeTime;
    public bool IsCharging => isCharging;

    [Header("蓄力震动")]
    public Transform spriteRoot;             // 拖入 Sprite 子物体
    public float shakeIntensity = 0.05f;

    [Header("放屁跳跃")]
    public float fartJumpForce = 5f;        // 最小跳跃力
    public float fartJumpMaxForce = 15f;    // 最大跳跃力

    [Header("地面检测")]
    public Transform groundCheck;           // 脚底空子物体
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.1f;

    private bool isGrounded = false;
    private bool waitingToLand = false;     // 放屁跳跃后等待落地

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        HandleMovement();
        HandleFart();
        HandleShake();
    }

    void HandleMovement()
    {
        if (isLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
            return;
        }

        float input = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(input * moveSpeed, rb.linearVelocity.y);
        animator.SetBool("IsWalking", input != 0);

        if (input > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (input < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void HandleLanding()
    {
        if (!waitingToLand) return;

        Debug.Log($"waitingToLand: true, isGrounded: {isGrounded}");

        if (isGrounded)
        {
            waitingToLand = false;
            isLocked = false;
            animator.SetBool("IsCharging", false);  // 落地后才关掉
        }
    }

    void HandleShake()
    {
        if (spriteRoot == null) return;

        if (!isCharging)
        {
            spriteRoot.localPosition = Vector3.zero;
            return;
        }

        float intensity = shakeIntensity * ChargeRatio;
        spriteRoot.localPosition = new Vector3(
            Random.Range(-intensity, intensity),
            Random.Range(-intensity, intensity),
            0f
        );
    }

    void HandleFart()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            chargeTimer = 0f;
            animator.SetBool("IsCharging", true);   // 开始蓄力动画
            animator.Play("Idle", 0, 0f);
        }

        if (isCharging)
            chargeTimer = Mathf.Clamp(chargeTimer + Time.deltaTime, 0f, maxChargeTime);

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // 删掉这行 animator.SetBool("IsCharging", false);
            animator.SetTrigger("Fart");
            SpawnFart(ChargeRatio);

            float jumpForce = Mathf.Lerp(fartJumpForce, fartJumpMaxForce, ChargeRatio);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            isCharging = false;
            chargeTimer = 0f;
            isLocked = true;
            StartCoroutine(UnlockAfterFart());
        }
    }

    System.Collections.IEnumerator UnlockAfterFart()
    {
        yield return null;
        waitingToLand = true;  // 不等动画，立刻开始检测落地
    }

    void SpawnFart(float chargeRatio)
    {
        if (fartPrefab == null) return;

        float direction = transform.localScale.x > 0 ? 1f : -1f;  // 正负对调
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(fartOffset.x * direction, fartOffset.y);

        GameObject fart = Instantiate(fartPrefab, spawnPos, Quaternion.identity);

        FartEffect effect = fart.GetComponent<FartEffect>();
        if (effect != null)
            effect.ApplyCharge(chargeRatio, direction);  // 传入 direction
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        Debug.Log($"碰撞到: {collision.gameObject.name}, layer: {collision.gameObject.layer}, waitingToLand: {waitingToLand}");
        if (!waitingToLand) return;

        

        // 检查是否是从上方落到地面
        if (((1 << collision.gameObject.layer) & groundLayer) == 0) return;

        // 确认是从上方碰撞（不是撞墙）
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                waitingToLand = false;
                isLocked = false;
                animator.SetBool("IsCharging", false);
                animator.Play("Idle", 0, 0f);
                return;
            }
        }
    }

}