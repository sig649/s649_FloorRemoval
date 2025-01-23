//using System;
///using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
//using System.IO;
//using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace s649VT
{
    [BepInPlugin("s649_VanillaTweaks", "s649 Vanilla Tweaks", "0.0.0.0")]

    public class Main : BaseUnityPlugin
    {
        private static ConfigEntry<bool> flagModInfiniteDigOnField;
        private static ConfigEntry<bool> flagModInfiniteDigOnFieldToNothing;


        public static bool configFlagModInfiniteDigOnField => flagModInfiniteDigOnField.Value;
        public static bool configFlagModInfiniteDigOnFieldToNothing => flagModInfiniteDigOnFieldToNothing.Value;
        

        private void Start()
        {
            flagModInfiniteDigOnField = Config.Bind("#FUNC_01a", "MOD_INFINITE_DIG", true, "Mod digging infinite dirt chunk on field");
            flagModInfiniteDigOnFieldToNothing = Config.Bind("#FUNC_01b", "CHANGE_TO_DIGGING_NOTHING_ON_FIELD", false, "Digging nothing on field");

            //UnityEngine.Debug.Log("[LS]Start [configLog:" + propFlagEnablelLogging.ToString() + "]");
            var harmony = new Harmony("Main");
            new Harmony("Main").PatchAll();
        }
        
    }
    //++++EXE++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [HarmonyPatch]
    public class MapExe{
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Map), "MineFloor")]
        public static bool Prefix(Map __instance, Point point, Chara c, bool recoverBlock, bool removePlatform){
            if(!Main.configFlagModInfiniteDigOnField){return true;} //#FUNC_01a Flag:falseなら何もしない
            //----debug------------------------------------------------------------------------
            string text = "[LS]MF [";
            text += "Map:" + __instance.ToString() + "][";
            text += "P:" + point.ToString() + "][";
            text += "C:" + c.ToString() + "][";
            text += "rB:" + recoverBlock.ToString() + "][";
            text += "rP:" + removePlatform.ToString() + "][";
            text += "]";
            //---debug kokomade--------------------------------------------------------------------
            if(Main.configFlagModInfiniteDigOnFieldToNothing){return false;} //#FUNC_01b　Flag:trueなら掘りつつアイテム入手をスキップ
            if(point.sourceFloor.id == 4 ){
                //Debug.Log("Floor is hatake");
                int num = UnityEngine.Random.Range(0, 99);
                Thing t = null;
                switch(num){
                    case < 25 and >= 10 : t = ThingGen.Create("stone");
                        break;
                    case < 10 and >= 2 : t = ThingGen.Create("pebble");
                        break;
                    case < 2 : t = ThingGen.Create("rock");
                        break;
                }
                if(t != null){
                    //c.Pick(t);
                    __instance.TrySmoothPick(point, t, c);
                }
                return false;
            }
            return true;
            //Debug.Log(text);
        }
        
    }
}
//------------template--------------------------------------------------------------------------------------------
/*
[HarmonyPatch]

[HarmonyPrefix]
[HarmonyPostfix]

[HarmonyPatch(typeof(----),"method")]
public class ------{}

public static void ----(type arg){}
public static bool Pre--(type arg){}

[HarmonyPatch]
public class PreExe{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(------), "+++++++")]
    public static bool Prefix(type arg){}
}

[HarmonyPatch]
public class PostExe{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(------), "+++++++")]
    public static void Postfix(type arg){}
}

*/

//////trash box//////////////////////////////////////////////////////////////////////////////////////////////////
///
/*
    [HarmonyPatch(typeof(Zone))]
    [HarmonyPatch(nameof(Zone.Activate))]
    public class HarmonyAct
    {
        //[HarmonyPostfix]
        
        static void Postfix(Zone __instance)
        {
            Zone z = __instance;
            Zone topZo = z.GetTopZone();
            FactionBranch br = __instance.branch;
            //Lg("[LS]Fooked!");
            if (Main.propFlagEnablelLogging)
            {
                Lg("[LS]CALLED : Zone.Activate ");
                string text;

                text = ("[LS]Ref : [Z:" + z.id.ToString() + "]");
                //text += (" [id:" + z.id.ToString() + "]");
                text += (" [Dlv:" + z.DangerLv.ToString() + "]");
                text += (" [blv:" + Mathf.Abs(z.lv).ToString() + "]");
                text += (" [bDLV:" + z._dangerLv.ToString() + "]");
                text += (" [Dlfi:" + z.DangerLvFix.ToString() + "]");
                if(topZo != null && z != topZo){text += (" [tpZ:" + topZo.NameWithLevel + "]");}
                if(br != null){text += (" [br:" + br.ToString() + "]");}
                if(z.ParentZone != null && z != z.ParentZone)text += (" [PaZ: " + z.ParentZone.id.ToString() + "]") ;
                 text += (" [Pce:" + z.isPeace.ToString() + "]");
                 text += (" [Twn:" + z.IsTown.ToString() + "]");
                Lg(text);
                //text = ("[LS]Charas : " + EClass._map.charas.Count);
                //text += (" [Stsn:" + z.isPeace.ToString() + "]");
            }
            
        }
        public static void Lg(string t)
        {
            UnityEngine.Debug.Log(t);
        }
        
    }

    [HarmonyPatch(typeof(HotItem))]
    public class HotPatch {
        //[HarmonyPrefix]
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HotItem.TrySetAct))]
        static void FookPostExe(HotItem __instance){
            //Debug.Log("[LS]Fooking->" + __instance.ToString());
        }
    }  

    [HarmonyPatch]
    public class TickPatch{
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara),"TickConditions")]
        public static void TickExe(Chara __instance){
            Chara c = __instance;
            if(c.IsPC){
                //Debug.Log("[LS]QuestMain : " + QuestMain.Phase.ToString());
            }
        }
    }
    */
/*
    [HarmonyPatch(typeof(TraitDoor))]
    public class PatchAct2 {

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TraitDoor.TryOpen))]
        static void FookPreExe(TraitDoor __instance, Chara c, ref bool __state){
            __state = __instance.IsOpen() ? true : false;
            //if(c.IsPC){ Lg("[LS]TraitDoor.TryOpen Called! by->" + c.ToString());}
            
            
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TraitDoor.TryOpen))]
        static void FookPostExe(TraitDoor __instance, Chara c, bool __state){
            if(!__state && __instance.IsOpen()){
                if(c.IsPC){ 
                    //Lg("[LS]TraitDoor.Close->Open!" + c.ToString());
                    }
            }
           
        }
        
        
        public static void Lg(string t)
        {
            UnityEngine.Debug.Log(t);
        }
    }
   
public class Main : BaseUnityPlugin {
    private void Start() {
        var harmony = new Harmony("NerunTest");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(Zone))]
[HarmonyPatch(nameof(Zone.Activate))]
class ZonePatch {
    static void Prefix() {
        Debug.Log("Harmoney Prefix");
    }
    static void Postfix(Zone __instance) {
        Debug.Log("Harmoney Postfix");
        }
}
*/

/*
        
        */
        /*
        [HarmonyPatch(typeof(Zone))]
        [HarmonyPatch(nameof(Zone.Activate))]
        class ZonePatch {
        static void Postfix(Zone __instance) {
            Lg("[LS]CALLED : Zone.Activate " + __instance.ToString());
            Lg("[LS]Zone : [DLV : " + __instance.DangerLv.ToString() + "]");
            Lg("[LS]Player : [dst : " + EClass.player.stats.deepest.ToString() + "]");
            }
        }
        */

        /*
        static void PostMoveZone(Player __instance)
        {
            Lg("[LS]Fooked!MoveZone!");
            if (Main.propFlagEnablelLogging.Value)
            {
                int dst = EClass.player.stats.deepest;
                Lg("[LS]CALLED : Player.MoveZone ");
                Lg("[LS]Player : [dst : " + dst.ToString() + "]");
            }
        }

        */


        /*
        [HarmonyPatch(typeof(Card), "AddExp")]
        
        class CardPatch
        {
            [HarmonyPrefix]
            static bool AddExpHook(Card __instance)
            {
                Lg("[LS]Fooked:AddExp!");
                
                if (Main.propFlagEnablelLogging.Value)
                {
                    if(__instance.IsPC){
                        Lg("[LS]Card : [name : " + __instance.ToString() + "]");
                    }
                    //Lg("[LS]Card : [name : " + dst.ToString() + "]");
                    //Lg("[LS]Player : [dst : " + dst.ToString() + "]");
                }
                return true;
            }
        }
        */

        /*
        [HarmonyPatch(typeof(Zone), "DangerLv", MethodType.Getter)]
        class ZonePatch {
            [HarmonyPrefix]
            static bool Prefix(Zone __instance) {
                Lg("[LS]CALLED : Zone.DangerLV ");
                //Lg("[LS]Zone : [Z.toSt : " + __instance.ToString() + "]");
                //Lg("[LS]Zone : [DLV : " + __instance.DangerLv.ToString() + "]");
                //Lg("[LS]Player : [dst : " + EClass.player.stats.deepest.ToString() + "]");
                return true;
            }
        }


        //public static void Lg(string t)
        //{
        //    UnityEngine.Debug.Log(t);
        //}
        //public static bool IsOnGlobalMap(){
        //    return (EClass.pc.currentZone.id == "ntyris") ? true : false;
        //}

        */

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Map), "DropBlockComponent")]
        public static void Postfix(Point point,TileRow r,SourceMaterial.Row mat, bool recoverBlock, bool isPlatform, Chara c){
            string text = "[LS]DBC [";
            //text += "Map:" + __instance.ToString() + "][";
            text += "P:" + point.ToString() + "][";
            text += "r:" + r.ToString() + "][";
            text += "rid:" + r.id.ToString() + "][";
            text += "mat:" + mat.ToString() + "][";
            text += "rB:" + recoverBlock.ToString() + "][";
            text += "iP:" + isPlatform.ToString() + "][";
            //text += "c:" + c.ToString() + "][";
            text += "]";
            Debug.Log(text);
        }
        */
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThingGen), "CreateRawMaterial")]
        public static void Postfix(SourceMaterial.Row row){
            Debug.Log("[LS]TG->CRM : " + row.ToString());
        }*/