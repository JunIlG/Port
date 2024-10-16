using UnityEngine;

public class CharacterPullComponent : MonoBehaviour
{
    public float maxDistance = 5.0f;
    public float pullStrength = 10.0f;

    private Rigidbody2D rb;
    private Transform target;
    private LineRenderer lineRenderer;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        rb = target.GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
        Vector3 directionToPull = transform.position - target.position;
        float distance = directionToPull.magnitude;

        // 최대 거리 이상 멀어지면
        if (distance > maxDistance)
        {
            Vector3 pullForce = directionToPull.normalized * (distance / maxDistance) * pullStrength;

            // debug pullforce
            Debug.DrawRay(transform.position, pullForce, Color.red);

            rb.AddForce(pullForce);
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, target.position);
    }
}
