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
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Linq;

namespace ShiftClockFaceDetect
{
    /// <summary>
    /// This page will be used to add new workers.
    /// To add a worker you will need to picture him and enter his first name, last name and id.
    /// </summary>
    public sealed partial class AddWindow : Page
    {
        public ObservableCollection<string> cams;
        private FilterInfoCollection fic;
        private Image<Bgr, Byte> bgrFrame = null;
        private Image<Gray, Byte> tempdetectedFace = null;
        private Image<Gray, Byte> detectedFace = null;
        private FrameSource fsource;
        private CascadeClassifier cascadeClassifier;
        private string selectedcam = "";
        private DispatcherTimer frameTimer;
        private CascadeClassifier haarcascade = null;
        public AddWindow()
        {
            // Here we are making the application ready by adding all the cameras available to the drop down button. 
            // Setting the timer for between frames.
            this.InitializeComponent();
            fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            cams = new ObservableCollection<String>();
            foreach (FilterInfo fic2 in fic)
            {
                cams.Add(fic2.Name);
            }
            fsource = null;
            frameTimer = new DispatcherTimer();
            frameTimer.Interval = TimeSpan.FromMilliseconds(Config.TimerResponseValue);
            frameTimer.Tick += Device_NewFrame;
            captureperson.Visibility = Visibility.Collapsed;
            capturedpersongrid.Visibility = Visibility.Collapsed;
            startvid.Visibility = Visibility.Collapsed;
        }
        // Closing all the cameras and resseting it all.
        private void CloseVid()
        {
            if (frameTimer.IsEnabled)
            {
                frameTimer.Stop();
                fsource = null;
                showvid.Visibility = Visibility.Collapsed;
                selectedcam = null;
                startvid.Content = "Start";
                haarcascade = null;
                captureperson.Visibility = Visibility.Collapsed;
                startvid.Visibility = Visibility.Collapsed;
            }
            else
            {
                return;
            }
        }
        // Make sure that we have the camera that was chosen selected.
        private void ComboBox_ChangeCam(object sender, SelectionChangedEventArgs e)
        {
            selectedcam = e.AddedItems[0].ToString();
            if (!File.Exists(Config.HaarCascadePath))
            {
                ShowError("File Not Found", "Haarcascade file can't be found!", "Ok");
                CloseVid();
            }
            else
            {
                haarcascade = new CascadeClassifier(Config.HaarCascadePath);
                startvid.Visibility = Visibility.Visible;
            }
            CloseVid();
        }
        // Opening the camera and displaying a new frame every time the timer tick has we have setted before.
        private void CaptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (startvid.Content.Equals("Start") && selectedcam != null)
            {
                showvid.Visibility = Visibility.Visible;
                fsource = new FrameSource(camselect.SelectedIndex);
                startvid.Content = "Stop";
                frameTimer.Start();
                captureperson.Visibility = Visibility.Visible;
            }
            else
            {
                CloseVid();
            }
        }
        //Capturing the face and displaying it on the side in gray and white
        private void CapturePerson_Click(object sender, RoutedEventArgs e)
        {
            if (frameTimer.IsEnabled && HowManyFaces() == 1)
            {
                //Capture the person
                capturedpersongrid.Visibility = Visibility.Visible;
                showselected.Source = ImageToImageSource(tempdetectedFace);
                detectedFace = tempdetectedFace;
            }
            else
            {
                // If there is more than one face or 0 faces we can't capture.
                if (HowManyFaces() == 0)
                {
                    ShowError("Error using the camera!", "The Camera didn't detect any faces.", "Ok");
                }
                else
                {
                    ShowError("Error using the camera!", "Options:\n1.The camera detected too many faces (please remove all unwanted).\n2.The camera hasn't turned on right.\nPlease try again.", "Ok");
                }
                
            }
        }
        // Making sure that we have all the fields filled properly and adding the new worker to the DB.
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if(firstname.Text.Equals("") && lastname.Text.Equals(""))
            {
                ShowError("Person name", "The person's name hasn't been entered correctly.\nFirst and last name is missing!\nPlease try again.","Ok");
            }
            else if (firstname.Text.Equals(""))
            {
                ShowError("Person name", "The person's name hasn't been entered correctly.\nFirst name is missing!\nPlease try again.", "Ok");
            }
            else if (lastname.Text.Equals(""))
            {
                ShowError("Person name", "The person's name hasn't been entered correctly.\nLast name is missing!\nPlease try again.", "Ok");
            }
            else if (IdTextBox.Text.Length < 9)
            {
                ShowError("Person id", "The person's ID hasn't been entered correctly.\nID is to short!\nPlease try again.", "Ok");
            }
            else
            {
                if (!IsNameInProperForm(firstname.Text).Equals("ok"))
                {
                    ShowError("Person name", "The person's first name hasn't been entered correctly.\n" + IsNameInProperForm(firstname.Text) + "\nPlease try again.", "Ok");
                }
                else if (!IsNameInProperForm(lastname.Text).Equals("ok"))
                {
                    ShowError("Person name", "The person's last name hasn't been entered correctly.\n" + IsNameInProperForm(lastname.Text) + "\nPlease try again.", "Ok");
                }
                else
                {

                    Worker w = new Worker(detectedFace, firstname.Text + " " + lastname.Text, Int32.Parse(IdTextBox.Text));
                    string temp = DBManager.AddWorker(w);
                    if (temp.Equals("Success"))
                    {
                        ShowError("Success", "Adding succeeded.", "Ok");
                        CloseVid();
                    }
                    else
                    {
                        ShowError("Adding error", temp, "Ok");
                    }
                }
            }
        }
        private void TextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        //Checking that the name given (first/last) is going through all the requirements.
        private string IsNameInProperForm(string name)
        {
            if (!char.IsUpper(name[0]))
            {
                return "The name is not starting with an upper case letter!";
            }
            if(!Regex.IsMatch(name, @"^[a-zA-Z]+$"))
            {
                return "The name contains other character than alpabet!";
            }
            return "ok";
        }
        // Creating and showing a custom message box.
        private async void ShowError(string t, string c, string close)
        {
            ContentDialog notfound = new ContentDialog
            {
                Title = t,
                Content = c,
                CloseButtonText = close
            };
            notfound.XamlRoot = startvid.XamlRoot;
            ContentDialogResult result = await notfound.ShowAsync();
        }
        // Transforming the actual image into imagesource by saving it.
        private ImageSource ImageToImageSource(Image<Bgr, Byte> img)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                Bitmap bitmap = img.ToBitmap();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                bitmapImage.SetSource(stream.AsRandomAccessStream());
            }
            return bitmapImage;
        }
        private ImageSource ImageToImageSource(Image<Gray, Byte> img)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                Bitmap bitmap = img.ToBitmap();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                bitmapImage.SetSource(stream.AsRandomAccessStream());
            }
            return bitmapImage;
        }
        // Everty time the timer tick it will come here, take a new photo and displaying it on the screen.
        private void Device_NewFrame(object sender, object e)
        {
            //If the file that is used to recognize faces is missing we can't proceed.
            if (!File.Exists(Config.HaarCascadePath))
            {
                ShowError("File Not Found", "Haarcascade file can't be found!", "Ok");
                CloseVid();
            }
            else
            {
                try
                {
                    Mat img = new Mat();
                    // Capturing a frame from the chosen camera
                    fsource.NextFrame(img);
                    if (img != null)
                    {
                        //Transforming the image to gray frame and recognizing faces.
                        bgrFrame = img.ToImage<Bgr, Byte>();
                        Image<Gray, byte> grayframe = bgrFrame.Convert<Gray, byte>();
                        tempdetectedFace = grayframe;
                        System.Drawing.Rectangle[] faces = haarcascade.DetectMultiScale(grayframe, 1.2, 10, new System.Drawing.Size(50, 50), new System.Drawing.Size(200, 200));
                        //We always want to detect and record one face at a time so if we got more than 1 face we need to stop the proccess.
                        if (faces.Length > 1)
                        {
                            CloseVid();
                            ShowError("Too many faces", "Too many faces are found, please remove all unwanted faces.", "Ok");
                        }
                        //Drawing a rectangle around the face on the screen
                        foreach (System.Drawing.Rectangle face in faces)
                        {
                            bgrFrame.Draw(face, new Bgr(255, 255, 0), 2);
                        }
                        showvid.Source = ImageToImageSource(bgrFrame);
                    }

                }
                catch (Exception excep) { }
            }
        }
        // return how many faces there is in the variable tempdetectedFace, that is always updated to the current frame in gray and white.
        private int HowManyFaces()
        {
            System.Drawing.Rectangle[] faces = haarcascade.DetectMultiScale(tempdetectedFace, 1.2, 10, new System.Drawing.Size(50, 50), new System.Drawing.Size(200, 200));
            foreach (System.Drawing.Rectangle face in faces)
            {
                tempdetectedFace.Draw(face, new Gray(), 2);
            }
            return faces.Length;
        }
    }
}
