using System.Collections;
using UnityEngine;

public class TeleportSystem : MonoBehaviour 
{
    public Transform aLocation, bLocation;
    public SpriteRenderer aSprite, bSprite;
    public LayerMask playerLayer;
    public Color portalColor;

    public float teleportRadius = 0.5f;
    public float teleportCooldown = 1.0f;

    private bool canTeleport = true;

    private void Start()
    {
        aSprite = aLocation.GetComponent<SpriteRenderer>();
        bSprite = bLocation.GetComponent<SpriteRenderer>();

        aSprite.color = portalColor;
        bSprite.color = portalColor;
    }

    private void FixedUpdate() 
    {
        CheckColliderAndTeleportPlayer(); 
    }
    
    void CheckColliderAndTeleportPlayer()
    {
        if (canTeleport == false) return;

        bool teleprotPlayer = false;

        Collider2D aHitCollider = Physics2D.OverlapCircle(aLocation.position, teleportRadius, playerLayer);
        if (aHitCollider != null)
        {
            Vector3 playerALocationGap = aLocation.position - aHitCollider.transform.position;

            aHitCollider.transform.position = bLocation.position - playerALocationGap;
            teleprotPlayer = true;
        }

        Collider2D bHitCollider = Physics2D.OverlapCircle(bLocation.position, teleportRadius, playerLayer);
        if (bHitCollider != null)
        {
            Vector3 playerBLocationGap = bLocation.position - bHitCollider.transform.position;

            bHitCollider.transform.position = aLocation.position - playerBLocationGap;
            teleprotPlayer = true;
        }

        if (teleprotPlayer == true)
        {
            canTeleport = false;

            Color disableColor = portalColor;
            disableColor.a = 0.2f;

            aSprite.color = disableColor;
            bSprite.color = disableColor;

            StartCoroutine("EnableTeleport");
        }
    }

    IEnumerator EnableTeleport()
    {
        yield return new WaitForSeconds(teleportCooldown);

        aSprite.color = portalColor;
        bSprite.color = portalColor;
        
        canTeleport = true;
    }

    // aLocation과 bLocation에 원을 디버깅하고 싶어
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aLocation.position, teleportRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(bLocation.position, teleportRadius);
    }
}

