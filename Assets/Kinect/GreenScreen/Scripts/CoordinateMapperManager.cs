using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Runtime.InteropServices;
using System;

public class CoordinateMapperManager : MonoBehaviour
{
	//基本參數賦予，KinectSensor、座標對應變數、多資源讀取器、深度資訊。
	private KinectSensor m_pKinectSensor;
	private CoordinateMapper m_pCoordinateMapper;
	private MultiSourceFrameReader m_pMultiSourceFrameReader;
	private DepthSpacePoint[] m_pDepthCoordinates;
	//儲存彩色資料與身體資料的緩衝區。
	private byte[] pColorBuffer;
	private byte[] pBodyIndexBuffer;
	//儲存深度資料的緩衝區。
	private ushort[] pDepthBuffer;
	//不同影像讀取的解析度設置。
	const int        cDepthWidth  = 512;
	const int        cDepthHeight = 424;
	const int        cColorWidth  = 1920;
	const int        cColorHeight = 1080;

	long frameCount = 0;

	double elapsedCounter = 0.0;
	double fps = 0.0;
	//彩色影像貼圖。
	Texture2D m_pColorRGBX;

	bool nullFrame = false;

	void Start()
	{
		//定義陣列變數大小。
		pColorBuffer = new byte[cColorWidth * cColorHeight * 4];
		pBodyIndexBuffer = new byte[cDepthWidth * cDepthHeight];
		pDepthBuffer = new ushort[cDepthWidth * cDepthHeight];
		//定義貼圖模式。
		m_pColorRGBX = new Texture2D (cColorWidth, cColorHeight, TextureFormat.RGBA32, false);

		m_pDepthCoordinates = new DepthSpacePoint[cColorWidth * cColorHeight];
		//呼叫Kinect初始化的Function。
		InitializeDefaultSensor ();
	}
	//OnGUI尺寸。
	Rect fpsRect = new Rect(10, 10, 200, 30);
	Rect nullFrameRect = new Rect(10, 50, 200, 30);
	//顯示現在有沒有讀取到KINECT以及FPS數量。
	void OnGUI () 
	{
		GUI.Box (fpsRect, "FPS: " + fps.ToString("0.00"));

		if (nullFrame)
		{
			GUI.Box (nullFrameRect, "NULL MSFR Frame");
		}
	}
	//讀取彩色貼圖。
	public Texture2D GetColorTexture()
	{
		return m_pColorRGBX;
	}
	//讀取影響屬於玩家或不屬於玩家之變化。
	public byte[] GetBodyIndexBuffer()
	{
		return pBodyIndexBuffer;
	}
	//讀取深度影像座標。
	public DepthSpacePoint[] GetDepthCoordinates()
	{
		return m_pDepthCoordinates;
	}
	//初始化Kinect、開啟多重內容讀取器。
	void InitializeDefaultSensor()
	{	
		m_pKinectSensor = KinectSensor.GetDefault();
		
		if (m_pKinectSensor != null)
		{
			// Initialize the Kinect and get coordinate mapper and the frame reader
			m_pCoordinateMapper = m_pKinectSensor.CoordinateMapper;
			
			m_pKinectSensor.Open();
			if (m_pKinectSensor.IsOpen)
			{
				m_pMultiSourceFrameReader = m_pKinectSensor.OpenMultiSourceFrameReader(
					FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
			}
		}
		
		if (m_pKinectSensor == null)
		{
			UnityEngine.Debug.LogError("No ready Kinect found!");
		}
	}
	//將不同解析度對照同步，讓不同的讀取內容可以互相配對。
	void ProcessFrame()
	{
		//此段和下句可以達成同樣的效果，不過因為此段要使用IntPtr的關係與記憶體的釋放控制，下句則沒有這樣子的效果。
		//m_pCoordinateMapper.MapColorFrameToDepthSpace(pDepthBuffer,m_pDepthCoordinates);

		var pDepthData = GCHandle.Alloc(pDepthBuffer, GCHandleType.Pinned);
		var pDepthCoordinatesData = GCHandle.Alloc(m_pDepthCoordinates, GCHandleType.Pinned);

		m_pCoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
			pDepthData.AddrOfPinnedObject(), 
			(uint)pDepthBuffer.Length * sizeof(ushort),
			pDepthCoordinatesData.AddrOfPinnedObject(), 
			(uint)m_pDepthCoordinates.Length);

		pDepthCoordinatesData.Free();
		pDepthData.Free();

		m_pColorRGBX.LoadRawTextureData(pColorBuffer);
		m_pColorRGBX.Apply ();
	}
	
	
	void Update()
	{
		// Get FPS
		//計算當elapsedCounter被加到 1 的時候，總共會經過幾個畫面，以此得知FPS。
		elapsedCounter+=Time.deltaTime;
		if(elapsedCounter > 1.0)
		{
			fps = frameCount / elapsedCounter;
			frameCount = 0;
			elapsedCounter = 0.0;
		}
		//如果多資源讀取器沒有啟動，則跳離。
		if (m_pMultiSourceFrameReader == null) 
		{
			return;
		}
		//多資源讀取器讀取當前資訊，然後再使用using的方式讀取當前的深度、彩色、身體確認資訊，讓資源可以在讀取完後自動釋放。
		var pMultiSourceFrame = m_pMultiSourceFrameReader.AcquireLatestFrame();
		if (pMultiSourceFrame != null) 
		{
			frameCount++;
			nullFrame = false;

			using(var pDepthFrame = pMultiSourceFrame.DepthFrameReference.AcquireFrame())
			{
				using(var pColorFrame = pMultiSourceFrame.ColorFrameReference.AcquireFrame())
				{
					using(var pBodyIndexFrame = pMultiSourceFrame.BodyIndexFrameReference.AcquireFrame())
					{
						// Get Depth Frame Data.
						if (pDepthFrame != null)
						{
							var pDepthData = GCHandle.Alloc (pDepthBuffer, GCHandleType.Pinned);
							pDepthFrame.CopyFrameDataToIntPtr(pDepthData.AddrOfPinnedObject(), (uint)pDepthBuffer.Length * sizeof(ushort));
							pDepthData.Free();
						}
						
						// Get Color Frame Data
						if (pColorFrame != null)
						{
							var pColorData = GCHandle.Alloc (pColorBuffer, GCHandleType.Pinned);
							pColorFrame.CopyConvertedFrameDataToIntPtr(pColorData.AddrOfPinnedObject(), (uint)pColorBuffer.Length, ColorImageFormat.Rgba);
                            pColorData.Free();
                        }
                        
                        // Get BodyIndex Frame Data.
                        if (pBodyIndexFrame != null)
                        {
							var pBodyIndexData = GCHandle.Alloc (pBodyIndexBuffer, GCHandleType.Pinned);
							pBodyIndexFrame.CopyFrameDataToIntPtr(pBodyIndexData.AddrOfPinnedObject(), (uint)pBodyIndexBuffer.Length);
							pBodyIndexData.Free();
                        }
					}
				}
			}

			ProcessFrame();
        }
        else
		{
			nullFrame = true;
		}
	}
	//結束的時候關閉KINECT。
	void OnApplicationQuit()
	{
		pDepthBuffer = null;
		pColorBuffer = null;
		pBodyIndexBuffer = null;

		if (m_pDepthCoordinates != null)
		{
			m_pDepthCoordinates = null;
		}

		if (m_pMultiSourceFrameReader != null)
		{
			m_pMultiSourceFrameReader.Dispose();
			m_pMultiSourceFrameReader = null;
		}
		
		if (m_pKinectSensor != null)
		{
			m_pKinectSensor.Close();
			m_pKinectSensor = null;
		}
	}
}

