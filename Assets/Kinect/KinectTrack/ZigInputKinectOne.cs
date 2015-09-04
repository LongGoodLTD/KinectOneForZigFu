using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

public class ZigInputKinectOne : IZigInputReader
{
    private ZigDepth _depth=null;
    private ZigImage _image=null;
    private ZigLabelMap _labelMap=null;
    private KinectSensor _sensor = null;
    private CoordinateMapper _mapper = null;
    public ZigInputKinectOne()
    {
        _sensor = KinectSensor.GetDefault();
        _mapper = _sensor.CoordinateMapper;
        _depth = new KinectOneDepth(_sensor);
        _image = new KinectOneImage(_sensor);
        _labelMap = new KinectOneLabelMap(_sensor);
    }
    public bool AlignDepthToRGB { get; set; }
    public ZigDepth Depth { get { return _depth; } }
    public ZigImage Image { get { return _image; } }
    public ZigLabelMap LabelMap { get { return _labelMap; } }
    public KinectSensor Sensor { get { return _sensor; } }
    public CoordinateMapper Mapper { get { return _mapper; } }
    public event EventHandler<NewUsersFrameEventArgs> NewUsersFrame;

    public void DoSomething(List<ZigInputUser> inputUsers)
    {
        NewUsersFrameEventArgs _eventArgs = new NewUsersFrameEventArgs(inputUsers);
        OnNewUsersFrameResched(_eventArgs);
    }
    protected virtual void OnNewUsersFrameResched(NewUsersFrameEventArgs args)
    {
        EventHandler<NewUsersFrameEventArgs> handlers = NewUsersFrame;
        if (handlers != null)
        {
            handlers(this, args);
        }
    }
    public Vector3 ConvertImageToWorldSpace(Vector3 imagePosition)
    {
        return Vector3.zero;
    }
    public Vector3 ConvertWorldToImageSpace(Vector3 worldPosition)
    {
        return Vector3.zero;
    }
    public void Init(ZigInputSettings settings)
    {

    }
    public void Shutdown()
    {
        if (_depth != null) ((KinectOneDepth)_depth).Shutdown();
        if (_image != null) ((KinectOneImage)_image).Shutdown();
        if (_labelMap != null) ((KinectOneLabelMap)_labelMap).Shutdown();
    }
    public void Update()
    {
        if (_depth != null) ((KinectOneDepth)_depth).UpdateDepth();
        if (_image != null) ((KinectOneImage)_image).UpdateImage();
        if (_labelMap != null) ((KinectOneLabelMap)_labelMap).UpdateLabelMap();
    }

}

