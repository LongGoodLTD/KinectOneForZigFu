using UnityEngine;
using System.Collections;
using Windows.Kinect;

//讀取深度影像資訊。
public class KinectOneDepth : ZigDepth
{

    public short[] data
    {
        get
        {
            short[] _data = new short[_Data.Length];
            for (int i = 0; i < _Data.Length; i++) _data[i] = (short)_Data[i];
            return _data;
        }
    }
    public int xres;
    public int yres;

	//宣告基本變數。
    private KinectSensor _Sensor;
    public KinectSensor Sensor { get { return _Sensor; } }
    private DepthFrameReader _Reader;
    private ushort[] _Data;
	//取得數值的Get Function。
    public ushort[] GetData()
    {
        return _Data;
    }
	//KINECT開機與初始化，開啟深度影像Reader。
    public KinectOneDepth() : base(0,0)
    {
        _Sensor = KinectSensor.GetDefault();
        Initialize();
    }
    public KinectOneDepth(KinectSensor sensor)
        : base(0, 0)
    {
        _Sensor = sensor;
        Initialize();
    }
    void Initialize()
    {
        if (_Sensor != null)
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();
            //讀取深度畫面的Pixels總數，定義為陣列型態。
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
            xres = _Sensor.DepthFrameSource.FrameDescription.Width;
            yres = _Sensor.DepthFrameSource.FrameDescription.Height;
            base.xres = xres;
            base.yres = yres;
        }
    }
	//每個Updata讀取每個畫面的資訊儲存到DATA陣列之中。
	public void UpdateDepth () 
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
    public void Shutdown()
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
