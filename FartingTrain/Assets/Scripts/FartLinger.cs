using UnityEngine;

public class FartLinger : MonoBehaviour
{
    public float lingerDuration = 2f;
    public float currentSize = 1f;

    private float timer = 0f;
    private CircleCollider2D col;

    void Awake()
    {
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    public void Setup(float size)
    {
        currentSize = size;
        col.radius = size * 0.5f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lingerDuration;

        // 大小随时间衰减
        currentSize = Mathf.Lerp(currentSize, 0f, progress);
        col.radius = currentSize * 0.5f;

        if (timer >= lingerDuration)
            Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        NPCController npc = other.GetComponent<NPCController>();
        if (npc == null) return;

        npc.OnFartContact(currentSize);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        NPCController npc = other.GetComponent<NPCController>();
        if (npc == null) return;
        npc.OnFartLeave();
    }
}