using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverPanel;        // 拖入 GameOver Panel

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        InnocentManager.Instance.onInnocenceChanged.AddListener(CheckGameOver);
    }

    void CheckGameOver(float ratio)
    {
        if (ratio <= 0f && !isGameOver)
            TriggerGameOver();
    }

    public void TriggerGameOver(bool isExplosion = false)
    {
        if (isGameOver) return;
        isGameOver = true;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = false;

        NPCController[] allNPCs = FindObjectsOfType<NPCController>();

        if (isExplosion)
        {
            // 宇宙大屁：全场所有 NPC
            foreach (NPCController npc in allNPCs)
                npc.OnBigFartExplosion();
        }
        else
        {
            // 普通 Game Over：周围范围内 NPC
            float range = 5f;
            foreach (NPCController npc in allNPCs)
            {
                if (Vector3.Distance(npc.transform.position, player.transform.position) <= range)
                    npc.OnBigFartExplosion();
            }
        }

        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        // 等待 NPC 动画和清白条反应
        yield return new WaitForSeconds(1.5f);

        gameOverPanel.SetActive(true);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}