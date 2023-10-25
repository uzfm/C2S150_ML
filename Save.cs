using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using System.Linq.Expressions;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace C2S150_ML
{  




    public class SETS
    {

        public static Data_Serializable Data { get; set; } = new Data_Serializable();

        [Serializable()]
        public class Data_Serializable
        {

            public bool BlobsInvert { get; set; }


            public bool LiveVideoOFF   { get; set; }
            public int  DoublingFlaps  { get; set; }
            public bool ShowGoodMosaic { get; set; }

            //GRAF ML
            public int LimitinGraphPoints { get; set; }
            public int UpdateVisibleArea { get; set; }
            public int AxisYMaxValue { get; set; }

            // Camera Getings
            public bool CameraAnalis_1 { get; set; }
            public bool CameraAnalis_2 { get; set; }
            public decimal GEIN1 { get; set; }
            public decimal GEIN2 { get; set; }
            public bool AnalisLock { get; set; }
            //ACQ
            public decimal ACQGEIN1 { get; set; }
            public decimal ACQGEIN2 { get; set; }
            public string ACQ_Pach { get; set; }



            public bool SetingsCameraStart { get; set; }  // Setings cameras if start program

            //Pash Simuletion IMG
            public string PashTestIMG { get; set; }

            //ID (Master - Slave) Camera Setings
            public int ID_CAM { get; set; }



            public USB_HID.DATA_Save USB = new USB_HID.DATA_Save();
            public EMGU.DATA_Save EMGUDT = new EMGU.DATA_Save();
            public VIS.DATA_Save Vis = new VIS.DATA_Save();

            public  void SET(){
                 Data.USB    = USB_HID.Data;
                 Data.EMGUDT = EMGU.Data;
                 Data.Vis =    VIS.Data;
            }

            public void READ()
            {
                USB_HID.Data = Data.USB;
                EMGU.Data = Data.EMGUDT;
                VIS.Data  = Data.Vis;
            }

        }


        public bool Save()
        {
            string url = System.Windows.Forms.Application.StartupPath;

            if ((STGS.DT.SampleType != "")&&(STGS.DT.SampleType != null)) { url = Path.Combine(STGS.DT.URL_SampleType, STGS.DT.SampleType); }


            Data.SET();

            try
            {
                string filePath = Path.Combine(url, "SETINGS TYPE.json");

                string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Console.WriteLine("Serialize JSON OK");
                return true;
            }
            catch
            {
                Console.WriteLine("Serialize JSON ERROR");
                return false;
            }
        }


        public bool Read()
        {
            string url = System.Windows.Forms.Application.StartupPath;
            if ((STGS.DT.SampleType != "") && (STGS.DT.SampleType != null)) { url = Path.Combine(STGS.DT.URL_SampleType, STGS.DT.SampleType); }
            try
            {
                string filePath = Path.Combine(url, "SETINGS TYPE.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Data = JsonConvert.DeserializeObject<Data_Serializable>(json);
                    Data.READ();
                   

                    Console.WriteLine("Deserialize JSON OK");
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("Deserialize JSON ERROR");
            }

            return false;
        }
        public bool Read( string SampleType )
        {
            string url = System.Windows.Forms.Application.StartupPath;
            if ((SampleType != "") ) { url = Path.Combine(STGS.DT.URL_SampleType, SampleType); }
            try
            {
                string filePath = Path.Combine(url, "SETINGS TYPE.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Data = JsonConvert.DeserializeObject<Data_Serializable>(json);
                    Data.READ();


                    Console.WriteLine("Deserialize JSON OK");
                    return true;
                }
                else
                {
                   
                }
            }
            catch
            {
                Console.WriteLine("Deserialize JSON ERROR");
            }

            return false;
        }


    }





    //____________________________________________ JSON  _______________________________________________________________-

    [Serializable()]
    public class Data
    {
        public string URL_SampleType = Path.Combine(@"../../../", "Sample Type");
        public string     SampleType { get; set; } // шлях для Моделі
        public string Password { get; set; }
    }

    class STGS
    {
        static public Data DT = new Data();

        private const string FileName = "settings.json";

        public bool Save()
        {
            string url = System.Windows.Forms.Application.StartupPath;

            try
            {
                string filePath = Path.Combine(url, FileName);

                string json = JsonConvert.SerializeObject(DT, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Console.WriteLine("Serialize JSON OK");
                return true;
            }
            catch
            {
                  Console.WriteLine("Serialize JSON ERROR");
                return false;
            }
        }

        public bool Read()
        {
            string url = System.Windows.Forms.Application.StartupPath;
            try
            {
                string filePath = Path.Combine(url, FileName);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    DT = JsonConvert.DeserializeObject<Data>(json);

                    Console.WriteLine("Deserialize JSON OK");
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("Deserialize JSON ERROR");
            }

            return false;
        }
    }


}