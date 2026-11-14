using UnityEngine;
using System.Collections.Generic;

public class BuzzCharacterMotion : MonoBehaviour {
    public enum Motion { Idle, WalkInPlace, Wave }
    public Motion motion = Motion.Wave;
    public float speed = 1f;
    Dictionary<string, Transform> joints = new Dictionary<string, Transform>();
    Dictionary<string, Quaternion> rest = new Dictionary<string, Quaternion>();
    Vector3 hipsRest;
    void Awake(){foreach(var t in GetComponentsInChildren<Transform>()){joints[t.name]=t;rest[t.name]=t.localRotation;}if(joints.ContainsKey("Hips"))hipsRest=joints["Hips"].localPosition;}
    void Pose(string n,float x=0,float y=0,float z=0){if(joints.TryGetValue(n,out var t))t.localRotation=rest[n]*Quaternion.Euler(x,y,z);}
    void LateUpdate(){float t=Time.time*speed;float w=Mathf.Sin(t*5);bool walk=motion==Motion.WalkInPlace;bool wave=motion==Motion.Wave;
        if(joints.TryGetValue("Hips",out var h))h.localPosition=hipsRest+Vector3.up*(walk?.035f*(1-Mathf.Cos(t*10)):.018f*Mathf.Sin(t*2));
        Pose("Body",0,0,2*Mathf.Sin(t*2));Pose("Head",0,7*Mathf.Sin(t*1.5f),3*Mathf.Sin(t*2));
        Pose("Left Shoulder",walk?30*w:0,0,0);Pose("Right Shoulder",walk?-30*w:0,0,wave?115:0);
        Pose("Left Elbow",walk?-12:0);Pose("Right Elbow",walk?-12:0,0,wave?35+18*Mathf.Sin(t*7):0);
        Pose("Right Hand",0,0,wave?15*Mathf.Sin(t*7):0);
        Pose("Left Hip",walk?-33*w:0);Pose("Right Hip",walk?33*w:0);
        Pose("Left Knee",walk?Mathf.Max(0,w)*37:0);Pose("Right Knee",walk?Mathf.Max(0,-w)*37:0);
        Pose("Left Wing",0,12*Mathf.Sin(t*9));Pose("Right Wing",0,-12*Mathf.Sin(t*9));
        Pose("Left Antenna",4*Mathf.Sin(t*3));Pose("Right Antenna",-4*Mathf.Sin(t*3));
    }
}
