using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionExit(UnityEngine.Collision collision)
    {
        rb.velocity = new Vector3(0,0,0);
    }
}
