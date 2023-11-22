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
using System.Drawing.Printing;


namespace ShiftClockFaceDetect
{
    /// <summary>
    /// Clock in/out window, in this window you can clock in and out existing workers by looking at the camera.
    /// </summary>
    public sealed partial class ClockWindow : Page
    {
        public ObservableCollection<string> cams;
        private FilterInfoCollection fic;
        private Image<Bgr, Byte> bgrFrame = null;
        private Image<Gray, Byte> detectedFace = null;
        private FrameSource fsource;
        private CascadeClassifier cascadeClassifier;
        private string selectedcam = "";
        private DispatcherTimer frameTimer;
        private CascadeClassifier haarcascade = null;
        
        public ClockWindow()
        {
            // Setting all the cameras available and configuring the timer.
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
            startvid.Visibility = Visibility.Collapsed;
        }
        // Closes the video and resseting everything to default.
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
            }
            else
            {
                CloseVid();
            }
        }
        // Creating and showing a custom message box.
        private async void ShowError(string t, string c, string close)
        {
            CloseVid();
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
                        System.Drawing.Rectangle[] faces = haarcascade.DetectMultiScale(grayframe, 1.2, 10, new System.Drawing.Size(50, 50), new System.Drawing.Size(200, 200));
                        //We always want to detect and record one face at a time so if we got more than 1 face we need to stop the proccess.
                        if (faces.Length > 1)
                        {
                            ShowError("Too many faces", "Too many faces are found, please remove all unnecessary faces to proceed.", "Ok");
                            return;
                        }
                        //now we will set a rectangle around the face on the screen
                        foreach (System.Drawing.Rectangle face in faces)
                        {
                            bgrFrame.Draw(face, new Bgr(255, 255, 0), 2);
                            if (faces.Length == 1)
                            {
                                //clock the person in or out,make sure to close vid at the end
                                string w = DBManager.ClockInOut(grayframe.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic));
                                if (w.Equals("Undetected"))
                                {
                                    CvInvoke.PutText(bgrFrame, "Undetected", new Point(face.X-2, face.Y-2), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.8, new Bgr(0, 0, 255).MCvScalar, 1);
                                }
                                else
                                {
                                    ShowError("Success", w, "Ok");
                                    CloseVid();
                                }
                            }
                        }
                        showvid.Source = ImageToImageSource(bgrFrame);
                    }

                }
                catch (Exception excep) { }
            }
        }
    }
}

