using UnityEngine;
using System.Collections;
using Windows.Kinect;

public enum DepthViewMode
{
    SeparateSourceReaders,
    MultiSourceReader,
}
//將讀取到的深度影像資料顯示出來，分為多組來源與單組來源兩種方式，各自由不同的Script傳入。
//建立一個Mesh，作為畫面總數的壓縮，Mesh點的數量為所有Pixel的十六分之一，因此還需要計算將所有的Pixel運算平均值，再套用到完整的畫面之中。
public class DepthSourceView : MonoBehaviour
{
	//基本模式設定為單組Kinect模式。
    public DepthViewMode ViewMode = DepthViewMode.SeparateSourceReaders;
	//單來源的彩色影像物件、深度影像物件以及多組來源物件。
    public GameObject ColorSourceManager;
    public GameObject DepthSourceManager;
    public GameObject MultiSourceManager;
	//宣告基本變數。
    private KinectSensor _Sensor;
    private CoordinateMapper _Mapper;
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Vector2[] _UV;
    private int[] _Triangles;
    
	//壓縮成十六分之一的話，長與高都要壓縮四分之一。
    // Only works at 4 right now
    private const int _DownsampleSize = 4;
	//深度的縮放比率。
    private const double _DepthScale = 0.1f;
	//旋轉物件的速度。
    private const int _Speed = 50;
	//單來源的彩色影像物件、深度影像物件以及多組來源物件的目標Script。
    private MultiSourceManager _MultiManager;
    private ColorSourceManager _ColorManager;
    private DepthSourceManager _DepthManager;

	//KINECT開機與初始化，建立要呈現深度畫面的Mesh。
    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Mapper = _Sensor.CoordinateMapper;
            var frameDesc = _Sensor.DepthFrameSource.FrameDescription;

			//降低其解析度，壓縮每個邊四分之一的大小。
            // Downsample to lower resolution
            CreateMesh(frameDesc.Width / _DownsampleSize, frameDesc.Height / _DownsampleSize);

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }
	//建立Mesh的Function。
    void CreateMesh(int width, int height)
    {
		//建立Mesh所需要的基本參數。
        _Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[width * height];
        _UV = new Vector2[width * height];
        _Triangles = new int[6 * ((width - 1) * (height - 1))];

		//計算三角面的配置。
        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                _Vertices[index] = new Vector3(x, -y, 0);
                _UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + width;
                    int bottomRight = bottomLeft + 1;

                    _Triangles[triangleIndex++] = topLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
    }
	//介面UI顯示目前使用的是單組資源配置還是多組資源配置。
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
        GUI.TextField(new Rect(Screen.width - 250 , 10, 250, 20), "DepthMode: " + ViewMode.ToString());
        GUI.EndGroup();
    }

    void Update()
    {
        if (_Sensor == null)
        {
            return;
        }
		//點擊滑鼠右鍵切換單組資源配置或是多組資源配置。
        if (Input.GetButtonDown("Fire1"))
        {
            if(ViewMode == DepthViewMode.MultiSourceReader)
            {
                ViewMode = DepthViewMode.SeparateSourceReaders;
            }
            else
            {
                ViewMode = DepthViewMode.MultiSourceReader;
            }
        }
		//使用上下左右鍵旋轉Mesh物件。
        float yVal = Input.GetAxis("Horizontal");
        float xVal = -Input.GetAxis("Vertical");

        transform.Rotate(
            (xVal * Time.deltaTime * _Speed), 
            (yVal * Time.deltaTime * _Speed), 
            0, 
            Space.Self);
		//如果切換到了單組資源配置的執行工作。   
        if (ViewMode == DepthViewMode.SeparateSourceReaders)
        {
            if (ColorSourceManager == null)
            {
                return;
            }
            
            _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
            if (_ColorManager == null)
            {
                return;
            }
            
            if (DepthSourceManager == null)
            {
                return;
            }
            
            _DepthManager = DepthSourceManager.GetComponent<DepthSourceManager>();
            if (_DepthManager == null)
            {
                return;
            }
            
            gameObject.renderer.material.mainTexture = _ColorManager.GetColorTexture();
            RefreshData(_DepthManager.GetData(),
                _ColorManager.ColorWidth,
                _ColorManager.ColorHeight);
        }
		else//如果切換到了多組資源配置的執行工作。
        {
            if (MultiSourceManager == null)
            {
                return;
            }
            
            _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
            if (_MultiManager == null)
            {
                return;
            }
            
            gameObject.renderer.material.mainTexture = _MultiManager.GetColorTexture();
            
            RefreshData(_MultiManager.GetDepthData(),
                        _MultiManager.ColorWidth,
                        _MultiManager.ColorHeight);
        }
    }
	//透過不同模式所傳入的資料來源將從不同的Script匯入該Function中，去運算每次Update中所得到的Mesh。
    private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight)
    {
		Debug.Log("W = " + colorWidth);
		Debug.Log("H = " + colorHeight);
        var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
        
        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
		Debug.Log("Old " + depthData.Length);
        _Mapper.MapDepthFrameToColorSpace(depthData, colorSpace);
		Debug.Log("New " + depthData.Length);
        
        for (int y = 0; y < frameDesc.Height; y += _DownsampleSize)
        {
            for (int x = 0; x < frameDesc.Width; x += _DownsampleSize)
            {
				int indexX = x / _DownsampleSize;//Mesh的X軸點數。
				int indexY = y / _DownsampleSize;//Mesh的Y軸點數。
				int smallIndex = (indexY * (frameDesc.Width / _DownsampleSize)) + indexX; //Mesh的所有點數以一維方式排列所取得的總數。
                
				double avg = GetAvg(depthData, x, y, frameDesc.Width, frameDesc.Height);//計算每4X4個Pixel的深度平均值。
                
				avg = avg * _DepthScale;//降低深度的比率。
                
				_Vertices[smallIndex].z = (float)avg;//給予每個Mesh的點Z值。
                
                // Update UV mapping with CDRP
                var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];

                _UV[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }
        
        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
    }
	//每16個Pixel計算一個平均值。
    private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
    {
        double sum = 0.0;
        
        for (int y1 = y; y1 < y + 4; y1++)
        {
            for (int x1 = x; x1 < x + 4; x1++)
            {
                int fullIndex = (y1 * width) + x1;
                
                if (depthData[fullIndex] == 0)
                    sum += 4500;
                else
                    sum += depthData[fullIndex];
                
            }
        }

        return sum / 16;
    }
	//關閉Sensor等等動作。
    void OnApplicationQuit()
    {
        if (_Mapper != null)
        {
            _Mapper = null;
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
