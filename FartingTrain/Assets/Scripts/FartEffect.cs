using UnityEngine;

public class FartEffect : MonoBehaviour
{
    [Header("生命周期")]
    public float lifetime = 2.5f;

    [Header("初速度")]
    public float launchSpeedMin = 2f;
    public float launchSpeedMax = 3.5f;
    public float launchAngleMin = 100f;
    public float launchAngleMax = 150f;

    [Header("物理衰减")]
    public float drag = 2.5f;

    [Header("缩放")]
    public float maxScaleMultiplier = 2.5f;
    public float scaleInDuration = 0.2f;

    [Header("透明度")]
    public float fadeStartTime = 0.5f;

    [Header("蓄力范围")]
    public float chargeMinScale = 0.5f;
    public float chargeMaxScale = 2f;
    public float chargeMinSpeed = 0.5f;
    public float chargeMaxSpeed = 2f;

    [Header("NPC检测")]
    public LayerMask npcLayer;

    private float timer = 0f;
    private Vector3 initialScale;
    private SpriteRenderer sr;
    private Animator animator;
    private bool outTriggered = false;
    private Vector2 velocity;
    private NPCController lastHitNPC = null;

    public float CurrentSize => transform.localScale.x;

    void Awake()
    {
        initialScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        InitVelocity(1f, 1f);
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    public void ApplyCharge(float chargeRatio, float facingDirection)
    {
        float scaleMult = Mathf.Lerp(chargeMinScale, chargeMaxScale, chargeRatio);
        float speedMult = Mathf.Lerp(chargeMinSpeed, chargeMaxSpeed, chargeRatio);

        transform.localScale = initialScale * scaleMult;
        initialScale = transform.localScale;

        InitVelocity(speedMult, facingDirection);
    }

    void InitVelocity(float speedMult, float facingDirection)
    {
        float angle = Random.Range(launchAngleMin, launchAngleMax);
        float rad = angle * Mathf.Deg2Rad;
        float speed = Random.Range(launchSpeedMin, launchSpeedMax) * speedMult;

        velocity = new Vector2(Mathf.Cos(rad) * facingDirection, Mathf.Sin(rad)) * speed;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        // 速度衰减
        velocity = Vector2.Lerp(velocity, Vector2.zero, drag * Time.deltaTime);
        transform.position += (Vector3)(velocity * Time.deltaTime);

        // 缩放
        float burstScale = Mathf.Clamp01(timer / scaleInDuration);
        float growScale = Mathf.Lerp(1f, maxScaleMultiplier, progress);
        transform.localScale = initialScale * burstScale * growScale;

        // 自转
        transform.Rotate(0f, 0f, 20f * (1f - progress) * Time.deltaTime);

        // 主动检测 NPC
        CheckNPCContact();

        // 透明度淡出
        if (sr != null)
        {
            float fadeProgress = Mathf.InverseLerp(fadeStartTime, 1f, progress);
            Color c = sr.color;
            c.a = 1f - fadeProgress;
            sr.color = c;

            if (progress >= fadeStartTime && !outTriggered)
            {
                if (animator != null)
                    animator.SetTrigger("Out");
                outTriggered = true;
            }

            if (c.a <= 0f)
                Destroy(gameObject);
        }
    }

    void CheckNPCContact()
    {
        // 去掉 timer < scaleInDuration 的等待
        float radius = transform.localScale.x * 0.5f;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, npcLayer);

        if (hit != null)
        {
            NPCController npc = hit.GetComponent<NPCController>();
            if (npc != null)
            {
                lastHitNPC = npc;
                npc.OnFartContact(CurrentSize);
            }
        }
        else
        {
            if (lastHitNPC != null)
            {
                lastHitNPC.OnFartLeave();
                lastHitNPC = null;
            }
        }
    }

    void OnDestroy()
    {
        if (lastHitNPC != null)
            lastHitNPC.OnFartLeave();
    }
}