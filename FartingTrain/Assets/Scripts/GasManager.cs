using UnityEngine;
using UnityEngine.Events;

public class GasManager : MonoBehaviour
{
    public static GasManager Instance { get; private set; }

    [Header("蓄屁设置")]
    public float maxGas = 100f;
    public float currentGas = 0f;
    public float gasIncreaseRate = 5f;      // 每秒增加量
    public float fartDeduction = 30f;       // 放屁减少量
    public float bigFartDeduction = 80f;    // 大屁减少量

    [Header("宇宙大屁阈值")]
    public float explosionThreshold = 100f;

    public UnityEvent<float> onGasChanged;
    public UnityEvent onGasExplosion;       // 触发宇宙大屁

    [Header("放屁减少比例")]
    public float maxFartDeductionRatio = 0.4f;  // 满蓄力最多减少几成

    private bool hasExploded = false;       // 防止连续触发

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (hasExploded) return;

        currentGas = Mathf.Clamp(currentGas + gasIncreaseRate * Time.deltaTime, 0f, maxGas);
        onGasChanged?.Invoke(currentGas / maxGas);

        if (currentGas >= explosionThreshold)
            TriggerExplosion();
    }

    public void DeductGas(float amount)
    {
        currentGas = Mathf.Clamp(currentGas - amount, 0f, maxGas);
        onGasChanged?.Invoke(currentGas / maxGas);
    }

    void TriggerExplosion()
    {
        hasExploded = true;
        currentGas = 0f;
        onGasChanged?.Invoke(0f);
        onGasExplosion?.Invoke();           // 通知 FartExplosion 触发
    }

    public void ResetExplosion()
    {
        hasExploded = false;                // 宇宙大屁结束后重置
    }
}