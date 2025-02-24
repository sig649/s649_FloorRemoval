using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
using System.IO;
//using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;//v0.3.3.0 grep
using s649FR.Main;
//using Main = s649FR.Main;

namespace s649FR
{
    namespace ZonePatch {
        [HarmonyPatch(typeof(Zone))]//v0.3.4.0
        [HarmonyPatch(nameof(Zone.Activate))]
        internal static class ZoneMain {
            private static void Postfix(Zone __instance) {   
                if(PatchMain.configDebugLogging){
                    //Debug.Log("[FR]CALLED : Zone.Activate " + __instance.ToString());
                    Debug.Log("[FR]Zone : [DLV : " + __instance.DangerLv.ToString() + "]");
                }
                PatchMain.currentDLV = __instance.DangerLv;  //v0.3.4.0
            }
        }
    }
}



///////////////////////////////////////////////////////////
///trashbox
///

// コンフィグファイルが変更されていたらリロード
            
        //if (File.GetLastWriteTime(Config.ConfigFilePath) > Main.LastConfigLoadTime) {
        //    
        //    Main.LastConfigLoadTime = DateTime.Now;
        //    Debug.Log("[VT] Configuration reloaded due to Zone.Activate.");
        //}
                
                //    Main.LoadConfig();
                //    Debug.Log("[FR] Configuration reloaded due to Zone.Activate.");