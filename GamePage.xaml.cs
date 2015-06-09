using CHIP8_VM.Common;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CHIP8_VM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        Chip8 chip8;
        Color systemAccentColor = Colors.Green;//(Color)Application.Current.Resources["PhoneAccentColor"];
        NavigationHelper navHelper;
        public GamePage()
        {
            this.InitializeComponent();
            navHelper = new NavigationHelper(this);
            navHelper.LoadState += NavHelper_LoadState;
            navHelper.SaveState += NavHelper_SaveState;

        }

        private async void NavHelper_SaveState(object sender, SaveStateEventArgs e)
        {  }

        private async void NavHelper_LoadState(object sender, LoadStateEventArgs e)
        { }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped;
            var stream = Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Games").AsTask().Result.GetFileAsync(App.chosenProgram).AsTask().Result.OpenReadAsync().AsTask().Result;

            chip8 = new Chip8();
            chip8.dotSize = (int)(canvas.Height / Chip8.screenH);
            chip8.mediaElement = mediaElemt;
            chip8.InitVM(stream.AsStreamForRead());
            navHelper.OnNavigatedTo(e);
            await Task.Delay(1000);
            chip8.State = VMState.Running;

        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navHelper.OnNavigatedFrom(e);
        }
        public void Canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            chip8.displayCanvas = sender;
            chip8.displayArgs = args;
            chip8.Update();
        }

        private void GameBtn_Press(object sender, object e)
        {
            var btn = sender as Button;
            int result;
            if (!int.TryParse(btn.Content.ToString(), out result))
                throw new Exception("Wrong button content");

            if (chip8.State == VMState.WaitForKey)
                chip8.KeyPressed(result);
            chip8.key[result] = true;
        }

        private void GameBtn_Release(object sender, object e)
        {
            var btn = sender as Button;
            int result;
            if (!int.TryParse(btn.Content.ToString(), out result))
                throw new Exception("Wrong button content");

            chip8.key[result] = false;
        }

    }
}
