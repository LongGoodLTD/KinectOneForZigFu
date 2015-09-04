using UnityEngine;
using System.Collections;
public enum DepthImagePos { Invisible, RightMin, LeftMin, LeftMain } 
public class BaseUserDepthViewer : MonoBehaviour {
    public bool IsActivate = true;
    protected float imgWidth, imgHeight, baseX, baseY;
    protected float[] depthHistogramMap;

    protected Color32[] depthToColor;
    protected Color32[] outputPixels;
    protected ResolutionData textureSize;

    public Color32 defaultColor;
    public Color32 bgColor = Color.blue;
    public Color32[] labelToColor;
    public Color32 BaseColor = Color.yellow;

    public void CalImage2Screen(DepthImagePos pos)
    {
        switch (pos)
        {
            case DepthImagePos.Invisible:
                imgWidth = Screen.width / 10f;
                imgHeight = imgWidth * 3 / 4;

                baseX = 10f;
                baseY = Screen.height - imgHeight - 10;

                break;

            case DepthImagePos.RightMin:
                //RightMin
                imgWidth = Screen.width / 6f;
                imgHeight = imgWidth * 3 / 4;

                baseX = Screen.width - imgWidth - 10f;
                baseY = Screen.height - imgHeight - 10f;

                break;
            case DepthImagePos.LeftMin:

                //leftMin;
                imgWidth = Screen.width / 6f;
                imgHeight = imgWidth * 3 / 4;

                baseX = 10f;
                baseY = Screen.height - imgHeight - 10;


                break;
            case DepthImagePos.LeftMain:
                //leftMain;
                imgWidth = Screen.width / 2f;
                imgHeight = imgWidth * 3 / 4;
                baseX = (Screen.height - imgHeight) / 2;
                baseX = Mathf.Clamp(baseX, 0f, 1600f);
                baseY = baseX;

                break;
            default:
                //leftMain;
                imgWidth = Screen.width / 2f;
                imgHeight = imgWidth * 3 / 4;
                baseX = (Screen.height - imgHeight) / 2;
                baseX = Mathf.Clamp(baseX, 0f, 1600f);
                baseY = baseX;

                break;
        }


    }
}
