using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShiftClockFaceDetect
{
    // Communicate with the db(using LiteDB).
    internal static class DBManager
    {
        private static string ClockinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Config.AppName);
        private static string DBPath = Path.Combine(ClockinPath, "ClockInDB");
        private static string DBName = DateTime.Now.Month+"-"+DateTime.Now.Year+".db";
        private static EigenFaceRecognizer recognizer;

        public static void InitializeDB()
        {
            // Checking if the DB of the current month exists if so continue else creating a new one and if we had one before saving it and copying all the workers to the new one.
            if (!Directory.Exists(ClockinPath))
            {
                Directory.CreateDirectory(ClockinPath);
                Directory.CreateDirectory(DBPath);
                using (var db = new LiteDatabase(Path.Combine(DBPath, DBName))) { }
                return;
            }
            if (!File.Exists(Path.Combine(DBPath, DBName)))
            {
                if (File.Exists(Path.Combine(DBPath, GetPrevDBName())))
                {
                    CopyDBToDB(GetPrevDBName(), DBName);
                }
                else
                {
                    using (var db = new LiteDatabase(Path.Combine(DBPath, DBName))) { }
                }
            }
            using (var db = new LiteDatabase(Path.Combine(DBPath, DBName))){ }
        }
        // Return the name of the previous db.
        public static string GetPrevDBName()
        {
            if (DateTime.Now.Month==1)
                return 12 + "-" + (DateTime.Now.Year-1) + ".db";
            else 
                return (DateTime.Now.Month-1) + "-" + DateTime.Now.Year + ".db";
        }
        // Copying all the workers from the old to the new db.
        public static void CopyDBToDB(string from, string to)
        {
            List<Worker> pworkers = new List<Worker>();
            using (var db = new LiteDatabase(Path.Combine(DBPath, from)))
            {
                ILiteCollection<Worker> col = db.GetCollection<Worker>();
                List<Worker> cworkers = col.FindAll().ToList();
                foreach (Worker worker in cworkers)
                {
                    Worker temp = new Worker(worker.workerid, 0, worker.face, worker.name, new DateTime(worker.enterTime));
                    pworkers.Add(temp);
                }
            }
            using (var db = new LiteDatabase(Path.Combine(DBPath, to)))
            {
                ILiteCollection<Worker> col = db.GetCollection<Worker>();
                col.Insert(pworkers);
            }
        }
        // Adding a worker to the current db, before adding making sure that there is no 2 worker with the same id.
        public static string AddWorker(Worker w)
        {
            try
            {
                using (var db = new LiteDatabase(Path.Combine(DBPath, DBName)))
                {
                    ILiteCollection<Worker> col = db.GetCollection<Worker>();
                    
                    List<Worker> workers = col.FindAll().ToList();
                    foreach (Worker worker in col.FindAll().ToList())
                    {
                        if (worker.id == w.id)
                            return "There is already a worker with the same ID.";
                    }     
                    col.Insert(w);
                }
                return "Success";
            }
            catch
            {
                return "Adding failed, please try again.";
            }
        }
        // Return all the workers from the current db in List<Worker>.
        public static List<Worker> GetWorkerList()
        {
            using (var db = new LiteDatabase(Path.Combine(DBPath, DBName)))
            {
                ILiteCollection<Worker> col = db.GetCollection<Worker>();
                return col.Query().ToList();
            }  
        }
        // Returning all the old and new db names.
        public static List<string> GetAllDBNames()
        {
            List<string> result = new List<string>();
            string temp = DateTime.Now.Month + "-" + DateTime.Now.Year + ".db";
            while (File.Exists(Path.Combine(DBPath, temp)))
            {
                result.Add(temp.Substring(0, temp.Length - 3));
                string temp2 = temp.Substring(0,temp.Length-3);
                string []temp3 = temp2.Split('-');
                int tmonth = Int32.Parse(temp3[0]);
                int tyear = Int32.Parse(temp3[1]);
                if (tmonth == 1)
                    temp = 12 + "-" + (tyear - 1) + ".db";
                else
                    temp = (tmonth - 1) + "-" + tyear + ".db";
            }
            return result;
        }
        // Return a List of workers from the chosen db.
        public static List<Worker> GetAllSpecifiedDB(string tdb)
        {
            using (var db = new LiteDatabase(Path.Combine(DBPath, tdb)))
            {
                ILiteCollection<Worker> col = db.GetCollection<Worker>();
                return col.FindAll().ToList();
            }
        }
        // Getting all the workers names from the current db.
        public static List<string> GetAllWorkerNames()
        {
            using (var db = new LiteDatabase(Path.Combine(DBPath, DBName)))
            {
                ILiteCollection<Worker> col = db.GetCollection<Worker>();
                List<string> result = new List<string>();
                List<Worker> lw = col.FindAll().ToList();
                foreach (Worker worker in lw)
                {
                    result.Add(worker.name + "-" + worker.id);
                }
                return result;
            }
        }
        // Removing a worker from the current db by id.
        public static bool RemoveWorker(int id)
        {
            using (var db = new LiteDatabase(Path.Combine(DBPath, DBName)))
            {
                try
                {
                    ILiteCollection<Worker> col = db.GetCollection<Worker>();
                    List<Worker> temp = col.Query().Where(x => x.id == id).ToList();
                    foreach (Worker w in temp)
                    {
                        col.Delete(w.workerid);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        // Clocking in or out a worker depend on if he is working already or not.
        public static string ClockInOut(Image<Gray,Byte> person)
        {
            string result = "Undetected";
            try
            {
                using (var db = new LiteDatabase(Path.Combine(DBPath, DBName)))
                {
                    ILiteCollection<Worker> col = db.GetCollection<Worker>();
                    List<Worker> workers = col.FindAll().ToList();
                    if (workers.Count <= 0)
                    {
                        return result;
                    }
                    // Recognizing faces using eigenface method.
                    recognizer = new EigenFaceRecognizer(workers.Count);
                    VectorOfMat imageList = new VectorOfMat();
                    VectorOfInt indexList = new VectorOfInt();
                    int i = 0;
                    foreach (Worker worker in workers)
                    {
                        imageList.Push(worker.ConvertByteToImage().Mat);
                        indexList.Push(new[] { i++ });
                    }
                    CascadeClassifier haarCascade = new CascadeClassifier(Config.HaarCascadePath);
                    // Trainig the recognizer with each face.
                    recognizer.Train(imageList, indexList);
                    FaceRecognizer.PredictionResult res = recognizer.Predict(person);
                    if (workers[res.Label].enterTime == 1)
                    {
                        workers[res.Label].enterTime = (DateTime.Now).Ticks;
                        result = workers[res.Label].name + " clocked in.\nHave a good shift.";
                    }
                    else
                    {
                        long mtemp = Convert.ToInt64(DateTime.Now.Subtract(workers[res.Label].GetDateTime()).TotalMilliseconds);
                        workers[res.Label].enterTime = 1;
                        if(mtemp > Config.MaxShiftTime)
                        {
                            workers[res.Label].hoursmilli += Config.MaxShiftTime;
                            result = workers[res.Label].name + " clocked out.\nThank you for working with us.\n(You have clocked out more than maximum shift time so you have been only inserted the time of the max shift that is "+ Config.MaxShiftTime/ 3600000 + ")";
                        }
                        else
                        {
                            workers[res.Label].hoursmilli += mtemp;
                            result = workers[res.Label].name + " clocked out.\nThank you for working with us.";
                        }
                    }
                    col.Update(workers[res.Label]);
                }
                return result;
            }
            catch
            {
                return result;
            }
        }
    }
}
