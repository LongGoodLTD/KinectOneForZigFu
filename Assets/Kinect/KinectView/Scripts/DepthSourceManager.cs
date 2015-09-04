using UnityEngine;
using System.Collections;
using Windows.Kinect;

//讀取深度影像資訊。
public class DepthSourceManager : MonoBehaviour
{   
	//宣告基本變數。
    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private ushort[] _Data;
	//取得數值的Get Function。
    public ushort[] GetData()
    {
        return _Data;
    }
	//KINECT開機與初始化，開啟深度影像Reader。
    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null) 
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();
			//讀取深度畫面的Pixels總數，定義為陣列型態。
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }
    }
    
	//每個Updata讀取每個畫面的資訊儲存到DATA陣列之中。
	void Update () 
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_Data);
                frame.Dispose();
                frame = null;
            }
        }
    }
	//結束的時候關閉KINECT。
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}
