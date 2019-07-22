/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TanksMP
{          
    /// <summary>
    /// Responsible for spawning AI bots when in offline mode, otherwise gets disabled.
    /// </summary>
	public class BotSpawner : NetworkBehaviour
    {                
        /// <summary>
        /// Amount of bots to spawn across all teams.
        /// </summary>
        public int maxBots;
        
        /// <summary>
        /// Selection of bot prefabs to choose from.
        /// </summary>
        public GameObject[] prefabs;
        
        
        void Awake()
        {
            //disabled when not in offline mode
            if ((NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode) != NetworkMode.Offline)
                this.enabled = false;
        }


        IEnumerator Start()
        {
            //wait a second for all script to initialize
            yield return new WaitForSeconds(1);

            //loop over bot count
			for(int i = 0; i < maxBots; i++)
            {
                //randomly choose bot from array of bot prefabs
                int randIndex = Random.Range(0, prefabs.Length);
                GameObject obj = (GameObject)GameObject.Instantiate(prefabs[randIndex], Vector3.zero, Quaternion.identity);

                //let the local host determine the team assignment
                BasePlayer p = obj.GetComponent<BasePlayer>();
                p.teamIndex = GameManager.GetInstance().GetTeamFill();
                p.myName = string.Format("Bot {0}", Random.Range(1, 1000));

                //spawn bot across the simulated private network
                NetworkServer.Spawn(obj, prefabs[randIndex].GetComponent<NetworkIdentity>().assetId);
                
                //increase corresponding team size
                GameManager.GetInstance().size[p.teamIndex]++;
                GameManager.GetInstance().ui.OnTeamSizeChanged(SyncListInt.Operation.OP_DIRTY, p.teamIndex);
                
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
