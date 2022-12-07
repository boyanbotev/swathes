using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Transform player;

    void Update()
    {
        transform.position = player.position;
        transform.rotation = player.transform.rotation;
    }


}
