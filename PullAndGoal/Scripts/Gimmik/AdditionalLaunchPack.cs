using UnityEngine;

public class AdditionalLaunchPack : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool canAdditionalLaunch = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (canAdditionalLaunch == false)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponent<PlayerState>();

            if (playerState != null)
            {
                playerState.GetAdditionalLaunchPack();

                Invoke("ReactivateGameObject", 5f);
                
                spriteRenderer.enabled = false;
                canAdditionalLaunch = false;
            }
        }
    }

    void ReactivateGameObject()
    {
        spriteRenderer.enabled = true;
        canAdditionalLaunch = true;
    }
}

