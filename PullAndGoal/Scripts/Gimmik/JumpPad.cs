using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private Vector2 jumpPower;
    [SerializeField] private bool setVelocity = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerState playerState = collision.GetComponent<PlayerState>();
            if (playerState.lastJumpPad == this) return;

            playerState.lastJumpPad = this;

            if (setVelocity)
            {
                collision.GetComponent<Rigidbody2D>().linearVelocity = (transform.rotation * jumpPower);
            }
            else
            {
                collision.GetComponent<Rigidbody2D>().AddForce(transform.rotation * jumpPower);
            }
        }
    }
}
