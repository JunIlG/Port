using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Floater : MonoBehaviour
{
    // refereces
    private Rigidbody rb;
    public Wave wave;
    
    // physics properties
    public float depthBefSub;
    public float displacementAmt;
    public float waterDrag;
    public float waterAngularDrag;


    public Transform[] effectors;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() 
    {
        foreach(Transform effector in effectors)
        {
            rb.AddForceAtPosition(Physics.gravity / effectors.Length, effector.position, ForceMode.Acceleration);

            float height = wave.GetHeight(effector.position);

            if (effector.position.y < height)
            {
                float displacementMult = Mathf.Clamp01(height - effector.position.y / depthBefSub) * displacementAmt;
                rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMult, 0f), effector.position, ForceMode.Acceleration);
                rb.AddForce(displacementMult * -rb.linearVelocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                rb.AddTorque(displacementMult * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    private void OnDrawGizmosSelected() 
    {
        if (effectors == null) return;

        Gizmos.color = Color.red;
        foreach (Transform effector in effectors)
        {
            Gizmos.DrawSphere(effector.position, 0.1f);
        }
    }
}
