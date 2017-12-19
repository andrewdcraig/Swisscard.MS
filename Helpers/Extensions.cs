using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EnterpriseManagement.UI.DataModel;
using System.Windows;
using System.Windows.Media;

namespace Swisscard.MS.Helpers
{
    public static class Extensions
    {
        public static void AddToCollection(this object collection, object item)
        {
            var list = collection as IList<IDataItem>;
            if (list == null)
                throw new ArgumentException("Object is not a collection", "collection");
            var dataItem = item as IDataItem;
            if (dataItem == null)
                throw new ArgumentException("Item must be an IdataItem instance", "item");

            list.Add(dataItem);
        }

        public static void RemoveFromCollection(this object collection, object item)
        {
            var list = collection as IList<IDataItem>;
            if (list == null)
                throw new ArgumentException("Object is not a collection", "collection");
            var dataItem = item as IDataItem;
            if (dataItem == null)
                throw new ArgumentException("Item must be an IdataItem instance", "item");

            list.Remove(dataItem);
        }

        public static void MoveToCollection(this object item, object from, object to)
        {
            to.AddToCollection(item);
            from.RemoveFromCollection(item);
        }

        public static T FindAncestor<T>(DependencyObject current)
                 where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
