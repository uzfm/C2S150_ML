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





namespace C2S150_ML
{  // класс и его члены объявлены как public


    public class DataSV
    {

        private static string DirectoriPash;
        BinaryFormatter Format  = new BinaryFormatter();
        public SetingS  SetingS = new SetingS();


            public void DirectSave(string Pash){
                // Збереження строки в файл
                File.WriteAllText("SavePach.txt", Pash);
            }

            string DirectRead(){
            // Зчитування строки з файлу
            try {
            DirectoriPash = DirectoriPash = File.ReadAllText("SavePach.txt");
            }
            catch { }
            return DirectoriPash;
            }



        // Серелізація
        public void Serializ(){
            DirectRead();
            SetingS.SET();
            try
            {
                using (FileStream fs = new FileStream(DirectoriPash, FileMode.OpenOrCreate))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    Format.Serialize(fs, SetingS);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                }
                Console.WriteLine("Seriliz Bin OK ");
            }
             catch { Console.WriteLine("Seriliz Bin ERROR "); }
        }


        // десериализация
        public void Deserializ()
        {
            DirectRead();
            try
            {
             
                ///string PachSV = Properties.Settings.Default.PachStngRedSelect;
                using (FileStream fs = new FileStream(DirectoriPash, FileMode.OpenOrCreate))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    SetingS = (SetingS)Format.Deserialize(fs);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                }
                SetingS.READ();
                Console.WriteLine("DeSeriliz Bin OK ");
            }
            catch { Console.WriteLine("DeSeriliz Bin ERROR "); }
        }





    
    }

    public class SETS
    {

        public static DATA_Save Data = new DATA_Save();

        [Serializable()]
        public class DATA_Save{
            //для виривнюваня фону
            public int[] GreyScaleMax = new int[2] { 0, 0 };
            public int[] GreyScaleMin = new int[2] { 0, 0 };
            public double[] GreySizeMax = new double[2] { 0, 0 };
            public double[] GreySizeMin = new double[2] { 0, 0 };
            public bool  BlobsInvert  { get; set; }


            public bool LiveVideoOFF  { get; set; }
            public int  DoublingFlaps { get; set; }


            //GRAF ML
            public int LimitinGraphPoints { get; set; }
            public int UpdateVisibleArea  { get; set; }
            public int AxisYMaxValue      { get; set; }

            // Camera Getings
            public bool CameraAnalis_1 { get; set; }
            public bool CameraAnalis_2 { get; set; }
            public decimal GEIN1 { get; set; }
            public decimal GEIN2 { get; set; }
          
            public bool SetingsCameraStart { get; set; }  // Setings cameras if start program

            //Pash Simuletion IMG
            public string PashTestIMG { get; set; }

            // Camera Setings
            public bool LiveViewCam { get; set; }

        }
    }




    // ***Save Read/Write***//
    [Serializable()]
    public class SetingS
    {
        EMGU.DATA_Save    EMGUdata = new EMGU.DATA_Save();
        USB_HID.DATA_Save USB_HIDdata = new USB_HID.DATA_Save();
        SETS.DATA_Save DATA_Save = new SETS.DATA_Save();



        public void SET()
        {
            EMGUdata    = EMGU.Data;
            USB_HIDdata = USB_HID.Data;
            DATA_Save   = SETS.Data;
        }


        public void READ()
        {
            EMGU.Data    = EMGUdata;
            USB_HID.Data = USB_HIDdata;
            SETS.Data    = DATA_Save;
        }



    }






}