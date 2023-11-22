using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShiftClockFaceDetect
{
    /// <summary>
    /// This window will be used to remove unnecessary workers.
    /// </summary>
    public sealed partial class RemoveWindow : Page
    {
        public ObservableCollection<string> workernames;
        public RemoveWindow()
        {
            this.InitializeComponent();
            //Getting all of the workers from DB
            List<string> wname = DBManager.GetAllWorkerNames();
            workernames = new ObservableCollection<string>();
            //Displaying all workers in dropdown button
            foreach (string w in wname)
            {
                workernames.Add(w);
            }
        }
        private void removebtn_Click(object sender, RoutedEventArgs e)
        {
            //Getting the worker you want to delete and deleting him from the DB
            string temp = PNames.SelectedItem.ToString().Split("-")[1];
            if (DBManager.RemoveWorker(Int32.Parse(temp))) {
                ShowError("Success", "Removing the person ended successfully.","Ok");
                PNames.SelectedItem = null;
            }
            else
            {
                //If an error occurred unexpectedly
                ShowError("Error", "Removing the person didn't end successfully.", "Ok");
            }

        }
        private async void ShowError(string t, string c, string close)
        {
            //Display a message box on screen.
            ContentDialog notfound = new ContentDialog
            {
                Title = t,
                Content = c,
                CloseButtonText = close
            };
            notfound.XamlRoot = PNames.XamlRoot;
            ContentDialogResult result = await notfound.ShowAsync();
        }
    }
}
