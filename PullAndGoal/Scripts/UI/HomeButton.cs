using UnityEngine;

public class HomeButton : MonoBehaviour
{
    public void OnHomeButtonClick()
    {
        LevelManager.Instance.GoHome();

        // Play sound
    }
}
