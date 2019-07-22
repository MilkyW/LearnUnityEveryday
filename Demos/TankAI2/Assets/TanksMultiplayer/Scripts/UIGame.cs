/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace TanksMP
{
    /// <summary>
    /// UI script for all elements, team events and user interactions in the game scene.
    /// </summary>
    /// 
    public class RankingListData
    {
        public GameObject m_obj;
        public List<int> m_list;

        public RankingListData(GameObject _obj, List<int> _list)
        {
            m_obj = _obj;
            m_list = _list;
        }
    }


    public class UIGame : MonoBehaviour
    {
        /// <summary>
        /// Joystick components controlling player movement and actions on mobile devices.
        /// </summary>
        public UIJoystick[] controls;

        /// <summary>
        /// UI sliders displaying team fill for each team using absolute values.
        /// </summary>
        public Slider[] teamSize;

        public Text[] playerName;

        public Text textGameTime;

        /// <summary>
        /// UI texts displaying kill scores for each team.
        /// </summary>
        public Text[] teamScore;

        /// <summary>
        /// UI texts displaying kill scores for this local player.
        /// [0] = Kill Count, [1] = Death Count
        /// </summary>
        public Text[] killCounter;



        /// <summary>
        /// Mobile crosshair aiming indicator for local player.
        /// </summary>
        public GameObject aimIndicator;

        /// <summary>
        /// UI text for indicating player death and who killed this player.
        /// </summary>
        public Text deathText;

        /// <summary>
        /// UI text displaying the time in seconds left until player respawn.
        /// </summary>
        public Text spawnDelayText;

        /// <summary>
        /// UI text for indicating game end and which team has won the round.
        /// </summary>
        public Text gameOverText;

        /// <summary>
        /// UI window gameobject activated on game end, offering sharing and restart buttons.
        /// </summary>
        public GameObject gameOverMenu;

        public GameObject gameStart;

        public GameObject gameRankingList;
        public Text[] RLplayerName;
        public Text[] RLteamScore;
        public Text[] RLkillCount;
        public Text[] RLdeathCount;
        public Text[] RLitemCount;

        public Text IPText;

        //initialize variables
        IEnumerator Start()
        {
            //wait until the network is ready
            while (GameManager.GetInstance() == null || (GameManager.GetInstance().localPlayer == null && !GameManager.isMaster()))
                yield return null;

            //on non-mobile devices hide joystick controls, except in editor
#if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_WEBGL)
                ToggleControls(false);
#endif

            //on mobile devices enable additional aiming indicator
#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
            if (aimIndicator != null)
            {
                Transform indicator = Instantiate(aimIndicator).transform;
                indicator.SetParent(GameManager.GetInstance().localPlayer.shotPos);
                indicator.localPosition = new Vector3(0f, 0f, 3f);
            }
#endif

            gameStart.SetActive(GameManager.isMaster());
            //IPText.transform.parent.gameObject.SetActive(GameManager.isMaster());
            //IPText.text = NetworkManagerCustom.singleton.networkAddress;

            //play background music
            AudioManager.PlayMusic(1);
        }


        public void setGameTime(float _gameTime)
        {
            textGameTime.text = string.Format("{0:D2}:{1:D2}", (uint)_gameTime / 60, (uint)_gameTime % 60);
        }

        public void GameStart()
        {
            GameManager.GetInstance().isStart = true;
            GameManager.GetInstance().gameTime = 300;
            gameStart.SetActive(false);
        }

        /// <summary>
        /// Method called by the SyncList operation over the Network when its content changes.
        /// This is an implementation for changes to the team fill, updating the slider values.
        /// Parameters: type of operation, index of team which received updates.
        /// </summary>
        public void OnTeamSizeChanged(UnityEngine.Networking.SyncListInt.Operation op, int index)
        {
            teamSize[index].value = GameManager.GetInstance().size[index];
            playerName[index].text = PlayerPrefs.GetString(PrefsKeys.playerName);
            playerName[index].text = GameManager.GetInstance().names[index];
        }

        public void OnTeamNameChanged(UnityEngine.Networking.SyncListString.Operation op, int index)
        {
            playerName[index].text = GameManager.GetInstance().names[index];
        }


        /// <summary>
        /// Method called by the SyncList operation over the Network when its content changes.
        /// This is an implementation for changes to the team score, updating the text values.
        /// Parameters: type of operation, index of team which received updates.
        /// </summary>
        public void OnTeamScoreChanged(UnityEngine.Networking.SyncListInt.Operation op, int index)
        {
            teamScore[index].text = GameManager.GetInstance().score[index].ToString();
            teamScore[index].GetComponent<Animator>().Play("Animation");
        }


        /// <summary>
        /// Enables or disables visibility of joystick controls.
        /// </summary>
        public void ToggleControls(bool state)
        {
            for (int i = 0; i < controls.Length; i++)
                controls[i].gameObject.SetActive(state);
        }


        /// <summary>
        /// Sets death text showing who killed the player in its team color.
        /// Parameters: killer's name, killer's team
        /// </summary>
        public void SetDeathText(string playerName, Team team)
        {
            //hide joystick controls while displaying death text
#if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
            ToggleControls(false);
#endif

            //show killer name and colorize the name converting its team color to an HTML RGB hex value for UI markup
            deathText.text = "KILLED BY\n<color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + playerName + "</color>";
        }


        /// <summary>
        /// Set respawn delay value displayed to the absolute time value received.
        /// The remaining time value is calculated in a coroutine by GameManager.
        /// </summary>
        public void SetSpawnDelay(float time)
        {
            spawnDelayText.text = Mathf.Ceil(time) + "";
        }


        /// <summary>
        /// Hides any UI components related to player death after respawn.
        /// </summary>
        public void DisableDeath()
        {
            //show joystick controls after disabling death text
#if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
            ToggleControls(true);
#endif

            //clear text component values
            deathText.text = string.Empty;
            spawnDelayText.text = string.Empty;
        }


        /// <summary>
        /// Set game end text and display winning team in its team color.
        /// </summary>
        public void SetGameOverText(Team team)
        {
            //hide joystick controls while displaying game end text
#if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
            ToggleControls(false);
#endif

            //show winning team and colorize it by converting the team color to an HTML RGB hex value for UI markup
            gameOverText.text = "TEAM <color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + team.name + "</color> WINS!";
        }


        /// <summary>
        /// Displays the game's end screen. Called by GameManager after few seconds delay.
        /// Tries to display a video ad, if not shown already.
        /// </summary>
        public void ShowGameOver()
        {
            //hide text but enable game over window
            gameOverText.gameObject.SetActive(false);
            gameOverMenu.SetActive(true);

            //check whether an ad was shown during the game
            //if no ad was shown during the whole round, we request one here
#if UNITY_ADS
            if(!UnityAdsManager.didShowAd())
                UnityAdsManager.ShowAd(true);
#endif
        }

        public void ShowRankingList()
        {
            int forCount = 4;
            List<RankingListData> list = new List<RankingListData>();
            for (int i = 0; i < forCount; ++i)
            {
                if (RLplayerName[i] != null)
                    RLplayerName[i].text = playerName[i].text;
                List<int> tempList = new List<int>();
                if (RLteamScore[i] != null)
                {
                    RLteamScore[i].text = teamScore[i].text;
                    tempList.Add(int.Parse(RLteamScore[i].text));                   
                }
                if (RLkillCount[i] != null)
                {
                    int killCount = GameManager.GetInstance().killCount[i];
                    RLkillCount[i].text = killCount.ToString();
                    tempList.Add(killCount);             
                }
                if (RLdeathCount[i] != null)
                {
                    int deathCount = GameManager.GetInstance().deathCount[i];
                    RLdeathCount[i].text = deathCount.ToString();
                    tempList.Add(deathCount);
                }
                if (RLitemCount[i] != null)
                {
                    int itemCount = GameManager.GetInstance().ItemCount[i];
                    RLitemCount[i].text = itemCount.ToString();
                    tempList.Add(itemCount);
                }
                RankingListData data = new RankingListData(RLteamScore[i].transform.parent.gameObject, tempList);
                list.Add(data);
            }
                      
            list.Sort(sortScore);
            for (int i = 0; i < list.Count; ++i)
            {
                list[i].m_obj.transform.SetAsFirstSibling();
            }

            gameRankingList.SetActive(true);
        }

        public int sortScore(RankingListData a, RankingListData b)
        {
            int res = 0;
            for (int i = 0; i < 4; )
            {
                int scoreA = a.m_list[i];
                int scoreB = b.m_list[i];               
                if (i != 2) //deathCount
                {
                    res = sortScore(scoreA, scoreB, true);
                }
                else
                {
                    res = sortScore(scoreA, scoreB, false);
                }
                if (res != 0)
                    return res;
                else
                {
                    ++i;
                }
            }
            return res;
        }

        public int sortScore(int a, int b, bool BtoS)
        {
            if (!BtoS)
            {
                return b.CompareTo(a);
            }
            else
            {
                return a.CompareTo(b);
            }
        }

        
        /// <summary>
        /// Returns to the starting scene and immediately requests another game session.
        /// In the starting scene we have the loading screen and disconnect handling set up already,
        /// so this saves us additional work of doing the same logic twice in the game scene. The
        /// restart request is implemented in another gameobject that lives throughout scene changes.
        /// </summary>
        public void Restart()
        {
            GameObject gObj = new GameObject("RestartNow");
            gObj.AddComponent<UIRestartButton>();
            DontDestroyOnLoad(gObj);
            
            Quit();
        }


        /// <summary>
        /// Stops receiving further network updates by hard disconnecting, then load starting scene.
        /// </summary>
        public void Disconnect()
        {
            UnityEngine.Networking.NetworkManager.singleton.StopHost();
            Quit();
        }


        /// <summary>
        /// Loads the starting scene. Disconnecting already happened when presenting the GameOver screen.
        /// </summary>
        public void Quit()
        {	
			SceneManager.LoadScene(0);
        }
    }
}
