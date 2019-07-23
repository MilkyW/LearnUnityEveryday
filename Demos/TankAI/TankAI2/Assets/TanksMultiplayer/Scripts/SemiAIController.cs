using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TanksMP
{
    public class SemiAIController : BaseControl
    {
        public int shootTime = 0;
        public int hitTime = 0;
        public float hitRate = 0;
        private int lastScore = 0;
        private const float bulletSpeed = 18.0f;
        private const float tankSpeed = 8.0f;
        private Vector3 lastTarget;
        private Vector3 currentTarget;

        private Vector3[] lastPosition = new Vector3[4];

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
            Invoke("DrawTrailsInit", 2.0f);
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

#pragma warning disable 0219
            //movement variables
            Vector2 moveDir = Vector2.zero;
            Vector2 turnDir = Vector2.zero;
#pragma warning restore 0219

            moveDir = MoveJoystick();
            //turnDir = RotateMouse();
            //turnDir = RotateJoystick();
            //turnDir = SimpleRotate();
            //turnDir = ExpectBulletRotate();
            turnDir = ExpectTankRotate();

            //shoot bullet on left mouse click
            if (tankPlayer.bShootable && (turnDir != Vector2.zero || Input.GetButton("Fire1")))
            {
                tankPlayer.Shoot();
                shootTime++;
                hitRate = ((float)hitTime) / shootTime;
                lastTarget = currentTarget;
            }

            if (GameManager.GetInstance().GetScore(tankPlayer.teamIndex) != lastScore)
            {
                lastScore = GameManager.GetInstance().GetScore(tankPlayer.teamIndex);
                hitTime++;
                hitRate = ((float)hitTime) / shootTime;
            }

            //replicate input to mobile controls for illustration purposes
#if UNITY_EDITOR
            GameManager.GetInstance().ui.controls[0].position = moveDir;
            GameManager.GetInstance().ui.controls[1].position = turnDir;
#endif
        }

        private Vector3 MoveJoystick()
        {
            Vector2 moveDir = Vector2.zero;

            //reset moving input when no arrow keys are pressed down
            if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            {
                //MoveEnd();
            }
            else
            {
                //read out moving directions and calculate force
                moveDir.x = Input.GetAxis("Horizontal");
                moveDir.y = Input.GetAxis("Vertical");
                tankPlayer.SimpleMove(moveDir);
            }

            return moveDir;
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

        private Vector3 RotateJoystick()
        {
            Vector3 hitPos = Vector3.zero;

            if (Input.GetAxisRaw("HorizontalR") != 0 || Input.GetAxisRaw("VerticalR") != 0)
            {
                hitPos.x = Input.GetAxis("HorizontalR");
                hitPos.z = -Input.GetAxis("VerticalR");
            }

            //we've converted the mouse position to a direction
            Vector3 turnDir = new Vector2(hitPos.x, hitPos.z);

            //rotate turret to look at the mouse direction
            tankPlayer.RotateTurret(hitPos.x, hitPos.z);

            return turnDir;
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

        private Vector3 ExpectBulletRotate()
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
            Vector3 turnDir = Vector2.zero;
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

                //rotate turret to look at the mouse direction
                tankPlayer.RotateTurret(hitPos.x, hitPos.z);
            }

            return turnDir;
        }

        private Vector3 ExpectTankRotate()
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
                if (comp.teamIndex != tankPlayer.teamIndex && comp.IsAlive
                    && (target - origin).magnitude < minDistance)
                {
                    Vector3 delta = origin - target;
                    float theta = Vector3.Angle(delta, comp.Velocity) * Mathf.Deg2Rad;
                    float alpha = Mathf.Asin(tankSpeed / bulletSpeed * Mathf.Sin(theta));
                    float time = Mathf.Sin(alpha) * delta.magnitude
                        / Mathf.Sin(Mathf.PI - alpha - theta) / tankSpeed;
                    Quaternion direction = Quaternion.LookRotation(-delta, Vector3.up)
                        * Quaternion.AngleAxis(alpha * Mathf.Rad2Deg, Vector3.up);

                    target += comp.Velocity * time;
                    float distance = (target - origin).magnitude;

                    if (distance < minDistance
                       && (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit,
                       distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2, ~layerMask)
                       || raycastHit.collider.gameObject == tankPlayer.gameObject))
                    {
                        //Debug.DrawRay(target, (origin - target).normalized * (distance - transform.TransformVector(tankPlayer.GetComponent<BoxCollider>().size).x / 2), Color.black);
                        minDistance = distance;
                        hitPos = target;
                        //Debug.DrawLine(origin, hitPos, Color.red);
                    }

                    else
                    {
                        target -= comp.Velocity * time;
                        if (!Physics.SphereCast(target, bulletRadius, origin - target, out raycastHit, (origin - target).magnitude, ~layerMask)
                        || raycastHit.collider.gameObject == tankPlayer.gameObject)
                        {
                            //Debug.DrawRay(target, origin - target, Color.white);
                            minDistance = (target - origin).magnitude;
                            hitPos = target;
                            //Debug.DrawLine(origin, hitPos, Color.blue);
                        }
                    }
                }
                pl.GetComponent<Collider>().enabled = true;
            }
            if (minDistance != ((tankPlayer.currentBullet == 2) ? 3 : 1) * bulletSpeed)
            {
                Debug.DrawLine(origin, hitPos, Color.red);
                currentTarget = hitPos;
                hitPos -= origin;

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);

                //rotate turret to look at the mouse direction
                tankPlayer.RotateTurret(hitPos.x, hitPos.z);
            }

            return turnDir;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(lastTarget, 0.5f);
        }

        private void DrawTrailsInit()
        {
            GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
            foreach (var pl in allplayer)
            {
                var comp = pl.GetComponent<BasePlayer>();
                lastPosition[comp.teamIndex] = comp.Position;
            }
            InvokeRepeating("DrawTrails", Time.fixedDeltaTime, Time.fixedDeltaTime);
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
                    Debug.DrawLine(lastPosition[index], comp.Position, color, 1.0f);
                }
                lastPosition[index] = comp.Position;
            }
        }

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

