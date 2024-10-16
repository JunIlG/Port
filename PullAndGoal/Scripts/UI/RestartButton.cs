using UnityEngine;

public class RestartButton : MonoBehaviour
{
    public void OnRestartButtonClick()
    {
        LevelManager.Instance.RestartLevel();
    }
}
