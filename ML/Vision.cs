using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.Util.TypeEnum;
using Emgu.Util;
using Emgu.CV.Util;
using System.Drawing;
using Emgu.CV.XObjdetect;
using Emgu.CV.Features2D;
using System.Windows.Forms;

namespace C2S150_ML
{
   public class VIS
    {
        public static DATA_Save Data = new DATA_Save();

        [Serializable()]
        public class DATA_Save
        {
            //для виривнюваня фону
         
            public byte  blurA      { get; set; }
            public byte  ThresholdA { get; set; }


            public byte blurB { get; set; }
            public byte ThresholdB { get; set; }
            public int ArcLengthB { get; set; }

            public int ArcLengthTest { get; set; }

        }

        static public bool ArcLengTestChecd = false;
        Image<Bgr, byte>   ColorBlobimg = new Image<Bgr, byte>(100, 100, new Bgr(255, 0, 0));
        VectorOfVectorOfPoint conturs = new VectorOfVectorOfPoint();


        public  Image<Bgr, byte>  DetectBlob(Mat img_Dtc, Label labelDectContur) {
       
            Mat ouput = new Mat();

            ////Середня фільтрація Blur
            CvInvoke.Blur(img_Dtc, ouput, new Size(Data.blurA, Data.blurA), new Point(-1, -1));
            var   _imgGry = ouput.ToImage<Gray, byte>();
            Image<Bgr, byte>  _img  = ouput.ToImage<Bgr, byte>();

            // Визначення середнього значення інтенсивності
            MCvScalar meanIntensity = CvInvoke.Mean(_imgGry);
            int Threshld = (int) (meanIntensity.V0);

            //___________________________________

            // Визначення мінімального та максимального значення інтенсивності
            double MinValue = 0.0;
            double MaxValue = 0.0;
            Point minLocation = new Point();
            Point maxLocation = new Point();

            // Виконання адаптивної порогової обробки
            // Визначення мінімального та максимального значення інтенсивності
            CvInvoke.MinMaxLoc(_imgGry, ref MinValue, ref MaxValue, ref minLocation, ref maxLocation);

            var test = 255/ (Threshld  - MinValue); //2.47
            var test2 = (Threshld - MinValue)/test; //41.6

            CvInvoke.Threshold(_imgGry, _imgGry, MinValue - Data.ThresholdA+test2, 255, ThresholdType.Binary);


              Mat hierarchy = new Mat(); 

              CvInvoke.FindContours(~_imgGry, conturs, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            //Test img
           ColorBlobimg = new Image<Bgr, byte>(img_Dtc.Width, img_Dtc.Height, new Bgr(10, 10, 10));

            // Фільтрування та відображення закритих контурів
            for (int i = 0; i < conturs.Size ; i++) {

                double perimeter = CvInvoke.ArcLength(conturs[i], true);

                // Якщо контур є закритим, вивести його шукаєм більш менш кругляшкі
                if (conturs[i].Size >= 1){
                
                    CvInvoke.DrawContours(ColorBlobimg, conturs, i, new MCvScalar(20, 20, 255), 1); //RED
                    labelDectContur.Text = "Detected";
                    labelDectContur.ForeColor = Color.Red;
                    return ColorBlobimg;
                }
                else {  CvInvoke.DrawContours(ColorBlobimg, conturs, -1, new MCvScalar(20, 255, 20), 1); //GEEN
                }
            }
                      labelDectContur.Text = "Not detected";
                      labelDectContur.ForeColor = Color.Green;
            return    ColorBlobimg;
        }



        public bool _DetectBlob(Mat img_Dtc)
        {
            Mat ouput = new Mat();

            ////Середня фільтрація Blur
            CvInvoke.Blur(img_Dtc, ouput, new Size(Data.blurA, Data.blurA), new Point(-1, -1));
            var _imgGry = ouput.ToImage<Gray, byte>();
            Image<Bgr, byte> _img = ouput.ToImage<Bgr, byte>();

            // Визначення середнього значення інтенсивності
            MCvScalar meanIntensity = CvInvoke.Mean(_imgGry);
            int Threshld = (int)(meanIntensity.V0);

            //___________________________________

            // Визначення мінімального та максимального значення інтенсивності
            double MinValue = 0.0;
            double MaxValue = 0.0;
            Point minLocation = new Point();
            Point maxLocation = new Point();

            // Виконання адаптивної порогової обробки
            // Визначення мінімального та максимального значення інтенсивності
            CvInvoke.MinMaxLoc(_imgGry, ref MinValue, ref MaxValue, ref minLocation, ref maxLocation);

            var test = 255 / (Threshld - MinValue); //2.47
            var test2 = (Threshld - MinValue) / test; //41.6

            CvInvoke.Threshold(_imgGry, _imgGry, MinValue - Data.ThresholdA + test2, 255, ThresholdType.Binary);
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(~_imgGry, conturs, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            //Test img
            ColorBlobimg = new Image<Bgr, byte>(img_Dtc.Width, img_Dtc.Height, new Bgr(10, 10, 10));



            // Фільтрування та відображення закритих контурів
            for (int i = 0; i < conturs.Size; i++){

                double perimeter = CvInvoke.ArcLength(conturs[i], true);
                if (conturs[i].Size >= 1)
                { 
                    return true;
                }
            }

            return false;
        }










        public bool _DetectBlobBlack(Mat img_Dtc)
        {
            Mat ouput = new Mat();

            ////Середня фільтрація Blur
            CvInvoke.Blur(img_Dtc, ouput, new Size(Data.blurB, Data.blurB), new Point(-1, -1));

            var _imgGry = ouput.ToImage<Gray, byte>();

            Image<Bgr, byte> _img = ouput.ToImage<Bgr, byte>();

            CvInvoke.Threshold(_imgGry, _imgGry, Data.ThresholdB, 255, ThresholdType.Binary);

            Mat hierarchy = new Mat();

            CvInvoke.FindContours(~_imgGry, conturs, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            // Фільтрування та відображення закритих контурів
            for (int i = 0; i < conturs.Size; i++)
            {
                double perimeter = CvInvoke.ArcLength(conturs[i], true);

                // Zise contur Check
                if ((perimeter >= Data.ArcLengthB)&&(!ArcLengTestChecd))
{
                    return true;
                }
                else {

                // Zise contur Check Test
                if ((perimeter >= Data.ArcLengthTest) && (ArcLengTestChecd))
                {

                    return true;
                } }

            }


            return false;
        }

        public Image<Bgr, byte> DetectBlobBlack(Mat img_Dtc, Label labelDectContur) {

            Mat ouput = new Mat();

            ////Середня фільтрація Blur
            CvInvoke.Blur(img_Dtc, ouput, new Size(Data.blurB, Data.blurB), new Point(-1, -1));

            var _imgGry = ouput.ToImage<Gray, byte>();

            Image<Bgr, byte> _img = ouput.ToImage<Bgr, byte>();

            CvInvoke.Threshold(_imgGry, _imgGry, Data.ThresholdB , 255, ThresholdType.Binary);

            Mat hierarchy = new Mat();

            CvInvoke.FindContours(~_imgGry, conturs, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            //Test img
            ColorBlobimg = new Image<Bgr, byte>(img_Dtc.Width, img_Dtc.Height, new Bgr(10, 10, 10));

            // Фільтрування та відображення закритих контурів
            for (int i = 0; i < conturs.Size; i++)
            {
                double perimeter = CvInvoke.ArcLength(conturs[i], true);
                //VectorOfPoint approx = new VectorOfPoint();
                // CvInvoke.ApproxPolyDP(conturs[i], approx, 0.05 * perimeter, true);

                // Zise contur Check
                if ((perimeter >= Data.ArcLengthB) && (!ArcLengTestChecd))
                {
                    CvInvoke.DrawContours(ColorBlobimg, conturs, i, new MCvScalar(20, 20, 255), 1); // RED 
                    labelDectContur.Text = "Detected";
                    labelDectContur.ForeColor = Color.Red;
                    return ColorBlobimg;
                }else{
                
                // Zise contur Check
                if ((perimeter >= Data.ArcLengthTest) && (ArcLengTestChecd))
                {
                    CvInvoke.DrawContours(ColorBlobimg, conturs, i, new MCvScalar(20, 20, 255), 1); // RED 
                    labelDectContur.Text = "Detected";
                    labelDectContur.ForeColor = Color.Red;
                    return ColorBlobimg;
                }}}



            CvInvoke.DrawContours(ColorBlobimg, conturs, -1, new MCvScalar(20, 255, 20), 1); // GRIN
            labelDectContur.Text = "Not detected";
            labelDectContur.ForeColor = Color.Green;
            return ColorBlobimg;
        }






































        public Image<Bgr, byte>[] DetectBlobTEST(Mat img_Dtc)
        {
            Mat ouput = new Mat();

            ////Середня фільтрація
            CvInvoke.Blur(img_Dtc, ouput, new Size(Data.blurA, Data.blurA), new Point(-1, -1));

            Image<Bgr, byte>[] _img = new Image<Bgr, byte>[2];
            _img[0] = new Image<Bgr, byte>(img_Dtc.Width, img_Dtc.Height);
            _img[1] = new Image<Bgr, byte>(img_Dtc.Width, img_Dtc.Height);

            var _imgGry = ouput.ToImage<Gray, byte>();
            _img[0] = ouput.ToImage<Bgr, byte>();


            //___________________________________


            // Тепер ви можете використовувати 'averageGradient' для подальших обчислень або візуалізації

            // Обчисліть середнє значення інтенсивності
            //  MCvScalar meanIntensity = CvInvoke.Mean(averageGradient);

            // Визначення середнього значення інтенсивності
            MCvScalar meanIntensity = CvInvoke.Mean(_imgGry);

            // Відображення результату
            //CvInvoke.Imshow("Average Gradient", averageGradient);
            //CvInvoke.WaitKey(0);

            int Threshld = (int)(meanIntensity.V0);

            //___________________________________

            // Визначення мінімального та максимального значення інтенсивності
            double MinValue = 0.0;
            double MaxValue = 0.0;
            Point minLocation = new Point();
            Point maxLocation = new Point();

            // Визначення мінімального та максимального значення інтенсивності
            CvInvoke.MinMaxLoc(_imgGry, ref MinValue, ref MaxValue, ref minLocation, ref maxLocation);

            var test = 255 / (Threshld - MinValue); //2.47
            var test2 = (Threshld - MinValue) / test; //41.6

            CvInvoke.Threshold(_imgGry, _imgGry, MinValue - Data.ThresholdA + test2, 255, ThresholdType.Binary);


            // Виконання адаптивної порогової обробки
            /*  */
            // CvInvoke.AdaptiveThreshold(_imgGry, _imgGry, 5, AdaptiveThresholdType.MeanC, ThresholdType.Binary, threshold, maxValue);

            Mat hierarchy = new Mat();
            CvInvoke.FindContours(~_imgGry, conturs, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);



            // Застосуйте алгоритм Кені для знаходження контурів
            //Mat cannyEdges = new Mat();
            //CvInvoke .Canny(_imgGry, cannyEdges, threshold, maxValue,l2Gradient:false,apertureSize: 3); // Параметри порогу
            // _img[1] = cannyEdges.ToImage<Bgr, byte>();

            ColorBlobimg = new Image<Bgr, byte>(img_Dtc.Width, img_Dtc.Height, new Bgr(10, 10, 10));

            // Фільтрування та відображення закритих контурів
            for (int i = 0; i < conturs.Size; i++)
            {
                double perimeter = CvInvoke.ArcLength(conturs[i], true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(conturs[i], approx, 0.04 * perimeter, true);

                // Якщо контур є закритим, вивести його шукаєм більш менш кругляшкі
                if (approx.Size >= 4)
                {
                    CvInvoke.DrawContours(ColorBlobimg, conturs, i, new MCvScalar(20, 20, 255), 1);
                }
                else { CvInvoke.DrawContours(ColorBlobimg, conturs, -1, new MCvScalar(20, 255, 20), 1); }
            }


            /**/


            _img[1] = ColorBlobimg;
            return _img;
        }

    }
}
