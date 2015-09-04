using UnityEngine;
using System.Collections;

public class KinectOneController : MonoBehaviour {
#if !UNITY_ANDROID
    [HideInInspector]
    public static ZigInputKinectOne _kinectOne;
    void Awake()
    {
        DontDestroyOnLoad(this);
        Debug.Log("KinectOneController Awake()");
//        SysCont.SP.SetZigFuToKinectOne();
    }
    void Start()
    {
    }
#endif
}
