using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShiftClockFaceDetect
{
    /// <summary>
    /// This page will prevent from others expect admins that knows the password to get in the admin tabs and do all of the admin operations.
    /// The password can be changed in all time at the Config.cs file
    /// </summary>
    public sealed partial class LoginWindow : Page
    {
        public LoginWindow()
        {
            this.InitializeComponent();
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (password.Password.Equals(Config.Passwd))
            {
                Frame.Navigate(typeof(ManagerWindow));
            }
            else
            {
                ContentDialog wrongpass = new ContentDialog
                {
                    Title = "Wrong password",
                    Content = "Wrong password, please try again",
                    CloseButtonText = "Close"
                };
                wrongpass.XamlRoot = password.XamlRoot;
                ContentDialogResult result = await wrongpass.ShowAsync();
            }
        }
    }
}
