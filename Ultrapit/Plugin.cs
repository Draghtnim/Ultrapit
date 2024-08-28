using BepInEx;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Logic;
using System.Reflection;
using System.IO;
using System.Net.Http;
using TMPro;
using Newtonsoft.Json.Linq;

namespace Ultrapit
{
    [BepInPlugin("Ultrapit.draghtnim.ultrakill", "Ultrapit", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private AssetBundle terminal;
        public static bool IsCustomLevel = false;

        //hello! i am the hijacker!
        private UnityEngine.SceneManagement.Scene scene;
        private GameObject pitobj;
        private GameObject newpitobj;
        AssetBundle bundlepit;
        private static Plugin _instance;
        public static Plugin Instance => _instance;
        string assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static string getConfigPath()
        {
            return Path.Combine(Paths.ConfigPath + Path.DirectorySeparatorChar + "EnvyLevels");
        }

        public static GameObject FindObjectEvenIfDisabled(string rootName, string objPath = null, int childNum = 0, bool useChildNum = false)
        {
            GameObject obj = null;
            GameObject[] objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            bool gotRoot = false;
            foreach (GameObject obj1 in objs)
            {
                if (obj1.name == rootName)
                {
                    obj = obj1;
                    gotRoot = true;
                }
            }
            if (!gotRoot)
                goto returnObject;
            else
            {
                GameObject obj2 = obj;
                if (objPath != null)
                {
                    obj2 = obj.transform.Find(objPath).gameObject;
                    if (!useChildNum)
                    {
                        obj = obj2;
                    }
                }
                if (useChildNum)
                {
                    GameObject obj3 = obj2.transform.GetChild(childNum).gameObject;
                    obj = obj3;
                }
            }
        returnObject:
            return obj;
        }

        private async void Awake()
        {

            _instance = this;

            //bundlepit = AssetBundle.LoadFromMemory(Properties.Resources.Crusher1);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isNotBootstrapOrIntro = SceneHelper.CurrentScene != "Bootstrap" && SceneHelper.CurrentScene != "Intro";
            bool isMainMenu = SceneHelper.CurrentScene == "Main Menu";

            if (isMainMenu)
            {
                //hello again

                pitobj = GameObject.Find("Pit (2)");
                pitobj.SetActive(false);
                pitobj = GameObject.Find("Music");
                pitobj.SetActive(false);
                pitobj = GameObject.Find("Player");
                pitobj.transform.position = Vector3.zero;

                UnityEngine.Debug.Log("okay the path is: "+assemblyLocation);
                var PitBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyLocation+ "/ResourceFiles", "UltrapitBundle.resource"));
                if (PitBundle == null)
                {
                    Debug.Log("Failed to load AssetBundle!");
                    return;
                }
                else
                {
                    UnityEngine.Debug.Log("Loaded something.");

                }
                GameObject[] prefabs = PitBundle.LoadAllAssets<GameObject>();

                int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);

                GameObject randomPrefab = prefabs[randomIndex];
                UnityEngine.Debug.Log("your random gameobject");
                UnityEngine.Debug.Log(randomPrefab.name);



                //var prefab = PitBundle.LoadAsset<GameObject>("Pit (2)");
                Instantiate(randomPrefab);

                PitBundle.Unload(false);
                //back to our schedueled code!
                ShaderManager.CreateShaderDictionary();
                _instance.StartCoroutine(ShaderManager.ApplyShadersAsyncContinuously());
                UnityEngine.Debug.Log("envy???");
                //InstantiateEnvyScreen(isMainMenu);
            }

            if (ShaderManager.shaderDictionary.Count <= 0)
            {
                StartCoroutine(ShaderManager.LoadShadersAsync());
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (SceneHelper.CurrentScene == "Main Menu")
            { 
                //InstantiateEnvyScreen(true);
                ShaderManager.CreateShaderDictionary();
            }
        }

        private void InstantiateEnvyScreen(bool mainMenu)
        {
            GameObject envyScreenPrefab = terminal.LoadAsset<GameObject>("EnvyScreen.prefab");
            // Fun Fact: my dumbass forgot to put envyscreen in the assetbundle and i was stuck debugging it for 2 hours RAHHHHHHHHHHHHH --thebluenebula
            // smart ass --doomah
            // i find it funny how your laptop broke right after saying that --thebluenebula
            if (envyScreenPrefab == null)
            {
                Debug.LogError("EnvyScreen prefab not found in the terminal bundle.");
                return;
            }

            GameObject canvasObject = GameObject.Find("/Canvas/Main Menu (1)");
            if (mainMenu == false)
            {
                canvasObject = FindObjectEvenIfDisabled("Canvas", "PauseMenu");
            }

            if (canvasObject == null)
            {
                return;
            }

            GameObject instantiatedObject = Instantiate(envyScreenPrefab);

            instantiatedObject.transform.SetParent(canvasObject.transform, false);
            instantiatedObject.transform.localPosition = Vector3.zero;
            instantiatedObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
