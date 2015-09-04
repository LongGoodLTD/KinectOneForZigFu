### Introducion 

Hi Everyone,

We are LongGood Ltd,. We are dedicated to motion-sensing rehabilitation.

Unfortunately, Kinect360 has been discontinued, and KinectOne is higher quality than Xtion and Kinect. So we decide to use KinectOne to be one of our choice. After studying Zig's structure, We implement an plugin named KinectOneForZigFu. We share it to who use ZigFu and want to integrate KinectOne.

### Requirement

* [ZigFu](http://zigfu.com/en/)
* [KinectOne Driver](http://www.microsoft.com/en-us/download/details.aspx?id=44561)
* [Kinect V2 Unity Pro Packages](https://go.microsoft.com/fwlink/p/?LinkId=513177)

All other requirement about KinectOne you have to satisfy.

### Modification

###### First of all, you have to modify zig.cs.

1. Add KinectOneController GameObject

```csharp
    public GameObject kinectOneObject;
```

2. Modify Awake()

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
3. Modify Start()

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
4. Add following functions

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

![HierarchyView](https://octodex.github.com/raw/HierarchyView.png)

###### Third, Set KinectOneController to ZigFu GameObject:

![ZigFu_Setup](https://octodex.github.com/raw/ZigFu_Setup.png)

###### Finally, Set Zigfu and Blockman user to  KinectOneToZig:

![KinectOneToZig_InspectorView](https://octodex.github.com/raw/KinectOneToZig_InspectorView.png)

Okay, done.

Plug in your KinectOne and OnGui will show the depth view or removal background color image depends on whether user is tracked or not.