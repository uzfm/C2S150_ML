//using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
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


//using Emgu.CV.Dnn;
//using Emgu.CV.UI;
//using Emgu.CV.Util;
//using Emgu.CV.CvEnum;



using System.Drawing.Imaging;
using System.Diagnostics;

namespace C2S150_ML
{


   public class EMGU
    {

        //  HID HID = new HID();
      public static DATA_Save Data = new DATA_Save();


        [Serializable()]
        public class DATA_Save{

            //для виривнюваня фону
            public     int[] GreyScaleMax = new int[2] { 0, 0 };
            public    int [] GreyScaleMin  = new int[2] { 0, 0 };
            public  double[] GreySizeMax = new double[2] { 0, 0 };
            public  double[] GreySizeMin = new double[2] { 0, 0 };

        }


        static public List<Image<Bgr, byte>> ListMast = new List <Image<Bgr, byte>>();
        static public List<Image<Bgr, byte>> ListSlav = new List<Image<Bgr, byte>>();
        static public ImageList[]            MosaicsAnalis = new ImageList[2];



        public Bitmap ToHSV(Bitmap img) {
            Mat imgHSV = new Mat(100, 100, DepthType.Cv16U, 1);
            CvInvoke.CvtColor(img.ToImage<Rgb, byte>(), imgHSV, ColorConversion.Rgb2Hsv);
            return imgHSV.ToBitmap();
        }

        public static void InstMosaics(){
            MosaicsAnalis[Master] = new ImageList();
            MosaicsAnalis[Slave] = new ImageList();

        }


        public delegate void Mosaics(int ID, Bitmap Imag);
        //static public event Mosaics Mosaica;
        static public bool[,] AnalysisMode = new bool[2, 5] { { false, false, false, false, false }, { false, false, false, false, false } };
        public const int Master = 0;
        public const int Slave = 1;
        VideoCapture capture = null;

        //============================== подія захвата кадра ====================================//
        private void Capture_ImageGrabbed(Mat mat1)
        {
            Mat mat = new Mat();
            capture.Retrieve(mat);
            Image<Rgb, byte> test = mat.ToImage<Rgb, byte>().Flip(FlipType.Horizontal);

            FindContur(test);
        }
        //======================================================================================//


        //==============================ВИЗНАЧАЄМ ТА МАЛЮЄМО КОНТУРИ з відео потоку ==============================//
        public Bitmap FindContur(Image<Rgb, byte> img)
        {
            //переводим в чорно білу картинку з певним затемненням
            Image<Gray, byte> btmaptest = img.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            //створюємо пустий контур
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarhy = new Mat();
            //шукаємо контури
            CvInvoke.FindContours(btmaptest, contours, hierarhy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            // малюємо знайдені контури
            CvInvoke.DrawContours(img, contours, -1, new MCvScalar(10000, 1, 2), 1, LineType.FourConnected);
            //вивисти картинку на екран

            return img.ToBitmap();
            //  panAndZoomPictureBox1.Image = img.AsBitmap();
        }
        //========================================================================================================//




        //============================== Загрузити Image для подальшоъ обробки ==============================//
        static public Image<Bgr, byte>[] Origenal_img_Camera  = new Image<Bgr, byte>[2];
        static public Image<Bgr, byte>[] Origenal_Img_Mosaics = new Image<Bgr, byte>[2];

        //public static double[,] ColorMaxB = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
        //public static double[,] ColorMaxG = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
        //public static double[,] ColorMaxR = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };

        //public static double[,] ColorMinB = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
        //public static double[,] ColorMinG = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
        //public static double[,] ColorMinR = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
        public static Color[,] ColorMax { get; set; } = new Color[2, 5];
        public static Color[,] ColorMin { get; set; } = new Color[2, 5];


        // public static  int  SetingsID=0;
        public static int[] SetingsID = new int[2];

        public static int [] GreyScale = new int[2] { 0, 0 };
        public static double[] GreyMax  = new double[2]  { 0, 0 };
        public static double[] GreyMin = new double[2]  { 0, 0 };

        public static double[,] Max_Contur = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
        public static double[,] Min_Contur = new double[2, 5] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };


        public static bool[] SelectConturs = new bool[2] { false, false }; // дозволяє виділити глоби




        ///static public Image<Bgr, byte> Origenal_img;
           public Bitmap Origenal_Image(int ID) {  try { return Origenal_img_Camera[ID].Mat.ToBitmap(); } catch {  Help.Mesag(" Original image not found "); } return null; }
           public void Load_Image(Bitmap img, int ID) {  Origenal_img_Camera[ID] =  img.ToImage<Bgr, byte>(); }  // (new ToBitmap(img)); 
           public void Load_Image_Mosaics(Bitmap img, int ID) { Origenal_Img_Mosaics[ID] = img.ToImage<Bgr, byte>(); }








        public Bitmap FilterColourGry (int ID, Bitmap img ){


            Image<Bgr, Byte> Image  =  img.ToImage<Bgr, byte>();


            Image<Gray, Byte> GrayImg = Image.Convert<Gray, Byte>();

             GrayImg = Image.Convert<Gray, byte>().ThresholdBinary(new Gray(EMGU.ColorMin[ID, 0].R), new Gray(EMGU.ColorMax[ID, 0].R));

            return GrayImg.Mat.ToBitmap();
        }


        //***********************************************************************//
        public Bitmap ResivColBackgMin(int ID)
        {
            if (Origenal_img_Camera[ID] != null)
            {
                Image<Bgr, byte> Background = new Image<Bgr, byte>(100, 100, 
                    new Bgr(ColorMin[ID, SetingsID[ID]].B, ColorMin[ID, SetingsID[ID]].G, ColorMin[ID, SetingsID[ID]].R));
                return Background.Mat.ToBitmap();
            }
            return null;
        }
        public Bitmap ResivColBackgMax(int ID)
        {
            if (Origenal_img_Camera[ID] != null)
            {
                Image<Bgr, byte> Background = new Image<Bgr, byte>(100, 100, 
                    new Bgr(ColorMax[ID, SetingsID[ID]].B, ColorMax[ID, SetingsID[ID]].G, ColorMax[ID, SetingsID[ID]].R));
                return Background.Mat.ToBitmap();
            }
            return null;
        }





        public Bitmap ContactImags(Bitmap ImagM, Bitmap ImagS){
            Mat NewMat = new Mat();
            Image<Bgr, byte> imgM = ImagM.ToImage<Bgr, byte>();
            Image<Bgr, byte> imgS = ImagS.ToImage<Bgr, byte>();
            CvInvoke. HConcat(imgS, imgM, NewMat);
            //  Image<Bgr, byte> img = new Image<Bgr, byte>(NewMat.Bitmap);
            // CvInvoke.HConcat(ContIMG, img, NewMat);
            return NewMat.ToBitmap();
        }



        public Image<Bgr, byte> ContactImags(Image<Bgr, byte> ImagM, Image<Bgr, byte> ImagS)
        {
            Mat NewMat = new Mat();
            Image<Bgr, byte> imgM = ImagM;
            Image<Bgr, byte> imgS = ImagS;
            CvInvoke.HConcat(imgS, imgM, NewMat);
            //  Image<Bgr, byte> img = new Image<Bgr, byte>(NewMat.Bitmap);
            // CvInvoke.HConcat(ContIMG, img, NewMat);
            return NewMat.ToImage<Bgr, byte>();
        }



        //========================================================================================//
        /// <summary>

        //теcтуваня та візуалізація границь лопатока для сортування 
        public Bitmap SetingsSeparation(int Output,int ID)
        {
            MCvScalar drawingColor = new Bgr(Color.Green).MCvScalar;
            Image<Bgr, byte> img = new Image<Bgr, byte>(1700, 400);
            if (Origenal_img_Camera[0] != null)
            {
                img = new Image<Bgr, byte>(Origenal_img_Camera[Master].Data);
            }



            int Shoulder = ((Fleps.Aperture[ID] - (Fleps.Backdown[ID] * 2)) / Fleps.ShoulderPic[ID]);
            int X = Fleps.Backdown[ID];

            Rectangle Rect = new Rectangle(X, 0, Shoulder, Heigh);

            if (0 == Output) { CvInvoke.Rectangle(img, Rect, drawingColor, -100); }
            else { CvInvoke.Rectangle(img, Rect, drawingColor, 2); }

            for (int i = 1; i < Fleps.ShoulderPic[ID]; i++)
            {
                if (i == Output)
                {
                    Rect = new Rectangle(X = Shoulder + X, 0, Shoulder, Heigh);
                    CvInvoke.Rectangle(img, Rect, drawingColor, -100);
                }
                else
                {
                    Rect = new Rectangle(X = Shoulder + X, 0, Shoulder, Heigh);
                    CvInvoke.Rectangle(img, Rect, drawingColor, 2);
                }
            }
            return img.ToBitmap();
        }



        public Bitmap SetingsLineDetect(int Line, int ID) {
            int X = Fleps.Backdown[ID];
            Rectangle Rect = new Rectangle(0,0, 0, 0);

            MCvScalar drawingColor = new Bgr(Color.Red).MCvScalar;
            Image<Bgr, byte> img = new Image<Bgr, byte>(1700, 400);
            if (Origenal_img_Camera[0] != null)
            {
                img = new Image<Bgr, byte>(Origenal_img_Camera[ID].Data);
            }

            Rect = new Rectangle(0, Line, 3000, 0);
            CvInvoke.Rectangle(img, Rect, drawingColor, 2);

            return img.ToBitmap();
        }

        //Green img
        public Bitmap bgrImgGreen(Bitmap img)
        {

            Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(img.Width,img.Height, new Bgr(0, 255, 0));

            return img1.ToBitmap();
        }

        //Визначення кaналу для відправки на контролер
        public int SeparationChenal(int Position, int ID)
        {

            int Shoulder = ((Fleps.Aperture[ID] - (Fleps.Backdown[ID] * 2)) / Fleps.ShoulderPic[ID]);
            int X = Fleps.Backdown[ID];

            int Output = 0;

            double IdxPosition = (Shoulder + Position) / Shoulder;

            Output = (int)IdxPosition;

            MCvScalar drawingColor = new Bgr(Color.Green).MCvScalar;
            Image<Bgr, byte> img = new Image<Bgr, byte>(Origenal_img_Camera[Master].Data);
            Rectangle Rect = new Rectangle(X, 0, Shoulder, Heigh);

            if (0 == Output) { CvInvoke.Rectangle(img, Rect, drawingColor, -100); }
            else { CvInvoke.Rectangle(img, Rect, drawingColor, 2); }
            for (int i = 1; i < Fleps.ShoulderPic[ID]; i++)
            {
                if (i == Output)
                {
                    Rect = new Rectangle(X = Shoulder + X, 0, Shoulder, Heigh);
                    CvInvoke.Rectangle(img, Rect, drawingColor, -100);
                }
                else
                {
                    Rect = new Rectangle(X = Shoulder + X, 0, Shoulder, Heigh);
                    CvInvoke.Rectangle(img, Rect, drawingColor, 2);
                }
            }
            return 0;
        }

        static Image<Bgr, byte> ImgPosition;


        //Визначення кaналу для відправки на контролер

        static public int[] SeparationChenalTest(int ID, int Position, int Length, bool RES)
        {
            int[] Output = new int[3];
            try
            {  

                int Shoulder = ((Fleps.Aperture[ID] - (Fleps.Backdown[ID] * 2)) / Fleps.ShoulderPic[ID]); //ширини однієї лопаткі

                int X = Fleps.Backdown[ID];

                // для двох флепсів
                ///double IdxPosition = (Position +  Length  - (Backdown)) / (Shoulder);
                         Output[0] = (Position + (Length) - (Fleps.Backdown[ID])) / (Shoulder);                                                               
                         Output[1] = (Position + (Length) - (Fleps.Backdown[ID])) % (Shoulder); //-> зміщення лопатки в право 

                if ((Output[1] > ((Shoulder / 2) + Fleps.DoublingFlaps[ID]))  || (Output[1] < ((Shoulder / 2) - Fleps.DoublingFlaps[ID]))) {
                if ( Output[1]> ((Shoulder/2) ))      { Output[1] = Output[0] +1;  }else
                                                            { Output[1] = Output[0] -1;  }
                }
                if (Output[1] > 14)
                { Output[1] = Output[0]; }

                //Output[0] = (Position + (Length - (DoublingFlaps)) - (Backdown*2)) / (Shoulder); //<- зміщення лопатки в ліво 
                if (Output[0] > 14) 
                { Output[0] = Output[0]=0; }
                // для трьох флепсів
                //Output[0] = (Position + ((Length / 2) +  Shoulder) - (Backdown)) / (Shoulder);
                //Output[1] = (Position + ( Length / 2) - (Backdown)) / (Shoulder);
                //Output[2] = (Position + ((Length / 2) -  Shoulder) - (Backdown)) / (Shoulder);


                Output[0]++;
                Output[1]++;
                //Output[2]++;
            }
            catch
            {
                Output[0]=0;
                Output[1]=0;
                Output[2]=0;
                Help.Mesag(" Check parameters (Flaps Settings)");
                return Output;
            } 
                return Output;

        }




        public static int[] Count_Contur = new int[2];
      static  Image<Bgr, byte>[] ImagContact = new Image<Bgr, byte>[2];



        static Image<Bgr, byte> ImagContactM  ;
        static Image<Bgr, byte> ImagContactS;


        static int[,,] LocationImg_1 = new int[2, 20, 3]; // ID ,H,W
        static int[,,] LocationImg_2 = new int[2, 20, 3]; // ID ,H,W
        static byte[,] LocationImgIDX = new byte[2, 2];

        public static bool StartAnalis  = false;    // синхронізація і початок аналізу

        private static Image<Bgr, byte>[] imgROI = new Image<Bgr, byte>[2];      //вирізання img
      //  private static int             [] imgCount = new int[2] { 0, 0 };        //підраховуємо кадри для синхронізації
        private static int             [] PeletsCount = new int[2] { 0, 0 };     //підраховуємо частинок для синхронізації
      //  static int                     [] ContName = new int[2];

        static Rectangle  box = new Rectangle();




        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //==============================                                 ВИЗНАЧАЄМ ТА МАЛЮЄМО КОНТУРИ з FOTO                                                      ==============================//
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


     public   struct  Fleps {
        static public int [] Aperture      = new int[2] { 1792,1792 };  //  8192; //1792; // 896;   //Ширина видимості поля сортування в (пікселях по осі (Х))
        static public int [] ShoulderPic   = new int[2] { 14, 14 };     //Кількість робочих лопаток 1-10шт
        static public int [] Backdown      = new int[2] { 0, 0 };     //Відступ від країв видимості ширини поля в (пікселях по осі(Х))
        static public int [] DoublingFlaps = new int[2] { 100, 100 };  // 100;   //ширина лопатки + (ширина семпла) після якої вмикаються дві лопатки (800/14)=(ширина камери/кількість лопаток)
        //=========================================================================================================//
     } ;

        public const  int  Heigh      = 20;  // 100;   //Висота малювання флепса











        #region BackgroundSetings

        public Image<Bgr, byte> WriteTextINimg(Image<Bgr, byte> img, string Text, Rectangle ROI, Color colour)
        {

            //   public Rectangle[] ROI { get; set; }


            // Write information next to marked object створити точку в центрі для запису інформації
            Point center = new Point(ROI.X + ROI.Width / 2, ROI.Y + ROI.Height / 2);


            //створюєм інформацію для запису
            var info = new string[] { Text, $"Position: {center.X}, {center.Y}" };
            // for (int i = 0; i < lines.Length; i++)
            // {
            //  int y = i * 10 + origin.Y; // Moving down on each line

         // створюємо колір
            MCvScalar drawingColor = new Bgr(colour).MCvScalar;

         // записуєм строчку на вирізаний малюнок
            CvInvoke.PutText(img, info[0], center, FontFace.HersheyPlain, 1, drawingColor, 1);
         // CvInvoke.PutText(img, "?????", new System.Drawing.Point(50, 50), FontFace.HersheyComplex, 1.0, new Bgr(0, 255, 0).MCvScalar);
         // вивисти картинку на екран
         // Mosaica(ID, imgROI[ID].Bitmap);




            return img;
        }

        //================================================== ВИРІВНЮВАННЯ ФОНУ ================================================================================================//



  









        #endregion BackgroundSetings
        //================================================== *************************** ================================================================================================//
     static   public Bitmap FindContursExplicit(Int16 ID, Int16 ContourID, Bitmap Imag, out bool FindGlobs)
        {
            bool Return = false;

            Image<Bgr, byte> img = Imag.ToImage<Bgr, byte>();
            Image<Bgr, byte> imgROI = new Image<Bgr, byte>(img.Data);

            //створюємо пустий контур
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();


            int[,] hierarchy = CvInvoke.FindContourTree(
                 img.InRange(new Bgr(ColorMin[ID, SetingsID[ID]].B, ColorMin[ID, SetingsID[ID]].G, ColorMin[ID, SetingsID[ID]].R),
                             new Bgr(ColorMax[ID, SetingsID[ID]].B, ColorMax[ID, SetingsID[ID]].G, ColorMax[ID, SetingsID[ID]].R))
                , contours, ChainApproxMethod.LinkRuns);


            //ChainCode           0   вихідні контури у коді ланцюга Фрімана. Усі інші методи виводять багатокутники(послідовності вершин).
            //ChainApproxNone     1   перевести всі точки з ланцюгового коду в точки;
            //ChainApproxSimple   2   стискати горизонтальний, вертикальний та діагональний сегменти, тобто функція залишає лише їхні кінцеві точки;
            //ChainApproxTc89L1   3
            //ChainApproxTc89Kcos 4   застосувати один із ароматів алгоритму наближення ланцюга Teh-Chin
            //LinkRuns            5   використовувати абсолютно інший алгоритм пошуку контуру за допомогою зв'язування горизонтальних сегментів 1s. За допомогою цього методу можна використовувати лише режим пошуку СПИСОК

            double SumTemp=0.0;

            for (int Count_Contur = 0; Count_Contur < contours.Size; Count_Contur++)
            {
                double temp = CvInvoke.ContourArea(contours[Count_Contur]);
                SumTemp = temp + temp;

               // if ( SumTemp >  SV.DT_BIN.ConturPointMax[ID]) { Return = true; }
               

                if ((temp > (Min_Contur[ID, ContourID])) && (temp < Max_Contur[ID, ContourID]))
                {

                    // створюємо колір
                    // MCvScalar drawingColor = new Bgr(Color.Blue).MCvScalar;

                    // Getting minimal rectangle which contains the contour витягуєм один контур
                    // Rectangle box = CvInvoke.BoundingRectangle(contours[Count_Contur]);

                    //вирізати картинку по контору
                    //imgROI.ROI = box;

                    // if (AnalysisMode[ID, 1] == true) { Return = true;  if (SelectConturs[ID]==true) { CvInvoke.DrawContours(imgROI, contours, Count_Contur, new MCvScalar(10000, 1, 2), 0, LineType.FourConnected); } }

                    Return = true;
                    //вивисти картинку на екран
                    //Mosaica(ID, imgROI.Bitmap);
                }

            }


            FindGlobs = Return;
            return imgROI.ToBitmap();

        }

        static public double FindContursExplicit(Int16 ID, Int16 ContourID, Image<Bgr, byte> img, out bool FindGlobs)
        {
            bool Return = false;

            Image<Bgr, byte> imgROI = new Image<Bgr, byte>(img.Data);

            //створюємо пустий контур
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();


            int[,] hierarchy = CvInvoke.FindContourTree(
                 img.InRange(new Bgr(ColorMin[ID, SetingsID[ID]].B, ColorMin[ID, SetingsID[ID]].G, ColorMin[ID, SetingsID[ID]].R),
                             new Bgr(ColorMax[ID, SetingsID[ID]].B, ColorMax[ID, SetingsID[ID]].G, ColorMax[ID, SetingsID[ID]].R))
                , contours, ChainApproxMethod.LinkRuns);


            //ChainCode           0   вихідні контури у коді ланцюга Фрімана. Усі інші методи виводять багатокутники(послідовності вершин).
            //ChainApproxNone     1   перевести всі точки з ланцюгового коду в точки;
            //ChainApproxSimple   2   стискати горизонтальний, вертикальний та діагональний сегменти, тобто функція залишає лише їхні кінцеві точки;
            //ChainApproxTc89L1   3
            //ChainApproxTc89Kcos 4   застосувати один із ароматів алгоритму наближення ланцюга Teh-Chin
            //LinkRuns            5   використовувати абсолютно інший алгоритм пошуку контуру за допомогою зв'язування горизонтальних сегментів 1s. За допомогою цього методу можна використовувати лише режим пошуку СПИСОК

            double SumTemp = 0.0;

            for (int Count_Contur = 0; Count_Contur < contours.Size; Count_Contur++)
            {
                double temp = CvInvoke.ContourArea(contours[Count_Contur]);
                SumTemp = temp + temp;

              //  if (SumTemp > SV.DT_BIN.ConturPointMax[ID]){ Return = true; }
                

                if ((temp > (Min_Contur[ID, ContourID])) && (temp < Max_Contur[ID, ContourID]))
                {

                    // створюємо колір
                    // MCvScalar drawingColor = new Bgr(Color.Blue).MCvScalar;

                    // Getting minimal rectangle which contains the contour витягуєм один контур
                    // Rectangle box = CvInvoke.BoundingRectangle(contours[Count_Contur]);

                    //вирізати картинку по контору
                    //imgROI.ROI = box;

                    // if (AnalysisMode[ID, 1] == true) { Return = true;  if (SelectConturs[ID]==true) { CvInvoke.DrawContours(imgROI, contours, Count_Contur, new MCvScalar(10000, 1, 2), 0, LineType.FourConnected); } }
               
                    Return = true;

                    FindGlobs = Return;

                    return temp;
                    //вивисти картинку на екран
                    //Mosaica(ID, imgROI.Bitmap);
                }

            }


            FindGlobs = Return;
            return 0.0;

        }




        Image<Bgr, byte> Resolt;
        Mat NewMatM = new Mat();



        public Image<Bgr, Byte> CleanBegraundROI(Image img, int ID)
        {

            Image<Bgr, Byte> Data = new Image<Bgr, byte>(img.Width, img.Height);
            Image<Bgr, Byte> Data2 = new Image<Bgr, byte>(img.Width, img.Height);


            Data = new Bitmap(img, 100, 100).ToImage<Bgr, byte>(); // fail 100x50pix
            Data2 = new Bitmap(img, 100, 100).ToImage<Bgr, byte>();
            /// byte[,,] data = Data.Data;

            //маскування певних кольорів білим//
            Resolt = new Image<Bgr, byte>(100, 100, new Bgr(255, 255, 255));
            Image<Gray, byte> Maska = Data.Convert<Gray, byte>();

            Maska = Data.InRange(new Bgr(ColorMin[ID, SetingsID[ID]].B, ColorMin[ID, SetingsID[ID]].G, ColorMin[ID, SetingsID[ID]].R),
                                 new Bgr(ColorMax[ID, SetingsID[ID]].B, ColorMax[ID, SetingsID[ID]].G, ColorMax[ID, SetingsID[ID]].R)); 
            CvInvoke.BitwiseAnd(Data2, Data2, Resolt, ~Maska);
         

            //CvInvoke.Imshow("CleanBegraund", Resolt);
            return Resolt;
        }
}















    class CutImages{
        public Image<Gray, byte>[] Img       { get; set; }
        public int                 CountAry  { get; set; }
        public Rectangle[]         ROI       { get; set; }
        public int                 Count     { get; set; }
    }

    class CutImg{
        public Mat Img       { get; set; }
        public Rectangle ROI { get; set; }
        public int ID        { get; set; }
    }



    class Trace
    {
        public Rectangle[] ROI { get; set; }
        public int Frame { get; set; }
        public bool [] Found { get; set; }
    }


    class Img
    {
        public Bitmap[] Bmap { get; set; } = new Bitmap[2];
    }

    class DTLimg
    {
        public Image<Gray, Byte> Img ; // зображення з двох сторін
        public int ID;                 // індефікатор масива назва семпла по сторонам
        public String Name ;           // назва семпла по сторонам
        public float  Value;           // детальна значеня по усіх назвах сеплів для двох сторін

                                            

    };

}
