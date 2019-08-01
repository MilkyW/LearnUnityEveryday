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
        private float lazy = 0;
        GameObject toAvoid = null;

        private bool startPhase = true;

        [System.Serializable]
        public class Arguments
        {
            public float bulletRadius = 0.2f;
            public float tankWidth = 1.2f;
            public float tankLength = 1.7f;
        }
        public Arguments arguments;

        [System.Serializable]
        public class BulletInfo
        {
            public BasePlayer target;
            public float expireTime;
            public bool inited;
            public bool found;
        }
        public Dictionary<Bullet, BulletInfo> bulletInfos = new Dictionary<Bullet, BulletInfo>();
        public BasePlayer lastTargetPlayer;

        [System.Serializable]
        public class EnemyInfo
        {
            public int ammo;
            public int hitPoints;
            public bool willDie;
            public bool found;
        }
        public Dictionary<BasePlayer, EnemyInfo> enemyInfos = new Dictionary<BasePlayer, EnemyInfo>();

        [System.Serializable]
        public class ItemInfo
        {
            public float respawnTime;
            public bool inited;
            public bool found;
        }
        public Dictionary<Collectible, ItemInfo> itemInfos = new Dictionary<Collectible, ItemInfo>();

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
                    if (!bi.inited)
                    {
                        if (bullet.owner == tankPlayer.gameObject)
                            bi.target = lastTargetPlayer;
                        bi.expireTime = Time.time + bullet.despawnDelay;
                        bi.inited = true;
                    }
                    bi.found = true;
                }
                else
                {
                    BulletInfo bi = new BulletInfo();
                    bulletInfos.Add(bullet, bi);
                    if (bullet.owner == tankPlayer.gameObject)
                        bi.target = lastTargetPlayer;
                    bi.expireTime = Time.time + bullet.despawnDelay;
                    bi.inited = true;
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

        private void UpdateEnemyInfo()
        {
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            foreach (var pls in enemyInfos)
            {
                pls.Value.found = false;
            }
            foreach (var pl in allplayer)
            {
                BasePlayer player = pl.GetComponent<BasePlayer>();
                if (enemyInfos.ContainsKey(player))
                {
                    var pi = enemyInfos[player];
                    pi.found = true;
                }
                else
                {
                    EnemyInfo ei = new EnemyInfo();
                    enemyInfos.Add(player, ei);
                    ei.found = true;
                }
            }
            foreach (var eis in enemyInfos)
            {
                if (eis.Value.found == false || !eis.Key.IsAlive)
                {
                    eis.Value.willDie = false;
                }
            }
        }

        private void UpdateItemInfo()
        {
            GameObject[] allitem = GameObject.FindGameObjectsWithTag("Powerup");
            foreach (var iis in itemInfos)
            {
                iis.Value.found = false;
            }
            foreach (var it in allitem)
            {
                Collectible co = it.GetComponent<Collectible>();
                ItemInfo iti = null;
                if (itemInfos.ContainsKey(co))
                {
                    iti = itemInfos[co];
                }
                else
                {
                    iti = new ItemInfo();
                    itemInfos.Add(co, iti);
                }
                iti.inited = false;
                iti.found = true;
            }
            foreach (var iti in itemInfos)
            {
                if (!iti.Value.found && !iti.Value.inited)
                {
                    iti.Value.respawnTime
                        = Time.time + ((iti.Key.GetType().Name == "PowerupHealth") ? 10.0f : 15.0f);
                    iti.Value.inited = true;
                    //Debug.Log(iti.Key.name.ToString() + iti.Value.respawnTime.ToString());
                    //Debug.Log(iti.Value.respawnTime.ToString());
                }
                if (!iti.Value.found && iti.Value.respawnTime < Time.time)
                {
                    iti.Value.respawnTime
                        = Time.time + ((iti.Key.GetType().Name == "PowerupHealth") ? 10.0f : 15.0f);
                }
            }
        }

        private bool WillDie(BasePlayer target)
        {
            LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
            int health = target.health;
            int shield = target.shield;
            float height = tankPlayer.shotPos.position.y;
            Vector3 tankPosition = target.transform.TransformPoint(target.GetComponent<BoxCollider>().center);
            tankPosition.y = height;
            Vector3 tankVelocity = target.Velocity;
            tankVelocity.y = 0;
            float myMinReachTime = (tankPlayer.Position - target.Position).magnitude / bulletSpeed;
            float minDistance = (arguments.tankWidth * 0.5f + arguments.bulletRadius + 0.1f);

            foreach (var bis in bulletInfos)
            {
                if (bis.Key.gameObject.activeInHierarchy && bis.Value.inited && health > 0)
                {
                    Vector3 bulletVelocity = bis.Key.Velocity;
                    bulletVelocity.y = 0;
                    Vector3 bulletPosition = bis.Key.transform.TransformPoint(bis.Key.GetComponent<SphereCollider>().center);
                    bulletPosition.y = height;

                    float maxReachTime = bis.Value.expireTime - Time.time;
                    RaycastHit raycastHit;
                    float bulletRadius = 0.25f;
                    bis.Key.GetComponent<Collider>().enabled = false;
                    if (Physics.SphereCast(bulletPosition, bulletRadius, bulletVelocity, out raycastHit, bulletSpeed * maxReachTime - 0.2f, ~layerMask))
                    {
                        maxReachTime = (bulletPosition - raycastHit.point).magnitude / bulletSpeed;
                    }
                    bis.Key.GetComponent<Collider>().enabled = true;

                    if (maxReachTime < myMinReachTime)
                    {
                        Vector3 tankP = tankPosition;
                        for (float i = 0; i < maxReachTime + Time.fixedDeltaTime; i += Time.fixedDeltaTime)
                        {
                            if ((tankP - bulletPosition).magnitude < minDistance)
                            {
                                if (shield > 0)
                                    shield--;
                                else
                                    health -= bis.Key.damage;
                                break;
                            }
                            tankP += tankVelocity * Time.fixedDeltaTime;
                            bulletPosition += bulletVelocity * Time.fixedDeltaTime;
                        }
                    }
                }
            }

            if (health <= 0)
            {
                if (enemyInfos.ContainsKey(target))
                    enemyInfos[target].willDie = true;
                return true;
            }

            //if (target.health > 5)
            //    return false;

            //foreach (var bis in bulletInfos)
            //{
            //    if (bis.Key.enabled && bis.Value.inited && bis.Value.target == target
            //        && target.health < bis.Key.damage + 1)
            //    {
            //        return true;
            //    }
            //}

            if (enemyInfos.ContainsKey(target))
                enemyInfos[target].willDie = false;
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

        private void Update()
        {
            UpdateBulletInfo();
            UpdateEnemyInfo();
            UpdateItemInfo();
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

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
                //if (agent.path.corners.Length > 1)
                //    moveDir = agent.path.corners[1] - tankPlayer.Position;
                MoveWhere(ref turnDir);
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

        private void MoveWhere(ref Vector2 moveDir)
        {
            if (Time.time > lazy || !toAvoid || !toAvoid.activeInHierarchy)
            {
                Vector3 tankPosition = tankPlayer.Position;
                tankPosition.y = 0;

                float minShieldTimeCost = 300;
                float minBulletTimeCost = 300;
                float minHealthTimeCost = 300;
                Collectible shield = null;
                Collectible bullet = null;
                Collectible health = null;

                if (startPhase)
                {
                    foreach (var iti in itemInfos)
                    {
                        if (iti.Key.GetType().Name == "PowerupShield")
                        {
                            shield = iti.Key;
                            break;
                        }
                    }
                    if (shield == null || shield.gameObject.activeInHierarchy)
                    {
                        agent.isStopped = false;
                        tankPlayer.MoveTo(Vector3.zero);
                        return;
                    }
                    else
                    {
                        startPhase = false;
                    }
                }

                foreach (var iti in itemInfos)
                {
                    if (iti.Key.GetType().Name == "PowerupShield" && tankPlayer.shield < 3)
                    {
                        Vector3 coPosition = iti.Key.transform.position;
                        coPosition.y = 0;
                        float timeCost = (coPosition - tankPosition).magnitude / tankSpeed;
                        float timeWait = iti.Value.respawnTime - Time.time;
                        if (timeWait > timeCost)
                            timeCost = timeWait;
                        if (timeCost < minShieldTimeCost)
                        {
                            minShieldTimeCost = timeCost;
                            shield = iti.Key;
                        }
                    }

                    else if (iti.Key.GetType().Name == "PowerupBullet" && tankPlayer.currentBullet == 0)
                    {
                        Vector3 coPosition = iti.Key.transform.position;
                        coPosition.y = 0;
                        float timeCost = (coPosition - tankPosition).magnitude / tankSpeed;
                        if (((PowerupBullet)iti.Key).bulletIndex == 1)
                            timeCost *= 0.75f;
                        float timeWait = iti.Value.respawnTime - Time.time;
                        if (timeWait > timeCost)
                            timeCost = timeWait;
                        if (timeCost < minBulletTimeCost)
                        {
                            minBulletTimeCost = timeCost;
                            bullet = iti.Key;
                        }
                    }

                    else if (iti.Key.GetType().Name == "PowerupHealth" && tankPlayer.health < 10)
                    {
                        Vector3 coPosition = iti.Key.transform.position;
                        coPosition.y = 0;
                        float timeCost = (coPosition - tankPosition).magnitude / tankSpeed;
                        float timeWait = iti.Value.respawnTime - Time.time;
                        if (timeWait > timeCost)
                            timeCost = timeWait;
                        if (timeCost < minHealthTimeCost)
                        {
                            minHealthTimeCost = timeCost;
                            health = iti.Key;
                        }
                    }
                }

                float minCost = 300;
                Vector3 target = Vector3.zero;

                if (shield || bullet || health)
                {
                    minShieldTimeCost *= 1.0f * ((float)tankPlayer.health / tankPlayer.maxHealth) * ((float)tankPlayer.health / tankPlayer.maxHealth);
                    minBulletTimeCost *= 1.3f;
                    minHealthTimeCost *= 1.7f * ((float)tankPlayer.health / tankPlayer.maxHealth);
                    if (minShieldTimeCost < minBulletTimeCost && minShieldTimeCost < minHealthTimeCost)
                    {
                        minCost = minShieldTimeCost;
                        target = shield.transform.position;
                    }
                    else if (minBulletTimeCost < minHealthTimeCost)
                    {
                        minCost = minBulletTimeCost;
                        target = bullet.transform.position;
                    }
                    else
                    {
                        minCost = minHealthTimeCost;
                        target = health.transform.position;
                    }
                }

                bool foundHorror = false;

                GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
                foreach (var pl in allplayer)
                {
                    BasePlayer player = pl.GetComponent<BasePlayer>();
                    if (player.teamIndex != tankPlayer.teamIndex && player.IsAlive
                        && (!enemyInfos.ContainsKey(player) || !enemyInfos[player].willDie))
                    {
                        Vector3 plPosition = player.Position;
                        plPosition.y = 0;
                        float timeCost = (plPosition - tankPosition).magnitude * 1.0f / tankSpeed;
                        int points = 0;
                        int ammo = player.shield;
                        int usefulAmmo = tankPlayer.ammo - ammo;
                        if (tankPlayer.currentBullet == 1 && usefulAmmo > 0)
                        {
                            if (player.health <= 5)
                            {
                                points = 3;
                                ammo += 1;
                            }
                            else if (usefulAmmo > 1 || player.health <= 8)
                            {
                                points = 4;
                                ammo += 2;
                            }
                            else
                            {
                                points = 5;
                                ammo += 3;
                            }
                        }
                        else
                        {
                            points = (player.health + 2) / 3 + 2;
                            ammo += (player.health + 2) / 3;
                        }

                        int hisPoints = 0;
                        int hisAmmo = tankPlayer.shield;
                        usefulAmmo = player.ammo - hisAmmo;
                        if (player.currentBullet == 1 && usefulAmmo > 0)
                        {
                            if (tankPlayer.health <= 5)
                            {
                                hisPoints = 3;
                                hisAmmo += 1;
                            }
                            else if (usefulAmmo > 1 || tankPlayer.health <= 8)
                            {
                                hisPoints = 4;
                                hisAmmo += 2;
                            }
                            else
                            {
                                hisPoints = 5;
                                hisAmmo += 3;
                            }
                        }
                        else
                        {
                            hisPoints = (tankPlayer.health + 2) / 3 + 2;
                            hisAmmo += (tankPlayer.health + 2) / 3;
                        }

                        //if ((float)points / ammo > (float)hisPoints / hisAmmo)
                        if (ammo < hisAmmo && points / (float)ammo > hisPoints / (float)hisAmmo)
                        {
                            timeCost *= ((float)ammo / points);
                            if (timeCost < minCost)
                            {
                                minCost = timeCost;
                                target = plPosition;
                            }
                        }

                        else if (timeCost < Time.fixedDeltaTime * 10 && timeCost < minCost)
                        {
                            toAvoid = player.gameObject;
                            foundHorror = true;
                        }

                    }
                }

                agent.isStopped = false;
                target.y = 0;
                tankPlayer.MoveTo(target);

                RaycastHit raycastHit;
                LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
                float height = tankPlayer.shotPos.position.y;
                tankPosition = tankPlayer.transform.TransformPoint(tankPlayer.GetComponent<BoxCollider>().center);
                tankPosition.y = height;
                Vector3 tankVelocity = tankPlayer.Velocity;
                tankVelocity.y = 0;
                float minAvoidTime = Time.fixedDeltaTime;
                float minReachTime = 3;
                float minDistance = (arguments.tankLength * 0.5f + arguments.bulletRadius + 0.25f);

                foreach (var bis in bulletInfos)
                {
                    if (bis.Key.gameObject.activeInHierarchy && bis.Value.inited
                        && bis.Key.owner.GetComponent<BasePlayer>().teamIndex != tankPlayer.teamIndex)
                    {
                        Vector3 bulletVelocity = bis.Key.Velocity;
                        bulletVelocity.y = 0;
                        Vector3 bulletPosition = bis.Key.transform.TransformPoint(bis.Key.GetComponent<SphereCollider>().center);
                        bulletPosition.y = height;

                        float maxReachTime = bis.Value.expireTime - Time.time;
                        float bulletRadius = 0.22f;
                        bis.Key.GetComponent<Collider>().enabled = false;
                        if (Physics.SphereCast(bulletPosition, bulletRadius, bulletVelocity, out raycastHit, bulletSpeed * maxReachTime, ~layerMask))
                        {
                            maxReachTime = (bulletPosition - raycastHit.point).magnitude / bulletSpeed;
                        }
                        bis.Key.GetComponent<Collider>().enabled = true;

                        Vector3 tankP = tankPosition;
                        for (float i = 0; i < maxReachTime + Time.fixedDeltaTime - 0.001f && i < minReachTime; i += Time.fixedDeltaTime)
                        {
                            if ((tankP - bulletPosition).magnitude < minDistance)
                            {
                                if (i > minAvoidTime)
                                {
                                    toAvoid = bis.Key.gameObject;
                                    minReachTime = i;
                                    foundHorror = true;
                                }
                                //else
                                //{
                                //    toAvoid = bis.Key.owner;
                                //}
                                break;
                            }
                            tankP += tankVelocity * Time.fixedDeltaTime;
                            bulletPosition += bulletVelocity * Time.fixedDeltaTime;
                        }
                    }
                }

                if (foundHorror)
                {
                    if (toAvoid.GetComponent<Bullet>())
                    {
                        lazy = Time.time + minReachTime;
                        agent.isStopped = true;
                        Vector3 horror = toAvoid.transform.position;
                        horror.y = 0;
                        tankPlayer.transform.LookAt(horror);
                        tankPlayer.transform.Rotate(new Vector3(0, 90, 0));
                        Vector3 delta0 = Vector3.zero;
                        Vector3 delta1 = Vector3.zero;

                        tankPlayer.GetComponent<Collider>().enabled = false;
                        if (Physics.BoxCast(tankPosition, new Vector3(arguments.tankWidth, height, arguments.tankLength) / 2, tankPlayer.transform.forward,
                             out raycastHit, tankPlayer.transform.rotation, 100, ~layerMask))
                        {
                            delta0 = tankPosition - raycastHit.point;
                            delta0.y = 0;
                        }
                        tankPlayer.transform.Rotate(new Vector3(0, 180, 0));
                        if (Physics.BoxCast(tankPosition, new Vector3(arguments.tankWidth, height, arguments.tankLength) / 2, tankPlayer.transform.forward,
                            out raycastHit, tankPlayer.transform.rotation, 100, ~layerMask))
                        {
                            delta1 = tankPosition - raycastHit.point;
                            delta1.y = 0;
                        }
                        tankPlayer.GetComponent<Collider>().enabled = true;

                        if (delta0.magnitude > delta1.magnitude)
                        {
                            tankPlayer.transform.Rotate(new Vector3(0, 180, 0));
                        }

                        Vector3 delta = tankPlayer.transform.forward;
                        delta.y = 0;
                        moveDir = delta;
                        tankPlayer.SimpleMove(moveDir);
                    }

                    else if (toAvoid.GetComponent<BasePlayer>())
                    {
                        lazy = Time.time + Time.fixedDeltaTime;
                        agent.isStopped = false;
                        float radius = 20;
                        float angle = Random.value * 360;
                        Vector3 toPosition = toAvoid.transform.position
                            + (tankPlayer.Position - toAvoid.transform.position) * 2 + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
                        toPosition.y = 0;
                        tankPlayer.MoveTo(toPosition);
                    }

                }
            }

            else
            {
                if (agent.isStopped)
                {
                    if (toAvoid.GetComponent<Bullet>())
                    {
                        tankPlayer.SimpleMove(moveDir);
                    }
                }
                else
                {
                    float radius = 20;
                    float angle = Random.value * 360;
                    Vector3 toPosition = toAvoid.transform.position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
                    toPosition.y = 0;
                    tankPlayer.MoveTo(toPosition);
                }
            }

            return;
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
                            Debug.DrawRay(target, (origin - target).normalized * (distance - arguments.tankWidth * 0.5f), Color.black);
#endif
                            if (distance < minDistance
                               && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit,
                               distance - arguments.tankWidth * 0.5f, ~layerMask)
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
            if (tankPlayer.IsAlive && agent.isStopped)
            {
                LayerMask layerMask = LayerMask.GetMask("Powerup") | LayerMask.GetMask("Bullet");
                RaycastHit raycastHit;
                if (Physics.Raycast(origin, moveDir, out raycastHit, tankSpeed * Time.fixedDeltaTime, ~layerMask))
                {
                    origin = raycastHit.point;
                    origin.x -= moveDir.normalized.x * arguments.tankWidth * 0.5f;
                    origin.z -= moveDir.normalized.y * arguments.tankWidth * 0.5f;
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
                            Debug.DrawRay(target, (origin - target).normalized * (distance - arguments.tankWidth * 0.5f), Color.red);
#endif
                            if (distance < minDistanceA
                               && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit,
                               distance - arguments.tankWidth * 0.5f, ~layerMask)
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
            if (Physics.Raycast(target, compVelocity, out raycastHit, tankSpeed * Time.fixedDeltaTime, ~layerMask))
            {
                target = raycastHit.point;
                target.x -= compVelocity.normalized.x * (arguments.tankWidth * 0.5f);
                target.z -= compVelocity.normalized.y * (arguments.tankWidth * 0.5f);
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
            float minDistance = ((tankPlayer.currentBullet == 2) ? 3.0f : 1.0f) * bulletSpeed;
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
                if (comp.teamIndex != tankPlayer.teamIndex && comp.IsAlive && !WillDie(comp))
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
                        if ((target - origin).magnitude < minDistanceB
                            && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                        || raycastHit.collider.gameObject == tankPlayer.gameObject))
                        {
                            minDistanceB = (target - origin).magnitude;
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
                        Debug.DrawRay(target, (origin - target).normalized * (distance - arguments.tankWidth * 0.5f), Color.black);
#endif
                        if (distance < minDistanceB
                           && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit,
                           distance - arguments.tankWidth * 0.5f, ~layerMask)
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
                Debug.DrawLine(origin, hitPos, getTeamColor(tankPlayer.teamIndex));
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
                if (agents[i] != null && agents[i].gameObject.activeInHierarchy)
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

            foreach (var eis in enemyInfos)
            {
                if (eis.Value.willDie)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireCube(eis.Key.Position, new Vector3(3f, 3f, 3f));
                }
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

