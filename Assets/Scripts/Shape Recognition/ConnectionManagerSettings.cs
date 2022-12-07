using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManagerSettings : MonoBehaviour
{
    public
    int maxXDifference = 38;
    public
    int maxYDifference = 45;
    //was 57, 68 for woman blue

    private void Start()
    {
        ConnectionManager.EstablishSceneMaxConnectionDistances(maxXDifference, maxYDifference);
    }
}
