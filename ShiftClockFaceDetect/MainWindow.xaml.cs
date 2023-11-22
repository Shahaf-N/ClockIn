using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using AForge.Video.DirectShow;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Superres;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ApplicationSettings;

namespace ShiftClockFaceDetect
{
    // Navigation window
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            DBManager.InitializeDB();
            DBManager.GetAllDBNames();
        }
        // Allow to navigate to clockin/out and manager window.
        private void navView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate(typeof(SettingWindow), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                string temp = args.SelectedItemContainer.Tag.ToString();
                Type navPageType = null;
                if (temp == "ClockWindow")
                    navPageType = typeof(ClockWindow);
                else
                    navPageType = typeof(LoginWindow);
                NavView_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate( Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = contentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                contentFrame.Navigate(navPageType, null, transitionInfo);
            }
        }
        public static void SwitchTheme(ElementTheme theme)
        {
            //((FrameworkElement)MainWindow.Content).RequestedTheme = theme;
        }
    }
}
