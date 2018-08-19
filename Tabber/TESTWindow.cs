using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Tabber
{
    internal class TESTWindow:Window
    {
        private static List<KeyValuePair<object, Window>> objects;

        static TESTWindow()
        {
            objects = new List<KeyValuePair<object, Window>>();
        }
        private TESTWindow(object followObject, string name, Rect rect)
        {
            objects.Add(new KeyValuePair<object, Window>(followObject, this));
            int rnd = new Random().Next(1, 40);
            Background = typeof(Brushes).GetProperties(BindingFlags.Static| BindingFlags.Public).First(f=> rnd-- < 1).GetValue(null,null) as Brush;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Content = new System.Windows.Controls.Label() { Content = name, Background = Brushes.Transparent, Foreground = Brushes.Black };
            Topmost = true;
            Opacity = 0.2;
            Left = Application.Current.MainWindow.Left + rect.X;
            Top = Application.Current.MainWindow.Top + rect.Y;
            Width = rect.Width;
            Height = rect.Height;
            Show();
        }
        public static void HighlightPosition(object followObject, string name, Rect rect)
        {
            if (objects.Any(a => a.Key.Equals(followObject)))
            {
                var keyValuePair = objects.FirstOrDefault(a => a.Key.Equals(followObject));

                keyValuePair.Value.Left = Application.Current.MainWindow.Left + rect.X;
                keyValuePair.Value.Top = Application.Current.MainWindow.Top + rect.Y;
                keyValuePair.Value.Width = rect.Width;
                keyValuePair.Value.Height = rect.Height;
            }
            else
            {
                new TESTWindow(followObject, name, rect);
            }
        }
    }
}
