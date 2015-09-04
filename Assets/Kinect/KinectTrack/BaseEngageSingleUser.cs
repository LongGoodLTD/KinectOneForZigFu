using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEngageSingleUser : MonoBehaviour {
    protected ZigTrackedUser _engagedTrackedUser;
    public ZigTrackedUser engagedTrackedUser { get { return _engagedTrackedUser; } }
    public List<GameObject> EngagedUsers;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
