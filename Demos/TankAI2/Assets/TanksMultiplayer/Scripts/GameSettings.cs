using System;
using UnityEngine;

namespace TanksMP
{
    class GameSettings : MonoBehaviour
    {
        public static GameSettings instance;

        private void Awake()
        {
            instance = this;
        }


        private void OnDestroy()
        {
            if(instance == this)
            {
                instance = null;
            }
        }

        public GameObject PlayerControl;
        public GameObject PlayerAI;
        public GameObject BotAI;

        private GameSettings.GameMode gameMode;

        public enum GameMode
        {
            DebugMode,
            MatchMode,
        }

        public void SetGameMode(GameSettings.GameMode mode) {
            gameMode = mode;
        }

        public GameObject GetPlayerController() {
            return PlayerAI;
        }

        public GameObject GetBotController() {
            return BotAI;
        }
    }
}
