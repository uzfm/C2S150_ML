using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;


using VIBR_TABLE = C2S150_ML.USB_HID.PLC_C2S150.VIBRATING;
using LIGHT = C2S150_ML.USB_HID.PLC_C2S150.LIGHT;
using FLAPS = C2S150_ML.USB_HID.PLC_C2S150.FLAPS;
using CAMERA = C2S150_ML.USB_HID.PLC_C2S150.CAMERA;
using AUTOLOADER = C2S150_ML.USB_HID.PLC_C2S150.AUTOLOADER;
using SEPARATOR = C2S150_ML.USB_HID.PLC_C2S150.SEPARATOR;
using COOLING = C2S150_ML.USB_HID.PLC_C2S150.COOLING;

using Emgu.CV.CvEnum;
using System.Threading.Tasks;
using System.Threading;
using LiveCharts;
using LiveCharts.Wpf;


using LiveCharts.Defaults;
using LiveCharts.WinForms;

/// <summary>
/// SORTER 30 000 = 1kg
///  0.0333333333333333 = 1PCS
///  nid 1250 of sec
///  66.66666666666667 fps
///  19pcs on 1fps
/// </summary>

namespace C2S150_ML
{
    public partial class Sorter : Form
    {



        DLS DLS;
        static int ID;
        SETS _SETS = new SETS();
        FlowCamera flowCamera = new FlowCamera();
        USB_HID USB_HID = new USB_HID();



        public List<Image<Gray, byte>> MosaicGrey = new List<Image<Gray, byte>>();
        public List<Image<Gray, byte>> MosaicsTeachGrey = new List<Image<Gray, byte>>();

        public ImageList Mosaics = new ImageList();
        public ImageList MosaicsTeach = new ImageList();
        static List<DTLimg> MosaicDTlist; //буфер ліст для буферезації перед сортуваням




        private ChartValues<ObservablePoint> chartValues;
        private DateTime startTime;
        STGS STGS = new STGS();


        public Sorter()
        {
            InitializeComponent();




            ////******   USB HID INSTAL *********//
            USB_HID.InstalDevice("C1");
            CAMERA.ON();
            InstMosaics();
            /////////////////////////////////////////////////////////////////////////
            ///   File SAVE 
            ///   START File
            STGS.Read();
            TextBoxSemplTyp.Text = STGS.DT.SampleType;
            // ModelML.Text = STGS.DT.Name_Model;


            _SETS.Read();
           
            RefreshSetings();


           
            timer1.Enabled = true;
            //*************  initialization of cameras  *********************






           // DLS.InstCOM_Setings(DLS.Master);
            //DLS.InstCOM_Setings(DLS.Slave);
            //***************************************************************

            //************************    запустити поток      ********************************/
            Flow.AnalisBlobs();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();

            //запустити поток на аналіз Img
            //********************************************************************************/

            AnalisPredict.MosaicaEvent += MosaicaEvent;








            // Створюємо колекцію даних для графіка
            chartValues = new ChartValues<ObservablePoint>();

            //////////////////////////Налаштування вісей графіка
            cartesianChart1.AxisX.Add(new Axis { Title = "TIME" });
            cartesianChart1.AxisY.Add(new Axis { Title = "DATA" });

            // Додавання осі X з часовою міткою
            //cartesianChart1.AxisX.Add(new Axis { LabelFormatter = value => new DateTime((long)value).ToString("HH:mm:ss") });
            //cartesianChart1.AxisY.Add(new Axis { LabelFormatter = val => val.ToString("F0") });
            // Додавання серії даних
            LineSeries series = new LineSeries
            {
                Title = "Відношення хороших/поганих зразків",
                Values = chartValues,
                DataLabels = true,


            };



            // Додавання серії даних до Cartesian Chart
            cartesianChart1.Series.Add(series);

            startTime = DateTime.Now;

            // Діаграма швиткості
            solidGauge1.To = 150;

            LoadSempelsName();


        }

        private void button54_Click(object sender, EventArgs e) {
            Calc.GoodSamples = 0;
            Calc.BadSamples = 0;
            SpidIdxEv = 0;
            Calc.BlobsMaster = 0;
            Calc.BlobsSlave = 0;
            /////////////// Діаграма швиткості ////////////////
            solidGauge1.Value = 0.00;
            SpidKgH.Text = "0.00";
        }


        short TimOutRefresh;
        int RatioSampls;
        double RatioSamplsOll;
        int SpidIdxEv = 0;

        double SamplsOLL;

        private int rowIndex = 0;
        const int sampleCount = 25000; // Кількість семплів на кілограм
        double timeInSeconds = 3600; // Інтервал часу в секундах
        private void TimerRefreshChart()
        {


            if (buttonStartAnalic.Text == "Stop Analysis") {
                TimOutRefresh++;

                if (SETS.Data.ID_CAM == DLS.Slave)
                {        SamplsOLL = Calc.BlobsSlave;
                } else { SamplsOLL = Calc.BlobsMaster; }

                double ratio3 = 0;
                // Діаграма швиткості середння Kg/h
                if (SpidIdxEv != 0) { ratio3 = (((SamplsOLL) / (double)sampleCount)) / ((double)(SpidIdxEv / 2) / (double)timeInSeconds); }   // Діаграма швиткості середння
                SpidIdxEv++;
                if (TimOutRefresh >= 10) { TimOutRefresh = 0;

                    // Отримання поточного часу та значення для додавання до даних
                    DateTime currentTime = DateTime.Now;
                    double ratio2 = 0;
                    if ((SpidIdxEv != 0)) { ratio2 = ((SamplsOLL - (double)RatioSamplsOll) / (double)sampleCount) * (double)timeInSeconds / 5; }   // Діаграма швиткості
                    if (ratio2 < 0) { ratio2 = 0; };



                    // Обчислення відношення хороших/поганих зразків за хвилину
                    double ratio = ((SamplsOLL - (double)Calc.BadSamples) / (double)((SamplsOLL - (double)Calc.BadSamples) + Calc.BadSamples)) * 100;
                    double ratio1 = ((double)Calc.BadSamples / SamplsOLL) * 100;

                    double GoodKg = 0.0;
                    double TotalKg = 0.0;
                    if (Calc.GoodSamples != 0) { GoodKg = ((SamplsOLL-(double)Calc.BadSamples) / (double)sampleCount); }                             // Кількість Good Kg
                    if (Calc.BadSamples != 0) { TotalKg = (SamplsOLL / (double)sampleCount); }  // Кількість Total Kg
                    RatioSamplsOll = SamplsOLL;


                    ratio = Math.Round(ratio, 4);
                    ratio1 = Math.Round(ratio1, 4);
                    ratio2 = Math.Round(ratio2, 2);
                    ratio3 = Math.Round(ratio3, 2);

                    GoodKg = Math.Round(GoodKg, 2);
                    TotalKg = Math.Round(TotalKg, 2);



                    ///////////// Додавання даних до таблиці ////////////
                    dataGridView1.Rows.Add(currentTime, ratio, ratio1, ratio2, GoodKg, TotalKg);

                    /////////////// Діаграма швиткості ////////////////
                    solidGauge1.Value = ratio2;        // Митєва швиткість раз в 5 секунд   
                    SpidKgH.Text = ratio3.ToString();  // Швиткість Kg\H

                    // Прокрутка таблиці до останнього рядка
                    dataGridView1.FirstDisplayedScrollingRowIndex = rowIndex;
                    rowIndex++;

                    //if (Calc.BadSamples != RatioSampls) {}
                    int Ratio = Calc.BadSamples - RatioSampls;
                    RatioSampls = Calc.BadSamples;

                    //double value = Math.Sin((currentTime - startTime).TotalSeconds);
                    // ratio = Math.Round(ratio, 2);
                    // Додавання даних до серії
                    chartValues.Add(new ObservablePoint(currentTime.Ticks, Ratio));

                    // Оновлення видимої області графіка
                    cartesianChart1.AxisX[0].MaxValue = currentTime.Ticks;
                    cartesianChart1.AxisX[0].MinValue = currentTime.Ticks - TimeSpan.FromSeconds((double)numericUpDown7.Value).Ticks;

                    // Оновлення видимої області графіка
                    cartesianChart1.AxisY[0].MaxValue = SETS.Data.AxisYMaxValue;
                    cartesianChart1.AxisY[0].MaxRange = 1;

                    cartesianChart1.AxisY[0].MinValue = 0;
                    // Налаштування формату стовпців для відображення без дробної частини


                    // Обмеження кількості точок на графіку
                    if (chartValues.Count > (double)numericUpDown6.Value)
                        chartValues.RemoveAt(0);

                    // Оновлення міток осі X з часом у форматі "HH:mm:ss"
                    cartesianChart1.AxisX[0].LabelFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");
                } }

        }


        ////////////////////////////////////////////////////////////////////////////////////////
        static int ImgListCout = 0;
        static int ErceCont = 0;
        static int srtVisulMosaic;
        static int chartIDX;
        static int TimerCountChart1 = 0;
        private static int MosaicsCoutOllBad = 0;
        private static int MosaicsCoutOllGood = 0;


        private void RefreshMosaics()
        {


            if ((button2.Text != "Start Analysis") && (TimerCountChart1 >= (numericUpDown3.Value * 2)))
            {

                chartIDX++;
                // FlowAnalis.ContaminationCount = 0;
                TimerCountChart1 = 0;
            }

            TimerCountChart1++;



            if ((MosaicDTlist.Count != 0) && (ImgListCout <= (Convert.ToInt32(PageCauntMosaic.Text))))
            {


                // if (ImgListCout < MosaicDTlist.Count) { ErceCont = 0; } else { ErceCont++; };
                // if (ErceCont >= 10) { ErceCont = 0; FlowAnalis.QualityRecognition = false; }
                //візуалізація мозаїки
                int Q = ImgListCout;
                for (; Q < MosaicDTlist.Count; Q++)
                {
                    if (Q >= (Convert.ToInt32(PageCauntMosaic.Text))) { break; }
                    if ((srtVisulMosaic < 100000))
                    {
                        if (MosaicDTlist[Q].Img != null)
                        {
                            if (MouseAddImage.Checked) { }
                            MosaicGrey.Add(MosaicDTlist[Q].Img[0]);
                            Mosaics.Images.Add(MosaicDTlist[Q].Img[0].ToBitmap());
                            listView1.LargeImageList = Mosaics;
                            listView1.Items.Add(new ListViewItem { ImageIndex = MosaicsCoutOllBad, Text = MosaicsCoutOllBad.ToString() + "S",  /*BackColor = Color.Blue,*/  });
                            MosaicsCoutOllBad++;
                            srtVisulMosaic++;
                        }
                    } else { ClearMosaic(); }
                } ImgListCout = Q;

            }



        }


        void ClearMosaic()
        {
            Mosaics.Images.Clear();
         
            listView1.Clear();
            srtVisulMosaic = 0;
            MosaicDTlist = new List<DTLimg>();
            MosaicsCoutOllBad = 0;
            MosaicGrey.Clear();
            MosaicsTeachGrey.Clear();
            ImgListCout = 0;

        }

        void ClearMosaicVi()
        {
            Mosaics.Images.Clear();
            listView1.Clear();
            srtVisulMosaic = 0;
            MosaicsCoutOllBad = 0;
            MosaicGrey.Clear();
            MosaicsTeachGrey.Clear();
            ImgListCout = 0;

        }



        private void InstMosaics()
        {
            // listView1.View = View.LargeIcon;                //відображати назву картинкі
            Mosaics.ImageSize = new Size(100, 100);      //розмір виводу картинкі
            Mosaics.ColorDepth = ColorDepth.Depth16Bit;
            // Mosaics[Master].Images.Add ( imig);               //додати картинку до списку   
            // Set the view to show details.
            //listView1.View = View.Details;
            // Allow the user to edit item text.
            listView1.LabelEdit = true;
            // Allow the user to rearrange columns.
            listView1.AllowColumnReorder = true;
            // Display check boxes.
            // listView1.CheckBoxes = true;
            // Select the item and subitems when selection is made.
            listView1.FullRowSelect = true;
            // Display grid lines.
            listView1.GridLines = true;
            //Sort the items in the list in ascending order.
            // listView1.Sorting = SortOrder.Ascending;





            // listView1.View = View.LargeIcon;                //відображати назву картинкі
            MosaicsTeach.ImageSize = new Size(100, 100);      //розмір виводу картинкі
            MosaicsTeach.ColorDepth = ColorDepth.Depth16Bit;
            listView2.LabelEdit = true;
            // Allow the user to rearrange columns.
            listView2.AllowColumnReorder = true;

            listView2.FullRowSelect = true;

            listView2.GridLines = true;
            ;


            MosaicDTlist = new List<DTLimg>();
        }


        //***********************************************************************************************************************************

        private void MosaicaEvent(DTLimg dTLimg)
        {
            BeginInvoke((MethodInvoker)(() => MosaicDTlist.Add(dTLimg)));  // добавляєм нормальні семпли
        }


        //*********************************************************************************************************************************




        private void button13_Click(object sender, EventArgs e)
        {


            // buttonStartAnalic.Enabled = false;
            FlowAnalis.StartAnais = true;//включення живого відео

            if (buttonStartAnalic.Text == "Start Analysis"){
                SEPARATOR.ON();  // Metal separator
                AUTOLOADER.ON();  // Autoloder
                buttonStartAnalic.BackColor = Color.Red;

                LIGHT.ON();
                COOLING.ON();

                buttonStartAnalic.Text = "Stop Analysis";
                StartTable.Text = "Stop Table";
                OnLight.Text =  "OFF Light";
                button40.Text = "OFF Cooling";
                button43.Text = "OFF Autoloader";
                button44.Text = "OFF Metal separator";
                Thread.Sleep(500);
                Flow.STARTsorting = true;
                Flow.StartSorting();
                VIBR_TABLE.SET(VIBR_TABLE.Typ.ON);
            }else {

                buttonStartAnalic.BackColor = Color.GreenYellow;
                

                VIBR_TABLE.SET(VIBR_TABLE.Typ.OFF);
                Flow.StartSorting();
                Flow.STARTsorting = false;
                Thread.Sleep(100);

                LIGHT.OFF();
                COOLING.OFF();

                buttonStartAnalic.Text = "Start Analysis";
                StartTable.Text = "Start Table";
                OnLight.Text  = "ON Light";
                button40.Text = "ON Cooling";
                button43.Text = "ON Autoloader";
                button44.Text = "ON Metal separator";
            }
        }


   

        /// <summary>
        /// -----------------------    SAVE VALUE   ----------------------------------------+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Save_Click(object sender, EventArgs e) {

              STGS.DT.SampleType = TextBoxSemplTyp.Text;
            // Запис у файл
              STGS.Save();


             SaveSetValue();
             _SETS.Save();
  


  

        }

        void SaveSetValue()
        {
              //CAMERA
            SETS. Data.GEIN1    = GAIN1.Value;
            SETS.Data.GEIN2    = GAIN2.Value;
            if (SETS.Data.ID_CAM == DLS.Master) { 
                SETS.Data.ACQGEIN1 = numericACQ_Gain.Value;} else {
                SETS.Data.ACQGEIN2 = numericACQ_Gain.Value;}


                SETS.Data.ACQ_Pach = textBox5.Text;
                SETS.Data.AnalisLock = AnalisLock.Checked;
            //-----------------------------------
            SETS.Data.ShowGoodMosaic = checkBox1.Checked;
            SETS.Data.SetingsCameraStart = SetingsCameraStart.Checked;
            SETS.Data.CameraAnalis_1     = checkBox4.Checked;
            SETS.Data.CameraAnalis_2     = checkBox3.Checked;
            SETS.Data.PashTestIMG        = textBoxTestImg.Text;
          if (radioButtonCam1.Checked) { 
             SETS.Data.ID_CAM = DLS.Master;} else {
             SETS.Data.ID_CAM = DLS.Slave; }

 

            SETS.Data.BlobsInvert = InvertBlobs.Checked;

            USB_HID.Data.Light_IR     = LockIR.Checked;
            USB_HID.Data.Light_Top    = LockTop.Checked;
            USB_HID.Data.Light_Back   = LockBack.Checked;
            USB_HID.Data.Light_Bottom = LockBottom.Checked;




            VIS.Data.blurA      = (byte)numericUpDown10.Value;
            VIS.Data.ThresholdA = (byte)numericUpDown11.Value;
            VIS.Data.maxValueA  = (byte)numericUpDown9.Value;

            VIS.Data.blurB = (byte)numericUpDown12.Value;
            VIS.Data.ThresholdB = (byte)numericUpDown13.Value;
            VIS.Data.ArcLengthB = (int) numericUpDown5.Value;

        }

        void RefreshSetings()
        {
            try
            {

                SetingsCameraStart.Checked = SETS.Data.SetingsCameraStart;
                checkBox4.Checked = SETS.Data.CameraAnalis_1;
                checkBox3.Checked = SETS.Data.CameraAnalis_2;
                textBoxTestImg.Text = SETS.Data.PashTestIMG;

                 checkBox1.Checked = SETS.Data.ShowGoodMosaic;

                if (SETS.Data.ID_CAM == DLS.Master) { radioButtonCam1.Checked = true; }
                                                else{ radioButtonCam2.Checked = true; }

                //---------------------------------------------------

                GreyScaleMax_.Value = EMGU.Data.GreyScaleMax[ID];
                GreyScaleMin_.Value = EMGU.Data.GreyScaleMin[ID];

                GreyMax_.Value = (decimal)EMGU.Data.GreySizeMax[ID];
                GreyMin_.Value = (decimal)EMGU.Data.GreySizeMin[ID];

                Hz_Table.Value = USB_HID.Data.Hz_Table;
                PWM_Table.Value = USB_HID.Data.PWM_Table;
                OutputDelay.Value = USB_HID.Data.Fleps_Time_OFF;
                FLAPS.Time_OFF(OutputDelay.Value);

                checkBox13.Checked = SETS.Data.LiveVideoOFF; 
        

                numericUpDown1.Value = SETS.Data.DoublingFlaps; //
                numericUpDown6.Value = SETS.Data.LimitinGraphPoints;
                numericUpDown7.Value = SETS.Data.UpdateVisibleArea;
                numericUpDown8.Value = SETS.Data.AxisYMaxValue;

                GAIN1.Value        = SETS.Data.GEIN1;
                GAIN2.Value        = SETS.Data.GEIN2;
                textBox5.Text      = SETS.Data.ACQ_Pach;
                AnalisLock.Checked = SETS.Data.AnalisLock;

                if (SETS.Data.ID_CAM==DLS.Master) { numericACQ_Gain.Value = SETS.Data.ACQGEIN1; }
                                             else { numericACQ_Gain.Value = SETS.Data.ACQGEIN2; }



                InvertBlobs.Checked = SETS.Data.BlobsInvert;

                LockIR.Checked = USB_HID.Data.Light_IR;
                LockTop.Checked = USB_HID.Data.Light_Top;
                LockBack.Checked = USB_HID.Data.Light_Back;
                LockBottom.Checked = USB_HID.Data.Light_Bottom;




                numericUpDown10.Value =  VIS.Data.blurA;
                numericUpDown11.Value = VIS.Data.ThresholdA;
                numericUpDown9.Value   = VIS.Data.maxValueA;

                numericUpDown12.Value = VIS.Data.blurB;
                numericUpDown13.Value = VIS.Data.ThresholdB;
                numericUpDown5.Value = VIS.Data.ArcLengthB;
            }
            catch
            {
                
                Help.Mesag("saved data not correct !");
            }
        }




   

        private void MastConturMax_ValueChanged(object sender, EventArgs e){

            EMGU.Data.GreySizeMax[ID] = (double)GreyMax_.Value;
        }



        private void GreyMin__ValueChanged(object sender, EventArgs e)
        {
            EMGU.Data.GreySizeMin[ID] = (double)GreyMin_.Value;
        }

        //****************************/


        private void MastMinR_ValueChanged(object sender, EventArgs e)
        {
           
            EMGU.Data.GreyScaleMax[ID] = (int)GreyScaleMax_.Value;

        }



        private void GreyScaleMin__ValueChanged(object sender, EventArgs e)
        {
           
            EMGU.Data.GreyScaleMin[ID] =(int)GreyScaleMin_.Value;
        }


        /************************/

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (USB_HID.HidStatus == true)
            {
                HidConect.Text = "connected"; HidConect.ForeColor = Color.Green;
            }
            else { HidConect.Text = "not connected"; HidConect.ForeColor = Color.Red; }

            ProcesingAnalis.Text = Flow.CountProcesingCamera.ToString();
            BuferImgIdx.Text = FlowCamera.BuferImg.Count.ToString();
            BuferImgCaun.Text = FlowCamera.BoxImgM.Count.ToString();
            CauntListImages.Text = FlowCamera.ImgSave.Count.ToString();

            if (SETS.Data.ID_CAM== DLS.Slave) {
                    CauntSamls.Text = Calc.BlobsSlave.ToString();
            }else { CauntSamls.Text = Calc.BlobsMaster.ToString(); }

            toolStripStatusLabel5.Text = DLS.elapsedMs.ToString();
            toolStripStatusLabel6.Text = FlowCamera.BatchSizePreict.ToString();

            if (FlowCamera.LiveImage != null) {
                LiveView.Image = FlowCamera.LiveImage.ToBitmap();
            }
            TimerRefreshChart();

            RefreshMosaics();


        }

        private void USER_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSetings();
        }

        private void Soreter_FormClosed(object sender, FormClosedEventArgs e)
        {
            Flow.StopPotocBlobsPredict();
            Flow.StopPotocAnalisBlobs();
            Flow.StopPotocHID();
            CAMERA.OFF();

        }

        static Mat TestImage = new Mat();
        private void button13_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // дії, які виконуються при виборі картинки

                Stopwatch watch;
                watch = Stopwatch.StartNew();
                TestImage = CvInvoke.Imread(openFileDialog1.FileName);
                LiveView.Image = TestImage.ToImage<Gray, Byte>().ToBitmap();
                watch.Stop();
                Console.WriteLine("Load Model: -- " + watch.ElapsedMilliseconds + " ms");
            }
        }

        FlowAnalis flowAnalis = new FlowAnalis();
        private void button14_Click(object sender, EventArgs e)
        {
            Image<Gray, Byte> img = new Image<Gray, Byte>(100, 100);

            img = TestImage.ToImage<Gray, Byte>();

            FlowAnalis.StartAnais = true;


            // flowAnalis.FindBlobs(img);


            // Flow flow = new Flow();
            // flow.PredictFlow(img);
            //flow.PredictFlow(img);
            //flow.PredictFlow(img);
            //flow.PredictFlow(img);
            //flow.PredictFlow(img);
            //flow.PredictFlow(img);

            IProducerConsumerCollection<Image<Gray, byte>> tmp2 = FlowCamera.BoxM; //створити ліст імідж
            tmp2.TryAdd(img /*.Resize(4096, 50, Inter.Linear)*/);




            // LiveView.Image =FlowAnalis.Img_Test;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearMosaic();

        }


        int ImagCouAnn;
        Bitmap LearnImg = new Bitmap(100, 100);
        int SelectITMs;
        private void button15_Click(object sender, EventArgs e)
        {

            MosaicsTeach.Images.Add(MosaicGrey[SelectITMs].ToBitmap());
            MosaicsTeachGrey.Add(MosaicGrey[SelectITMs]);

            listView2.LargeImageList = MosaicsTeach;
            listView2.Items.Add(new ListViewItem { ImageIndex = ImagCouAnn, Text = "Images",  /*nema imag*/ });
            ImagCouAnn++;
            // label21.Text = MY_ML.ImagCouAnn.ToString();
            // label21.Text += " PCS ";

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {


            if (MouseAddImage.Checked == true)
            {
                DTLimg.SelectITM.Add(SelectITMs);

                MosaicsTeach.Images.Add(MosaicGrey[SelectITMs].ToBitmap());
                MosaicsTeachGrey.Add(MosaicGrey[SelectITMs]);
                listView2.LargeImageList = MosaicsTeach;
                listView2.Items.Add(new ListViewItem { ImageIndex = ImagCouAnn, Text = "Images",  /*nema imag*/ });

                ImagCouAnn++;
                // label21.Text = MY_ML.ImagCouAnn.ToString();
                // label21.Text += " PCS ";


            }
            else { Help.Mesag("You cannot select image or generate a report when the images are not sorted! "); }

        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {


            if (listView1.FocusedItem != null)
            {
                SelectITMs = listView1.FocusedItem.Index;
                listView1.SelectedItems.Clear();
                listView1.Select();
                listView1.Items[listView1.FocusedItem.Index].Selected = true;
                listView1.Items[listView1.FocusedItem.Index].Focused = true;


                LearnImg = new Bitmap(Mosaics.Images[SelectITMs]);

            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            ImagCouAnn = 0;
            listView2.Clear();
            MosaicsTeach.Images.Clear();
            DTLimg.SelectITM.Clear();
            MosaicsTeachGrey.Clear();
        }

        private void button17_Click(object sender, EventArgs e)
        {


            SaveSample(true);


        }


        void SaveSample(bool AskMsg)
        {


            if (comboBox2.Text == "")
            {
                MessageBox.Show("The name field cannot be empt", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            };






            string pashImg = Path.Combine(Application.StartupPath, "Data", comboBox2.Text); //створити назву Img
            DialogResult result = DialogResult.Yes;
            if (AskMsg == true) { result = MessageBox.Show("Do you want Add Images ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information); }



            if (false == Directory.Exists(pashImg)) { Directory.CreateDirectory(pashImg); }
            if (result == DialogResult.Yes)
            {

                for (int i = 0; i < MosaicsTeach.Images.Count; i++)
                {
                    DateTime dateOnly = new DateTime();
                    dateOnly = DateTime.Now;

                    String DataFile = dateOnly.Month.ToString() + ".";
                    DataFile = DataFile + dateOnly.Day.ToString() + ".";
                    DataFile = DataFile + dateOnly.Year.ToString() + ". ";
                    DataFile = DataFile + dateOnly.Hour.ToString() + ".";
                    DataFile = DataFile + dateOnly.Minute.ToString() + ".";
                    DataFile = DataFile + dateOnly.Second.ToString() + " ";




                    Bitmap IMGconvert = new Bitmap(MosaicsTeach.Images[i], 100, 100);


                    // MosaicsTeach.Images[i].Save(pashImg + "\\" + DataFile + "img" + i.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    //IMGconvert.Save(pashImg + "\\" + DataFile + "img" + i.ToString() + ".jpg", System.Drawing.Imaging. ImageFormat.Jpeg);



                    MosaicsTeachGrey[i].Save(pashImg + "\\" + DataFile + "img" + i.ToString() + ".jpg");





                }

            }



        }



        private void button18_Click(object sender, EventArgs e)
        {
            //  ML.Inst(Path.Combine(Application.StartupPath, "Data")); 
        }

        private void button19_Click(object sender, EventArgs e)
        {
            // Flow flow = new Flow();
            Flow.BlobsPredict();
            //Flow.FindBlobCam();
        }


        private void PWM_Table_Click(object sender, EventArgs e)
        {

            if (StartTable.Text == "Stop Table")
            { VIBR_TABLE.SET(VIBR_TABLE.Typ.PWM, PWM_Table.Value); }
            else { USB_HID.Data.PWM_Table = PWM_Table.Value; }

        }



        private void Hz_Table_ValueChanged(object sender, EventArgs e)
        {
            if (StartTable.Text == "Stop Table")
            { VIBR_TABLE.SET(VIBR_TABLE.Typ.Frequency, Hz_Table.Value); }
            else { USB_HID.Data.Hz_Table = Hz_Table.Value; } }

        private void StartTable_Click(object sender, EventArgs e)
        {
            if (StartTable.Text == "Start Table") {
                VIBR_TABLE.SET(VIBR_TABLE.Typ.PWM, PWM_Table.Value);
                VIBR_TABLE.SET(VIBR_TABLE.Typ.Frequency, Hz_Table.Value);
                StartTable.Text = "Stop Table";
            }
            else
            {
                VIBR_TABLE.SET(VIBR_TABLE.Typ.PWM, 0);
                VIBR_TABLE.SET(VIBR_TABLE.Typ.Frequency, 0);
                StartTable.Text = "Start Table";
            }
        }

        private void OnLight_Click(object sender, EventArgs e) {
            if (OnLight.Text == "ON Light") {
                LIGHT.ON();

                OnLight.Text = "OFF Light";
            } else {
                LIGHT.OFF();


                OnLight.Text = "ON Light";
            }

        }


        private void buttonLiveVideo_Click(object sender, EventArgs e)
        {
            if (buttonLiveVideo.Text == "LIVE VIDEO ON") {
                Flow.STARTsorting = true;
                Flow.StartSorting();
                buttonLiveVideo.Text = "LIVE VIDEO OFF";

            }
            else {
                Flow.StartSorting();
                Flow.STARTsorting = false;
                buttonLiveVideo.Text = "LIVE VIDEO ON";

            }


        }

        private void SaveImgList_CheckedChanged(object sender, EventArgs e)
        {
            FlowCamera.SaveImages = SaveImgList.Checked;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            FlowCamera.ImgSave.Clear();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (FlowCamera.ImgSave.Count > 0)
            {
                for (int i = 0; i < FlowCamera.ImgSave.Count; i++)
                {

                    Image<Gray, byte> ImagAI = new Image<Gray, byte>(100, 100);
                    FlowCamera.ImgSave.TryDequeue(out ImagAI);
                    if (textBoxTestImg.Text == "") { break; }
                    ImagAI.Save(textBoxTestImg.Text + "\\" + "Image" + i.ToString() + ".jpg");


                }
            }
        }
        //static ANLImg_M AnalisI_M = new ANLImg_M();
        static int CantIdx = 0;

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK)
                {

                    textBoxTestImg.Text = FBD.SelectedPath;
                }
                else { MessageBox.Show("Choose directory please", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        int IdxShou = 0;
        static Mat NewMat = new Mat();
        static Image<Gray, byte> img = new Image<Gray, byte>(100, 100);
        public static int CountContact = 0;
        Image<Gray, byte> ImagContact;

        static Mat NewMatAI = new Mat();
        static Image<Gray, byte> imgAI = new Image<Gray, byte>(100, 100);
        public static int CountContactAI = 0;
        static Image<Gray, byte> ImagContactAI;


        private void timer2_Tick(object sender, EventArgs e)
        {

            string urlMaster = textBoxTestImg.Text + "\\" + "Image" + IdxShou++ + ".jpg";
            FlowAnalis.Setings = true;
            string[] files = Directory.GetFiles(textBoxTestImg.Text, "*.jpg");

            int count = files.Length;


            if (IdxShou <= files.Length)
            {
                try
                {
                    Bitmap imM = new Bitmap(urlMaster);
                    textBox2.Text = IdxShou.ToString();
                    Emgu.CV.Mat imOriginalM = imM.ToImage<Bgr, byte>().Mat;
                    IProducerConsumerCollection<Image<Gray, byte>> CollecTempM = FlowCamera.BoxImgM;
                    CollecTempM.TryAdd(imOriginalM.ToImage<Gray, byte>());

                    Bitmap imS = new Bitmap(urlMaster);
                    textBox2.Text = IdxShou.ToString();
                    Emgu.CV.Mat imOriginalS = imS.ToImage<Bgr, byte>().Mat;
                    IProducerConsumerCollection<Image<Gray, byte>> CollecTempS = FlowCamera.BoxImgS;
                    CollecTempS.TryAdd(imOriginalS.ToImage<Gray, byte>());



                }
                catch { }
            }
            else { }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            IdxShou = 0;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (button23.Text == "Run")
            {
                button23.Text = "Pause";
                timer2.Enabled = true;
            }
            else
            {

                button23.Text = "Run";
                timer2.Enabled = false;
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            timer2_Tick(null, null);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            IdxShou--;
            IdxShou--;
            timer2_Tick(null, null);
        }


        private void button26_Click(object sender, EventArgs e) {

            ClearMosaicVi();

            int Q = Convert.ToInt32(NabrMosaicVisul.Text);

            ImgListCout = Q;
            for (; Q < MosaicDTlist.Count; Q++) {
                if (Q >= (Convert.ToInt32(NabrMosaicVisul.Text) + Convert.ToInt32(PageCauntMosaic.Text))) {
                    NabrMosaicVisul.Text = (Convert.ToInt32(NabrMosaicVisul.Text) + Convert.ToInt32(PageCauntMosaic.Text)).ToString(); break; }
                if ((srtVisulMosaic < 100000))
                {
                    if (MosaicDTlist[Q].Img != null)
                    {
                        if (MouseAddImage.Checked) { }
                        MosaicGrey.Add(MosaicDTlist[Q].Img[0]);
                        Mosaics.Images.Add(MosaicDTlist[Q].Img[0].ToBitmap());
                        listView1.LargeImageList = Mosaics;
                        listView1.Items.Add(new ListViewItem { ImageIndex = MosaicsCoutOllBad, Text = MosaicsCoutOllBad.ToString() + "S",  /*BackColor = Color.Blue,*/  });
                        MosaicsCoutOllBad++;
                        srtVisulMosaic++;
                    }
                }
                else { ClearMosaic(); }
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {

            if ((Convert.ToInt32(NabrMosaicVisul.Text) != (Convert.ToInt32(PageCauntMosaic.Text))))
            {
                ClearMosaicVi();
                int Q = Convert.ToInt32(NabrMosaicVisul.Text);
                ImgListCout = Q;
                if (Q != Convert.ToInt32(PageCauntMosaic.Text)) NabrMosaicVisul.Text = (Convert.ToInt32(NabrMosaicVisul.Text) - Convert.ToInt32(PageCauntMosaic.Text)).ToString();
                for (; Q < MosaicDTlist.Count; Q++) {
                    if ((srtVisulMosaic < 100000))
                    {
                        if (MosaicDTlist[Q].Img != null)
                        {
                            if (MouseAddImage.Checked) { }
                            MosaicGrey.Add(MosaicDTlist[Q].Img[0]);
                            Mosaics.Images.Add(MosaicDTlist[Q].Img[0].ToBitmap());
                            listView1.LargeImageList = Mosaics;
                            listView1.Items.Add(new ListViewItem { ImageIndex = MosaicsCoutOllBad, Text = MosaicsCoutOllBad.ToString() + "S",  /*BackColor = Color.Blue,*/  });
                            MosaicsCoutOllBad++;
                            srtVisulMosaic++;
                        }
                    }
                    else { ClearMosaic(); }
                }
            } else { NabrMosaicVisul.Text = PageCauntMosaic.Text; }

        }

        private void PageCauntMosaic_TextChanged(object sender, EventArgs e)
        { ClearMosaicVi();
            NabrMosaicVisul.Text = PageCauntMosaic.Text;
        }

        #region FLEPS
        private void button27_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps1, true); }
        private void button28_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps2, true); }
        private void button29_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps3, true); }
        private void button30_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps4, true); }
        private void button31_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps5, true); }
        private void button32_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps6, true); }
        private void button33_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps7, true); }
        private void button39_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps8, true); }
        private void button42_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps9, true); }
        private void button45_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps10, true); }
        private void button46_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps11, true); }
        private void button47_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps12, true); }
        private void button48_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps13, true); }
        private void button49_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps14, true); }
        private void button50_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps15, true); }
        private void button52_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps16, true); }
        private void button53_Click(object sender, EventArgs e) { FLAPS.SET(FLAPS.Typ.Fps17, true); }

        #endregion FLEPS

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            AnalisPredict.MosaicShowOll = checkBox7.Checked;
        }

        private void checkBox13_Click(object sender, EventArgs e)
        {
            SETS.Data.LiveVideoOFF = checkBox13.Checked;
        }



        private void СontourSelect_CheckedChanged(object sender, EventArgs e)
        {
            AnalisPredict.FlapsTest = СontourSelect.Checked;
        }



        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            numericUpDown1_Click(null, null);
        }

        private void numericUpDown1_Click(object sender, EventArgs e)
        {
            SETS.Data.DoublingFlaps = (int)numericUpDown1.Value;
        }

        private void Flapslocking_Click(object sender, EventArgs e)
        {
            AnalisPredict.Flapslocking = Flapslocking.Checked;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e) { SETS.Data.LimitinGraphPoints = (int)numericUpDown6.Value; }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e) { SETS.Data.UpdateVisibleArea = (int)numericUpDown7.Value; }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e) { SETS.Data.AxisYMaxValue = (int)numericUpDown8.Value; }


        /**********************                              _____________________________                 *********/



      int   IdxShouTest=0;
        string[] files;
        VIS vision = new VIS();

        Emgu.CV.Mat imOriginalM;

       void TestImgBlb() {

            string urlMaster = textBox4.Text + "\\" + "Image" + IdxShouTest++ + ".jpg";
        
           files = Directory.GetFiles(@textBox4.Text, "*.jpg");

            int count = files.Length;


            if (IdxShouTest <= files.Length)
            {
                try
                {
                    Bitmap imM = new Bitmap(files[IdxShouTest]);
                    textBox3.Text = IdxShouTest.ToString();

                    imOriginalM = imM.ToImage<Bgr, byte>().Resize(64,64,interpolationType: Inter.Linear).Mat;

                    Stopwatch watch = Stopwatch.StartNew();

                    //for (int i = 0; i < 100; i++){}

                    Image<Bgr, byte> ImagesViw = new Image<Bgr, byte>(100,100);
                    if (AnalysisTest.Checked)
                    {
                             ImagesViw = vision.DetectBlob(imOriginalM);
                    } else { 
                        ImagesViw = vision.DetectBlobBlack(imOriginalM); }
                    


                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    toolStripStatusLabel5.Text = elapsedMs.ToString();
                   // Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

                    pictureBox1.Image = ImagesViw.ToBitmap();
                    pictureBox2.Image = imOriginalM.ToBitmap();


                }
                catch { }
            }
            else { }

        }


      void SetingsValGrayImg (){
 try{
            if (IdxShouTest <= files.Length)
            {
               
                
                    Bitmap imM = new Bitmap(files[IdxShouTest]);
                    textBox3.Text = IdxShouTest.ToString();

                    imOriginalM = imM.ToImage<Bgr, byte>().Resize(64, 64, interpolationType: Inter.Linear).Mat;

                    Stopwatch watch = Stopwatch.StartNew();


                    Image<Bgr, byte> ImagesViw = new Image<Bgr, byte>(100, 100);

                    if (AnalysisTest.Checked)
                    {
                        ImagesViw = vision.DetectBlob(imOriginalM);
                    }
                    else
                    {
                        ImagesViw = vision.DetectBlobBlack(imOriginalM);
                    }


                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    toolStripStatusLabel5.Text = elapsedMs.ToString();
                    // Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

                    pictureBox1.Image = ImagesViw.ToBitmap();
                    pictureBox2.Image = imOriginalM.ToBitmap();


                }
               
            } catch { }

        }





        private void button41_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK)
                {

                    textBox4.Text = FBD.SelectedPath;
                }
                else { MessageBox.Show("Choose directory please", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            IdxShouTest = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IdxShouTest --;
            IdxShouTest --;
            TestImgBlb();
        }
        private void button4_Click(object sender, EventArgs e)
        { TestImgBlb();

        }

        private void numericUpDown9_Click(object sender, EventArgs e)
        {
            SetingsValGrayImg();
            SaveSetValue();
        }

        private void numericUpDown11_Click(object sender, EventArgs e)
        {
            SetingsValGrayImg();
            SaveSetValue();
        }

        private void numericUpDown13_Click(object sender, EventArgs e)
        {
            SetingsValGrayImg();
            SaveSetValue();
        }
        private void numericUpDown10_Click(object sender, EventArgs e)
        {
            SetingsValGrayImg();
            SaveSetValue();
        }

        private void numericUpDown12_Click(object sender, EventArgs e)
        {
            SetingsValGrayImg();
            SaveSetValue();
        }

        short coutTim=0;

        private void timer3_Tick(object sender, EventArgs e)
        {

            //if (coutTim==5) {  DLS. InstCOM_Setings(DLS.Slave);}

            if (coutTim == 5) {
                coutTim++;
                try { DLS = new DLS(); }
            catch { Help.Mesag("Cameras are not connected"); }
           }

            if (coutTim > 8)
            { timer3.Stop();
                coutTim = 0;


                GAIN1.Enabled = DLS.Devis.Status[DLS.Master];
                GAIN2.Enabled = DLS.Devis.Status[DLS.Slave];

                if ((GAIN1.Value <= 10) && (GAIN1.Value >= 1))
                {
                    if (GAIN1.Value != (decimal)DLS.Devis.Gain[DLS.Master]) { GAIN1_Click(null, null); }
    
                } else { GAIN1.Value = (decimal)DLS.Devis.Gain[DLS.Master]; }


                if ((GAIN2.Value <= 10) && (GAIN2.Value >= 1))
                {
                    if (GAIN2.Value != (decimal)DLS.Devis.Gain[DLS.Slave]) { GAIN2_Click(null, null); }

                }
                else { GAIN2.Value = (decimal)DLS.Devis.Gain[DLS.Slave]; }

                // Завантаження Вирівнювання Фону
              //if(DLS.button_Load_FF_Click(SETS.Data.ACQ_Pach)) { 
               // DLS.checkBox_FaltField_Click(DLS.Master, checkBoxAcqSet.Checked);
              //  DLS.checkBox_FaltField_Click(DLS.Slave, checkBoxAcqSet.Checked);
              //  }


            }



                coutTim++;
        }

        private void GAIN1_Click(object sender, EventArgs e)
        {
            try { 
            DLS.SetGain((double)GAIN1.Value, DLS.Master);
            }
            catch {
            Help.Mesag("Reset Program"); }
        }



        private void GAIN2_Click(object sender, EventArgs e)
        {
            try
            {
                DLS.SetGain((double)GAIN2.Value, DLS.Slave);
        }
            catch {
            Help.Mesag("Reset Program"); }
}

        private void button55_Click(object sender, EventArgs e)
        {
            DLS.button_Load_FF_Click(SETS.Data.ID_CAM);
        }

        private void button56_Click(object sender, EventArgs e)
        {
            DLS.checkBox_FaltField_Click(SETS.Data.ID_CAM, true);
        }

        private void button57_Click(object sender, EventArgs e)
        {
            Flow.StartSorting();
            Flow.STARTsorting = false;
            buttonLiveVideo.Text = "LIVE VIDEO ON";

            checkBoxAcqSet.Checked = false;
            LIGHT.ON();
            OnLight.Text = "OFF Light";
            DLS.Acq_Bright_Click(SETS.Data.ID_CAM);
            LIGHT.OFF();
            OnLight.Text = "ON Light";
        }

        private void Acq_Dark_Click(object sender, EventArgs e){
            Flow.StartSorting();
            Flow.STARTsorting = false;
            buttonLiveVideo.Text = "LIVE VIDEO ON";

            checkBoxAcqSet.Checked = false;
            LIGHT.OFF();
            OnLight.Text = "ON Light";
            DLS. Acq_Dark_Click(SETS.Data.ID_CAM);
            LIGHT.ON();
            OnLight.Text = "OFF Light";
        }

        private void button59_Click(object sender, EventArgs e)
        {
            checkBoxAcqSet.Checked = false;
            DLS.Save_Acq_File(SETS.Data.ID_CAM, SETS.Data.ACQ_Pach);
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            DLS.checkBox_FaltField_Click(SETS.Data.ID_CAM, checkBoxAcqSet.Checked);
        }

        private void zcheckBox1_Click(object sender, EventArgs e)
        {
               if (button44.Text == "ON Metal separator")
            { SEPARATOR.ON(); button44.Text = "OFF Metal separator"; } else 
            { SEPARATOR.OFF(); button44.Text = "ON Metal separator"; }
        }

        private void button43_Click(object sender, EventArgs e)
        {
            if (button43.Text == "ON Autoloader")
            { AUTOLOADER.ON(); ; button43.Text = "OFF Autoloader"; } else 
            { AUTOLOADER.OFF(); button43.Text = "ON Autoloader"; }      
        }

        private void button40_Click(object sender, EventArgs e)
        {
            if (button40.Text == "ON Cooling")
            { COOLING.ON(); ; button40.Text = "OFF Cooling"; }
            else
            { COOLING.OFF(); button40.Text = "ON Cooling"; }
        }

        private void radioButtonCam1_Click(object sender, EventArgs e)
        { SETS.Data.ID_CAM = DLS.Master; RefreshSetings(); }

        private void radioButtonCam2_Click(object sender, EventArgs e)
        { SETS.Data.ID_CAM = DLS.Slave; RefreshSetings(); }

        private void ACQ_PachButton(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK) {

                    textBox5.Text = FBD.SelectedPath;
                }else { MessageBox.Show("Choose directory please", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }







        //____________________________________Save semple Type____________________________________________________



        private void CreatSamplTyp_Click(object sender, EventArgs e)
        {

            if (textBoxСreateSample.Text != "")
            {

                DialogResult YESNO = MessageBox.Show("Do you want add new sample type " + textBoxСreateSample.Text + "  ?", "Waring!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if ( YESNO == DialogResult.Yes)
                {



                    var PathType = Path.Combine(STGS.DT.URL_SampleType, textBoxСreateSample.Text);

                    if (false == Directory.Exists(STGS.DT.URL_SampleType)) { Directory.CreateDirectory(STGS.DT.URL_SampleType); }
                    if (false == Directory.Exists(PathType)) { Directory.CreateDirectory(PathType); }
                    textBoxСreateSample.Text = "";
                }
            }
        }

    



        void LoadSempelsName()
        {
            try
            {
                int idx = 0;
                string[] SamlCatalogPath = new string[Directory.GetDirectories(STGS.DT.URL_SampleType).Length];
                string[] pathSmpl = new string[Directory.GetDirectories(STGS.DT.URL_SampleType).Length];
                SamlCatalogPath = Directory.GetDirectories(STGS.DT.URL_SampleType);


                for (idx = 0; idx < SamlCatalogPath.Length; idx++)
                { pathSmpl[idx] += Path.GetFileName(SamlCatalogPath[idx]); }

                comboBoxSetingsName.Items.Clear();
                comboBox1.Items.Clear();

                int x = 0;
                string[] NemSmpls = new string[pathSmpl.Length];

                foreach (var i in pathSmpl)
                {
                    if ((DTLimg.NameGp == null) || (DTLimg.NameGp.Length != NemSmpls.Length)) { DTLimg.NameGp = new string[pathSmpl.Length]; }
                    DTLimg.NameGp[x] = i;
                    comboBoxSetingsName.Items.Add(i);
                    comboBox1.Items.Add(i);
                    NemSmpls[x++] = i;
                }

              //  DSV.TF_DT.Name = NemSmpls;


            } catch { Help.Mesag(" problem with Catalogue (Sample Type) "); }
        }

        private void buttonDeleteTypeSempl_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Yes;
            result = MessageBox.Show("Do you want delete sample type '" + comboBox1.Text + "' ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);


            if (result == DialogResult.Yes)
            {
                if ((TextBoxSemplTyp.Text != comboBox1.Text)&&(comboBox1.Text != STGS.DT.SampleType))
                {
                    
                    //видалити деректрію
                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(STGS.DT.URL_SampleType, comboBox1.Text));
                        dirInfo.Delete(true);
                        comboBox1.Text = "";
                    }
                    catch { MessageBox.Show("The directory cannot be deleted 'directory is not found' "); }
                }
                else { MessageBox.Show("The sample type cannot be deleted because it is currently in use !!!"); }
            }
            
        }

        private void comboBox1_Click(object sender, EventArgs e)
        { LoadSempelsName();}

        private void comboBoxSetingsName_Click(object sender, EventArgs e)
        { LoadSempelsName();


        
        }

        private void comboBoxSetingsName_TextChanged(object sender, EventArgs e)
        {

            if (comboBoxSetingsName.Text != "")
            {


                DialogResult result = DialogResult.Yes;
                result = MessageBox.Show("Do you want to choose new type '" + comboBoxSetingsName.Text + "' ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {

                    TextBoxSemplTyp.Text = comboBoxSetingsName.Text;
                     comboBoxSetingsName.Items.Clear();

         
                  //  result = MessageBox.Show("Apply new settings '" + comboBoxSetingsName.Text + "' ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                  //  if (result == DialogResult.Yes)
                  //  {
                        if (_SETS.Read(comboBoxSetingsName.Text))
                        {
                           RefreshSetings();
                        }else {   MessageBox.Show("Unable to apply new settings!!! Maybe the file doesn't exist yet, you need to make the first save. If you press save, the current settings will be saved in the" + "sample type" + "file");}; 
                   // }

                    }
            }
            


        }

        private void OutputDelay_Click(object sender, EventArgs e)
        {
            FLAPS.Time_OFF(OutputDelay.Value);
        }


    }

}
