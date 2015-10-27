using System;
using LeagueSharp.Common;
using LeagueSharp;
using 花边_花式多合一.Core;
using System.Drawing;
using System.Linq;

namespace 花边_花式多合一
{
    class InitializeMenu
    {
        public static Menu Menu;

        internal static void Load多合一Menu()
        {
            Menu = new Menu("花边-功能合集", "Credit : NightMoon", true);

            #region 打野选项 : 打野计时 远程打野点 惩戒使用

            Menu.SubMenu("关于打野").SubMenu("打野计时").AddItem(new MenuItem("Timer", "大地图显示").SetValue(true));
            Menu.SubMenu("关于打野").SubMenu("打野计时").AddItem(new MenuItem("Timer1", "小地图显示").SetValue(true));
            Menu.SubMenu("关于打野").SubMenu("打野计时").AddItem(new MenuItem("JungleTimerFormat", "时间格式:").SetValue(new StringList(new[] { "分:秒", "秒" })));
            Menu.SubMenu("关于打野").SubMenu("打野计时").AddItem(new MenuItem("JungleActive", "启动").SetValue(false));

            //Menu.SubMenu("关于打野").SubMenu("远程打野点").AddItem(new MenuItem("wushangdaye", "启动").SetValue(false));

            Menu.SubMenu("关于打野").SubMenu("惩戒使用").AddItem(new MenuItem("AutoSmite", "启动").SetValue(false));
            var mainMenu = Menu.SubMenu("关于打野").SubMenu("惩戒使用").AddSubMenu(new Menu("野怪设置", "野怪设置"));
            {
                if (AutoSmite.IsSummonersRift)
                {
                    mainMenu.AddItem(new MenuItem("ElSmite.Activated", "自动惩戒按键").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle, true)));
                    mainMenu.AddItem(new MenuItem("SRU_Dragon", "小龙").SetValue(true));
                    mainMenu.AddItem(new MenuItem("SRU_Baron", "大龙").SetValue(true));
                    mainMenu.AddItem(new MenuItem("SRU_Red", "红Buff").SetValue(true));
                    mainMenu.AddItem(new MenuItem("SRU_Blue", "蓝buff").SetValue(true));

                    //Bullshit smites
                    mainMenu.AddItem(new MenuItem("SRU_Gromp", "Gromp").SetValue(false));
                    mainMenu.AddItem(new MenuItem("SRU_Murkwolf", "Wolves").SetValue(false));
                    mainMenu.AddItem(new MenuItem("SRU_Krug", "Krug").SetValue(false));
                    mainMenu.AddItem(new MenuItem("SRU_Razorbeak", "Chicken camp").SetValue(false));
                    mainMenu.AddItem(new MenuItem("Sru_Crab", "Crab").SetValue(false));
                }

                if (AutoSmite.IsTwistedTreeline)
                {
                    mainMenu.AddItem(new MenuItem("TT_Spiderboss", "Boss").SetValue(true));
                    mainMenu.AddItem(new MenuItem("TT_NGolem", "石头人").SetValue(true));
                    mainMenu.AddItem(new MenuItem("TT_NWolf", "三狼").SetValue(true));
                    mainMenu.AddItem(new MenuItem("TT_NWraith", "幽灵").SetValue(true));
                }
            }
            Menu.SubMenu("关于打野").SubMenu("惩戒使用").AddItem(new MenuItem("Credit", "这个自动惩戒的作者"));
            Menu.SubMenu("关于打野").SubMenu("惩戒使用").AddItem(new MenuItem("Credit1", "是jQuery还有Asuna"));
            Menu.SubMenu("关于打野").SubMenu("惩戒使用").AddItem(new MenuItem("Credit2", "这是L#最好的惩戒"));
            Menu.SubMenu("关于打野").SubMenu("惩戒使用").AddItem(new MenuItem("Credit3", "没有之一"));
            Menu.SubMenu("关于打野").SubMenu("惩戒使用").AddItem(new MenuItem("Credit4", "在此表示敬意!"));

            //Menu.SubMenu("关于打野").SubMenu("打野监控").AddItem(new MenuItem("JungleslackEnable", "启动").SetValue(false));

            Menu.SubMenu("关于打野").SubMenu("打野监控").AddItem(new MenuItem("JungleTrackerEnable", "启动").SetValue(false));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("本地信号提示").AddItem(new MenuItem("pingdragon", "小龙被攻击").SetValue(true));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("本地信号提示").AddItem(new MenuItem("pingbaron", "大龙被攻击").SetValue(true));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("本地信号提示").AddItem(new MenuItem("pingsmall", "小怪被攻击").SetValue(true));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("本地信号提示").AddItem(new MenuItem("pingfow", "仅无视野下提示").SetValue(true));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("本地信号提示").AddItem(new MenuItem("pingscreen", "仅野怪不在屏幕显示范围内").SetValue(true));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("本地信号提示").AddItem(new MenuItem("pingdelay", "本地信号提示延迟").SetValue(new Slider(10, 1, 20)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("显示设置").AddItem(new MenuItem("colortracked", "野怪存活-监控颜色").SetValue(Color.FromArgb(255, 0, 255, 0)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("显示设置").AddItem(new MenuItem("colorguessed", "野怪存活-猜测颜色").SetValue(Color.FromArgb(255, 0, 255, 255)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("显示设置").AddItem(new MenuItem("colorattacking", "野怪正被攻击颜色").SetValue(Color.FromArgb(255, 255, 0, 0)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("显示设置").AddItem(new MenuItem("colordisengaged", "野怪失去仇恨颜色").SetValue(Color.FromArgb(255, 255, 210, 0)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("显示设置").AddItem(new MenuItem("colordead", "野怪已死亡颜色").SetValue(Color.FromArgb(255, 200, 200, 200)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("显示设置").AddItem(new MenuItem("circleradius", "线圈半径").SetValue(new Slider(300, 1, 500)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").SubMenu("显示设置").AddItem(new MenuItem("circlewidth", "线圈宽度").SetValue(new Slider(1, 1, 4)));
            Menu.SubMenu("关于打野").SubMenu("打野监控").AddItem(new MenuItem("updatetick", "刷新时间").SetValue(new Slider(150, 0, 1000)));

            #endregion

            #region 防御塔选项 : 塔血量显示 攻击范围

            Menu.SubMenu("关于防御塔").SubMenu("塔血量显示").AddItem(new MenuItem("TIHealth", "显示方式").SetValue(new StringList(new[] { "百分比", "数字" })));
            Menu.SubMenu("关于防御塔").SubMenu("塔血量显示").AddItem(new MenuItem("HealthActive", "启动").SetValue(false));

            Menu.SubMenu("关于防御塔").SubMenu("塔攻击范围").AddItem(new MenuItem("RangeEnabled", "启动").SetValue(false));

            #endregion

            #region 眼位技能 : 眼位监控 技能监控 进草插眼 反隐监控-显示敌人位置 (敌人消失警示 潜行提示)

            Menu.SubMenu("眼位技能").SubMenu("眼位监控").AddItem(new MenuItem("TrackEnemyWards", "启动").SetValue(false));

            Menu.SubMenu("眼位技能").SubMenu("技能监控").AddItem(new MenuItem("TrackEnemySpells", "启动").SetValue(false));
            Menu.SubMenu("眼位技能").SubMenu("技能监控").AddItem(new MenuItem("TrackEnemyCooldown", "检测敌人CD").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("技能监控").AddItem(new MenuItem("TrackAllyCooldown", "检测友军CD").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("技能监控").AddItem(new MenuItem("TrackNoMana", "检测蓝量耗损").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("技能监控").AddItem(new MenuItem("TrackEnemyRecalls", "检测回城").SetValue(true));

            Menu.SubMenu("眼位技能").SubMenu("进草插眼").AddItem(new MenuItem("ward", "敌人进草自动插眼").SetValue(false));
            Menu.SubMenu("眼位技能").SubMenu("进草插眼").AddItem(new MenuItem("wardC", "仅连招使用").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("进草插眼").AddItem(new MenuItem("Combo", "连招按键").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.SubMenu("眼位技能").SubMenu("反隐监测").AddItem(new MenuItem("Fanyin", "启用").SetValue(false));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").AddItem(new MenuItem("USEVISIONWARD", "使用真眼").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").AddItem(new MenuItem("USEORACLESLENS", "使用升级过的扫描").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").AddItem(new MenuItem("USELIGHTBRINGER", "使用灯泡").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").AddItem(new MenuItem("USEHEXTECHSWEEPER", "使用升级过的灯泡").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").AddItem(new MenuItem("USESNOWBALL", "使用雪球").SetValue(true));

            Menu.SubMenu("眼位技能").SubMenu("反隐监测").SubMenu("敌人位置").AddItem(new MenuItem("DRAWCIRCLE", "显示敌人去处").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").SubMenu("敌人位置").AddItem(new MenuItem("PINGMODE", "提示敌人Miss方式").SetValue(new StringList(new string[] { "仅自己可见", "发送队伍", "禁止" }, 0)));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").SubMenu("敌人位置").AddItem(new MenuItem("PINGCOUNT", "打提示时间").SetValue(new Slider(1, 1, 100)));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").SubMenu("敌人位置").AddItem(new MenuItem("DRAWWAPOINTS", "显示敌人最后位置").SetValue(true));
            Menu.SubMenu("眼位技能").SubMenu("反隐监测").SubMenu("敌人位置").AddItem(new MenuItem("ENABLEWDHG", "启用").SetValue(false));

            var spells = WhereDidHeGo.AntiStealthSpells.Where(p => p.ChampionName == ObjectManager.Player.ChampionName.ToLower());
            if (spells != null)
            {
                Menu antiStealthSpells = Menu.SubMenu("眼位技能").SubMenu("反隐监测").AddSubMenu(new Menu("显示技能", "wdhgrevealspells"));
                foreach (var spell in spells)
                {
                    Menu antiStealthSpell = new Menu(String.Format("{0} ({1})", ObjectManager.Player.Spellbook.GetSpell(spell.Spell).Name, spell.Spell), "wdhg" + spell.Spell.ToString());
                    antiStealthSpell.AddItem(new MenuItem(String.Format("DETECT{0}", spell.Spell.ToString()), "危险等级").SetValue(new Slider(spell.StealthDetectionLevel, 1, 3)));
                    antiStealthSpell.AddItem(new MenuItem(String.Format("USE{0}", spell.Spell.ToString()), "启用").SetValue(true));
                    antiStealthSpells.AddSubMenu(antiStealthSpell);
                }
            }

            #endregion

            #region 自动使用 : 自动击杀 自动灯笼 自动中亚 自动大天使 自动转身躲技能 活化剂

            Menu.SubMenu("自动功能").SubMenu("活化剂").AddItem(new MenuItem("Autoactivator", "启动").SetValue(false));
            Menu.SubMenu("自动功能").SubMenu("活化剂").AddItem(new MenuItem("nabbactivator.menu.potions.on_health_percent", "自动补血-最低百分比").SetValue(new Slider(50, 0, 100)));
            Menu.SubMenu("自动功能").SubMenu("活化剂").AddItem(new MenuItem("nabbactivator.menu.potions.on_mana_percent", "自动补蓝-最低百分比").SetValue(new Slider(50, 0, 100)));
            Menu.SubMenu("自动功能").SubMenu("活化剂").AddItem(new MenuItem("nabbactivator.menu.combo_button", "连招按键").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.SubMenu("自动功能").SubMenu("自动击杀").AddItem(new MenuItem("AutoKs", "启动").SetValue(false));
            Menu.SubMenu("自动功能").SubMenu("自动击杀").AddItem(new MenuItem("Ignite", "点燃击杀").SetValue(true));
            Menu.SubMenu("自动功能").SubMenu("自动击杀").AddItem(new MenuItem("Smite", "惩戒击杀").SetValue(true));
            Menu.SubMenu("自动功能").SubMenu("自动击杀").AddItem(new MenuItem("AA", "普攻击杀").SetValue(true));

            Menu.SubMenu("自动功能").SubMenu("自动灯笼").AddItem(new MenuItem("AutoLanter", "启动").SetValue(false));
            Menu.SubMenu("自动功能").SubMenu("自动灯笼").AddItem(new MenuItem("AutoLantern", "低血量自动捡灯笼").SetValue(true));
            Menu.SubMenu("自动功能").SubMenu("自动灯笼").AddItem(new MenuItem("LowLantern", "低血量标准").SetValue(new Slider(20, 10, 50)));
            Menu.SubMenu("自动功能").SubMenu("自动灯笼").AddItem(new MenuItem("LanternHotkey", "捡灯笼按键").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.SubMenu("自动功能").SubMenu("自动灯笼").AddItem(new MenuItem("LanternReady", "准备灯笼").SetValue(false));
            Menu.SubMenu("自动功能").SubMenu("自动灯笼").AddItem(new MenuItem("PermaShowLantern", "水印显示").SetValue(false));

            Menu.SubMenu("自动功能").SubMenu("自动转身").AddItem(new MenuItem("AutoTurnAround", "启动").SetValue(false));
            Menu.SubMenu("自动功能").SubMenu("自动转身").AddSubMenu(new Menu("躲避技能", "躲避技能"));
            foreach (var champ in TurnAround.ExistingChampions)
            {
                Menu.SubMenu("自动功能").SubMenu("自动转身").SubMenu("躲避技能")
                    .AddSubMenu(new Menu(champ.CharName + " 躲避!", champ.CharName));
                Menu.SubMenu("自动功能").SubMenu("自动转身").SubMenu("躲避技能")
                    .SubMenu(champ.CharName)
                    .AddItem(new MenuItem(champ.Slot.ToString(), champ.SpellName).SetValue(true));
            }

            Menu.SubMenu("自动功能").SubMenu("自动中亚").AddItem(new MenuItem("AutoZhongya", "启动").SetValue(false));
            var spellmenu = Menu.SubMenu("自动功能").SubMenu("自动中亚").AddSubMenu(new Menu("技能设置", "技能设置"));
            var miscmenu = Menu.SubMenu("自动功能").SubMenu("自动中亚").AddSubMenu(new Menu("杂项设置", "杂项设置"));
            miscmenu.AddItem(new MenuItem("enablehpzhonya", "低血量自动中亚").SetValue(true));
            miscmenu.AddItem(new MenuItem("enableseraph", "使用 炽天使之拥").SetValue(true));
            miscmenu.AddItem(new MenuItem("hptozhonya", "使用中亚丨低血量指标")).SetValue(new Slider(25, 0, 100));
            miscmenu.AddItem(new MenuItem("minspelldmg", "受到技能伤害 % (非危险状态)")).SetValue(new Slider(45, 0, 100));
            miscmenu.AddItem(new MenuItem("remaininghealth", "极限血量指标 %")).SetValue(new Slider(15, 0, 100));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    foreach (var spell in AutoZhongya.DangerousSpells.AvoidableSpells)
                    {
                        if (spell.Source.ToLower() == hero.ChampionName.ToLower())
                        {
                            var subMenu = Menu.SubMenu("自动功能").SubMenu("自动中亚").AddSubMenu(new Menu(spell.DisplayName, spell.DisplayName));

                            subMenu.AddItem(new MenuItem("Enabled" + spell.DisplayName, "启用!").SetValue(true));
                            //   subMenu.AddItem(new MenuItem("spelldelay", "Spell Delay")).SetValue(new Slider(0, 0, 2000));

                            spellmenu.AddSubMenu(subMenu);
                        }
                    }
                }

            }

            #region  自动加点

            Menu.SubMenu("自动功能").SubMenu("自动加点").AddItem(new MenuItem("AutoLevelsEnable", "启动").SetValue(false));
            foreach (var entry in AutoLevels.DefaultSpellSlotPriorities)
            {
                MenuItem menuItem = AutoLevels.MakeSlider(
                    entry.Key.ToString(), entry.Key.ToString(), entry.Value, 1, AutoLevels.DefaultSpellSlotPriorities.Count);
                menuItem.ValueChanged += AutoLevels.menuItem_ValueChanged;
                Menu.SubMenu("自动功能").SubMenu("自动加点").AddItem(menuItem);

                var subMenu = new Menu(entry.Key + " 额外设置", entry.Key + "extra");
                subMenu.AddItem(AutoLevels.MakeSlider(entry.Key + "extra", "等级达到多少内启动?", 1, 1, 18));
                Menu.SubMenu("自动功能").SubMenu("自动加点").AddSubMenu(subMenu);
            }
            AutoLevels._activate = Menu.SubMenu("自动功能").SubMenu("自动加点").AddItem(new MenuItem("activate", "几级后开始自动加点?").SetValue(new StringList(new[] { "2", "3", "4" })));
            AutoLevels._delay = Menu.SubMenu("自动功能").SubMenu("自动加点").AddItem(new MenuItem("AutoLevelsdelay", "加点延迟 (ms)").SetValue(new Slider(0, 0, 2000)));

            #endregion
            #endregion

            #region 其他功能 :  检测同行 人性化设置 时间显示 送人头 杀人自动屏蔽线圈 (未知是否工作) 经验分流

            #region 人性化设置 (技能 移动 人性化)

            /* Menu.SubMenu("其他功能").SubMenu("人性化设置").AddItem(new MenuItem("Humanizer", "启动").SetValue(false));
             for (var i = 0; i <= 3; i++)
             {
                 var spell = Core.Humanizer.SpellList[i];
                 var spellsmenu = Menu.SubMenu("其他功能").SubMenu("人性化设置").AddSubMenu(new Menu("技能:" + spell, spell));
                 spellsmenu.AddItem(new MenuItem("Enabled" + i, "技能延迟 " + spell, true).SetValue(true));
                 spellsmenu.AddItem(new MenuItem("MinDelay" + i, "最小延迟", true).SetValue(new Slider(80)));
                 spellsmenu.AddItem(new MenuItem("MaxDelay" + i, "最大延迟", true).SetValue(new Slider(200, 100, 400)));
             }
             var move = Menu.SubMenu("其他功能").SubMenu("人性化设置").AddSubMenu(new Menu("移动设置", "Movement"));
             move.AddItem(new MenuItem("MovementEnabled", "启动").SetValue(true));
             move.AddItem(new MenuItem("MinDelay", "最小延迟")).SetValue(new Slider(80));
             move.AddItem(new MenuItem("MaxDelay", "最大延迟")).SetValue(new Slider(200, 100, 400));*/

            #endregion

            #region 时间显示 (现实时间显示)

            Menu.SubMenu("其他功能").SubMenu("时间显示").AddItem(new MenuItem("TimeEnable", "启动").SetValue(true));
            Menu.SubMenu("其他功能").SubMenu("时间显示").AddItem(new MenuItem("atTop", "距离顶部").SetValue(new Slider(0, 0, 500)));
            Menu.SubMenu("其他功能").SubMenu("时间显示").AddItem(new MenuItem("atRight", "距离右边").SetValue(new Slider(1004, 0, 1500)));

            #endregion

            #region 检测同行 (检测Bol 和 L#)

            Menu.SubMenu("其他功能").SubMenu("检测同行").AddItem(new MenuItem("CheckEnable", "启动").SetValue(false));
             var detectionType = Menu.SubMenu("其他功能").SubMenu("检测外挂").AddItem(new MenuItem("detection", "检测").SetValue(new StringList(new[] { "Preferred", "Safe", "AntiHumanizer" })));
             detectionType.ValueChanged += (sender, args) =>
             {
                 foreach (var detector in CheckMoreL._detectors)
                 {
                     detector.Value.ForEach(item => item.ApplySetting((Interface.DetectorSetting)detectionType.GetValue<StringList>().SelectedIndex));
                 }
             };
             Menu.SubMenu("其他功能").SubMenu("检测同行").AddItem(new MenuItem("1", "提示:"));
             Menu.SubMenu("其他功能").SubMenu("检测同行").AddItem(new MenuItem("2", "一旦关闭右上角的提示，就只"));
             Menu.SubMenu("其他功能").SubMenu("检测同行").AddItem(new MenuItem("3", "有重新载入脚本才会再次显示"));

            #endregion

            #region 显示最佳AA后摇

            Menu.SubMenu("其他功能").SubMenu("显示最佳AA后摇").AddItem(new MenuItem("WindUpEnable", "启动").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("显示最佳AA后摇").AddItem(new MenuItem("Sayy", "在走砍的的AA后摇自己手动设置"));

            #endregion

            #region 一键摸眼(支持卡特 瞎子 武器)

            Menu.SubMenu("其他功能").SubMenu("一键摸眼").AddItem(new MenuItem("WardJumpEnable", "启动").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("一键摸眼").AddItem(new MenuItem("DrawingsWardJump", "显示摸眼范围").SetValue(true));
            Menu.SubMenu("其他功能").SubMenu("一键摸眼").AddItem(new MenuItem("wardjumpKey", "摸眼按键").SetValue(new KeyBind('Z', KeyBindType.Press)));
            Menu.SubMenu("其他功能").SubMenu("一键摸眼").AddItem(new MenuItem("Say1", "仅支持瞎子 卡特 武器"));

            #endregion

            #region 送人头

            Menu.SubMenu("其他功能").SubMenu("送人头").AddItem(new MenuItem("SongrentoupEnable", "启动").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("送人头").AddItem(new MenuItem("Feeding.Activated", "花式送人头").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("送人头").AddItem(new MenuItem("Feeding.FeedMode", "送头地点:").SetValue(new StringList(new[] { "中路", "下路", "上路", "随机" })));
            var feedingMenu = Menu.SubMenu("其他功能").SubMenu("送人头").AddSubMenu(new Menu("送头设置", "FeedingMenu"));
            {
                feedingMenu.AddItem(new MenuItem("Spells.Activated", "使用加速技能去送").SetValue(true));
                feedingMenu.AddItem(new MenuItem("Items.Activated", "使用加速物品去送").SetValue(true));
                feedingMenu.AddItem(new MenuItem("Attacks.Disabled", "送人头途中不A人").SetValue(true));
                feedingMenu.AddItem(new MenuItem("Surrender.Activated", "自动20投").SetValue(true));
                feedingMenu.AddItem(new MenuItem("Surrender.Activated.Say", "结束自动叫对面给赞").SetValue(true));

            }

            #endregion

            #region 杀人自动屏蔽线圈 (三杀 四杀 五杀 超神 自动屏蔽线圈 防止截图截到)

            Menu.SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("AutoDisableDrawingEnable", "启动").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("死亡屏蔽显示", "死亡屏蔽显示").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("已连杀人数", "已连杀人数(请勿随意更改)").SetValue(new Slider(0, 0, 8)));
            Menu.SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("超神屏蔽显示", "超神屏蔽显示").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("多杀屏蔽显示", "多杀屏蔽显示").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("多杀屏蔽时间", "多杀屏蔽时间").SetValue(new Slider(4, 0, 10)));
            Menu.SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("未知是否工作", "未知是否工作!"));

            #endregion

            #region 经验分流

            Menu.SubMenu("其他功能").SubMenu("经验分流").AddItem(new MenuItem("SharedExperienceEnable", "启动").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("经验分流").AddItem(new MenuItem("SharedExperienceshowEnemies", "文本显示敌人").SetValue(true));
            Menu.SubMenu("其他功能").SubMenu("经验分流").AddItem(new MenuItem("SharedExperienceonlyShowInv", "仅文本显示非视野的敌人").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("经验分流").AddItem(new MenuItem("SharedExperiencedrawPredictionCircle", "显示预计线圈").SetValue(true));
            Menu.SubMenu("其他功能").SubMenu("经验分流").AddItem(new MenuItem("SharedExperienceinvColor", "仅文本显示非视野的敌人颜色").SetValue(Color.FromArgb(255, 245, 25, 25)));
            Menu.SubMenu("其他功能").SubMenu("经验分流").AddItem(new MenuItem("SharedExperiencepositionX", "文本横坐标").SetValue(new Slider(142, -100, 200)));
            Menu.SubMenu("其他功能").SubMenu("经验分流").AddItem(new MenuItem("SharedExperiencepositionY", "文本纵坐标").SetValue(new Slider(21, -100, 100)));

            Menu.SubMenu("其他功能").SubMenu("经验分流").AddSubMenu(new Menu("英雄列表", "英雄列表"));
            Menu.SubMenu("其他功能").SubMenu("经验分流").SubMenu("英雄列表").AddItem(new MenuItem("SharedExperiencedrawchampionlist", "显示英雄列表").SetValue(false));
            Menu.SubMenu("其他功能").SubMenu("经验分流").SubMenu("英雄列表").AddItem(new MenuItem("SharedExperienceposX", "列表横坐标").SetValue(new Slider(Drawing.Width / 2, 0, Drawing.Width)));
            Menu.SubMenu("其他功能").SubMenu("经验分流").SubMenu("英雄列表").AddItem(new MenuItem("SharedExperienceposY", "列表纵坐标").SetValue(new Slider(Drawing.Height / 2, 0, Drawing.Height)));

            #endregion

            #endregion

            #region 开发功能 : 无限视距 屏蔽线圈 禁止脚本发言 靠近显示防御塔范围

            Menu.SubMenu("开发功能").AddItem(new MenuItem("Explore", "启动").SetValue(false));
            Menu.SubMenu("开发功能").AddItem(new MenuItem("Disable Drawing", "屏蔽线圈开关 默认按键:Home").SetValue(new KeyBind(36, KeyBindType.Toggle)));
            Menu.SubMenu("开发功能").AddItem(new MenuItem("disable say", "禁止脚本发所有人").SetValue(false));
            Menu.SubMenu("开发功能").AddItem(new MenuItem("SaySomething", "刷屏阻止脚本载入信息").SetValue(false));
            //Menu.SubMenu("开发功能").AddItem(new MenuItem("zoom hack", "无限视距").SetValue(false));
            //Menu.SubMenu("开发功能").AddItem(new MenuItem("Tower Ranges", "显示防御塔范围(靠近)").SetValue(false));
            Menu.Item("Disable Drawing").ValueChanged += Explore.Flowers_ValueChanged;

            #endregion

            #region 信息

            #region 多合一功能介绍

            /*            Menu.AddSubMenu(new Menu("功能介绍", "功能介绍"));
                        Menu.SubMenu("功能介绍").AddSubMenu(new Menu("打野选项", "打野选项"));
                        Menu.SubMenu("功能介绍").SubMenu("打野选项").AddItem(new MenuItem("打野计时", "打野计时"));
                        Menu.SubMenu("功能介绍").SubMenu("打野选项").AddItem(new MenuItem("远程打野点", "远程打野点"));
                        Menu.SubMenu("功能介绍").SubMenu("打野选项").AddItem(new MenuItem("惩戒使用", "惩戒使用"));
                        Menu.SubMenu("功能介绍").AddSubMenu(new Menu("防御塔选项", "防御塔选项"));
                        Menu.SubMenu("功能介绍").SubMenu("防御塔选项").AddItem(new MenuItem("塔血量显示", "塔血量显示"));
                        Menu.SubMenu("功能介绍").SubMenu("防御塔选项").AddItem(new MenuItem("攻击范围", "攻击范围"));
                        Menu.SubMenu("功能介绍").AddSubMenu(new Menu("眼位技能", "眼位技能"));
                        Menu.SubMenu("功能介绍").SubMenu("眼位技能").AddItem(new MenuItem("眼位监控", "眼位监控"));
                        Menu.SubMenu("功能介绍").SubMenu("眼位技能").AddItem(new MenuItem("技能监控", "技能监控"));
                        Menu.SubMenu("功能介绍").SubMenu("眼位技能").AddItem(new MenuItem("进草插眼", "进草插眼"));
                        Menu.SubMenu("功能介绍").SubMenu("眼位技能").AddItem(new MenuItem("反隐监控-显示敌人位置", "反隐监控 - 显示敌人位置(敌人消失警示 潜行提示)"));
                        Menu.SubMenu("功能介绍").AddSubMenu(new Menu("自动使用", "自动使用"));
                        Menu.SubMenu("功能介绍").SubMenu("自动使用").AddItem(new MenuItem("自动击杀", "自动击杀"));
                        Menu.SubMenu("功能介绍").SubMenu("自动使用").AddItem(new MenuItem("自动灯笼", "自动灯笼"));
                        Menu.SubMenu("功能介绍").SubMenu("自动使用").AddItem(new MenuItem("自动中亚", "自动中亚"));
                        Menu.SubMenu("功能介绍").SubMenu("自动使用").AddItem(new MenuItem("自动大天使", "自动大天使"));
                        Menu.SubMenu("功能介绍").SubMenu("自动使用").AddItem(new MenuItem("自动转身躲技能", "自动转身躲技能"));
                        Menu.SubMenu("功能介绍").AddSubMenu(new Menu("其他功能", "其他功能"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").AddItem(new MenuItem("自动加点", "自动加点"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").AddItem(new MenuItem("检测同行", "检测同行 (检测Bol 和 L#)"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").AddItem(new MenuItem("人性化设置", "人性化设置 (技能 移动 人性化)"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").AddItem(new MenuItem("时间显示", "时间显示 (现实时间显示)"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").AddItem(new MenuItem("送人头", "送人头(自动去送)"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").AddItem(new MenuItem("送人头", "送人头(自动去送)"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").AddSubMenu(new Menu("杀人自动屏蔽线圈", "杀人自动屏蔽线圈"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("三杀 四杀 五杀 超神", "三杀 四杀 五杀 超神"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("自动屏蔽线圈", "自动屏蔽线圈防止截图截到"));
                        Menu.SubMenu("功能介绍").SubMenu("其他功能").SubMenu("杀人自动屏蔽线圈").AddItem(new MenuItem("未知是否工作", "未知是否工作"));
                        Menu.SubMenu("功能介绍").AddSubMenu(new Menu("开发功能", "开发功能"));
                        Menu.SubMenu("功能介绍").SubMenu("开发功能").AddItem(new MenuItem("无限视距", "无限视距(仅外服使用)"));
                        Menu.SubMenu("功能介绍").SubMenu("开发功能").AddItem(new MenuItem("屏蔽线圈", "屏蔽线圈 (默认按键:Home)"));
                        Menu.SubMenu("功能介绍").SubMenu("开发功能").AddItem(new MenuItem("禁止脚本发言", "禁止脚本发言"));
                        Menu.SubMenu("功能介绍").SubMenu("开发功能").AddItem(new MenuItem("靠近显示防御塔范围", "靠近显示防御塔范围"));*/

            #endregion

            Menu.AddItem(new MenuItem("注意", "注意::::::::::"));
            Menu.AddItem(new MenuItem("第一次使用多合一", "第一次使用多合一请手动打开你要使用的功能"));
            Menu.AddItem(new MenuItem("默认全部关闭", "默认全部关闭"));

            Menu.AddItem(new MenuItem("NightMoon", "作者 : 花边下丶情未央"));
            //Menu.AddItem(new MenuItem("Qingyi", "作者 : 晴依"));

            Menu.AddItem(new MenuItem("Lost", "脚本加载自动喊话").SetValue(true));

            Menu.AddItem(new MenuItem("NightMoon1", "  "));

            Menu.AddItem(new MenuItem("zhuming", "狗儿子可以改名字说是你的"));
            Menu.AddItem(new MenuItem("zhuming1", "反正不是某些人喜欢改名字吗"));
            Menu.AddItem(new MenuItem("zhuming2", "然后说是自用的玩意 自己写的"));
            Menu.AddItem(new MenuItem("zhuming3", "有本事自己写出来 装你麻痹"));
            Menu.AddItem(new MenuItem("zhuming4", "废物就是废物 不会写就只会偷"));
            Menu.AddItem(new MenuItem("zhuming5", "准备做下个所谓的脚本防封吗?"));

            #endregion 

            Menu.AddToMainMenu();
        }
    }   
}
