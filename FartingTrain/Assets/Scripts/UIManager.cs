using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("蓄力条")]
    public Image chargeFillBar;
    public Gradient chargeGradient;

    [Header("社死头像")]
    public Image avatarBackground;
    public Image avatarBackground2;
    public Image avatarFill;

    [Header("高社死值 (正常)")]
    public Sprite bgNormal;
    public Sprite bg2Normal;
    public Sprite fillNormal;

    [Header("中社死值")]
    public Sprite bgWorried;
    public Sprite bg2Worried;
    public Sprite fillWorried;

    [Header("低社死值")]
    public Sprite bgDead;
    public Sprite bg2Dead;
    public Sprite fillDead;

    [Header("切换阈值")]
    public float worriedThreshold = 0.6f;
    public float deadThreshold = 0.3f;

    [Header("蓄屁条")]
    public Image gasFillBar;
    public Gradient gasGradient;

    [Header("行驶进度指针")]
    public RectTransform progressPointer;    // 拖入指针的 RectTransform
    public float pointerMinX = -300f;        // 最左边的位置（起点）
    public float pointerMaxX = 300f;         // 最右边的位置（终点）

    [Header("放屁按钮")]
    public RectTransform fartButton;
    public float buttonPressScale = 0.85f;

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
        // fill 水位
        avatarFill.fillAmount = ratio;

        // 图片切换
        Sprite bg, bg2, fill;
        if (ratio < deadThreshold)
        {
            bg = bgDead; bg2 = bg2Dead; fill = fillDead;
        }
        else if (ratio < worriedThreshold)
        {
            bg = bgWorried; bg2 = bg2Worried; fill = fillWorried;
        }
        else
        {
            bg = bgNormal; bg2 = bg2Normal; fill = fillNormal;
        }

        avatarBackground.sprite = bg;
        avatarBackground2.sprite = bg2;
        avatarFill.sprite = fill;
    }

    void UpdateGasBar(float ratio)
    {
        gasFillBar.fillAmount = ratio;
        gasFillBar.color = gasGradient.Evaluate(ratio);
    }



    public void OnFartButtonDown()
    {
        PlayerController.Instance?.OnFartButtonDown();
        StartCoroutine(ScaleButton(buttonPressScale, 0.08f));
    }

    public void OnFartButtonUp()
    {
        PlayerController.Instance?.OnFartButtonUp();
        StartCoroutine(ScaleButton(1f, 0.12f));
    }

    IEnumerator ScaleButton(float targetScale, float duration)
    {
        Vector3 startScale = fartButton.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fartButton.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }

        fartButton.localScale = endScale;
    }
}