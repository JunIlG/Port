using System;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public bool dead = false;

    [Header("View")]
    [SerializeField] private GameObject explosionPrefab;

    [HideInInspector] public JumpPad lastJumpPad = null;

    public event Action OnGetLaunchPack;

    [Header("Jump")]
    [SerializeField] private PhysicsMaterial2D defaultMaterial;
    [SerializeField] private PhysicsMaterial2D noBounceMaterial;

    [Header("Check Ground")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask noneBounceGroundLayerMask;
    public float crushThreshold = 1f;
    public event Action OnLanded;
    public event Action OnTakenOff;
    public bool _isGrounded;
    public bool isGrounded
    {
        get { return _isGrounded; }
        set 
        {
            if (isGrounded == value) return;

            _isGrounded = value;

            if (isGrounded)
                OnLanded();
            else
                OnTakenOff();
        }
    }


    private void Start()
    {
        OnLanded += () => lastJumpPad = null;
        OnTakenOff += () => transform.SetParent(null);
    }

    private void Update()
    {
        isGrounded = IsGrounded();

        if (IsNoneBounceGrounded())
        {
            GetComponent<Rigidbody2D>().sharedMaterial = noBounceMaterial;
        }
        else
        {
            GetComponent<Rigidbody2D>().sharedMaterial = defaultMaterial;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Vector2 force = Vector2.zero;

            foreach (ContactPoint2D cp in collision.contacts)
            {
                force += cp.normal * cp.separation;
            }

            if (force.magnitude > crushThreshold)
            {
                Die();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeathZone"))
        {
            Die();
        }
        else if (collision.CompareTag("Star"))
        {
            GetStar(collision.gameObject);
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down * groundCheckDistance));
        Gizmos.DrawLine(transform.position, transform.position + ((Vector3.down + Vector3.left).normalized * groundCheckDistance));
        Gizmos.DrawLine(transform.position, transform.position + ((Vector3.down + Vector3.right).normalized * groundCheckDistance));
    }

    #region CheckGround

    private bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayerMask | noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + Vector2.left, groundCheckDistance, groundLayerMask | noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + Vector2.right, groundCheckDistance, groundLayerMask | noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + (Vector2.left / 2f), groundCheckDistance, groundLayerMask | noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + (Vector2.right / 2f), groundCheckDistance, groundLayerMask | noneBounceGroundLayerMask);
    }

    private bool IsNoneBounceGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance + 0.35f, noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + Vector2.left, groundCheckDistance + 0.35f, noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + Vector2.right, groundCheckDistance + 0.35f, noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + (Vector2.left / 2f), groundCheckDistance + 0.35f, noneBounceGroundLayerMask) ||
               Physics2D.Raycast(transform.position, Vector2.down + (Vector2.right / 2f), groundCheckDistance + 0.35f, noneBounceGroundLayerMask);
    }

    #endregion

    #region DeathZoneCheck

    private void Die()
    {
        if (dead) return;
        dead = true;

        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GetComponent<SpriteRenderer>().enabled = false;

        StartCoroutine(LevelManager.Instance.GameOver());
    }

    #endregion

    #region StarCheck

    private void GetStar(GameObject star)
    {
        LevelManager.Instance.GetStar();
        AudioManager.Instance.PlaySFX("Star");
        star.SetActive(false);
    }

    public void GetAdditionalLaunchPack()
    {
        // OnGetLaunchPack을 호출한다
        OnGetLaunchPack?.Invoke();

    }

    #endregion
}
