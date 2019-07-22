/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.AI;

namespace TanksMP
{          
    /// <summary>
    /// Networked player class implementing movement control and shooting.
	/// Contains both server and client logic in an authoritative approach.
    /// </summary> 
	public class Player : BasePlayer
    {      

        //reference to this rigidbody

        /// <summary>
        /// Initialize synced values on every client.
        /// </summary>
        public override void OnStartClient()
        {
            SetupTank();
            label.text = myName;
			//call hooks manually to update
            OnHealthChange(health);
            OnShieldChange(shield);
            controlMode = ControlMode.Slave;
        }


        /// <summary>
        /// Initialize camera and input for this local client.
		/// This is being called after OnStartClient.
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            //initialized already on host migration
            if (GameManager.GetInstance().localPlayer != null)
                return;

			//set a global reference to the local player
            GameManager.GetInstance().localPlayer = this;

			//get components and set camera target
            camFollow = Camera.main.GetComponent<FollowTarget>();
            camFollow.target = turret;

            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;

            SetupController(GameSettings.instance.GetPlayerController());
            //SetupController(GameSettings.instance.GetBotController());

            //initialize input controls for mobile devices
            //[0]=left joystick for movement, [1]=right joystick for shooting
        }

        protected void Start()
        {
            if(controller != null)
            {
                controller.Run();
            }
        }

        //on shot drag start set small delay for first shot
        void ShootBegin()
        {
            nextFire = Time.time + 0.25f;
        }
        
        /// <summary>
        /// Called on all clients on game end providing the winning team.
        /// This is when a target kill count or goal (e.g. flag captured) was achieved.
        /// </summary>
        [ClientRpc]
        public override void RpcGameOver(int teamIndex)
        {
			//display game over window
            GameManager.GetInstance().DisplayGameOver(teamIndex);
        }
    }
}
