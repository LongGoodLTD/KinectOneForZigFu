using UnityEngine;
using System.Collections;

public class ColorSourceView : MonoBehaviour
{
    public GameObject ColorSourceManager;
    private ColorSourceManager _ColorManager;
	
    
	//設定彩色畫面顯示面板的材質。
    void Start ()
    {
//        gameObject.renderer.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
		//如果找不到彩色畫面控制器物件的話，跳離Function。
        if (ColorSourceManager == null)
        {
            return;
        }
		//讀取控制器Script，如果找不到的話，跳離Function。
		_ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
        if (_ColorManager == null)
        {
            return;
        }
		//讀取貼圖。
//		gameObject.renderer.material.mainTexture = _ColorManager.GetColorTexture();
    }


	void OnGUI() {
		if (!_ColorManager.GetColorTexture()) {
			Debug.LogError("Assign a Texture in the inspector.");
			return;
		}
		GUI.DrawTexture(new Rect(115, (9*28)+40, 16*28, -9*28), _ColorManager.GetColorTexture() , ScaleMode.StretchToFill, true, 10.0F);
	}


}
