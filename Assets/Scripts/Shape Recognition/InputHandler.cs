using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    MouseLook mouseLook;
    private void Start()
    {
        mouseLook = FindObjectOfType<MouseLook>();
    }
    // Update is called once per frame
    void Update()
    {
        //right mouse
        if (Input.GetMouseButtonDown(1))
        {
            mouseLook.SetIsActive(true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            mouseLook.SetIsActive(false);
        }


        //left mouse

        if (Input.GetMouseButtonUp(0))
        {
            ConnectionManager.OnMouseUp();
        }

        if (Input.GetMouseButton(0))
        {
            mouseLook.SetIsActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ConnectionManager.isLeftClick = true;
        }
        else
        {
            ConnectionManager.isLeftClick = false;
        }
    }
}
