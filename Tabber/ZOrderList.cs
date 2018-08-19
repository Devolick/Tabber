using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace Tabber
{
    internal class ZOrderList : LinkedList<Window>, IEnumerable<Window>
    {
        private bool orderFound;

        internal ZOrderList()
            : base() {
            orderFound = false;
        }

        internal void OrderFound()
        {
            orderFound = true;
        }
        internal bool IsBackOfFront(Window front, Window back)
        {
            if (orderFound)
            {
                Debug.WriteLine($"front:{front.GetHashCode()},{First.Value.GetHashCode()}|back:{back.GetHashCode()},{First.Next.Value.GetHashCode()}");
                string ordersString = string.Empty;
                foreach (var item in (LinkedList<Window>)this)
                {
                    ordersString += $"{item.GetHashCode()},";
                }
                Debug.WriteLine($"orders:{ordersString}");
            }
            if (Count > 1)
            {
                return front.Equals(First.Value) && back.Equals(First.Next.Value);
            }
            else
            {
                return false;
            }
        }
        internal void AddCheckedFirst(Window window)
        {
            if(!(this as LinkedList<Window>).Any(a => a.Equals(window)))
            {
                AddFirst(window);
            }
        }
        internal void TopReorder(Window window)
        {
            Remove(window);
            AddFirst(window);
        }
        internal void ActiveReorder(Window front, Window back)
        {
            TopReorder(back);
            back.Topmost = true;
            back.Topmost = false;
            TopReorder(front);
            back.Topmost = true;
            back.Topmost = false;
        }

        public new IEnumerator<Window> GetEnumerator()
        {
            orderFound = false;
            foreach (Window window in (LinkedList<Window>)this)
            {
                if (!orderFound) yield return window;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
