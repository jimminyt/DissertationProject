using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System;
using System.Collections.Generic;

public class objectFinding : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public ARCameraBackground m_ARCameraBackground;
    public RenderTexture renderedTexture;
    public Image canvasImage;

    public enum option1 { Full_screen, Middle_box, Placement_box, No_occlusion };
    public option1 findType;
    public enum option2 { Basic_colour_object, Basic_colour_background};
    public option2 borderType;

    private int count = 0;
    private Texture2D cameraTexture;
    private Texture2D imageTexture;
    private int imageWidth;
    private int imageHeight;
    private GameObject cubeObject;
    private float tolerance = 0.4f;



    // Start is called before the first frame update
    void Start()
    {
        GetCameraImage();
        imageHeight = cameraTexture.height;
        imageWidth = cameraTexture.width;
        // Image texture is for displaying to user additional data like the object pixel overlay during occlusion
        imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);

        for (int y = 0; y < imageHeight; ++y)
        {
            for (int x = 0; x < imageWidth; ++x)
            {
                imageTexture.SetPixel(x, y, Color.clear);
            }
        }
        imageTexture.Apply();
        canvasImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));

    }

    void Update()
    {
        // Cube area and full image code
        if (count == 10 && findType != option1.Placement_box)
        {
            GetCameraImage();
            if (findType == option1.Middle_box)
            {
                ProcessCubeArea();
            }
            else if (findType == option1.Full_screen)
            {
                ProcessImage();

            }


            count = 0;
        }
        // Placement box finding code
        else if (findType == option1.Placement_box)
        {
            if (count > 10)
            {

                cubeObject = GameObject.FindWithTag("mainCube");

                if (cubeObject != null)
                {
                    Touch touch = Input.GetTouch(0);
                    if (Input.touchCount > 0)
                    {
                        if (touch.phase == TouchPhase.Ended)
                        {
                            // Touch in bottom left corner to check for box
                            if (touch.position.x < 250 && touch.position.y < 125)
                            {
                                for (int y = 0; y < imageHeight; ++y)
                                {
                                    for (int x = 0; x < imageWidth; ++x)
                                    {
                                        if (checkBox(x, y))
                                        {

                                            imageTexture.SetPixel(x, y, Color.red);

                                        }
                                    }

                                }
                            }
                        }
                    }
                    imageTexture.Apply();
                    canvasImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));

                }
                else
                {
                    GetCameraImage();
                    canvasImage.sprite = Sprite.Create(cameraTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));
                }

            }
            else if (count == 10)
            {
                GetCameraImage();
                canvasImage.sprite = Sprite.Create(cameraTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));

            }
        }
        count += 1;



    }

    // Method taken from unity forums https://forum.unity.com/threads/how-to-get-camera-texture-in-arfoundation.543827/
    private void GetCameraImage()
    {
        Graphics.Blit(null, renderedTexture, m_ARCameraBackground.material);
        // Copy the RenderTexture from GPU to CPU
        var activeRenderTexture = RenderTexture.active;
        RenderTexture.active = renderedTexture;
        cameraTexture = new Texture2D(renderedTexture.width, renderedTexture.height, TextureFormat.RGBA32, false);
        //cameraTexture = new Texture2D(600, 160, TextureFormat.RGBA32, false);
        if (cameraTexture != null)
        {
            cameraTexture.ReadPixels(new Rect(0, 0, renderedTexture.width, renderedTexture.height), 0, 0);
            //cameraTexture.ReadPixels(new Rect(822, 48, 600, 160), 0, 0);
        }
        cameraTexture.Apply();
        RenderTexture.active = activeRenderTexture;

    }

    //Processes full screen image
    private void ProcessImage()
    {
        // Two options for getting background midpixel and object midpixel
        var midPixel = Color.clear;
        if (borderType == option2.Basic_colour_object)
        {
            midPixel = cameraTexture.GetPixel(Convert.ToInt32(cameraTexture.width / 2.0), Convert.ToInt32(cameraTexture.height / 2.0));
        }
        else if (borderType == option2.Basic_colour_background)
        {
            midPixel = cameraTexture.GetPixel(Convert.ToInt32(cameraTexture.width / 2.0) + 300, Convert.ToInt32(cameraTexture.height / 2.0));
        }

        Color[] imageArray = cameraTexture.GetPixels();


        for (int y = 0; y < cameraTexture.height; ++y)
        {
            for (int x = 0; x < cameraTexture.width; ++x)
            {
                var k = x + (cameraTexture.width * y);
                var pixelColor = imageArray[k];
                if (ComparePixels(pixelColor, midPixel) >= tolerance)
                {
                    imageArray[k] = Color.clear;
                }
            }
        }

        imageTexture.SetPixels(imageArray);
        imageTexture.Apply();
        canvasImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));
    }

    // Processess subsection in centre of the screen
    private void ProcessCubeArea()
    {
        var midPixel = Color.clear; //Set default
        if (borderType == option2.Basic_colour_object) // Chosen pixel at centre to get object
        {
            midPixel = cameraTexture.GetPixel(Convert.ToInt32(cameraTexture.width / 2.0), Convert.ToInt32(cameraTexture.height / 2.0));
            // Selects subsection
            for (int y = -150; y < 150; ++y)
            {
                for (int x = -300; x < 300; ++x)
                {
                    var pixelColor = cameraTexture.GetPixel(x + (cameraTexture.width / 2), y + (cameraTexture.height / 2));
                    if (ComparePixels(pixelColor, midPixel) >= tolerance)
                    {
                        imageTexture.SetPixel(x + (cameraTexture.width / 2), y + (cameraTexture.height / 2), Color.clear);
                    }
                    else
                    {
                        imageTexture.SetPixel(x + (cameraTexture.width / 2), y + (cameraTexture.height / 2), pixelColor);
                    }
                }
            }
        }

        else if (borderType == option2.Basic_colour_background) // Chosen pixel to the right of centre to get the background
        {
            midPixel = cameraTexture.GetPixel(Convert.ToInt32(cameraTexture.width / 2.0) + 250, Convert.ToInt32(cameraTexture.height / 2.0)); // 250 to right
            for (int y = -150; y < 150; ++y)
            {
                for (int x = -300; x < 300; ++x)
                {
                    var pixelColor = cameraTexture.GetPixel(x + (cameraTexture.width / 2), y + (cameraTexture.height / 2));
                    if (ComparePixels(pixelColor, midPixel) >= tolerance)
                    {
                        imageTexture.SetPixel(x + (cameraTexture.width / 2), y + (cameraTexture.height / 2), pixelColor);
                    }
                    else
                    {
                        imageTexture.SetPixel(x + (cameraTexture.width / 2), y + (cameraTexture.height / 2), Color.clear);
                    }
                }
            }
        }

        imageTexture.Apply();
        canvasImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));
    }


    // Checks for the bounding box at position x,y
    private bool checkBox(int x, int y)
    {
        var output = false;
        // Generates variables for raycast
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0)); // Ray at location x,y on screen
        RaycastHit hit;
        int layerMask = 1 << 8; // Layermask makes it only so it will collide with layer 8
        // If there is a collision with cube, output true to signal box located there
        if (Physics.Raycast(ray, out hit, 100, layerMask))
        {
            output = true;
        }
        return output;
    }

    // Compares the RBG values of two pixels and returns a difference value
    private float ComparePixels(Color pixel1, Color pixel2)
    {
        return (Math.Abs(pixel1.r - pixel2.r) + Math.Abs(pixel1.g - pixel2.g) + Math.Abs(pixel1.b - pixel2.b));
    }


}