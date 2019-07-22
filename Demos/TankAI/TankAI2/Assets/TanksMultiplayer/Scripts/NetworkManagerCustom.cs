/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace TanksMP
{
    /// <summary>
    /// Custom implementation of the Unity Networking NetworkManager class. This script is
    /// responsible for connecting to the Matchmaker, spawning players and handling disconnects.
    /// </summary>
	public class NetworkManagerCustom : NetworkManager
	{
        /// <summary>
        /// Event fired when a connection to the matchmaker service failed.
        /// </summary>
        public static event Action connectionFailedEvent;

        /// <summary>
        /// Starts initializing and connecting to a game. Depends on the selected network mode.
        /// </summary>
        public static void StartMatch(NetworkMode mode)
        {
            switch (mode)
            {
                //tries to retrieve a list of all games currently available on the matchmaker
                case NetworkMode.Online:
                    //add a filter attribute considering the selected game mode on the matchmaker as well
                    singleton.StartMatchMaker();
                    singleton.matchMaker.ListMatches(0, 20, "", false, 0, PlayerPrefs.GetInt(PrefsKeys.gameMode), singleton.OnMatchList);
                    break;

                //search for open LAN games on the current network, otherwise open a new one
                case NetworkMode.LAN:
                    (singleton as NetworkManagerCustom).CreateMatch();
                    singleton.StartCoroutine((singleton as NetworkManagerCustom).DiscoverNetwork());
                    break;

                //start a single LAN game but do not make it public over the network (offline)
                case NetworkMode.Offline:
                    (singleton as NetworkManagerCustom).CreateMatch();
                    singleton.StartHost(singleton.connectionConfig, 1);
                    break;
            }
        }

        public static void StartAsServer()
        {
            (singleton as NetworkManagerCustom).CreateMatch();
            singleton.StartCoroutine((singleton as NetworkManagerCustom).StartNetwork(true));
        }
        public static void StartAsClient(bool bDiscovery = true)
        {
            (singleton as NetworkManagerCustom).CreateMatch();
            if(bDiscovery)
                singleton.StartCoroutine((singleton as NetworkManagerCustom).StartNetwork(false));
        }

        /// <summary>
        /// Override for the callback received when the list of matchmaker matches returns.
        /// This method decides if we can join a game or need to create our own session.
        /// </summary>
        public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
		{
            //the list of matches could not be retrieved,
            //call the connection failed event
            if(!success)
            {
                if (connectionFailedEvent != null)
                    connectionFailedEvent();

                return;
            }

            //there is a list of matches
            if (matchList != null)
            {
                //create temporary list of matches available returned by the matchmaker
                //loop over them and exclude matches with maximum amount of players,
                //as we don't want to join a game that is full already
                List<MatchInfoSnapshot> availableMatches = new List<MatchInfoSnapshot>();
                foreach (MatchInfoSnapshot match in matchList)
                {
                    if (match.currentSize < match.maxSize)
                        availableMatches.Add(match);
                }

                //if there is no match we could join after filtering, create our own match
                //otherwise just join a random match chosen from the filtered list
                if (availableMatches.Count == 0)
                    CreateMatch();
                else
                    matchMaker.JoinMatch(availableMatches[UnityEngine.Random.Range(0, availableMatches.Count - 1)].networkId,
                                         "", "", "", 0, PlayerPrefs.GetInt(PrefsKeys.gameMode), OnMatchJoined);
            }
        }


        /// <summary>
        /// Searches for other LAN games in the current network.
        /// </summary>
        public IEnumerator DiscoverNetwork()
        {
            //start listening to other hosts
            NetworkDiscoveryCustom discovery = GetComponent<NetworkDiscoveryCustom>();
            discovery.Initialize();
            discovery.StartAsClient();

            //wait few seconds for broadcasts to arrive
            yield return new WaitForSeconds(8);

            //we haven't found a match, open our own
            if(discovery.running)
            {
                discovery.StopBroadcast();
                yield return new WaitForSeconds(0.5f);

                discovery.StartAsServer();
                StartHost();
            }
        }

        public IEnumerator StartNetwork(bool bHost)
        {
            //start listening to other hosts
            NetworkDiscoveryCustom discovery = GetComponent<NetworkDiscoveryCustom>();
            discovery.Initialize();
            if (bHost)
            {
                discovery.StartAsServer();
                StartHost();
            }
            else
            {
                discovery.StartAsClient();

                while(discovery.running)
                    yield return new WaitForEndOfFrame();
            }
        }

        public void StopNetwork()
        {
            NetworkDiscoveryCustom discovery = GetComponent<NetworkDiscoveryCustom>();
            discovery.StopBroadcast();
        }


        //creates a new match with default values
        void CreateMatch()
        {
            int gameMode = PlayerPrefs.GetInt(PrefsKeys.gameMode);
            //load the online scene randomly out of all available scenes for the selected game mode
            //we are checking for a naming convention here, if a scene starts with the game mode abbreviation
            string activeGameMode = ((GameMode)gameMode).ToString();
            List<string> matchingScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string[] scenePath = SceneUtility.GetScenePathByBuildIndex(i).Split('/');
                if (scenePath[scenePath.Length - 1].StartsWith(activeGameMode))
                {
                    matchingScenes.Add(scenePath[scenePath.Length - 1].Replace(".unity", ""));
                }
            }

            //check that your scene begins with the game mode abbreviation
            if (matchingScenes.Count == 0)
            {
                Debug.LogWarning("No Scene for selected Game Mode found in Build Settings!");
                return;
            }

            //get random scene out of available scenes and assign it as the online scene
            onlineScene = matchingScenes[UnityEngine.Random.Range(0, matchingScenes.Count)];

            //double check to only start matchmaking match in online mode
            //if(PlayerPrefs.GetInt(PrefsKeys.networkMode) == 0)
            //    matchMaker.CreateMatch("", matchSize, true, "", "", "", 0, gameMode, OnMatchCreate);
        }


        /// <summary>
        /// Override for the callback received when a new match has been created.
        /// In this case the user is the host and has to initialize the server part.
        /// </summary>
        public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            //our own match could not be created,
            //call the connection failed event
            if(!success)
            {
                if (connectionFailedEvent != null)
                    connectionFailedEvent();

                return;
            }

            //start hosting the match
            StartHost(matchInfo);
        }

        
        /// <summary>
        /// Override for the callback received when a match has been joined.
        /// In this case the user is the client and has to initialize the client part.
        /// </summary>
        public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            //an existing match could not be joined
            //we can't call the failed event due to a Unity bug       
            if(!success)
            {
                //workaround for UNET not being able to access scene objects in this callback
                //this means calling the failed event will fail... reloading the scene here instead
                int currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
                return;
            }

            //start joining the match
            StartClient(matchInfo);
        }


        /// <summary>
        /// Override for callback received (on the server) when a new client joins the game.
        /// Same as in the UNET source, but modified AddPlayer method with more parameters.
        /// </summary>
        public override void OnClientConnect(NetworkConnection conn)
	    {   
            //if the client connected but did not load the online scene
            if (!clientLoadedScene)
            {
                //Ready/AddPlayer is usually triggered by a scene load completing (see OnClientSceneChanged).
                //if no scene was loaded, maybe because we don't have separate online/offline scenes, then do it here.
                ClientScene.Ready(conn);
                if (autoCreatePlayer)
	            {
	                ClientScene.AddPlayer(conn, 0, GetJoinMessage());
	            }
	        }
	    }


        /// <summary>
        /// Override for the callback received when a client finished loading the game scene.
        /// Same as in the UNET source, but modified AddPlayer method with more parameters.
        /// </summary>
	    public override void OnClientSceneChanged(NetworkConnection conn)
	    {          
	        //always become ready
            ClientScene.Ready(conn);
	        if(!autoCreatePlayer)
	            return;

            if (GameManager.isMaster() && PlayerPrefs.GetInt(PrefsKeys.networkMode)!=(int) NetworkMode.Offline)
                return;

	        //no players exists?
	        bool addPlayer = (ClientScene.localPlayers.Count == 0);		
	        bool foundPlayer = false;
            //try to search for local player gameobject
	        foreach (var playerController in ClientScene.localPlayers)
	        {
	            if(playerController.gameObject != null)
	            {
	                foundPlayer = true;
	                break;
	            }
	        }

            //there are players, but their gameobjects have all been deleted
	        if(!foundPlayer)
	        {
                //we should add a local player
	            addPlayer = true;
	        }

            //create the player gameobject
	        if(addPlayer)
	        {
                ClientScene.AddPlayer(conn, 0, GetJoinMessage());
            }
	    }


        /// <summary>
        /// Override for the callback received on the server when a client requests creating its player prefab.
        /// Nearly the same as in the UNET source OnServerAddPlayerInternal method, but reading out the message passed in,
        /// effectively handling user player prefab selection, assignment to a team and spawning it at the team area.
        /// </summary>
	    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
	    {   
            //read the user message
	        JoinMessage message = null;
	        if (extraMessageReader != null)
	        {
	            message = extraMessageReader.ReadMessage<JoinMessage>();
	            //Debug.Log("The extra string value is: " + message.prefabId + " " + message.playerName);
	        }
	      
            //read prefab index to spawn out of the JoinMessage the client sent along with its request
            //then try to get prefab of the registered spawnable prefabs in the NetworkManager inspector (Spawn Info section)
	        GameObject playerObj = null;
            if(spawnPrefabs.Count > message.prefabId)
                playerObj = spawnPrefabs[message.prefabId];

            //debug some errors with incorrect configuration
	        if (playerObj == null)
	        {
	            if (LogFilter.logError) { Debug.LogError("The Player is empty on the NetworkManager. Please check Registered Prefabs + StringMessage object."); }
	            return;
	        }

	        if (playerObj.GetComponent<NetworkIdentity>() == null)
	        {
	            if (LogFilter.logError) { Debug.LogError("The Player does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab."); }
	            return;
	        }

	        if (playerControllerId < conn.playerControllers.Count  && conn.playerControllers[playerControllerId].IsValid && conn.playerControllers[playerControllerId].gameObject != null)
	        {
	            if (LogFilter.logError) { Debug.LogError("There is already a player at that playerControllerId for this connection."); }
	            return;
	        }

            //get the team value for this player
            int teamIndex = GameManager.GetInstance().GetTeamFill();
            //get spawn position for this team and instantiate the player there
            Vector3 startPos = GameManager.GetInstance().GetSpawnPosition(teamIndex);
	        playerObj = (GameObject)Instantiate(playerObj, startPos, Quaternion.identity);
            
            //assign name (also in JoinMessage) and team to Player component
            BasePlayer p = playerObj.GetComponent<BasePlayer>();
            p.myName = message.playerName;
            p.teamIndex = teamIndex;

            //update the game UI to correctly display the increased team size
            GameManager.GetInstance().size[p.teamIndex]++;
            GameManager.GetInstance().names[p.teamIndex] = message.playerName;
            //GameManager.GetInstance().ui.OnTeamSizeChanged(SyncListInt.Operation.OP_DIRTY, p.teamIndex);
            GameManager.GetInstance().ui.OnTeamNameChanged(SyncListString.Operation.OP_DIRTY, p.teamIndex);
            //finally map the player gameobject to the connection requesting it
            NetworkServer.AddPlayerForConnection(conn, playerObj, playerControllerId);

            //TODO:temporary add default control
        }


        /// <summary>
        /// Override for the callback received on the server when a client disconnects from the game.
        /// Updates the game UI to correctly display the decreased team size.  This is not called for
        /// the server itself, thus the workaround in GameManager's OnHostMigration method is needed.
        /// </summary>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            BasePlayer p = conn.playerControllers[0].gameObject.GetComponent<BasePlayer>();

            Collectible[] collectibles = p.GetComponentsInChildren<Collectible>(true);
            for (int i = 0; i < collectibles.Length; i++)
            {
                //let the player drop the Collectible
                int changedIndex = GameManager.GetInstance().AddBufferedCollectible(collectibles[i].netId, p.gameObject.transform.position, new NetworkInstanceId(0));
                GameManager.GetInstance().OnCollectibleStateChanged(SyncListCollectible.Operation.OP_DIRTY, changedIndex);
            }

            GameManager.GetInstance().size[p.teamIndex]--;
            GameManager.GetInstance().ui.OnTeamSizeChanged(SyncListInt.Operation.OP_DIRTY, p.teamIndex);
            base.OnServerDisconnect(conn);
        }
        
        
        /// <summary>
        /// Override for the callback received when a client disconnected.
        /// Eventual cleanup of internal high level API UNET variables.
        /// </summary>
        public override void OnStopClient()
        {
            //because we are not using the automatic scene switching and cleanup by Unity Networking,
            //the current network scene is still set to the online scene even after disconnecting.
            //so to clean that up for internal reasons, we simply set it to an empty string here
            networkSceneName = "";
        }


        //constructs the JoinMessage for the client by reading its device settings
        private JoinMessage GetJoinMessage()
        {
            JoinMessage message = new JoinMessage();
            message.prefabId = short.Parse(Encryptor.Decrypt(PlayerPrefs.GetString(PrefsKeys.activeTank)));
            message.playerName = PlayerPrefs.GetString(PrefsKeys.playerName);
            return message;
        }
	}


    /// <summary>
    /// Network Mode selection for preferred network type.
    /// </summary>
    public enum NetworkMode
    {
        Online,
        LAN,
        Offline
    }
    
    
    /// <summary>
    /// The client message constructed for the add player request to the server.
    /// You can extend this class to send more data at the point of joining a match.
    /// </summary>
    [System.Serializable]
    public class JoinMessage : MessageBase
    {
        /// <summary>
        /// The player prefab index to be spawned, selected by the user.
        /// </summary>
        public short prefabId;
        
        /// <summary>
        /// The user name entered in the game settings.
        /// </summary>
        public string playerName;
    }
}
