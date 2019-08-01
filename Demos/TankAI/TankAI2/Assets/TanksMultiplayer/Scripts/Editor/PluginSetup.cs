using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace TanksMP
{
    public class PluginSetup : EditorWindow
    {
        private static string packagesPath;
        private static Packages selectedPackage = Packages.UnityNetworking;
        private enum Packages
        {
            UnityNetworking = 0,
            PhotonPUN = 1
        }


        [MenuItem("Window/Tanks Multiplayer/Network Setup")]
        static void Init()
        {
            packagesPath = "/Packages/";
            EditorWindow window = EditorWindow.GetWindowWithRect(typeof(PluginSetup), new Rect(0, 0, 360, 260), false, "Network Setup");

            var script = MonoScript.FromScriptableObject(window);
            string thisPath = AssetDatabase.GetAssetPath(script);
            packagesPath = thisPath.Replace("/PluginSetup.cs", packagesPath);
        }


        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tanks Multiplayer - Network Setup", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Please choose the network provider you would like to use:");

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            selectedPackage = (Packages)EditorPrefs.GetInt("TanksMP_Provider", 0);
            selectedPackage = (Packages)EditorGUILayout.EnumPopup(selectedPackage);

            if (EditorPrefs.GetInt("TanksMP_Provider", 0) != (int)selectedPackage)
            {
                EditorPrefs.SetInt("TanksMP_Provider", (int)selectedPackage);
            }

            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                switch (selectedPackage)
                {
                    case Packages.UnityNetworking:
                        Application.OpenURL("https://unity3d.com/services/multiplayer");
                        break;
                    case Packages.PhotonPUN:
                        Application.OpenURL("https://www.photonengine.com/en/Realtime");
                        break;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Step 1: Import Network Package"))
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AssetDatabase.ImportPackage(packagesPath + selectedPackage.ToString() + ".unitypackage", false);

                //force recompile to let Photon set up platform defines etc.
                string defineGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (defineGroup.Contains("TANKSMP")) defineGroup = defineGroup.Replace("TANKSMP", ""); else defineGroup += ";TANKSMP";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineGroup);

                Debug.Log("Tanks Multiplayer - Network Setup: Wait for the compiler to finish on Step 1, then press Step 2!");
            }

            if (GUILayout.Button("Step 2: Setup Package Contents"))
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    Debug.LogError("Tanks Multiplayer - Network Setup: Please wait for the compiler to finish before executing Step 2.");
                    return;
                }

                Setup();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Note:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("For a detailed comparison about features and pricing, please");
            EditorGUILayout.LabelField("refer to the official pages for UNET or Photon. The features");
            EditorGUILayout.LabelField("of this asset are the same across both multiplayer services.");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please read the PDF documentation for further details.");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Support links: Window > Tanks Multiplayer > About.");
        }


        void Setup()
        {
            string[] scenes = System.IO.Directory.GetFiles(".", "*.unity", System.IO.SearchOption.AllDirectories);

            if (selectedPackage == Packages.UnityNetworking)
            {
                NetworkManagerCustom netManager = (NetworkManagerCustom)AssetDatabase.LoadAssetAtPath("Assets/TanksMultiplayer/Prefabs/Network.prefab", typeof(NetworkManagerCustom));
                System.Reflection.PropertyInfo playerPrefab = netManager.GetType().GetProperty("playerPrefab");
                playerPrefab.SetValue(netManager, Resources.Load("TankFree"), null);
                AssetDatabase.ImportAsset("Assets/TanksMultiplayer/Scripts/GameManager.cs");
            }

            if (selectedPackage == Packages.PhotonPUN)
            {
                #if !PUN_2_OR_NEWER
                    Debug.LogError("Tanks Multiplayer - Network Setup: Could not find PhotonViewHandler. Did you import Photon yet?");
                #else

                EditorWindow photonWin = Photon.Pun.PhotonViewHandler.GetWindowWithRect(typeof(Photon.Pun.PhotonViewHandler), new Rect(0,0,0,0));
                System.Reflection.MethodInfo hierarchyMethod = photonWin.GetType().GetMethod("HierarchyChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);

                //automatic assignment of PhotonView IDs in non-loaded scenes
                foreach (string scene in scenes)
                {
                    if (scene.EndsWith("Game.unity"))
                    {
                        EditorSceneManager.OpenScene(scene);

                        //we have to disconnect all prefab connections first, because otherwise they can't be manipulated
                        //in the PhotonViewHandler.HierarchyChange method - seems like a Unity bug
                        ObjectSpawner[] objects = FindObjectsOfType(typeof(ObjectSpawner)) as ObjectSpawner[];
                        if (objects == null || objects.Length == 0) continue;

                        for (int i = 0; i < objects.Length; i++)
                            if(PrefabUtility.IsPartOfPrefabInstance(objects[i]))
                                PrefabUtility.UnpackPrefabInstance(objects[i].gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                        hierarchyMethod.Invoke(photonWin, null);
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), EditorSceneManager.GetActiveScene().path);
                    }
                }

                photonWin.Close();
                #endif
            }

            foreach (string scene in scenes)
            {
                if (scene.EndsWith("Intro.unity"))
                {
                    EditorSceneManager.OpenScene(scene);
                    break;
                }
            }

            Debug.Log("Tanks Multiplayer - Setup Done!");
        }
    }
}