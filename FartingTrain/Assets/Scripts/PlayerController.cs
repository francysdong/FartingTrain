using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    [Header("放屁设置")]
    public GameObject fartPrefab;          // 拖入屁的 Prefab
    public Vector2 fartOffset = new Vector2(-0.5f, 0f); // 生成位置偏移（角色身后）

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Fart");
            SpawnFart();
        }
    }

    void SpawnFart()
    {
        if (fartPrefab == null) return;

        // 根据角色朝向决定偏移方向
        float direction = transform.localScale.x > 0 ? -1f : 1f;
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(fartOffset.x * direction, fartOffset.y);

        GameObject fart = Instantiate(fartPrefab, spawnPos, Quaternion.identity);

        // 如果屁有自动销毁脚本就不需要下面这行
        // Destroy(fart, 2f);
    }
}