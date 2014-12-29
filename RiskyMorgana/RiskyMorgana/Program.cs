using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace RiskyMorgana
{
    internal class Program
    {
        private const string Champion = "Morgana";
        public static Orbwalking.Orbwalker orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        private static Obj_AI_Hero Player;

        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != Champion)
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 525);

            Q.SetSkillshot(0.25f, 72f, 1400f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 175f, 1200f, false, SkillshotType.SkillshotCircle);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);

            Menu = new Menu("Risky Morgana!", Champion, true);

            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);
            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            Menu.AddSubMenu(new Menu("Combo Settings", "combo"));
            Menu.SubMenu("combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Menu.SubMenu("combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Menu.SubMenu("combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Wave Clear", "Wave"));
            Menu.SubMenu("Wave").AddItem(new MenuItem("UseQWave", "Use Q")).SetValue(true);
            Menu.SubMenu("Wave").AddItem(new MenuItem("UseWWave", "Use W")).SetValue(true);
            Menu.SubMenu("Wave").AddItem(new MenuItem("ActiveWave", "WaveClear Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            Menu.AddToMainMenu();

            Game.PrintChat("<font color='#FF00BF'>Risky Morgana loaded. Credits:</font> <font color='#FF0000'>Devq,Taerarenai,Braum,Worstping, <3</font><font color='#FFFF00'>");

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            Player = ObjectManager.Player;


            orbwalker.SetAttack(true);
            if (Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Menu.Item("ActiveWave").GetValue<KeyBind>().Active)
            {
                WaveClear();
            }
        }

        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (Q.IsReady())
                castSkillshot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChance.High);

            if (W.IsReady())
                castSkillshot(W, W.Range, TargetSelector.DamageType.Magical, HitChance.High);
           
        }

        private static void WaveClear() //credits too Braum!!!!!!!!!!!!! <3
        {
            var useQ = Menu.Item("UseQWave").GetValue<bool>();
            var useW = Menu.Item("UseWWave").GetValue<bool>();

            List<Obj_AI_Base> rangedMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30, MinionTypes.Ranged);
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30, MinionTypes.All);

            if (useQ && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) && minion.Health > ObjectManager.Player.GetAutoAttackDamage(minion))
                    {
                        Q.Cast(minion);
                    }
                }
            }

            if (useW && W.IsReady())
            {
                MinionManager.FarmLocation rang = W.GetCircularFarmLocation(rangedMinions, W.Width);
                MinionManager.FarmLocation alll = W.GetCircularFarmLocation(allMinions, W.Width);

                if (rang.MinionsHit >= 3)
                {
                    W.Cast(rang.Position);
                }

                else if (alll.MinionsHit >= 2 || allMinions.Count == 1)
                {
                    W.Cast(alll.Position);
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Menu.Item("CircleLag").GetValue<bool>()) 
            {
                if (Menu.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White, Menu.Item("CircleThickness").GetValue<Slider>().Value, Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Menu.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, Color.White, Menu.Item("CircleThickness").GetValue<Slider>().Value, Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Menu.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White, Menu.Item("CircleThickness").GetValue<Slider>().Value, Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Menu.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White, Menu.Item("CircleThickness").GetValue<Slider>().Value, Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Menu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
                }
                if (Menu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Green);
                }
                if (Menu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Red);
                }
                if (Menu.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Purple);
                }
            }
        }

        public static void castSkillshot(Spell spell, float range, LeagueSharp.Common.TargetSelector.DamageType type, HitChance hitChance)
        {
            var target = LeagueSharp.Common.TargetSelector.GetTarget(range, type);
            if (target == null || !spell.IsReady())
                return;
            spell.UpdateSourcePosition();
            if (spell.GetPrediction(target).Hitchance >= hitChance)
                spell.Cast(target, packets());
        }

        public static bool packets()
        {
            return Menu.Item("usePackets").GetValue<bool>();
        }
    }
}