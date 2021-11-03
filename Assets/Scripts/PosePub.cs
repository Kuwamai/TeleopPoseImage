using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PoseMsg = RosMessageTypes.Geometry.PoseMsg;
using PointMsg = RosMessageTypes.Geometry.PointMsg;
using QuaternionMsg = RosMessageTypes.Geometry.QuaternionMsg;
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
        ros.RegisterPublisher<PoseMsg>(topicName);
    }

    void Update()
    {
        var window = uwcTexture.window;
        int w = (int)(window.width/32f*9f/16f*4f);
        int h = window.height - offset.y;
        int x = offset.x;
        int y = offset.y;

        CreateTextureIfNeeded(w, h);
        if (window == null || window.width == 0) return;

        if (window.GetPixels(colors, x, y, w, h)) {
            texture.SetPixels32(colors);
            texture.Apply();
            Vector3 pos = Vector3.zero;
            Vector3Int pos_int = Vector3Int.zero;
            for(int i=0;i<32;i++)
            {
                Vector4 color = texture.GetPixel(Mathf.FloorToInt(w/8f), Mathf.FloorToInt(h/64f+h/32f*i));
                Vector3Int color_bit = Vector3Int.RoundToInt(color);
                pos_int.x += color_bit.x << i;
                pos_int.y += color_bit.y << i;
                pos_int.z += color_bit.z << i;
            }
            pos.x = asfloat(pos_int.x);
            pos.y = asfloat(pos_int.y);
            pos.z = asfloat(pos_int.z);
            Debug.Log(pos);
        }

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

}