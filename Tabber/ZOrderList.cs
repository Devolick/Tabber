using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace Tabber
{
    internal class ZOrderList : LinkedList<TabberControl>, IEnumerable<TabberControl>
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
        internal bool IsBackOfFront(TabberControl front, TabberControl back)
        {
            if (Count > 1)
            {
                return front.Equals(First.Value) && back.Equals(First.Next.Value);
            }
            else
            {
                return false;
            }
        }
        internal void AddCheckedFirst(TabberControl tabberControl)
        {
            if(!(this as LinkedList<TabberControl>).Any(a => a.Equals(tabberControl)))
            {
                AddFirst(tabberControl);
            }
        }
        internal void TopReorder(TabberControl tabberControl)
        {
            Remove(tabberControl);
            AddFirst(tabberControl);
        }
        internal void ActiveReorder(TabberControl front, TabberControl back)
        {
            TopReorder(back);
            TopReorder(front);
        }
        
        public new IEnumerator<TabberControl> GetEnumerator()
        {
            orderFound = false;
            LinkedListNode<TabberControl> node = base.First;
            if(node != null)
            {
                do
                {
                    yield return node.Value;
                    node = node.Next;
                }
                while (node != null && !orderFound);
            }
            else
            {
                yield break;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
