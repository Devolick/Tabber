using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Tabber
{
    public class TabberCollection : IEnumerable
    {
        private ItemCollection collection;

        public object this[int index] {
            get { return collection[index]; }
            set { collection[index] = value; }
        }

        public bool IsCurrentBeforeFirst { get; }
        public IEnumerable SourceCollection { get; }
        public bool IsEmpty { get; }
        public int Count { get; }
        public bool IsCurrentAfterLast { get; }
        public bool CanSort { get; }
        public Predicate<object> Filter { get; set; }
        public bool CanFilter { get; }
        public bool NeedsRefresh { get; }
        public SortDescriptionCollection SortDescriptions { get; }
        public bool CanGroup { get; }
        public ObservableCollection<GroupDescription> GroupDescriptions { get; }
        public ReadOnlyObservableCollection<object> Groups { get; }
        public int CurrentPosition { get; }
        public object CurrentItem { get; }

        public TabberCollection()
        {
            CreateCollection();
        }
        private void CreateCollection()
        {
            Type param = typeof(DependencyObject);
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            ConstructorInfo ctor = typeof(ItemCollection).GetConstructors(flags)
                    .First(f => f.GetParameters().Count() == 1 && param.IsAssignableFrom(f.GetParameters()[0].ParameterType));
            collection = (ItemCollection)ctor.Invoke(new object[] { null });
        }

        public int Add(object newItem)
        {
            return collection.Add(newItem);
        }
        public void Clear()
        {
            collection.Clear();
        }
        public bool Contains(object containItem)
        {
            return collection.Contains(containItem);
        }
        public void CopyTo(Array array, int index)
        {
            collection.CopyTo(array, index);
        }
        public IDisposable DeferRefresh()
        {
            return collection.DeferRefresh();
        }
        public object GetItemAt(int index)
        {
            return collection.GetItemAt(index);
        }
        public int IndexOf(object item)
        {
            return collection.IndexOf(item);
        }
        public void Insert(int insertIndex, object insertItem)
        {
            collection.Insert(insertIndex, insertItem);
        }
        public bool MoveCurrentTo(object item)
        {
            return collection.MoveCurrentTo(item);
        }
        public bool MoveCurrentToFirst()
        {
            return collection.MoveCurrentToFirst();
        }
        public bool MoveCurrentToLast()
        {
            return collection.MoveCurrentToLast();
        }
        public bool MoveCurrentToNext()
        {
            return collection.MoveCurrentToNext();
        }
        public bool MoveCurrentToPosition(int position)
        {
            return collection.MoveCurrentToPosition(position);
        }
        public bool MoveCurrentToPrevious()
        {
            return collection.MoveCurrentToPrevious();
        }
        public bool PassesFilter(object item)
        {
            return collection.PassesFilter(item);
        }
        public void Remove(object removeItem)
        {
            collection.PassesFilter(removeItem);
        }
        public void RemoveAt(int removeIndex)
        {
            collection.RemoveAt(removeIndex);
        }

        public IEnumerator<object> GetEnumerator()
        {
            foreach (object item in collection)
            {
                yield return item;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
