using System.Collections;
using UnityEngine;

public class NextLevelButton : MonoBehaviour
{
    public void OnNextLevelButtonClick()
    {
        LevelManager.Instance.NextLevel();
    }
}
