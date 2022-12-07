using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float verticalAxis;
    float horizontalAxis;
    [SerializeField]
    float speed = 0.2f;

    Rigidbody rb;

    private void Start()
    {      
        rb = GetComponent<Rigidbody>();
        if (rb)
            rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        verticalAxis = Input.GetAxis("Vertical");
        horizontalAxis = Input.GetAxis("Horizontal");

        transform.Translate(Vector3.forward * verticalAxis * speed * 30 * Time.deltaTime);
        transform.Translate(Vector3.right * horizontalAxis * speed * 30 * Time.deltaTime);

        if (verticalAxis == 0 && horizontalAxis == 0)
        {
            rb.velocity = new Vector3(0, 0, 0);
        }
    }
}
