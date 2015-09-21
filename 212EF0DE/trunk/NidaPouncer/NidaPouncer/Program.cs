﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace NidaPouncer
{
    //Big thanks to blm95
    class Program
    {
        public static Dictionary<Vector3, Vector3> positions;
        public static String champName = "Nidalee";
        
        public static Obj_AI_Base player;
        public static Menu menu;
        
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell W;

        static void Main(string[] args)
        {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;       
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            player =  ObjectManager.Player;
            if (player.BaseSkinName != champName) return;
            fillPositions();

            W = new Spell(SpellSlot.W, 375f);
            SpellList.Add(W);

            menu = new Menu("Nida Pouncer", "NidaPMenu", true);

            menu.AddSubMenu(new Menu("Pouncer Drawing", "Drawing"));
            menu.SubMenu("Drawing").AddItem(new MenuItem("PouncerDr", "Draw Pounce Spots").SetValue(true));

            menu.AddSubMenu(new Menu("Pouncer Flee", "FleeM"));
            menu.SubMenu("FleeM").AddItem(new MenuItem("FleeKey", "Flee").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            menu.AddToMainMenu();

            Game.PrintChat("Nida Pouncer By DZ191 Loaded");  
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("PouncerDr").GetValue<bool>())
            {
                foreach (KeyValuePair<Vector3, Vector3> entry in positions)
                {
                    if (player.Distance(entry.Key) <= 1500f && player.Distance(entry.Value) <= 1500f)
                    {
                        Drawing.DrawCircle(entry.Key, 75f, Color.GreenYellow);
                        Drawing.DrawCircle(entry.Value, 75f, Color.GreenYellow);
                    }
                }
            }
        }


        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (menu.Item("FleeKey").GetValue<KeyBind>().Active)
           {
               foreach (KeyValuePair<Vector3, Vector3> entry in positions)
               {
                   if (player.Distance(entry.Key) <= 220f || player.Distance(entry.Value) <= 220f)
                   {
                       Vector3 closest = entry.Key;
                       Vector3 farther = entry.Value;
                       if (player.Distance(entry.Key) < player.Distance(entry.Value)) { closest = entry.Key; farther = entry.Value; }
                       if (player.Distance(entry.Key) > player.Distance(entry.Value)) { closest = entry.Value; farther = entry.Key; }
                       Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(closest.X, closest.Y)).Send();
                       W.Cast(farther, true); 
                   }
               }
           }
        }
        

        public static void fillPositions()
        {
            positions = new Dictionary<Vector3, Vector3>();
            Vector3 pos0 = new Vector3(6393.7299804688f, 8341.7451171875f, -63.87451171875f);
            Vector3 pos1 = new Vector3(6612.1625976563f, 8574.7412109375f, 56.018413543701f);
            positions.Add(pos0, pos1);
            Vector3 pos2 = new Vector3(7041.7885742188f, 8810.1787109375f, 0f);
            Vector3 pos3 = new Vector3(7296.0341796875f, 9056.4638671875f, 55.610824584961f);
            positions.Add(pos2, pos3);
            Vector3 pos4 = new Vector3(4546.0258789063f, 2548.966796875f, 54.257415771484f);
            Vector3 pos5 = new Vector3(4185.0786132813f, 2526.5520019531f, 109.35539245605f);
            positions.Add(pos4, pos5);
            Vector3 pos6 = new Vector3(2805.4074707031f, 6140.130859375f, 55.182941436768f);
            Vector3 pos7 = new Vector3(2614.3215332031f, 5816.9438476563f, 60.193073272705f);
            positions.Add(pos6, pos7);
            Vector3 pos8 = new Vector3(6696.486328125f, 5377.4013671875f, 61.310482025146f);
            Vector3 pos9 = new Vector3(6868.6918945313f, 5698.1455078125f, 55.616455078125f);
            positions.Add(pos8, pos9);
            Vector3 pos10 = new Vector3(1677.9854736328f, 8319.9345703125f, 54.923847198486f);
            Vector3 pos11 = new Vector3(1270.2786865234f, 8286.544921875f, 50.334892272949f);
            positions.Add(pos10, pos11);
            Vector3 pos12 = new Vector3(2809.3254394531f, 10178.6328125f, -58.759708404541f);
            Vector3 pos13 = new Vector3(2553.8962402344f, 9974.4677734375f, 53.364395141602f);
            positions.Add(pos12, pos13);
            Vector3 pos14 = new Vector3(5102.642578125f, 10322.375976563f, -62.845260620117f);
            Vector3 pos15 = new Vector3(5483f, 10427f, 54.5009765625f);
            positions.Add(pos14, pos15);
            Vector3 pos16 = new Vector3(6000.2373046875f, 11763.544921875f, 39.544124603271f);
            Vector3 pos17 = new Vector3(6056.666015625f, 11388.752929688f, 54.385917663574f);
            positions.Add(pos16, pos17);
            Vector3 pos18 = new Vector3(1742.34375f, 7647.1557617188f, 53.561042785645f);
            Vector3 pos19 = new Vector3(1884.5321044922f, 7995.1459960938f, 54.930736541748f);
            positions.Add(pos18, pos19);
            Vector3 pos20 = new Vector3(3319.087890625f, 7472.4760742188f, 55.027889251709f);
            Vector3 pos21 = new Vector3(3388.0522460938f, 7101.2568359375f, 54.486026763916f);
            positions.Add(pos20, pos21);
            Vector3 pos22 = new Vector3(3989.9423828125f, 7929.3422851563f, 51.94282913208f);
            Vector3 pos23 = new Vector3(3671.623046875f, 7723.146484375f, 53.906265258789f);
            positions.Add(pos22, pos23);
            Vector3 pos24 = new Vector3(4936.8452148438f, 10547.737304688f, -63.064865112305f);
            Vector3 pos25 = new Vector3(5156.7397460938f, 10853.216796875f, 52.951190948486f);
            positions.Add(pos24, pos25);
            Vector3 pos26 = new Vector3(5028.1235351563f, 10115.602539063f, -63.082695007324f);
            Vector3 pos27 = new Vector3(5423f, 10127f, 55.15357208252f);
            positions.Add(pos26, pos27);
            Vector3 pos28 = new Vector3(6035.4819335938f, 10973.666015625f, 53.918266296387f);
            Vector3 pos29 = new Vector3(6385.4013671875f, 10827.455078125f, 54.63500213623f);
            positions.Add(pos28, pos29);
            Vector3 pos30 = new Vector3(4747.0625f, 11866.421875f, 41.584358215332f);
            Vector3 pos31 = new Vector3(4743.23046875f, 11505.842773438f, 51.196254730225f);
            positions.Add(pos30, pos31);
            Vector3 pos32 = new Vector3(6749.4487304688f, 12980.83984375f, 44.903495788574f);
            Vector3 pos33 = new Vector3(6701.4965820313f, 12610.278320313f, 52.563804626465f);
            positions.Add(pos32, pos33);
            Vector3 pos34 = new Vector3(3114.1865234375f, 9420.5078125f, -42.718975067139f);
            Vector3 pos35 = new Vector3(2757f, 9255f, 53.77322769165f);
            positions.Add(pos34, pos35);
            Vector3 pos36 = new Vector3(2786.8354492188f, 9547.8935546875f, 53.645294189453f);
            Vector3 pos37 = new Vector3(3002.0930175781f, 9854.39453125f, -53.198081970215f);
            positions.Add(pos36, pos37);
            Vector3 pos38 = new Vector3(3803.9470214844f, 7197.9018554688f, 53.730079650879f);
            Vector3 pos39 = new Vector3(3664.1088867188f, 7543.572265625f, 54.18229675293f);
            positions.Add(pos38, pos39);
            Vector3 pos40 = new Vector3(2340.0886230469f, 6387.072265625f, 60.165466308594f);
            Vector3 pos41 = new Vector3(2695.6096191406f, 6374.0634765625f, 54.339839935303f);
            positions.Add(pos40, pos41);
            Vector3 pos42 = new Vector3(3249.791015625f, 6446.986328125f, 55.605854034424f);
            Vector3 pos43 = new Vector3(3157.4558105469f, 6791.4458007813f, 54.080295562744f);
            positions.Add(pos42, pos43);
            Vector3 pos44 = new Vector3(3823.6242675781f, 5923.9130859375f, 55.420352935791f);
            Vector3 pos45 = new Vector3(3584.2561035156f, 6215.4931640625f, 55.6123046875f);
            positions.Add(pos44, pos45);
            Vector3 pos46 = new Vector3(5796.4809570313f, 5060.4116210938f, 51.673671722412f);
            Vector3 pos47 = new Vector3(5730.3081054688f, 5430.1635742188f, 54.921173095703f);
            positions.Add(pos46, pos47);
            Vector3 pos48 = new Vector3(6007.3481445313f, 4985.3803710938f, 51.673641204834f);
            Vector3 pos49 = new Vector3(6388.783203125f, 4987f, 51.673400878906f);
            positions.Add(pos48, pos49);
            Vector3 pos50 = new Vector3(7040.9892578125f, 3964.6728515625f, 57.192108154297f);
            Vector3 pos51 = new Vector3(6668.0073242188f, 3993.609375f, 51.671356201172f);
            positions.Add(pos50, pos51);
            Vector3 pos52 = new Vector3(7763.541015625f, 3294.3481445313f, 54.872283935547f);
            Vector3 pos53 = new Vector3(7629.421875f, 3648.0581054688f, 56.908012390137f);
            positions.Add(pos52, pos53);
            Vector3 pos54 = new Vector3(4705.830078125f, 9440.6572265625f, -62.586814880371f);
            Vector3 pos55 = new Vector3(4779.9809570313f, 9809.9091796875f, -63.09009552002f);
            positions.Add(pos54, pos55);
            Vector3 pos56 = new Vector3(4056.7907714844f, 10216.12109375f, -63.152275085449f);
            Vector3 pos57 = new Vector3(3680.1550292969f, 10182.296875f, -63.701038360596f);
            positions.Add(pos56, pos57);
            Vector3 pos58 = new Vector3(4470.0883789063f, 12000.479492188f, 41.59789276123f);
            Vector3 pos59 = new Vector3(4232.9799804688f, 11706.015625f, 49.295585632324f);
            positions.Add(pos58, pos59);
            Vector3 pos60 = new Vector3(5415.5708007813f, 12640.216796875f, 40.682685852051f);
            Vector3 pos61 = new Vector3(5564.4409179688f, 12985.860351563f, 41.373748779297f);
            positions.Add(pos60, pos61);
            Vector3 pos62 = new Vector3(6053.779296875f, 12567.381835938f, 40.587882995605f);
            Vector3 pos63 = new Vector3(6045.4555664063f, 12942.313476563f, 41.211364746094f);
            positions.Add(pos62, pos63);
            Vector3 pos64 = new Vector3(4454.66015625f, 8057.1313476563f, 42.799690246582f);
            Vector3 pos65 = new Vector3(4577.8681640625f, 7699.3686523438f, 53.31339263916f);
            positions.Add(pos64, pos65);
            Vector3 pos66 = new Vector3(7754.7700195313f, 10449.736328125f, 52.890430450439f);
            Vector3 pos67 = new Vector3(8096.2885742188f, 10288.80078125f, 53.66955947876f);
            positions.Add(pos66, pos67);
            Vector3 pos68 = new Vector3(7625.3139648438f, 9465.7001953125f, 55.008113861084f);
            Vector3 pos69 = new Vector3(7995.986328125f, 9398.1982421875f, 53.530490875244f);
            positions.Add(pos68, pos69);
            Vector3 pos70 = new Vector3(9767f, 8839f, 53.044532775879f);
            Vector3 pos71 = new Vector3(9653.1220703125f, 9174.7626953125f, 53.697280883789f);
            positions.Add(pos70, pos71);
            Vector3 pos72 = new Vector3(10775.653320313f, 7612.6943359375f, 55.35241317749f);
            Vector3 pos73 = new Vector3(10665.490234375f, 7956.310546875f, 65.222145080566f);
            positions.Add(pos72, pos73);
            Vector3 pos74 = new Vector3(10398.484375f, 8257.8642578125f, 66.200691223145f);
            Vector3 pos75 = new Vector3(10176.104492188f, 8544.984375f, 64.849853515625f);
            positions.Add(pos74, pos75);
            Vector3 pos76 = new Vector3(11198.071289063f, 8440.4638671875f, 67.641044616699f);
            Vector3 pos77 = new Vector3(11531.436523438f, 8611.0087890625f, 53.454048156738f);
            positions.Add(pos76, pos77);
            Vector3 pos78 = new Vector3(11686.700195313f, 8055.9624023438f, 55.458232879639f);
            Vector3 pos79 = new Vector3(11314.19140625f, 8005.4946289063f, 58.438243865967f);
            positions.Add(pos78, pos79);
            Vector3 pos80 = new Vector3(10707.119140625f, 7335.1752929688f, 55.350387573242f);
            Vector3 pos81 = new Vector3(10693f, 6943f, 54.870254516602f);
            positions.Add(pos80, pos81);
            Vector3 pos82 = new Vector3(10395.380859375f, 6938.5009765625f, 54.869094848633f);
            Vector3 pos83 = new Vector3(10454.955078125f, 7316.7041015625f, 55.308219909668f);
            positions.Add(pos82, pos83);
            Vector3 pos84 = new Vector3(10358.5859375f, 6677.1704101563f, 54.86909866333f);
            Vector3 pos85 = new Vector3(10070.067382813f, 6434.0815429688f, 55.294486999512f);
            positions.Add(pos84, pos85);
            Vector3 pos86 = new Vector3(11161.98828125f, 5070.447265625f, 53.730766296387f);
            Vector3 pos87 = new Vector3(10783f, 4965f, -63.57177734375f);
            positions.Add(pos86, pos87);
            Vector3 pos88 = new Vector3(11167.081054688f, 4613.9829101563f, -62.898971557617f);
            Vector3 pos89 = new Vector3(11501f, 4823f, 54.571090698242f);
            positions.Add(pos88, pos89);
            Vector3 pos90 = new Vector3(11743.823242188f, 4387.4672851563f, 52.005855560303f);
            Vector3 pos91 = new Vector3(11379f, 4239f, -61.565242767334f);
            positions.Add(pos90, pos91);
            Vector3 pos92 = new Vector3(10388.120117188f, 4267.1796875f, -63.61775970459f);
            Vector3 pos93 = new Vector3(10033.036132813f, 4147.1669921875f, -60.332069396973f);
            positions.Add(pos92, pos93);
            Vector3 pos94 = new Vector3(8964.7607421875f, 4214.3833007813f, -63.284225463867f);
            Vector3 pos95 = new Vector3(8569f, 4241f, 55.544258117676f);
            positions.Add(pos94, pos95);
            Vector3 pos96 = new Vector3(5554.8657226563f, 4346.75390625f, 51.680099487305f);
            Vector3 pos97 = new Vector3(5414.0634765625f, 4695.6860351563f, 51.611679077148f);
            positions.Add(pos96, pos97);
            Vector3 pos98 = new Vector3(7311.3393554688f, 10553.6015625f, 54.153884887695f);
            Vector3 pos99 = new Vector3(6938.5209960938f, 10535.8515625f, 54.441242218018f);
            positions.Add(pos98, pos99);
            Vector3 pos100 = new Vector3(7669.353515625f, 5960.5717773438f, -64.488967895508f);
            Vector3 pos101 = new Vector3(7441.2182617188f, 5761.8989257813f, 54.347793579102f);
            positions.Add(pos100, pos101);
            Vector3 pos102 = new Vector3(7949.65625f, 2647.0490722656f, 54.276401519775f);
            Vector3 pos103 = new Vector3(7863.0063476563f, 3013.7814941406f, 55.178623199463f);
            positions.Add(pos102, pos103);
            Vector3 pos104 = new Vector3(8698.263671875f, 3783.1169433594f, 57.178703308105f);
            Vector3 pos105 = new Vector3(9041f, 3975f, -63.242683410645f);
            positions.Add(pos104, pos105);
            Vector3 pos106 = new Vector3(9063f, 3401f, 68.192077636719f);
            Vector3 pos107 = new Vector3(9275.0751953125f, 3712.8935546875f, -63.257461547852f);
            positions.Add(pos106, pos107);
            Vector3 pos108 = new Vector3(12064.340820313f, 6424.11328125f, 54.830627441406f);
            Vector3 pos109 = new Vector3(12267.9375f, 6742.9453125f, 54.83561706543f);
            positions.Add(pos108, pos109);
            Vector3 pos110 = new Vector3(12797.838867188f, 5814.9653320313f, 58.281986236572f);
            Vector3 pos111 = new Vector3(12422.740234375f, 5860.931640625f, 54.815074920654f);
            positions.Add(pos110, pos111);
            Vector3 pos112 = new Vector3(11913.165039063f, 5373.34375f, 54.050819396973f);
            Vector3 pos113 = new Vector3(11569.1953125f, 5211.7143554688f, 57.787326812744f);
            positions.Add(pos112, pos113);
            Vector3 pos114 = new Vector3(9237.3603515625f, 2522.8937988281f, 67.796775817871f);
            Vector3 pos115 = new Vector3(9344.2041015625f, 2884.958984375f, 65.500213623047f);
            positions.Add(pos114, pos115);
            Vector3 pos116 = new Vector3(7324.2783203125f, 1461.2199707031f, 52.594970703125f);
            Vector3 pos117 = new Vector3(7357.3852539063f, 1837.4309082031f, 54.282878875732f);
            positions.Add(pos116, pos117);

        }
        
    }
}
