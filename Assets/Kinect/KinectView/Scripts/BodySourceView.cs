using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
	public Material BoneMaterial;
    public GameObject BodySourceManager;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    void Update () 
    {
		//如果沒有負責建立骨骼資料的GameObject則跳出這個Function。
        if (BodySourceManager == null)
        {
            return;
        }
		//讀取建立骨骼資料的Script。
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
		//取得記錄骨骼資料的_Data陣列，如果取到的資料為null則離開這個Function。。
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
		//new一個新的uLong的List，用來記錄當下(該次Update)所讀取到的骨骼的ID。
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
		//new一個新的uLong的List，顯示當前已經建立出來的骨骼模型，用來與上面建立的當前讀取骨骼做比對。
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
		//如果在比對的過程中，骨骼模型的ID並沒有在當前骨骼的ID中，代表此玩家已經離開了，因此刪除該GameObject。
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }
		//在刪除已經不在的玩家之後，繼續比對如果骨骼物件中沒有當前讀取到的骨骼ID，則建立新的骨骼物件。
		//若是比對的結果是骨骼物件也有繼續在當前ID中讀取到該ID，代表玩家還在進行中，則進行更新玩家動作的Function。
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }
	//透過骨骼ID建立新的骨骼物件。
    private GameObject CreateBodyObject(ulong id)
    {
		Debug.Log("Create new Player!");
        GameObject body = new GameObject("Body:" + id);
		//以迴圈的方式將所有的骨骼一個一個的去計算。
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
			//產生一個新的基礎方塊。
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//賦予其LineRenderer的Component，給予其兩個座標，賦予其材質球，在設定線段的粗細。
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
			//設定該方塊的尺寸，並且給他對應的物件名稱，並且parent到主物件上面。
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
	//更新所有骨架物件的位置與關節之間的連線。
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
		//以迴圈的方式將所有的骨骼一個一個的去計算。
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
			//抓取到來源關節，以及重置目標關節為空物件。
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
			//運用Key和Velue的關係讀取目標關節。
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
			//讀取出骨頭線段的出發點，也就是目前關節的位置。
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
			//讀取關節的LineRenderer，如果有目標關節的話，賦予其座標0和座標1的位置，同時設定骨頭線段的顏色。
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
			}
			//如果沒有目標關節的話，代表此為末端關節，不需要LineRenderer，將其關閉。
            else
            {
                lr.enabled = false;
            }
        }
    }
	//骨頭線段的顏色判定，如果可以追蹤的設定為綠色，無法則設定為紅色。
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
	//將關節和關節間的位置放大，讓人的形狀更明顯。
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
