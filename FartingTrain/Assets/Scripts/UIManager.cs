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

    [Header("蓄屁条")]
    public Image gasFillBar;
    public Gradient gasGradient;

    [Header("行驶进度指针")]
    public RectTransform progressPointer;    // 拖入指针的 RectTransform
    public float pointerMinX = -300f;        // 最左边的位置（起点）
    public float pointerMaxX = 300f;         // 最右边的位置（终点）

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
        GasManager.Instance.onGasChanged.AddListener(UpdateGasBar);

        UpdateInnocenceBar(1f);
        UpdateGasBar(0f);
    }

    void Update()
    {
        if (player != null)
            UpdateChargeBar(player.ChargeRatio);

        if (TrainManager.Instance != null && progressPointer != null)
        {
            float progress = TrainManager.Instance.Progress;
            float targetX = Mathf.Lerp(pointerMinX, pointerMaxX, progress);
            progressPointer.anchoredPosition = new Vector2(targetX, progressPointer.anchoredPosition.y);
        }
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

    void UpdateGasBar(float ratio)
    {
        gasFillBar.fillAmount = ratio;
        gasFillBar.color = gasGradient.Evaluate(ratio);
    }
}