using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;

public class KinectOneInput : ZigInput
{
    private static ZigInputKinectOne _kinectOne;
    private static KinectOneInput _instance;
    public static ZigInputType InputType;
    public IZigInputReader reader;
    public static ZigInputSettings Settings;
    void Awake()
    {
        _instance = this;
        Debug.Log(_instance);
        _kinectOne = new ZigInputKinectOne();
        _kinectOne.Init(Settings);
        Depth = _kinectOne.Depth;
        Image = _kinectOne.Image;
        LabelMap = _kinectOne.LabelMap;
        List<ZigInputUser> inputUsers = new List<ZigInputUser>();
        NewUsersFrameEventArgs frameEventArgs = new NewUsersFrameEventArgs(inputUsers);
        _kinectOne.NewUsersFrame += new System.EventHandler<NewUsersFrameEventArgs>(OnNewUsersFrame);
    }
    void Start()
    {
    }
    private void OnNewUsersFrame(object sender, NewUsersFrameEventArgs e)
    {
        List<ZigInputUser> users = e.Users;
        //parse users and do something
    }
    public void KinectOne_UpdateUser()
    {
        List<ZigInputUser> listUsers = new List<ZigInputUser>();
        _kinectOne.DoSomething(listUsers);
    }
    public static ZigDepth Depth { get; private set; }
    public static ZigImage Image { get; private set; }
    public static ZigLabelMap LabelMap { get; private set; }

    public static ZigInput Instance { get { return _instance; } }

    public Dictionary<int, ZigTrackedUser> TrackedUsers { get; private set; }

    public void AddListener(GameObject listener) { }
    public static Vector3 ConvertImageToWorldSpace(Vector3 imagePosition)
    {
        return _kinectOne.ConvertImageToWorldSpace(imagePosition);
    }
    public static Vector3 ConvertWorldToImageSpace(Vector3 worldPosition)
    {
        return _kinectOne.ConvertWorldToImageSpace(worldPosition);
    }
    public ZigInputKinectOne getKinectOne() { return _kinectOne; }
    public void UpdateMaps()
    {
        _kinectOne.Update();
        SendMessage("KinectOne_Update", _instance);
    }
    void Update()
    {
    }
    void LateUpdate()
    {
        UpdateMaps();
    }
    public KinectOneDepth GetDepthSensor()
    {
        return (KinectOneDepth)_kinectOne.Depth;
    }
    public KinectOneImage GetImageSensor()
    {
        return (KinectOneImage)_kinectOne.Image;
    }
    public KinectOneLabelMap GetLabelMapSensor()
    {
        return (KinectOneLabelMap)_kinectOne.LabelMap;
    }
    public KinectSensor GetSensor()
    {
        return _kinectOne.Sensor;
    }
    public CoordinateMapper GetMapper()
    {
        return _kinectOne.Mapper;
    }
}