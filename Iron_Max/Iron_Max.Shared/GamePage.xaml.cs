using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MonoGame.Framework;

namespace Iron_Max
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : SwapChainBackgroundPanel
    {
       readonly GameSystemIMax _game;
       public static bool Pause = false;
      
        public GamePage(string launchArguments)
        {
            this.InitializeComponent();
            Pause_Game.Visibility = Visibility.Visible;
            
            
            _game = XamlGame<GameSystemIMax>.Create(launchArguments, Window.Current.CoreWindow, this);
        }
        
        
        private void Pause_Game_Click(object sender, RoutedEventArgs e)
        {
            Pause = !Pause;
            if (!Pause)
            {
                Pause_Block.Visibility = Visibility.Collapsed;
                Pause_game_text.Visibility = Visibility.Collapsed;
            }
            else
                Pause_Block.Visibility = Visibility.Visible;
                Pause_game_text.Visibility = Visibility.Visible;
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            Pause = !Pause;
            Pause_Block.Visibility = Visibility.Collapsed;
            Pause_game_text.Visibility = Visibility.Collapsed;
        }

        private void Back_Menu_Click(object sender, RoutedEventArgs e)
        {
          //  _game.launchedLevelGame = false;
            Pause = !Pause;
            (App.Current as App).WindowMenuLaunched();
           
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            _game.ReloadCurrentLevel(); 
            Pause = !Pause;
            Pause_game_text.Visibility = Visibility.Collapsed;
            Pause_Block.Visibility = Visibility.Collapsed;

        }

        private void back_Menu_button_Levels_Click(object sender, RoutedEventArgs e)
        {
        
            (App.Current as App).WindowMenuLaunched();
        }

    }
}
