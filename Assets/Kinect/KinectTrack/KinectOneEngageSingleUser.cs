using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class KinectOneEngageSingleUser : BaseEngageSingleUser
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    public ZigSkeleton user;
    public Transform zig;

    private Dictionary<ulong, ZigTrackedUser> _Bodies = new Dictionary<ulong, ZigTrackedUser>();
    private BodySourceManager _BodyManager;

    private Dictionary<Kinect.JointType, ZigJointId> _kinectToZigMapping = new Dictionary<Kinect.JointType, ZigJointId>()
    {
        {Kinect.JointType.Head , ZigJointId.Head},
        {Kinect.JointType.SpineShoulder , ZigJointId.Neck},
        {Kinect.JointType.SpineMid , ZigJointId.Torso},
        {Kinect.JointType.SpineBase , ZigJointId.Waist},
        //{Kinect.JointType.left , ZigJointId.LeftCollar},
        {Kinect.JointType.ShoulderLeft , ZigJointId.LeftShoulder},
        {Kinect.JointType.ElbowLeft , ZigJointId.LeftElbow},
        {Kinect.JointType.WristLeft , ZigJointId.LeftWrist},
        {Kinect.JointType.HandLeft , ZigJointId.LeftHand},
        {Kinect.JointType.HandTipLeft , ZigJointId.LeftFingertip},
        //{Kinect.JointType , ZigJointId.RightCollar},
        {Kinect.JointType.ShoulderRight , ZigJointId.RightShoulder},
        {Kinect.JointType.ElbowRight , ZigJointId.RightElbow},
        {Kinect.JointType.WristRight , ZigJointId.RightWrist},
        {Kinect.JointType.HandRight , ZigJointId.RightHand},
        {Kinect.JointType.HandTipRight , ZigJointId.RightFingertip},
        {Kinect.JointType.HipLeft , ZigJointId.LeftHip},
        {Kinect.JointType.KneeLeft , ZigJointId.LeftKnee},
        {Kinect.JointType.AnkleLeft , ZigJointId.LeftAnkle},
        {Kinect.JointType.FootLeft , ZigJointId.LeftFoot},
        {Kinect.JointType.HipRight , ZigJointId.RightHip},
        {Kinect.JointType.KneeRight , ZigJointId.RightKnee},
        {Kinect.JointType.AnkleRight , ZigJointId.RightAnkle},
        {Kinect.JointType.FootRight , ZigJointId.RightFoot}
    };

    void Update()
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
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }
        //new一個新的uLong的List，顯示當前已經建立出來的骨骼模型，用來與上面建立的當前讀取骨骼做比對。
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        //如果在比對的過程中，骨骼模型的ID並沒有在當前骨骼的ID中，代表此玩家已經離開了，因此刪除該GameObject。
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                UserLost(trackingId);
                //Destroy(_Bodies[trackingId]);
            }
        }
        //在刪除已經不在的玩家之後，繼續比對如果骨骼物件中沒有當前讀取到的骨骼ID，則建立新的骨骼物件。
        //若是比對的結果是骨骼物件也有繼續在當前ID中讀取到該ID，代表玩家還在進行中，則進行更新玩家動作的Function。

        if (trackedUserId() == 0 || (trackedUserId() != 0 && _Bodies.ContainsKey(trackedUserId())))
        {
            ulong whichOneShouldBeCheck = 0;
            float nearestZ = 9999999f;
            foreach (var body in data)
            {
                Kinect.Joint sourceJoint = body.Joints[Kinect.JointType.SpineShoulder];
                if (sourceJoint.Position.Z == 0f) continue;
                if (nearestZ > sourceJoint.Position.Z)
                {
                    nearestZ = sourceJoint.Position.Z;
                    whichOneShouldBeCheck = body.TrackingId;
                }
            }
            trackedId = whichOneShouldBeCheck;
        }
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    UserFound(body.TrackingId);
//                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                RefreshBodyObject(body, body.TrackingId);
                if (body.TrackingId == trackedUserId())
                {
                    Kinect_UpdateUser();
                }
            }
        }
    }
    ulong trackedId =0;
    public ulong trackedUserId()
    {
        return trackedId;
    }
    void UserFound(ulong userId)
    {
        Debug.Log("UserFound(Kinect User Found): " + userId + ", index = " + ((userId + 1) % 6));
        UpdateLgTrackedUser(userId, Vector3.zero);
    }

    void UpdateLgTrackedUser(ulong user_id, Vector3 vec)
    {
        ZigInputUser inputUser = new ZigInputUser((int)((user_id + 1) % 6), vec);
        inputUser.SkeletonData = new List<ZigInputJoint>();
        if (trackedUserId() == user_id) inputUser.Tracked = true;
        ZigTrackedUser tUser;
        try
        {
            tUser = _Bodies[user_id];
            tUser.Update(inputUser);
        }
        catch (System.Exception e)
        {
            tUser = new ZigTrackedUser(inputUser);
            _Bodies[user_id] = tUser;
        }
        zig.SendMessage("Zig_UserFound", tUser);
    }

    void UserLost(ulong userId)
    {
        Debug.Log("UserFound(Kinect User Lost): " + userId + ", index = " + ((userId + 1) % 6));
        ZigTrackedUser tUser = _Bodies[userId];
        if (trackedId == userId)
        {
            _engagedTrackedUser = null;
            trackedId = 0;
        }
        zig.SendMessage("Zig_UserLost", tUser);
        _Bodies.Remove(userId);
    }

    private Vector3 headP;
    private Quaternion headQ;
    IEnumerator JointUpdate(ulong userId, Kinect.Joint joint)
    {
        string jointName;
        float x, y, z;
        Vector3 vec = GetVector3FromJoint(joint);
        Quaternion q = Quaternion.identity;
        ZigJointId jointId;
        // argStr: user,jointName,x,y,z
        jointName = joint.ToString();

        jointId = _kinectToZigMapping[joint.JointType];
        if (joint.JointType == Kinect.JointType.Head)
        {
            headP = vec;
            headQ = q;
            UpdateLgTrackedUser(userId, headP);
        }
        ZigTrackedUser tUser = _Bodies[userId];
        ZigInputJoint inputJoint = new ZigInputJoint(jointId, vec, q, true);
        inputJoint.GoodPosition = true;
        inputJoint.GoodRotation = true;
        tUser.Skeleton[(int)jointId] = inputJoint;

        yield return 1;
    }

    //更新所有骨架物件的位置與關節之間的連線。
    private void RefreshBodyObject(Kinect.Body body, ulong userId)
    {
        ZigTrackedUser bodyObject = _Bodies[userId];
        //以迴圈的方式將所有的骨骼一個一個的去計算。
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            //抓取到來源關節，以及重置目標關節為空物件。
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            if (!_kinectToZigMapping.ContainsKey(sourceJoint.JointType))
            {
                continue;
            }
            //讀取出骨頭線段的出發點，也就是目前關節的位置。
            //Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            //jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            StartCoroutine(JointUpdate(userId,sourceJoint));
        }
    }
    private void Kinect_UpdateUser()
    {
        ulong userId = trackedUserId();
        ZigTrackedUser tUser = _Bodies[userId];
        _engagedTrackedUser = tUser;
        foreach (GameObject go in EngagedUsers)
        {
            go.SendMessage("Zig_UpdateUser", tUser, SendMessageOptions.DontRequireReceiver);
        }
    }
    //將關節和關節間的位置放大，讓人的形狀更明顯。
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        int K=1000;
        return new Vector3(joint.Position.X * K, joint.Position.Y * K, joint.Position.Z * K);
    }
}
