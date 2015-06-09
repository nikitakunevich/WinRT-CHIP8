using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
//using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using Windows.UI;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Microsoft.Graphics.Canvas;
using Windows.Storage;
using Windows.UI.Notifications;
using CHIP8_VM.Common;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace CHIP8_VM
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //private string[] games = { "BLINKY.ch8", "MAZE.ch8", "PONG.ch8" };
        NavigationHelper navHelper;
        public MainPage()
        {
            this.InitializeComponent();
            this.navHelper = new NavigationHelper(this);
            this.navHelper.LoadState += NavHelper_LoadState;
            this.navHelper.SaveState += NavHelper_SaveState;
        }

        private async void NavHelper_SaveState(object sender, SaveStateEventArgs e)
        { }

        private async void NavHelper_LoadState(object sender, LoadStateEventArgs e)
        { }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape | DisplayOrientations.Portrait;
            StatusBar stBar = StatusBar.GetForCurrentView();
            stBar.HideAsync().AsTask().Wait();
            
            var instFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var gamefld = instFolder.GetFolderAsync("Games").AsTask().Result;
            var gameFileList = gamefld.GetFilesAsync().AsTask().Result.Select((fl) => fl.Name).ToList();
            //fill combobox with games
            gameList.ItemsSource = gameFileList;
            gameList.SelectedItem = gameFileList[0];


            navHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navHelper.OnNavigatedFrom(e);
        }

        private async void disasmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (App.chosenProgram == null)
            {
                var mes = new Windows.UI.Popups.MessageDialog("Program is not selected");
                await mes.ShowAsync();
            }
            else
                Frame.Navigate(typeof(DisasmPage));
        }

        private void gameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.chosenProgram = (string)e.AddedItems.First();
        }

        private async void playBtn_Click(object sender, RoutedEventArgs e)
        {
            if (App.chosenProgram == null)
            {
                var mes  = new Windows.UI.Popups.MessageDialog("Program is not selected");
                await mes.ShowAsync();
            }
            else
            {
                Frame.Navigate(typeof(GamePage));
            }
        }
    }
}
