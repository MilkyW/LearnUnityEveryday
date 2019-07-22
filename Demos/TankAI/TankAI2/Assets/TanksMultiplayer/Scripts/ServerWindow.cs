using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TanksMP
{
    public class ServerWindow : MonoBehaviour
    {
        UIMain uimain;

        public GameObject objContent;
        public GameObject objTempItem;
        Dictionary<string, GameObject> dictServers = new Dictionary<string, GameObject>();

        public static ServerWindow Instance;
        private void Awake()
        {
            Instance = this;

            objTempItem.SetActive(false);
        }

        public void Init(UIMain uimain)
        {
            this.uimain = uimain;
        }
        public void AddAServer(string address,int port)
        {
            if (dictServers.ContainsKey(address))
                return;

            GameObject obj = Instantiate(objTempItem,objContent.transform) as GameObject;

            ServerItem item = obj.GetComponent<ServerItem>();
            item.Init(address,port, ConnectServer);
            dictServers[address] = obj;
        }

        void ConnectServer(string address,int port)
        {
            uimain.Connect(address,port);
        }

        public void RemAServer(string address)
        {
            if (dictServers.ContainsKey(address) == false)
                return;

            GameObject.Destroy(dictServers[address]);
            dictServers[address] = null;
        }
    }
}