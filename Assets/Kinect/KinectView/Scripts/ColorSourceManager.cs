using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceManager : MonoBehaviour 
{
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }

    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    private Texture2D _Texture;
    private byte[] _Data;
    
    public Texture2D GetColorTexture()
    {
        return _Texture;
    }
	//初始化Sensor，開啟彩色畫面的讀取器，建立新的影像編碼描述。
	void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null) 
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();
            
            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;
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
    
    void Update () 
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
