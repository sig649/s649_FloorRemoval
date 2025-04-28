using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using s649FR;

namespace s649FR
{
    namespace Main 
    {
        [BepInPlugin("s649_FloorRemoval", "s649 Floor Removal", "0.5.0.1")]  
        public class PatchMain : BaseUnityPlugin
        {
            //ConfigEntry + prop------------------------------------------------------------------------------
            //general
            private static ConfigEntry<bool> CE_F01_00_a_ModDigOnField;//#F_01_00_a
            //private static ConfigEntry<bool> flagModInfiniteDigOnFieldToNothing;//#F01_00_b
            private static ConfigEntry<bool> CE_F01_01_Replace2DirtFloor;//#F01_01
            private static ConfigEntry<bool> CE_F02_00_DrawingWaterByEmptyBottle;//#F02_00  
            private static ConfigEntry<bool> CE_F02_01_ReplaceWaterFloor;//#F02_01
            //private static ConfigEntry<bool> CE_F02_01_a_ToggleReplaceWaterFloorFunction;//#F02_01_a //v0.3.1.0
            private static ConfigEntry<KeyCode> CE_KeyCode;//v0.3.0.0
            private static ConfigEntry<bool> CE_DebugLogging;//#F02_01
            //................................................................................
            public static bool config_F01_00_a_ModDigOnField => CE_F01_00_a_ModDigOnField.Value;//#F01_00_a
            //public static bool configFlagModInfiniteDigOnFieldToNothing => flagModInfiniteDigOnFieldToNothing.Value;//#F01_00_b
            public static bool config_F01_01_Replace2DirtFloor => CE_F01_01_Replace2DirtFloor.Value;//#F01_01
            public static bool config_F02_00_DrawingWaterByEmptyBottle => CE_F02_00_DrawingWaterByEmptyBottle.Value;//#F02_00
            public static bool config_F02_01_ReplaceWaterFloor => CE_F02_01_ReplaceWaterFloor.Value;//#F2_01
            //public static bool config_F02_01_a_ToggleReplaceWaterFloorFunction => CE_F02_01_a_ToggleReplaceWaterFloorFunction.Value;//#F2_01_a //v0.3.1.0

            public static bool configDebugLogging => CE_DebugLogging.Value;
            


            
            //--gatya----------------------------------------------------------------------------------------
            private static ConfigEntry<string> CE_GatyaListSSR;
            private static ConfigEntry<string> CE_GatyaListSR;
            private static ConfigEntry<string> CE_GatyaListR;
            private static ConfigEntry<string> CE_GatyaListMaterial;
            private static ConfigEntry<string> CE_GatyaListJunk;
            
            private static ConfigEntry<int> CE_Value_T1Border;
            private static ConfigEntry<int> CE_Value_T2Border;
            private static ConfigEntry<int> CE_Value_T3Border;
            private static ConfigEntry<int> CE_Value_T4Border;
            private static ConfigEntry<int> CE_Value_T5Border;
            
            
            //....................................................................................
            
            public static List<string> list_Gatya_SSR => CE_GatyaListSSR.Value.Split(',').Select(s => s.Trim()).ToList();
            public static List<string> list_Gatya_SR => CE_GatyaListSR.Value.Split(',').Select(s => s.Trim()).ToList();
            public static List<string> list_Gatya_R => CE_GatyaListR.Value.Split(',').Select(s => s.Trim()).ToList();
            public static List<string> list_Gatya_M => CE_GatyaListMaterial.Value.Split(',').Select(s => s.Trim()).ToList();
            public static List<string> list_Gatya_J => CE_GatyaListJunk.Value.Split(',').Select(s => s.Trim()).ToList();
            
            public static int cf_Value_T1Border {
		        get {return Mathf.Clamp(CE_Value_T1Border.Value,0,100);}
	        }
            public static int cf_Value_T2Border {
		        get {return Mathf.Clamp(CE_Value_T2Border.Value,100,1000);}
	        }
            public static int cf_Value_T3Border {
		        get {return Mathf.Clamp(CE_Value_T3Border.Value,1000,10000);}
	        }
            public static int cf_Value_T4Border {
		        get {return Mathf.Clamp(CE_Value_T4Border.Value,10000,100000);}
	        }
            public static int cf_Value_T5Border {
		        get {return Mathf.Clamp(CE_Value_T5Border.Value,100000,1000000);}
	        }
            //class  prop----------------------------------------------------------
            internal static int currentDLV = 0;//v0.3.4.0 
            internal static bool IsFunctionKeyDown = false;//v0.3.0.0

            
            //loading---------------------------------------------------------------------------------------------------
            private void LoadConfig()
            {
                CE_F02_00_DrawingWaterByEmptyBottle = Config.Bind("#FUNC_02_00", "DRAWING_WATER_BY_EMPTY_BOTTLE", true, "If true, you can drawing water from floor of water by empty bottle");

                CE_F02_01_ReplaceWaterFloor = Config.Bind("#FUNC_02_01", "REPLACE_WATER_FLOOR", true, "If Press left shift ley, drawing water changes water floor");
                //CE_F02_01_a_ToggleReplaceWaterFloorFunction = Config.Bind("#FUNC_02_01_a", "Toggle_REPLACE_WATER_FLOOR", false, "If true, toggle replacing function by function key");//v0.3.1.0

                CE_F01_00_a_ModDigOnField = Config.Bind("#FUNC_01_00_a", "MOD_DIG_ON_FIELD", true, "Change the deliverables when you perform a dig in the field");
                //flagModInfiniteDigOnFieldToNothing = Config.Bind("#FUNC_01_00_b", "CHANGE_TO_DIGGING_NOTHING_ON_FIELD", false, "Nothing will be dug out of the field(Not Recommend)");
                CE_F01_01_Replace2DirtFloor = Config.Bind("#FUNC_01_01", "REPLACE_FLOOR_AFTER_DIGGING_CHUNK", true, "Replace some floors with dirt floors after digging");

                CE_GatyaListSSR = Config.Bind("#List-Gatya","LIST_SSR_GATYA","STATUE,MEDAL,MEDAL,MEDAL,MEDAL","List of items produced by SSR Tier");
                CE_GatyaListSR = Config.Bind("#List-Gatya","LIST_SR_GATYA","TREASUREMAP,TICKET,TICKET,RARECOIN,RARECOIN","List of items produced by SR Tier");
                CE_GatyaListR = Config.Bind("#List-Gatya","LIST_R_GATYA","SCRATCH,PLATINA,COIN,CASINO_C,GOLD","List of items produced by R Tier");
                CE_GatyaListMaterial = Config.Bind("#List-Gatya","LIST_Material_GATYA","ORE,SEED,NEEDLE,SKIN,BRANCH,BONE,SCRAP,WOOD,FRAGMENT,JUNK","List of items produced by Material Tier");
                CE_GatyaListJunk = Config.Bind("#List-Gatya","LIST_Junk_GATYA","PAPER,GRAVE,CAN,BOTTLE,WOOD,RUBBER,SCRAP,GARBAGE","List of Junk items produced by Material Tier in Junk List");
                
                CE_Value_T1Border = Config.Bind("Value-Gatya","Value_T1_Border",100,"Value of T1 (SSR) border");
                CE_Value_T2Border = Config.Bind("Value-Gatya","Value_T2_Border",1000,"Value of T1 (SR) border");
                CE_Value_T3Border = Config.Bind("Value-Gatya","Value_T3_Border",10000,"Value of T1 (R) border");
                CE_Value_T4Border = Config.Bind("Value-Gatya","Value_T4_Border",100000,"Value of T1 (Material) border");
                CE_Value_T5Border = Config.Bind("Value-Gatya","Value_T5_Border",250000,"Value of T1 (Stone) border");


                CE_KeyCode = Config.Bind<KeyCode>("#KeyBind", "Function_Key", KeyCode.LeftShift, "Function_key");//v0.3.0.0
                CE_DebugLogging = Config.Bind("#z_Debug", "DEBUG_LOGGING", false, "For Debug");
            }
            //////////////////////////////////////////////////////////////////////
        
            
            internal static bool IsOnGlobalMap(){
                return (EClass.pc.currentZone.id == "ntyris") ? true : false;
            }
            private string TorF(bool b){//v0.3.4.0 ->private
                return (b)? "T": "F";
            }
            private void Start()
            {
                LoadConfig();
                if(configDebugLogging){
                    string text = "[s649-FR]Config";
                    text += ("[01_00_a/" + TorF(config_F01_00_a_ModDigOnField) + "]");
                    text += ("[01_01/" + TorF(config_F01_01_Replace2DirtFloor) + "]");
                    text += ("[02_00/" + TorF(config_F02_00_DrawingWaterByEmptyBottle) + "]");
                    text += ("[02_01/" + TorF(config_F02_01_ReplaceWaterFloor) + "]");
                    Debug.Log(text);
                    text = "[FR]Key :";
                    text += CE_KeyCode.Value.ToString();
                    Debug.Log(text);
                }
                //LastConfigLoadTime = DateTime.Now;
                //SetupConfigWatcher();
                //UnityEngine.Debug.Log("[LS]Start [configLog:" + propFlagEnablelLogging.ToString() + "]");
                var harmony = new Harmony("PatchMain");
                new Harmony("PatchMain").PatchAll();
            }
            private void Update(){
                bool keyDown = Input.GetKeyDown(CE_KeyCode.Value);
                if(!IsFunctionKeyDown && keyDown){
                    IsFunctionKeyDown = true;
                    return;
                }
                bool keyUp = Input.GetKeyUp(CE_KeyCode.Value);
                if(IsFunctionKeyDown && keyUp){
                    IsFunctionKeyDown = false;
                    return;
                }
            }
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
    [HarmonyPatch]
    public class TaskPlowModding{   //v0.1.1.0
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaskPlow), "GetHitResult")]
        public static bool Prefix(HitResult __result, TaskPlow __instance){
            Point pos = __instance.pos;
            if(pos.cell.IsTopWater || pos.HasObj){return true;}
            if(pos.IsFarmField){
                __result = HitResult.Valid;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaskPlow), "CanPerform")]
        public static bool Prefix(bool __result, TaskPlow __instance){
            Point pos = __instance.pos;
            if(pos.IsFarmField){
                __result = true;
                return false;
            }
            return true;
        }
    }
    */
/*
    [HarmonyPatch(typeof(Zone))]
    [HarmonyPatch(nameof(Zone.Activate))]
    class ZonePatch {
        static void Postfix(Zone __instance) {
            // コンフィグファイルが変更されていたらリロード
            
            //if (File.GetLastWriteTime(Config.ConfigFilePath) > Main.LastConfigLoadTime) {
            //    
            //    Main.LastConfigLoadTime = DateTime.Now;
            //    Debug.Log("[VT] Configuration reloaded due to Zone.Activate.");
            //}
            
            Main.LoadConfig();
            Debug.Log("[VT] Configuration reloaded due to Zone.Activate.");
        }
    }
    */
    /*
    [HarmonyPatch]
    public class PostExe{
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "OnLoad")]
        public static void Postfix(){
            Main.LoadConfig();
        }
    }
    */
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

        //+++++++ TaskDig +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /*
    [HarmonyPatch(typeof(TaskDig))]
    public class PreExe{
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaskDig), "GetHitResult")]
        public static bool Prefix(TaskDig __instance, HitResult __result){
            if(!Main.config_F01_01_Replace2DirtFloor){return true;} //#FUNC_01Another Flag:falseなら何もしない
            
            //ここからバニラの除外処理
            if(EClass._zone.IsRegion && __instance.GetTreasureMap() != null) {
                return true;
            }
            if(__instance.mode == TaskDig.Mode.RemoveFloor){
                return true;
            }
            Point pos = __instance.pos;
            if (EClass._zone.IsSkyLevel && (pos.Installed != null || pos.Charas.Count >= 2 || (pos.HasChara && pos.FirstChara != EClass.pc))){
                return true;
            }
            if (!pos.IsInBounds || pos.IsWater || pos.HasObj || (!EClass._zone.IsPCFaction && pos.HasBlock)){
                return true;
            }
            //ここまでバニラの除外処理
            if (!pos.HasBridge && pos.sourceFloor.id == 40){
                __result = HitResult.Valid;
                return false;
            }
            return true;
        }
    }
    */


    //configが更新された後にリロードができるように
        /*
        public static DateTime LastConfigLoadTime { get; private set; }
        
        private FileSystemWatcher configWatcher;
        private void SetupConfigWatcher()
        {
            configWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(Config.ConfigFilePath),
                Filter = Path.GetFileName(Config.ConfigFilePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            configWatcher.Changed += OnConfigFileChanged;
            configWatcher.EnableRaisingEvents = true;
        }
        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            LoadConfig();
            Debug.Log("[VT]Configuration reloaded.");
        }
        */

        /*
            public static bool FlagModDiggingOnField(Point point){
            if(config_F01_00_a_ModDigOnField){
                return true;
            } else {
                return false;
            }
            }*/