using UnityEngine;
using System.Collections;
using Windows.Kinect;

//讀取BodyIndex影像資訊。
public class KinectOneLabelMap : ZigLabelMap
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
    private BodyIndexFrameReader _Reader;
    private byte[] _Data;
	//取得數值的Get Function。
    public byte[] GetData()
    {
        return _Data;
    }
    //KINECT開機與初始化，開啟BodyIndez影像Reader。
    public KinectOneLabelMap()
        : base(0, 0)
    {
        _Sensor = KinectSensor.GetDefault();
        Initialize();
    }
    public KinectOneLabelMap(KinectSensor sensor)
        : base(0, 0)
    {
        _Sensor = sensor;
        Initialize();
    }
    void Initialize()
    {

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyIndexFrameSource.OpenReader();
            //讀取深度畫面的Pixels總數，定義為陣列型態。
            _Data = new byte[_Sensor.BodyIndexFrameSource.FrameDescription.LengthInPixels];
            xres = _Sensor.BodyIndexFrameSource.FrameDescription.Width;
            yres = _Sensor.BodyIndexFrameSource.FrameDescription.Height;

        }
    }
	//每個Updata讀取每個畫面的資訊儲存到DATA陣列之中。
	public void UpdateLabelMap () 
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
