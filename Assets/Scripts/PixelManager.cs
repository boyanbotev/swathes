using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixelManager : MonoBehaviour
{
    [SerializeField]
    RenderTexture renderTexture;
    int width = 1700; //856
    int height = 1200;  //856

    //0,0,856,856
    Rect rectReadPicture = new Rect (0,0,1700,1200);
    public Vector3 size = new Vector3(1000, 10, 100);

    Camera camera;

    [SerializeField]
    Transform pixelRowHolder;
    List<Transform> pixelObjects;

    [SerializeField]
    RawImage testImage;

    private void Start()
    {
        camera = Camera.main;

        LocatePixels();

        //ChoosePixelTexture();
       // InvokeRepeating("ChangePixelsColour", 0.1f, 0.1f);
    }

    void LocatePixels()
    {
        pixelObjects = new List<Transform>();
        for (int i = 0; i < pixelRowHolder.childCount; i++)
        {
            Transform pixelRow = pixelRowHolder.GetChild(i);
            for (int k = 0; k < pixelRow.childCount; k++)
            {
                pixelObjects.Add(pixelRow.GetChild(k));
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ChangePixelsColour();
    }

    Texture2D RenderTextureToTexture2D()
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        //Initialize and Render
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;

        //Read pixels
        tex.ReadPixels(rectReadPicture, 0, 0);

        //Clean up
        camera.targetTexture = null;
        RenderTexture.active = null; //added to avoid errors
        DestroyImmediate(rt);

        //return
        return tex;
    }

    void ChangePixelsColour()
    {
        Texture2D texture = RenderTextureToTexture2D();

        foreach (var pixelObject in pixelObjects)
        {
            Image pixelImage = pixelObject.GetComponent<Image>();

            int x = Mathf.FloorToInt(((pixelObject.position.x-120)/0.9f)-50); //+450  then -220
            int y = Mathf.FloorToInt(((pixelObject.position.y+50)/0.9f))-50;  //-100  then -100

            Color currentPixelColor = texture.GetPixel(x, y);
            pixelImage.color = currentPixelColor;
        }

        //testImage.texture = texture;

        Destroy(texture);
    }

    void ChoosePixelTexture()
    {
        foreach (var pixelObject in pixelObjects)
        {
            Image pixelImage = pixelObject.GetComponent<Image>();

            int randomInt = Random.Range(1, 3);
            if (randomInt > 1)
            {
                pixelImage.sprite = Resources.Load("Assets/Art/jem 1.png") as Sprite;

            }
            else
            {
                pixelImage.sprite = Resources.Load("Assets / Art / jem 2.png") as Sprite;
            }
        }
    }
}
