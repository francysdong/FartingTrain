using UnityEngine;

public class FartEffect : MonoBehaviour
{
    [Header("生命周期")]
    public float lifetime = 2.5f;

    [Header("初速度")]
    public float launchSpeedMin = 2f;        // 喷出最小速度
    public float launchSpeedMax = 3.5f;      // 喷出最大速度
    public float launchAngleMin = 100f;      // 喷出角度范围（相对正右方，朝后上方）
    public float launchAngleMax = 150f;

    [Header("物理衰减")]
    public float drag = 2.5f;                // 速度衰减系数

    [Header("缩放")]
    public float maxScaleMultiplier = 2.5f;  // 最终膨胀到初始的几倍
    public float scaleInDuration = 0.2f;     // 初始弹出时间

    [Header("透明度")]
    public float fadeStartTime = 0.5f;       // 从生命周期几成开始淡出

    private float timer = 0f;
    private Vector3 initialScale;
    private SpriteRenderer sr;
    private Animator animator;
    private bool outTriggered = false;

    private Vector2 velocity;               // 当前速度

    void Start()
    {
        initialScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // 随机喷出方向（后上方扇形范围内）
        float angle = Random.Range(launchAngleMin, launchAngleMax);
        float rad = angle * Mathf.Deg2Rad;
        float speed = Random.Range(launchSpeedMin, launchSpeedMax);
        velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * speed;

        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        // === 速度衰减（模拟空气阻力）===
        velocity = Vector2.Lerp(velocity, Vector2.zero, drag * Time.deltaTime);
        transform.position += (Vector3)(velocity * Time.deltaTime);

        // === 缩放：弹出 → 持续膨胀 ===
        float burstScale = Mathf.Clamp01(timer / scaleInDuration);           // 0→1 快速弹出
        float growScale = Mathf.Lerp(1f, maxScaleMultiplier, progress);      // 1→max 持续膨胀
        transform.localScale = initialScale * burstScale * growScale;

        // === 缓慢自转（越来越慢）===
        transform.Rotate(0f, 0f, 20f * (1f - progress) * Time.deltaTime);

        // === 透明度淡出 ===
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
}