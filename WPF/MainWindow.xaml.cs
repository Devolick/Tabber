using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tabber;

namespace WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();      
        }

        private void TabberItem_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            Debug.WriteLine("TabberItem_Closing");
        }
        private void TabberItem_DragPressed(object sender, EventArgs e)
        {
            Debug.WriteLine("TabberItem_DragPressed");
        }
        private void TabberItem_DragReleased(object sender, EventArgs e)
        {
            Debug.WriteLine("TabberItem_DragReleased");
        }

        //First select item and then double click for Pin it.
        private void PinTabberItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TabberItem).Pin = !(sender as TabberItem).Pin;
            (sender as TabberItem).Header = $"{string.Concat((sender as TabberItem).Header.ToString().TakeWhile((c) => c != ':'))}: {(sender as TabberItem).Pin}";
        }
    }
}
