/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace TanksMP
{
    /// <summary>
    /// Custom implementation of the Unity Networking NetworkMigrationManager class. This script is
    /// responsible for handling host disconnects and ensuring that a client takes over the connection.
    /// </summary>
	public class NetworkMigrationManagerCustom : NetworkMigrationManager
    {
        /// <summary>
        /// Override for the callback received when a connection to the match host is lost.
        /// Integrates the host migration workflow found in the UNET source, but in an automated manner.
        /// </summary>
        protected override void OnClientDisconnectedFromHost(NetworkConnection conn, out SceneChangeOption sceneChange)
        {
            //stay in the current game scene
            sceneChange = SceneChangeOption.StayInOnlineScene;

            //determine the new host
            PeerInfoMessage hostInfo;
            bool newHost;
            if(FindNewHost(out hostInfo, out newHost))
            {
                //if this client is determined to be the new host, wait for the switch
                //otherwise wait for another client taking over the hosting functionality
                newHostAddress = hostInfo.address;
                if (newHost)
                    waitingToBecomeNewHost = true;
                else
                    waitingReconnectToNewHost = true;
            }
            else
            {
                //we didn't find a host, leave the game
                if (LogFilter.logError) Debug.LogError("No Host found.");
                
                NetworkManager.singleton.SetupMigrationManager(null);
                NetworkManager.singleton.StopHost();
                Reset(ClientScene.ReconnectIdInvalid);
            }

            if (LogFilter.logDebug) Debug.Log("BecomeHost: " + waitingToBecomeNewHost + ", ReconnectToNewHost: " + waitingReconnectToNewHost);

            //we were chosen for hosting the new game
            if (waitingToBecomeNewHost)
            {
                //take over hosting functionality
                bool success = BecomeNewHost(NetworkManager.singleton.networkPort);

                if(success && (NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode) == NetworkMode.LAN)
                {
                    NetworkDiscovery discovery = GetComponent<NetworkDiscovery>();
                    discovery.Initialize();
                    discovery.StartAsServer();
                }
            }
            else if (waitingReconnectToNewHost)
            {
                //clear old host network data
                Reset(oldServerConnectionId);
                Reset(ClientScene.ReconnectIdHost);
                //choose different host address and reconnect
                NetworkManager.singleton.networkAddress = newHostAddress;
                NetworkManager.singleton.client.ReconnectToNewHost(newHostAddress, NetworkManager.singleton.networkPort);
            }
        }
    }
}
