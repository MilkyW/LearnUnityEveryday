using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.AI;


namespace TanksMP
{

    public enum ControlMode
    {
        Slave,
        Master,
    }

    public abstract class BasePlayer : NetworkBehaviour
    {
        /// <summary>
        /// Player name synced across the network.
        /// </summary>
		[HideInInspector]
        [SyncVar]
        public string myName;

        /// <summary>
        /// UI Text displaying the player name.
        /// </summary>
        public Text label;

        /// <summary>
        /// Team value assigned by the server.
        /// </summary>
        [HideInInspector]
        [SyncVar]
        public int teamIndex;

        /// <summary>
        /// Current health value.
        /// </summary>
		[SyncVar(hook = "OnHealthChange")]
        public int health = 10;

        /// <summary>
        /// Maximum health value at game start.
        /// </summary>
        [HideInInspector]
        public int maxHealth;

        /// <summary>
        /// Current shield value absorbing hits.
        /// </summary>
		[SyncVar(hook = "OnShieldChange")]
        public int shield = 0;

        protected bool bIsAlive = true;

        /// <summary>
        /// Current turret rotation and shooting direction.
        /// </summary>
        [HideInInspector]
        [SyncVar(hook = "OnTurretRotation")]
        public int turretRotation;

        /// <summary>
        /// Amount of special ammunition left.
        /// </summary>
		[HideInInspector]
        [SyncVar]
        public int ammo = 0;

        /// <summary>
        /// Index of currently selected bullet.
        /// </summary>
        [HideInInspector]
        [SyncVar]
        public int currentBullet = 0;

        [System.NonSerialized]
        public BaseControl controller;

        // components

        /// <summary>
        /// UI Slider visualizing health value.
        /// </summary>
        public Slider healthSlider;

        /// <summary>
        /// UI Slider visualizing shield value.
        /// </summary>
        public Slider shieldSlider;

        /// <summary>
        /// Clip to play when a shot has been fired.
        /// </summary>
        public AudioClip shotClip;

        /// <summary>
        /// Clip to play on player death.
        /// </summary>
        public AudioClip explosionClip;

        /// <summary>
        /// Object to spawn on shooting.
        /// </summary>
        public GameObject shotFX;

        /// <summary>
        /// Object to spawn on player death.
        /// </summary>
        public GameObject explosionFX;

        /// <summary>
        /// Turret to rotate with look direction.
        /// </summary>
        public Transform turret;

        /// <summary>
        /// Position to spawn new bullets at.
        /// </summary>
        public Transform shotPos;

        /// <summary>
        /// Array of available bullets for shooting.
        /// </summary>
        public GameObject[] bullets;

        /// <summary>
        /// MeshRenderers that should be highlighted in team color.
        /// </summary>
        public MeshRenderer[] renderers;

        public NavMeshAgent agent;

        /// <summary>
        /// Reference to the camera following component.
        /// </summary>
        [HideInInspector]
        public FollowTarget camFollow;

        /// <summary>
        /// Last player gameobject that killed this one.
        /// </summary>
        [HideInInspector]
        public GameObject killedBy;
        
        public Vector3 Position { get { return transform.position; } }

        public Vector3 Velocity { get { return transform.forward * moveSpeed; } }

        public bool IsAlive { get { return bIsAlive; } }

        public ControlMode controlMode;
        // constants
        protected const float sendRate = 0.1f;
        protected const float fireRate = 0.75f;
        protected const float moveSpeed = 8f;

        // local vars
        protected float nextRotate;
        protected float nextFire;

        public void SetupController(GameObject botPrefab)
        {
            agent.speed = moveSpeed;

            if(botPrefab == null)
            {
                controlMode = ControlMode.Slave;
            }
            else
            {
                controlMode = ControlMode.Master;
                var inst = GameObject.Instantiate<GameObject>(botPrefab);
                inst.transform.parent = transform;
                var comp = inst.GetComponent<BaseControl>();
                controller = comp as BaseControl;
                if(controller != null)
                {
                    controller.Init(this);
                }
            }
        }

        //continously check for input on desktop platforms
        protected virtual void FixedUpdate()
        {
            if (!GameManager.GetInstance().isStart)
                return;

            //skip further calls for remote clients
            if (controlMode == ControlMode.Slave)
            {
                //keep turret rotation updated for all clients
                ApplyTurretRotation(turretRotation);
                return;
            }

            if (GameManager.GetInstance().IsGameOver())
            {
                if (controller != null)
                {
                    controller.Stop();
                }
            }

            // TODO: move to control begin
            if (controller != null)
            {
                controller.ControlFixUpdate();
            }
        }

        protected virtual void Update()
        {
            if (!GameManager.GetInstance().isStart)
                return;

            if (controller != null)
            {
                controller.ControlUpdate();
            }
        }


        // function for controls
        public virtual void SimpleMove(Vector2 direction = default(Vector2))
        {
            //if direction is not zero, rotate player in the moving direction relative to camera
            if (direction != Vector2.zero)
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y))
                                     * Quaternion.Euler(0, camFollow.camTransform.eulerAngles.y, 0);

            //create movement vector based on current rotation and speed
            Vector3 movementDir = transform.forward * moveSpeed * Time.deltaTime;
            agent.Move(movementDir);
        }

        public virtual void MoveTo(Vector3 destination)
        {
            agent.SetDestination(destination);
        }

        /* turret control & sync begin */
        public virtual void RotateTurret(float dx = 0, float dz = 0)
        {
            //don't rotate without values
            if (dx == 0 && dz == 0)
                return;

            //get rotation value as angle out of the direction we received
            int newRotation = (int)(Quaternion.LookRotation(new Vector3(dx, 0, dz)).eulerAngles.y + camFollow.camTransform.eulerAngles.y);

            //limit rotation value send rate to server:
            //only send every 'sendRate' seconds and skip minor incremental changes
            if (Time.time >= nextRotate)
            {
                //set next update timestamp and send to server
                nextRotate = Time.time + sendRate;
                turretRotation = newRotation;
                CmdRotateTurret(newRotation);
            }

            turret.rotation = Quaternion.Euler(0, newRotation, 0);
        }

        [Command]
        void CmdRotateTurret(int value)
        {
            turretRotation = value;
        }

        virtual protected void OnTurretRotation(int value)
        {
            if (controlMode == ControlMode.Slave)
            {
                ApplyTurretRotation(value);
            }
        }
        /* turret control & sync end */

        /* shoot & sync begin */
        public virtual void Shoot()
        {
            if (bShootable)
            {
                //set next shot timestamp
                nextFire = Time.time + fireRate;
                //send shot request with origin to server
                CmdShoot((short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10));
            }
        }

        public void AimAndShoot(Vector3 targetPosition) {
            if (bShootable)
            {
                RaycastHit hit;
                if(Physics.Linecast(Position, targetPosition, out hit))
                {
                    Vector3 delta = targetPosition - Position;
                    RotateTurret(delta.x, delta.z);
                    Shoot();
                }
            }
        }

        //Command creating a bullet on the server
        [Command]
        void CmdShoot(short xPos, short zPos)
        {
            //calculate center between shot position sent and current server position (factor 0.6f = 40% client, 60% server)
            //this is done to compensate network lag and smoothing it out between both client/server positions
            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(xPos / 10f, shotPos.position.y, zPos / 10f), 0.6f);

            //spawn bullet using pooling, locally
            GameObject obj = PoolManager.Spawn(bullets[currentBullet], shotCenter, turret.rotation);
            Bullet blt = obj.GetComponent<Bullet>();
            blt.owner = gameObject;

            //if it is a static bullet (such as a mine), overwrite Y-position with current tank height, so it spawns at the ground
            if (blt.speed == 0)
            {
                shotCenter.y = transform.position.y;
                obj.transform.position = shotCenter;
            }

            //spawn bullet networked
            NetworkServer.Spawn(obj, bullets[currentBullet].GetComponent<NetworkIdentity>().assetId);

            //check for current ammunition
            //if ran out of ammo: reset bullet
            if (currentBullet != 0)
            {
                ammo--;
                if (ammo <= 0)
                    currentBullet = 0;
            }

            //send event to all clients for spawning effects
            if (shotFX || shotClip)
                RpcOnShot();
        }


        //called on all clients after bullet spawn
        //spawn effects or sounds locally, if set
        [ClientRpc]
        protected void RpcOnShot()
        {
            if (shotFX) PoolManager.Spawn(shotFX, shotPos.position, Quaternion.identity);
            if (shotClip) AudioManager.Play3D(shotClip, shotPos.position, 0.1f);
        }

        /* shoot & sync end */

        /* tank state change begin */
        //hook for updating health locally
        virtual protected void OnHealthChange(int value)
        {
            health = value;
            healthSlider.value = (float)health / maxHealth;
        }


        //hook for updating shield locally
        virtual protected void OnShieldChange(int value)
        {
            shield = value;
            shieldSlider.value = shield;
        }
        /* tank state change end */


        /// <summary>
        /// Server only: calculate damage to be taken by the Player,
        /// triggers score increase and respawn workflow on death.
        /// </summary>
        [Server]
        public void TakeDamage(Bullet bullet)
        {
            //reduce shield on hit
            if (shield > 0)
            {
                shield--;
                return;
            }

            //substract health by damage
            health -= bullet.damage;

            //bullet killed the player
            if (health <= 0)
            {
                //the game is already over so don't do anything
                if (GameManager.GetInstance().IsGameOver()) return;

                //get killer and increase score for that team
                BasePlayer other = bullet.owner.GetComponent<BasePlayer>();
                GameManager.GetInstance().AddScore(ScoreType.Kill, other.teamIndex);
                GameManager.GetInstance().AddScore(ScoreType.KillCount, other.teamIndex);
                GameManager.GetInstance().AddScore(ScoreType.DeathCount, teamIndex);
                //the maximum score has been reached now
                if (GameManager.GetInstance().IsGameOver())
                {
                    //tell all clients the winning team
                    RpcGameOver(other.teamIndex);
                    return;
                }

                //the game is not over yet, reset runtime values
                //also tell all clients to despawn this player
                health = maxHealth;
                currentBullet = 0;

                //clean up collectibles on this player by letting them drop down
                Collectible[] collectibles = GetComponentsInChildren<Collectible>(true);
                for (int i = 0; i < collectibles.Length; i++)
                {
                    int changedIndex = GameManager.GetInstance().AddBufferedCollectible(collectibles[i].netId, transform.position, new NetworkInstanceId(0));
                    GameManager.GetInstance().OnCollectibleStateChanged(SyncListCollectible.Operation.OP_DIRTY, changedIndex);
                }

                //tell the dead player who killed him (owner of the bullet)
                short senderId = 0;
                if (bullet.owner != null)
                    senderId = (short)bullet.owner.GetComponent<NetworkIdentity>().netId.Value;

                RpcDie(senderId);
                GameManager.GetInstance().PendingRevive(this);
            }
            else
            {
                BasePlayer other = bullet.owner.GetComponent<BasePlayer>();
                GameManager.GetInstance().AddScore(ScoreType.Hit, other.teamIndex);
            }
        }



        [ClientRpc]
        public virtual void RpcGameOver(int teamIndex)
        {

        }
        [ClientRpc]
        public void RpcDie(short senderId)
        {
            gameObject.SetActive(false);
            killedBy = senderId > 0 ? ClientScene.FindLocalObject(new NetworkInstanceId((uint)senderId)) : null;
            bIsAlive = false;
            // TODO: ScoreBegin
            if (explosionFX != null)
            {
                //spawn death particles locally using pooling and colorize them in the player's team color
                GameObject particle = PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
                ParticleColor pColor = particle.GetComponent<ParticleColor>();
                if (pColor) pColor.SetColor(GameManager.GetInstance().teams[teamIndex].material.color);
            }

            //play sound clip on player death
            if (explosionClip != null) AudioManager.Play3D(explosionClip, transform.position);

            if (isServer)
            {
                agent.Warp(GameManager.GetInstance().GetSpawnPosition(teamIndex));
            }

            if (controller != null)
            {
                controller.Stop();
            }

            if (isLocalPlayer)
            {
                if(killedBy != null)
                {
                    camFollow.target = killedBy.transform;
                }
                camFollow.HideMask(true);
                GameManager.GetInstance().DisplayDeath();
            }
        }

        [ClientRpc]
        public virtual void RpcRespawn()
        {
            gameObject.SetActive(true);
            killedBy = null;
            bIsAlive = true;
            if (isServer)
            {
                agent.Warp(GameManager.GetInstance().GetSpawnPosition(teamIndex));
            }

            if (isLocalPlayer)
            {
                ResetTank();
            }

            if (controller != null)
            {
                controller.Run();
            }
        }

        protected virtual void ResetTank()
        {
            camFollow.target = turret;
            camFollow.HideMask(false);

            agent.Warp(GameManager.GetInstance().GetSpawnPosition(teamIndex));
            transform.rotation = Quaternion.identity;
        }
        /// <summary>
        /// Command telling the server that this client is ready for respawn.
        /// This is when the respawn delay is over or a video ad has been watched.
        /// </summary>
        [Command]
        public void CmdRespawn()
        {
            RpcRespawn();
        }

        public bool bShootable { get { return Time.time > nextFire; } }

        protected virtual void Awake()
        {
            maxHealth = health;
        }


        /* turret control & sync end */
        protected void ApplyTurretRotation(int value)
        {
            turretRotation = value;
            turret.rotation = Quaternion.Euler(0, turretRotation, 0);
        }

        protected virtual void SetupTank()
        {
            Team team = GameManager.GetInstance().teams[teamIndex];
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = team.material;
            }
        }
    }
}

