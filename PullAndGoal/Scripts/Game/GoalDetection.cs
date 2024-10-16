using Unity.VisualScripting;
using UnityEngine;

public class GoalDetection : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LevelClear();
        }
    }

    private void LevelClear()
    {
        AudioManager.Instance.PlaySFX("Goal");
        LevelManager.Instance.ClearLevel();
    }
}
