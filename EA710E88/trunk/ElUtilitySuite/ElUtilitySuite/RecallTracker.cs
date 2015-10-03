namespace ElUtilitySuite
{
    //Recall tracker from BaseUlt

    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;
    using SharpDX.Direct3D9;

    using Color = System.Drawing.Color;
    using Font = SharpDX.Direct3D9.Font;

    #endregion

    internal class RecallTracker
    {
        #region Static Fields

        private static readonly float BarX = Drawing.Width * 0.425f;

        private static readonly int BarWidth = (int)(Drawing.Width - 2 * BarX);

        private static readonly float Scale = (float)BarWidth / 8000;

        #endregion

        #region Fields

        public List<EnemyInfo> EnemyInfo = new List<EnemyInfo>();

        private readonly int BarHeight = 10; //6

        private readonly float BarY = Drawing.Height * 0.80f;

        private readonly List<Obj_AI_Hero> enemies;

        private readonly List<Obj_AI_Hero> Heroes;

        private readonly int SeperatorHeight = 5;

        private readonly Font Text;

        private Utility.Map.MapType Map;

        #endregion

        #region Constructors and Destructors

        public RecallTracker()
        {
            this.Heroes = ObjectManager.Get<Obj_AI_Hero>().ToList();
            this.enemies = this.Heroes.Where(x => x.IsEnemy).ToList();

            this.EnemyInfo = this.enemies.Select(x => new EnemyInfo(x)).ToList();
            this.Map = Utility.Map.GetMap().Type;

            this.Text = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                    {
                        FaceName = "Calibri", Height = 13, Width = 6, OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });

            Obj_AI_Base.OnTeleport += this.Obj_AI_Base_OnTeleport;
            Drawing.OnPreReset += this.Drawing_OnPreReset;
            Drawing.OnPostReset += this.Drawing_OnPostReset;
            Drawing.OnDraw += this.Drawing_OnDraw;
            AppDomain.CurrentDomain.DomainUnload += this.CurrentDomainDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += this.CurrentDomainDomainUnload;
        }

        #endregion

        #region Methods

        private void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            this.Text.Dispose();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!InitializeMenu.Menu.Item("showRecalls").GetValue<bool>() || Drawing.Direct3DDevice == null)
            {
                return;
            }

            var indicated = false;

            var fadeout = 1f;
            var count = 0;

            foreach (var enemyInfo in
                this.EnemyInfo.Where(
                    x =>
                    x.Player.IsValid<Obj_AI_Hero>() && x.RecallInfo.ShouldDraw() && !x.Player.IsDead
                    && x.RecallInfo.GetRecallCountdown() > 0).OrderBy(x => x.RecallInfo.GetRecallCountdown()))
            {
                if (!enemyInfo.RecallInfo.LockedTarget)
                {
                    fadeout = 1f;
                    var color = Color.White;

                    if (enemyInfo.RecallInfo.WasAborted())
                    {
                        fadeout = enemyInfo.RecallInfo.GetDrawTime() / (float)enemyInfo.RecallInfo.FADEOUT_TIME;
                        color = Color.Yellow;
                    }

                    this.DrawRect(
                        BarX,
                        this.BarY,
                        (int)(Scale * enemyInfo.RecallInfo.GetRecallCountdown()),
                        this.BarHeight,
                        1,
                        Color.FromArgb(255, Color.Green));
                    this.DrawRect(
                        BarX + Scale * enemyInfo.RecallInfo.GetRecallCountdown() - 1,
                        this.BarY - this.SeperatorHeight,
                        0,
                        this.SeperatorHeight + 1,
                        1,
                        Color.FromArgb(0, Color.Green));

                    this.Text.DrawText(
                        null,
                        enemyInfo.Player.ChampionName,
                        (int)BarX
                        + (int)
                          (Scale * enemyInfo.RecallInfo.GetRecallCountdown()
                           - (float)(enemyInfo.Player.ChampionName.Length * this.Text.Description.Width) / 2),
                        (int)this.BarY - this.SeperatorHeight - this.Text.Description.Height - 1,
                        new ColorBGRA(color.R, color.G, color.B, (byte)(color.A * fadeout)));
                }
                else
                {
                    if (!indicated && (int)enemyInfo.RecallInfo.EstimatedShootT != 0)
                    {
                        indicated = true;
                        this.DrawRect(
                            BarX + Scale * enemyInfo.RecallInfo.EstimatedShootT,
                            this.BarY + this.SeperatorHeight + this.BarHeight - 3,
                            0,
                            this.SeperatorHeight * 2,
                            2,
                            Color.Orange);
                    }

                    this.DrawRect(
                        BarX,
                        this.BarY,
                        (int)(Scale * enemyInfo.RecallInfo.GetRecallCountdown()),
                        this.BarHeight,
                        1,
                        Color.FromArgb(255, Color.Red));
                    this.DrawRect(
                        BarX + Scale * enemyInfo.RecallInfo.GetRecallCountdown() - 1,
                        this.BarY + this.SeperatorHeight + this.BarHeight - 3,
                        0,
                        this.SeperatorHeight + 1,
                        1,
                        Color.IndianRed);

                    this.Text.DrawText(
                        null,
                        enemyInfo.Player.ChampionName,
                        (int)BarX
                        + (int)
                          (Scale * enemyInfo.RecallInfo.GetRecallCountdown()
                           - (float)(enemyInfo.Player.ChampionName.Length * this.Text.Description.Width) / 2),
                        (int)this.BarY + this.SeperatorHeight + this.Text.Description.Height / 2,
                        new ColorBGRA(255, 92, 92, 255));
                }

                count++;
            }

            if (count > 0)
            {
                if (count != 1)
                {
                    fadeout = 1f;
                }

                this.DrawRect(
                    BarX,
                    this.BarY,
                    BarWidth,
                    this.BarHeight,
                    1,
                    Color.FromArgb((int)(40f * fadeout), Color.White));

                this.DrawRect(
                    BarX - 1,
                    this.BarY + 1,
                    0,
                    this.BarHeight,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
                this.DrawRect(
                    BarX - 1,
                    this.BarY - 1,
                    BarWidth + 2,
                    1,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
                this.DrawRect(
                    BarX - 1,
                    this.BarY + this.BarHeight,
                    BarWidth + 2,
                    1,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
                this.DrawRect(
                    BarX + 1 + BarWidth,
                    this.BarY + 1,
                    0,
                    this.BarHeight,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
            }
        }

        private void Drawing_OnPostReset(EventArgs args)
        {
            this.Text.OnResetDevice();
        }

        private void Drawing_OnPreReset(EventArgs args)
        {
            this.Text.OnLostDevice();
        }

        private void DrawRect(float x, float y, int width, int height, float thickness, Color color)
        {
            for (var i = 0; i < height; i++)
            {
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
            }
        }

        private void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || unit.IsAlly)
            {
                return;
            }

            var recall = Packet.S2C.Teleport.Decoded(unit, args);
            var enemyInfo =
                this.EnemyInfo.Find(x => x.Player.NetworkId == recall.UnitNetworkId).RecallInfo.UpdateRecall(recall);

            if (recall.Type == Packet.S2C.Teleport.Type.Recall)
            {
                switch (recall.Status)
                {
                    case Packet.S2C.Teleport.Status.Abort:
                        if (InitializeMenu.Menu.Item("notifRecAborted").GetValue<bool>())
                        {
                            this.ShowNotification(
                                enemyInfo.Player.ChampionName + ": Recall ABORTED",
                                Color.Orange,
                                4000);
                        }

                        break;
                    case Packet.S2C.Teleport.Status.Finish:
                        if (InitializeMenu.Menu.Item("notifRecFinished").GetValue<bool>())
                        {
                            this.ShowNotification(
                                enemyInfo.Player.ChampionName + ": Recall FINISHED",
                                Color.White,
                                4000);
                        }

                        break;
                }
            }
        }

        private void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion
    }

    internal class EnemyInfo
    {
        #region Fields

        public int LastSeen;

        public Obj_AI_Hero Player;

        public RecallInfo RecallInfo;

        #endregion

        #region Constructors and Destructors

        public EnemyInfo(Obj_AI_Hero player)
        {
            this.Player = player;
            this.RecallInfo = new RecallInfo(this);
        }

        #endregion
    }

    internal class RecallInfo
    {
        #region Fields

        public float EstimatedShootT;

        public int FADEOUT_TIME = 3000;

        public bool LockedTarget;

        private readonly EnemyInfo EnemyInfo;

        private Packet.S2C.Teleport.Struct AbortedRecall;

        private int AbortedT;

        private Packet.S2C.Teleport.Struct Recall;

        #endregion

        #region Constructors and Destructors

        public RecallInfo(EnemyInfo enemyInfo)
        {
            this.EnemyInfo = enemyInfo;
            this.Recall = new Packet.S2C.Teleport.Struct(
                this.EnemyInfo.Player.NetworkId,
                Packet.S2C.Teleport.Status.Unknown,
                Packet.S2C.Teleport.Type.Unknown,
                0);
        }

        #endregion

        #region Public Methods and Operators

        public int GetDrawTime()
        {
            var drawtime = 0;

            if (this.WasAborted())
            {
                drawtime = this.FADEOUT_TIME - (Utils.TickCount - this.AbortedT);
            }
            else
            {
                drawtime = this.GetRecallCountdown();
            }

            return drawtime < 0 ? 0 : drawtime;
        }

        public int GetRecallCountdown()
        {
            var time = Utils.TickCount;
            var countdown = 0;

            if (time - this.AbortedT < this.FADEOUT_TIME)
            {
                countdown = this.AbortedRecall.Duration - (this.AbortedT - this.AbortedRecall.Start);
            }
            else if (this.AbortedT > 0)
            {
                countdown = 0;
            }
            else
            {
                countdown = this.Recall.Start + this.Recall.Duration - time;
            }

            return countdown < 0 ? 0 : countdown;
        }

        public bool IsPorting()
        {
            return this.Recall.Type == Packet.S2C.Teleport.Type.Recall
                   && this.Recall.Status == Packet.S2C.Teleport.Status.Start;
        }

        public bool ShouldDraw()
        {
            return this.IsPorting() || (this.WasAborted() && this.GetDrawTime() > 0);
        }

        public override string ToString()
        {
            var drawtext = this.EnemyInfo.Player.ChampionName + ": " + this.Recall.Status;

            var countdown = this.GetRecallCountdown() / 1000f;

            if (countdown > 0)
            {
                drawtext += " (" + countdown.ToString("0.00", CultureInfo.InvariantCulture) + "s)";
            }

            return drawtext;
        }

        public EnemyInfo UpdateRecall(Packet.S2C.Teleport.Struct newRecall)
        {
            this.LockedTarget = false;
            this.EstimatedShootT = 0;

            if (newRecall.Type == Packet.S2C.Teleport.Type.Recall
                && newRecall.Status == Packet.S2C.Teleport.Status.Abort)
            {
                this.AbortedRecall = this.Recall;
                this.AbortedT = Utils.TickCount;
            }
            else
            {
                this.AbortedT = 0;
            }

            this.Recall = newRecall;
            return this.EnemyInfo;
        }

        public bool WasAborted()
        {
            return this.Recall.Type == Packet.S2C.Teleport.Type.Recall
                   && this.Recall.Status == Packet.S2C.Teleport.Status.Abort;
        }

        #endregion
    }
}