using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ConnectionManager
{
    static bool isShapeMode = false;
    public static bool isLeftClick = false;

    static List<PixelScript> selectedJems;

    static LineGenerator lineGenerator;
    static ShapeChecker shapeChecker;
    static WwiseManager wwiseManager;

    public static int maxXDifference = 38;//was 25
    public static int maxYDifference = 45;//was 30

    public static void OnJemClicked(PixelScript jem)
    {
        if (!isLeftClick)
        {
            return;
        }

        selectedJems = new List<PixelScript>();
        isShapeMode = true;
        //highlight clicked jem
        jem.SelectJem();

        //add to list
        selectedJems.Add(jem);

        //play sfx
        if (wwiseManager != null) { wwiseManager.PlayNote(); }
    }

    public static void OnJemEnter(PixelScript jem)
    {
        if (isShapeMode)
        {
            if (selectedJems.Count > 1)
            {
                //if jem is jem before last
                if (jem == selectedJems[selectedJems.Count - 2])
                {
                    //remove last selected jem 
                    selectedJems[selectedJems.Count - 1].DeselectJem();
                    selectedJems.Remove(selectedJems[selectedJems.Count - 1]);
                    //remove last line
                    lineGenerator.RemoveLastLine();

                    Debug.Log("IS JEM BEFORE LAST, JEM COUNT = " + selectedJems.Count);
                }
            }

            CheckAndAddJem(jem, false);
        }
    }

    static void CheckAndAddJem(PixelScript jem, bool IsIntermediateJem)
    {
        //check if jem is already selected
        foreach (var selectedJem in selectedJems)
        {
            if (jem == selectedJem)
            {
                return;
            }
        }

        //check if right colour
        if (!IsSameColour(jem))
        {
            return;
        }

        //check if it's adjacent to last selected jem
        if (!IsAdjacentToPreviousJem(jem))
        {
            return;
        }

        //check if it's same jem type
        if (!IsSameJemType(jem))
        {
            return;
        }

        //play sfx
        if (wwiseManager != null) { wwiseManager.PlayNote(); }

        //add line
        lineGenerator.MakeLine(jem.transform.position.x, jem.transform.position.y,
            selectedJems[selectedJems.Count - 1].transform.position.x, selectedJems[selectedJems.Count - 1].transform.position.y, Color.white, "standard");

        //highlight
        jem.SelectJem();

        //add to shape
        //if it's a standard jem, just add it
        if (!IsIntermediateJem)
        {
            selectedJems.Add(jem);
        }
        //else, add it at the penultimate position
        else
        {
            selectedJems.Insert(selectedJems.Count - 2, jem);
        }
        /*
        //if going to add new jem,
        //check if there is a jem directly between this and previous
        //if so, 
        //remove current jem
        //add this jem
        //add the current jem at penultimate position
        if (JemsNearCentre().Length > 0 && !IsIntermediateJem)
        {
            Transform jemNearCentre = JemsNearCentre()[0];
            PixelScript jemNearCentreScript = jemNearCentre.GetComponent<PixelScript>();
            CheckAndAddJem(jemNearCentreScript, true);
            Debug.Log("SELECTING INTERMEDIATE JEM, JEM COUNT = " + selectedJems.Count);
        }
        else if (IsIntermediateJem)
        {
            //make the line not be last, so that it won't be deleted first when going back over shape
            lineGenerator.ChangeLastLineSiblingIndex();
        }*/
    }

    public static void OnMouseUp()
    {
        //inputhandler triggers this when mouse goes up, regardless of where it is
        if (selectedJems!=null)
        {
            foreach (var jem in selectedJems)
            {
                jem.DeselectJem();
            }

            shapeChecker.OnShapeComplete(selectedJems);

            selectedJems.Clear();
        }

        isShapeMode = false;
        lineGenerator.RemoveLines("standard");
    }

    static Transform[] JemsNearCentre()
    {
        //get centre of shape
        Vector3 jemCentre = GetPolygonCentre();

        //cycle through jems, see if one is near centre
        PixelScript pixelScript = selectedJems[0].GetComponent<PixelScript>();
        Transform pixelParent = pixelScript.transform.parent;
        List<Transform> allJemTransforms = new List<Transform>();

        for (int i = 0; i < pixelParent.childCount; i++)
        {
            allJemTransforms.Add(pixelParent.GetChild(i));
        }

        List<Transform> nearbyJems = new List<Transform>();
        foreach (Transform jemTransform in allJemTransforms)
        {
            Vector3 jemPos = jemTransform.position;
            float distance = (jemCentre - jemPos).magnitude;

            if (distance < 20)// was 20
            {
                nearbyJems.Add(jemTransform);
            }
            //Debug.Log("nearbyjems.count = " + nearbyJems.Count);
        }

        return nearbyJems.ToArray();
    }

    static Vector3 GetPolygonCentre()
    {
        Vector3 pos = Vector3.zero;
        Debug.Log("selectedJems.Count = " + selectedJems.Count);
        pos += selectedJems[selectedJems.Count-1].transform.position + selectedJems[selectedJems.Count - 2].transform.position;
        return pos / 2;
    }

    static bool IsSameColour(PixelScript jem)
    {
        Color jemColor = jem.transform.GetComponent<Image>().color;
        Color lastSelectedJemColor = selectedJems[selectedJems.Count -1].GetComponent<Image>().color;

        //compare rgb values of jem with last selected
        float rDifference = Mathf.Max(jemColor.r, lastSelectedJemColor.r) - Mathf.Min(jemColor.r, lastSelectedJemColor.r);      
        float gDifference = Mathf.Max(jemColor.g, lastSelectedJemColor.g) - Mathf.Min(jemColor.g, lastSelectedJemColor.g);       
        float bDifference = Mathf.Max(jemColor.b, lastSelectedJemColor.b) - Mathf.Min(jemColor.b, lastSelectedJemColor.b);
        float averageRGBDifference = (rDifference + gDifference + bDifference) / 3;

        //make up for eye distinguishing less well between brighter colours   {in dark levels?}
        float brightness = (jemColor.r + jemColor.g + jemColor.b) / 3;

        if (averageRGBDifference > 0.15f  /*was 0.045f +(brightness/13)        then was 0.08f         then 0.1f*/   )
        {
            return false;
        }

        return true;
    }

    static bool IsAdjacentToPreviousJem(PixelScript jem)
    {
        Vector3 jemPos = jem.transform.position;
        Vector3 lastSelectedJemPos = selectedJems[selectedJems.Count - 1].transform.position;

        float xDifference = Mathf.Max(jemPos.x, lastSelectedJemPos.x) - Mathf.Min(jemPos.x, lastSelectedJemPos.x);

        if (xDifference > maxXDifference)
        {
            Debug.Log("POSITION INAPPROPRIATE:  X DIFFERENCE TOO HIGH");
            return false;
        }

        float yDifference = Mathf.Max(jemPos.y, lastSelectedJemPos.y) - Mathf.Min(jemPos.y, lastSelectedJemPos.y);

        if (yDifference > maxYDifference)
        {
            Debug.Log("POSITION INAPPROPRIATE:  Y DIFFERENCE TOO HIGH");
            return false;
        }

        return true;
    }

    static bool IsSameJemType(PixelScript jem)
    {
        if(jem.gameObject.GetComponent<Image>().sprite == selectedJems[selectedJems.Count-1].gameObject.GetComponent<Image>().sprite)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void EstablishLineGenerator(LineGenerator line)
    {
        lineGenerator = line;
    }

    public static void EstablishShapeChecker(ShapeChecker shape)
    {
        shapeChecker = shape;
    }

    public static void EstablishWwiseManager(WwiseManager wwise)
    {
        wwiseManager = wwise;
    }

    public static void EstablishSceneMaxConnectionDistances(int maxX, int maxY)
    {
        maxXDifference = maxX;
        maxYDifference = maxY;
    }
}
