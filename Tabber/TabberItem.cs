using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tabber
{
    public class TabberItem: TabItem
    {
        private Point deltaPosition;    
        private ContentWindow window;
        private bool dragTabAndWindow;

        public TabberItem()
        {
            dragTabAndWindow = false;
            deltaPosition = new Point(0, 0);
            PreviewMouseLeftButtonDown += TabberItem_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += TabberItem_PreviewMouseLeftButtonUp;
            PreviewMouseMove += TabberItem_PreviewMouseMove;
        }

        private void TabberItem_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && dragTabAndWindow)
            {
                Point newPosition = e.GetPosition(this);
                deltaPosition = new Point(newPosition.X - deltaPosition.X, newPosition.Y - deltaPosition.Y);
                if (Math.Abs(deltaPosition.X) > 4 || Math.Abs(deltaPosition.Y) > 4)
                {
                    window = new ContentWindow(this);
                    dragTabAndWindow = false;
                    window.DragMove();
                }
            }
        }
        private void TabberItem_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PreviewMouseEvent(sender, e);
        }
        private void TabberItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PreviewMouseEvent(sender, e);
        }
        private void PreviewMouseEvent(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                dragTabAndWindow = true;
                deltaPosition = e.GetPosition(this);
            }
            else
            {
                dragTabAndWindow = false;
            }
        }


    }
}
