/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TanksMP
{          
    /// <summary>
    /// Implementation of AI bots by overriding methods of the Player class.
    /// </summary>
	public class PlayerBot : BasePlayer
    {
        // TODO: BotBegin

        
        //timestamp when next shot should happen
        private float nextShot;
        //TODO: BotEnd

        public bool isLocalBot = true;

        //called before SyncVar updates
        void Start()
        {           
            //get components and set camera target
            camFollow = Camera.main.GetComponent<FollowTarget>();
            // init agent position
            var spawnPoint = GameManager.GetInstance().GetSpawnPosition(teamIndex);
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;

            if (isServer) {
                agent.Warp(spawnPoint);
            }

            SetupTank();
            label.text = myName;
            
			//call hooks manually to update
            OnHealthChange(health);
            OnShieldChange(shield);

            if (isServer)
            {
                SetupController(GameSettings.instance.GetBotController());
            }
            else
            {
                SetupController(null);
            }

            if (controller != null)
            {
                controller.Run();
            }
            
            // TODO: change to enable bot
			//start enemy detection routine
            //if(isLocalBot || isLocalPlayer)
            //    StartCoroutine(DetectPlayers());
            // TODO: change to enable bot end
        }
        
        
        // move to bot begin
        //sets inRange list for player detection
        //IEnumerator DetectPlayers()
        //{
        //    //wait for initialization
        //    yield return new WaitForEndOfFrame();
            
        //    //detection logic
        //    while(true)
        //    {
        //        //empty list on each iteration
        //        inRange.Clear();

        //        //casts a sphere to detect other player objects within the sphere radius
        //        Collider[] cols = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Player"));
        //        //loop over players found within bot radius
        //        for (int i = 0; i < cols.Length; i++)
        //        {
        //            //get other Player component
        //            //only add the player to the list if its not in this team
        //            Player p = cols[i].gameObject.GetComponent<Player>();
        //            if(p.teamIndex != teamIndex && !inRange.Contains(cols[i].gameObject))
        //            {
        //                inRange.Add(cols[i].gameObject);   
        //            }
        //        }
                
        //        //wait a second before doing the next range check
        //        yield return new WaitForSeconds(1);
        //    }
        //}
        // move to bot end
        
        //calculate random point for movement on navigation mesh
        //private void RandomPoint(Vector3 center, float range, out Vector3 result)
        //{
        //    //clear previous target point
        //    result = Vector3.zero;

        //    //try to find a valid point on the navmesh with an upper limit (10 times)
        //    for (int i = 0; i < 10; i++)
        //    {
        //        //find a point in the movement radius
        //        Vector3 randomPoint = center + (Vector3)Random.insideUnitCircle * range;
        //        randomPoint.y = 0;
        //        NavMeshHit hit;

        //        //if the point found is a valid target point, set it and continue
        //        if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
        //        {
        //            result = hit.position;
        //            break;
        //        }
        //    }

        //    //set the target point as the new destination
        //    agent.SetDestination(result);
        //}
        
        
//        void FixedUpdate()
//        {
//            if ( !GameManager.GetInstance().isStart )
//                return;

//            //skip further calls for remote clients
//            if (controlMode == ControlMode.Slave)
//            {
//                //keep turret rotation updated for all clients
//                ApplyTurretRotation(turretRotation);
//                return;
//            }

//            //TODO: handle game over

//            //don't execute anything if the game is over already,
//            //but termine the agent and path finding routines
//            if (GameManager.GetInstance().IsGameOver())
//            {
//                if(controller != null)
//                {
//                    controller.ControlStop();
//                }
////                agent.isStopped = true;
////                StopAllCoroutines();
////                enabled = false;
//                return;
//            }
//            //TODO: handle game over

//            //don't continue if this bot is marked as dead
//            if (!bIsAlive)
//            {
//                return;
//            }

//            if(controller != null)
//            {
//                controller.ControlFixUpdate();
//            }

            /*
            //no enemy players are in range
            if(inRange.Count == 0)
            {
                //if this bot reached the the random point on the navigation mesh,
                //then calculate another random point on the navmesh on continue moving around
                //with no other players in range, the AI wanders from team spawn to team spawn
                if (Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
                {
                    int teamCount = GameManager.GetInstance().teams.Length;
                    RandomPoint(GameManager.GetInstance().teams[Random.Range(0, teamCount)].spawn.position, range, out targetPoint);
                }
            }
            else
            {
                //if we reached the targeted point, calculate a new point around the enemy
                //this simulates more fluent "dancing" movement to avoid being shot easily
                if(Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
                {
                    RandomPoint(inRange[0].transform.position, range * 2, out targetPoint);
                }
                
                //shooting loop 
                for(int i = 0; i < inRange.Count; i++)
                {
                    RaycastHit hit;
                    //raycast to detect visible enemies and shoot at their current position
                    if (Physics.Linecast(transform.position, inRange[i].transform.position, out hit))
                    {
                        //get current enemy position and rotate this turret
                        Vector3 lookPos = inRange[i].transform.position;
                        turret.LookAt(lookPos);
                        turret.eulerAngles = new Vector3(0, turret.eulerAngles.y, 0);
                        
                        //find shot direction and shoot there
                        Vector3 shotDir = lookPos - transform.position;
                        Shoot(new Vector2(shotDir.x, shotDir.z));
                        break;
                    }
                }
            }
            */
        //}


        /// <summary>
        /// Override of the base method to handle bot respawn separately.
        /// </summary>
        //protected override void RpcRespawn()
        //{
        //    if (isLocalBot)
        //        StartCoroutine(Respawn());
        //    else
        //    {
        //        bool isActive = gameObject.activeInHierarchy;
        //        if(isActive)
        //        {
        //            isDead = true;
        //            inRange.Clear();
        //            agent.isStopped = true;
        //        }

        //        base.Respawn(senderId);

        //        isActive = gameObject.activeInHierarchy;
        //        if (isActive)
        //        {
        //            agent.Warp(targetPoint);
        //            agent.isStopped = false;
        //            isDead = false;

        //            if (isLocalPlayer)
        //                StartCoroutine(DetectPlayers());
        //        }                
        //    }
        //}


        //the actual respawn routine
        //IEnumerator Respawn()
        //{
        //    //stop AI updates
        //    isDead = true;
        //    inRange.Clear();
        //    agent.isStopped = true;

        //    //detect whether the current user was responsible for the kill
        //    //yes, that's my kill: increase local kill counter
        //    if (killedBy == GameManager.GetInstance().localPlayer.gameObject)
        //    {
        //        GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
        //        GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
        //    }

        //    if (explosionFX)
        //    {
        //        //spawn death particles locally using pooling and colorize them in the player's team color
        //        GameObject particle = PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
        //        ParticleColor pColor = particle.GetComponent<ParticleColor>();
        //        if (pColor) pColor.SetColor(GameManager.GetInstance().teams[teamIndex].material.color);
        //    }

        //    //play sound clip on player death
        //    if (explosionClip) AudioManager.Play3D(explosionClip, transform.position);

        //    //toggle visibility for all rendering parts (off)
        //    ToggleComponents(false);
        //    //wait global respawn delay until reactivation
        //    yield return new WaitForSeconds(GameManager.GetInstance().respawnTime);
        //    //toggle visibility again (on)
        //    ToggleComponents(true);

        //    //respawn and continue with pathfinding
        //    targetPoint = GameManager.GetInstance().GetSpawnPosition(teamIndex);
        //    transform.position = targetPoint;
        //    agent.Warp(targetPoint);
        //    agent.isStopped = false;
        //    isDead = false;
        //}


        //disable rendering or blocking components
        //void ToggleComponents(bool state)
        //{
        //    GetComponent<Rigidbody>().isKinematic = state;
        //    GetComponent<Collider>().enabled = state;

        //    for (int i = 0; i < transform.childCount; i++)
        //        transform.GetChild(i).gameObject.SetActive(state);
        //}
    }
}
