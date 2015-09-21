using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
namespace NotepadSharp
{
    class Program
    {
        static List<string> notes = new List<string>();
        static Render.Rectangle noteBox = new Render.Rectangle(0,0,1,1,Color.Black);
        private static Circle color = new Circle(true, System.Drawing.Color.Black, 0f);
        static Menu rootMenu = new Menu("Notepad#", "Notepad#", true);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
            Drawing.OnDraw += OnDraw;
            Game.OnInput += inputArg =>
            {
                try
                {
                        string input = inputArg.Input;
                        
                        if (input.StartsWith("/add ", true, null))
                        {
                            notes.Add(input.Substring(4).Trim());
                            inputArg.Process = false;
                        }
                        else if (input.StartsWith("/del ", true, null))
                        {
                            input = input.Remove(0, 4);
                            int number = 0;
                            if(int.TryParse(input.Trim(), out number))
                                notes.RemoveAt(number);
                            inputArg.Process = false;
                        }
                        else if (input.StartsWith("/clear", true, null))
                        {
                            notes.Clear();
                            inputArg.Process = false;
                        }

                }
                catch (Exception e)
                {
                }
            };
            Game.OnChat += chatArg =>
            {
                if (chatArg.Sender == null || !chatArg.Sender.IsValid)
                    return;
            };
        }

        private static void OnLoad(EventArgs e)
        {
            rootMenu.AddSubMenu(new Menu("Position", "Position", false));
            rootMenu.SubMenu("Position").AddItem(new MenuItem("X", "X", false).SetValue(new Slider(0, 0, Drawing.Width)));
            rootMenu.SubMenu("Position").AddItem(new MenuItem("Y", "Y", false).SetValue(new Slider(0, 0, Drawing.Height)));
            rootMenu.AddItem(
                new MenuItem("Drawn Color", "Drawn Color", false).SetValue(new Circle(false, System.Drawing.Color.SkyBlue, 0f)));
            rootMenu.AddToMainMenu();
        }

        private static void OnDraw(EventArgs e)
        {
            noteBox.X = rootMenu.SubMenu("Position").Item("X").GetValue<Slider>().Value;
            noteBox.Y = rootMenu.SubMenu("Position").Item("Y").GetValue<Slider>().Value;
            //Play with these values a bit; assuming 12 pixels high for each note and 4 pixels wide for each letter.
            int height = notes.Count*15;
            int width = (notes.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length + 3) * 10;
            noteBox.Height = height;
            noteBox.Width = width;
            drawLines();
            for (int i = 0; i < notes.Count; i++)
            {
                Drawing.DrawText((float) (noteBox.X + 2), noteBox.Y + 12*i, rootMenu.Item("Drawn Color").GetValue<Circle>().Color, i + ": " + notes[i]);
            }
            
        }

        static void drawLines()
        {   
            //Top left to Bottom left
            Drawing.DrawLine((float)noteBox.X, (float)noteBox.Y, (float)(noteBox.X), (float)(noteBox.Y + noteBox.Height+2), 3f, rootMenu.Item("Drawn Color").GetValue<Circle>().Color);
            //Top right to bottom right
            Drawing.DrawLine((float)(noteBox.X+noteBox.Width), (float)noteBox.Y, (float)(noteBox.X+noteBox.Width), (float)(noteBox.Y + noteBox.Height), 3f, rootMenu.Item("Drawn Color").GetValue<Circle>().Color);
            //Bottom left to bottom right
            Drawing.DrawLine((float)noteBox.X, (float)(noteBox.Y+noteBox.Height), (float)(noteBox.X+noteBox.Width), (float)(noteBox.Y + noteBox.Height), 3f, rootMenu.Item("Drawn Color").GetValue<Circle>().Color);
            //Top left to top right
            Drawing.DrawLine((float)noteBox.X, (float)noteBox.Y-.5f, (float)(noteBox.X+noteBox.Width), (float)(noteBox.Y-.5f), 3f, rootMenu.Item("Drawn Color").GetValue<Circle>().Color);
        }
    }
}
