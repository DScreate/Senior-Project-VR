﻿using System;
using UnityEngine;
using OpenCVForUnity;
using System.Collections;

public class WebcamTextureController : MonoBehaviour
{
    [Tooltip("Set the name of the device to use.")]
    public string requestedDeviceName;

    private int defaultWidth = 640;
    private int defaultHeight = 480;

    [Tooltip("Set the width of WebCamTexture.")]
    public int webcamRequestedWidth;
    [Tooltip("Set the width of WebCamTexture.")]
    public int webcamRequestedHeight;

    public int WebcamHeight
    {
        get
        {
            return webcamTexture.height;
        }
    }

    public int WebcamWidth
    {
        get
        {
            return webcamTexture.width;
        }
    }

    private WebCamTexture webcamTexture;

    private int deviceIndex;

    private bool initialized = false;

    //destination mat for changing webcam texture to opencv mat
    private Mat webcamMat;

    //used to save memory
    private Color32[] colors;

    public WebCamTexture WebcamTex
    {
        get { return webcamTexture; }
    }

    public Mat WebcamMat
    {
        get
        {
            if (webcamTexture.didUpdateThisFrame)
                Utils.webCamTextureToMat(webcamTexture, webcamMat, colors);

            return webcamMat;
        }
    }

    public Color32[] Colors
    {
        get { return colors; }
    }

    public void Initialize()
    {
        if (!initialized)
        {
            InitializeWebcamTexture();

            webcamTexture.Play();

            StartCoroutine(WaitForWebcamToInitialize());

            webcamMat = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC4);

            colors = new Color32[webcamTexture.width * webcamTexture.height];

            initialized = true;

            Debug.Log("Webcam width: " + webcamTexture.width + ". Webcam height: " + webcamTexture.height);

            //Need to delete this and change references in other classes to WebcamHeight and WebcamWidth
            webcamRequestedWidth = webcamTexture.width;
            webcamRequestedHeight = webcamTexture.height;
        }
    }

    private void InitializeWebcamTexture()
    {
        if (webcamRequestedWidth < 100 || webcamRequestedHeight < 100)
            SetDefaultValuesForWebcamSize();

        if (!String.IsNullOrEmpty(requestedDeviceName))
            InitializeWebcamTextureWithDeviceName();

        if (webcamTexture == null)
            InitializeWebcamTextureWithDefaultDevice();
    }

    private void SetDefaultValuesForWebcamSize()
    {
        Debug.Log("WebcamTexture width and height must be greater than 100.");
        webcamRequestedWidth = defaultWidth;
        webcamRequestedHeight = defaultHeight;
    }

    private void InitializeWebcamTextureWithDeviceName()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].name == requestedDeviceName)
            {
                webcamTexture = new WebCamTexture(requestedDeviceName, webcamRequestedWidth, webcamRequestedHeight);
                deviceIndex = i;
                break;
            }
        }
    }

    private void InitializeWebcamTextureWithDefaultDevice()
    {
        webcamTexture = new WebCamTexture(webcamRequestedWidth, webcamRequestedHeight);
        deviceIndex = 0;
    }

    private IEnumerator WaitForWebcamToInitialize()
    {
        while (webcamTexture.width < 100)
        {
            yield return new WaitForEndOfFrame();
        }
    }

    public void ChangeWebcamTextureToNextAvailable()
    {        
        string nextWebcamDeviceName = GetNextWebCamDevice().name;

        webcamTexture.Stop();

        webcamTexture = new WebCamTexture(nextWebcamDeviceName, webcamRequestedWidth, webcamRequestedHeight);

        webcamTexture.Play();

        StartCoroutine(WaitForWebcamToInitialize());

        if (webcamTexture.width != WebcamWidth || webcamTexture.height != WebcamHeight)
            ReinitializeWebcamWithPreviousValues();

        Debug.Log("Webcam width: " + webcamTexture.width + ". Webcam height: " + webcamTexture.height + ". Webcam device name: " + nextWebcamDeviceName);
    }

    private WebCamDevice GetNextWebCamDevice()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        WebCamDevice nextDevice;

        int i = deviceIndex + 1;

        if (i >= devices.Length)
            i = 0;

        nextDevice = devices[i];
        deviceIndex = i;

        return nextDevice;
    }

    //Program does not support dynamically changing the size of the webcam
    //Reverts webcam to previous webcam with original width and height
    private void ReinitializeWebcamWithPreviousValues()
    {
        Debug.Log("New WebcamTexture dimensions must match old WebcamTexture dimensions.");

        webcamRequestedWidth = WebcamWidth;
        webcamRequestedHeight = WebcamHeight;

        InitializeWebcamTexture();

        webcamTexture.Play();

        StartCoroutine(WaitForWebcamToInitialize());        
    }

    public bool DidUpdateThisFrame()
    {
        return (initialized) ? webcamTexture.didUpdateThisFrame : false;
    }
}
