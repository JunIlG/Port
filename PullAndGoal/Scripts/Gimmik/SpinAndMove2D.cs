using UnityEngine;

public class SpinAndMove2D : MonoBehaviour
{
    public float rotationSpeed = 100f;

    public float travelTime = 2f;

    private Vector2 startPosition;
    public Vector2 localEndPosition;

    private bool movingToEnd = true;
    private float elapsedTime = 0f;

    void Start()
    {
        startPosition = transform.position;
        localEndPosition += startPosition;
    }

    void FixedUpdate()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        elapsedTime += Time.fixedDeltaTime;

        if (travelTime == 0f)
            return;

        float t = elapsedTime / travelTime;

        if (movingToEnd)
        {
            transform.position = Vector2.Lerp(startPosition, localEndPosition, t);
        }
        else
        {
            transform.position = Vector2.Lerp(localEndPosition, startPosition, t);
        }

        if (t >= 1f)
        {
            movingToEnd = !movingToEnd;
            elapsedTime = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.collider.CompareTag("Player"))
    //    {
    //        collision.transform.SetParent(null);
    //    }
    //}

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + localEndPosition);
    }
}
