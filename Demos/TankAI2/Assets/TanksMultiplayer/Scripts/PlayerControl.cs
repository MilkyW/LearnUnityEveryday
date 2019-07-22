using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TanksMP
{
    public class PlayerControl : BaseControl
    {
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
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

#pragma warning disable 0219
            //movement variables
            Vector2 moveDir = Vector2.zero;
            Vector2 turnDir = Vector2.zero;
#pragma warning restore 0219

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
            turnDir = new Vector2(hitPos.x, hitPos.z);

            //rotate turret to look at the mouse direction
            tankPlayer.RotateTurret(hitPos.x, hitPos.z);

            //shoot bullet on left mouse click
            if (Input.GetButton("Fire1"))
                tankPlayer.Shoot();

            //replicate input to mobile controls for illustration purposes
#if UNITY_EDITOR
            GameManager.GetInstance().ui.controls[0].position = moveDir;
            GameManager.GetInstance().ui.controls[1].position = turnDir;
#endif
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
