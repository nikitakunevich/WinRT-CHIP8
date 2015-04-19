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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace CHIP8_VM
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }

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

            /*
            Utility.Disassembly dsm = new Utility.Disassembly();
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = folder.GetFolderAsync("Games").AsTask().Result;
            var file = folder.GetFileAsync("BRIX.ch8").AsTask().Result;
            var stream = dsm.Disassemble(file.OpenReadAsync().AsTask().Result.AsStreamForRead());
            byte[] test = new byte[11];
            byte[] buf = new byte[8096];
            int unreadBytesCount = buf.Length;
            stream.Read(buf, 0, buf.Length);
            disasmText.Text = System.Text.UTF8Encoding.UTF8.GetString(buf, 0, buf.Length);*/
        }

        private void CanvasControl_Draw(Microsoft.Graphics.Canvas.CanvasControl sender, Microsoft.Graphics.Canvas.CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            var rnd = new Random();
            var clrs = new Color[]{Colors.Black, Colors.White};
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    session.FillRectangle(i * 8, j * 8, 8, 8, clrs[(int)Math.Round(rnd.NextDouble())]);
                }
            }
            sender.Invalidate();
        }
    }
}
