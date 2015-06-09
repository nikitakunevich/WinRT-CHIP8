using CHIP8_VM.Common;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CHIP8_VM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DisasmPage : Page
    {
        private NavigationHelper navigationHelper;
        public DisasmPage()
        {
            this.InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private async void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        { }

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        { }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var gameFile = Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Games")
                               .AsTask().Result.GetFileAsync(App.chosenProgram).AsTask().Result;
            var fileStream = gameFile.OpenReadAsync().AsTask().Result;
            asmView.Text = Utility.Disassembly.DisassembleToText(fileStream.AsStreamForRead());
            fileStream.Dispose();
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }


    }
}
