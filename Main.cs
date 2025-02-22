//using System;
///using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
using System.IO;
//using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace s649FR
{
    [BepInPlugin("s649_FloorRemoval", "s649 Floor Removal", "0.3.2.0")]  
    public class Main : BaseUnityPlugin
    {
        private static ConfigEntry<bool> CE_F01_00_a_ModDigOnField;//#F_01_00_a
        //private static ConfigEntry<bool> flagModInfiniteDigOnFieldToNothing;//#F01_00_b
        private static ConfigEntry<bool> CE_F01_01_Replace2DirtFloor;//#F01_01
        private static ConfigEntry<bool> CE_F02_00_DrawingWaterByEmptyBottle;//#F02_00  
        private static ConfigEntry<bool> CE_F02_01_ReplaceWaterFloor;//#F02_01
        private static ConfigEntry<bool> CE_F02_01_a_ToggleReplaceWaterFloorFunction;//#F02_01_a //v0.3.1.0

        private static ConfigEntry<KeyCode> CE_KeyCode;//v0.3.0.0
        private static ConfigEntry<bool> CE_DebugLogging;//#F02_01
        
        public static bool config_F01_00_a_ModDigOnField => CE_F01_00_a_ModDigOnField.Value;//#F01_00_a
        //public static bool configFlagModInfiniteDigOnFieldToNothing => flagModInfiniteDigOnFieldToNothing.Value;//#F01_00_b
        public static bool config_F01_01_Replace2DirtFloor => CE_F01_01_Replace2DirtFloor.Value;//#F01_01
        public static bool config_F02_00_DrawingWaterByEmptyBottle => CE_F02_00_DrawingWaterByEmptyBottle.Value;//#F02_00
        public static bool config_F02_01_ReplaceWaterFloor => CE_F02_01_ReplaceWaterFloor.Value;//#F2_01
        public static bool config_F02_01_a_ToggleReplaceWaterFloorFunction => CE_F02_01_a_ToggleReplaceWaterFloorFunction.Value;//#F2_01_a //v0.3.1.0

        public static bool IsFunctionKeyDown = false;//v0.3.0.0
        public static bool configDebugLogging => CE_DebugLogging.Value;
        
        private void LoadConfig()
        {
            CE_F02_00_DrawingWaterByEmptyBottle = Config.Bind("#FUNC_02_00", "DRAWING_WATER_BY_EMPTY_BOTTLE", true, "If true, you can drawing water from floor of water by empty bottle");

            CE_F02_01_ReplaceWaterFloor = Config.Bind("#FUNC_02_01", "REPLACE_WATER_FLOOR", true, "If Press left shift ley, drawing water changes water floor");
            CE_F02_01_a_ToggleReplaceWaterFloorFunction = Config.Bind("#FUNC_02_01_a", "Toggle_REPLACE_WATER_FLOOR", false, "If true, toggle replacing function by function key");//v0.3.1.0

            CE_F01_00_a_ModDigOnField = Config.Bind("#FUNC_01_00_a", "MOD_DIG_ON_FIELD", true, "Change the deliverables when you perform a dig in the field");
            //flagModInfiniteDigOnFieldToNothing = Config.Bind("#FUNC_01_00_b", "CHANGE_TO_DIGGING_NOTHING_ON_FIELD", false, "Nothing will be dug out of the field(Not Recommend)");
            CE_F01_01_Replace2DirtFloor = Config.Bind("#FUNC_01_01", "REPLACE_FLOOR_AFTER_DIGGING_CHUNK", true, "Replace some floors with dirt floors after digging");
            CE_KeyCode = Config.Bind<KeyCode>("#KeyBind", "Function_Key", KeyCode.LeftShift, "Function_key");//v0.3.0.0
            CE_DebugLogging = Config.Bind("#z_Debug", "DEBUG_LOGGING", false, "For Debug");
        }
        //////////////////////////////////////////////////////////////////////
        
        /*
        public static bool FlagModDiggingOnField(Point point){
            if(config_F01_00_a_ModDigOnField){
                return true;
            } else {
                return false;
            }
        }*/
         public string TorF(bool b){
            return (b)? "T": "F";
        }
        private void Start()
        {
            LoadConfig();
            if(configDebugLogging){
                string text = "[FR]Config";
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
            var harmony = new Harmony("Main");
            new Harmony("Main").PatchAll();
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
    //++++EXE++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [HarmonyPatch]
    public class MapExe{
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Map), "MineFloor")]
        public static bool Prefix(Map __instance, Point point, Chara c, bool recoverBlock, bool removePlatform){
            if(!Main.config_F01_00_a_ModDigOnField || Main.IsFunctionKeyDown){return true;} //#FUNC_01a Flag:falseならvanilla //
            //if(Main.config_F01_01_Replace2DirtFloor){return true;} //#FUNC_01Another Flag:trueなら何もしない
            
            //----debug------------------------------------------------------------------------
            /*
            string text = "[LS]MF [";
            text += "Map:" + __instance.ToString() + "][";
            text += "P:" + point.ToString() + "][";
            text += "C:" + c.ToString() + "][";
            text += "rB:" + recoverBlock.ToString() + "][";
            text += "rP:" + removePlatform.ToString() + "][";
            text += "]";
            */
            //---debug kokomade--------------------------------------------------------------------
            //if(Main.configFlagModInfiniteDigOnFieldToNothing){return false;} //#FUNC_01b　Flag:trueなら掘りつつアイテム入手をスキップ
            if(point.sourceFloor.id == 4){
                //Debug.Log("[FR]PointMat" + point.matFloor.id.ToString());
                int matF = point.matFloor.id;
                int num = UnityEngine.Random.Range(0, 99999);
                //int[] numbers = new listlize(num);
                //int bingo = exeBingo(numbers);
                int seed;
                //string prod = "";
                Thing t = null;

                if(Main.configDebugLogging){
                    string text = "[FR]Gatya ";
                    text += "[num:" + num.ToString() + "]";
                    //text += "[Bingo:" + bingo.ToString() + "]";
                    Debug.Log(text);
                }

                switch(num){//v0.3.2.0
                    //SS rare
                    case 0 : t = ThingGen.Create("828");//うみみゃあkouun
                    break;
                    case 1 : t = ThingGen.Create("659");//iyashi
                    break;
                    case 2 : t = ThingGen.Create("758");//genso
                    break;
                    case 3 : t = ThingGen.Create("759");//daiti
                    break;
                    case 4 : t = ThingGen.Create("806");//syuukaku
                    break;
                    case 5 : t = ThingGen.Create("1190");//wind
                    break;
                    case 6 : t = ThingGen.Create("1191");//machine
                    break;
                    case 7 or 77 or 777 or 7777 or 77777 : seed = EClass.pc.LV * 1000;
                        if(seed < 1000){seed = 1000;};
                        t = ThingGen.CreateCurrency(UnityEngine.Random.Range(seed, seed*100));
                    break;
                    case 8 : t = ThingGen.Create("medal").SetNum(5);
                    break;
                    case 9 : t = ThingGen.Create("ticket_fortune").SetNum(EClass.rnd(9) + 1);
                    break;
                    case 10 : t = ThingGen.Create("scratchcard").SetNum((EClass.rnd(9) + 1) * 10);
                    break;

                    //S rare
                    case >= 11 and < 50 : t = ThingGen.Create("map_treasure");
                    break;
                    case >= 50 and < 100 : t = ThingGen.Create("money2").SetNum(EClass.rnd(4) + 1);//gold bar
                    break;
                    case >= 100 and < 200 : t = ThingGen.Create("plat").SetNum(EClass.rnd(9) + 1);;//plat
                    break;
                    case >= 200 and < 250 : t = ThingGen.Create("gacha_coin_gold");
                    break;
                    case >= 250 and < 350 : t = ThingGen.Create("gacha_coin_silver").SetNum(EClass.rnd(2) + 1);
                    break;
                    case >= 350 and < 500 : t = ThingGen.Create("gacha_coin").SetNum(EClass.rnd(4) + 1);
                    break;
                    case >= 500 and < 750 : t = ThingGen.Create("casino_coin").SetNum((EClass.rnd(9) + 1) * 10);
                    break;
                    case >= 750 and < 1000: 
                        seed = EClass.pc.LV * 10;
                        if(seed < 10){seed = 10;};
                        t = ThingGen.CreateCurrency(UnityEngine.Random.Range(seed/10, seed*10));
                    break;

                    //uncommon
                    case >= 1000 and < 2000 : t = ThingGen.Create("seed");
                        break;
                    case >= 2000 and < 3000 : t = ThingGen.Create("needle");
                        break;
                    case >= 3000 and < 4000 : t = ThingGen.Create("scrap", 78);//plastic
                        break;
                    case >= 4000 and < 5000 : t = ThingGen.Create("bone");
                        break;
                    case >= 5000 and < 6000 : t = ThingGen.Create("725");//animal bone
                        break;
                    case >= 6000 and < 7000 : t = ThingGen.Create("fang");
                        break;
                    case >= 7000 and < 8000 : t = ThingGen.Create("skin");
                        break;
                    case >= 8000 and < 9000 : t = ThingGen.Create("vine");
                        break;
                    case >= 9000 and < 10000 : t = ThingGen.Create("branch");
                        break;
                    case >= 10000 and < 10500 : t = ThingGen.Create("158");//stone rubble
                        break;
                    case >= 10500 and < 11000 : t = ThingGen.Create("181");//stone rubble
                        break;
                    case >= 11000 and < 11500 : t = ThingGen.Create("184");//bone r
                        break;
                    case >= 11500 and < 12000 : t = ThingGen.Create("185");//bone r
                        break;
                    case >= 12000 and < 12500 : t = ThingGen.Create("186");//stone r
                        break;
                    case >= 12500 and < 13000 : t = ThingGen.Create("187");//stone r
                        break;
                    case >= 13000 and < 13500 : t = ThingGen.Create("scrubber");//darekagasuteta
                        break;
                    case >= 13500 and < 14000 : t = ThingGen.Create("tissue");//darekagasuteta
                        break;
                    case >= 14000 and < 14333 : t = ThingGen.Create("529");//can
                        break; 
                    case >= 14333 and < 14666 : t = ThingGen.Create("1170");//can
                        break; 
                    case >= 14666 and < 15000 : t = ThingGen.Create("236");//can
                        break;
                    case >= 15000 and < 16000 : t = ThingGen.Create("726");//bottle
                        break;
                    case >= 16000 and < 17000 : t = ThingGen.Create("728");//bottle
                        break;
                    case >= 17000 and < 17500  : t = ThingGen.Create("219");//map
                        break;
                    case >= 17500 and < 18000  : t = ThingGen.Create("220");//scroll
                        break; 
                    case >= 18000 and < 18500  : t = ThingGen.Create("221");//scroll
                        break;
                    case >= 18500 and < 19000  : t = ThingGen.Create("216");//paper
                        break; 
                    case >= 19000 and < 19500  : t = ThingGen.Create("217");//paper
                        break;
                    case >= 19500 and < 20000  : t = ThingGen.Create("218");//paper
                        break;
                    
                    case >= 20000 and < 21000: t = ThingGen.Create("ore",78);//plastic
                        break;
                    //commmon
                    case >= 21000 and < 25000: t = ThingGen.Create("rock");
                        break;
                    case >= 25000 and < 35000 : t = ThingGen.Create("pebble").SetNum(EClass.rnd(1) + 1);
                        break;
                    case >= 35000 and < 50000 : t = ThingGen.Create("stone").SetNum(EClass.rnd(4) + 1);
                        break;
                    default  : t = ThingGen.Create("chunk",matF);//respectfloormaterial
                        break;
                    
                }
                if(t != null){
                    //c.Pick(t);
                    __instance.TrySmoothPick(point, t, c);
                }
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Map), "MineFloor")]
        public static void Postfix(Map __instance, Point point, Chara c){
            if(Main.configDebugLogging){
                //Debug.Log("[FR]KD[" + ((Main.IsFunctionKeyDown)? "T" : "F") + "]");
                //Debug.Log("[FR]KD[" + ((Main.IsFunctionKeyDown)? "T" : "F") + "]");
            }
            if(Main.config_F01_01_Replace2DirtFloor && ContainsChunk(point)){//edit //v0.3.1.1
                if(point.sourceFloor.id == 4){
                    if(!Main.config_F01_00_a_ModDigOnField || Main.IsFunctionKeyDown){
                        point.SetFloor(45, 40);
                    }
                } else {
                    point.SetFloor(45, 40);
                }
            }
            /*
            if(Main.config_F01_01_Replace2DirtFloor && ContainsChunk(point) && point.sourceFloor.id == 4){
                if(!Main.config_F01_00_a_ModDigOnField || Main.IsFunctionKeyDown){
                    point.SetFloor(45, 40);
                }
            } */  
        }
        private static bool ContainsChunk(Point point){
            if(point.sourceFloor.components[0].Contains("chunk@soil") || point.sourceFloor.components[0].Contains("chunk@snow") || point.sourceFloor.components[0].Contains("chunk@ice")){
                //if(Main.FlagModDiggingOnField(point)){
                //   return false; 
                //}
                return true;
            } else {
                return false;
            }
        }    
        /*
        private int[] listlize(int n){
            int num0,num1,num2,num3,num4;
            if(n >= 10000){
                num0 = n / 10000;
                n = n - num0 * 10000;
            } else {
                num0 = 0;
            }
            if(n >= 1000){
                num1 = n / 1000;
                n = n - num1 * 1000;
            } else {
                num1 = 0;
            }
            if(n >= 100){
                num2 = n / 100;
                n = n - num2 * 100;
            } else {
                num2 = 0;
            }
            if(n >= 10){
                num3 = n / 10;
                n = n - num3 * 10;
            } else {
                num3 = 0;
            }
            num4 = n;
            int[] res = new int[]{num0,num1,num2,num3,num4};
            return res;
        }
        private int exeBingo(int[] numbers){
            int r = 0;
            int hitnum = new int[]{0,0,0,0,0};
            for(int i = 0;i < numbers.Length; i++){
                for(int j = i + 1;j < numbers.Length; j++){
                    if(numbers[i] == numbers[j]){
                        hitnum[i] += 1;
                    }
                }
            }
            for(int i = 0; i < hitnum.Length; i++){
                if(hitnum[i] >= r){
                    r = hitnum[i];
                }
            }
            return r;
        }*/
    }

    [HarmonyPatch]
    public class TraitPotionEmptyPatch{
        internal static Point pos = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TraitPotionEmpty), "CanUse")]
        internal static bool Prefix(Chara c, Point p, ref bool __result){
            if(!Main.config_F02_00_DrawingWaterByEmptyBottle){return true;}
            if(p.cell.IsTopWater){
                __result = p.cell.IsTopWaterAndNoSnow;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TraitPotionEmpty), "OnUse")]
        internal static bool Prefix(Chara c, Point p,TraitPotionEmpty __instance, ref bool __result){
            TraitWell well = __instance.GetWell(p);
            if(well == null){
                SE.Play("water_farm");
                pos = p;
                //int idCell = pos.cell;
                //Debug.Log("[OK]source:" + pos.cell.ToString());
                switch ((pos.HasBridge ? pos.sourceBridge : pos.sourceFloor).alias)
                {
                    case "floor_water_shallow":
                        ChangeFloor("floor_water_shallow2");
                        break;
                    case "floor_water":
                        ChangeFloor("floor_water_shallow");
                        break;
                    case "floor_water_deep":
                        ChangeFloor("floor_water");
                        break;
                    default:
                        ChangeFloor("floor_raw3");
                        break;
                }
                
                if (EClass.rnd(3) == 0)
                {
                    c.stamina.Mod(-1);
                }
                
                //Debug.Log("czbioid = " + EClass.pc.currentZone.biome.id.ToString());
                __instance.owner.ModNum(-1);
                string biome = EClass.pc.currentZone.biome.id.ToString();
                Thing t;
                if(biome == "Sand" || biome == "Water"){//sea
                    t = ThingGen.Create("1142");//siomizu
                } else {
                    t = ThingGen.Create("water_dirty");
                }
                t.blessedState = BlessedState.Normal;//v0.2.0.0
                c.Pick(t);
                __result = true;
                return false;

            }
            return true;
        }

        internal static void ChangeFloor(string id)
        {
            if(!Main.config_F02_01_ReplaceWaterFloor){//edit v0.3.1.0
                return ;
            }
            if(!Main.config_F02_01_a_ToggleReplaceWaterFloorFunction){//add v0.3.1.0
                if(!Main.IsFunctionKeyDown){return;}
            } else {
                if(Main.IsFunctionKeyDown){return;}
            }
            
            SourceFloor.Row row = EClass.sources.floors.alias[id];
            if (pos.HasBridge)
            {
                pos.cell._bridge = (byte)row.id;
                if (id == "floor_raw3")
                {
                    pos.cell._bridgeMat = 45;
                }
            }
            else
            {
                pos.cell._floor = (byte)row.id;
                if (id == "floor_raw3")
                {
                    pos.cell._floorMat = 45;
                }
            }
            EClass._map.SetLiquid(pos.x, pos.z);
            pos.RefreshNeighborTiles();
        }
        
    }
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