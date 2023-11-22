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
using System.Drawing;
using System.Drawing.Printing;
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
    /// This page will make you a hours report from a specified month and year.
    /// </summary>
    public sealed partial class HoursReport : Page
    {
        public ObservableCollection<string> dbs;
        public HoursReport()
        {
            this.InitializeComponent();
            dbs = new ObservableCollection<string>();
            //Retrieving all the months that have been documented and displaying them on dropdown button.
            List<string> temp = DBManager.GetAllDBNames();
            foreach (string t in temp)
            {
                dbs.Add(t);
            }
        }
        private void savenprint_Click(object sender, RoutedEventArgs e)
        {
            // Getting all the a List of workers from the specified month of year that has been chosen.
            // Creating, saving and printing a pdf document that is containing all the information.
            string chosendb = DBNames.SelectedItem.ToString();
            List<Worker> workers = DBManager.GetAllSpecifiedDB(chosendb + ".db");
            string res = "Name(id) - Hours\n\n";
            foreach (Worker worker in workers)
            {
                double thours = worker.hoursmilli / (3600000);
                res += worker.name + "(" + worker.id + ")" + " - " + thours + "\n";
            }
            res += "\nClockIn system.";
            PrintDocument p = new PrintDocument();
            p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
            {
                e1.Graphics.DrawString(res, new Font("Times New Roman", 20), new SolidBrush(Color.Black), new RectangleF(0, 0, p.DefaultPageSettings.PrintableArea.Width, p.DefaultPageSettings.PrintableArea.Height));

            };
            try
            {
                p.Print();
            }
            catch (Exception ex)
            {
                ShowError("Error", "An error occurred while trying to print.","Ok");
            }
        }
        private async void ShowError(string t, string c, string close)
        {
            //Creating a custom message box.
            ContentDialog notfound = new ContentDialog
            {
                Title = t,
                Content = c,
                CloseButtonText = close
            };
            notfound.XamlRoot = DBNames.XamlRoot;
            ContentDialogResult result = await notfound.ShowAsync();
        }
    }
}
