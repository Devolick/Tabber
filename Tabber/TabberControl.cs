using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Tabber
{
    /// <summary>
    /// Drag tab control
    /// </summary>
    public class TabberControl : System.Windows.Controls.TabControl
    {
        private Window currentWindow;
        private ShadowTabberItem shadowItem;
        private TabPanel tabPanel;
        private TabItem lastSelectedItem;
        private bool mainTabberIsEmpty;
        private bool firstSort;

        internal static ZOrderList ZOrders { get; private set; }

        /// <summary>
        /// Will hold the last tab from dragging.
        /// </summary>
        public bool Pinned
        {
            get { return (bool)GetValue(PinnedProperty); }
            set { SetValue(PinnedProperty, value); }
        }
        /// <summary>
        /// Will hold the last tab from dragging.
        /// </summary>
        public static readonly DependencyProperty PinnedProperty =
            DependencyProperty.Register("Pinned", typeof(bool), typeof(TabberControl), new PropertyMetadata(false));

        static TabberControl()
        {
            ZOrders = new ZOrderList();
        }
        public TabberControl()
            :base()
        {
            firstSort = true;
            mainTabberIsEmpty = false;
            lastSelectedItem = null;
            shadowItem = null;

            Loaded += TabberControl_Loaded;
            Unloaded += TabberControl_Unloaded;
        }

        private void TabberControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ZOrders.Remove(this);
        }
        private void TabberControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                currentWindow = Window.GetWindow(this);
                currentWindow.Activated += ContentWindow_Activated;
                currentWindow.Closing += CurrentWindow_Closing;
            }
            tabPanel = FindItemsPanel(this);
            ZOrders.AddCheckedFirst(this);

            firstSort = false;
            SortPins();
        }
        private void CurrentWindow_Closing(object sender, CancelEventArgs e)
        {
            foreach (TabberItem item in Items)
            {
                e.Cancel = !item.CanClosing();
                if (e.Cancel) break;
            }
        }
        private void ContentWindow_Activated(object sender, EventArgs e)
        {
            ZOrders.TopReorder(this);
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

        internal void Enter(ContentWindow window)
        {
            if (object.ReferenceEquals(window, currentWindow)) return;

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
        internal void Move(ContentWindow window)
        {
            if (object.ReferenceEquals(window, currentWindow)) return;

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
                    FakeTab(window.GetDragPosition());
                }
                else
                {
                    if (shadowItem != null)
                    {
                        Items.Remove(shadowItem);
                        shadowItem = null;
                        lastSelectedItem.IsSelected = true;
                    }
                    oldReplaceItem = null;
                }
            }
        }
        internal void Over(ContentWindow window)
        {
            if (object.ReferenceEquals(window, currentWindow)) return;

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
                    currentWindow.Activate();
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
                    currentWindow.Activate();
                }
            }
        }

        private TabItem oldReplaceItem;
        private void FakeTab(Point drahPosition)
        {
            TabItem replaceItem = null;
            foreach (TabItem tab in Items)
            {
                Rect tabRect = TabItemRect(tab);
                if (tabRect.Contains(drahPosition))
                {
                    replaceItem = tab;
                    break;
                }
            }
            if (replaceItem != null)
            {
                TabberItem tabberItem = replaceItem as TabberItem;
                if (tabberItem != null && !tabberItem.Pin && (oldReplaceItem ==null || !oldReplaceItem.Equals(replaceItem))) {
                    oldReplaceItem = replaceItem;
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
                    shadowItem.IsSelected = true;
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
                oldReplaceItem = null;
                shadowItem.IsSelected = true;
            }
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
            if (!ZOrders.IsBackOfFront(window.GetTabberControl(), this))
            {
                ZOrders.ActiveReorder(window.GetTabberControl(), this);
                currentWindow.Topmost = true;
                currentWindow.Topmost = false;
                window.Topmost = true;
                window.Topmost = false;
            }
        }

        internal void SortPins()
        {
            if (firstSort) return; 

            TabberItem selectedItem = (TabberItem)SelectedItem;
            for (int i = 1; i <= Items.Count; i++)
                for (int j = 0; j < Items.Count - i; j++)
                {
                    if (!(Items[j] as TabberItem).Pin &&
                        (Items[j + 1] as TabberItem).Pin)
                    {
                        object nextTemp = Items[j + 1];
                        Items.Remove(nextTemp);
                        Items.Insert(j, nextTemp);
                    }
                }
            if(selectedItem == null && Items.Count > 0)
            {
                (Items[0] as TabberItem).IsSelected = true;
            }
            else if(Items.Count > 0)
            {
                selectedItem.IsSelected = true;
            }
        }
    }
}
