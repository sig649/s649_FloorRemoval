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
    [BepInPlugin("s649_FloorRemoval", "s649 Floor Removal", "0.1.0.0")]

    public class Main : BaseUnityPlugin
    {
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
        private void LoadConfig()
        {
            CE_DrawingWaterByEmptyBottle = Config.Bind("#FUNC_02_00", "DRAWING_WATER_BY_EMPTY_BOTTLE", true, "If true, you can drawing water from floor of water by empty bottle");

            CE_FlagReplaceWaterFloor = Config.Bind("#FUNC_02_01", "REPLACE_WATER_FLOOR", true, "If true, drawing water changes water floor");

            flagModInfiniteDigOnField = Config.Bind("#FUNC_01_00_a", "MOD_INFINITE_DIG", true, "You will be able to get stones from the field");
            flagModInfiniteDigOnFieldToNothing = Config.Bind("#FUNC_01_00_b", "CHANGE_TO_DIGGING_NOTHING_ON_FIELD", false, "Nothing will be dug out of the field");
            flagModDiggingChunk = Config.Bind("#FUNC_01_01", "MOD_DIGGING_CHUNK", true, "Replace some floors with dirt floors after digging");
        }
        //////////////////////////////////////////////////////////////////////
        
        private static ConfigEntry<bool> flagModInfiniteDigOnField;//#F_01_00_a
        private static ConfigEntry<bool> flagModInfiniteDigOnFieldToNothing;//#F01_00_b
        private static ConfigEntry<bool> flagModDiggingChunk;//#F01_01
        private static ConfigEntry<bool> CE_DrawingWaterByEmptyBottle;//#F02_00
        
        private static ConfigEntry<bool> CE_FlagReplaceWaterFloor;//#F02_01

        

        public static bool configFlagModInfiniteDigOnField => flagModInfiniteDigOnField.Value;
        public static bool configFlagModInfiniteDigOnFieldToNothing => flagModInfiniteDigOnFieldToNothing.Value;
        public static bool configFlagModDiggingChunk => flagModDiggingChunk.Value;
        public static bool configDrawingWaterByEmptyBottle => CE_DrawingWaterByEmptyBottle.Value;
        public static bool configReplaceWaterFloor => CE_FlagReplaceWaterFloor.Value;
        
        
        
        public static bool CanDigStoneOnField(Point point){
            if(configFlagModInfiniteDigOnField && point.sourceFloor.id == 4){
                return true;
            } else {
                return false;
            }
        }
        private void Start()
        {
            LoadConfig();
            //LastConfigLoadTime = DateTime.Now;
            //SetupConfigWatcher();
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
            //if(Main.configFlagModDiggingChunk){return true;} //#FUNC_01Another Flag:trueなら何もしない
            
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
            if(Main.configFlagModInfiniteDigOnFieldToNothing){return false;} //#FUNC_01b　Flag:trueなら掘りつつアイテム入手をスキップ
            if(point.sourceFloor.id == 4){
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Map), "MineFloor")]
        public static void Postfix(Map __instance, Point point, Chara c){
            if(Main.configFlagModDiggingChunk && ContainsChunk(point)){
                point.SetFloor(45, 40);
            }   
        }
        private static bool ContainsChunk(Point point){
            if(point.sourceFloor.components[0].Contains("chunk@soil") || point.sourceFloor.components[0].Contains("chunk@snow") || point.sourceFloor.components[0].Contains("chunk@ice")){
                if(Main.CanDigStoneOnField(point)){
                   return false; 
                }
                return true;
            } else {
                return false;
            }
        }
    }

    [HarmonyPatch]
    public class TraitPotionEmptyPatch{
        internal static Point pos = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TraitPotionEmpty), "CanUse")]
        internal static bool Prefix(Chara c, Point p, ref bool __result){
            if(!Main.configDrawingWaterByEmptyBottle){return true;}
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
                
                c.Pick(t);
                __result = true;
                return false;

            }
            return true;
        }

        internal static void ChangeFloor(string id)
        {
            if(!Main.configReplaceWaterFloor){
                return ;
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
            if(!Main.configFlagModDiggingChunk){return true;} //#FUNC_01Another Flag:falseなら何もしない
            
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