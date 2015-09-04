using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;
public class KinectOneViewer : BaseUserDepthViewer {
#if !UNITY_ANDROID
    Texture2D texture;
    public Renderer target;
    KinectOneEngageSingleUser m_engageuser;
    Kinect.CoordinateMapper _mapper;
    KinectOneDepth depthInfo;
    void Start()
    {
        KinectOneInput input = ((KinectOneInput)KinectOneInput.Instance);
        _mapper = input.GetMapper();
        depthInfo = input.GetDepthSensor();
        textureSize = new ResolutionData(depthInfo.Sensor.DepthFrameSource.FrameDescription.Width, depthInfo.Sensor.DepthFrameSource.FrameDescription.Height);
        texture = new Texture2D(depthInfo.Sensor.DepthFrameSource.FrameDescription.Width, depthInfo.Sensor.DepthFrameSource.FrameDescription.Height,TextureFormat.RGBA32,false);
        texture.wrapMode = TextureWrapMode.Clamp;
        depthHistogramMap = new float[depthInfo.Sensor.DepthFrameSource.DepthMaxReliableDistance];
        depthToColor = new Color32[depthInfo.Sensor.DepthFrameSource.DepthMaxReliableDistance];
        outputPixels = new Color32[textureSize.Width * textureSize.Height];

        if (null != target)
        {
            target.material.mainTexture = texture;
        }
        m_engageuser = this.GetComponent<KinectOneEngageSingleUser>();
        CalImage2Screen(DepthImagePos.LeftMain);
    }
    void KinectOne_Update(KinectOneInput kinectOneInput)
    {
        if (IsActivate)
        {
            UpdateHistogram((KinectOneDepth)KinectOneInput.Depth);
            ZigTrackedUserCenter((KinectOneLabelMap)KinectOneInput.LabelMap);
            UpdateTexture((KinectOneDepth)KinectOneInput.Depth, (KinectOneLabelMap)KinectOneInput.LabelMap, (KinectOneImage)KinectOneInput.Image);
        }
    }
    void UpdateTexture(KinectOneDepth depth, KinectOneLabelMap labelmap, KinectOneImage image)
    {
        int trackedUserId = -1;
        if (m_engageuser != null && m_engageuser.engagedTrackedUser != null)
            trackedUserId = m_engageuser.engagedTrackedUser.Id;
        if (trackedUserId == -1)
        {
            GetNoTrackingTexture(depth);
        }
        else
        {
            Kinect.ColorSpacePoint[] colorSpacePoints = new Kinect.ColorSpacePoint[depth.xres * depth.yres];
            _mapper.MapDepthFrameToColorSpace(depth.GetData(), colorSpacePoints);
            GetRemoveBackground(labelmap, image, colorSpacePoints);
        }
        texture.SetPixels32(outputPixels);
        texture.Apply();
    }
    void GetRemoveBackground(KinectOneLabelMap labelmap, KinectOneImage image, Kinect.ColorSpacePoint[] colorSpaces)
    {
        byte[] data = labelmap.GetData();
        Color32[] _image = image.GetColorTexture().GetPixels32();
        int imageWidth = image.Sensor.ColorFrameSource.FrameDescription.Width;
        for (int i = 0; i < outputPixels.Length; i++)
        {
            byte indexValue = data[i];
            if (IsValidFloatValue(colorSpaces[i].X) || IsValidFloatValue(colorSpaces[i].Y) || indexValue == 255)
            {
                outputPixels[i] = new Color32(0, 0, 0, 0);
            }
            else
            {
                float x=colorSpaces[i].X, y=colorSpaces[i].Y;
                int colorIndex = (int)y * imageWidth + (int)x;
                outputPixels[i] = (indexValue > 0) ? ((indexValue <= labelToColor.Length) ? labelToColor[indexValue - 1] : defaultColor) : bgColor;
                if (indexValue == m_engageuser.engagedTrackedUser.Id && colorIndex < _image.Length && colorIndex > 0)
                {
                    outputPixels[i] = _image[colorIndex];
                }
            }
        }
    }
    bool IsValidFloatValue(float val)
    {
        return (float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val) || float.IsNaN(val) || float.IsInfinity(val));
    }
    void GetNoTrackingTexture(KinectOneDepth depth)
    {
        ushort[] data = depth.GetData();
        Debug.Log(outputPixels.Length);
        for (int i = 0; i < outputPixels.Length; i++)
        {
            int depthValue = Mathf.Clamp(data[i], 0, depthToColor.Length - 1);
            outputPixels[i] = depthToColor[depthValue];
        }
    }
    //Get Tracked user's index
    short trackedUserIndex = 255;
    void ZigTrackedUserCenter(KinectOneLabelMap labelmap)
    {
        if (m_engageuser == null || m_engageuser.engagedTrackedUser == null)
        {
            trackedUserIndex = 255;
            return;
        }
        ZigTrackedUser _engagedUser = m_engageuser.engagedTrackedUser;
        Kinect.CameraSpacePoint positionPoint = new Kinect.CameraSpacePoint();
        positionPoint.X = _engagedUser.Position.x;
        positionPoint.Y = _engagedUser.Position.y;
        positionPoint.Z = _engagedUser.Position.z;
        Kinect.DepthSpacePoint depthSpacePoint = _mapper.MapCameraPointToDepthSpace(positionPoint);
        short[] labelData = labelmap.data;
        int index = (int)(depthSpacePoint.Y * depthInfo.xres + depthSpacePoint.X);
        trackedUserIndex = (index >= 0 && index < labelData.Length) ? labelData[index] : (short)255;
    }
    void UpdateHistogram(KinectOneDepth depth)
    {
        int i, numOfPoints = 0;

        System.Array.Clear(depthHistogramMap, 0, depthHistogramMap.Length);
        short[] rawDepthMap = depth.data;

        int depthIndex = 0;
        // assume only downscaling
        // calculate the amount of source pixels to move per column and row in
        // output pixels
        int factorX = depth.xres / textureSize.Width;
        int factorY = ((depth.yres / textureSize.Height) - 1) * depth.xres;
        for (int y = 0; y < textureSize.Height; ++y, depthIndex += factorY)
        {
            for (int x = 0; x < textureSize.Width; ++x, depthIndex += factorX)
            {
                int pixel = rawDepthMap[depthIndex];
                pixel=Mathf.Clamp(pixel, 0, depthHistogramMap.Length-1);
                if (pixel != 0)
                {
                    depthHistogramMap[pixel]++;
                    numOfPoints++;
                }
            }
        }
        depthHistogramMap[0] = 0;
        if (numOfPoints > 0)
        {
            for (i = 1; i < depthHistogramMap.Length; i++)
            {
                depthHistogramMap[i] += depthHistogramMap[i - 1];
            }
            depthToColor[0] = Color.black;
            for (i = 1; i < depthHistogramMap.Length; i++)
            {
                float intensity = (1.0f - (depthHistogramMap[i] / numOfPoints));
                //depthHistogramMap[i] = intensity * 255;
                depthToColor[i].r = (byte)(BaseColor.r * intensity);
                depthToColor[i].g = (byte)(BaseColor.g * intensity);
                depthToColor[i].b = (byte)(BaseColor.b * intensity);
                depthToColor[i].a = 255;//(byte)(BaseColor.a * intensity);
            }
        }


    }
    void OnGUI()
    {
        if (null == target && IsActivate)
        {
            //GUI.DrawTexture(new Rect(Screen.width - 2*texture.width - 10, Screen.height - 2*texture.height - 10, 2*texture.width, 2*texture.height), texture);
            GUI.DrawTexture(new Rect(baseX, baseY + imgHeight, imgWidth, -imgHeight), texture);
        }
    }
#endif
}
