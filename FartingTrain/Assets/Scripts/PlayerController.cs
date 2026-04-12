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

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleFart();
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

    void HandleFart()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            // 删掉了 isLocked = true
            chargeTimer = 0f;
        }

        if (isCharging)
            chargeTimer = Mathf.Clamp(chargeTimer + Time.deltaTime, 0f, maxChargeTime);

        if (Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetTrigger("Fart");
            SpawnFart(ChargeRatio);
            isCharging = false;
            chargeTimer = 0f;
            isLocked = true;                    // 松开时才锁定
            StartCoroutine(UnlockAfterFart());
        }
    }

    System.Collections.IEnumerator UnlockAfterFart()
    {
        // 等一帧让 Animator 切换到 Fart 状态
        yield return null;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float waitTime = state.length;
        yield return new WaitForSeconds(waitTime);

        isLocked = false;
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
}