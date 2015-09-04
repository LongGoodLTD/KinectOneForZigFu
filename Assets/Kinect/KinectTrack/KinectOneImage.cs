using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class KinectOneImage : ZigImage 
{
    public int xres { get; private set; }
    public int yres { get; private set; }

    private KinectSensor _Sensor;
    public KinectSensor Sensor { get { return _Sensor; } }
    private ColorFrameReader _Reader;
    private Texture2D _Texture;
    private byte[] _Data;
    public Color32[] data
    {
        get
        {
            Color32[] _data = new Color32[_Data.Length / 4];
            for (var i = 0; i < _Data.Length; i += 4)
            {
                Color32 color = new Color32(_Data[i + 0], _Data[i + 1], _Data[i + 2], _Data[i + 3]);
                _data[i / 4] = color;
            }
            return _data;
        }
    }
    public Texture2D GetColorTexture()
    {
        return _Texture;
    }
    //初始化Sensor，開啟彩色畫面的讀取器，建立新的影像編碼描述。
    public KinectOneImage()
        : base(0, 0)
    {
        _Sensor = KinectSensor.GetDefault();
        Initialize();
    }
    public KinectOneImage(KinectSensor sensor)
        : base(0, 0)
    {
        _Sensor = sensor;
        Initialize();
    }
    void Initialize()
    {
        if (_Sensor != null)
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();

            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            xres = frameDesc.Width;
            yres = frameDesc.Height;
            //建立貼圖物件，建立像素儲存的陣列。
            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            _Data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];
            //若Sensor還沒開起，將Sensor開啟。
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }
    public void UpdateImage () 
    {

        if (_Reader != null) 
        {
			//取得當下讀取到的彩色畫面。
            var frame = _Reader.AcquireLatestFrame();
			//每次Update都會依照其編碼的方式將資訊寫入陣列資料之中。
			if (frame != null)
            {
				frame.CopyConvertedFrameDataToArray(_Data, ColorImageFormat.Rgba);
				_Texture.LoadRawTextureData(_Data);
				_Texture.Apply();
                
                frame.Dispose();
                frame = null;
            }
        }
    }
	//關閉的時候會將Sensor關閉。
    public  void Shutdown()
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
