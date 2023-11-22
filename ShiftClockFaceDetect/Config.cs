using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftClockFaceDetect
{
    public static class Config
    {
        public static string HaarCascadePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\..\\..\\Resources\\haarcascade_frontalface_default.xml"; // Location of the file that is used to recognize faces.
        public static int TimerResponseValue = 1; // Number is in milliseconds, decides how often we will take a new frame.
        public static string Passwd = "1234"; // Password used to login.
        public static string AppName = "ClockIn";   //App name will affect db name also
        public static long MaxShiftTime = 28800000; //In milliseconds(8 hours).
    }
}
