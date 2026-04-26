using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("关卡按钮")]
    public Button[] levelButtons;
    public int unlockedLevels = 1;

    void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1;
            bool isUnlocked = levelIndex <= unlockedLevels;

            levelButtons[i].interactable = isUnlocked;

            // 只改锁定的按钮颜色，解锁的保持原样
            if (!isUnlocked)
            {
                TextMeshProUGUI buttonText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.color = Color.gray;
            }

            levelButtons[i].onClick.AddListener(() => SceneLoader.Instance.LoadLevel(levelIndex));
        }
    }

    public void OnBackPressed()
    {
        SceneLoader.Instance.LoadTitle();
    }
}