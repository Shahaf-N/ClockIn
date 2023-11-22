using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace ShiftClockFaceDetect
{
    // Saves all the worker information.
    class Worker
    {
        [BsonId] public ObjectId workerid { get; set; }
        public long hoursmilli { get; set; } //hours represented in milliseconds
        public byte[] face { get; set; }
        public string name { get; set; }
        public long enterTime { get; set; }
        public int id { get; set; }
        public Worker()
        {
            
        }
        public Worker(Image<Gray, byte> face, string name,int id)
        {
            this.workerid = ObjectId.NewObjectId();
            this.hoursmilli = 0;
            this.face = face.Resize(100,100, Emgu.CV.CvEnum.Inter.Cubic).Bytes;
            this.name = name;
            this.enterTime = 1;
            this.id = id;
        }
        [BsonCtor]
        public Worker(ObjectId workerid, long hours, byte[] face, string name, long dateTime, int id)
        {
            this.workerid = workerid;
            this.hoursmilli = hours;
            this.face = face;
            this.name = name;
            this.enterTime = dateTime;
            this.id = id;
        }
        public Worker(ObjectId workerid, long hours, byte[] face,string name,DateTime dateTime)
        {
            this.workerid = workerid;
            this.hoursmilli = hours;
            this.face = face;
            this.name = name;
            this.enterTime = dateTime.ToBinary();
        }

        public Image<Gray, byte> ConvertByteToImage()
        {
            Image<Gray, byte> res = new Image<Gray, byte>(100, 100);
            res.Bytes = face;
            return res;
        }
        public DateTime GetDateTime()
        {
            return new DateTime(enterTime);
        }
    }
}
