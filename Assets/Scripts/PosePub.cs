using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PoseMsg = RosMessageTypes.Geometry.PoseMsg;
using PointMsg = RosMessageTypes.Geometry.PointMsg;
using QuaternionMsg = RosMessageTypes.Geometry.QuaternionMsg;

public class PosePub : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "arm_pose";
    
    public float publishMessageFrequency = 0.5f;
    private float timeElapsed;

    void Start()
    {
        ros = ROSConnection.instance;
        ros.RegisterPublisher<PoseMsg>(topicName);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessageFrequency)
        {
            PoseMsg msg_data = new PoseMsg(
                new PointMsg(1, 2, 3),
                new QuaternionMsg(4, 5, 6, 7)
            );

            ros.Send(topicName, msg_data);
            timeElapsed = 0;
        }
    }
}
