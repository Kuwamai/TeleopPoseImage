using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PoseMsg = RosMessageTypes.Geometry.PoseMsg;
using PoseStampedMsg = RosMessageTypes.Geometry.PoseStampedMsg;
using PointMsg = RosMessageTypes.Geometry.PointMsg;
using QuaternionMsg = RosMessageTypes.Geometry.QuaternionMsg;
using HeaderMsg = RosMessageTypes.Std.HeaderMsg;
using static Unity.Mathematics.math;

namespace uWindowCapture
{

public class PosePub : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "arm_pose";
    public float publishMessageFrequency = 0.5f;
    private float timeElapsed;

    [SerializeField] UwcWindowTexture uwcTexture;
    [SerializeField] Vector2Int offset;
    public Texture2D texture;
    Color32[] colors;

    void CreateTextureIfNeeded(int w, int h)
    {
        if (!texture || texture.width != w || texture.height != h)
        {
            colors = new Color32[w * h];
            texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            GetComponent<Renderer>().material.mainTexture = texture;
        }
    }

    void Start()
    {
        ros = ROSConnection.instance;
        ros.RegisterPublisher<PoseStampedMsg>(topicName);
    }

    void Update()
    {
        var window = uwcTexture.window;
        int w = (int)(window.width/32f*9f/16f*4f);
        int h = window.height - offset.y;
        int x = offset.x;
        int y = offset.y;
        Vector3[] vec3 = new Vector3[4];
        Vector3Int[] vec3_int = new Vector3Int[4];
        Matrix4x4 rot_mat = Matrix4x4.identity;
        Quaternion rot_qua = Quaternion.identity;

        CreateTextureIfNeeded(w, h);
        if (window == null || window.width == 0) return;

        if (window.GetPixels(colors, x, y, w, h)) {
            texture.SetPixels32(colors);
            texture.Apply();

            for(int wi=0;wi<4;wi++)
            {
                vec3[wi] = Vector3.zero;
                vec3_int[wi] = Vector3Int.zero;

                for(int hi=0;hi<32;hi++)
                {
                    Vector4 color = texture.GetPixel(Mathf.FloorToInt(w/8f+w/4f*wi), Mathf.FloorToInt(h/64f+h/32f*hi));
                    Vector3Int color_bit = Vector3Int.RoundToInt(color);
                    vec3_int[wi].x += color_bit.x << hi;
                    vec3_int[wi].y += color_bit.y << hi;
                    vec3_int[wi].z += color_bit.z << hi;
                }
                vec3[wi].x = asfloat(vec3_int[wi].x);
                vec3[wi].y = asfloat(vec3_int[wi].y);
                vec3[wi].z = asfloat(vec3_int[wi].z);
            }
            for(int i=0;i<3;i++)
            {
                rot_mat.SetRow(i, new Vector4(vec3[i+1].x, vec3[i+1].y, vec3[i+1].z, 0));
            }
            rot_qua = rot_mat.rotation;
            Debug.Log(vec3[0]);
            Debug.Log(rot_qua);
        }

        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessageFrequency)
        {
            PoseMsg pose = new PoseMsg(
                new PointMsg(vec3[0].z, -vec3[0].x, vec3[0].y),
                new QuaternionMsg(rot_qua.z, -rot_qua.x, rot_qua.y, -rot_qua.w)
            );
            
            HeaderMsg header = new HeaderMsg();
            header.frame_id = "unity";
            PoseStampedMsg posestamped = new PoseStampedMsg(
                header,
                pose
            );

            ros.Send(topicName, posestamped);
            timeElapsed = 0;
        }
    }
}

}