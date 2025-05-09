using System;
using System.IO;
using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;

//using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;//v0.3.3.0 grep
using s649FR.Main;
//using Main = s649FR.Main;

namespace s649FR {
    namespace MapPatch {
        [HarmonyPatch]
        internal class MapExe{//v0.3.4.0 ->internal
            internal static int reroll(int num,int LUC){//v0.3.3.0 add
                int amari;
                while(LUC > 0){
                    if(IsLuckNumber(num)){//v0.3.3.1 moved
                        break;
                    }
                    if(LUC > 1000){
                        //amari = (LUC > 100)?LUC % 100 : LUC;
                        //amari = (amari == 0)? 100: amari;
                        num = (int)(num * Random.Range(0.1f,1.0f));//v0.4.0.0
                        LUC /= 10;//v0.4.0.0 edit
                    } else {
                        amari = (LUC > 100)?LUC % 100 : LUC;
                        amari = (amari == 0)? 100: amari;
                        //num = Random.Range(num * (200 - amari) / 200, num);
                        num = (int)(num * Random.Range((float)((200 - amari)/200f), 1.0f));//v0.4.0.0

                        LUC -= 100;
                    }
                }
                return num;
            }
            internal static bool IsLuckNumber(int n){//v0.4.0.0 edit
                switch(n){
                    case <= 50 : return true;
                    case 77 or 777 or 7777 or 77777 or 777777: return true;
                }
                return false;
            }
            internal static string GetListRandom(string[] sList){//v0.4.0.0
                if(sList == null){return "";} else {
                    return sList[Random.Range(0,sList.Length)];
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Map), "MineFloor")]
            internal static bool MineFloorPrefix(Map __instance, Point point, Chara c, bool recoverBlock, bool removePlatform){//v0.3.4.0 ->internal  //v0.3.3.0 namefix
                if(PatchMain.IsOnGlobalMap()){return true;}//v0.4.0.0 add
                if(!PatchMain.config_F01_00_a_ModDigOnField || PatchMain.IsFunctionKeyDown){return true;} //#FUNC_01a Flag:falseならvanilla //
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
                    int num = EClass.rnd(1000000);//v0.4.0.0 edit
                    int onum = num;
                    int LUC = EClass.pc.LUC;//v0.3.3.0
                    //int[] numbers = new listlize(num);
                    //int bingo = exeBingo(numbers);
                    int seed;
                    int DLV = PatchMain.currentDLV;//v0.3.4.0
                    string prod = "-";
                    Thing t = null;

                    num = reroll(num, LUC);//v0.3.3.0
                    
                    if(num < 1000 || IsLuckNumber(num)){//v0.4.0.0
                        if(IsLuckNumber(num) || num < 50){
                            //あなたは財宝を掘り当てた SSR~SR
                            SE.Play("ding_skill");
                            Msg.Say("digTreasure");
                        } else {
                            Msg.Say("dropReward");// R
                        }
                    
                    }
                
                    switch(num){//v0.4.0.0 edit
                        //SS rare(~7 or luckynumber)
                        case 0 : prod = "828";
                        t = (DLV >= 10)? ThingGen.Create(prod) : ThingGen.Create("medal").SetNum(5);//うみみゃあkouun
                        break;
                        case 1 : prod = "659";
                        t = (DLV >= 10)? ThingGen.Create(prod) : ThingGen.Create("medal").SetNum(5);//iyashi 659
                        break;
                        case 2 : prod = "758";
                        t = (DLV >= 10)? ThingGen.Create(prod) : ThingGen.Create("medal").SetNum(5);//genso 758
                        break;
                        case 3 : prod = "759";
                        t = (DLV >= 10)? ThingGen.Create(prod) : ThingGen.Create("medal").SetNum(5);//daiti 759
                        break;
                        case 4 : prod = "806";
                        t = (DLV >= 10)? ThingGen.Create(prod) : ThingGen.Create("medal").SetNum(5);//syuukaku 806
                        break;
                        case 5 : prod = "1190";
                        t = (DLV >= 10)? ThingGen.Create(prod) : ThingGen.Create("medal").SetNum(5);//wind 1190
                        break;
                        case 6 : prod = "1191";
                        t = (DLV >= 10)? ThingGen.Create(prod) : ThingGen.Create("medal").SetNum(5);//machine 1191
                        break;
                        case 7 or 77 or 777 or 7777 or 77777 or 777777:
                        prod = "777";
                        seed = EClass.pc.LV * 1000;//v0.4.0.0 num add
                        if(seed < 1000){seed = 1000;};
                        float DLVinf = (DLV > 0)?(float)Math.Sqrt(DLV):1f;//v0.4.1.0 float
                        if(DLVinf < 1f){DLVinf = 1f;} else if(DLVinf > 100f){DLVinf = 100f;}//v0.4.1.0 float
                        if(DLV > 0){seed = (int)Math.Floor(DLVinf * seed);}//v0.4.1.0 tweak
                        t = ThingGen.CreateCurrency((int)Math.Floor(seed * Random.Range(1f, 2f)));//v0.4.1.0 tweak
                        break;

                        //S rare(~50)
                        case >= 8 and < 25: prod = "medal";
                        t = ThingGen.Create("medal").SetNum((EClass.rnd(3) + 1));//v0.4.0.0 down
                        break;
                        case >= 25 and < 50: prod = "map_treasure";
                        t = ThingGen.Create("map_treasure", -1, EClass.pc.LV);//v0.4.0.0 moved edit
                        break;

                        //rare(~1000)
                        case >= 50 and < 100 : prod = "ticket_fortune";
                        t = ThingGen.Create("ticket_fortune").SetNum(EClass.rnd(5) + 1);//v0.4.0.0 down
                        break;
                        case >= 100 and < 150 :  prod = "scratchcard";
                        t = ThingGen.Create("scratchcard").SetNum((EClass.rnd(5) + 1) * 2);//v0.4.0.0 edit
                        break;
                        case >= 150 and < 250 : prod = "money2";
                        t = ThingGen.Create("money2").SetNum(EClass.rnd(5) + 1);//gold bar
                        break;
                        case >= 250 and < 350 : prod = "plat";
                        t = ThingGen.Create("plat").SetNum(EClass.rnd(10) + 1);;//plat
                        break;
                        case >= 350 and < 400 : prod = "gacha_coin_gold";
                        t = ThingGen.Create("gacha_coin_gold");
                        break;
                        case >= 400 and < 450 : prod = "gacha_coin_silver";
                        t = ThingGen.Create("gacha_coin_silver").SetNum(EClass.rnd(2) + 1);
                        break;
                        case >= 450 and < 500 : prod = "gacha_coin";
                        t = ThingGen.Create("gacha_coin").SetNum(EClass.rnd(5) + 1);
                        break;
                        case >= 500 and < 750 : prod = "casino_coin";
                        t = ThingGen.Create("casino_coin").SetNum((int)(Random.Range(1f,10f) * 10));
                        break;
                        case >= 750 and < 1000: prod = "oren";//v0.4.0.0 tweak
                        seed = (EClass.pc.LV < 0)? 100 : EClass.pc.LV * 100;//v0.4.0.1 mangaichi
                        //if(seed < 100){seed = 100;}v//v0.4.0.1 huyoura!
                        if(seed > DLV*100 && DLV > 0){seed = DLV * 100;}//v0.4.0.1 cap add
                        t = ThingGen.CreateCurrency((int)Math.Floor(Random.Range(1f, 10f) * seed));//v0.4.1.0 edit
                        break;
                        //R-
                        case >= 1000 and < 10000 : //v0.4.1.0 ore
                            prod = "ore";
                            t = ThingGen.Create(prod, 78);//plastic ore
                        break;
                        //uncommon+(~25000)
                        case >= 10000 and < 25000 : //v0.4.1.0 bone,fang ->UC ore->UC++
                        //string[] ucPlasList = new string[]{"seed","needle","bone","fang","skin","vine","branch","ore"};
                        string[] ucPlasList = new string[]{"seed","needle","skin","vine","branch"};
                        //prod = ucPlasList[Random.Range(0, ucPlasList.Length)];
                            prod = GetListRandom(ucPlasList);
                            t = ThingGen.Create(prod);
                        break;
                        
                        //uncommon(~100000)  junk? hahaha...
                        case >= 25000 and < 100000 : 
                        /* uncommon list
                        animal bone : 725 
                        scrap@plast : scrap@78
                        //stone r : 158 , 181  //nanimodenai
                        scrubber, tissue //darekagasuteta
                        can : 236, 529, 1170
                        bottle : 726, 727,728
                        paper : 191, 193, 196, 197, 219, 220, 221 216, 217, 218, 206,207, 729,730
                        1172 : completely worthless ...  to->scrap
                        grave : 930,950,951,952,931,947,948,949,944,945,946
                        wood r : 182,183
                        rubber : 1178,1179
                        Lcat : 1180 => rubber
                        rope : 209,210
                        fish bone : 738 -> bone
                        fossil : 1053 -> bone
                        haizai : 891,892 -> scrap
                        */
                        string[] uclist = new string[]{"bone","scrap","suteta","can","bottle","paper","grave","wood","rubber","rope"};
                        //prod = uclist[Random.Range(0, uclist.Length)];
                        prod = GetListRandom(uclist);
                        switch(prod){
                            case "bone" : if(EClass.rnd(10) == 0){
                                t = ThingGen.Create("1053");//fossil
                            } else {
                                switch(EClass.rnd(50))//v0.4.1.0 edit add bone fang
                                {
                                    case >= 0 and < 25: t = ThingGen.Create("bone");//bone
                                    break;
                                    case >= 25 and < 40 : t = ThingGen.Create("fang");//fang
                                    break;
                                    case >= 40 and < 45 : t = ThingGen.Create("725");//animal
                                    break;
                                    default : t = ThingGen.Create("738");//deadfish
                                    break;
                                }
                                /*
                                if(EClass.rnd(2) == 0){
                                    t = ThingGen.Create("725");//animal
                                } else {
                                    t = ThingGen.Create("738");//deadfish
                                }
                                */
                            }
                            break;
                            case "scrap" : //v0.4.0.2 edit
                            if(EClass.rnd(10) == 0)
                            {
                                if(EClass.rnd(10000) != 0)//v0.4.0.3 tweak
                                {
                                    t = ThingGen.Create("1172", 14);//completely worthless /v0.4.0.4 change dou
                                    t.Dye("gold");// /v0.4.1.1 dyed
                                } else 
                                {
                                    t = ThingGen.Create("1172");//completely worthless
                                }
                            } else if(EClass.rnd(2) == 0)
                            {
                                if(EClass.rnd(2) == 0)
                                {
                                    t = ThingGen.Create("891");//haizai1
                                } else 
                                {
                                    t = ThingGen.Create("892"); //haizai2
                                }
                            } else
                            {
                                t = ThingGen.Create("scrap", 78);//plastic
                            }
                            break;
                            case "suteta" :  if(EClass.rnd(2) == 0){
                                t = ThingGen.Create("scrubber");
                            } else {
                                t = ThingGen.Create("tissue");
                            }
                            break;
                            case "can" : 
                                string[] canlist = new string[]{"236","529","1170"};//v0.4.0.0fix
                                //prod = canlist[Random.Range(0, canlist.Length)];
                                //prod = GetListRandom(canlist);
                                t = ThingGen.Create(GetListRandom(canlist));
                            break;
                            case "bottle" : 
                                string[] bottlelist = new string[]{"726","727","728"};
                                t = ThingGen.Create(GetListRandom(bottlelist));
                            break;
                            case "paper" : 
                                string[] paperlist = new string[]{"191","193","196","197","219","220","221","216","217","218","206","207","729","730"};
                                t = ThingGen.Create(GetListRandom(paperlist));
                            break;
                            case "grave" : 
                            if(EClass.rnd(10) == 0){//gold
                                switch(EClass.rnd(3)){
                                    case 0 :t = ThingGen.Create("944");
                                    break;
                                    case 1 :t = ThingGen.Create("945");
                                    break;
                                    default :t = ThingGen.Create("946");
                                    break;
                                }
                            } else {
                                string[] gravelist = new string[]{"930","950","951","952","931","947","948","949"};//grave : 930,950,951,952,931,947,948,949,944,945,946
                                t = ThingGen.Create(GetListRandom(gravelist));
                            } 
                            break;
                            case "wood" : //wood r : 182,183
                            if(EClass.rnd(2) == 0){
                                t = ThingGen.Create("182");
                            } else {
                                t = ThingGen.Create("183");
                            }
                            break;
                            case "rubber" : //rubber : 1178,1179  Lcat : 1180 => rubber
                            if(EClass.rnd(10) == 0){
                                t = ThingGen.Create("1180");
                            } else {
                                if(EClass.rnd(2) == 0){
                                    t = ThingGen.Create("1178");
                                } else {
                                    t = ThingGen.Create("1179");
                                }
                            }
                            break;
                            case "rope" : if(EClass.rnd(2) == 0){//rope : 209,210
                                t = ThingGen.Create("209");
                            } else {
                                t = ThingGen.Create("210");
                            }
                            break;
                            default : 
                            break;
                        }
                        break;
                    
                    
                        //commmon(~500000)  /v0.4.1.0
                        case >= 100000 and < 150000: prod = "rock";
                        t = ThingGen.Create("rock");
                        break;
                        case >= 150000 and < 250000 : prod = "pebble";
                        t = ThingGen.Create("pebble").SetNum(EClass.rnd(2) + 1);
                        break;
                        case >= 250000 and < 500000 : prod = "stone";
                        t = ThingGen.Create("stone").SetNum(EClass.rnd(5) + 1);
                        break;
                        default  : t = ThingGen.Create("chunk",matF);//respectfloormaterial
                        break;
                    }
                    if(t != null){
                        //c.Pick(t);
                        __instance.TrySmoothPick(point, t, c);
                    }
                    if(PatchMain.configDebugLogging){
                        string text = "[FR]Gatya ";
                        text += "[num:" + num.ToString() + "]";
                        text += "[onum:" + onum.ToString() + "]";
                        text += "[LUC:" + LUC.ToString() + "]";
                        text += "[LV:" + EClass.pc.LV.ToString() + "]";
                        text += "[DLV:" + DLV.ToString() + "]";
                        text += "[prod:" + prod + "]";
                        
                        Debug.Log(text);
                    }
                    return false;
                }
                
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Map), "MineFloor")]
            internal static void MineFloorPostfix(Map __instance, Point point, Chara c){//v0.3.4.0 ->internal   //v0.3.3.0 namefix
                if(PatchMain.IsOnGlobalMap()){return;}//v0.4.0.0 add
                //if(Main.configDebugLogging){
                //Debug.Log("[FR]KD[" + ((Main.IsFunctionKeyDown)? "T" : "F") + "]");
                //Debug.Log("[FR]KD[" + ((Main.IsFunctionKeyDown)? "T" : "F") + "]");
                //}
                if(PatchMain.config_F01_01_Replace2DirtFloor && ContainsChunk(point)){//edit //v0.3.1.1
                    if(point.sourceFloor.id == 4){
                        if(!PatchMain.config_F01_00_a_ModDigOnField || PatchMain.IsFunctionKeyDown){
                            point.SetFloor(45, 40);
                        }
                    } else {
                        point.SetFloor(45, 40);
                    }
                }
                
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
        }   
    }
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////
///trash box


/*
                if(Main.config_F01_01_Replace2DirtFloor && ContainsChunk(point) && point.sourceFloor.id == 4){
                if(!Main.config_F01_00_a_ModDigOnField || Main.IsFunctionKeyDown){
                    point.SetFloor(45, 40);
                }
                } */  




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



        /*
                    case >= 5000 and < 6000 : t = ThingGen.Create("725");//animal bone
                        break;
                    case >= 3000 and < 4000 : t = ThingGen.Create("scrap", 78);//plastic
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
                    */



                    /*
                        case >= 1000 and < 2000 : t = ThingGen.Create("seed");
                        break;
                        case >= 2000 and < 3000 : t = ThingGen.Create("needle");
                        break;
                    
                        case >= 4000 and < 5000 : t = ThingGen.Create("bone");
                        break;
                    
                        case >= 6000 and < 7000 : t = ThingGen.Create("fang");
                        break;
                        case >= 7000 and < 8000 : t = ThingGen.Create("skin");
                        break;
                        case >= 8000 and < 9000 : t = ThingGen.Create("vine");
                        break;
                        case >= 9000 and < 10000 : t = ThingGen.Create("branch");
                        break;
                        case >= 20000 and < 25000: t = ThingGen.Create("ore",78);//plastic ore
                        break;
                        */