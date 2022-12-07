using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseManager : MonoBehaviour
{
    public AkEvent PlayNotes1;
    public AkEvent PlayNotes2;
    public AkEvent PlayChords;

    private void Awake()
    {
        ConnectionManager.EstablishWwiseManager(this);
    }
    public void PlayNote()
    {
        Debug.Log("Playing Note");
        if (PlayNotes1 != null)
        { 
            PlayNotes1.HandleEvent(gameObject); 
        }
        else
        {
            Debug.Log("PlayNote is null");
        }
    }
}
