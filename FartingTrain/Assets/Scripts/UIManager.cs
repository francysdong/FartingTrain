using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("蓄力条")]
    public Image chargeFillBar;
    public Gradient chargeGradient;

    [Header("清白值条")]
    public Image innocenceFillBar;
    public Gradient innocenceGradient;

    private PlayerController player;

    [Header("蓄屁条")]
    public Image gasFillBar;
    public Gradient gasGradient;            // 绿→黄→红

    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        InnocentManager.Instance.onInnocenceChanged.AddListener(UpdateInnocenceBar);
        GasManager.Instance.onGasChanged.AddListener(UpdateGasBar);  // 新增

        UpdateInnocenceBar(1f);
        UpdateGasBar(0f);                   // 初始蓄屁为空
    }

    void UpdateGasBar(float ratio)
    {
        gasFillBar.fillAmount = ratio;
        gasFillBar.color = gasGradient.Evaluate(ratio);
    }

    void Update()
    {
        if (player == null) return;
        UpdateChargeBar(player.ChargeRatio);
    }

    void UpdateChargeBar(float ratio)
    {
        chargeFillBar.fillAmount = ratio;
        chargeFillBar.color = chargeGradient.Evaluate(ratio);
    }

    void UpdateInnocenceBar(float ratio)
    {
        innocenceFillBar.fillAmount = ratio;
        innocenceFillBar.color = innocenceGradient.Evaluate(ratio);
    }
}