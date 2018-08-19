using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Tabber.Win32;
using System.Windows.Media;

namespace Tabber
{
    public class TabberControl : System.Windows.Controls.TabControl
    {
        internal static ZOrderList ZOrders { get; private set; }

        private Window currentWindow;
        private ShadowTabberItem shadowItem;
        private TabPanel tabPanel;
        private TabItem lastSelectedItem;
        private bool mainTabberIsEmpty;


        public bool Pinned { get; set; }

        static TabberControl()
        {
            ZOrders = new ZOrderList();
        }
        public TabberControl()
            :base()
        {
            mainTabberIsEmpty = false;
            lastSelectedItem = null;
            shadowItem = null;

            Loaded += TabberControl_Loaded;
            Unloaded += TabberControl_Unloaded;
            ContentWindow.Enter += ContentWindow_Enter;
            ContentWindow.Move += ContentWindow_Move;
            ContentWindow.Over += ContentWindow_Over;

        }

        private void TabberControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ContentWindow.Enter -= ContentWindow_Enter;
            ContentWindow.Move -= ContentWindow_Move;
            ContentWindow.Over -= ContentWindow_Over;
            ZOrders.Remove(currentWindow);
        }
        private void TabberControl_Loaded(object sender, RoutedEventArgs e)
        {
            currentWindow = Window.GetWindow(this);
            currentWindow.Activated += ContentWindow_Activated;
            tabPanel = FindItemsPanel(this);
            ZOrders.AddCheckedFirst(currentWindow);
        }
        private void ContentWindow_Activated(object sender, EventArgs e)
        {
            ZOrders.TopReorder(currentWindow);
        }

        private TabPanel FindItemsPanel(DependencyObject startNode)
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);
            DependencyObject current = null;
            for (int i = 0; i < count; i++)
            {
                current = VisualTreeHelper.GetChild(startNode, i);
                if ((current as TabPanel) != null)
                {
                    return (TabPanel)current;
                }
                else
                {
                    current = FindItemsPanel(current);
                    if (current != null) return (TabPanel)current;
                }
            }
            return null;
        }

        private void ContentWindow_Enter(ContentWindow window)
        {
            if (window.Equals(this)) return;

            mainTabberIsEmpty = (currentWindow as ContentWindow) == null && Items.Count < 1;
            if (!mainTabberIsEmpty)
            {
                foreach (TabItem item in Items)
                {
                    if (item.IsSelected)
                    {
                        lastSelectedItem = item;
                        break;
                    }
                }
            }
            shadowItem = null;
        }
        private void ContentWindow_Move(ContentWindow window)
        {
            if (window.Equals(this)) return;

            if (mainTabberIsEmpty)
            {
                if (EmptyItemsPanelRect(window.GetDragItem())
                        .Contains(window.GetDragPosition()))
                {
                    FocusOrderWindow(window);
                    if (shadowItem == null)
                    {
                        shadowItem = new ShadowTabberItem();
                        shadowItem.Header = new string(' ', 24);
                        Items.Add(shadowItem);
                        shadowItem.IsSelected = true;
                    }
                }
                else
                {
                    if (shadowItem != null)
                    {
                        Items.Remove(shadowItem);
                        shadowItem = null;
                    }
                }
            }
            else
            {
                if (ItemsPanelRect()
                        .Contains(window.GetDragPosition()))
                {
                    FocusOrderWindow(window);
                    FakeTab(window);
                }
                else
                {
                    if (shadowItem != null)
                    {
                        Items.Remove(shadowItem);
                        shadowItem = null;
                        lastSelectedItem.IsSelected = true;
                    }
                }
            }
        }
        private void ContentWindow_Over(ContentWindow window)
        {
            if (window.Equals(this)) return;

            if (mainTabberIsEmpty)
            {
                if (shadowItem != null)
                {
                    ZOrders.OrderFound();
                    int dropItemIndex = Items.IndexOf(shadowItem);
                    Items.Remove(shadowItem);
                    TabberItem replacedItem = window.ReplaceItem();
                    Items.Add(replacedItem);
                    replacedItem.IsSelected = true;
                }
            }
            else
            {
                if (shadowItem != null)
                {
                    ZOrders.OrderFound();
                    int dropItemIndex = Items.IndexOf(shadowItem);
                    Items.Remove(shadowItem);
                    TabberItem replacedItem = window.ReplaceItem();
                    Items.Insert(dropItemIndex, replacedItem);
                    replacedItem.IsSelected = true;
                }
            }
        }

        private void FakeTab(ContentWindow window)
        {
            TabItem replaceItem = null;
            foreach (TabItem tab in Items)
            {
                Rect tabRect = TabItemRect(tab);
                if (tabRect.Contains(window.GetDragPosition()))
                {
                    replaceItem = tab;
                    break;
                }
            }
            if (replaceItem != null)
            {
                if ((replaceItem as TabberItem) != null) {
                    int shadowIndex = shadowItem != null ? Items.IndexOf(shadowItem) : -1;
                    if (shadowItem == null)
                    {
                        shadowItem = new ShadowTabberItem();
                        shadowItem.Header = new string(' ', 24);
                    }
                    int replaceItemIndex = Items.IndexOf(replaceItem);

                    if (shadowIndex > -1 && shadowIndex < replaceItemIndex)
                    {
                        Items.Remove(replaceItem);
                        Items.Insert(shadowIndex, replaceItem);
                    }
                    else
                    {
                        Items.Remove(shadowItem);
                        Items.Insert(replaceItemIndex, shadowItem);
                    }
                }
            }
            else if((Items[Items.Count - 1] as ShadowTabberItem) == null)
            {
                if (shadowItem == null)
                {
                    shadowItem = new ShadowTabberItem();
                    shadowItem.Header = new string(' ', 24);
                    Items.Add(shadowItem);
                }
                else
                {
                    Items.Remove(shadowItem);
                    Items.Add(shadowItem);
                }
            }

            shadowItem.IsSelected = true;
        }
        private Rect TabItemRect(TabItem tab)
        {
            Point locationFromScreen = tab.PointToScreen(new Point(0, 0));
            Rect tabRect = new Rect(
                new Point(locationFromScreen.X, locationFromScreen.Y),
                new Size(tab.ActualWidth, tab.ActualHeight));

            return tabRect;
        }
        private Rect ItemsPanelRect()
        {
            Point locationFromScreen = tabPanel.PointToScreen(new Point(0, 0));
            return new Rect(
                locationFromScreen.X,
                locationFromScreen.Y,
                tabPanel.ActualWidth,
                tabPanel.ActualHeight);
        }
        private Rect EmptyItemsPanelRect(TabItem tab)
        {
            Point locationFromScreen = tabPanel.PointToScreen(new Point(0, 0));
            return new Rect(
                locationFromScreen.X,
                locationFromScreen.Y,
                tabPanel.ActualWidth,
                tab.ActualHeight);
        }
        private void FocusOrderWindow(ContentWindow window)
        {
            ZOrders.OrderFound();
            Debug.WriteLine("FocusOrderWindow:"+ currentWindow.GetHashCode());
            if (!ZOrders.IsBackOfFront(window, currentWindow))
            {
                currentWindow.Focus();
                window.Focus();
            }
        }
    }
}
