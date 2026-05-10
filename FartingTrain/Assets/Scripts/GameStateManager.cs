using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject winPanel;

    private bool isGameOver = false;
    private bool isWin = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);

        InnocentManager.Instance.onInnocenceChanged.AddListener(CheckGameOver);
        TrainManager.Instance.onArrived.AddListener(TriggerWin);
    }

    void CheckGameOver(float ratio)
    {
        if (ratio <= 0f && !isGameOver)
            TriggerGameOver();
    }

    public void TriggerGameOver(bool isExplosion = false)
    {
        if (isGameOver || isWin) return;
        isGameOver = true;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = false;

        NPCController[] allNPCs = FindObjectsOfType<NPCController>();

        if (isExplosion)
        {
            foreach (NPCController npc in allNPCs)
                npc.OnBigFartExplosion();
        }
        else
        {
            float range = 5f;
            foreach (NPCController npc in allNPCs)
            {
                if (player != null && Vector3.Distance(npc.transform.position, player.transform.position) <= range)
                    npc.OnBigFartExplosion();
            }
        }

        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1.5f);
        gameOverPanel.SetActive(true);
    }

    public void TriggerWin()
    {
        if (isGameOver || isWin) return;
        isWin = true;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = false;

        TrainManager.Instance.StopTrain();
        winPanel.SetActive(true);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToLevelSelect()
    {
        SceneLoader.Instance.LoadLevelSelect();
    }

    public void NextLevel()
    {
        SceneLoader.Instance.LoadLevelSelect();
    }
}