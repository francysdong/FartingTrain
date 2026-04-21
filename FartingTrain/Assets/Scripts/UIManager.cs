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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        InnocentManager.Instance.onInnocenceChanged.AddListener(UpdateInnocenceBar);

        // 初始化清白值条
        UpdateInnocenceBar(1f);
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