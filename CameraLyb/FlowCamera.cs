
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
        public static ConcurrentQueue<Image<Gray, byte>>   ImgSave = new ConcurrentQueue<Image<Gray, byte>>();  // буфер для збереження тестових image
        public static ConcurrentQueue<Image<Gray, byte>>   BoxM    = new ConcurrentQueue<Image<Gray, byte>>();

        public static ConcurrentQueue<Image<Gray, byte>> BoxImgM = new ConcurrentQueue<Image<Gray, byte>>();//  буфер для imags з камер master - slave
        public static ConcurrentQueue<Image<Gray, byte>> BoxImgS = new ConcurrentQueue<Image<Gray, byte>>();//  буфер для imags з камер master - slave

        public static ConcurrentQueue<CutImg> BuferImg = new ConcurrentQueue<CutImg>();

       
        public static int BatchSizePreict;
        //Live Viwe
        public static Image<Bgr, byte> LiveImage;

        // public static ConcurrentQueue<Image<Gray, byte>> BoxS = new ConcurrentQueue<Image<Gray, byte>>();
        static Image<Gray, byte>[] imgDT = new Image<Gray, byte>[2];

        
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
        public static int CauntOllBlob = 0;
        static Image<Gray, byte> imgAI = new Image<Gray, byte>(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width);
        static Mat NewMatAI = new Mat();
        public static int CountContactAI = 0;
        static Image<Gray, byte> ImagContactAI;
        static Image<Bgr, byte> ImagContactVI = new Image<Bgr, byte>(8192,400);

      
        int ZiseImages = 10; // число в скільки раз вжимаємо фото

        public static int WidthAI ;
        public static int AperturaWidth;
        int HeightAI ;

        static Image<Gray, byte> ImagAI;
        static Image<Gray, byte> ImagAI_Old;
        static CutCTR CutCTR_Old = new CutCTR();

        public static bool PotocStartAnalisBlobs=false;

        static Mat NewMat;
        static Image<Bgr, byte> imgStic;



        public void AnalisBlobs()
        {


            while (PotocStartAnalisBlobs)
            {

                if (FlowCamera.BoxImgM.Count != 0)
                {



                    FlowCamera.BoxImgM.TryDequeue(out ImagAI);


                    if ((ImagAI != null))
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        WidthAI = ImagAI.Width / ZiseImages;
                        HeightAI = ImagAI.Height / ZiseImages;
                        AperturaWidth = ImagAI.Width;

                        /////////зжимаємо фото
                        imgAI = ImagAI.Resize(WidthAI, HeightAI, Inter.Linear);

                        var CutCTR_SV = FindBlobMini(imgAI.Copy());

                        Rectangle BoxROI    = new Rectangle();
                        Rectangle BoxROIcaT = new Rectangle();


                        if ((SETS.Data.LiveVideoOFF)&&(SETS.Data.LiveViewCam)){CvInvoke.CvtColor(ImagAI, ImagContactVI, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr);}

                        int closestIndex = 0;
                        bool OK = false;
                        IProducerConsumerCollection<CutImg> CollecTemp = FlowCamera.BuferImg;

                        for (int i = 0; i < CutCTR_SV.CUT.Length; i++) {
                       
                            if (CutCTR_SV.CUT[i]){
                            
                                //if (CutCTR_SV.ROI[i].Y > HeightAI)
                                //{


                                BoxROI.X = (CutCTR_SV.ROI[i].X * 10);
                                BoxROI.Y = (CutCTR_SV.ROI[i].Y * 10);  //20
                                BoxROI.Height = (CutCTR_SV.ROI[i].Height * 10); //вниз 19
                                BoxROI.Width = CutCTR_SV.ROI[i].Width * 10; //в ліво 12



                                if (BoxROI.Y == 0)
                                {

                                    //CtrFind = false;
                                    //if (SelectDoubl == true)
                                    //{ CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(0, 0, 200), 1, LineType.FourConnected); }
                                    int RoiX = 100;
                                    ImagAI.ROI = Rectangle.Empty;
                                    //  if ((BoxROI.Y + (BoxROI.Height) >= ImagAI.Height)) {

                                    for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                                    {
                                        if (CutCTR_SV.CUT[i])
                                        {

                                            BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * 10);
                                            int distance = Math.Abs(BoxROI.X - BoxROIcaT.X);

                                            if ((distance < RoiX))
                                            {
                                                RoiX = distance;
                                                closestIndex = idx;
                                            }
                                        }
                                    }



                                    //Stmpl дотикається до верху (Позначаєм Червоним но не враховуємо)
                                    if ((CutCTR_Old.CUT.Length == 0) || (RoiX >= 100))
                                    {
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CauntOllBlob++;
                                        CollecTemp.TryAdd(CutImgClass);
                                        CutImgClass = new CutImg();
                                        if (SETS.Data.LiveVideoOFF)
                                        {CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.DarkOrange).MCvScalar, 5);}
                                        break;
                                    }


                                    BoxROIcaT.X = (CutCTR_Old.ROI[closestIndex].X * 10);
                                    BoxROIcaT.Y = (CutCTR_Old.ROI[closestIndex].Y * 10);  //20
                                    BoxROIcaT.Height = (CutCTR_Old.ROI[closestIndex].Height * 10); //вниз 19
                                    BoxROIcaT.Width = CutCTR_Old.ROI[closestIndex].Width * 10; //в ліво 12

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
                                    CauntOllBlob++;
                                    CollecTemp.TryAdd(CutImgClass);
                                    CutImgClass = new CutImg();
                                    CutCTR_Old.NULL[closestIndex] = false;
                                    if (SETS.Data.LiveVideoOFF)
                                    {CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Blue).MCvScalar, 5);}


                                }
                                else
                                {

                                    if ((BoxROI.Y + BoxROI.Height) != ImagAI.Height)
                                    {
                                        //Setmpl вилучено для подальшлго аналізу (Позначаєм Зелений)
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CauntOllBlob++;
                                        CollecTemp.TryAdd(CutImgClass);
                                        CutImgClass = new CutImg();
                                        if (SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Green).MCvScalar, 5); }

                                    }
                                    else
                                    {
                                        CutCTR_SV.NULL[i] = true;
                                        if (SETS.Data.LiveVideoOFF)
                                        {CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Red).MCvScalar, 5);}
                                    }
                                }
                            }
                        }




                        //Stmpl зєднуються з двох окремих картинок 

                        if (CutCTR_Old.CUT != null)
                        {
                            for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                            {
                                if (CutCTR_Old.NULL[idx] == true)
                                {
                                    BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * 10);
                                    BoxROIcaT.Y = (CutCTR_Old.ROI[idx].Y * 10);  //20
                                    BoxROIcaT.Height = CutCTR_Old.ROI[idx].Height * 10; //вниз 19
                                    BoxROIcaT.Width = CutCTR_Old.ROI[idx].Width * 10; //в ліво 12
                                    ImagAI_Old.ROI = BoxROIcaT;

                                    CutImgClass.Img = ImagAI_Old.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                    CutImgClass.ROI = BoxROI;
                                    CauntOllBlob++;
                                    CollecTemp.TryAdd(CutImgClass);
                                    CutImgClass = new CutImg();
                                    //if (SETS.Data.LiveVideoOFF) { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROIcaT, new Bgr(Color.Black).MCvScalar, 5); }
                                }
                            }
                        }



                        ImagAI.ROI = Rectangle.Empty;
                        ImagAI_Old = ImagAI.Copy();
                        ImagAI_Old.ROI = Rectangle.Empty;
                        CutCTR_Old = CutCTR_SV;



                        if ((FlowCamera.SaveImages == true) && (SETS.Data.LiveViewCam))
                        {
                            IProducerConsumerCollection<Image<Gray, byte>> tmpSave = FlowCamera.ImgSave; //створити ліст імідж
                            tmpSave.TryAdd(ImagAI.Copy()  /* imOriginal.ToImage<Gray, byte>()*/);
                        }

                        watch.Stop();
                        DLS.elapsedMs = watch.ElapsedMilliseconds;
                        // if (SETS.Data.LiveVideoOFF){LiveImage = ImagContactVI.Resize(2000, 100, Inter.Linear);}

                        if ((SETS.Data.LiveVideoOFF) && (SETS.Data.LiveViewCam)) { 

                            imgStic = ImagContactVI.Resize(2000,100, Inter.Linear);
                            //imgStic = ImagContactVI;

                            NewMat = new Mat();

                        if ((FlowCamera.LiveImage != null)&&(FlowCamera.LiveImage != null))
                        {
                                FlowCamera.LiveImage.ROI = new Rectangle(0, (imgStic.Height), imgStic.Width, (imgStic.Height));
                            CvInvoke.VConcat(FlowCamera.LiveImage, imgStic, NewMat);

                        }
                        else {
                            NewMat = imgStic.Mat;
                                FlowCamera.LiveImage = imgStic;
                            CvInvoke.VConcat(FlowCamera.LiveImage, imgStic, NewMat);
                        }
                            //    NewMat = img.Mat;
                            FlowCamera.LiveImage = NewMat.ToImage<Bgr, byte>().Clone();
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
                    //if (boxROI.Y > ((ImageAN.Height * 2) - 20/*Line detect*/)) { CtrFind = false; }
                    //SelectDoubl = true;
                    //Sample finish cut
                    //if (boxROI.Y == 0){
                    //    //CtrFind = false;
                    //    //if (SelectDoubl == true)
                    //    //{ CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(0, 0, 200), 1, LineType.FourConnected); }
                    //}


                    //Sample double
                    //if (Treker.Contains(boxROI.X))
                    //{
                    //    //CtrFind = false;
                    //    //if (SelectDoubl == true)
                    //    //{
                    //    //    CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(0, 255, 0), 1, LineType.FourConnected);
                    //    //}
                    //}
                    //else
                    //{


                    //    //Sample start cut
                    //    //if (((boxROI.Y + boxROI.Height) >= ImageAN.Height)) {
                    //    //   // CtrFind = false;
                    //    //   // if (SelectDoubl == true) { CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(255, 0, 0), 1, LineType.FourConnected); }
                    //    //}
                    //}


                    ///-----------ДОБАВИТИ ЗНАЙДЕНИЙ КОНТУР ДЛЯ АНАЛІЗУ---------------//
                    if (CtrFind == true){
                        CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(50, 255, 50), 1, LineType.FourConnected);
                        if (!(Treker.Contains(boxROI.X))) { TrekerRW.Add(boxROI.X); }
                        ImagAI.ROI = boxROI;
                        cutCTR.ROI [CnSize] = boxROI;
                        cutCTR.NULL[CnSize] = false;
                        cutCTR.CUT [CnSize++] = true;

                        //CutImgClass.Img = ImagAI.Resize(64, 64, Inter.Linear).Mat;
                        //CutImgClass.ROI = boxROI;
                        //CutImgClass.CountAry = (int)CnturSize;
                        //CauntOllBlob++;
                        //ImagAI.ROI = Rectangle.Empty;
                        //IProducerConsumerCollection<CutImg> CollecTemp = FlowCamera.BuferImg;
                        //CollecTemp.TryAdd(CutImgClass);
                        //CutImgClass = new CutImg();
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
        public static int CauntOllBlob = 0;
        static Image<Gray, byte> imgAI = new Image<Gray, byte>(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width);
        static Mat NewMatAI = new Mat();
        public static int CountContactAI = 0;
        static Image<Gray, byte> ImagContactAI;
        static Image<Bgr, byte> ImagContactVI = new Image<Bgr, byte>(8192, 300);


        int ZiseImages = 10; // число в скільки раз вжимаємо фото

        public static int WidthAI;
        public static int AperturaWidth;
        int HeightAI;

        static Image<Gray, byte> ImagAI;
        static Image<Gray, byte> ImagAI_Old;
        static CutCTR CutCTR_Old = new CutCTR();

        public static bool PotocStartAnalisBlobs = false;



        public void AnalisBlobs()
        {


            while (PotocStartAnalisBlobs)
            {

                if (FlowCamera.BoxImgS.Count != 0)
                {



                    FlowCamera.BoxImgS.TryDequeue(out ImagAI);


                    if ((ImagAI != null))
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        WidthAI = ImagAI.Width / ZiseImages;
                        HeightAI = ImagAI.Height / ZiseImages;
                        AperturaWidth = ImagAI.Width;

                        /////////зжимаємо фото
                        imgAI = ImagAI.Resize(WidthAI, HeightAI, Inter.Linear);

                        var CutCTR_SV = FindBlobMini(imgAI.Copy());

                        Rectangle BoxROI = new Rectangle();
                        Rectangle BoxROIcaT = new Rectangle();


                        if ((SETS.Data.LiveVideoOFF) && (!SETS.Data.LiveViewCam)) { CvInvoke.CvtColor(ImagAI, ImagContactVI, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr); }

                        int closestIndex = 0;
                        bool OK = false;
                        IProducerConsumerCollection<CutImg> CollecTemp = FlowCamera.BuferImg;

                        for (int i = 0; i < CutCTR_SV.CUT.Length; i++)
                        {

                            if (CutCTR_SV.CUT[i])
                            {

                                //if (CutCTR_SV.ROI[i].Y > HeightAI)
                                //{


                                BoxROI.X = (CutCTR_SV.ROI[i].X * 10);
                                BoxROI.Y = (CutCTR_SV.ROI[i].Y * 10);  //20
                                BoxROI.Height = (CutCTR_SV.ROI[i].Height * 10); //вниз 19
                                BoxROI.Width = CutCTR_SV.ROI[i].Width * 10; //в ліво 12



                                if (BoxROI.Y == 0)
                                {

                                    //CtrFind = false;
                                    //if (SelectDoubl == true)
                                    //{ CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(0, 0, 200), 1, LineType.FourConnected); }
                                    int RoiX = 100;
                                    ImagAI.ROI = Rectangle.Empty;
                                    //  if ((BoxROI.Y + (BoxROI.Height) >= ImagAI.Height)) {

                                    for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                                    {
                                        if (CutCTR_SV.CUT[i])
                                        {

                                            BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * 10);
                                            int distance = Math.Abs(BoxROI.X - BoxROIcaT.X);

                                            if ((distance < RoiX))
                                            {
                                                RoiX = distance;
                                                closestIndex = idx;
                                            }
                                        }
                                    }



                                    //Stmpl дотикається до верху (Позначаєм Червоним но не враховуємо)
                                    if ((CutCTR_Old.CUT.Length == 0) || (RoiX >= 100)) {
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CauntOllBlob++;
                                        CollecTemp.TryAdd(CutImgClass);
                                        CutImgClass = new CutImg();
                                        if (SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Red).MCvScalar, 5); }
                                        break;
                                    }


                                    BoxROIcaT.X = (CutCTR_Old.ROI[closestIndex].X * 10);
                                    BoxROIcaT.Y = (CutCTR_Old.ROI[closestIndex].Y * 10);  //20
                                    BoxROIcaT.Height = (CutCTR_Old.ROI[closestIndex].Height * 10); //вниз 19
                                    BoxROIcaT.Width = CutCTR_Old.ROI[closestIndex].Width * 10; //в ліво 12

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
                                    CauntOllBlob++;
                                    CollecTemp.TryAdd(CutImgClass);
                                    CutImgClass = new CutImg();
                                    CutCTR_Old.NULL[closestIndex] = false;
                                    if (SETS.Data.LiveVideoOFF)
                                    { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Blue).MCvScalar, 5); }


                                } else {

                                    // якщо семпл не дотикається до кінця то враховуємо
                                    //     ------  SEMPL DETEKT ----
                                    if ((BoxROI.Y + BoxROI.Height) != ImagAI.Height)
                                    {
                                        //Setmpl вилучено для подальшлго аналізу (Позначаєм Зелений)
                                        ImagAI.ROI = BoxROI;
                                        CutImgClass.Img = ImagAI.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                        CutImgClass.ROI = BoxROI;
                                        CauntOllBlob++;
                                        CollecTemp.TryAdd(CutImgClass);
                                        CutImgClass = new CutImg();
                                        if (SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Green).MCvScalar, 5); }

                                    }
                                    else
                                    {  //---    CAT SEMPELS   ------
                                        CutCTR_SV.NULL[i] = true;
                                        if (SETS.Data.LiveVideoOFF)
                                        { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROI, new Bgr(Color.Yellow).MCvScalar, 5); }
                                    }
                                }
                            }
                        }




                        //Stmpl зєднуються з двох окремих картинок 
                        //---    CAT SEMPELS  ADD ------
                        if (CutCTR_Old.CUT != null)
                        {
                            for (int idx = 0; idx < CutCTR_Old.CUT.Length; idx++)
                            {
                                if (CutCTR_Old.NULL[idx] == true)
                                {
                                    BoxROIcaT.X = (CutCTR_Old.ROI[idx].X * 10);
                                    BoxROIcaT.Y = (CutCTR_Old.ROI[idx].Y * 10);  //20
                                    BoxROIcaT.Height = CutCTR_Old.ROI[idx].Height * 10; //вниз 19
                                    BoxROIcaT.Width = CutCTR_Old.ROI[idx].Width * 10; //в ліво 12
                                    ImagAI_Old.ROI = BoxROIcaT;

                                    CutImgClass.Img = ImagAI_Old.Resize(Dim_ImgMosaic.Height, Dim_ImgMosaic.Width, Inter.Linear).Mat;
                                    CutImgClass.ROI = BoxROI;
                                    CauntOllBlob++;
                                    CollecTemp.TryAdd(CutImgClass);
                                    CutImgClass = new CutImg();
                                    if ((SETS.Data.LiveVideoOFF) && (!SETS.Data.LiveViewCam)) { CvInvoke.Rectangle(ImagContactVI.Mat, BoxROIcaT, new Bgr(Color.Black).MCvScalar, 5); }
                                }
                            }
                        }



                        ImagAI.ROI = Rectangle.Empty;
                        ImagAI_Old = ImagAI.Copy();
                        ImagAI_Old.ROI = Rectangle.Empty;
                        CutCTR_Old = CutCTR_SV;

                        if ((FlowCamera.SaveImages == true) && (!SETS.Data.LiveViewCam))
                        {
                            IProducerConsumerCollection<Image<Gray, byte>> tmpSave = FlowCamera.ImgSave; //створити ліст імідж
                            tmpSave.TryAdd(ImagAI.Copy()  /* imOriginal.ToImage<Gray, byte>()*/);
                        }

                        watch.Stop();
                        DLS.elapsedMs = watch.ElapsedMilliseconds;
                        if ((SETS.Data.LiveVideoOFF) && (!SETS.Data.LiveViewCam))
                        { FlowCamera.LiveImage = ImagContactVI.Resize(2000, 100, Inter.Linear); }

          
                    

              


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
            for (CountCNT = 0; CountCNT < contours.Size; CountCNT++)
            {
                double CnturSize = CvInvoke.ContourArea(contours[CountCNT]);

                //    ВИЗНАЧИТИ ЧИ ПРОХОДИТЬ ЗНАЙДЕНИЙ КОНТУР ПО РОЗМІРУ
                if (((CnturSize >= EMGU.Data.GreySizeMin[0]) && (CnturSize < EMGU.Data.GreySizeMax[0])))
                {
                    boxROI = CvInvoke.BoundingRectangle(contours[CountCNT]);
                    CtrFind = true;
                    //if (boxROI.Y > ((ImageAN.Height * 2) - 20/*Line detect*/)) { CtrFind = false; }
                    //SelectDoubl = true;
                    //Sample finish cut
                    //if (boxROI.Y == 0){
                    //    //CtrFind = false;
                    //    //if (SelectDoubl == true)
                    //    //{ CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(0, 0, 200), 1, LineType.FourConnected); }
                    //}


                    //Sample double
                    //if (Treker.Contains(boxROI.X))
                    //{
                    //    //CtrFind = false;
                    //    //if (SelectDoubl == true)
                    //    //{
                    //    //    CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(0, 255, 0), 1, LineType.FourConnected);
                    //    //}
                    //}
                    //else
                    //{


                    //    //Sample start cut
                    //    //if (((boxROI.Y + boxROI.Height) >= ImageAN.Height)) {
                    //    //   // CtrFind = false;
                    //    //   // if (SelectDoubl == true) { CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(255, 0, 0), 1, LineType.FourConnected); }
                    //    //}
                    //}


                    ///-----------ДОБАВИТИ ЗНАЙДЕНИЙ КОНТУР ДЛЯ АНАЛІЗУ---------------//
                    if (CtrFind == true)
                    {
                        CvInvoke.DrawContours(ImageAN, contours, CountCNT, new MCvScalar(50, 255, 50), 1, LineType.FourConnected);
                        if (!(Treker.Contains(boxROI.X))) { TrekerRW.Add(boxROI.X); }
                        ImagAI.ROI = boxROI;
                        cutCTR.ROI[CnSize] = boxROI;
                        cutCTR.NULL[CnSize] = false;
                        cutCTR.CUT[CnSize++] = true;

                        //CutImgClass.Img = ImagAI.Resize(64, 64, Inter.Linear).Mat;
                        //CutImgClass.ROI = boxROI;
                        //CutImgClass.CountAry = (int)CnturSize;
                        //CauntOllBlob++;
                        //ImagAI.ROI = Rectangle.Empty;
                        //IProducerConsumerCollection<CutImg> CollecTemp = FlowCamera.BuferImg;
                        //CollecTemp.TryAdd(CutImgClass);
                        //CutImgClass = new CutImg();
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
    //==============================                                 ВИЗНАЧАЄМ ТА МАЛЮЄМО КОНТУРИ з FOTO                                                      ==============================//
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    class AnalisPredict{


        public delegate void Mosaics(DTLimg dTLimg);
        static public event Mosaics MosaicaEvent;

        public static bool QualityRecognition = false;

        public static bool SelectionContamination = false;
        public const int MaxImageSave = 1000; //максимальна кількість фоток  яку можна додавати в "List" та зберігати та відтворювати в симуляторі
        public static bool Setings = false;  //признак симуляції 
        public int Count_Contur = new int();
               const int MaxBatchSizeML = 1000;


        public static int GoodSamples = 0;
        public static int BadSamples  = 0;



        private Image<Bgr, byte>[] ImagContact = new Image<Bgr, byte>[2];
        public int[] CountContact = new int[2] { 0, 0 };

        static public bool Flapslocking = false;
        static public bool FlapsTest= false;
        static public bool MosaicShowOll = false;
        static public bool PotocStartPredict = false;
        


        public void resetValue()
        {
            ImagContact[0] = null;
            ImagContact[1] = null;
            CountContact[0] = 0;
            CountContact[1] = 0;

            EMGU.ListMast.Clear();
            EMGU.ListSlav.Clear();

            GoodSamples = 0;
            BadSamples  = 0;
            ANLImg_M.CauntOllBlob = 0;
        }




        static int[] OUTPUT_BIT1 = new int[3];
        static int[] OUTPUT_BIT2 = new int[3];   //зроблено для спрацювання трьох електоро тяг (коли боб падає між двома лопатками)
        public static bool StartAnais = false;



        ML ml = new ML();

        public void Predict(){

            int[] Arie ;
                CutImg ImagAI = new CutImg();

            //List<Mat> ImgsPredict = new List<Mat>();
            List<Mat>       ImgsMosaic    = new List<Mat>();
            List<Rectangle> ImgsRectangle = new List<Rectangle>();
            if (ml.model == null)
            {
                //Model model = ml.ReadModel();
                ml.InstModel(Path.Combine(Application.StartupPath, "Data"));
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
                            ImgsMosaic.Add(ImagAI.Img);
                            ImgsRectangle.Add(ImagAI.ROI);
                        } else { break; }

                        if ( FlowCamera.BuferImg.Count == 0) { break; }
                    }

                    FlowCamera.BatchSizePreict = IdxBatch;


                    if (ImgsMosaic.Count != 0){
                        Stopwatch watch = Stopwatch.StartNew();
 



                              //PREDICT
                            var Predict = ml.PredictImage(ImgsMosaic);
                       
                          
                                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                          Console.WriteLine("First Prediction took: " + elapsedMs + " ms");
                            int idxRz = 0;
                            foreach (var pred in Predict.numpy())
                            {
                            if ((ImgsRectangle[idxRz].X!=0)&&(ImgsRectangle[idxRz].Width!=0)) {
                                //var numpyArray = value[0].numpy();
                                var class_index = np.argmax(pred);

                            if (((int)class_index != 1) || (MosaicShowOll)|| FlapsTest)
                            {
                                BadSamples++;
                                if (((!MosaicShowOll)||(FlapsTest))&&(!Flapslocking))
                                {


                                    var OUTPUT_BIT = SeparationChenal(ImgsRectangle[idxRz].X, ImgsRectangle[idxRz].Width, false);
                                    USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[0]);
                                    USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[1]);
                                    USB_HID.PLC_C2S150.FLAPS.SET((USB_HID.PLC_C2S150.FLAPS.Typ)OUTPUT_BIT[2]);

                                }
                                USB_HID.PLC_C2S150.FLAPS.SET();
                                DTLimg DTLimg = new DTLimg();
                                DTLimg.Img = ImgsMosaic[idxRz].ToImage<Gray, byte>();
                                MosaicaEvent(DTLimg);

                            }else { GoodSamples++; } idxRz++; }}

                               
                            


                       
                        ImgsMosaic   .Clear();
                        ImgsRectangle.Clear();
                    }

                }
                      Thread.Sleep(1);  }




        } //*******************************************************



        //Визначення кaналу для відправки на контролер
        const int Fleps = 16;/* кількість лопаток*/
         public int[] SeparationChenal(int Position, int Length, bool RES)
        {
          
            double ShouldDobl = (double)((double)((double)(ANLImg_M.AperturaWidth / (double)Fleps))/100) * (double)SETS.Data.DoublingFlaps/* % від ширини лопатки*/;
            int[] Output = new int[3];
            double Shoulder = ((double)(ANLImg_M.AperturaWidth) / (double)Fleps);
            double X = ShouldDobl;
            Output[0] =  (int)((((double)Position + ((double)(Length / 2))  + (ShouldDobl))) / (Shoulder));
            Output[1] =  (int) (((double)Position + ((double)(Length / 2))) / (Shoulder));
            Output[2] =  (int)((((double)Position + ((double)(Length / 2))  - (ShouldDobl))) / (Shoulder));

         

            return Output;
        }


    }




}

























    

    