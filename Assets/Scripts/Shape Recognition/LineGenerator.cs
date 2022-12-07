using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineGenerator : MonoBehaviour
{
    [SerializeField]
    Sprite lineImage;
    Vector2 graphScale;
    public float lineWidth = 5;

    private void Awake()
    {
        graphScale = new Vector2(1, 1);

        if (lineWidth == 0)
        {
            lineWidth = 5;
        }
    }
    void Start()
    {
        ConnectionManager.EstablishLineGenerator(this);
    }

    public void MakeLine(float ax, float ay, float bx, float by, Color col, string tagName)
    {
        GameObject NewObj = new GameObject();
        NewObj.name = "line from " + ax + " to " + bx;
        Image NewImage = NewObj.AddComponent<Image>();
        NewImage.sprite = lineImage;
        NewImage.color = col;
        NewImage.gameObject.layer = LayerMask.NameToLayer("UI");
        NewImage.gameObject.tag = tagName;
        RectTransform rect = NewObj.GetComponent<RectTransform>();
        rect.SetParent(transform);
        rect.localScale = Vector3.one;

        Vector3 a = new Vector3(ax * graphScale.x, ay * graphScale.y, 0);
        Vector3 b = new Vector3(bx * graphScale.x, by * graphScale.y, 0);


        rect.localPosition = (a + b) / 2;
        Vector3 dif = a - b;
        Debug.Log("ax = " + ax + ", bx = " + bx + ", ay = " + ay + ", by = " + by + ", a = " + a + ", b = " + b + ", dif = " + dif);
        rect.sizeDelta = new Vector3(dif.magnitude, lineWidth);
        rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
        Debug.Log("Line Maker vector = " + new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
    }

    public void ChangeLastLineSiblingIndex()
    {
        Transform line = transform.GetChild(transform.childCount - 1);
        line.SetSiblingIndex(transform.childCount - 2);
    }
    public void FadeOutLastLines(Transform[] jemTransforms)
    {
        //recreate shape
        for(int i = 0; i <jemTransforms.Length; i++)
        {
            int index;
            if (i == jemTransforms.Length - 1)
            {
                index = 0;
            }else
            {
                index = i + 1;
            }

            if(!IsTooLong(jemTransforms, i, index))
            {
                Debug.Log("DRAWING LINE BETWEEN JEMS, jemTransforms[i].pos = " + jemTransforms[i].position + ", jemTransforms[index].pos = " + jemTransforms[index].position);
                MakeLine(jemTransforms[i].position.x, jemTransforms[i].position.y,
                jemTransforms[index].transform.position.x, jemTransforms[index].transform.position.y, Color.white, "standard");
            }
        }

        //get standard lines (not display)
        List<Image> lines  = new List<Image>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Image line = transform.GetChild(i).GetComponent<Image>();

            if (line.gameObject.CompareTag("standard"))
            {
                lines.Add(line);
            }
        }

        //fade out lines
        StartCoroutine(FadeOutRoutine(lines.ToArray()));
    }

    public void RemoveLines(string tagName)
    {
        for(int i = 0; i<transform.childCount; i++)
        {
            GameObject line = transform.GetChild(i).gameObject;

            if (line.CompareTag(tagName))
            { 
                Destroy(line); 
            }
        }
    }

    public void RemoveLastLine()
    {
        GameObject line = transform.GetChild(transform.childCount-1).gameObject;
        Destroy(line);
    }

    public IEnumerator FadeOutRoutine(Image[] lines)
    {
        yield return new WaitForSeconds(0.005f);

        //if they are transparent, delete them
        //if not, make them more transparent
        if (lines[0].color.a > 0)
        {
            foreach (var standardLine in lines)
            {
                standardLine.color = new Color(standardLine.color.r, standardLine.color.g, standardLine.color.b, standardLine.color.a - 0.07f); 
            }
            StartCoroutine(FadeOutRoutine(lines));
        }
        else
        {
            RemoveLines("standard");
        }
    }

    bool IsTooLong(Transform[] jemTransforms, int i, int index)
    {
        //if the line is not too long to make
        Vector2 firstJem = jemTransforms[i].position;
        Vector2 lastJem = jemTransforms[index].position;
        float xDifference = Mathf.Max(firstJem.x, lastJem.x) - Mathf.Min(firstJem.x, lastJem.x);


        if (xDifference > ConnectionManager.maxXDifference)
        {
            Debug.Log("POSITION INAPPROPRIATE TO DRAW LINE:  X DIFFERENCE TOO HIGH");
            return true;
        }

        float yDifference = Mathf.Max(firstJem.y, lastJem.y) - Mathf.Min(firstJem.y, lastJem.y);

        if (yDifference > ConnectionManager.maxYDifference)
        {
            Debug.Log("POSITION INAPPROPRIATE TO DRAW LINE:  Y DIFFERENCE TOO HIGH");
            return true;
        }

        return false;
    }
}
