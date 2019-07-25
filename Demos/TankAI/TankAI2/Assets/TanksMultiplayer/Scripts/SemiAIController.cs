using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TanksMP
{
    public class SemiAIController : BaseControl
    {
        [System.Serializable]
        public struct Statistics
        {
            public int shootTime;
            public int hitTime;
            public float hitRate;
        }
        public Statistics statistics;

        private int lastScore = 0;
        private const float bulletSpeed = 18.0f;
        private const float tankSpeed = 8.0f;
        private Vector3 lastTarget;
        private Vector3 currentTarget;

        private Vector3[] lastAimPosition = new Vector3[4];
        private Vector3[] lastPosition = new Vector3[4];

        private NavMeshAgent[] agents = new NavMeshAgent[4];
        private NavMeshAgent agent;

        private float startAutoTime = 0;

        //movement variables
        private Vector2 moveDir = Vector2.zero;
        private Vector2 turnDir = Vector2.zero;

        [System.Serializable]
        public class BulletInfo
        {
            public BasePlayer target;
            public bool inited;
            public bool found;
        }
        public Dictionary<Bullet, BulletInfo> bulletInfos = new Dictionary<Bullet, BulletInfo>();
        public BasePlayer lastTargetPlayer;

        private void InitStatistics()
        {
            statistics.shootTime = 0;
            statistics.hitTime = 0;
            statistics.hitRate = 0;
        }

        private void InitEnemy()
        {
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            foreach (var pl in allplayer)
            {
                var comp = pl.GetComponent<BasePlayer>();
                int index = comp.teamIndex;
                agents[index] = pl.GetComponent<NavMeshAgent>();
            }
        }

        private void UpdateBulletInfo()
        {
            GameObject[] allbullet = GameObject.FindGameObjectsWithTag("Bullet");
            foreach (var bis in bulletInfos)
            {
                bis.Value.found = false;
            }
            foreach (var bl in allbullet)
            {
                Bullet bullet = bl.GetComponent<Bullet>();
                if (bulletInfos.ContainsKey(bullet))
                {
                    var bi = bulletInfos[bullet];
                    if (!bi.inited && bullet.owner == tankPlayer.gameObject)
                    {
                        bi.target = lastTargetPlayer;
                        bi.inited = true;
                    }
                    bi.found = true;
                }
                else
                {
                    BulletInfo bi = new BulletInfo();
                    bulletInfos.Add(bullet, bi);
                    if (bullet.owner == tankPlayer.gameObject)
                    {
                        bi.target = lastTargetPlayer;
                        bi.inited = true;
                    }
                    bi.found = true;
                }
            }
            foreach (var bis in bulletInfos)
            {
                if (bis.Value.found == false)
                {
                    bis.Value.inited = false;
                }
            }
        }

        private bool WillDie(BasePlayer target)
        {
            if (target.health > 5)
                return false;

            foreach (var bis in bulletInfos)
            {
                if (bis.Key.enabled && bis.Value.inited && bis.Value.target == target
                    && target.health < bis.Key.damage + 1)
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnInit()
        {
            base.OnInit();

#if !UNITY_STANDALONE && !UNITY_WEBGL
            GameManager.GetInstance().ui.controls[0].onDrag += Move;
            GameManager.GetInstance().ui.controls[0].onDragEnd += MoveEnd;

            GameManager.GetInstance().ui.controls[1].onDragBegin += ShootBegin;
            GameManager.GetInstance().ui.controls[1].onDrag += RotateTurret;
            GameManager.GetInstance().ui.controls[1].onDrag += Shoot;
#endif

#if UNITY_EDITOR
            InitStatistics();
            Invoke("DrawTrailsInit", 2.0f);
#endif

            Invoke("InitEnemy", 2.0f);
            agent = tankPlayer.GetComponent<NavMeshAgent>();
            agent.autoBraking = false;
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateBulletInfo();

#pragma warning disable 0219
            bool doShoot = false;
#pragma warning restore 0219

            //Debug.Log(agent.path.corners.Length.ToString());

            if (MoveJoystick(ref moveDir))
            {
                startAutoTime = Time.time + Time.fixedDeltaTime;
                agent.isStopped = true;
                tankPlayer.SimpleMove(moveDir);
            }
            else if (startAutoTime < Time.time)
            {
                agent.isStopped = false;
                tankPlayer.MoveTo(new Vector3(0, 0, 0));
                //if (agent.path.corners.Length > 1)
                //    moveDir = agent.path.corners[1] - tankPlayer.Position;
            }
            //turnDir = RotateMouse();
            //turnDir = RotateJoystick();
            //turnDir = SimpleRotate();
            //turnDir = ExpectBulletRotate();
            if (RotateJoystick(ref turnDir))
            {
                tankPlayer.RotateTurret(turnDir.x, turnDir.y);
            }
            //else if (doShoot = ExpectTankRotate(ref turnDir))
            else if (doShoot = NoSeeTankRotate(ref turnDir))
            {
                tankPlayer.RotateTurret(turnDir.x, turnDir.y);
            }

            //shoot bullet on left mouse click
            if (tankPlayer.bShootable && (doShoot || Input.GetButton("Fire1")))
            {
                tankPlayer.Shoot();
#if UNITY_EDITOR
                statistics.shootTime++;
                statistics.hitRate = ((float)statistics.hitTime) / statistics.shootTime;
#endif
                lastTarget = currentTarget;
            }

            if (GameManager.GetInstance().GetScore(tankPlayer.teamIndex) != lastScore)
            {
                lastScore = GameManager.GetInstance().GetScore(tankPlayer.teamIndex);
#if UNITY_EDITOR
                statistics.hitTime++;
                statistics.hitRate = ((float)statistics.hitTime) / statistics.shootTime;
#endif
            }

            //replicate input to mobile controls for illustration purposes
#if UNITY_EDITOR
            GameManager.GetInstance().ui.controls[0].position = moveDir;
            GameManager.GetInstance().ui.controls[1].position = turnDir;
#endif
        }

        private bool MoveJoystick(ref Vector2 moveDir)
        {

            //reset moving input when no arrow keys are pressed down
            if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            {
                return false;
            }
            else
            {
                //read out moving directions and calculate force
                moveDir.x = Input.GetAxis("Horizontal");
                moveDir.y = Input.GetAxis("Vertical");
                return true;
            }
        }

        private Vector3 RotateMouse()
        {
            //cast a ray on a plane at the mouse position for detecting where to shoot 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.up);
            float distance = 0f;
            Vector3 hitPos = Vector3.zero;
            //the hit position determines the mouse position in the scene
            if (plane.Raycast(ray, out distance))
            {
                hitPos = ray.GetPoint(distance) - tankPlayer.transform.position;
            }

            //we've converted the mouse position to a direction
            Vector3 turnDir = new Vector2(hitPos.x, hitPos.z);

            //rotate turret to look at the mouse direction
            tankPlayer.RotateTurret(hitPos.x, hitPos.z);

            return turnDir;
        }

        private bool RotateJoystick(ref Vector2 turnDir)
        {
            Vector3 hitPos = Vector3.zero;

            if (Input.GetAxisRaw("HorizontalR") != 0 || Input.GetAxisRaw("VerticalR") != 0)
            {
                hitPos.x = Input.GetAxis("HorizontalR");
                hitPos.z = -Input.GetAxis("VerticalR");

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);

                //rotate turret to look at the mouse direction
                tankPlayer.RotateTurret(hitPos.x, hitPos.z);

                return true;
            }

            return false;
        }

        private Vector3 SimpleRotate()
        {
            LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
            float height = tankPlayer.shotPos.position.y;
            //float radius = GameObject.FindGameObjectWithTag("Bullet").GetComponentInParent<SphereCollider>().radius;
            float bulletRadius = 0.2f;
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            RaycastHit raycastHit;
            float minDistance = ((tankPlayer.currentBullet == 2) ? 3 : 1) * bulletSpeed;
            Vector3 origin = tankPlayer.Position;
            origin.y = height;
            Vector3 hitPos = Vector3.zero;
            Vector3 turnDir = Vector2.zero;
            foreach (var pl in allplayer)
            {
                pl.GetComponent<Collider>().enabled = false;
                var comp = pl.GetComponent<BasePlayer>();
                //Vector3 target = comp.Position;
                Vector3 target = comp.transform.TransformPoint(comp.GetComponent<BoxCollider>().center);
                target += comp.Velocity * Time.fixedDeltaTime;
                target.y = height;
                Debug.DrawLine(origin, target, Color.blue);
                if (comp.teamIndex != tankPlayer.teamIndex && comp.IsAlive
                    && (target - origin).magnitude < minDistance
                        && Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                        && raycastHit.collider.gameObject == tankPlayer.gameObject)
                {
                    minDistance = (target - origin).magnitude;
                    hitPos = target;
                }
                pl.GetComponent<Collider>().enabled = true;
            }
            if (minDistance != ((tankPlayer.currentBullet == 2) ? 3 : 1) * bulletSpeed)
            {
                Debug.DrawLine(origin, hitPos, Color.red);
                hitPos -= origin;

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);

                //rotate turret to look at the mouse direction
                tankPlayer.RotateTurret(hitPos.x, hitPos.z);
            }

            return turnDir;
        }

        private bool ExpectBulletRotate(ref Vector2 turnDir)
        {
            LayerMask layerMask = LayerMask.GetMask("Powerup");
            float height = tankPlayer.shotPos.position.y;
            //float radius = GameObject.FindGameObjectWithTag("Bullet").GetComponentInParent<SphereCollider>().radius;
            float bulletRadius = 0.2f;
            GameObject[] allbullet = GameObject.FindGameObjectsWithTag("Bullet");
            RaycastHit raycastHit;
            float minDistance = ((tankPlayer.currentBullet == 2) ? 3 : 1) * bulletSpeed;
            Vector3 origin = tankPlayer.Position;
            origin.y = height;
            Vector3 hitPos = Vector3.zero;
            foreach (var bl in allbullet)
            {
                bl.GetComponent<Collider>().enabled = false;
                var comp = bl.GetComponent<Bullet>();
                //Vector3 target = comp.Position;
                Vector3 target = comp.transform.TransformPoint(comp.GetComponent<SphereCollider>().center);
                target += comp.Velocity * Time.fixedDeltaTime;
                target.y = height;
                Debug.DrawLine(origin, target, Color.blue);
                if (comp.owner.GetComponent<BasePlayer>().teamIndex != tankPlayer.teamIndex
                    && (target - origin).magnitude < minDistance
                        && Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                        && raycastHit.collider.gameObject == tankPlayer.gameObject)
                {
                    Vector3 delta = origin - target;
                    Vector3 project = Vector3.Project(comp.Velocity, delta);
                    Vector3 reflect = Vector3.Reflect(comp.Velocity, -project.normalized);
                    float distance = delta.magnitude / project.magnitude * reflect.magnitude;
                    Debug.DrawLine(origin, target, Color.green);
                    target = origin + reflect.normalized * distance;

                    if (distance < minDistance
                        && !Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, distance, ~layerMask))
                    {
                        minDistance = distance;
                        hitPos = target;
                    }
                }
                bl.GetComponent<Collider>().enabled = true;
            }
            if (minDistance != ((tankPlayer.currentBullet == 2) ? 3 : 1) * bulletSpeed)
            {
                Debug.DrawLine(origin, hitPos, Color.red);
                hitPos -= origin;

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);

                return true;
            }

            return false;
        }

        private bool ExpectTankRotate(ref Vector2 turnDir)
        {
            LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
            float height = tankPlayer.shotPos.position.y;
            //float radius = GameObject.FindGameObjectWithTag("Bullet").GetComponentInParent<SphereCollider>().radius;
            float bulletRadius = 0.21f;
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            RaycastHit raycastHit;
            Vector3 shootDelta = tankPlayer.shotPos.position - tankPlayer.Position;
            shootDelta.y = 0;
            float shootMore = shootDelta.magnitude;
            float minDistance = ((tankPlayer.currentBullet == 2) ? 3.0f : 1.0f) * bulletSpeed + shootMore;
            Vector3 origin = tankPlayer.Position;
            origin += tankPlayer.GetComponent<NavMeshAgent>().velocity * Time.fixedDeltaTime;
            origin.y = height;
            Vector3 hitPos = Vector3.zero;
            bool found = false;
            foreach (var pl in allplayer)
            {
                var comp = pl.GetComponent<BasePlayer>();
                if (comp.teamIndex != tankPlayer.teamIndex && comp.IsAlive)
                {
                    pl.GetComponent<Collider>().enabled = false;
                    //Vector3 target = comp.Position;
                    Vector3 target = comp.transform.TransformPoint(comp.GetComponent<BoxCollider>().center);
                    //Vector3 compVelocity = comp.Velocity;
                    Vector3 compVelocity = comp.GetComponent<NavMeshAgent>().velocity;
                    target += compVelocity * Time.fixedDeltaTime;
                    target.y = height;

                    if ((lastPosition[comp.teamIndex] - comp.Position).magnitude < tankSpeed * Time.fixedDeltaTime / 2)
                    {
#if UNITY_EDITOR
                        Debug.DrawRay(target, origin - target, Color.white);
#endif
                        if ((target - origin).magnitude < minDistance
                            && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                        || raycastHit.collider.gameObject == tankPlayer.gameObject))
                        {
                            minDistance = (target - origin).magnitude;
                            hitPos = target;
                            found = true;
                            //Debug.DrawLine(origin, hitPos, Color.blue);
                        }
                    }

                    else
                    {
#if UNITY_EDITOR
                        Debug.DrawLine(target, target + compVelocity * 0.5f, getTeamColor(comp.teamIndex), Time.fixedDeltaTime);
#endif
                        if ((target - origin).magnitude < minDistance)
                        {
                            Vector3 delta = origin - target;
                            float theta = Vector3.Angle(delta, compVelocity) * Mathf.Deg2Rad;
                            float alpha = Mathf.Asin(tankSpeed / bulletSpeed * Mathf.Sin(theta));
                            float time = Mathf.Sin(alpha) * delta.magnitude
                                / Mathf.Sin(Mathf.PI - alpha - theta) / tankSpeed;
                            Quaternion direction = Quaternion.LookRotation(-delta, Vector3.up)
                                * Quaternion.AngleAxis(alpha * Mathf.Rad2Deg, Vector3.up);

                            target += compVelocity * time;
                            float distance = (target - origin).magnitude;
#if UNITY_EDITOR
                            Debug.DrawRay(target, (origin - target).normalized * (distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2), Color.black);
#endif
                            if (distance < minDistance
                               && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit,
                               distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2, ~layerMask)
                               || raycastHit.collider.gameObject == tankPlayer.gameObject))
                            {
                                minDistance = distance;
                                hitPos = target;
                                found = true;
                                //Debug.DrawLine(origin, hitPos, Color.red);
                            }

                            //else
                            //{
                            //    target -= comp.Velocity * time;
                            //    if (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                            //    || raycastHit.collider.gameObject == tankPlayer.gameObject)
                            //    {
                            //        //Debug.DrawRay(target, origin - target, Color.white);
                            //        minDistance = (target - origin).magnitude;
                            //        hitPos = target;
                            //        //Debug.DrawLine(origin, hitPos, Color.blue);
                            //    }
                            //}
                        }
                    }

                    lastPosition[comp.teamIndex] = comp.Position;
                    pl.GetComponent<Collider>().enabled = true;
                }
            }
            if (found)
            {
#if UNITY_EDITOR
                Debug.DrawLine(origin, hitPos, Color.magenta);
#endif
                currentTarget = hitPos;
                hitPos -= origin;

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);
            }

            return found;
        }

        private void PredictPathInit(Vector3 currentPos, Vector3[] corners, out Vector3[] pos, out float[] timings)
        {
            pos = new Vector3[corners.Length + 1];
            timings = new float[pos.Length];
            pos[0] = currentPos;
            timings[0] = 0;
            for (int i = 1; i < pos.Length; i++)
            {
                pos[i] = corners[i - 1];
                timings[i] = timings[i - 1] + (pos[i] - pos[i - 1]).magnitude / tankSpeed;
            }
        }

        private bool AtPos(float timing, ref Vector3[] pos, ref float[] timings, out Vector3 at)
        {
            at = pos[0];
            if (timing > timings[timings.Length - 1])
            {
                at = pos[pos.Length - 1];
                return false;
            }

            for (int i = 1; i < timings.Length; i++)
            {
                if (timing < timings[i])
                {
                    at = Vector3.Lerp(pos[i - 1], pos[i], (timing - timings[i - 1]) / (timings[i] - timings[i - 1]));
                    return true;
                }
            }

            Debug.Assert(false);
            return true;
        }

        private Vector3 MyNextPos()
        {
            float height = tankPlayer.shotPos.position.y;
            Vector3 origin = tankPlayer.Position;
            if (agent.isStopped)
            {
                LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
                RaycastHit raycastHit;
                if (Physics.Raycast(origin, moveDir, out raycastHit, tankSpeed * Time.fixedDeltaTime, layerMask))
                {
                    origin = raycastHit.point;
                    origin.x -= moveDir.normalized.x * (transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).z / 2);
                    origin.z -= moveDir.normalized.y * (transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).z / 2);
                }
            }
            else
            {
                Vector3[] positions;
                float[] timings;
                PredictPathInit(origin, agent.path.corners, out positions, out timings);
                AtPos(Time.fixedDeltaTime, ref positions, ref timings, out origin);
            }
            origin.y = height;
            return origin;
        }

        private bool SeeTankRotate(ref Vector2 turnDir)
        {
            LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
            float height = tankPlayer.shotPos.position.y;
            //float radius = GameObject.FindGameObjectWithTag("Bullet").GetComponentInParent<SphereCollider>().radius;
            float bulletRadius = 0.25f;
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            RaycastHit raycastHit;
            Vector3 shootDelta = tankPlayer.shotPos.position - tankPlayer.Position;
            shootDelta.y = 0;
            Vector3 myNextPos = MyNextPos();
            float shootMore = shootDelta.magnitude;
            float minDistance = ((tankPlayer.currentBullet == 2) ? 3.0f : 1.0f) * bulletSpeed + shootMore;
            float minDistanceB = minDistance;
            float minDistanceA = minDistance * 0.75f;
            float minDistanceW = minDistance * 0.5f;
            //Vector3 origin = tankPlayer.Position;
            //origin += tankPlayer.GetComponent<NavMeshAgent>().velocity * Time.fixedDeltaTime;
            //origin.y = height;
            Vector3 hitPos = Vector3.zero;
            bool found = false;
            bool foundB = false;
            bool foundW = false;
            foreach (var pl in allplayer)
            {
                Vector3 origin = myNextPos;
                var comp = pl.GetComponent<BasePlayer>();
                if (comp.teamIndex != tankPlayer.teamIndex && comp.IsAlive && !WillDie(comp))
                {
                    pl.GetComponent<Collider>().enabled = false;
                    Vector3 target = comp.Position;
                    target.y = height;

                    NavMeshAgent ag = comp.GetComponent<NavMeshAgent>();
                    Vector3[] positions;
                    float[] timings;
                    PredictPathInit(target, ag.path.corners, out positions, out timings);

                    int iterations = 5;
                    origin = myNextPos + (target - origin).normalized * shootMore;
                    float timing = (target - origin).magnitude / bulletSpeed;
                    bool isOut = false;

                    if (!ag.isStopped)
                    {
                        for (int i = 0; i < iterations; i++)
                        {
                            if (AtPos(timing, ref positions, ref timings, out target))
                            {
                                origin = myNextPos + (target - origin).normalized * shootMore;
                                timing = (target - origin).magnitude / bulletSpeed;
                                //Debug.Log(timing.ToString());
                            }
                            else
                            {
                                isOut = true;
                                break;
                            }
                        }

                        if (!isOut)
                        {
#if UNITY_EDITOR
                            Debug.DrawRay(target, origin - target, Color.black);
#endif
                            if ((target - origin).magnitude < minDistanceB
                             && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                         || raycastHit.collider.gameObject == tankPlayer.gameObject))
                            {
                                minDistanceB = (target - origin).magnitude;
                                hitPos = target;
                                found = true;
                                foundB = true;
                                lastTargetPlayer = comp;
                                //Debug.DrawLine(origin, hitPos, Color.blue);
                            }
                        }

                        else if (!foundB && !foundW)
                        {
                            target = comp.transform.TransformPoint(comp.GetComponent<BoxCollider>().center);
                            //Vector3 compVelocity = comp.Velocity;
                            Vector3 compVelocity = comp.GetComponent<NavMeshAgent>().velocity;
                            target.y = height;
#if UNITY_EDITOR
                            Debug.DrawLine(target, target + compVelocity * 0.5f, getTeamColor(comp.teamIndex), Time.fixedDeltaTime);
#endif
                            Vector3 delta = origin - target;
                            float theta = Vector3.Angle(delta, compVelocity) * Mathf.Deg2Rad;
                            float alpha = Mathf.Asin(tankSpeed / bulletSpeed * Mathf.Sin(theta));
                            float time = Mathf.Sin(alpha) * delta.magnitude
                                / Mathf.Sin(Mathf.PI - alpha - theta) / tankSpeed;
                            Quaternion direction = Quaternion.LookRotation(-delta, Vector3.up)
                                * Quaternion.AngleAxis(alpha * Mathf.Rad2Deg, Vector3.up);

                            target += compVelocity * time;
                            float distance = (target - origin).magnitude;
#if UNITY_EDITOR
                            Debug.DrawRay(target, (origin - target).normalized * (distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2), Color.red);
#endif
                            if (distance < minDistanceA
                               && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit,
                               distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2, ~layerMask)
                               || raycastHit.collider.gameObject == tankPlayer.gameObject))
                            {
                                minDistanceA = distance;
                                hitPos = target;
                                found = true;
                                //Debug.DrawLine(origin, hitPos, Color.red);
                            }

                        }
                    }

                    if (!foundB)
                    {
                        target = comp.transform.TransformPoint(comp.GetComponent<BoxCollider>().center);
                        //Vector3 compVelocity = comp.Velocity;
                        Vector3 compVelocity = comp.GetComponent<NavMeshAgent>().velocity;
                        target += compVelocity * Time.fixedDeltaTime;
                        target.y = height;
                        origin = myNextPos + (target - origin).normalized * shootMore;
#if UNITY_EDITOR
                        Debug.DrawRay(target, origin - target, Color.white);
#endif
                        if ((target - origin).magnitude < minDistanceW
                            && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                            || raycastHit.collider.gameObject == tankPlayer.gameObject))
                        {
                            minDistanceW = (target - origin).magnitude;
                            hitPos = target;
                            found = true;
                            foundW = true;
                            lastTargetPlayer = comp;
                            //Debug.DrawLine(origin, hitPos, Color.blue);
                        }
                    }

                    lastPosition[comp.teamIndex] = comp.Position;
                    pl.GetComponent<Collider>().enabled = true;
                }
            }
            if (found)
            {
                Vector3 origin = myNextPos + (hitPos - myNextPos).normalized * shootMore;
#if UNITY_EDITOR
                Debug.DrawLine(origin, hitPos, Color.magenta);
#endif
                currentTarget = hitPos;
                hitPos -= origin;

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);
            }

            return found;
        }

        private Vector3 TargetNextPos(BasePlayer comp)
        {
            float height = tankPlayer.shotPos.position.y;
            LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
            RaycastHit raycastHit;
            Vector3 target = comp.transform.TransformPoint(comp.GetComponent<BoxCollider>().center);
            Vector3 compVelocity = comp.Velocity;
            //target += compVelocity * Time.fixedDeltaTime;
            if (Physics.Raycast(target, compVelocity, out raycastHit, tankSpeed * Time.fixedDeltaTime, layerMask))
            {
                target = raycastHit.point;
                target.x -= compVelocity.normalized.x * (transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).z / 2);
                target.z -= compVelocity.normalized.y * (transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).z / 2);
            }
            target.y = height;
            return target;
        }

        private bool NoSeeTankRotate(ref Vector2 turnDir)
        {
            LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
            float height = tankPlayer.shotPos.position.y;
            //float radius = GameObject.FindGameObjectWithTag("Bullet").GetComponentInParent<SphereCollider>().radius;
            float bulletRadius = 0.25f;
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            RaycastHit raycastHit;
            Vector3 shootDelta = tankPlayer.shotPos.position - tankPlayer.Position;
            shootDelta.y = 0;
            Vector3 myNextPos = MyNextPos();
            float shootMore = shootDelta.magnitude;
            float minDistance = ((tankPlayer.currentBullet == 2) ? 3.0f : 1.0f) * bulletSpeed + shootMore;
            float minDistanceB = minDistance;
            float minDistanceW = minDistance * 0.5f;
            Vector3 hitPos = Vector3.zero;
            bool found = false;
            bool foundB = false;
            bool foundW = false;
            foreach (var pl in allplayer)
            {
                Vector3 origin = myNextPos;
                var comp = pl.GetComponent<BasePlayer>();
                if (comp.teamIndex != tankPlayer.teamIndex && comp.IsAlive)// && !WillDie(comp))
                {
                    pl.GetComponent<Collider>().enabled = false;
                    Vector3 target = comp.transform.TransformPoint(comp.GetComponent<BoxCollider>().center);
                    target.y = height;
                    //Vector3 compVelocity = comp.Velocity;
                    Vector3 compVelocity = comp.GetComponent<NavMeshAgent>().velocity;
                    compVelocity.y = 0;
#if UNITY_EDITOR
                    Debug.DrawLine(target, target + compVelocity * 0.5f, getTeamColor(comp.teamIndex), Time.fixedDeltaTime);
#endif

                    if ((lastPosition[comp.teamIndex] - comp.Position).magnitude < tankSpeed * Time.fixedDeltaTime / 3.0f)
                    {
                        origin = myNextPos + (target - myNextPos).normalized * shootMore;
#if UNITY_EDITOR
                        Debug.DrawRay(target, origin - target, Color.white);
#endif
                        if ((target - origin).magnitude < minDistanceW
                            && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                        || raycastHit.collider.gameObject == tankPlayer.gameObject))
                        {
                            minDistanceW = (target - origin).magnitude;
                            hitPos = target;
                            foundW = true;
                            //Debug.DrawLine(origin, hitPos, Color.blue);
                        }
                    }

                    else
                    {
                        target = comp.transform.TransformPoint(comp.GetComponent<BoxCollider>().center);
                        target.y = height;
                        origin = myNextPos + (target - myNextPos).normalized * shootMore;
                        Vector3 delta = origin - target;
                        float theta = Vector3.Angle(delta, compVelocity) * Mathf.Deg2Rad;
                        float alpha = Mathf.Asin(tankSpeed / bulletSpeed * Mathf.Sin(theta));
                        float time = Mathf.Sin(alpha) * delta.magnitude
                            / Mathf.Sin(Mathf.PI - alpha - theta) / tankSpeed;
                        Quaternion direction = Quaternion.LookRotation(-delta, Vector3.up)
                            * Quaternion.AngleAxis(alpha * Mathf.Rad2Deg, Vector3.up);

                        target += compVelocity * time;
                        origin = myNextPos + (target - myNextPos).normalized * shootMore;
                        float distance = (target - origin).magnitude;
#if UNITY_EDITOR
                        Debug.DrawRay(target, (origin - target).normalized * (distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2), Color.black);
#endif
                        if (distance < minDistanceB
                           && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit,
                           distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2, ~layerMask)
                           || raycastHit.collider.gameObject == tankPlayer.gameObject))
                        {
                            minDistanceB = distance;
                            hitPos = target;
                            found = true;
                            foundB = true;
                            //Debug.DrawLine(origin, hitPos, Color.red);
                        }

                        else if (!foundB)
                        {
                            target = TargetNextPos(comp);
                            origin = myNextPos + (target - myNextPos).normalized * shootMore;
#if UNITY_EDITOR
                            Debug.DrawRay(target, origin - target, Color.white);
#endif
                            if ((target - origin).magnitude < minDistanceW
                                && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                                || raycastHit.collider.gameObject == tankPlayer.gameObject))
                            {
                                minDistanceW = (target - origin).magnitude;
                                hitPos = target;
                                found = true;
                                foundW = true;
                                lastTargetPlayer = comp;
                                //Debug.DrawLine(origin, hitPos, Color.blue);
                            }
                        }
                    }

                    lastPosition[comp.teamIndex] = comp.Position;
                    pl.GetComponent<Collider>().enabled = true;
                }
            }
            if (found)
            {
                Vector3 origin = myNextPos + (hitPos - myNextPos).normalized * shootMore;
#if UNITY_EDITOR
                Debug.DrawLine(origin, hitPos, Color.magenta);
#endif
                currentTarget = hitPos;
                hitPos -= origin;

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);
            }

            return found;
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.color = getTeamColor(tankPlayer.teamIndex);
            Gizmos.DrawWireSphere(lastTarget, 0.5f);
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(MyNextPos(), 0.5f);
            if (tankPlayer.bShootable)
            {
                Vector3 shootDelta = tankPlayer.shotPos.position - tankPlayer.Position;
                shootDelta.y = 0;
                float shootMore = shootDelta.magnitude;
                Gizmos.DrawWireSphere(tankPlayer.Position,
                    ((tankPlayer.currentBullet == 2) ? 3 : 1) * bulletSpeed + shootMore);
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(tankPlayer.Position,
                    (((tankPlayer.currentBullet == 2) ? 3 : 1) * bulletSpeed + shootMore) * 0.5f);
            }

            int myTeamIndex = tankPlayer.teamIndex;

            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i] != null && agents[i].enabled)
                {
                    Gizmos.color = getTeamColor(i);
                    //if (agents[i].GetComponent<BasePlayer>().teamIndex != myTeamIndex)
                    //{
                    //    Debug.Log(agents[i].path.corners.Length.ToString());
                    //    Debug.Log(agents[i].GetComponent<BasePlayer>().Position);
                    //    Debug.Log(agents[i].nextPosition.ToString());
                    //}

                    foreach (Vector3 pos in agents[i].path.corners)
                    {
                        Gizmos.DrawWireCube(pos, new Vector3(0.5f, 0.5f, 0.5f));
                        //if (agents[i].GetComponent<BasePlayer>().teamIndex != myTeamIndex)
                        //    Debug.Log(pos.ToString());
                    }
                    Gizmos.DrawWireCube(agents[i].destination, new Vector3(1f, 1f, 1f));
                }
            }

            foreach (var ag in agents)
            {

            }
        }

        private void DrawTrailsInit()
        {
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            foreach (var pl in allplayer)
            {
                var comp = pl.GetComponent<BasePlayer>();
                lastAimPosition[comp.teamIndex] = comp.Position;
            }
            InvokeRepeating("DrawTrails", Time.fixedDeltaTime, Time.fixedDeltaTime);
        }

        private Color getTeamColor(int index)
        {
            Color color = Color.white;
            switch (index)
            {
                case 0:
                    color = Color.red;
                    break;
                case 1:
                    color = Color.cyan;
                    break;
                case 2:
                    color = Color.green;
                    break;
                case 3:
                    color = Color.yellow;
                    break;
            }
            return color;
        }

        private void DrawTrails()
        {
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            foreach (var pl in allplayer)
            {
                var comp = pl.GetComponent<BasePlayer>();
                int index = comp.teamIndex;
                if (comp.IsAlive)
                {
                    Color color = getTeamColor(index);
                    Debug.DrawLine(lastAimPosition[index], comp.Position, color, 1.0f);
                }
                lastAimPosition[index] = comp.Position;
            }
        }

#endif

        protected override void OnStop()
        {
            base.OnStop();

            GameManager.GetInstance().ui.controls[0].OnEndDrag(null);
            GameManager.GetInstance().ui.controls[1].OnEndDrag(null);
        }

        protected override void OnRun()
        {
            base.OnRun();

            GameManager.GetInstance().ui.controls[0].OnEndDrag(null);
            GameManager.GetInstance().ui.controls[1].OnEndDrag(null);
        }

    }
}

