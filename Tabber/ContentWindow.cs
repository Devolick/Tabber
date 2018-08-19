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
        private static event Action<ContentWindow> Move;
        private static event Action<ContentWindow> Enter;
        private static event Action<ContentWindow> Over;

        private TabberControl tabberControl;
        private Point itemInnerPosition;
        private Point itemScreenPosition;
        private bool firstDrag;
        private bool inDrag;

        private Brush background;

        private TabItem testItem;

        private ContentWindow()
        {
            inDrag = false;
            Check += ContentWindow_Check;
            Move += ContentWindow_Move;
            Enter += ContentWindow_Enter;
            Over += ContentWindow_Over;
            Closed += ContentWindow_Closed;
            LocationChanged += (s,e)=> {
                if (inDrag)
                {
                    Move?.Invoke((ContentWindow)s);
                }
            };
        }
        internal ContentWindow (TabberItem item, bool firstDrag=true)
            :this()
        {
            testItem = item;
            this.firstDrag = firstDrag;
            itemInnerPosition = Mouse.GetPosition(item);
            itemScreenPosition = item.PointToScreen(Mouse.GetPosition(item));
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
        }
        private ContentWindow(TabberItem item, Rect rect, bool firstDrag=false)
            :this(item, firstDrag)
        {
            double captionHeight = WindowStyle == WindowStyle.None ? 0 : SystemParameters.CaptionHeight;
            Loaded += (s, e) => {
                Top = rect.Y - captionHeight;
                Top = Top < 0 ? 0 : Top;
                Left = rect.X;
                Left = Left < 0 ? 0 : Left;
                Width = rect.Width;
                Height = rect.Height;
            }; 
        }

        private void ContentWindow_Over(ContentWindow window)
        {
            if (window.Equals(this) || this.tabberControl.Items.Count < 1) return;

            this.tabberControl.Over(window);
        }
        private void ContentWindow_Enter(ContentWindow window)
        {
            if (window.Equals(this) || this.tabberControl.Items.Count < 1) return;

            this.tabberControl.Enter(window);
        }
        private void ContentWindow_Move(ContentWindow window)
        {
            if (window.Equals(this) || this.tabberControl.Items.Count < 1) return;

            this.tabberControl.Move(window);
        }
        private void ContentWindow_Closed(object sender, EventArgs e)
        {
            Check -= ContentWindow_Check;
            Enter -= ContentWindow_Enter;
            Over -= ContentWindow_Over;
            Move -= ContentWindow_Move;
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
                TabberItem item = (TabberItem)tabberControl.Items[0];
                Left = itemScreenPosition.X - itemInnerPosition.X;
                Left = Left < 0 ? 0 : Left;
                Top = itemScreenPosition.Y - itemInnerPosition.Y;
                Top = Top < 0 ? 0 : Top;
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
            TabberItem item = (TabberItem)tabberControl.Items[0];
            Point itemPosition = Mouse.GetPosition(item);
            return new Point(itemPosition.X + item.ActualWidth, itemPosition.Y + item.ActualHeight);
        }
        internal TabberItem ReplaceItem()
        {
            TabberItem item = (TabberItem)tabberControl.Items[0];
            tabberControl.Items.Remove(item);
            Hide();
            Close();
            return item;
        }
    }
}
