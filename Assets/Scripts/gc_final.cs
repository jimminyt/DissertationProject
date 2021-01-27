
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections.Generic;

public class gc_final : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public ARCameraBackground m_ARCameraBackground;
    public RenderTexture renderedTexture;
    public Image canvasImage;

    private int count = 0;
    private Texture2D cameraTexture;
    private Texture2D imageTexture;
    private List<int> labelArray = new List<int>();
    private List<float> strengthArray = new List<float>();
    private int imageWidth;
    private int imageHeight;
    private bool changed;
    private bool drawingObject = true;
    private bool startDrawing = false;



    // Start is called before the first frame update
    void Start()
    {
        GetCameraImage();
        imageHeight = cameraTexture.height;
        imageWidth = cameraTexture.width;
        imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);

        cameraTexture.Apply();
        canvasImage.sprite = Sprite.Create(cameraTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));


        // Set arrays
        for (int y = 0; y < imageHeight; ++y)
        {
            for (int x = 0; x < imageWidth; ++x)
            {
                imageTexture.SetPixel(x, y, Color.clear);
                labelArray.Add(0);
                strengthArray.Add(0.4f);

            }
        }
        changed = true;
        imageTexture.Apply();
    }

    void Update()
    {
        // Delay before getting first image reduces errors
        if (count == 10)
        {
            GetCameraImage();
        }
        // Once cube is placed can start running seeding process and then growcut when activated
        if (count > 10 && GameObject.FindWithTag("mainCube") != null)
        {
            if (startDrawing)
            {
                growCut();
            }
            else
            {
                checkTouch();
            }

        }
        count += 1;
        canvasImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));


    }


    private void growCut()
    {
        var labels_new = new List<int>(labelArray);
        var strength_new = new List<float>(strengthArray);
        // Main GrowCut loop
        if (changed == true)
        {
            changed = false;
            for (int y = 1; y < imageHeight - 1; ++y) //Smaller range to avoid edges
            {
                for (int x = 1; x < imageWidth - 1; ++x)
                {
                    var k = x + (imageWidth * y);
                    for (int i = -1; i < 2; ++i) //-1,0,1
                    {
                        for (int j = -1; j < 2; ++j)
                        {
                            if ((i != 0) || (j != 0))
                            {
                                var q = (x + i) + (imageWidth * (y + j));

                                if (labelArray[q] != 0)
                                {
                                    var attack = GetAttackForce(cameraTexture.GetPixel(x, y), cameraTexture.GetPixel(x + i, y + j)) * strengthArray[q];
                                    // If attack is successful
                                    if (attack > strengthArray[k])
                                    {
                                        changed = true;
                                        labels_new[k] = labelArray[q];
                                        strength_new[k] = attack;
                                    }
                                }
                            }
                        }
                    }

                }
            }


            labelArray = new List<int>(labels_new);
            strengthArray = new List<float>(strength_new);


            // Set display pixels
            for (int y = 0; y < imageHeight; ++y)
            {
                for (int x = 0; x < imageWidth; ++x)
                {
                    var k = x + (imageWidth * y);
                    if (labelArray[k] == 1)
                    {
                        imageTexture.SetPixel(x, y, Color.green);
                    }
                    else if (labelArray[k] == 2)
                    {
                        imageTexture.SetPixel(x, y, Color.black);
                    }
                }
            }


              } else
        {
            for (int y = 0; y < imageHeight; ++y)
            {
                for (int x = 0; x < imageWidth; ++x)
                {
                    var k = x + (imageWidth * y);
                    if (labelArray[k] == 1)
                    {
                        imageTexture.SetPixel(x, y, cameraTexture.GetPixel(x,y));
                    }
                    else if (labelArray[k] == 2)
                    {
                        imageTexture.SetPixel(x, y, Color.clear);
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

    //Sets a seed of a given label in a given pixel
    private void setSeed(int x, int y, int label)
    {
        var k = x + (imageWidth * y);
        labelArray[k] = label;
        strengthArray[k] = 1.0f;
    }

    // Function to get the attack force of a neightbour on a given pixel
    private float GetAttackForce(Color P, Color N)
    {
        return 1 - (ComparePixels(P, N) / 3);
    }

    // Compares the RBG values of two pixels and returns a difference value
    private float ComparePixels(Color pixel1, Color pixel2)
    {
        return (Math.Abs(pixel1.r - pixel2.r) + Math.Abs(pixel1.g - pixel2.g) + Math.Abs(pixel1.b - pixel2.b));
    }


    // Touch operation to place seeds
    private void checkTouch()
    {
        Touch touch = Input.GetTouch(0);
        if (Input.touchCount > 0)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                var yvar = Math.Floor(touch.position.y / 4); //Due to camera texture and touch having different y ranges
                var xvar = Math.Floor(touch.position.x / 4);
                // Touch in bottom left toggles placing object and background markers
                if (xvar < 150 && yvar < 50)
                {
                    if (drawingObject == true)
                    {
                        drawingObject = false;
                    }
                    else
                    {
                        drawingObject = true;
                    }
                // Begins GrowCut on bottom right tap
                } else if (xvar > 400 && yvar < 50)
                {
                    startDrawing = true;
                    GetCameraImage();
                }
                // Taps on screen create seeds
                else
                {
                    if (drawingObject)
                    {
                        imageTexture.SetPixel((int)xvar, (int)yvar, Color.green);
                        setSeed((int)xvar, (int)yvar, 1);

                    }
                    else
                    {
                        imageTexture.SetPixel((int)xvar, (int)yvar, Color.blue);
                        setSeed((int)xvar, (int)yvar, 2);

                    }
                    imageTexture.Apply();
                    canvasImage.sprite = Sprite.Create(imageTexture, new Rect(0.0f, 0.0f, cameraTexture.width, cameraTexture.height), new Vector2(0.5f, 0.5f));


                }
            }
        }
    }
}
