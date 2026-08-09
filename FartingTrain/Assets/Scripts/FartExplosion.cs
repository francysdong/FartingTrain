using UnityEngine;

public class FartExplosion : MonoBehaviour
{
    [Header("宇宙大屁设置")]
    public GameObject explosionFartPrefab;  // 可以用大一点的 FartEffect Prefab
    public float explosionSize = 10f;       // 覆盖全屏的大小

    void Start()
    {
        GasManager.Instance.onGasExplosion.AddListener(Explode);
    }

    void Explode()
    {
        NPCController[] allNPCs = FindObjectsOfType<NPCController>();
        foreach (NPCController npc in allNPCs)
            npc.OnBigFartExplosion();

        if (explosionFartPrefab != null)
        {
            GameObject explosion = Instantiate(explosionFartPrefab,
                                               FindObjectOfType<PlayerController>().transform.position,
                                               Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionSize;
        }

        // 直接清空清白值
        InnocentManager.Instance.SetInnocenceToZero();
        GameStateManager.Instance.TriggerGameOver(true);  // 宇宙大屁

        GasManager.Instance.ResetExplosion();
    }
}