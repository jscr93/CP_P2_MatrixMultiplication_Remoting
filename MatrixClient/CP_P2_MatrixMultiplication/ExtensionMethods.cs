using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MatrixMultiplicationClient
{
    public static class UIObservableCollection
    {
        public static void ClearOnUI<T>(this ICollection<T> collection)
        {
            Action ClearMethod = collection.Clear;
            Application.Current.Dispatcher.BeginInvoke(ClearMethod);
        }

        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        public static void UpdateScroll(this ListBox listbox)
        {
            Action scrollBottom = new Action(() => {
                    var border = (Border)VisualTreeHelper.GetChild(listbox, 0);
                    var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
            });
            Application.Current.Dispatcher.BeginInvoke(scrollBottom);
        }
    }
}