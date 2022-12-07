using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeChecker : MonoBehaviour
{
    [SerializeField]
    bool testMode = false;
    [SerializeField]
    bool regularJemShiftShapes = false;

    Vector2[] dirs;
    Color shapeColor;
    Sprite jemSprite;
    Shape[] shapes;
    int shapeIndex = 0;

    ShapeResultManager shapeResultManager;

    [SerializeField]
    int maxDistanceToJemCentre = 18;

    private void Start()
    {
        ConnectionManager.EstablishShapeChecker(this);

        shapeResultManager = FindObjectOfType<ShapeResultManager>();

        shapes = GetComponents<Shape>();
    }

    public void OnShapeComplete(List<PixelScript>jems)
    {
        //call check if Jem shift
        CheckIfJemShift(PixelScriptToTransformArray(jems.ToArray()));


        //if is test mode, print dirs
        if (testMode)
        {
            PrintDirsOfShape(jems.ToArray());
        }


        //call compare shape
        //get current shape
        Shape currentShape = GetNextShape();
        if (currentShape == null)
        {
            return;
        }
        else
        {
            dirs = currentShape.dirs;
            shapeColor = currentShape.shapeColor;
            jemSprite = currentShape.image;
        }

        #region Preliminary Checks
        //check if number of jems is equal to correct shape
        //if not, return
        if (jems.Count != dirs.Length)
        {
            Debug.Log("JEM NUMBER NOT EQUAL TO CORRECT SHAPE");
            return;
        }

        //check if color is equal to correct color
        //if not, return
        if(!IsSameColour(jems[0]))
        {
            Debug.Log("JEM COLOR NOT EQUAL TO CORRECT COLOR");
            return;
        }

        //check if jem type is equal to correct jem type
        //if not, return
        if (!IsSameJemType(jems[0]))
        {
            Debug.Log("WRONG JEM TYPE");
            return;
        }
        #endregion


        //get directions from jem to jem, store as list of Vector2s
        List<Vector2> jemPositions = new List<Vector2>();
        List<Vector2> playerShapeDirs = new List<Vector2>();
        foreach (var jem in jems)
        {
            jemPositions.Add(jem.transform.position);
        }

        for(int i = 0; i<jemPositions.Count; i++)
        {
            //get index of next jem in shape
            //if jem is last in list, get index of first jem   (that works for the square [doesn't check if last jem is adjacent to first jem])
            int nextJemIndex = i+1;
            if (i == jemPositions.Count-1)
            {
                nextJemIndex = 0;
            }

            Vector2 dir = jemPositions[nextJemIndex] - jemPositions[i];
            playerShapeDirs.Add(dir);
            //Debug.Log(dir);
        }

        //compare to normal direction shape
        CompareToMandalaShape(playerShapeDirs.ToArray(), jems.ToArray(), dirs, false);
    }

    void CheckIfJemShift(Transform[] jems)
    {
        //check has three points
        if (jems.Length != 3)
        {
            return;
        }

        //get jems near centre
        Transform[] nearbyJems = JemsNearCentre(jems);
        //check how MANY jems are inside polygon                      (code not using inside polygon check)
        if (nearbyJems.Length != 1)
        {
            Debug.Log("not a single jem near centre. Jems near centre = " + nearbyJems.Length);
            return;
        }


        //check if shape within is of different jem type
        Transform insidePolygonJem = nearbyJems[0];
        if (insidePolygonJem.gameObject.GetComponent<Image>().sprite == jems[0].gameObject.GetComponent<Image>().sprite)
        {
            return;
        }

        //check dir from last jem to first is not too long
        if (IsLastSideTooLong(jems))
        {
            //return;
        }

        //check sides are roughly the same length
        if (regularJemShiftShapes)
        {
            if (!AreSidesEqualLength(jems))
            {
                return;
            }
        }

        //if you want, you could:
        //get Vectors between points
        //get angles between vectors
        //check angles' difference to 90 degrees is not too much

        shapeResultManager.SwitchJemTypes(insidePolygonJem, jems);
        shapeResultManager.UnlockShape(jems);
        Debug.Log("Correct JEM SHIFT SHAPE!!");
    }

    void CompareToMandalaShape(Vector2[] playerShapeDirs, PixelScript[] jems, Vector2[] correctDirs, bool isReversed)
    {
        Debug.Log("comparing... correct first dir = " + correctDirs[0]);
        //find all occurences of first dir in correct shape
        //if none, return
        List<int> correctFirstDirOccurenceIDs = new List<int>();
        for (int i = 0; i < playerShapeDirs.Length; i++)
        {
            Debug.Log("player shape dirs = " +playerShapeDirs[i]);
            if (Mathf.RoundToInt(playerShapeDirs[i].x) == Mathf.RoundToInt(correctDirs[0].x) && Mathf.RoundToInt(playerShapeDirs[i].y) == Mathf.RoundToInt(correctDirs[0].y))
            {
                correctFirstDirOccurenceIDs.Add(i);
                Debug.Log("Adding correct firstdir occurence ID");
            }
        }

        if (correctFirstDirOccurenceIDs.Count == 0)
        {
            if (!isReversed)
            {
                CompareToMandalaShape(playerShapeDirs, jems, ReverseList(dirs), true);
            }

            Debug.Log("NO OCCURENCES OF FIRST DIR IN SHAPE");
            return;
        }

        //for each occurrence of first dir
        foreach (var firstDirOccurence in correctFirstDirOccurenceIDs)
        {
            //cycle through sequence from occurence of first dir, starting at [0] when you reach the last in sequence
            //if one of the dirs doesn't match, disregard the list
            //if all match, shape is correct
            bool isCorrectList = true;
            for (int i = 0; i < playerShapeDirs.Length; i++)
            {
                int ID = firstDirOccurence + i;
                if (playerShapeDirs.Length <= ID)
                {
                    ID -= playerShapeDirs.Length;
                }

                if (Mathf.RoundToInt(playerShapeDirs[ID].x) != Mathf.RoundToInt(correctDirs[i].x) || Mathf.RoundToInt(playerShapeDirs[ID].y) != Mathf.RoundToInt(correctDirs[i].y))
                {
                    isCorrectList = false;
                }
            }

            //if it's the right shape, implement effects
            if (isCorrectList)
            {
                ImplementShapeEffects(jems);
                Debug.Log("SHAPE IS CORRECT!!");
                return;
            }
            else
            {
                Debug.Log("not correct, mate");
            }

        }

        if (!isReversed)
        {
            Debug.Log("ANALYSING REVERSED SHAPE");
            //else, compare to reversed direction shape
            CompareToMandalaShape(playerShapeDirs, jems, ReverseList(dirs), true);
        }
    }

    void ImplementShapeEffects(PixelScript[] jems)
    {      
        shapeResultManager.UnlockShape(PixelScriptToTransformArray(jems));

        shapeIndex++;
        //if it's not the last shape, it's a VO
        //else, MANDALA
        if (shapeIndex < shapes.Length)
        {
            Debug.Log("VOOO");
            shapeResultManager.UnlockAudioSource();
        }
        else
        {
            Debug.Log("MANDALA");
            shapeResultManager.UnlockMandalaPiece();
        }
    }

    //get next shape in list
    Shape GetNextShape()
    {
        if (shapeIndex >= shapes.Length)
        {
            return null;
        }

        Shape shapeToSend = shapes[shapeIndex];
        return shapeToSend;
    }

    public void PrintDirsOfShape(PixelScript[] jems)
    {
        //get directions from jem to jem, store as list of Vector2s
        List<Vector2> jemPositions = new List<Vector2>();
        foreach (var jem in jems)
        {
            jemPositions.Add(jem.transform.position);
        }

        for (int i = 0; i < jemPositions.Count; i++)
        {
            //get index of next jem in shape
            //if jem is last in list, get index of first jem   (that works for the square [doesn't check if last jem is adjacent to first jem])
            int nextJemIndex = i + 1;
            if (i == jemPositions.Count - 1)
            {
                nextJemIndex = 0;
            }

            Vector2 dir = jemPositions[nextJemIndex] - jemPositions[i];
            Debug.Log("dir = "+ dir);
        }
    }

    Transform[] JemsNearCentre(Transform[] jems)
    {
        //get centre of shape
        Vector3 jemCentre = GetPolygonCentre(jems);

        //cycle through jems, see if one is near centre
        //PixelScript pixelScript = FindObjectOfType<PixelScript>();
        //Transform pixelParent = pixelScript.transform.parent;
        /*for (int i = 0; i < pixelParent.childCount; i++)
        {
            allJemTransforms.Add(pixelParent.GetChild(i));
        }*/
        
        PixelScript[] pixelScripts = FindObjectsOfType<PixelScript>();
        List<Transform> allJemTransforms = new List<Transform>();

        for (int i = 0; i < pixelScripts.Length; i++)
        {
            allJemTransforms.Add(pixelScripts[i].transform);
        }
        


        List <Transform> nearbyJems = new List<Transform>();
        foreach (Transform jemTransform in allJemTransforms)
        {
            Vector3 jemPos = jemTransform.position;
            float distance = (jemCentre - jemPos).magnitude;

            if (distance < maxDistanceToJemCentre)// was 20, then 18
            {
                nearbyJems.Add(jemTransform);
            }
            //Debug.Log("nearbyjems.count = " + nearbyJems.Count);
        }

        return nearbyJems.ToArray();
        /*

        //check if it's inside the polygon
        List<Transform> insidePolygonJems = new List<Transform>();
        foreach (var nearbyJem in nearbyJems)
        {
            Vector2 jemPos = nearbyJem.position;
            List<Vector2> polygon = new List<Vector2>();
            foreach(var j in jems)
            {
                Vector2 polygonPoint = j.position;
                polygon.Add(polygonPoint);
            }

            if (IsPointInPolygon(jemPos, polygon.ToArray()))
            {
                insidePolygonJems.Add(nearbyJem);
            }
        }

        

        //check how MANY jems are inside polygon
        if(insidePolygonJems.Count != 1)
        {
            return;
        }

        */
    }

    bool AreSidesEqualLength(Transform[] jems)
    {
        List<float> dirLengths = new List<float>();
        for (int i = 0; i < jems.Length; i++)
        {
            //get index of next jem in shape
            //if jem is last in list, get index of first jem   (that works for the square [doesn't check if last jem is adjacent to first jem])
            int nextJemIndex = i + 1;
            if (i == jems.Length - 1)
            {
                nextJemIndex = 0;
            }

            Vector2 dir = jems[nextJemIndex].position - jems[i].position;
            dirLengths.Add(dir.magnitude);      
        }

        //check dirs are of same length
        for (int b = 0; b < dirLengths.Count; b++)
        {
            int nextDirIndex = b + 1;
            if (b == jems.Length - 1)
            {
                nextDirIndex = 0;
            }

            float difference = Mathf.Max(dirLengths[b], dirLengths[nextDirIndex]) - Mathf.Min(dirLengths[b], dirLengths[nextDirIndex]);
            if (difference > ConnectionManager.maxXDifference / 4)
            {
                Debug.Log("SIDES NOT EQUAL");
                return false;
            }
        }
        return true;
    }
    bool IsLastSideTooLong(Transform[] jems)
    {
        Vector2 firstJem = jems[0].position;
        Vector2 lastJem = jems[jems.Length-1].position;

        float xDifference = Mathf.Max(firstJem.x, lastJem.x) - Mathf.Min(firstJem.x, lastJem.x);

        if (xDifference > ConnectionManager.maxXDifference)
        {
            Debug.Log("POSITION INAPPROPRIATE TO FINISH SHAPE:  X DIFFERENCE TOO HIGH");
            return true;
        }

        float yDifference = Mathf.Max(firstJem.y, lastJem.y) - Mathf.Min(firstJem.y, lastJem.y);

        if (yDifference > ConnectionManager.maxYDifference)
        {
            Debug.Log("POSITION INAPPROPRIATE TO FINISH SHAPE:  Y DIFFERENCE TOO HIGH");
            return true;
        }

        //Vector2 dir = firstJem - lastJem;

        //FOR REGULAR LAYOUTS TO ENFORCE SQUARES
        /*if (dir.magnitude > ConnectionManager.maxYDifference && jems.Length ==4)
        {
            return true;
        }*/

        return false;
    }

    Vector3 GetPolygonCentre(Transform[] jems)
    {
        Vector3 pos = Vector3.zero;
        foreach (Transform t in jems)
        {
            pos += t.position;
        }
        return pos / jems.Length;
    }

    public bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int polygonLength = polygon.Length, i = 0;
        bool inside = false;
        // x, y for tested point.
        float pointX = point.x, pointY = point.y;
        // start / end point for the current polygon segment.
        float startX, startY, endX, endY;
        Vector2 endPoint = polygon[polygonLength - 1];
        endX = endPoint.x;
        endY = endPoint.y;
        while (i < polygonLength)
        {
            startX = endX; startY = endY;
            endPoint = polygon[i++];
            endX = endPoint.x; endY = endPoint.y;
            //
            inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                      && /* if so, test if it is under the segment */
                      ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
        }
        return inside;
    }

    Vector2[] ReverseList(Vector2[] dirsToReverse)
    {
        //reverse each dir in list
        List<Vector2> reversedList = new List<Vector2>();
        foreach(var dir in dirsToReverse)
        {
            reversedList.Add(-dir);
        }

        //reverse list order
        reversedList.Reverse();

        return reversedList.ToArray();
    }

    bool IsSameColour(PixelScript jem)
    {
        Color jemColor = jem.transform.GetComponent<Image>().color;

        //compare rgb values of jem with last selected
        float rDifference = Mathf.Max(jemColor.r, shapeColor.r) - Mathf.Min(jemColor.r, shapeColor.r);
        float gDifference = Mathf.Max(jemColor.g, shapeColor.g) - Mathf.Min(jemColor.g, shapeColor.g);
        float bDifference = Mathf.Max(jemColor.b, shapeColor.b) - Mathf.Min(jemColor.b, shapeColor.b);
        float averageRGBDifference = (rDifference + gDifference + bDifference) / 3;

        //make up for eye distinguishing less well between brighter colours
        float brightness = (jemColor.r + jemColor.g + jemColor.b) / 3;

        if (averageRGBDifference > 1.5f)
        {
            return false;
        }

        return true;
    }

    bool IsSameJemType(PixelScript jem)
    {
        if (jem.gameObject.GetComponent<Image>().sprite == jemSprite)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    Transform[] PixelScriptToTransformArray(PixelScript[] jems)
    {
        //get transforms of jems
        List<Transform> jemTransforms = new List<Transform>();
        foreach (var jem in jems)
        {
            jemTransforms.Add(jem.transform);
        }
        return jemTransforms.ToArray();
    }
}
