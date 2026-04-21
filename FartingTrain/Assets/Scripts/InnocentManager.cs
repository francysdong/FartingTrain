using UnityEngine;
using UnityEngine.Events;

public class InnocentManager : MonoBehaviour
{
    public static InnocentManager Instance { get; private set; }

    [Header("清白值设置")]
    public float maxInnocence = 100f;
    public float currentInnocence = 100f;

    [Header("每级反应扣除量")]
    public float smallDeduction = 5f;
    public float mediumDeduction = 15f;
    public float largeDeduction = 30f;

    public UnityEvent<float> onInnocenceChanged;   // UI 监听用

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Deduct(int reactionLevel)
    {
        float amount = reactionLevel switch
        {
            1 => smallDeduction,
            2 => mediumDeduction,
            3 => largeDeduction,
            _ => 0f
        };

        currentInnocence = Mathf.Clamp(currentInnocence - amount, 0f, maxInnocence);
        onInnocenceChanged?.Invoke(currentInnocence / maxInnocence);

        if (currentInnocence <= 0f)
            Debug.Log("清白值归零！");  // 之后可以在这里触发 Game Over
    }
}