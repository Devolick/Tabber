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
    public class TabberControl : TabControl
    {
        private Window window;
        private ShadowTabberItem shadowItem;
        private TabPanel tabPanel;

        public TabberControl()
            :base()
        {
            window = Window.GetWindow(this);
            shadowItem = null;
            SizeChanged += TabberControl_SizeChanged;

            Loaded += TabberControl_Loaded;
        }

        private void TabberControl_Loaded(object sender, RoutedEventArgs e)
        {
            tabPanel = FindItemsPanel(this);
        }

        private void TabberControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private TabPanel FindItemsPanel(DependencyObject startNode)
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);
            DependencyObject current = null;
            for (int i = 0; i < count; i++)
            {
                current = VisualTreeHelper.GetChild(startNode, i);
                if ((current.GetType()).Equals(typeof(TabPanel)))
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

        internal void Enter(ContentWindow window)
        {
            shadowItem = null;
        }
        internal void Move(ContentWindow window)
        {
            if (ContainsItemsPanel(window.GetDragPosition()))
            {
                FakeTab(window);
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
        internal void Over(ContentWindow window)
        {
            if (shadowItem != null)
            {
                int dropItemIndex = Items.IndexOf(shadowItem);
                Items.Remove(shadowItem);
                TabberItem replacedItem = window.ReplaceItem();
                Items.Insert(dropItemIndex, replacedItem);
                replacedItem.IsSelected = true;
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
                    if (shadowItem == null)
                    {
                        shadowItem = new ShadowTabberItem();
                        shadowItem.Header = new string(' ', 24);
                    }
                    else
                    {
                        Items.Remove(shadowItem);
                    }
                    Items.Insert(Items.IndexOf(replaceItem), shadowItem);
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
        }
        private bool ContainsItemsPanel(Point dragPoint)
        {
            Rect itemPanelRect = ItemsPanelRect();
            //TESTWindow.HighlightPosition(this, "ItemsPanel", itemPanelRect);
            return itemPanelRect.Contains(dragPoint);
        }
        private Rect TabItemRect(TabItem tab)
        {
            Point tabPosition = Mouse.GetPosition(tab);
            Rect tabRect = new Rect(
                new Point(tabPosition.X, tabPosition.Y),
                new Size(tab.ActualWidth, tab.ActualHeight));

            return tabRect;
        }
        private Rect ItemsPanelRect()
        {
            Point tabberPosition = Mouse.GetPosition(tabPanel);
            return new Rect(
                tabberPosition.X - tabPanel.ActualWidth,
                tabberPosition.Y - tabPanel.ActualHeight,
                tabPanel.ActualWidth,
                tabPanel.ActualHeight);
        }

    }
}
