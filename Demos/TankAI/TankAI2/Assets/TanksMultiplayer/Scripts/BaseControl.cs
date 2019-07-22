using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TanksMP
{
    public class BaseControl : MonoBehaviour
    {
        // init controller
        public void Init(BasePlayer player) {
            tankPlayer = player;
            if (!inited)
            {
                OnInit();
            }
            inited = true;
        }

        // begin control
        public void Run() {
            if (!running)
            {
                OnRun();
                running = true;
            }
        }

        // end control
        public void Stop() {
            if(running)
            {
                OnStop();
                running = false;
            }
        }

        public void ControlFixUpdate() {
            if(running && inited)
            {
                OnFixedUpdate();
            }
        }

        public void ControlUpdate() {
            if(running && inited)
            {
                OnUpdate();
            }
        }


        virtual protected void OnInit() { }

        virtual protected void OnRun() { }

        virtual protected void OnStop() { }

        virtual protected void OnFixedUpdate() { }

        virtual protected void OnUpdate() { }



        protected BasePlayer tankPlayer;
        private bool running = false;
        private bool inited = false;
    }

}
