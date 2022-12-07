using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeResultManager : MonoBehaviour
{
    VOManager voiceOverManager;
    LineGenerator lineGenerator;
    Scene sceneManager;
    DisplayShape displayShape;

    private void Start()
    {
        voiceOverManager = FindObjectOfType<VOManager>();
        lineGenerator = FindObjectOfType<LineGenerator>();
        sceneManager = FindObjectOfType<Scene>();
        displayShape = FindObjectOfType<DisplayShape>();
    }

    public void SwitchJemTypes(Transform innerJem, Transform[] jems)
    {
        Image innerJemImage = innerJem.GetComponent<Image>();
        Sprite originalInnerSprite = innerJemImage.sprite;
        Sprite originalOuterSprite = jems[0].GetComponent<Image>().sprite;

        innerJem.GetComponent<Image>().sprite = originalOuterSprite;
        foreach(var jem in jems)
        {
            jem.GetComponent<Image>().sprite = originalInnerSprite;
        }
    }

    public void UnlockShape(Transform[] jemTransforms)
    {
        Debug.Log("UNLOCKING SHAPE");
        StartCoroutine(WaitAnInstantRoutine(jemTransforms));
    }

    public void UnlockAudioSource()
    {
        if (voiceOverManager != null)
        {
            voiceOverManager.PlayNextAudioSource();
        }

        if(displayShape != null)
        {
            displayShape.DisplayNextShape();
        }
    }

    public void UnlockMandalaPiece()
    {
        sceneManager.GoToNextLevel();
    }

    IEnumerator WaitAnInstantRoutine(Transform[] jemTransforms)
    {
        yield return new WaitForSeconds(0.005f);
        lineGenerator.FadeOutLastLines(jemTransforms);
    }
}
