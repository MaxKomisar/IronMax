﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Iron_Max
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Menu : Page
    {

        public Menu()
        {
            this.InitializeComponent();
        }

       

        private void StartGameIMax_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).WindowGameLaunched();
        }

        private void SettingIMax_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Setting));
        }

        private void AuthorGameIMax_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Author));
        }
    }
}
