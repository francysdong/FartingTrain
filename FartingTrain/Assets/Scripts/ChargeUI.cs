using UnityEngine;
using UnityEngine.UI;

public class ChargeUI : MonoBehaviour
{
    [Header("UI 引用")]
    public Image fillBar;
    public GameObject barRoot;

    [Header("颜色渐变")]
    public Gradient chargeGradient;

    private PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        // 删掉了 barRoot.SetActive(false)
    }

    void Update()
    {
        if (player == null) return;

        float ratio = player.ChargeRatio;

        fillBar.fillAmount = ratio;
        fillBar.color = chargeGradient.Evaluate(ratio);
        // 删掉了 isCharging 判断和 barRoot.SetActive()
    }
}