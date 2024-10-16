using UnityEngine;

public class PauseButton : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private GameObject PausePanel;
    
    public void OnPauseButtonClick()
    {
        PausePanel.SetActive(true);
        LevelManager.Instance.SetGameTimeScale(0.0f);
    }
}
