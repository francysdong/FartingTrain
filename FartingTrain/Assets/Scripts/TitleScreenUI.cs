using UnityEngine;

public class TitleScreenUI : MonoBehaviour
{
    public void OnStartPressed()
    {
        SceneLoader.Instance.LoadLevelSelect();
    }
}