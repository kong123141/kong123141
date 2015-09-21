
using System;
using System.Drawing;
using System.IO;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace Unban.exe
{
    class Program
    {
        public static SharpDX.Direct3D9.Texture taco;
        public static SharpDX.Direct3D9.Sprite sprite;
        public static SharpDX.Direct3D9.Device dxDevice = Drawing.Direct3DDevice;
        public static int i = 0;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Unban.exe By DZ191 Loaded. Credits to DETUKS");
            sprite = new Sprite(dxDevice);
           taco = Texture.FromMemory(
                    Drawing.Direct3DDevice,
                    (byte[])new ImageConverter().ConvertTo(LoadPicture("http://puu.sh/cP1qD/d23cd24220.jpg"), typeof(byte[])), 513, 744, 0,
                    Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
            Drawing.OnEndScene += Drawing_OnEndScene;
            Drawing.OnPreReset += DrawingOnOnPreReset;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnDomainUnload;
        }

        private static void CurrentDomainOnDomainUnload(object sender, EventArgs e)
        {
            sprite.Dispose();
        }

        private static void DrawingOnOnPostReset(EventArgs args)
        {
            sprite.OnResetDevice();
        }

        private static void DrawingOnOnPreReset(EventArgs args)
        {
            sprite.OnLostDevice();
        }
        private static Bitmap LoadPicture(string url)
        {

            System.Net.WebRequest request = System.Net.WebRequest.Create(url);
            System.Net.WebResponse response = request.GetResponse();
            System.IO.Stream responseStream = response.GetResponseStream();
            Bitmap bitmap2 = new Bitmap(responseStream);
            Console.WriteLine(bitmap2.Size);
            return (bitmap2);
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
           DrawTaco();
        }

        static void DrawTaco()
        {
            if (i == 360) i = 0;
            var IAngle = i*(Math.PI/180);
            var Sin = (float)Math.Sin(IAngle);
            var Cos = (float)Math.Cos(IAngle);
            sprite.Begin();
            sprite.Draw(taco, new ColorBGRA(255, 255, 255, 255), null, new Vector3(-450+200*Cos, 0+200*Sin, 0));
            sprite.End();
            i+=5;
           // Utility.DelayAction.Add(15, () => DrawTaco());
        }
    }
}
