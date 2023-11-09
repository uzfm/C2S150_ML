
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV;
//using Emgu.CV.Shape;
//using Emgu.Util;
//using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Emgu.CV.ImgHash;
using Emgu.Util.TypeEnum;
using System.Collections.Concurrent;
using Emgu.CV.Stitching;
using Emgu.CV.Cuda;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Tensorflow.NumPy;
using Tensorflow;
using Tensorflow.Keras.Engine;

namespace C2S150_ML
{
    class FlowAnalis
    {



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //==============================                                 ВИЗНАЧАЄМ ТА МАЛЮЄМО КОНТУРИ з FOTO                                                      ==============================//
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public delegate void Mosaics(DTLimg dTLimg);
        //static public event Mosaics MosaicaEvent;
        // public static ConcurrentQueue<Img> Box = new ConcurrentQueue<Img>();
        public static bool QualityRecognition = false;

        public static bool SelectionContamination = false;
        public const int MaxImageSave = 1000; //максимальна кількість фоток  яку можна додавати в "List" та зберігати та відтворювати в симуляторі
        public static bool Setings = false;  //признак симуляції 
        public  int Count_Contur = new int();
       


       static public bool Flapslocking = false;


        private  Image<Bgr, byte>[] ImagContact = new Image<Bgr, byte>[2];
        public int[] CountContact = new int[2] { 0, 0 };



      






 



        public void resetValue()
        {
            ImagContact[0] = null;
            ImagContact[1] = null;
            CountContact[0] = 0;
            CountContact[1] = 0;

            EMGU.ListMast.Clear();
            EMGU.ListSlav.Clear();
        }

















        static int[] OUTPUT_BIT1 = new int[3];
        static int[] OUTPUT_BIT2 = new int[3];   //зроблено для спрацювання трьох електоро тяг (коли боб падає між двома лопатками)
        //public static bool    StartAnais = false;






        ML ml = new ML();

        public void FindBlob()
        {


               Image<Gray, byte> ImagAI = new Image<Gray, byte>(100, 100);

            while (true)
            {


                if ((FlowCamera.BoxM.Count) != 0)
                {



                    FlowCamera.BoxM.TryDequeue(out ImagAI);

                    if (ImagAI != null)
                    {
                        try {

                        Int16 ID = 0;
                        List<Mat> ImgsPredict = new List<Mat>();
                        List<Mat> ImgsMosaic = new List<Mat>();



                        string[] ROILeng = new string[2];
                        int ListCutCunt = 0;

                        int OrigWidth = ImagAI.Width;
                        int OrigHeight = ImagAI.Height;

                        CutImages CutImage;

                        bool[] DT_Cnthn = new bool[2];

                        Image<Gray, byte> img_Camera = ImagAI.Clone();
                        Image<Gray, byte> imgROI = ImagAI.Clone();


                        // КОНТУР ДЛЯ ВИРІЗАННЯ ДЛЯ ПОДАЛЬШОГО АНАЛІЗУ
                        //створюємо пустий контур

                

                            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                            Mat ouput = new Mat();
                            ////Середня фільтрація
                            // CvInvoke.Blur(img_Camera, ouput, new Size(20, 20), new Point(-1, -1));
                            Image<Gray, byte> _img = new Image<Gray, byte>(img_Camera.Width, img_Camera.Height);
                            _img = img_Camera;
                            _img = _img.Convert<Gray, byte>().ThresholdBinary(new Gray(EMGU.Data.GreyScaleMin[ID]), new Gray(EMGU.Data.GreyScaleMax[ID]));

                            Mat hierarchy = new Mat();
                            //CvInvoke.FindContours(_img.Mat, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
                             CvInvoke.FindContours(_img.Mat, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);



                            ////////////////  створити ліст з описом найдених конторів   ///////////
                            int CountFindContur = (int)contours.Size;

                            if ((CountFindContur > 1)) { }


                            //************************** ПЕРЕБИРАЄМ УСІ ЗНАЙДЕНІ КОНТУРИ ДЛЯ ІНДИФІКАЦІЇ ДЕФЕКТІВ   **************************************************//
                            for (Count_Contur = 0; Count_Contur < CountFindContur; Count_Contur++)
                            {
                                double temp = CvInvoke.ContourArea(contours[Count_Contur]);




                                // ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
                                if ((temp >= EMGU.Data.GreySizeMin[ID]) && (temp < EMGU.Data.GreySizeMax[ID]))
                                {
                                    Rectangle boxROI = CvInvoke.BoundingRectangle(contours[Count_Contur]);
                                    imgROI.ROI = boxROI;

                                    ImgsMosaic.Add(imgROI.Resize(100, 100, Inter.Cubic).Mat);
                                    ImgsPredict.Add(imgROI.Resize(32, 32, Inter.Cubic).Mat);

                                    ListCutCunt++;

                                }



                            }


                            Stopwatch watch = Stopwatch.StartNew();
                            if (ml.model == null)
                            {
                                //Model model = ml.ReadModel();
                                ml.InstModel(Path.Combine(Application.StartupPath, "Data"));
                            }



                            watch.Stop();
                            var elapsedMs = watch.ElapsedMilliseconds;

                            if (ImgsPredict.Count != 0)
                            {
                                try {
                                var Predict = ml.PredictImage(ImgsPredict);

                                //  Task.Run(() => ml.PredictImage(ImgsPredict) );


                                Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

                                int idxRz = 0;
                                foreach (var pred in Predict.numpy())
                                {

                                    //var numpyArray = value[0].numpy();
                                    var class_index = np.argmax(pred);

                                    if ((int)class_index == 0)
                                    {
                                        DTLimg DTLimg = new DTLimg();
                                        DTLimg.Img = ImgsMosaic[idxRz].ToImage<Gray, byte>();
                                       // MosaicaEvent(DTLimg);
                                    }

                                    idxRz++;
                                }
 } catch { }


                            }



                             } catch { }


                        }
                    
                }
            }
                       
        } //*******************************************************









   
    
    
    }
    
}
