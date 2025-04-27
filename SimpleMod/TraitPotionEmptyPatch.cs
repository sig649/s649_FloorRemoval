using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
using System.IO;
//using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;//v0.3.3.0 grep
using s649FR.Main;


namespace s649FR {
    namespace TraitPotionEmptyPatch {
        [HarmonyPatch]
        internal class TaitPotionEmptyPatchMain{//v0.3.4.0 ->internal
            private static Point pos = null; //v0.3.4.0 ->private

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TraitPotionEmpty), "CanUse")]
            internal static bool CanUsePrePatch(Chara c, Point p, ref bool __result){//v0.3.3.0 namefix
                //potion_emptyが水の床に対して使用できるかの判定

                //除外処理
                if(PatchMain.IsOnGlobalMap()){return true;}//globalmapにいる
                if(!PatchMain.config_F02_00_DrawingWaterByEmptyBottle){return true;}//02_00がfalse

                if(p.cell.IsTopWater){
                    __result = p.cell.IsTopWaterAndNoSnow;//凍っていないかどうか
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TraitPotionEmpty), "OnUse")]
            internal static bool OnUsePrePatch(Chara c, Point p,TraitPotionEmpty __instance, ref bool __result){//v0.3.3.0 namefix
                //空き瓶を水の床に向かって使うときの処理

                //除外処理
                if(PatchMain.IsOnGlobalMap()){return true;}//v0.4.0.0 add
                //if(!PatchMain.config_F02_00_DrawingWaterByEmptyBottle){return true;}//02_00がfalse

                TraitWell well = __instance.GetWell(p);
                if(well == null)//井戸はないが水の床はある時
                {
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

            private static void ChangeFloor(string id){//v0.3.4.0 ->private
                //除外処理
                if(!PatchMain.config_F02_01_ReplaceWaterFloor){//edit v0.3.1.0
                    return ;
                }
                //if(!PatchMain.config_F02_01_a_ToggleReplaceWaterFloorFunction){//add v0.3.1.0
                //    if(!PatchMain.IsFunctionKeyDown){return;}
                //} else {
                //    if(PatchMain.IsFunctionKeyDown){return;}
                //}
                if(!PatchMain.IsFunctionKeyDown){return;}

                //置き換え処理　ソースはバニラの渇きの壺より
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
    }
}