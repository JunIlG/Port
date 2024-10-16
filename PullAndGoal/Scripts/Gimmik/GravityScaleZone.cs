using System.Collections.Generic;
using UnityEngine;

public class GravityScaleZone : MonoBehaviour
{
    public float toGravityScale = 1f; // Set this value to your desired gravity scale.
    private float originalGravityScale = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        originalGravityScale = rb.gravityScale;
        rb.gravityScale = toGravityScale;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        rb.gravityScale = originalGravityScale;
    }

}
