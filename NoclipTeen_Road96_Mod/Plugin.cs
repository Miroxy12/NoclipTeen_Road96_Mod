using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoclipTeen_Road96_Mod
{
    [BepInEx.BepInPlugin(mod_guid, "NoclipTeen", version)]
    [BepInEx.BepInProcess("Road 96.exe")]
    public class NoclipTeenMod : BasePlugin
    {
        private const string mod_guid = "miroxy12.noclipteen";
        private const string version = "1.0";
        private readonly Harmony harmony = new Harmony(mod_guid);
        internal static new ManualLogSource Log;
        public static string scenename = "";
        public static GameObject playerobj = null;
        public static bool isnoclipping = false;
        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo(mod_guid + " started, version: " + version);
            harmony.PatchAll(typeof(LoadSceneAsyncHook));
            AddComponent<ModMain>();
        }
    }
    public class ModMain : MonoBehaviour
    {
        void Awake()
        {
            NoclipTeenMod.Log.LogInfo("loading NoclipTeen");
        }
        void OnEnable()
        {
            NoclipTeenMod.Log.LogInfo("enabled NoclipTeen");
        }
        void Update()
        {
            if (NoclipTeenMod.scenename != "") {
                Scene scene = SceneManager.GetSceneByName(NoclipTeenMod.scenename);
                GameObject mainlogic = null;
                if (scene.isLoaded) {
                    GameObject[] gos = scene.GetRootGameObjects();
                    foreach (var go in gos) {
                        if (go.name.Contains("Logic") || go.name.Contains("LOGIC")) {
                            mainlogic = go;
                            for (int i = 0; i < mainlogic.transform.childCount; i++) {
                                if (mainlogic.transform.GetChild(i).gameObject.name.Equals("Player")) {
                                    NoclipTeenMod.playerobj = mainlogic.transform.GetChild(i).gameObject;
                                    NoclipTeenMod.scenename = "";
                                    //UnityEngine.Debug.Log(NoclipTeenMod.playerobj.name);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.N) && NoclipTeenMod.playerobj != null) {
                NoclipTeenMod.isnoclipping = !NoclipTeenMod.isnoclipping;
                if (NoclipTeenMod.isnoclipping) {
                    NoclipTeenMod.playerobj.GetComponent<CharacterController>().enabled = false; // needed to edit the transform directly
                } else {
                    NoclipTeenMod.playerobj.GetComponent<CharacterController>().enabled = true;
                }
            }
            if (NoclipTeenMod.isnoclipping) {
                float moveX = UnityEngine.Input.GetAxis("Horizontal");
                float moveZ = UnityEngine.Input.GetAxis("Vertical");

                Vector3 move = NoclipTeenMod.playerobj.transform.right * moveX + NoclipTeenMod.playerobj.transform.forward * moveZ;
                NoclipTeenMod.playerobj.transform.position += move * 9 * Time.deltaTime; // * 9 is the speed
            }
        }
    }
    [HarmonyPatch(typeof(SceneManager), "LoadSceneAsync", new System.Type[] { typeof(string), typeof(LoadSceneMode) })]
    public class LoadSceneAsyncHook
    {
        static void Postfix(string sceneName, LoadSceneMode mode)
        {
            // These are scenes that are logic but doesn't have a player object in which cause errors
            string[] bannedscene = {
                "000_Game/Scenes/SONYA_4/SONYA_4_Logic",
                "000_Game/Scenes/ALEX_1/ALEX_1_Logic",
                "000_Game/Scenes/GEN_DRIVE_1/GEN_DRIVE_1_LOGIC"
            };
            // These are scenes that are also logic scenes but without the "_Logic" at the end of the name
            string[] logicscene = {
                    "000_Game/Scenes/JAROD_9/JAROD_9",
                    "000_Game/Scenes/ZOE_4/ZOE_4",
                    "000_Game/Scenes/BORDERS/BORDER_ZOE",
                    "000_Game/Scenes/BORDERS/BorderExit_Zoe/BORDEREXIT_ZOE",
                    "000_Game/Scenes/BORDERS/BorderExit_CreditGold/BorderExit_CreditGold",
                    "000_Game/Scenes/SONYA_8/SONYA_8",
                    "000_Game/Scenes/BORDERS/BORDER_FINAL",
                    "000_Game/Scenes/ZOE_1/ZOE_1",
                    "000_Game/Scenes/ALEX_7/ALEX_7",
                    "000_Game/Scenes/FANNY_9/FANNY_9",
                    "000_Game/Scenes/STANMITCH_9/STANMITCH_9"
            };
            string[] tokens = null;
            bool islogicscene = false;

            NoclipTeenMod.playerobj = null;
            foreach (string i in bannedscene) {
                if (sceneName == i) {
                    return;
                }
            }
            if (sceneName.ToString().Contains("Logic") || sceneName.ToString().Contains("LOGIC")) {
                tokens = sceneName.Split('/');
                if (SceneManager.GetSceneByName(sceneName) != null) {
                    NoclipTeenMod.scenename = tokens[3];
                }
            } else {
                foreach (string i in logicscene) {
                    if (sceneName == i) {
                        islogicscene = true;
                        break;
                    }
                }
                if (islogicscene) {
                    tokens = sceneName.Split('/');
                    if (SceneManager.GetSceneByName(sceneName) != null) {
                        NoclipTeenMod.scenename = tokens[3];
                    }
                }
            }
        }
    }
}
