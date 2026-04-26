using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // ¿ç³¡¾°±£Áô
        }
        else Destroy(gameObject);
    }

    public void LoadTitle()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    public void LoadLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene("Level_" + levelIndex.ToString("00"));
    }
}
