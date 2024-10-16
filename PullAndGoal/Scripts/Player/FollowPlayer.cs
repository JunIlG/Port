using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;

    public void FixedUpdate()
    {
        float targetX = Mathf.Lerp(transform.position.x, playerTransform.position.x, Time.deltaTime * 8f);

        transform.position = new Vector3(targetX + 2.5f, transform.position.y, transform.position.z);
    }
}
