using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tabber
{
    /// <summary>
    /// Drag tab item
    /// </summary>
    public class TabberItem : System.Windows.Controls.TabItem
    {
        /// <summary>
        /// Will be called when tab should start to drag.
        /// </summary>
        public event EventHandler DragPressed;
        /// <summary>
        /// Will be called when tab should end to drag.
        /// </summary>
        public event EventHandler DragReleased;
        /// <summary>
        /// Will be called when the tab tries to close.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;

        private Point deltaPosition;
        private ContentWindow window;
        private bool dragTabAndWindow;
        private bool inDragMove;
        private bool moveEnter;

        /// <summary>
        /// Hold this tab pinned separate from unpinned. 
        /// </summary>
        public bool Pin
        {
            get { return (bool)GetValue(PinProperty); }
            set { SetValue(PinProperty, value); }
        }
        public static readonly DependencyProperty PinProperty =
            DependencyProperty.Register("Pin", typeof(bool), typeof(TabberItem), new PropertyMetadata(false, Pin_CallBack));
        private static void Pin_CallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((d as TabberItem).Parent as TabberControl).SortPins();
        }

        public TabberItem()
        {
            moveEnter = false;
            inDragMove = false;
            dragTabAndWindow = false;
            deltaPosition = new Point(0, 0);
            PreviewMouseLeftButtonDown += TabberItem_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += TabberItem_PreviewMouseLeftButtonUp;
            PreviewMouseMove += TabberItem_PreviewMouseMove;
            Unloaded += TabberItem_Unloaded;
        }

        private void TabberItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
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
                    Point innerMousePosition = Mouse.GetPosition(this);

                    if(moveEnter)
                    {
                        #region Drag Section
                        if (((innerMousePosition.Y <= 0 || innerMousePosition.Y >= ActualHeight) &&
                            (innerMousePosition.X > 0 && innerMousePosition.X < ActualWidth)) ||
                            (tabberControl.Items[0].Equals(this) && innerMousePosition.X <= 0) ||
                            (tabberControl.Items[tabberControl.Items.Count - 1].Equals(this) && innerMousePosition.X >= ActualWidth))
                        {
                            if (IsMouseCaptured)
                            {
                                Mouse.Capture(null);
                            }

                            DragPressed?.Invoke(this, new EventArgs());
                            Point dragPosition = this.PointToScreen(innerMousePosition);
                            Pin = false;
                            window = new ContentWindow(
                                this,
                                new Rect(
                                    dragPosition.X + (-ActualWidth / 2),
                                    dragPosition.Y + (-ActualHeight / 2),
                                    currentWindow.ActualWidth,
                                    currentWindow.ActualHeight),
                                true);
                            dragTabAndWindow = false;
                            inDragMove = true;
                            window.DragMove();
                            DragReleased?.Invoke(this, new EventArgs());
                        }
                        #endregion
                        #region Swap Section
                        else if (!tabberControl.Items[0].Equals(this) && innerMousePosition.X <= 0)
                        {
                            int index = tabberControl.Items.IndexOf(this);
                            TabberItem swapItem = (TabberItem)tabberControl.Items[index - 1];
                            if (swapItem.Pin == Pin)
                            {
                                tabberControl.Items.Remove(swapItem);
                                tabberControl.Items.Insert(index, swapItem);
                            }
                        }
                        else if (!tabberControl.Items[tabberControl.Items.Count - 1].Equals(this) && innerMousePosition.X >= ActualWidth)
                        {
                            int index = tabberControl.Items.IndexOf(this);
                            TabberItem swapItem = (TabberItem)tabberControl.Items[index + 1];
                            if (swapItem.Pin == Pin)
                            {
                                tabberControl.Items.Remove(swapItem);
                                tabberControl.Items.Insert(index, swapItem);
                            }
                        }
                        #endregion
                    }
                    moveEnter = TabItemLocalRect().Contains(Mouse.GetPosition(this));
                }
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                if (IsMouseCaptured && !inDragMove)
                {
                    Mouse.Capture(null);
                }
            }
        }
        private void TabberItem_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                dragTabAndWindow = true;
            }
            else
            {
                dragTabAndWindow = false;
            }
            if (IsMouseCaptured && !inDragMove)
            {
                Mouse.Capture(null);
            }
        }
        private void TabberItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Mouse.Capture(this, CaptureMode.SubTree);
                moveEnter = true;
                inDragMove = false;
                dragTabAndWindow = true;
            }
            else
            {
                dragTabAndWindow = false;
                if (IsMouseCaptured && !inDragMove)
                {
                    Mouse.Capture(null);
                }
            }
        }

        private Rect TabItemLocalRect()
        {
            Rect tabRect = new Rect(
                new Point(0, 0),
                new Size(ActualWidth, ActualHeight));

            return tabRect;
        }
        internal bool CanClosing()
        {
            CancelEventArgs cancelArgs = new CancelEventArgs();
            Closing?.Invoke(this, cancelArgs);
            return !cancelArgs.Cancel;
        }
        public void Close()
        {
            if (CanClosing())
            {
                TabberControl tabberControl = (Parent as TabberControl);
                tabberControl.Items.Remove(this);
                if (tabberControl.Items.Count < 1)
                {
                    Window.GetWindow(this).Close();
                }
            }
        }
    }
}
