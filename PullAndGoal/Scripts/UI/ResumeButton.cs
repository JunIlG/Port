using UnityEngine;

public class ResumeButton : MonoBehaviour
{

    [Header("View")]
    [SerializeField] private GameObject PausePanel;
    public void OnResumeButtonClick()
    {
        PausePanel.SetActive(false);
        LevelManager.Instance.SetGameTimeScale(1.0f);
    }

}
