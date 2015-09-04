### Introducion 

Hi Everyone,

We are [LongGood Ltd](http://www.rehabfun.com/),. We are dedicated to motion-sensing rehabilitation.

Kinect360 has been discontinued and KinectOne is higher quality than Xtion and Kinect. So we decide to use KinectOne to be one of our choice. After studying Zig's structure, We implement a plugin named KinectOneForZigFu. Now, we share it to who use ZigFu and want to integrate KinectOne.

### Requirement

* [ZigFu](http://zigfu.com/en/)
* [KinectOne Driver](http://www.microsoft.com/en-us/download/details.aspx?id=44561)
* [Kinect V2 Unity Pro Packages](https://go.microsoft.com/fwlink/p/?LinkId=513177)

All other requirement about KinectOne you have to satisfy.

### Modification

###### First of all, you have to modify zig.cs.

* Add KinectOneController GameObject

```csharp
    public GameObject kinectOneObject;
```

* Modify Awake()

```csharp

    void Awake()
    {

#if UNITY_WEBPLAYER
#if UNITY_EDITOR
        Debug.LogError("Depth camera input will not work in editor when target platform is Webplayer. Please change target platform to PC/Mac standalone.");
        return;
#endif
#endif

        DontDestroyOnLoad(this);
    }
```
* Modify Start()

```csharp
    void Start()
    {
        ZigInput.Settings = settings;
        if (inputType == ZigInputType.Auto || inputType == ZigInputType.OpenNI2)
        {
            IntPtr iPtr = IntPtr.Zero;
            int pNumber = 0;
            OpenNI2.OpenNI2Wrapper.OniStatus oniStatus = OpenNI2.OpenNI2Wrapper.oniGetDeviceList(ref iPtr, ref pNumber);
            if (pNumber == 0)
            {
                print("No OpenNI2!!!!");
                if (StartOpenNiORKinectSDK(ZigInputType.OpenNI) == false && StartOpenNiORKinectSDK(ZigInputType.KinectSDK) == false)
                {
                    print("No Device!!!!");
                    OpenKinectOne();
                }
                return;
            }
        }
        if (StartOpenNiORKinectSDK(inputType) == false)
        {
            print("No Device!!!!");
        }

    }
```
* Add following functions

```csharp
    bool StartOpenNiORKinectSDK(ZigInputType _inputType)
    {
        ZigInput.InputType = _inputType;
        try
        {
            if (ZigInput.Instance.ReaderInited == true)
                ZigInput.Instance.AddListener(gameObject);
            print(ZigInput.Instance.GetGestures().ToString());
            return true;
        }
        catch
        {
            print("Exception!! Can not open " + _inputType);
            return false;
        }
    }
    void OpenKinectOne()
    {
        try
        {
            kinectOneObject.SetActive(true);
        }
        catch
        {
            print("Setup KinectOne failed.");
        }
    }
```

###### Second, add KinectOneController prefabs to Hierarchy:

![HierarchyView](https://raw.githubusercontent.com/LongGoodLTD/KinectOneForZigFu/master/raw/HierarchyView.png)

###### Third, Set KinectOneController to ZigFu GameObject:

![ZigFu_Setup](https://raw.githubusercontent.com/LongGoodLTD/KinectOneForZigFu/master/raw/ZigFu_Setup.png)

###### Finally, Set Zigfu and Blockman user to  KinectOneToZig:

![KinectOneToZig_InspectorView](https://raw.githubusercontent.com/LongGoodLTD/KinectOneForZigFu/master/raw/KinectOneToZig_InspectorView.png)

Okay, done.

Plug in your KinectOne and OnGui will show the depth view or removal background color image depends on whether user is tracked or not.

### Contributing

1. Fork it ( https://github.com/LongGoodLTD/KinectOneForZigFu/fork )
2. Create your feature branch (`git checkout -b my-new-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin my-new-feature`)
5. Create a new Pull Request
