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
using STATUS = C2S150_ML.USB_HID.PLC_C2S150.STATUS;

using Emgu.CV.CvEnum;
using System.Threading.Tasks;
using System.Threading;
using LiveCharts;
using LiveCharts.Wpf;


using LiveCharts.Defaults;
using LiveCharts.WinForms;
using System.Runtime.InteropServices;

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

        ///////////////      ====== PROCES REAL_TIME ===     ///////////////////////////////////////////////////////////////
        [DllImport("Kernel32.dll")]
        static extern bool SetPriorityClass(IntPtr hProcess, int dwPriorityClass);

        DLS DLS;
        static int ID;
        SETS _SETS = new SETS();
        FlowCamera flowCamera = new FlowCamera();
        USB_HID USB_HID = new USB_HID();



        public List<Image<Gray, byte>> MosaicsTeachGrey = new List<Image<Gray, byte>>();

        public ImageList Mosaics = new ImageList();
        public ImageList MosaicsTeach = new ImageList();
        static List<DTLimg> MosaicDTlist; //буфер ліст для буферезації перед сортуваням




        private ChartValues<ObservablePoint> chartValues;
        private DateTime startTime;
        STGS STGS = new STGS();
        Flow Flow = new Flow();

        public Sorter()
        {


            InitializeComponent();
            //Help.WriteLineInstal(ConsolMesg);
            Flow.ProcessLoadImagesFunction(true);



            //SQL.UpdateGridSet(false,dataGridView2, SQL.TimSQL.Now, SQL.TimSQL.AddDayst);


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

            //************************    запустити 1-6-1 поток      ********************************/
            Flow.AnalisBlobs();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.BlobsPredict();
            Flow.USB_Hid();

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



            //YourSeries.DataLabelsTemplate = dataLabelTemplate;

            // Додавання до Графіка даних
            LineSeries series = new LineSeries {

                Title = "The number of bad samples per set time interval",
                Values = chartValues,
                DataLabels = true, // Відображення міток даних

                LabelPoint = point => {
                    double percentage = RatioBed;
                    return $"{percentage:F0} PCS";

                },

                DataLabelsTemplate = new System.Windows.DataTemplate(),

            };

            /////----------  SQL  ----------------------------------------------------------//
            SQL.DataGridNames(dataGridView1);
            SQL.Conect();
            SQL.Updat(false, dataGridView2, dateTimePicker1.Text, dateTimePicker2.Text);
            dateTimePicker1.Text = SQL.TimSQL.Now;
            dateTimePicker2.Text = SQL.TimSQL.AddDayst;
            //-------------------------------------------------------------------------------//

            // Додавання серії даних до Cartesian Chart
            cartesianChart1.Series.Add(series);

            startTime = DateTime.Now;

            // Діаграма швиткості
            solidGauge1.To = 150;

            LoadSempelsName();
            FlowCamera.LiveViewTV(LiveView);
            USB_HID.PLC_C2S150.FLAPS.FlepsLightInstal(
              Fleps1, Fleps2, Fleps3, Fleps4, Fleps5, Fleps6, Fleps7, Fleps8, Fleps9,
              Fleps10, Fleps11, Fleps12, Fleps13, Fleps14, Fleps15, Fleps16, Fleps17);


            if (SetingsCameraStart.Checked)
            {
                timer3.Enabled = false;
                try { DLS = new DLS(); }
                catch
                {
                    coutTim = 0;
                    Help.Mesag("Cameras are not connected"); Enabled = true; timer3.Stop();
                    Flow.ProcessLoadImagesFunction(false);

                }

            }


            //LOCK FORM
            LockFunk(true);
            PaaswortString.UseSystemPasswordChar = true;
           
            ///////////////      ====== PROCES REAL_TIME ===     ///////////////////////////////////////////////////////////////
            SetPriorityClass(Process.GetCurrentProcess().Handle, 0x00000100); /////////////////////////////////////////////////
            ///////////////      ====== PROCES REAL_TIME ===     ///////////////////////////////////////////////////////////////
        }
















        private void ClearAnalisData() {
            Calc.GoodSamples = 0;
            Calc.BadSamples = 0;
            Calc.BlobsMaster = 0;
            Calc.BlobsSlave = 0;
            dataGridView1.Rows.Clear();
            rowIndex = 0;
            //dataGridView1.Columns.Clear();
            /////////////// Діаграма швиткості ////////////////
            solidGauge1.Value = 0.00;
            SpidKgH.Text = "0.00";
            RatioSamplsOll = 0;
            RatioSampls = 0;
            SamplsOLL = 0;
            TimOutRefresh = 0;
            RatioBed = 0;
            RatioTimeInSeconds = 0;

            SizeCNT.Size100 = 0;
            SizeCNT.Size500 = 0;
            SizeCNT.Size1000 = 0;
            WatchSpeed.Restart();
        }


        static short TimOutRefresh = 0;
        int RatioSampls = 0;
        double RatioSamplsOll = 0;
        double SamplsOLL = 0;
        private int rowIndex = 0;
        double SecInH = 3600; //1h=sec

        static int RatioBed = 0;
        int RatioTimeInSeconds = 0;
        static bool StartStopGrid = false;


        // Створюємо об'єкт Stopwatch
        Stopwatch stopwatch = new Stopwatch();
        // Створюємо об'єкт Stopwatch
        Stopwatch WatchSpeed = new Stopwatch();

        static DateTime DataTime;
        private void TimerRefreshChart() {







            // Отримуємо час у секундах
            int TimeInSeconds = (int)WatchSpeed.Elapsed.TotalSeconds;



            double PcsGood = 0;
            double PcsBed = 0;

            if ((buttonStartAnalic.Text == "Stop Analysis") || (StartStopGrid)) {

                TimOutRefresh++;

                //if (SETS.Data.ID_CAM == DLS.Slave) { 
               SamplsOLL = Calc.BlobsSlave;
                //} else { SamplsOLL = Calc.BlobsMaster; }
               

                double SpeedKgh = 0;
                // formula -Kg/H ------  (((oll_Pcs_Kg/cof_Weight)=Wwight)/(Tims_Sec_Work/Sec_In_H))= Kg/H
                // Діаграма швиткості середння Kg/h
                if (TimeInSeconds != 0) { SpeedKgh = (((SamplsOLL) / (double)SampleWeight.Value) / (double)TimeInSeconds) * (double)SecInH; }   // Діаграма швиткості середння
                                                                                                                                                //4244 

                if (TimOutRefresh >= 10) {
                    TimOutRefresh = 0;
                    Warning.Text = "";



                    // Отримання поточного часу та значення для додавання до даних
                    DateTime currentTime = DateTime.Now;
                 
                    var Time = DateTime.Now.ToString("hh:mm:ss");

                    double Speed = 0;
                    if ((TimeInSeconds != 0)) { Speed = (((SamplsOLL - (double)RatioSamplsOll) / (double)SampleWeight.Value)) * (double)SecInH / (TimeInSeconds - RatioTimeInSeconds); }   // Діаграма швиткості// 5sec оновлення 
                    RatioTimeInSeconds = TimeInSeconds;
                    if (Speed < 0) { Speed = 0; };



                    // Обчислення відношення хороших/поганих зразків за хвилину
                    PcsGood = ((SamplsOLL - (double)Calc.BadSamples) / (double)((SamplsOLL - (double)Calc.BadSamples) + Calc.BadSamples)) * 100;
                    PcsBed = ((double)Calc.BadSamples / SamplsOLL) * 100;

                    double GoodKg = 0.0;
                    double BadKg = 0.0;
                    double TotalKg = 0.0;
                    if (Calc.BadSamples != 0) { GoodKg = ((SamplsOLL - (double)Calc.BadSamples) / (double)SampleWeight.Value); }  // Кількість Good Kg    
                    if (Calc.BadSamples != 0) { TotalKg = (SamplsOLL / (double)SampleWeight.Value); }  // Кількість Total Kg
                    if (Calc.GoodSamples != 0) { BadKg = (TotalKg - GoodKg); }  // Кількість Bad Kg
                    RatioSamplsOll = SamplsOLL;

                    if (Double.IsNaN(PcsGood)) { PcsGood = 0.0; } else { PcsGood = Math.Round(PcsGood, 2); }
                    if (Double.IsNaN(PcsBed)) { PcsBed = 0.0; } else { PcsBed = Math.Round(PcsBed, 2); }
                    if (Double.IsNaN(Speed))  {Speed = 0; }
                    if (Double.IsNaN(SpeedKgh)) { SpeedKgh = 0; }


                    Speed = Math.Round(Speed, 2);
                    SpeedKgh = Math.Round(SpeedKgh, 2);

                    GoodKg = Math.Round(GoodKg, 2);
                    TotalKg = Math.Round(TotalKg, 2);
                    BadKg = Math.Round(BadKg, 3);


                    ///////////// Додавання даних до таблиці ////////////
                    if ((buttonStartAnalic.Text == "Stop Analysis") && (!StartStopGrid)) {
                        // Починаємо вимірювання часу
                        stopwatch.Start();
                        WatchSpeed.Start();
                        DataTime = DateTime.Now;
                        
                        StartStopGrid = true; }

                    dataGridView1.Rows.Add(STGS.DT.SampleType, DataTime.ToString("MM/dd/yyyy hh:mm:ss tt"), Time,  PcsGood, PcsBed, SpeedKgh, GoodKg, BadKg, TotalKg, SizeCNT.Size100, SizeCNT.Size500, SizeCNT.Size1000);


                        if (buttonStartAnalic.Text == "Start Analysis")
                        {
                            // Починаємо вимірювання часу
                            stopwatch.Restart();
                            stopwatch.Stop();
                            WatchSpeed.Stop();
                            StartStopGrid = false;
                            SQL.SaveRow(dataGridView1);

                            //chart table filling

                            //int LengTypSempl = 0;

                            PDF_DT = new PDF_DT(SQL.СolumnNames.Length);
                            int lastRowIndex = dataGridView1.Rows.Count - 1; // Отримуємо індекс останнього рядка

                            for (int i = 0; i < SQL.СolumnNames.Length; i++)
                            {

                                PDF_DT.Name[i] = " ";
                                PDF_DT.Data[i] = "";
            
                                PDF_DT.IMG = new List<Bitmap>();

                            }

                            try
                            {

                                /********************************************************************/
                                ///////  ВИЗНАЧИТИ ВИД І НАЗВУ ВИДУ СЕМПЛА    /////
                                ///******************************************************************/
                                for (int Q = 0; Q < SQL.СolumnNames.Length; Q++)
                                {
                                    PDF_DT.Name[Q] = SQL.СolumnNames[Q];
                                    // Отримуємо значення з відповідної комірки останнього рядка dataGridView1
                                    PDF_DT.Data[Q] = dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[Q].Value.ToString();
                                    // reportDT.DataQunty[Q] = FlowAnalis.ContaminationSize[Q];

                                }
                            }
                            catch { }

                        }

                    





                    /////////////// Діаграма швиткості ////////////////
                    solidGauge1.Value = Speed;        // Митєва швиткість раз в 5 секунд   
                    SpidKgH.Text = SpeedKgh.ToString();  // Швиткість Kg\H

                    // Прокрутка таблиці до останнього рядка
                    dataGridView1.FirstDisplayedScrollingRowIndex = rowIndex;
                    rowIndex++;

                    //if (Calc.BadSamples != RatioSampls) {}
                    RatioBed = Calc.BadSamples - RatioSampls;
                    RatioSampls = Calc.BadSamples;

                    //double value = Math.Sin((currentTime - startTime).TotalSeconds);
                    // ratio = Math.Round(ratio, 2);
                    // Додавання даних до серії
                    chartValues.Add(new ObservablePoint(currentTime.Ticks, RatioBed));





                    // Оновлення видимої області графіка
                    cartesianChart1.AxisX[0].MaxValue = currentTime.Ticks;
                    cartesianChart1.AxisX[0].MinValue = currentTime.Ticks - TimeSpan.FromSeconds((double)numericUpDown7.Value).Ticks;

                    // Оновлення видимої області графіка
                    cartesianChart1.AxisY[0].MaxValue = (int)numericUpDown8.Value;
                    cartesianChart1.AxisY[0].MaxRange = 1;

                    cartesianChart1.AxisY[0].MinValue = 0;
                    // Налаштування формату стовпців для відображення без дробної частини
                    cartesianChart1.Zoom = ZoomingOptions.Xy;

                    // Обмеження кількості точок на графіку
                    if (chartValues.Count > (double)numericUpDown6.Value)
                        chartValues.RemoveAt(0);

                    // Оновлення міток осі X з часом у форматі "hh:mm:ss"
                    cartesianChart1.AxisX[0].LabelFormatter = value => new DateTime((long)value).ToString("hh:mm:ss");
                } }

        }


        ///////////////////////////////////    MOSAIC        /////////////////////////////////////////////////////
        static int ImgListCout = 0;

        static class SizeCNT
        {
            public static int Size100;
            public static int Size500;
            public static int Size1000;
        }


        private async void RefreshMosaics() {

            if ((MosaicDTlist.Count != 0)// && (ImgListCout <= (Convert.ToInt32(PageCauntMosaic.Text)))
                ) {

                //візуалізація мозаїки
                for (; ImgListCout < MosaicDTlist.Count; ImgListCout++)
                {
                    if ((MosaicDTlist[ImgListCout].SizeCNT >= 0) && (MosaicDTlist[ImgListCout].SizeCNT < 100)) { SizeCNT.Size100++; } else {
                        if ((MosaicDTlist[ImgListCout].SizeCNT >= 100) && (MosaicDTlist[ImgListCout].SizeCNT < 500)) { SizeCNT.Size500++; } else {
                            if ((MosaicDTlist[ImgListCout].SizeCNT >= 500) /* && (MosaicDTlist[ImgListCout].SizeCNT < 1000)*/) { SizeCNT.Size1000++; } } }


                    if (ImgListCout >= SETS.Data.MaxImagesMmosaic) { ClearMosaic(); break; }
                    if (MosaicDTlist[ImgListCout].Img != null) {
                        Mosaics.Images.Add(MosaicDTlist[ImgListCout].Img.AsBitmap());
                        listView1.LargeImageList = Mosaics;
                        listView1.VirtualListSize = Mosaics.Images.Count;// Задайте загальну кількість елементів

                    }

                    //  List VIWE
                    if (SETS.Data.MosaicRealTime) {
                        if ((visibleItemsPerPage == 0) || (ImgListCout == 0)) { listView1_Resize(null, null); } else {
                            int startIndex = Math.Max(0, ImgListCout - visibleItemsPerPage); // Отримайте індекс першого елемента для відображення
                            listView1.EnsureVisible(startIndex); /// Переконайтесь, що перший елемент видимий

                        }
                    }

                }
            }
        }


        public class ImageData {

            public string Group { get; set; }
            public Image<Gray, byte> Image { get; set; }

        }


        int visibleItemsPerPage;
        private void listView1_Resize(object sender, EventArgs e)
        {
            if (listView1.Items.Count != 0)
            {
                int itemHeight = listView1.GetItemRect(0).Height; // Визначте висоту одного елемента списку
                visibleItemsPerPage = listView1.ClientRectangle.Height / itemHeight;
            }
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            try
            {

                if (e.ItemIndex >= 0 && e.ItemIndex < ImgListCout)
                {
                    // Отримайте дані для відображення (зображення, текст і т. д.) для пункта з індексом e.ItemIndex
                    //var item = Mosaics.Images[e.ItemIndex];

                    // Створіть об'єкт для відображення
                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.ImageIndex = e.ItemIndex; // Індекс зображення (залежить від ваших даних)

                    listViewItem.Text = MosaicDTlist[e.ItemIndex].ID.ToString() + "_" + MosaicDTlist[e.ItemIndex].Name + (1 + e.ItemIndex).ToString();
                    // Встановіть колір тексту
                    if (MosaicDTlist[e.ItemIndex].Name == "good") { listViewItem.ForeColor = Color.Black; }
                    else
                    {
                        if (MosaicDTlist[e.ItemIndex].Name == "bad") { listViewItem.ForeColor = Color.Blue; }
                        else
                        {
                            if (MosaicDTlist[e.ItemIndex].Name == "Bad") { listViewItem.ForeColor = Color.Red; }
                            else
                            {
                                if (MosaicDTlist[e.ItemIndex].Name == "Good") { listViewItem.ForeColor = Color.Gray; }
                            }
                        }
                    }

                    // Передайте об'єкт для відображення у подію
                    e.Item = listViewItem;



                }
                else
                {  // Якщо індекс поза межами, можливо, встановіть e.Item в null або використайте інші значення за замовчуванням.
                    e.Item = new ListViewItem("Out of Range");
                }


            } catch { }
        }









        void ClearMosaic()
        {
            Mosaics.Images.Clear();
            listView1.VirtualListSize = 0; // Скидаємо кількість елементів у ListView

            listView1.Clear();
            MosaicDTlist = new List<DTLimg>();

            MosaicsTeachGrey.Clear();
            ImgListCout = 0;



        }

        void ClearMosaicVi()
        {
            Mosaics.Images.Clear();
            listView1.VirtualListSize = 0; // Скидаємо кількість елементів у ListView
            listView1.Clear();
            MosaicsTeachGrey.Clear();
            ImgListCout = 0;

        }



        private void InstMosaics() {



            listView1.View = View.LargeIcon;                //відображати назву картинкі
            Mosaics.ImageSize = new Size(64, 64);      //розмір виводу картинкі
            Mosaics.ColorDepth = ColorDepth.Depth16Bit;
            // Allow the user to edit item text.
            // listView1.LabelEdit = true;
            //// Allow the user to rearrange columns.
            // listView1.AllowColumnReorder = true;
            //// Select the item and subitems when selection is made.
            // listView1.FullRowSelect = true;
            //// Display grid lines.
            //listView1.GridLines = true;

            listView1.Dock = DockStyle.Fill;
            listView1.VirtualMode = true;
            listView1.TileSize = new Size(64, 64);


            // listView1.Groups.Add(new ListViewGroup("TEST", HorizontalAlignment.Left));
            listView1.Groups.Add(new ListViewGroup("Group B", HorizontalAlignment.Left));
            //Створіть групу "TEST", якщо вона не існує
            if (!listView1.Groups.Contains(new ListViewGroup("TEST")))
            {
                listView1.Groups.Add(new ListViewGroup("TEST", "TEST"));
            }


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
            // FlowAnalis.StartAnais = true;//включення живого відео

            if (buttonStartAnalic.Text == "Start Analysis") {
                SEPARATOR.ON();  // Metal separator
                AUTOLOADER.ON();  // Autoloder
                buttonStartAnalic.BackColor = Color.Salmon;

                LIGHT.ON();
                COOLING.ON();

                buttonStartAnalic.Text = "Stop Analysis";
                StartTable.Text = "Stop Table";
                OnLight.Text = "OFF Light";
                button40.Text = "OFF Cooling";
                button43.Text = "OFF Autoloader";
                button44.Text = "OFF Metal separator";
                Thread.Sleep(500);
                Flow.StartSorting(true);



            }
            else {


                buttonStartAnalic.BackColor = Color.GreenYellow;
                VIBR_TABLE.SET(VIBR_TABLE.Typ.OFF);

                Flow.StartSorting(false);
                Thread.Sleep(100);

                LIGHT.OFF();
                COOLING.OFF();

                buttonStartAnalic.Text = "Start Analysis";
                StartTable.Text = "Start Table";
                OnLight.Text = "ON Light";
                button40.Text = "ON Cooling";
                button43.Text = "ON Autoloader";
                button44.Text = "ON Metal separator";

                TimOutRefresh = 100; // для митевого запису в Grit Таблицю
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
            //ML
            STGS.DT.URL_ML = PachML.Text;

            //CAMERA
            SETS.Data.GEIN1 = GAIN1.Value;
            SETS.Data.GEIN2 = GAIN2.Value;


            if (SETS.Data.ID_CAM == DLS.Master) {
                SETS.Data.ACQGEIN1 = numericACQ_GainBright.Value;
                SETS.Data.ACQGEIN1_Black = numericACQ_GainBlack.Value;
            } else {
                SETS.Data.ACQGEIN2 = numericACQ_GainBright.Value;
                SETS.Data.ACQGEIN2_Black = numericACQ_GainBlack.Value;
            }


            //-----------------------------------
            SETS.Data.SetingsCameraStart = SetingsCameraStart.Checked;
            SETS.Data.CameraAnalis_1 = Camera1Lock.Checked;
            SETS.Data.CameraAnalis_2 = Camera2Lock.Checked;
            SETS.Data.PashTestIMG = textBoxTestImg.Text;
            SETS.Data.SampleWeight = SampleWeight.Value;
            SETS.Data.SignalLamp = numericUpDown4.Value;
            SETS.Data.SystemOFF = TimHoperOff.Value;


            if (SETS.Data.ID_CAM == DLS.Master)
            {
                SETS.Data.ACQ_SET1 = checkBoxAcqSet.Checked;
                radioButtonCam1.Checked = true;
            }
            else
            {
                SETS.Data.ACQ_SET2 = checkBoxAcqSet.Checked;
                radioButtonCam2.Checked = true;
            }

            if (radioButtonCam1.Checked) {
                checkBoxAcqSet.Checked = SETS.Data.ACQ_SET1;
                SETS.Data.ID_CAM = DLS.Master;
            } else {
                checkBoxAcqSet.Checked = SETS.Data.ACQ_SET2;
                SETS.Data.ID_CAM = DLS.Slave;
            }


            SETS.Data.MaxImagesMmosaic = (int)MaxImagesMmosaic.Value;
            SETS.Data.MosaicRealTime = MosaicRealTime.Checked;


            SETS.Data.BlobsInvert = InvertBlobs.Checked;
            SETS.Data.PachXLSX = richTextBox3.Text;
            SETS.Data.PachDB = richTextBox4.Text;
    
            SETS.Data.LiveVideoDelay = LiveViewDelay.Value;


            USB_HID.Data.Light_IR = LockIR.Checked;
            USB_HID.Data.Light_Top = LockTop.Checked;
            USB_HID.Data.Light_Back = LockBack.Checked;
            USB_HID.Data.Light_Bottom = LockBottom.Checked;


            VIS.Data.blurA = (byte)numericUpDown10.Value;
            VIS.Data.ThresholdA = (byte)numericUpDown11.Value;
            VIS.Data.blurB = (byte)numericUpDown12.Value;
            VIS.Data.ThresholdB = (byte)numericUpDown13.Value;
            VIS.Data.ArcLengthB = (int)numericUpDown5.Value;
            VIS.Data.ArcLengthTest = (int)numericUpDown9.Value;

            PDF.Data.Comments = Comments.Text;
            PDF.Data.CreatedBy = CreatedBy.Text;
            PDF.Data.NameReport = NameReport.Text;
            PDF.Data.SampleType = SempleTyp.Text;
            PDF.Data.PathFileSave = PathFileSave.Text;
            PDF.Data.ShowImageInReport = ShowImageInReport.Checked;
        }

        void RefreshSetings()
        {

            //ML
              PachML.Text = STGS.DT.URL_ML;

            try
            {
                MosaicRealTime.Checked = SETS.Data.MosaicRealTime;
                MaxImagesMmosaic.Value = SETS.Data.MaxImagesMmosaic;
                SetingsCameraStart.Checked = SETS.Data.SetingsCameraStart;
                Camera1Lock.Checked = SETS.Data.CameraAnalis_1;
                Camera2Lock.Checked = SETS.Data.CameraAnalis_2;
                textBoxTestImg.Text = SETS.Data.PashTestIMG;
                LiveViewDelay.Value = SETS.Data.LiveVideoDelay;
                checkBoxAcqSet.Checked = SETS.Data.ACQ_SET1;
                SampleWeight.Value = SETS.Data.SampleWeight;

                numericUpDown4.Value = SETS.Data.SignalLamp;
                TimHoperOff.Value = SETS.Data.SystemOFF;

                if (SETS.Data.ID_CAM == DLS.Master) {
                    checkBoxAcqSet.Checked = SETS.Data.ACQ_SET1;
                    radioButtonCam1.Checked = true; }
                else {
                    checkBoxAcqSet.Checked = SETS.Data.ACQ_SET2;
                    radioButtonCam2.Checked = true; }

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
                richTextBox3.Text = SETS.Data.PachXLSX;
                richTextBox4.Text = SETS.Data.PachDB;


                GAIN1.Value = SETS.Data.GEIN1;
                GAIN2.Value = SETS.Data.GEIN2;

                if (SETS.Data.ID_CAM == DLS.Master) { 
                    numericACQ_GainBright.Value = SETS.Data.ACQGEIN1;
                    numericACQ_GainBlack.Value = SETS.Data.ACQGEIN1_Black;
                }
                else { 
                    numericACQ_GainBright.Value = SETS.Data.ACQGEIN2;
                    numericACQ_GainBlack.Value = SETS.Data.ACQGEIN2_Black;
                }



                InvertBlobs.Checked = SETS.Data.BlobsInvert;

                LockIR.Checked = USB_HID.Data.Light_IR;
                LockTop.Checked = USB_HID.Data.Light_Top;
                LockBack.Checked = USB_HID.Data.Light_Back;
                LockBottom.Checked = USB_HID.Data.Light_Bottom;




                numericUpDown10.Value = VIS.Data.blurA;
                numericUpDown11.Value = VIS.Data.ThresholdA;
                numericUpDown12.Value = VIS.Data.blurB;
                numericUpDown13.Value = VIS.Data.ThresholdB;
                numericUpDown5.Value = VIS.Data.ArcLengthB;
                numericUpDown9.Value = VIS.Data.ArcLengthTest;

                Comments.Text = PDF.Data.Comments;
                CreatedBy.Text = PDF.Data.CreatedBy;
                NameReport.Text = PDF.Data.NameReport;
                SempleTyp.Text = PDF.Data.SampleType;
                PathFileSave.Text = PDF.Data.PathFileSave;
                ShowImageInReport.Checked = PDF.Data.ShowImageInReport;


            }
            catch
            {

                Help.Mesag("saved data not correct !");
            }
        }






        private void MastConturMax_ValueChanged(object sender, EventArgs e) {

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

            EMGU.Data.GreyScaleMin[ID] = (int)GreyScaleMin_.Value;
        }


        /************************/
        STATUS StatusDvise = new STATUS();
   
       static int SignalLamp;
      static  bool SignalLampRepit = false;

        private void timer1_Tick(object sender, EventArgs e)
        {



            if (USB_HID.HidStatus == true)
            {
                HidConect.Text = "connected"; HidConect.ForeColor = Color.Green;
            } else { HidConect.Text = "not connected"; HidConect.ForeColor = Color.Red; }

            StatusDvise = new STATUS();
            Warning.Text = StatusDvise.Door; 

            // STOP ERROR
            if (StatusDvise.Stop) {
                Warning.Text = "STOP";
                // FlowAnalis.StartAnais = true;//включення живого відео
                if (buttonStartAnalic.Text == "Stop Analysis") { button13_Click(null, null); } }  // STOP ERROR

            // HOPER LEVEL HIGH
            if (StatusDvise.SensorHigh) { ProgresBar.Value = 100; SignalLamp = 0; } else {
                // HOPER LEVEL LOW
                if (StatusDvise.SensorLow) { ProgresBar.Value = 50; SignalLamp = 0; } else {
                    ProgresBar.Value = 0;

                    if (buttonStartAnalic.Text == "Stop Analysis")
                    {
                        SignalLamp++;
                        //SIGNAL LAMP
                        if ((SignalLamp > (SETS.Data.SignalLamp * 2))) {
                            if (SignalLampRepit) { LIGHT.YELLO_ERROR(false); SignalLampRepit = false; } else {
                                Warning.Text = "HOPPER IS EMPTY";
                                LIGHT.YELLO_ERROR(true); SignalLampRepit = true; }
                        } } else { SignalLamp = 0; } } }


            //STOP
            if (buttonStartAnalic.Text == "Stop Analysis") {
                Calc.StopSustem++;

            if ((Calc.StopSustem > (SETS.Data.SystemOFF * 2)) && (buttonStartAnalic.Text == "Stop Analysis")) {
                Warning.Text = "AUTOMATIC STOP";
                 Calc.StopSustem = 0;
                button13_Click(null, null); }
    
                } else { Calc.StopSustem = 0; }



            BuferImgIdx.Text = FlowCamera.BuferImg.Count.ToString();
            BuferImgCaun.Text = FlowCamera.BoxImgM.Count.ToString();
            CauntListImages.Text = FlowCamera.ImgSave.Count.ToString();

            if (SETS.Data.ID_CAM == DLS.Slave) {
                CauntSamls.Text = Calc.BlobsSlave.ToString();
            } else { CauntSamls.Text = Calc.BlobsMaster.ToString(); }

            toolStripStatusLabel5.Text = DLS.elapsedMs.ToString();
            toolStripStatusLabel6.Text = FlowCamera.BatchSizePreict.ToString();


            TimerRefreshChart();

            RefreshMosaics();


        }

        private void USER_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSetings();
        }

        private void Soreter_FormClosed(object sender, FormClosedEventArgs e)
        {
            Flow.StopPotoc();
            COOLING.OFF();
            AUTOLOADER.OFF();
            SEPARATOR.OFF();
            VIBR_TABLE.SET(Type: VIBR_TABLE.Typ.OFF);
            LIGHT.OFF();
            CAMERA.OFF();
            FLAPS.RUN();

        }




        private void button2_Click(object sender, EventArgs e)
        {
            ClearMosaic();

            ClearAnalisData();

        }


        int ImagCouAnn;
        Bitmap LearnImg = new Bitmap(100, 100);
        int SelectITMs;
        private void button15_Click(object sender, EventArgs e)
        {

            MosaicsTeach.Images.Add(MosaicDTlist[SelectITMs].Img[0].ToBitmap());
            MosaicsTeachGrey.Add(MosaicDTlist[SelectITMs].Img[0]);

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


                MosaicsTeach.Images.Add(MosaicDTlist[SelectITMs].Img[0].ToBitmap());
                MosaicsTeachGrey.Add(MosaicDTlist[SelectITMs].Img[0]);
                listView2.LargeImageList = MosaicsTeach;
                listView2.Items.Add(new ListViewItem { ImageIndex = ImagCouAnn, Text = "Images",  /*nema imag*/ });
                ImagCouAnn++;



            }
            else { Help.Mesag("You cannot select image or generate a report when the images are not sorted! "); }

        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {



        }

        private void button60_Click(object sender, EventArgs e)
        {

            if (listView1.FocusedItem != null)
            {
                int idx = listView1.SelectedIndices[0]; //початковий індекс з масиву
                for (int Q = 0; Q < MosaicDTlist.Count; Q++)
                {
                    MosaicsTeach.Images.Add(MosaicDTlist[idx].Img[0].ToBitmap());
                    MosaicsTeachGrey.Add(MosaicDTlist[idx].Img[0]);
                    listView2.LargeImageList = MosaicsTeach;
                    listView2.Items.Add(new ListViewItem { ImageIndex = ImagCouAnn, Text = "Images",  /*nema imag*/ });
                    ImagCouAnn++;
                    idx++;
                }
            }
            else { Help.Mesag("You need to select several images from the mosaic"); }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            ImagCouAnn = 0;
            listView2.Clear();
            MosaicsTeach.Images.Clear();
            MosaicsTeachGrey.Clear();
        }

        private void button17_Click(object sender, EventArgs e)
        {


            SaveSample(true);


        }


        void SaveSample(bool AskMsg)
        {


            if (comboBoxBedGood.Text == "")
            {
                MessageBox.Show("The name field cannot be empt", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            };








            DialogResult result = DialogResult.Yes;
            if (AskMsg == true) { result = MessageBox.Show("Do you want Add Images to " + comboBoxBedGood.Text + " ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information); }



            string PshData = Path.Combine(PachML.Text, "Data"); //створити шлях до каталога "Data"
            string PshSempls = Path.Combine(PshData, STGS.Data.ML_NAME); //створити шлях до каталога "SAMPLES"
            string PashImg = Path.Combine(PshSempls, comboBoxBedGood.Text); ///створити шлях до каталога "Bad Good"

            if (false == Directory.Exists(PshData)) { Directory.CreateDirectory(PshData); }// якщо нема пакі то створюєм
            if (false == Directory.Exists(PshSempls)) { Directory.CreateDirectory(PshSempls); }// якщо нема пакі то створюєм
            if (false == Directory.Exists(PashImg)) { Directory.CreateDirectory(PashImg); }// якщо нема пакі то створюєм

            if (result == DialogResult.Yes) {
                for (int i = 0; i < MosaicsTeach.Images.Count; i++) {

                    DateTime dateOnly = new DateTime();
                    dateOnly = DateTime.Now;

                    String DataFile = dateOnly.Month.ToString() + ".";
                    DataFile = DataFile + dateOnly.Day.ToString() + ".";
                    DataFile = DataFile + dateOnly.Year.ToString() + ". ";
                    DataFile = DataFile + dateOnly.Hour.ToString() + ".";
                    DataFile = DataFile + dateOnly.Minute.ToString() + ".";
                    DataFile = DataFile + dateOnly.Second.ToString() + " ";
                    MosaicsTeachGrey[i].Save(PashImg + "\\" + DataFile + "img" + i.ToString() + ".jpg");

                }

            }

        }


        private void button61_Click(object sender, EventArgs e) {
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK)
                {

                    PachML.Text = FBD.SelectedPath;

            
            string PshData = Path.Combine(PachML.Text, "Data"); //створити шлях до каталога "Data"
            string PshSempls = Path.Combine(PshData, STGS.Data.ML_NAME); //створити шлях до каталога "SAMPLES"
                                                                 // string PashImg = Path.Combine(PshSempls, comboBoxBedGood.Text); ///створити шлях до каталога "Bed Good"

            if (false == Directory.Exists(PshData)) { Directory.CreateDirectory(PshData); }// якщо нема пакі то створюєм
            if (false == Directory.Exists(PshSempls)) { Directory.CreateDirectory(PshSempls); }// якщо нема пакі то створюєм
            // if (false == Directory.Exists(PashImg))   { Directory.CreateDirectory(PashImg); }// якщо нема пакі то створюєм

                }
                else { MessageBox.Show("Choose directory please", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }

        }



        private void button6_Click(object sender, EventArgs e)
        {
            string PshData = Path.Combine(PachML.Text, "Data"); //створити шлях до каталога "Data"
            string PshSempls = Path.Combine(PshData, STGS.Data.ML_NAME); //створити шлях до каталога "SAMPLES"

            // Отримати поточний каталог (куди вказує відносний шлях)
            string currentDirectory = Directory.GetCurrentDirectory();

            // Об'єднати відносний шлях з поточним каталогом для отримання повного шляху
            string fullPath = Path.Combine(currentDirectory, PshSempls);

            string absolutePath = Path.GetFullPath(fullPath);

            if (System.IO.Directory.Exists(absolutePath))
            {
                Process.Start("explorer.exe", absolutePath);
            }
            else
            {
                Console.WriteLine("Папка не існує.");
            }
        }




        private void button62_Click(object sender, EventArgs e)
        {


            // Отримати поточний каталог (куди вказує відносний шлях)
            string currentDirectory = Directory.GetCurrentDirectory();

            // Об'єднати відносний шлях з поточним каталогом для отримання повного шляху
            string fullPath = Path.Combine(currentDirectory, STGS.Data.URL_SampleType);

            if (false == Directory.Exists(fullPath)) { Directory.CreateDirectory(fullPath); }// якщо нема пакі то створюєм

            string absolutePath = Path.GetFullPath(fullPath);

            if (System.IO.Directory.Exists(absolutePath))
            {
                Process.Start("explorer.exe", absolutePath);
            }
            else
            {
                Console.WriteLine("Папка не існує.");
            }
        }



        private void button18_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want Teach ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);


            if (result == DialogResult.Yes) {

                string DataPath = Path.Combine(STGS.DT.URL_ML, "Data");

                // Об'єднати відносний шлях з поточним каталогом для отримання повного шляху

                //string fullPath = Path.Combine(currentDirectory, DataPath);
                //string absolutePath = Path.GetFullPath(fullPath);
                Flow.ProcessLerningFunction(DataPath, "C2S_150");
                Close();

            }
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

            if (buttonLiveVideo.Text == "SNEP IMG ACQ")
            { 
                buttonStartAnalic.Enabled = true;
                DLS.ImgSnapSet = false;
                Flow.StartSorting(false);
                buttonLiveVideo.Text = "LIVE VIDEO ON";
                LIGHT.OFF();
                OnLight.Text = "ON Light";
                buttonLiveVideo.BackColor = Color.LightGreen;
                button19_Click(null, null);
                return;
            }
                
            if (buttonStartAnalic.Text == "Start Analysis")
            {
                if (buttonLiveVideo.Text == "LIVE VIDEO ON")
                {
                    buttonStartAnalic.Enabled = false;
                    Flow.LiweVive(true);
                    buttonLiveVideo.Text = "LIVE VIDEO OFF";
                    LIGHT.ON();
                    OnLight.Text = "OFF Light";
                    buttonLiveVideo.BackColor = Color.Salmon;

                }
                else
                {
                    buttonStartAnalic.Enabled = true;
                    DLS.ImgSnapSet = false;
                    Flow.StartSorting(false);
                    buttonLiveVideo.Text = "LIVE VIDEO ON";
                    LIGHT.OFF();
                    OnLight.Text = "ON Light";
                    buttonLiveVideo.BackColor = Color.LightGreen;

                }
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
 try
                {
            string urlMaster = textBoxTestImg.Text + "\\" + "Image" + IdxShou++ + ".jpg";
            FlowAnalis.Setings = true;
            string[] files = Directory.GetFiles(textBoxTestImg.Text, "*.jpg");

            int count = files.Length;


            if (IdxShou <= files.Length)
            {
               

                    if (SETS.Data.ID_CAM == 0) { 
                    Bitmap imM = new Bitmap(urlMaster);
                    textBox2.Text = IdxShou.ToString();
                    Emgu.CV.Mat imOriginalM = imM.ToImage<Bgr, byte>().Mat;
                    IProducerConsumerCollection<Image<Gray, byte>> CollecTempM = FlowCamera.BoxImgM;
                    CollecTempM.TryAdd(imOriginalM.ToImage<Gray, byte>());
                    } else {
                   
                    Bitmap imS = new Bitmap(urlMaster);
                    textBox2.Text = IdxShou.ToString();
                    Emgu.CV.Mat imOriginalS = imS.ToImage<Bgr, byte>().Mat;
                    IProducerConsumerCollection<Image<Gray, byte>> CollecTempS = FlowCamera.BoxImgS;
                    CollecTempS.TryAdd(imOriginalS.ToImage<Gray, byte>()); 
                    }



            }
            else { }


                }
                catch { }

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






        private void PageCauntMosaic_TextChanged(object sender, EventArgs e)
        { ClearMosaicVi();
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

        private void AnalisLock_Click(object sender, EventArgs e)
        {
            FlowCamera.AnalisLock = AnalisLock.Checked;
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

        void TestImgBlb()
        {
            string PshData = Path.Combine(PachML.Text, "Data"); //створити шлях до каталога "Data"
            string PshSempls = Path.Combine(PshData, STGS.Data.ML_NAME, comboBoxImgTypTest.Text); //створити шлях до каталога "SAMPLES"


            try
                {
            string urlMaster = PshSempls + "\\" + "Image" + IdxShouTest++ + ".jpg";
            files = Directory.GetFiles(@PshSempls, "*.jpg");

            int count = files.Length;

            if (files != null)
            {
                if (IdxShouTest <= files.Length)
            {
                    Bitmap imM = new Bitmap(files[IdxShouTest]);
                    textBox3.Text = IdxShouTest.ToString();

                    imOriginalM = imM.ToImage<Bgr, byte>().Resize(64, 64, interpolationType: Inter.Linear).Mat;

                    Stopwatch watch = Stopwatch.StartNew();

               

                    Image<Bgr, byte> ImagesViw = new Image<Bgr, byte>(100, 100);
                    if (AnalysisTest.Checked)
                    {
                        ImagesViw = vision.DetectBlob(imOriginalM, labelDectContur);
                    }
                    else
                    {
                        ImagesViw = vision.DetectBlobBlack(imOriginalM, labelDectContur);
                    }



                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    toolStripStatusLabel5.Text = elapsedMs.ToString();
                    // Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

                    pictureBox1.Image = ImagesViw.ToBitmap();
                    pictureBox2.Image = imOriginalM.ToBitmap();


               
            }
        }
       }
                catch { }
        }


      void SetingsValGrayImg (){
            try
            {

                if (files != null) { 

                if (IdxShouTest <= files.Length)
                {


                    Bitmap imM = new Bitmap(files[IdxShouTest]);
                    textBox3.Text = IdxShouTest.ToString();

                    imOriginalM = imM.ToImage<Bgr, byte>().Resize(64, 64, interpolationType: Inter.Linear).Mat;

                    Stopwatch watch = Stopwatch.StartNew();


                    Image<Bgr, byte> ImagesViw = new Image<Bgr, byte>(100, 100);

                    if (AnalysisTest.Checked)
                    {
                        ImagesViw = vision.DetectBlob(imOriginalM, labelDectContur);
                    }
                    else
                    {
                        ImagesViw = vision.DetectBlobBlack(imOriginalM, labelDectContur);
                    }


                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    toolStripStatusLabel5.Text = elapsedMs.ToString();
                    // Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

                    pictureBox1.Image = ImagesViw.ToBitmap();
                    pictureBox2.Image = imOriginalM.ToBitmap();


                }
            }
               
            } catch { }

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
    


            if (coutTim == 1)
            {
                Flow.ProcessLoadImagesFunction();
            }
            if (coutTim == 5)
            {
                Enabled = true;
                try {  DLS = new DLS(); }  catch {
                    timer3.Enabled = false;
                    coutTim = 0;
                    Help.Mesag("Cameras are not connected"); Enabled = true; timer3.Enabled = false;
                    Flow.ProcessLoadImagesFunction(false);
             
                } }
              
                Enabled = false;
            if (coutTim > 10){
                timer3.Enabled = false;
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

                }else { GAIN2.Value = (decimal)DLS.Devis.Gain[DLS.Slave]; }


                // Завантаження Вирівнювання Фону
                if (!SETS.Data.CameraAnalis_1) { SetACQ_File(DLS.Master); }
                if (!SETS.Data.CameraAnalis_2) { SetACQ_File(DLS.Slave);  }
                Flow.ProcessLoadImagesFunction(false);
         
                Enabled = true;
            }



                coutTim++;
        }

        private void GAIN1_Click(object sender, EventArgs e)
        {
            try { 
            DLS.SetGain((double)GAIN1.Value, DLS.Master);
            } catch {      Help.Mesag("Reset Program"); }
           
      
        }





        private void button57_Click(object sender, EventArgs e)
        {
            FlowCamera.AnalisLock = true;
            buttonStartAnalic.Enabled = false;
            string PathType = Path.Combine(STGS.Data.URL_SampleType, TextBoxSemplTyp.Text);
            string PshACQ = Path.Combine(PathType, "ACQ"); //створити шлях до IMG

            Bitmap BlekImg = new Bitmap(2000, 100);
            LiveView.Image = BlekImg;

            checkBoxAcqSet.Checked = false;
            DLS.checkBox_FaltField_Click(SETS.Data.ID_CAM, checkBoxAcqSet.Checked);

            OnLight.Text = "OFF Light";
            buttonLiveVideo.Text = "SNEP IMG ACQ";
            buttonLiveVideo.BackColor = Color.Salmon;
            
            LiveView.Image = DLS.Acq_Bright_Simply(SETS.Data.ID_CAM, PshACQ);

            OnLight.Text = "ON Light";
            buttonLiveVideo.Text = "LIVE VIDEO ON";
            DLS.Save_FF_File(SETS.Data.ID_CAM, PshACQ);

            checkBoxAcqSet.Checked = true;
            //DLS.checkBox_FaltField_Click(SETS.Data.ID_CAM, checkBoxAcqSet.Checked);
            SetACQ_File(SETS.Data.ID_CAM);

            DLS.StartCAMERA(SETS.Data.ID_CAM);
            LIGHT.ON();
            OnLight.Text = "OFF Light";
            buttonLiveVideo.Text = "SNEP IMG ACQ";
        }


        //Black
        private void button14_Click(object sender, EventArgs e)
        {
            FlowCamera.AnalisLock = true;
            buttonStartAnalic.Enabled = false;
            string PathType = Path.Combine(STGS.Data.URL_SampleType, TextBoxSemplTyp.Text);
            string PshACQ = Path.Combine(PathType, "ACQ"); //створити шлях до IMG

            Bitmap BlekImg = new Bitmap(2000, 100);
            LiveView.Image = BlekImg;

            checkBoxAcqSet.Checked = false;
            DLS.checkBox_FaltField_Click(SETS.Data.ID_CAM, checkBoxAcqSet.Checked);

            OnLight.Text = "ON Light";
            buttonLiveVideo.Text = "LIVE VIDEO ON";
            buttonLiveVideo.BackColor = Color.Salmon;

            LiveView.Image = DLS.Acq_Dark_Simply(SETS.Data.ID_CAM, PshACQ);


            //DLS.Save_FF_File(SETS.Data.ID_CAM, PshACQ);

            //checkBoxAcqSet.Checked = true;
            //DLS.checkBox_FaltField_Click(SETS.Data.ID_CAM, checkBoxAcqSet.Checked);
            SetACQ_File(SETS.Data.ID_CAM);

            DLS.StartCAMERA(SETS.Data.ID_CAM);
            LIGHT.ON();
            OnLight.Text = "OFF Light";
            buttonLiveVideo.Text = "SNEP IMG ACQ";
            buttonStartAnalic.Enabled = false;
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

    







        //____________________________________Save semple Type____________________________________________________



        private void CreatSamplTyp_Click(object sender, EventArgs e)
        {

            if (textBoxСreateSample.Text != "")
            {

                DialogResult YESNO = MessageBox.Show("Do you want add new sample type " + textBoxСreateSample.Text + "  ?", "Waring!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if ( YESNO == DialogResult.Yes)
                {



                    string PathType = Path.Combine(STGS.Data.URL_SampleType, textBoxСreateSample.Text);
                    string PshACQ = Path.Combine(PathType, "ACQ"); //створити шлях до каталога "Data"
                    string PshData = Path.Combine(PathType, "Data"); //створити шлях до каталога "Data"
                    string PshSempls = Path.Combine(PshData, STGS.Data.ML_NAME); //створити шлях до каталога "SAMPLES"

                    if (false == Directory.Exists(STGS.Data.URL_SampleType)) { Directory.CreateDirectory(STGS.Data.URL_SampleType); }
                    if (false == Directory.Exists(PathType)) { Directory.CreateDirectory(PathType); }
                    if (false == Directory.Exists(PshSempls)) { Directory.CreateDirectory(PshSempls); }// якщо нема пакі то створюєм
                    if (false == Directory.Exists(PshACQ)) { Directory.CreateDirectory(PshACQ); }// Дані з вирівнюванням фону картинкі



                    // назви каталогів з видами збруднення з --  comboBoxBedGood ----
                    for (int i = 0; i < comboBoxBedGood.Items.Count; i++) { 
                        string PashImg = Path.Combine(PshSempls, comboBoxBedGood.Items[i].ToString()); ///створити шлях до каталога "Bad Good"
                       if (false == Directory.Exists(PashImg)) { Directory.CreateDirectory(PashImg); }}// якщо нема пакі то створюєм
                         
                 



                    textBoxСreateSample.Text = "";
                }
            }
        }

    



        void LoadSempelsName()
        {
            try
            {
                int idx = 0;
                string[] SamlCatalogPath = new string[Directory.GetDirectories(STGS.Data.URL_SampleType).Length];
                string[] pathSmpl = new string[Directory.GetDirectories(STGS.Data.URL_SampleType).Length];
                SamlCatalogPath = Directory.GetDirectories(STGS.Data.URL_SampleType);


                for (idx = 0; idx < SamlCatalogPath.Length; idx++)
                { pathSmpl[idx] += Path.GetFileName(SamlCatalogPath[idx]); }

                comboBoxSetingsName.Items.Clear();
                textBoxСreateSample.Items.Clear();

                int x = 0;
                string[] NemSmpls = new string[pathSmpl.Length];

                foreach (var i in pathSmpl)
                {
    
                    comboBoxSetingsName.Items.Add(i);
                    textBoxСreateSample.Items.Add(i);
                    NemSmpls[x++] = i;
                }

              //  DSV.TF_DT.Name = NemSmpls;


            } catch { Help.Mesag(" problem with Catalogue (Sample Type) "); }
        }

        private void buttonDeleteTypeSempl_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Yes;
            result = MessageBox.Show("Do you want delete sample type '" + textBoxСreateSample.Text + "' ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);


            if (result == DialogResult.Yes)
            {
                if ((TextBoxSemplTyp.Text != textBoxСreateSample.Text)&&(textBoxСreateSample.Text != STGS.DT.SampleType))
                {
                    
                    //видалити деректрію
                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(STGS.Data.URL_SampleType, textBoxСreateSample.Text));
                        dirInfo.Delete(true);
                        textBoxСreateSample.Text = "";
                    }
                    catch { MessageBox.Show("The directory cannot be deleted 'directory is not found' "); }
                }
                else { MessageBox.Show("The sample type cannot be deleted because it is currently in use !!!"); }
            }
            
        }

        private void comboBox1_Click(object sender, EventArgs e)
        { LoadSempelsName();}

        private void comboBoxSetingsName_Click(object sender, EventArgs e)
        { LoadSempelsName(); }

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
                           button19_Click(null, null);
                        }
                    else {   MessageBox.Show("Unable to apply new settings!!! Maybe the file doesn't exist yet, you need to make the first save. If you press save, the current settings will be saved in the" + "sample type" + "file");}; 
                   // }

                    }
            }
            


        }

        private void OutputDelay_Click(object sender, EventArgs e)
        {
            FLAPS.Time_OFF(OutputDelay.Value);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            AnalisPredict.FlapsTestBleak = checkBox2.Checked;
            VIS.ArcLengTestChecd = checkBox2.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            AnalisPredict.MosaicShowGood = checkBox1.Checked;
        }


        private void button63_Click(object sender, EventArgs e)
        {
            DateTime selectedDate1 = dateTimePicker1.Value;
            string formattedDate1 = selectedDate1.ToString("MM/dd/yyyy");

            DateTime selectedDate2 = dateTimePicker2.Value;
            string formattedDate2 = selectedDate2.ToString("MM/dd/yyyy");

            SQL.Updat(true, dataGridView2, formattedDate1, formattedDate2);  }

        private void button1_Click(object sender, EventArgs e)
        {
            SQL.XLSX_Save(richTextBox3);
        }


      private void button66_Click(object sender, EventArgs e)
        {
            SQL.DeleteRow(dataGridView2);
        }


        private void button26_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK)
                {

                    richTextBox3.Text = FBD.SelectedPath;
                }
                else { MessageBox.Show("Choose directory please", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void button64_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK)
                {

                    richTextBox4.Text = FBD.SelectedPath;
                }
                else { MessageBox.Show("Choose directory please", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }



     


        private void numericACQ_Gain_Click(object sender, EventArgs e)
        {
            try
            {
                FlowCamera.AnalisLock = true;
                AnalisLock.Checked = true;
                DLS.SetGain((double)numericACQ_GainBright.Value, SETS.Data.ID_CAM);
            }
            catch { Help.Mesag("Reset Program"); }
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
            try{
                FlowCamera.AnalisLock = true;
                AnalisLock.Checked=true;
                DLS.SetGain((double)numericACQ_GainBright.Value, SETS.Data.ID_CAM);
            }catch { Help.Mesag("Reset Program"); }
        }

        private void button27_Click_1(object sender, EventArgs e)
        {
            try
            {
                FlowCamera.AnalisLock = true;
                AnalisLock.Checked = true;
                DLS.SetGain((double)numericACQ_GainBlack.Value, SETS.Data.ID_CAM);
            }
            catch { Help.Mesag("Reset Program"); }
        }

        private void numericACQ_GainBlack_Click(object sender, EventArgs e)
        {
            try
            {
                FlowCamera.AnalisLock = true;
                AnalisLock.Checked = true;
                DLS.SetGain((double)numericACQ_GainBlack.Value, SETS.Data.ID_CAM);
            }
            catch { Help.Mesag("Reset Program"); }
        }

        private void button19_Click(object sender, EventArgs e){
            try
            {
                FlowCamera.AnalisLock = false;
                AnalisLock.Checked = false;
               // DLS.SetGain((double)GAIN1.Value, DLS.Master);
                //DLS.SetGain((double)GAIN2.Value, DLS.Slave);

                GAIN1.Enabled = DLS.Devis.Status[DLS.Master];
                GAIN2.Enabled = DLS.Devis.Status[DLS.Slave];

                if ((GAIN1.Value <= 10) && (GAIN1.Value >= 1))
                {
                    if (GAIN1.Value != (decimal)DLS.Devis.Gain[DLS.Master]) { GAIN1_Click(null, null); }

                }  else { GAIN1.Value = (decimal)DLS.Devis.Gain[DLS.Master]; }
              


                if ((GAIN2.Value <= 10) && (GAIN2.Value >= 1))
                {
                    if (GAIN2.Value != (decimal)DLS.Devis.Gain[DLS.Slave]) { GAIN2_Click(null, null); }

                }else { GAIN2.Value = (decimal)DLS.Devis.Gain[DLS.Slave]; }


                // Завантаження Вирівнювання Фону
                if (!SETS.Data.CameraAnalis_1) { SetACQ_File(DLS.Master); }
                if (!SETS.Data.CameraAnalis_2) { SetACQ_File(DLS.Slave); }



            }
            catch { Help.Mesag("Reset Program"); }
            
        }

        private void checkBox1_Click(object sender, EventArgs e){
            SetACQ_File(SETS.Data.ID_CAM);
        }

        void SetACQ_File(int ID_CAM) {

            string PathType = Path.Combine(STGS.Data.URL_SampleType, TextBoxSemplTyp.Text);
            string PshACQ = Path.Combine(PathType, "ACQ"); //створити шлях до IMG
            
            DLS.Load_FF_File (ID_CAM , PshACQ);
            DLS.checkBox_FaltField_Click(ID_CAM, checkBoxAcqSet.Checked);
            
        }


        private void GAIN2_Click(object sender, EventArgs e)
        {
            try
            {
                DLS.SetGain((double)GAIN2.Value, DLS.Slave);
            }
            catch
            {
                Help.Mesag("Reset Program");
            }
        }





        PDF ReportPDF = new PDF();
        PDF_DT PDF_DT ;

        //  [Obsolete]
        private void MakeReportButton_Click(object sender, EventArgs e)
        {

            if (PDF.Data.ShowImageInReport) { 
            foreach (var DT in MosaicDTlist)
            {
                PDF_DT.IMG.Add( DT.Img.ToBitmap());
            }}
           


            ReportPDF.ReportSet(PDF_DT);
            PDF_DT.IMG.Clear();

            //}
            //else { Help.ErrorMesag("You cannot select image or generate a report when the images are not sorted! "); }
        }

        private void button51_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK)
                {

                    PathFileSave.Text = FBD.SelectedPath;

                }
                else { MessageBox.Show("Choose directory please", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void checkBox6_Click(object sender, EventArgs e)
        {
            if (checkBox6.Checked) { PaaswortString.UseSystemPasswordChar = false; } else
            { PaaswortString.UseSystemPasswordChar = true;   }
        }


        void LockFunk(bool LockSoft)
        {

            if (LockSoft) {
                
                WorkTable.Enabled = false;
                groupBox34.Enabled = false;
                VisionSettings.Enabled = false;
                PathDatabases.Enabled = false;


                button51.Enabled = false;
                PathFileSave.Enabled = false;
                FillingHopperError.Enabled = false;
                FlapsSettings.Enabled = false;
                SampleWeightgroup.Enabled = false;
                LightLock.Enabled = false;
                SettingsTypeBox.Enabled = false;
                CamerasSettings.Enabled = false;
                Hz_Table.Enabled = false;
                richTextBox3.Enabled = false;
                button26.Enabled = false;
            

            } else {

                WorkTable.Enabled = true;
                groupBox34.Enabled = true;
                VisionSettings.Enabled = true;
                PathDatabases.Enabled = true;

                button51.Enabled = true;
                PathFileSave.Enabled = true;
                FillingHopperError.Enabled = true;
                FlapsSettings.Enabled = true;
                SampleWeightgroup.Enabled = true;
                LightLock.Enabled = true;
                SettingsTypeBox.Enabled = true;
                CamerasSettings.Enabled = true;
                Hz_Table.Enabled = true;
                richTextBox3.Enabled = true;
                button26.Enabled = true;
            
            }


        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (PaaswortString.Text != "")
            {
                if ((PaaswortString.Text == "1304")|| (PaaswortString.Text == STGS.DT.Password))
                {
                   
                    LockFunk(false);
                    PaaswortString.Text = "";
                    PasworLable.Text = "OK";
                    IDtex.Text = "Admin mode"; // Label visual Mode
                }
                else { PaaswortString.Text = ""; Help.Mesag("Password is incorrect"); }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
           
            LockFunk(true);
            IDtex.Text = "User mode"; // Label visual Mode
            PasworLable.Text = "-";
        }




        /// <summary>
        /// change your password
        /// </summary>
        int PaswortStepChange = 0;
        string PaaswortStringRepeet;
        private void button5_Click(object sender, EventArgs e)
        {
            PasworLable.Text = "";

            if (IDtex.Text == "Admin mode") // Label visual Mode
            {  
                
                
                if ((PaaswortString.Text == "")&&(PaswortStepChange==0)){
                DialogResult result = DialogResult.Yes;
                result = MessageBox.Show("Do you want to change your password ?'"  , "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes){
                        PaswortStepChange = 1;
                        PaaswortString.Text = "";
                        PasworLable.Text = "Set new Password";
                        return;
                    }  }



                if ((PaaswortString.Text.Length > 3) && (PaswortStepChange == 1))
                {
                    PaaswortStringRepeet = PaaswortString.Text;
                    PaaswortString.Text = "";
                    PasworLable.Text = "Repeat";
                    PaswortStepChange = 2;
                    return;
                }
                else
                {
                   
                    
                    if (PaswortStepChange == 1) {PaaswortString.Text = ""; Help.Mesag("Password is incorrect"); return; }
                }



                if ((PaaswortString.Text == PaaswortStringRepeet) && (PaswortStepChange == 2)) {
                    PaswortStepChange = 0;
                    STGS.DT.Password = PaaswortString.Text;
                    STGS.Save();
                    PaaswortString.Text = "";
                    PasworLable.Text = "Change OK";
                    LockFunk(true);
                    IDtex.Text = "User mode"; // Label visual Mode
                 
                }
                else
                {
                    PaaswortString.Text = "";
                    if (PaswortStepChange == 2) { Help.Mesag("Password is incorrect"); }
                    PaswortStepChange = 0;
                   
                }

            }else { PaswortStepChange = 1; Help.Mesag("Password changing possible only in admin mode"); };
        }



        private void PaaswortString_Enter(object sender, KeyEventArgs e)
        {
         if (e.KeyCode == Keys.Enter)
            {
                button11_Click(null, null);
            }
        }


    }

}
