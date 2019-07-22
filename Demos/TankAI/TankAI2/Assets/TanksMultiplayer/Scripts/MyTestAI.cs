using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TanksMP
{
    public class MyTestAI : BaseControl
    {

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (lockPlayer == null || !lockPlayer.IsAlive)
            {
                lockPlayer = null;
                GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Player");
                foreach (var pl in allplayer)
                {
                    var comp = pl.GetComponent<BasePlayer>();
                    if (comp.teamIndex != tankPlayer.teamIndex && comp.IsAlive)
                    {
                        lockPlayer = comp;
                        break;
                    }
                }
            }

            if (lockPlayer != null)
            {
                tankPlayer.MoveTo(lockPlayer.Position);

                if (tankPlayer.bShootable)
                {
                    tankPlayer.AimAndShoot(lockPlayer.Position);
                }
            }

        }

        BasePlayer lockPlayer;
        //GameObject lockPowerUp;
    }

}
