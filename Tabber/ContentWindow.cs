using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Tabber.Win32;

namespace Tabber
{
    internal sealed class ContentWindow : System.Windows.Window
    {
        private static event Action<ContentWindow> Check;
        internal static event Action<ContentWindow> Enter;
        internal static event Action<ContentWindow> Move;
        internal static event Action<ContentWindow> Over;

        private TabberControl tabberControl;
        private bool firstDrag;
        private bool inDrag;
        private Brush background;


        private ContentWindow()
        {
            inDrag = false;
            Check += ContentWindow_Check;
        }
        internal ContentWindow(TabberItem item, Rect rect, bool firstDrag=false)
            :this()
        {
            this.firstDrag = firstDrag;
            (item.Parent as TabberControl).Items.Remove(item);
            tabberControl = new TabberControl();
            tabberControl.Items.Add(item);
            Content = tabberControl;
            if (firstDrag)
            {
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                background = Background;
                Background = Brushes.Transparent;
            }
            Top = rect.Y;
            Left = rect.X;
            Width = rect.Width;
            Height = rect.Height;
            Closed += ContentWindow_Closed;
            LocationChanged += ContentWindow_LocationChanged;

        }

        private void ContentWindow_Closed(object sender, EventArgs e)
        {
            Check -= ContentWindow_Check;
        }
        private void ContentWindow_LocationChanged(object sender, EventArgs e)
        {
            if (inDrag)
            {
                Move?.Invoke(this);
            }
        }

        private void ContentWindow_Check(ContentWindow window)
        {
            if (window.Equals(this)) return;

            bool isEmpty = tabberControl.Items.Count < 1;
            if (isEmpty)
            {
                Left = Left + (Width / 2);
                Top = Top + (Height / 2);
                Width = 0;
                Height = 0;
                Hide();
                Close();
            }
        }

        internal new void DragMove()
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragBegin();
                base.DragMove();
                DragEnd();
            }
        }
        private void DragBegin()
        {
            if (firstDrag)
            {
                Show();
                Activate();
                Check?.Invoke(this);
            }
            inDrag = true;
            Enter?.Invoke(this);
        }
        private void DragEnd()
        {
            Over?.Invoke(this);
            inDrag = false;
            if (firstDrag)
            {
                if (tabberControl.Items.Count == 1)
                {
                    TabberItem item = (TabberItem)tabberControl.Items[0];
                    ContentWindow newWindow = new ContentWindow(
                        item,
                        new Rect(Left, Top, ActualWidth, ActualHeight),
                        false);
                    Hide();
                    newWindow.Show();
                    newWindow.Activate();
                    Close();
                }
            }
            firstDrag = false;
        }

        internal Point GetDragPosition()
        {
            TabberItem item = GetDragItem();
            Point locationFromScreen = item.PointToScreen(new Point(0, 0));

            return locationFromScreen;
        }
        internal TabberItem GetDragItem()
        {
            return (TabberItem)tabberControl.Items[0];
        }
        internal TabberItem ReplaceItem()
        {
            TabberItem item = (TabberItem)tabberControl.Items[0];
            tabberControl.Items.Remove(item);
            Hide();
            Close();
            return item;
        }
        internal TabberControl GetTabberControl()
        {
            return tabberControl;
        }
    }
}
