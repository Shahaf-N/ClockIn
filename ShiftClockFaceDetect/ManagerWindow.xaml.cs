using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShiftClockFaceDetect
{
    /// <summary>
    /// Can navigate from this window to all the manager operations.(If you get in to this window you have already entered the password).
    /// </summary>
    public sealed partial class ManagerWindow : Page
    {
        public ManagerWindow()
        {
            this.InitializeComponent();
        }
        // Displaying a side bar to navigate to the remove, add and report windows.
        private void navView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate(typeof(SettingWindow), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                string temp = args.SelectedItemContainer.Tag.ToString();
                Type navPageType = null;
                if (temp == "GetReport")
                    navPageType = typeof(HoursReport);
                else if (temp == "AddWindow")
                    navPageType = typeof(AddWindow);
                else
                    navPageType = typeof(RemoveWindow);
                NavView_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(Type navPageType, NavigationTransitionInfo transitionInfo)
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
    }
}
