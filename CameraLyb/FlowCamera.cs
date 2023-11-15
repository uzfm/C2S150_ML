
using System.Collections.Concurrent;
using Emgu.CV;
using Emgu.CV.Structure;


using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using System.Collections.Generic;

using System.IO;
using System.Diagnostics;

using Tensorflow.NumPy;


using System.Windows.Forms;
using System.Threading;

namespace C2S150_ML
{
    class FlowCamera {

        public static bool SaveImages = false;

        // public static ConcurrentQueue<Image<Gray, byte> > ImgSave = new ConcurrentQueue<Image<Gray, byte>>();
        public static ConcurrentQueue<Image<Gray, byte>> ImgSave = new ConcurrentQueue<Image<Gray, byte>>();  // буфер для збереження тестових image
        public static ConcurrentQueue<Image<Gray, byte>> BoxM = new ConcurrentQueue<Image<Gray, byte>>();

        public static ConcurrentQueue<Image<Gray, byte>> BoxImgM = new ConcurrentQueue<Image<Gray, byte>>();//  буфер для imags з камер master - slave
        public static ConcurrentQueue<Image<Gray, byte>> BoxImgS = new ConcurrentQueue<Image<Gray, byte>>();//  буфер для imags з камер master - slave

        public static ConcurrentQueue<CutImg> BuferImg = new ConcurrentQueue<CutImg>();


        public static int  BatchSizePreict;
        static public bool AnalisLock = false;
        //Live Viwe


        // public static ConcurrentQueue<Image<Gray, byte>> BoxS = new ConcurrentQueue<Image<Gray, byte>>();
        static Image<Gray, byte>[] imgDT = new Image<Gray, byte>[2];


        private static readonly object LiveImageLock = new object(); // Об'єкт блокування для синхронізації доступу



        public static Image<Bgr, byte> LiveImage;

        //public static Image<Bgr, byte> LiveImageTV
        //{

        //    get
        //    {
        //        lock (LiveImageLock)
        //        {
        //            return LiveImage;
        //        }
        //    }
        //    set
        //    {
        //        lock (LiveImageLock)
        //        {
        //            LiveImage = value;
        //        }
        //    }
        //}

        public static int LiveVideoDelay;



        public static PictureBox LiveViewTv = new PictureBox();
        static public void LiveViewTV(PictureBox LiveView) { LiveViewTv = LiveView; }

 

  





    }








    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //==============================                                 ВИЗНАЧАЄМ ТА МАЛЮЄМО КОНТУРИ з FOTO                                                      ==============================//
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    class ANLImg_M
    {

        const int Master = 0;


        // Selected Image Rectengel
        bool SelectDoubl = false;
        //
        static HashSet<int> Treker = new HashSet<int>();
               HashSet<int> TrekerRW = new HashSet<int>();

        bool CtrFind = false;
        int CountCNT = new int();
        
        //Images Analis
        struct Dim_ImgMosaic
        {
            public const int Width = 64;
            public const int Height = 64;

        }


        List<CutImg> ListCutImg = new List<CutImg>();
        CutImg CutImgClass = new CutImg();
        public static Emgu.CV.UI.ImageBox ImageLiveVI = new Emgu.CV.UI.ImageBox();
        //public static int CauntOllBlob = 0;
        static Image<Gray, byte> imgAI = new Image<Gray, byte>(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width);
        static Mat NewMatAI = new Mat();
        public static int CountContactAI = 0;
        static Image<Gray, byte> ImagContactAI;
        static Image<Bgr, byte> ImagContactVI = new Image<Bgr, byte>(8192,400);

      
        int ZiseCompres = 5; // число в скільки раз вжимаємо фото

        public static int WidthAI ;
        public static int AperturaWidth;
        public static int AperturaHeight;
        int HeightAI ;

        static Image<Gray, byte> ImagAI;
        static Image<Gray, byte> ImagAI_Old;
        static CutCTR CutCTR_Old = new CutCTR();

        public static bool PotocStartAnalisBlobs=false;

        static Mat NewMat;
        static Image<Bgr, byte> imgOld;
        static Image<Bgr, byte> imgStic;



        public void AnalisBlobs()
        {
            CutCTR_Old.CUT     = new bool[1];
            CutCTR_Old.CUT[0]  = new bool();
            CutCTR_Old.NULL    = new bool[1];
            CutCTR_Old.NULL[0] = new bool();
            CutCTR_Old.ROI     = new Rectangle[1];
            CutCTR_Old.ROI[0]  = new Rectangle();

            while (PotocStartAnalisBlobs)
            {

                if (FlowCamera.BoxImgM.Count != 0)
                {



                    FlowCamera.BoxImgM.TryDequeue(out ImagAI);


                    if ((ImagAI != null))
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        WidthAI = ImagAI.Width / ZiseCompres;
                        HeightAI = ImagAI.Height / ZiseCompres;
                        AperturaWidth = ImagAI.Width;
                        AperturaHeight = ImagAI.Height;

                        /////////зжимаємо фото
                        imgAI = ImagAI.Resize(WidthAI, HeightAI, Inter.Linear);

                        var CutCTR_SV = FindBlobMini(imgAI.Copy());

                        Rectangle BoxROI    = new Rectangle();
                        Rectangle BoxROIcaT = new Rectangle();


                        if ((!SETS.Data.LiveVideoOFF)&&(SETS.Data.ID_CAM == DLS.Master)) {CvInvoke.CvtColor(ImagAI, ImagContactVI, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr);}

                        int closestIndex = 0;
                        bool OK = false;
                        IProducerConsumerCollection<CutImg> CollecTemp = FlowCamera.BuferImg;

                        for (int i = 0; i < CutCTR_SV.CUT.Length; i++) {

                            if (CutCTR_SV.CUT[i])
                            {

                                //if (CutCTR_SV.ROI[i].Y > HeightAI)
                                //{


                                BoxROI.X = (CutCTR_SV.ROI[i].X * ZiseCompres);
                                BoxROI.Y = (CutCTR_SV.ROI[i].Y * ZiseCompres);  //20
                                BoxROI.Height = (CutCTR_SV.ROI[i].Height * ZiseCompres); //вниз 19
                                BoxROI.Width = CutCTR_SV.ROI[i].Width * ZiseCompres; //в ліво 12




                               

                                    if (BoxROI.Y == 0)
                                {


                                    int RoiX = 100;


                                    ImagAI.ROI = Rectangle.Empty;
                                    //  if ((BoxROI.Y + (BoxROI.Height) >= ImagAI.Height)) {
                                    ///////////////////////////-------------       Визначити зразок для склейки     --------------------//////////////////////////////////////
                                    for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                                    {
                                        if (CutCTR_Old.NULL[idx])
                                        {

                                            BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * ZiseCompres);
                                            int distance = Math.Abs(BoxROI.X - BoxROIcaT.X);
                                            //ВИЗНАЧИТИ ПОХИБКУ ВІДХИЛЕННЯ РОЗРІЗАНОГО СЕМПЛА
                                            if ((distance < RoiX))
                                            {
                                                RoiX = distance;
                                                closestIndex = idx;
                                            }

                                        }
                                    }



                                    //Контур не знайдений вирізаєм що знайшло TOP (Позначаєм Оранжевим но не враховуємо)
                                    if ((CutCTR_Old.CUT.Length == 0) || (RoiX >= 100))
                                    {
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CutImgClass.ID = Master;

                                        if ((!FlowCamera.AnalisLock)&& (CutImgClass.ROI.Width != 0) && (CutImgClass.ROI.Height != 0))
                                        { Calc.BlobsMaster++; CollecTemp.TryAdd(CutImgClass); }
                                        CutImgClass = new CutImg();
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.DarkOrange).MCvScalar, 5); }
                                        //break;
                                    }
                                    else
                                    {


                                        BoxROIcaT.X = (CutCTR_Old.ROI[closestIndex].X * ZiseCompres);
                                        BoxROIcaT.Y = (CutCTR_Old.ROI[closestIndex].Y * ZiseCompres);  //20
                                        BoxROIcaT.Height = (CutCTR_Old.ROI[closestIndex].Height * ZiseCompres); //вниз 19
                                        BoxROIcaT.Width = CutCTR_Old.ROI[closestIndex].Width * ZiseCompres; //в ліво 12

                                        //ImagAI_Old.ROI = BoxROIcaT;
                                        //ImagAI.    ROI = BoxROI;
                                        if (BoxROIcaT.X >= BoxROI.X)
                                        {

                                            if (BoxROIcaT.Width > BoxROI.Width)
                                            { BoxROIcaT.Width = BoxROI.Width = BoxROIcaT.Width + (BoxROIcaT.X - BoxROI.X); }
                                            else { BoxROIcaT.Width = BoxROI.Width; }

                                            BoxROIcaT.X = BoxROI.X;
                                            ImagAI_Old.ROI = BoxROIcaT;
                                            ImagAI.ROI = BoxROI;
                                        }
                                        else
                                        {


                                            if (BoxROI.Width > BoxROIcaT.Width)
                                            { BoxROIcaT.Width = BoxROI.Width = BoxROI.Width + (BoxROI.X - BoxROIcaT.X); }
                                            else { BoxROI.Width = BoxROIcaT.Width; }

                                            BoxROI.X = BoxROIcaT.X;
                                            ImagAI_Old.ROI = BoxROIcaT;
                                            ImagAI.ROI = BoxROI;
                                        }

                                        //Stmpl дотикається до верху (Позначаєм Синій но не враховуємо) а додаєм в буфер
                                        //Stmpl зєднуються з двох окремих картинок 

                                        CvInvoke.VConcat(ImagAI_Old, ImagAI, NewMatAI);

                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = NewMatAI.ToImage<Gray, byte>().Copy().Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CutImgClass.ID = Master;

                                        if ((!FlowCamera.AnalisLock)&& (CutImgClass.ROI.Width != 0) && (CutImgClass.ROI.Height != 0))
                                        { Calc.BlobsMaster++; CollecTemp.TryAdd(CutImgClass); }
                                        CutImgClass = new CutImg();
                                        CutCTR_Old.NULL[closestIndex] = false;
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Blue).MCvScalar, 5); }

                                    }
                                }
                                else
                                {

                                    if ((BoxROI.Y + BoxROI.Height) < AperturaHeight)
                                    {
                                        //Setmpl вилучено для подальшлго аналізу (Позначаєм Зелений)
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CutImgClass.ID = Master;

                                        if ((!FlowCamera.AnalisLock)&& (CutImgClass.ROI.Width != 0) && (CutImgClass.ROI.Height != 0))
                                        { Calc.BlobsMaster++; CollecTemp.TryAdd(CutImgClass); }
                                        CutImgClass = new CutImg();
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Green).MCvScalar, 5); }

                                    }
                                    else
                                    {

                                        CutCTR_SV.NULL[i] = true;
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Red).MCvScalar, 5); }
                                    }
                                }


                  


                            }
                        }



                        //Контур не знайдений вирізаєм що знайшло TOP (Позначаєм Оранжевим но не враховуємо)
                        //коли не знайшло куска старої картинки добавляємо тещо знайшло  ВЕРХ !!!!
                        if (CutCTR_Old.CUT != null)
                        {
                            for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                            {
                                if (CutCTR_Old.NULL[idx] == true)
                                {
                                    BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * ZiseCompres);
                                    BoxROIcaT.Y = (CutCTR_Old.ROI[idx].Y * ZiseCompres);  //20
                                    BoxROIcaT.Height = CutCTR_Old.ROI[idx].Height * ZiseCompres; //вниз 19
                                    BoxROIcaT.Width = CutCTR_Old.ROI[idx].Width * ZiseCompres; //в ліво 12
                                    ImagAI_Old.ROI = BoxROIcaT;

                                    CutImgClass.Img = ImagAI_Old.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                    CutImgClass.ROI = BoxROI;
                                    CutImgClass.ID = Master;

                                    if ((!FlowCamera.AnalisLock)&& (CutImgClass.ROI.Width != 0) && (CutImgClass.ROI.Height != 0)) 
                                    { Calc.BlobsMaster++; CollecTemp.TryAdd(CutImgClass); }
                                    CutImgClass = new CutImg();
                                    ///if (!SETS.Data.LiveVideoOFF) { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROIcaT, new Bgr(Color.Black).MCvScalar, 5); }
                                }
                            }
                        }



                        ImagAI.ROI = Rectangle.Empty;
                        ImagAI_Old = ImagAI.Copy();
                        ImagAI_Old.ROI = Rectangle.Empty;
                        CutCTR_Old = CutCTR_SV;//Зберігаємо картинку для складання зрізів


                       //__________________  VIWE  ________________________________________________________________________________________
                        if ((FlowCamera.SaveImages == true) && (SETS.Data.ID_CAM == DLS.Master))
                        {
                            IProducerConsumerCollection<Image<Gray, byte>> tmpSave = FlowCamera.ImgSave; //створити ліст імідж
                            CutImgClass.ID = Master;
                            tmpSave.TryAdd(ImagAI.Copy()  /* imOriginal.ToImage<Gray, byte>()*/);
                        }

                        watch.Stop();
                        DLS.elapsedMs = watch.ElapsedMilliseconds;

                        // if (!SETS.Data.LiveVideoOFF){LiveImage = ImagContactVI.Resize(2000, 100, Inter.Linear);}



                        if ((!SETS.Data.LiveVideoOFF) && (SETS.Data.ID_CAM == DLS.Master)) {

                            FlowCamera.LiveVideoDelay++
                                ;
                            imgStic = ImagContactVI.Resize(2000,100, Inter.Linear);
                            //imgStic = ImagContactVI;
                            NewMat = new Mat();

                        if ((FlowCamera.LiveImage != null)) {
                            FlowCamera.LiveImage.ROI = new Rectangle(0, (imgStic.Height), imgStic.Width, (imgStic.Height));
                            CvInvoke.VConcat(FlowCamera.LiveImage, imgStic, NewMat);    }else {
                    
                            NewMat = imgStic.Mat;
                            imgOld = imgStic;
                            CvInvoke.VConcat(imgOld, imgStic, NewMat); }
                            FlowCamera.LiveImage = NewMat.ToImage<Bgr, byte>();

                          if(FlowCamera.LiveVideoDelay > SETS.Data.LiveVideoDelay) {
                                FlowCamera.LiveVideoDelay = 0;
                                FlowCamera.LiveViewTv.Image = FlowCamera.LiveImage.ToBitmap();}
                                
                              

                           }



                        //////////////////
                        Thread.Sleep(1);
                        //return true;
                    }
                }
            }
        }







        /********************   пошук контурів   **************************************/
        public CutCTR FindBlobMini(Image<Gray, byte> ImagAI){
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var _img = ImagAI.Convert<Gray, byte>().ThresholdBinary(new Gray( EMGU.Data.GreyScaleMin[0]), new Gray(EMGU.Data.GreyScaleMax[0]));

            Mat hierarchy = new Mat();

            //var cont =CvInvoke.FindContourTree(~_img.Mat, contours, ChainApproxMethod.ChainApproxSimple);/* визначаються контури які не торкраються краю картинки*/
            if (SETS.Data.BlobsInvert) { CvInvoke.FindContours(_img, contours, hierarchy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple); } else
            {
                CvInvoke.FindContours(~_img, contours, hierarchy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            }

            CutImages CutImage = new CutImages();
            Image<Bgr, byte> ImageAN = new Image<Bgr, byte>(ImagAI.Width, ImagAI.Height);
            Rectangle boxROI;

            //ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
            int CnSize = 0;

            for (int i = 0; i < contours.Size; i++) {
                double CnturSize = CvInvoke.ContourArea(contours[i]);
                //    ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
                if (((CnturSize >= EMGU.Data.GreySizeMin[0]) && 
                     (CnturSize < EMGU.Data.GreySizeMax[0])) )
                { CnSize++; }}
            
                CutCTR  cutCTR = new CutCTR();
                cutCTR.ROI = new Rectangle[CnSize];
                cutCTR.CUT = new bool[CnSize];
                cutCTR.NULL = new bool[CnSize];
                CnSize = 0;

            //  ***********ВИЗЧИТИ ЗАДВОЄННЯ ТА ЗАХВАТ КОНТОРУ  *************************//
            for (CountCNT = 0; CountCNT < contours.Size; CountCNT++){
                double CnturSize = CvInvoke.ContourArea(contours[CountCNT]);

                //    ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
                if (((CnturSize >= EMGU.Data.GreySizeMin[0]) && (CnturSize < EMGU.Data.GreySizeMax[0])) )
                {
                    boxROI = CvInvoke.BoundingRectangle(contours[CountCNT]);
                    CtrFind = true;
            

                    ///-----------ДОБАВИТИ ЗНАЙДЕНИЙ КОНТУР ДЛЯ АНАЛІЗУ---------------//
                    if (CtrFind == true){
                        CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(50, 255, 50), 1, LineType.FourConnected);
                        if (!(Treker.Contains(boxROI.X))) { TrekerRW.Add(boxROI.X); }
                        ImagAI.ROI = boxROI;
                        cutCTR.ROI [CnSize] = boxROI;
                        cutCTR.NULL[CnSize] = false;
                        cutCTR.CUT [CnSize++] = true;

                    }
                }
            }

            Treker = new HashSet<int>(TrekerRW);
            TrekerRW.Clear();
        

            return cutCTR;
        }



       public class CutCTR{
            public Rectangle[] ROI  { get; set; }
            public bool     [] CUT  { get; set; }
            public bool     [] NULL { get; set; }
        };
    }



    class ANLImg_S
    {

        const int Slave = 1;


        // Selected Image Rectengel
        bool SelectDoubl = false;
        //
        static HashSet<int> Treker = new HashSet<int>();
        HashSet<int> TrekerRW = new HashSet<int>();

        bool CtrFind = false;
        int CountCNT = new int();
        //
        struct Dim_ImgMosaic
        {
            public const int Width = 64;
            public const int Height = 64;

        }


        List<CutImg> ListCutImg = new List<CutImg>();
        CutImg CutImgClass = new CutImg();
        public static Emgu.CV.UI.ImageBox ImageLiveVI = new Emgu.CV.UI.ImageBox();
        static Image<Gray, byte> imgAI = new Image<Gray, byte>(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width);
        static Mat NewMatAI = new Mat();
        public static int CountContactAI = 0;
        static Image<Gray, byte> ImagContactAI;
        static Image<Bgr, byte> ImagContactVI = new Image<Bgr, byte>(8192, 300);


        int ZiseCompres = 5; // число в скільки раз вжимаємо фото

        public static int WidthAI;
        public static int AperturaWidth;
        public static int AperturaHeight;
        int HeightAI;

        static Image<Gray, byte> ImagAI;
        static Image<Gray, byte> ImagAI_Old;
        static CutCTR CutCTR_Old = new CutCTR();

        public static bool PotocStartAnalisBlobs = false;

        static Mat NewMat;
        static Image<Bgr, byte> imgStic;
        static Image<Bgr, byte> imgOld;

        public void AnalisBlobs()
        {


            CutCTR_Old.CUT = new bool[1];
            CutCTR_Old.CUT[0] = new bool();
            CutCTR_Old.NULL = new bool[1];
            CutCTR_Old.NULL[0] = new bool();
            CutCTR_Old.ROI = new Rectangle[1];
            CutCTR_Old.ROI[0] = new Rectangle();

            while (PotocStartAnalisBlobs)
            {

                if (FlowCamera.BoxImgS.Count != 0)
                {



                    FlowCamera.BoxImgS.TryDequeue(out ImagAI);


                    if ((ImagAI != null))
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        WidthAI = ImagAI.Width / ZiseCompres;
                        HeightAI = ImagAI.Height / ZiseCompres;
                        AperturaWidth = ImagAI.Width;
                        AperturaHeight = ImagAI.Height;
                        /////////зжимаємо фото
                        imgAI = ImagAI.Resize(WidthAI, HeightAI, Inter.Linear);

                        var CutCTR_SV = FindBlobMini(imgAI.Copy());

                        Rectangle BoxROI = new Rectangle();
                        Rectangle BoxROIcaT = new Rectangle();


                        if ((!SETS.Data.LiveVideoOFF) && (SETS.Data.ID_CAM == DLS.Slave)) { CvInvoke.CvtColor(ImagAI, ImagContactVI, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr); }

                        int closestIndex = 0;
                        bool OK = false;
                        IProducerConsumerCollection<CutImg> CollecTemp = FlowCamera.BuferImg;

                        for (int i = 0; i < CutCTR_SV.CUT.Length; i++)
                        {
                               /// перевірка чи знайдений контур відповідаєт розміру
                            if (CutCTR_SV.CUT[i])
                            {

                                //if (CutCTR_SV.ROI[i].Y > HeightAI)
                                //{


                                BoxROI.X = (CutCTR_SV.ROI[i].X * ZiseCompres);
                                BoxROI.Y = (CutCTR_SV.ROI[i].Y * ZiseCompres);  //20
                                BoxROI.Height = (CutCTR_SV.ROI[i].Height * ZiseCompres); //вниз 19
                                BoxROI.Width = CutCTR_SV.ROI[i].Width * ZiseCompres; //в ліво 12

                                

                               
                                    if (BoxROI.Y == 0){
                                

                                    //CtrFind = false;
                                    //if (SelectDoubl == true)
                                    //{ CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(0, 0, 200), 1, LineType.FourConnected); }
                                    int RoiX = 100;
                                    ImagAI.ROI = Rectangle.Empty;
                                    //  if ((BoxROI.Y + (BoxROI.Height) >= ImagAI.Height)) {

                                    for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                                    {
                                        if (CutCTR_Old.NULL[idx])
                                        {

                                            BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * ZiseCompres);
                                            int distance = Math.Abs(BoxROI.X - BoxROIcaT.X);

                                            //ВИЗНАЧИТИ ПОХИБКУ ВІДХИЛЕННЯ РОЗРІЗАНОГО СЕМПЛА
                                            if ((distance < RoiX))
                                            {
                                                RoiX = distance;
                                                closestIndex = idx;
                                            }

                                        }
                                    }


                                    //Контур не знайдений вирізаєм що знайшло TOP (Позначаєм Оранжевим но не враховуємо)
                                    if ((CutCTR_Old.CUT.Length == 0) || (RoiX >= 100))
                                    {
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CutImgClass.ID = Slave;

                                        if ((!FlowCamera.AnalisLock)&&(CutImgClass.ROI.Width!=0) && (CutImgClass.ROI.Height != 0)) 
                                        { CollecTemp.TryAdd(CutImgClass); Calc.BlobsSlave++; }
                                        CutImgClass = new CutImg();
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Red).MCvScalar, 5); }

                                    }
                                    else
                                    {


                                        BoxROIcaT.X = (CutCTR_Old.ROI[closestIndex].X * ZiseCompres);
                                        BoxROIcaT.Y = (CutCTR_Old.ROI[closestIndex].Y * ZiseCompres);  //20
                                        BoxROIcaT.Height = (CutCTR_Old.ROI[closestIndex].Height * ZiseCompres); //вниз 19
                                        BoxROIcaT.Width = CutCTR_Old.ROI[closestIndex].Width * ZiseCompres; //в ліво 12

                                        //ImagAI_Old.ROI = BoxROIcaT;
                                        //ImagAI.    ROI = BoxROI;
                                        if (BoxROIcaT.X >= BoxROI.X)
                                        {

                                            if (BoxROIcaT.Width > BoxROI.Width)
                                            { BoxROIcaT.Width = BoxROI.Width = BoxROIcaT.Width + (BoxROIcaT.X - BoxROI.X); }
                                            else { BoxROIcaT.Width = BoxROI.Width; }

                                            BoxROIcaT.X = BoxROI.X;
                                            ImagAI_Old.ROI = BoxROIcaT;
                                            ImagAI.ROI = BoxROI;
                                        }
                                        else
                                        {


                                            if (BoxROI.Width > BoxROIcaT.Width)
                                            { BoxROIcaT.Width = BoxROI.Width = BoxROI.Width + (BoxROI.X - BoxROIcaT.X); }
                                            else { BoxROI.Width = BoxROIcaT.Width; }

                                            BoxROI.X = BoxROIcaT.X;
                                            ImagAI_Old.ROI = BoxROIcaT;
                                            ImagAI.ROI = BoxROI;
                                        }

                                        //Stmpl дотикається до верху (Позначаєм Червоним но не враховуємо)
                                        CvInvoke.VConcat(ImagAI_Old, ImagAI, NewMatAI);
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = NewMatAI.ToImage<Gray, byte>().Copy().Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CutImgClass.ID = Slave;

                                        if ((!FlowCamera.AnalisLock)&& (CutImgClass.ROI.Width != 0) && (CutImgClass.ROI.Height != 0)) 
                                        { CollecTemp.TryAdd(CutImgClass); Calc.BlobsSlave++; }
                                        CutImgClass = new CutImg();
                                        CutCTR_Old.NULL[closestIndex] = false;
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Blue).MCvScalar, 5); }

                                    }
                                }
                                else
                                {

                                    // якщо семпл не дотикається до кінця то враховуємо
                                    //     ------  SEMPL DETEKT ----
                                    if ((BoxROI.Y + BoxROI.Height) < AperturaHeight)
                                    {
                                        //Setmpl вилучено для подальшлго аналізу (Позначаєм Зелений)
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CutImgClass.ID = Slave;

                                        if ((!FlowCamera.AnalisLock)&& (CutImgClass.ROI.Width != 0) && (CutImgClass.ROI.Height != 0)) 
                                        { CollecTemp.TryAdd(CutImgClass); Calc.BlobsSlave++; }
                                        CutImgClass = new CutImg();
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Green).MCvScalar, 5); }

                                    }
                                    else
                                    {  //---    CAT SEMPELS   ------
                                        CutCTR_SV.NULL[i] = true;
                                        if (!SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Yellow).MCvScalar, 5); }
                                    }
                                }

                           
                   





                            }
                        }




                        //коли не знайшло куска старої картинки добавляємо тещо знайшло  ВЕРХ !!!!                  
                        //---    CAT SEMPELS  ADD ------
                        if (CutCTR_Old.CUT != null)
                        {
                            for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                            {
                                if (CutCTR_Old.NULL[idx] == true)
                                {  
                                    BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * ZiseCompres);
                                    BoxROIcaT.Y = (CutCTR_Old.ROI[idx].Y * ZiseCompres);  //20
                                    BoxROIcaT.Height = CutCTR_Old.ROI[idx].Height * ZiseCompres; //вниз 19
                                    BoxROIcaT.Width = CutCTR_Old.ROI[idx].Width * ZiseCompres; //в ліво 12
                                    ImagAI_Old.ROI = BoxROIcaT;

                                    CutImgClass.Img = ImagAI_Old.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                    CutImgClass.ROI = BoxROI;
                                    CutImgClass.ID = Slave;

                                    if ((!FlowCamera.AnalisLock)&& (CutImgClass.ROI.Width != 0) && (CutImgClass.ROI.Height != 0)) 
                                    { CollecTemp.TryAdd(CutImgClass); Calc.BlobsSlave++;}
                                    CutImgClass = new CutImg();
                               //     if ((!SETS.Data.LiveVideoOFF) && (SETS.Data.ID_CAM == DLS.Slave)) { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROIcaT, new Bgr(Color.Black).MCvScalar, 5); }
                                } 
                            }
                        }



                        ImagAI.ROI = Rectangle.Empty;
                        ImagAI_Old = ImagAI.Copy();
                        ImagAI_Old.ROI = Rectangle.Empty;
                        CutCTR_Old = CutCTR_SV;

                        if ((FlowCamera.SaveImages == true) && (SETS.Data.ID_CAM == DLS.Slave))
                        {
                            IProducerConsumerCollection<Image<Gray, byte>> tmpSave = FlowCamera.ImgSave; //створити ліст імідж
                            CutImgClass.ID = Slave;

                            tmpSave.TryAdd(ImagAI.Copy()  /* imOriginal.ToImage<Gray, byte>()*/);
                        }

                        watch.Stop();
                        DLS.elapsedMs = watch.ElapsedMilliseconds;
                        //if ((!SETS.Data.LiveVideoOFF) && (!SETS.Data.LiveViewCam))
                        //{ FlowCamera.LiveImage = ImagContactVI.Resize(2000, 100, Inter.Linear); }

                        if ((!SETS.Data.LiveVideoOFF) && (SETS.Data.ID_CAM == DLS.Slave))
                        {
                            FlowCamera.LiveVideoDelay++;

                            imgStic = ImagContactVI.Resize(2000, 100, Inter.Linear);
                            //imgStic = ImagContactVI;

                            NewMat = new Mat();

                            if ((FlowCamera.LiveImage != null) && (FlowCamera.LiveImage != null))
                            {
                                FlowCamera.LiveImage.ROI = new Rectangle(0, (imgStic.Height), imgStic.Width, (imgStic.Height));
                                CvInvoke.VConcat(FlowCamera.LiveImage, imgStic, NewMat);

                            }
                            else
                            {
                                NewMat = imgStic.Mat;
                                imgOld = imgStic;
                                CvInvoke.VConcat(imgOld, imgStic, NewMat);
                            }
                            //    NewMat = img.Mat;
                            FlowCamera.LiveImage = NewMat.ToImage<Bgr, byte>();
                            if (FlowCamera.LiveVideoDelay > SETS.Data.LiveVideoDelay)
                            {
                                FlowCamera.LiveVideoDelay = 0;
                                FlowCamera.LiveViewTv.Image = FlowCamera.LiveImage.ToBitmap(); }
                        }





                        //////////////////
                        Thread.Sleep(1);
                        //return true;
                    }
                }
            }
         

        }







        /********************   пошук контурів   **************************************/
        public CutCTR FindBlobMini(Image<Gray, byte> ImagAI)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var _img = ImagAI.Convert<Gray, byte>().ThresholdBinary(new Gray(EMGU.Data.GreyScaleMin[0]), new Gray(EMGU.Data.GreyScaleMax[0]));

            Mat hierarchy = new Mat();

            //var cont =CvInvoke.FindContourTree(~_img.Mat, contours, ChainApproxMethod.ChainApproxSimple);/* визначаються контури які не торкраються краю картинки*/
            if (SETS.Data.BlobsInvert) { CvInvoke.FindContours(_img, contours, hierarchy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple); }
            else
            {
                CvInvoke.FindContours(~_img, contours, hierarchy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            }


            CutImages CutImage = new CutImages();
            Image<Bgr, byte> ImageAN = new Image<Bgr, byte>(ImagAI.Width, ImagAI.Height);
            Rectangle boxROI;

            //ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
            int CnSize = 0;


            for (int i = 0; i < contours.Size; i++)
            {
                double CnturSize = CvInvoke.ContourArea(contours[i]);
                //    ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
                if (((CnturSize >= EMGU.Data.GreySizeMin[0]) &&
                     (CnturSize < EMGU.Data.GreySizeMax[0])))
                { CnSize++; }
            }

            CutCTR cutCTR = new CutCTR();
            cutCTR.ROI = new Rectangle[CnSize];
            cutCTR.CUT = new bool[CnSize];
            cutCTR.NULL = new bool[CnSize];
            CnSize = 0;

            //  ***********ВИЗЧИТИ ЗАДВОЄННЯ ТА ЗАХВАТ КОНТОРУ  *************************//
            for (CountCNT = 0; CountCNT < contours.Size; CountCNT++){

                double CnturSize = CvInvoke.ContourArea(contours[CountCNT]);

                //ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
                if (((CnturSize >= EMGU.Data.GreySizeMin[0]) && (CnturSize < EMGU.Data.GreySizeMax[0])))
                {
                    boxROI = CvInvoke.BoundingRectangle(contours[CountCNT]);
                    CtrFind = true;

                    ///-----------ДОБАВИТИ ЗНАЙДЕНИЙ КОНТУР ДЛЯ АНАЛІЗУ---------------//
                    if (CtrFind == true)
                    {
                        CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(50, 255, 50), 1, LineType.FourConnected);
                        if (!(Treker.Contains(boxROI.X))) { TrekerRW.Add(boxROI.X); }
                        ImagAI.ROI = boxROI;
                        cutCTR.ROI[CnSize] = boxROI;
                        cutCTR.NULL[CnSize] = false;
                        cutCTR.CUT[CnSize++] = true;

                    }
                }
            }

            Treker = new HashSet<int>(TrekerRW);
            TrekerRW.Clear();


            return cutCTR;
        }



        public class CutCTR
        {
            public Rectangle[] ROI { get; set; }
            public bool[] CUT { get; set; }
            public bool[] NULL { get; set; }
        };
    }



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //==============================                                Analis  Predict                                                     ==============================//
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class Calc
    {


        public static int GoodSamples { get; set; } = 0;
        public static int BadSamples { get; set; } = 0;

        public static int BlobsMaster { get; set; } = 0;
        public static int BlobsSlave { get; set; } = 0;

        public static int StopSustem=0;

    }


    class AnalisPredict{


        public delegate void Mosaics(DTLimg dTLimg);
        static public event Mosaics MosaicaEvent;

        public static bool QualityRecognition = false;

        public static bool SelectionContamination = false;
        public const int MaxImageSave = 1000; //максимальна кількість фоток  яку можна додавати в "List" та зберігати та відтворювати в симуляторі
        public static bool Setings = false;  //признак симуляції 
        public int Count_Contur = new int();
               const int MaxBatchSizeML = 100;





        private Image<Bgr, byte>[] ImagContact = new Image<Bgr, byte>[2];
                      public int[] CountContact = new int[2] { 0, 0 };

        static public bool Flapslocking = false;
        static public bool FlapsTest= false;
        static public bool FlapsTestBleak = false;
        static public bool MosaicShowOll = false;
        static public bool MosaicShowGood = false;
        static public bool PotocStartPredict = false;
        


        public void resetValue()
        {
            ImagContact[0] = null;
            ImagContact[1] = null;
            CountContact[0] = 0;
            CountContact[1] = 0;

            EMGU.ListMast.Clear();
            EMGU.ListSlav.Clear();

            Calc.GoodSamples = 0;
            Calc.BadSamples = 0;
            Calc.BlobsMaster = 0;
            Calc.BlobsSlave = 0;
        }

      private  class Props{
            public Rectangle ROI { get; set; }
            public int ID { get; set; }
        }


        static int[] OUTPUT_BIT1 = new int[3];
        static int[] OUTPUT_BIT2 = new int[3];   //зроблено для спрацювання трьох електоро тяг (коли боб падає між двома лопатками)
        public static bool StartAnais = false;



        ML ml = new ML();
        VIS Vis = new VIS();
        public void Predict(){

            int[] Arie ;
                CutImg ImagAI = new CutImg();

            //List<Mat> ImgsPredict = new List<Mat>();
            List<Mat>       ImgsMosaic    = new List<Mat>();
            List<Props> ImgsRectangle = new List<Props>();
            VIS.DT DT_OUT = new VIS.DT();

            if (ml.model == null)
            {
                //Model model = ml.ReadModel();
                ml.InstModel(Path.Combine(STGS.DT.URL_ML, "Data"));
            }
             int IdxBatch;
            while (PotocStartPredict) {

                if ((FlowCamera.BuferImg.Count) != 0){
                    Arie = new int[101];
                 
                    for ( IdxBatch = 0; IdxBatch < MaxBatchSizeML; IdxBatch++){    
                                  ImagAI = new CutImg();
                        FlowCamera.BuferImg.TryDequeue(out ImagAI);
                       
                        //Buffer FUL
                        if ((ImagAI != null) && (ImagAI.Img != null)){
                            DT_OUT = Vis._DetectBlobBlack(ImagAI.Img);

                            if ((DT_OUT.Detect) || (FlapsTest)) {
                           
                                 // детектування чорного
                                if ((ImagAI.ROI.X != 0) && (ImagAI.ROI.Width != 0)){
                                    Calc.BadSamples++;


                                    if (!Flapslocking){ 
                                    var OUTPUT_BIT = SeparationChenal(ImagAI.ROI.X, ImagAI.ROI.Width, false);
                                    USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[0]);
                                    USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[1]);
                                    USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[2]);
                                    }

                                    if ((!MosaicShowGood) || (MosaicShowOll))
                                    {
                                        DTLimg DTLimg = new DTLimg();
                                        DTLimg.Img = ImagAI.Img.ToImage<Gray, byte>();
                                        DTLimg.Name = "bad";
                                        DTLimg.ID = ImagAI.ID;
                                        DTLimg.SizeCNT = DT_OUT.SizeCNT;
                                        MosaicaEvent(DTLimg);
                                    
                                    }else {     }





                                }
                            

                            }else{
                                DT_OUT = Vis._DetectBlob(ImagAI.Img);
                                // адаптивний сірий
                                if (DT_OUT.Detect) {
                                
                                    ImgsMosaic.Add(ImagAI.Img); // IMG analis
                                    Props props = new Props();
                                    props.ROI = ImagAI.ROI;
                                    props.ID  = ImagAI.ID;
                                    ImgsRectangle.Add(props); // Rectangl analis
                                }else {
                                    Calc.GoodSamples++;
                                    if ((MosaicShowGood)|| (MosaicShowOll))
                                    {
                                    
                                        DTLimg DTLimg = new DTLimg();
                                        DTLimg.Img = ImagAI.Img.ToImage<Gray, byte>();
                                        DTLimg.Name = "good";
                                        DTLimg.ID   = ImagAI.ID;
                                        MosaicaEvent(DTLimg); ;
                                    }
                                }  
                            }  
                        } else { break; }



                        if ( FlowCamera.BuferImg.Count == 0) { break; }
                    }






                             //--------------------PREDICT------------------//
                    if (ImgsMosaic.Count != 0){
                        Calc.StopSustem = 0;

                        var Predict = ml.PredictImage(ImgsMosaic);
                       

                            int idxRz = 0;
                            foreach (var pred in Predict.numpy())  {



                     
                                //var numpyArray = value[0].numpy();
                                var class_index = np.argmax(pred);

                                //------  BED  GOOD  ------------//
                                if (((int)class_index != 1))
                                {

                                    Calc.BadSamples++;
                                    if ((!Flapslocking) && (!FlapsTestBleak))
                                    {
                                        var OUTPUT_BIT = SeparationChenal(ImgsRectangle[idxRz].ROI.X, ImgsRectangle[idxRz].ROI.Width, false);
                                        USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[0]);
                                        USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[1]);
                                        USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[2]);
                                    }


                                    if ((!MosaicShowGood) || (MosaicShowOll))
                                    {
                                        DTLimg DTLimg = new DTLimg();
                                        DTLimg.Name = "Bad";
                                        DTLimg.ID = ImgsRectangle[idxRz].ID;
                                        DTLimg.Img = ImgsMosaic[idxRz].ToImage<Gray, byte>();

                                        MosaicaEvent(DTLimg);
                                    };

                                }
                                else
                                {
                                    Calc.GoodSamples++;
                                    if ((MosaicShowGood) || (MosaicShowOll))
                                    {
                                        DTLimg DTLimg = new DTLimg();
                                        DTLimg.Name = "Good";
                                        DTLimg.ID = ImgsRectangle[idxRz].ID;
                                        DTLimg.Img = ImgsMosaic[idxRz].ToImage<Gray, byte>();
                                        DTLimg.SizeCNT = DT_OUT.SizeCNT;
                                        MosaicaEvent(DTLimg);
                                    }
                                }
                                idxRz++;
                       
                        }

               
                          


                       
                        ImgsMosaic   .Clear();
                        ImgsRectangle.Clear();
                    }
                 USB_HID.PLC_C2S150.FLAPS.SET();
                }
                      Thread.Sleep(1);  }




        } //*******************************************************



        //Визначення кaналу для відправки на контролер

         public int[] SeparationChenal(int Position, int Length, bool RES)
        {
            int AperturaWidth = 0;
            //293- flaps
            if (ANLImg_M.AperturaWidth == 0) { AperturaWidth = ANLImg_S.AperturaWidth; } else 
                                           { AperturaWidth = ANLImg_M.AperturaWidth; }

            double ShouldDobl = (double)((double)((double)(AperturaWidth / (double)Fleps))/100) * (double)SETS.Data.DoublingFlaps/* % від ширини лопатки*/;
            int[] Output = new int[3];
            double Shoulder = ((double)(AperturaWidth) / (double)Fleps);
            double X = ShouldDobl;
            Output[0] =  (int)((((double)Position + ((double)(Length / 2))  + (ShouldDobl))) / (Shoulder));
            Output[1] =  (int) (((double)Position + ((double)(Length / 2))) / (Shoulder));
            Output[2] =  (int)((((double)Position + ((double)(Length / 2))  - (ShouldDobl))) / (Shoulder));

         

            return Output;
        }

        const int Fleps = 17; // Кількість лопаток
        double CameraWidth = ANLImg_M.AperturaWidth; // Ширина видимості камери в міліметрах
        double FlapWidth = 293.0; // Ширина однієї лопатки в міліметрах

        public int[] SeparationChenal2(int Position, int Length, bool RES)
        {
            // Визначаємо ширину однієї лопатки на основі загальної ширини лопаток
            double SingleFlapWidth = FlapWidth / Fleps;

            // Визначаємо ширину видимості однієї лопатки відносно ширини видимості камери
            double VisibleWidth = (SingleFlapWidth * CameraWidth) / FlapWidth;

            int[] Output = new int[3];

            // Визначаємо позиції для кожної з лопаток
            Output[0] = (int)((Position + (Length / 2) + (VisibleWidth / 2)) / VisibleWidth);
            Output[1] = (int)((Position + (Length / 2)) / VisibleWidth);
            Output[2] = (int)((Position + (Length / 2) - (VisibleWidth / 2)) / VisibleWidth);

            return Output;
        }





    }




}

























    

    