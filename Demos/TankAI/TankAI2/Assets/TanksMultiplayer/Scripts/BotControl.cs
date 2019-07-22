using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TanksMP
{
    public class BotControl : BaseControl
    {
        public float searchRange = 6;

        protected override void OnInit()
        {
            base.OnInit();
            agent = tankPlayer.agent;
        }

        override protected void OnRun() {
            lastScan = 0;
            inRange.Clear();
            targetPoint = tankPlayer.transform.position;
        }

        override protected void OnStop() {
            lastScan = 0;
            inRange.Clear();
        }

        override protected void OnFixedUpdate()
        {

            if (inRange.Count == 0)
            {
                //if this bot reached the the random point on the navigation mesh,
                //then calculate another random point on the navmesh on continue moving around
                //with no other players in range, the AI wanders from team spawn to team spawn
                if (Vector3.Distance(tankPlayer.transform.position, targetPoint) < agent.stoppingDistance)
                {
                    int teamCount = GameManager.GetInstance().teams.Length;
                    RandomPoint(GameManager.GetInstance().teams[Random.Range(0, teamCount)].spawn.position, searchRange, out targetPoint);
                }
            }
            else
            {
                //if we reached the targeted point, calculate a new point around the enemy
                //this simulates more fluent "dancing" movement to avoid being shot easily
                if (Vector3.Distance(tankPlayer.transform.position, targetPoint) < agent.stoppingDistance)
                {
                    RandomPoint(inRange[0].transform.position, searchRange * 2, out targetPoint);
                }

                if (tankPlayer.bShootable)
                {
                    //shooting loop 
                    for (int i = 0; i < inRange.Count; i++)
                    {
                        RaycastHit hit;
                        //raycast to detect visible enemies and shoot at their current position
                        if (Physics.Linecast(tankPlayer.transform.position, inRange[i].transform.position, out hit))
                        {
                            //get current enemy position and rotate this turret
                            Vector3 delta = inRange[i].transform.position - tankPlayer.Position;
                            tankPlayer.RotateTurret(delta.x, delta.z);
                            tankPlayer.Shoot();
                            break;
                        }
                    }
                }
            }
        }

        private void RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            //clear previous target point
            result = Vector3.zero;

            //try to find a valid point on the navmesh with an upper limit (10 times)
            for (int i = 0; i < 10; i++)
            {
                //find a point in the movement radius
                Vector3 randomPoint = center + (Vector3)Random.insideUnitCircle * range;
                randomPoint.y = 0;
                NavMeshHit hit;

                //if the point found is a valid target point, set it and continue
                if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    break;
                }
            }

            //set the target point as the new destination
            tankPlayer.MoveTo(result);
        }


        override protected void OnUpdate() {
            float t = Time.time;
            if(t - lastScan > 1)
            {
                // rescan
                inRange.Clear();
                Collider[] cols = Physics.OverlapSphere(tankPlayer.transform.position, searchRange, LayerMask.GetMask("Player"));

                for(int i = 0; i < cols.Length; ++i)
                {
                    BasePlayer p = cols[i].gameObject.GetComponent<BasePlayer>();
                    if(p != null && p.teamIndex != tankPlayer.teamIndex)
                    {
                        if (!inRange.Contains(cols[i].gameObject)){
                            inRange.Add(cols[i].gameObject);
                        }
                    }
                }
            }
        }

        private List<GameObject> inRange = new List<GameObject>();
        private float lastScan = 0;
        private Vector3 targetPoint;
        private NavMeshAgent agent;
    }
}
