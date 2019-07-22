using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

public class ServerItem : MonoBehaviour
{
    string address;
    int port;
    Action<string,int> action;

    static readonly string[] sBuiltInMatchNames = new string[5] { "分组A", "分组B", "分组C", "分组D", "分组E" };
    static readonly string[] sBuiltInMatchIPs = new string[5] { "172.26.180.5", "172.26.180.5", "172.26.180.5", "172.26.180.5", "172.26.180.5" };
    static readonly int[] sBuiltInMatchPorts = new int[5] { 7777, 7778, 7779, 7780, 7781 };

    public void Init(string address,int port,Action<string,int> action)
    {
        this.address = address;
        this.port = port;
        this.action = action;

        gameObject.SetActive(true);

        string ip = address.Replace("::ffff:", "");
        GetComponentInChildren<Text>().text = ip;
        for (int i=0;i<5;++i)
        {
            if (sBuiltInMatchIPs[i] == ip && sBuiltInMatchPorts[i] == port)
            {
                GetComponentInChildren<Text>().text = sBuiltInMatchNames[i];
                break;
            }
        }
    }

    public void Click()
    {
        action(address,port);
    }
}