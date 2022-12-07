using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayShape : MonoBehaviour
{
    Shape[] shapes;
    int shapeIndex;
    [SerializeField]
    GameObject pixelPrefab;
    ShapeResultManager shapeResultManager;
    LineGenerator lineGenerator;
    void Start()
    {
        shapeResultManager = FindObjectOfType<ShapeResultManager>();
        lineGenerator = FindObjectOfType<LineGenerator>();
        shapes = shapeResultManager.gameObject.GetComponents<Shape>();
        DisplayNextShape();
    }

    public void DisplayNextShape()
    {
        //get dirs of next shape
        Shape nextShape = GetNextShape();
        if (nextShape == null)
        {
            return;
        }

        Debug.Log("DISPLAYING SHAPE");

        //delete previous shape
        for(int i = 0; i< transform.childCount; i++)
        {
            GameObject line = transform.GetChild(i).gameObject;
            Destroy(line);
        }

        //for each dir, make a jem at the end of that dir.
        Vector2 lastJemPos = this.GetComponent<RectTransform>().transform.position;
        List<Transform> jemsToDisplay = new List<Transform>();
        foreach(var dir in nextShape.dirs)
        {
            //get position of jem using vector
            GameObject jem = Instantiate(pixelPrefab);
            RectTransform rect = jem.GetComponent<RectTransform>();
            rect.SetParent(transform);
            rect.localScale = Vector3.one *1.5f;
            rect.position = lastJemPos;
            rect.Translate(dir/1.5f);
            lastJemPos = rect.position;

            //create ui image with correct jemtype and color
            Image jemImage = jem.GetComponent<Image>();
            jemImage.sprite = nextShape.image;
            jemImage.color = new Color(nextShape.shapeColor.r, nextShape.shapeColor.g, nextShape.shapeColor.b, 1);
            jem.layer = this.gameObject.layer;

            //add jem to list
            jemsToDisplay.Add(jem.transform);
        }

        CentralizeShape(jemsToDisplay.ToArray());

        //draw lines between jems if not too long
        DrawLines(jemsToDisplay.ToArray());
    }

    void CentralizeShape(Transform[] jemsToDisplay)
    {
        //centre shape around parent transform
        Vector3 centrePos = GetPolygonCentre(jemsToDisplay);
        Vector2 centralizingDir = transform.position - centrePos;

        foreach (var jem in jemsToDisplay)
        {
            jem.transform.Translate(centralizingDir);
        }
    }

    void DrawLines(Transform[] jemsToDisplay)
    {
        //clear lines from last shape
        lineGenerator.RemoveLines("display");

        //draw lines between jems if not too long
        for (int i = 0; i < jemsToDisplay.Length; i++)
        {
            int index;
            if (i == jemsToDisplay.Length - 1)
            {
                index = 0;
            }
            else
            {
                index = i + 1;
            }

            if (!IsTooLong(jemsToDisplay, i, index))
            {
                Debug.Log("DRAWING LINE BETWEEN JEMS TO DISPLAY, jemsToDisplay[i].pos.x = " + jemsToDisplay[i].position.x + ", jemsToDisplay[index].pos.x = " + jemsToDisplay[index].position.x);
                lineGenerator.MakeLine(jemsToDisplay[i].position.x, jemsToDisplay[i].position.y,
                jemsToDisplay[index].transform.position.x, jemsToDisplay[index].transform.position.y, Color.white, "display");
            }
        }
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

    Shape GetNextShape()
    {
        if (shapeIndex >= shapes.Length)
        {
            return null;
        }

        Shape shapeToSend = shapes[shapeIndex];
        shapeIndex++;
        return shapeToSend;
    }

    bool IsTooLong(Transform[] jemTransforms, int i, int index)
    {
        //if the line is not too long to make
        Vector2 firstJem = jemTransforms[i].position;
        Vector2 lastJem = jemTransforms[index].position;
        float xDifference = Mathf.Max(firstJem.x, lastJem.x) - Mathf.Min(firstJem.x, lastJem.x);


        if (xDifference > ConnectionManager.maxXDifference/1.5)
        {
            Debug.Log("POSITION INAPPROPRIATE TO DRAW LINE:  X DIFFERENCE TOO HIGH");
            return true;
        }

        float yDifference = Mathf.Max(firstJem.y, lastJem.y) - Mathf.Min(firstJem.y, lastJem.y);

        if (yDifference > ConnectionManager.maxYDifference/1.5)
        {
            Debug.Log("POSITION INAPPROPRIATE TO DRAW LINE:  Y DIFFERENCE TOO HIGH");
            return true;
        }

        return false;
    }
}
