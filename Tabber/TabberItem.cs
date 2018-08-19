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
    public class TabberItem: System.Windows.Controls.TabItem
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
                Window currentWindow = Window.GetWindow(this);
                TabberControl tabberControl = (TabberControl)Parent;
                if (!((currentWindow as ContentWindow) == null && 
                    tabberControl.Items.Count < 2 && 
                    tabberControl.Pinned))
                {
                    Point newPosition = e.GetPosition(this);
                    deltaPosition = new Point(newPosition.X - deltaPosition.X, newPosition.Y - deltaPosition.Y);
                    if (Math.Abs(deltaPosition.X) > 4 || Math.Abs(deltaPosition.Y) > 4)
                    {
                        Point dragPosition = this.PointToScreen(new Point(0, 0));
                        window = new ContentWindow(
                            this,
                            new Rect(
                                dragPosition.X,
                                dragPosition.Y,
                                currentWindow.ActualWidth,
                                currentWindow.ActualHeight),
                            true);
                        dragTabAndWindow = false;
                        window.DragMove();
                    }
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
