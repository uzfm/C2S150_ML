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
        DataSV DataSV = new DataSV();
        FlowCamera flowCamera = new FlowCamera();
        USB_HID USB_HID = new USB_HID();



        public List<Image<Gray, byte>> MosaicGrey = new List<Image<Gray, byte>>();
        public List<Image<Gray, byte>> MosaicsTeachGrey = new List<Image<Gray, byte>>();

        public ImageList Mosaics = new ImageList();
        public ImageList MosaicsTeach = new ImageList();
        static List<DTLimg> MosaicDTlist; //буфер ліст для буферезації перед сортуваням




        private ChartValues<ObservablePoint> chartValues;
        private DateTime startTime;



        public Sorter()
        {
            InitializeComponent();

            ////******   USB HID INSTAL *********//
            USB_HID.InstalDevice("C1");
            CAMERA.ON();
            InstMosaics();
            DataSV.Deserializ();
            RefreshSetings();




            timer1.Enabled = true;
            //*************  initialization of cameras  *********************






           // DLS.InstCOM_Setings(DLS.Master);
            //DLS.InstCOM_Setings(DLS.Slave);
            //***************************************************************

            //************************    запустити поток      ********************************/
            Flow.AnalisBlobs();
            Flow.BlobsPredict();         //запустити поток на аналіз Img
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




        }

        private void button54_Click(object sender, EventArgs e) {
            AnalisPredict.GoodSamples = 0;
            AnalisPredict.BadSamples = 0;
            SpidIdxEv = 0;
            ANLImg_M.CauntOllBlob = 0;
            /////////////// Діаграма швиткості ////////////////
            solidGauge1.Value = 0.00;
            SpidKgH.Text = "0.00";
        }


        short TimOutRefresh;
        int RatioSampls;
        double RatioSamplsOll;
        int SpidIdxEv = 0;

        private int rowIndex = 0;
        int sampleCount = 25000; // Кількість семплів
        double timeInSeconds = 3600; // Інтервал часу в секундах
        private void TimerRefreshChart()
        {


            if (buttonStartAnalic.Text == "Stop Analysis") {

                TimOutRefresh++;


                double ratio3 = 0;

                if (SpidIdxEv != 0) { ratio3 = ((((double)AnalisPredict.GoodSamples + (double)AnalisPredict.BadSamples) / (double)sampleCount)) / ((double)(SpidIdxEv / 2) / (double)timeInSeconds); }   // Діаграма швиткості середння
                SpidIdxEv++;
                if (TimOutRefresh >= 10) { TimOutRefresh = 0;

                    // Отримання поточного часу та значення для додавання до даних
                    DateTime currentTime = DateTime.Now;
                    double ratio2 = 0;
                    if ((SpidIdxEv != 0)) { ratio2 = ((((double)AnalisPredict.GoodSamples + (double)AnalisPredict.BadSamples) - (double)RatioSamplsOll) / (double)sampleCount) * (double)timeInSeconds / 5; }   // Діаграма швиткості
                    if (ratio2 < 0) { ratio2 = 0; };



                    // Обчислення відношення хороших/поганих зразків за хвилину
                    double ratio = ((double)AnalisPredict.GoodSamples / (double)(AnalisPredict.GoodSamples + AnalisPredict.BadSamples)) * 100;
                    double ratio1 = ((double)AnalisPredict.BadSamples / (double)(AnalisPredict.GoodSamples + AnalisPredict.BadSamples)) * 100;

                    double GoodKg = 0.0;
                    double TotalKg = 0.0;
                    if (AnalisPredict.GoodSamples != 0) { GoodKg = ((double)AnalisPredict.GoodSamples / (double)sampleCount); }                             // Кількість Good Kg
                    if (AnalisPredict.BadSamples != 0) { TotalKg = (((double)AnalisPredict.GoodSamples + (double)AnalisPredict.BadSamples) / (double)sampleCount); }  // Кількість Total Kg
                    RatioSamplsOll = ((double)AnalisPredict.GoodSamples + (double)AnalisPredict.BadSamples);


                    ratio = Math.Round(ratio, 4);
                    ratio1 = Math.Round(ratio1, 4);
                    ratio2 = Math.Round(ratio2, 2);
                    ratio3 = Math.Round(ratio3, 2);

                    GoodKg = Math.Round(GoodKg, 2);
                    TotalKg = Math.Round(TotalKg, 2);



                    ///////////// Додавання даних до таблиці ////////////
                    dataGridView1.Rows.Add(currentTime, ratio, ratio1, ratio2, GoodKg, TotalKg);

                    /////////////// Діаграма швиткості ////////////////
                    solidGauge1.Value = ratio2;
                    SpidKgH.Text = ratio3.ToString();

                    // Прокрутка таблиці до останнього рядка
                    dataGridView1.FirstDisplayedScrollingRowIndex = rowIndex;
                    rowIndex++;

                    //if (AnalisPredict.BadSamples != RatioSampls) {}
                    int Ratio = AnalisPredict.BadSamples - RatioSampls;
                    RatioSampls = AnalisPredict.BadSamples;

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

            if (buttonStartAnalic.Text == "Start Analysis")
            {



                LIGHT.ON();
        
                buttonStartAnalic.Text = "Stop Analysis";
                StartTable.Text = "Stop Table";
                OnLight.Text = "OFF Light";
                button40.Text = "OFF Cooling";
                button43.Text = "OFF Screw Feeder";
                button44.Text = "OFF Metal separator";
                Thread.Sleep(500);
                Flow.STARTsorting = true;
                Flow.StartSorting();
                VIBR_TABLE.SET(VIBR_TABLE.Typ.ON);
            }
            else
            {

                VIBR_TABLE.SET(VIBR_TABLE.Typ.OFF);
                Flow.StartSorting();
                Flow.STARTsorting = false;
                Thread.Sleep(100);

                LIGHT.OFF();
          

                buttonStartAnalic.Text = "Start Analysis";
                StartTable.Text = "Start Table";
                OnLight.Text  = "ON Light";
                button40.Text = "ON Cooling";
                button43.Text = "ON Screw Feeder";
                button44.Text = "ON Metal separator";
            }
        }


        void RefreshSetings()
        {
            try
            {

                SetingsCameraStart.Checked = SETS.Data.SetingsCameraStart;
                checkBox4.Checked = SETS.Data.CameraAnalis_1;
                checkBox3.Checked = SETS.Data.CameraAnalis_2;
                textBox1.Text = SETS.Data.PashTestIMG;
                radioButtonCam1.Checked = SETS.Data.LiveViewCam;
                //---------------------------------------------------

                GreyScaleMax_.Value = EMGU.Data.GreyScaleMax[ID];
                GreyScaleMin_.Value = EMGU.Data.GreyScaleMin[ID];

                GreyMax_.Value = (decimal)EMGU.Data.GreySizeMax[ID];
                GreyMin_.Value = (decimal)EMGU.Data.GreySizeMin[ID];

                Hz_Table.Value = USB_HID.Data.Hz_Table;
                PWM_Table.Value = USB_HID.Data.PWM_Table;
                OutputDelay.Value = USB_HID.Data.Fleps_Time_OFF;
                FLAPS.Time_OFF(OutputDelay.Value);

                if (SETS.Data != null) { checkBox13.Checked = SETS.Data.LiveVideoOFF; } else {
                    SETS.Data = new SETS.DATA_Save();
                    Help.ErrorMesag("saved data setings not correct !");
                }

                numericUpDown1.Value = SETS.Data.DoublingFlaps; //
                numericUpDown6.Value = SETS.Data.LimitinGraphPoints;
                numericUpDown7.Value = SETS.Data.UpdateVisibleArea;
                numericUpDown8.Value = SETS.Data.AxisYMaxValue;

                  GAIN1.Value = SETS.Data.GEIN1;
                  GAIN2.Value = SETS.Data.GEIN2;
                 InvertBlobs.Checked = SETS.Data.BlobsInvert;

                 LockIR.Checked     = USB_HID.Data.Light_IR;
                 LockTop.Checked    = USB_HID.Data.Light_Top;
                 LockBack.Checked   = USB_HID.Data.Light_Back;
                 LockBottom.Checked = USB_HID.Data.Light_Bottom;

            }
            catch
            {

                EMGU.Data = new EMGU.DATA_Save();
                USB_HID.Data = new USB_HID.DATA_Save();
                Help.ErrorMesag("saved data not correct !");
            }
        }


        /// <summary>
        /// -----------------------    SAVE VALUE   ----------------------------------------+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Save_Click(object sender, EventArgs e) {


            SaveSetValue();
            DataSV.DirectSave("SaveSV.txt");
            DataSV.Serializ();
        }

        void SaveSetValue()
        {
            SETS.Data.GEIN1 = GAIN1.Value;
            SETS.Data.GEIN2 = GAIN2.Value;
            SETS.Data.SetingsCameraStart = SetingsCameraStart.Checked;
            SETS.Data.CameraAnalis_1 = checkBox4.Checked;
            SETS.Data.CameraAnalis_2 = checkBox3.Checked;
            SETS.Data.PashTestIMG = textBox1.Text;
            SETS.Data.LiveViewCam = radioButtonCam1.Checked;

            SETS.Data.BlobsInvert = InvertBlobs.Checked;

            USB_HID.Data.Light_IR     = LockIR.Checked;
            USB_HID.Data.Light_Top    = LockTop.Checked;
            USB_HID.Data.Light_Back   = LockBack.Checked;
            USB_HID.Data.Light_Bottom = LockBottom.Checked;

        }





        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            GreyMax_.Value = GreyMax.Value;
        }

        private void MastConturMax_ValueChanged(object sender, EventArgs e)
        {
            if (GreyMax_.Value != GreyMax.Value) { GreyMax.Value = Convert.ToInt32(GreyMax_.Value); }
            EMGU.Data.GreySizeMax[ID] = (double)GreyMax_.Value;
        }

        private void trackBar7_Scroll(object sender, EventArgs e) { GreyMin_.Value = GreyMin.Value; }

        private void GreyMin__ValueChanged(object sender, EventArgs e)
        {
            if (GreyMin_.Value != GreyMin.Value) { GreyMin.Value = Convert.ToInt32(GreyMin_.Value); }
            EMGU.Data.GreySizeMin[ID] = (double)GreyMin_.Value;
        }

        //****************************/
        private void trackBar1_Scroll(object sender, EventArgs e) { GreyScaleMax_.Value = GreyScaleMax.Value; }

        private void MastMinR_ValueChanged(object sender, EventArgs e)
        {
            if (GreyScaleMax_.Value != GreyScaleMax.Value) { GreyScaleMax.Value = Convert.ToInt32(GreyScaleMax_.Value); }
            EMGU.Data.GreyScaleMax[ID] = GreyScaleMax.Value;

        }

        private void GreyScaleMin_Scroll(object sender, EventArgs e)
        {
            GreyScaleMin_.Value = GreyScaleMin.Value;
        }

        private void GreyScaleMin__ValueChanged(object sender, EventArgs e)
        {
            if (GreyScaleMin_.Value != GreyScaleMin.Value) { GreyScaleMin.Value = Convert.ToInt32(GreyScaleMin_.Value); }
            EMGU.Data.GreyScaleMin[ID] = GreyScaleMin.Value;
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
            CauntSamls.Text = ANLImg_M.CauntOllBlob.ToString();
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
            else { Help.ErrorMesag("You cannot select image or generate a report when the images are not sorted! "); }

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
        private void PWM_Table_Entr(object sender, KeyPressEventArgs e) { PWM_Table_Click(null, null); }


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
                    if (textBox1.Text == "") { break; }
                    ImagAI.Save(textBox1.Text + "\\" + "Image" + i.ToString() + ".jpg");


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

                    textBox1.Text = FBD.SelectedPath;
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

            string urlMaster = textBox1.Text + "\\" + "Image" + IdxShou++ + ".jpg";
            FlowAnalis.Setings = true;
            string[] files = Directory.GetFiles(@textBox1.Text, "*.jpg");

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
        private void OutputDelay_Click(object sender, EventArgs e) { FLAPS.Time_OFF(OutputDelay.Value); }
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
        Vision vision = new Vision();

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
                    Image<Bgr, byte>[] ImagesViw = new Image<Bgr, byte>[2]; 

                    for (int i = 0; i < 1000; i++)
                    {

                         ImagesViw = vision.DetectBlob(imOriginalM, (int)numericUpDown10.Value, (int)numericUpDown11.Value, (int)numericUpDown9.Value);

                    }


                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    toolStripStatusLabel5.Text = elapsedMs.ToString();
                   // Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

                    pictureBox1.Image = ImagesViw[0].ToBitmap();
                    pictureBox2.Image = ImagesViw[1].ToBitmap();


                }
                catch { }
            }
            else { }

        }


      void SetingsValGrayImg (){

            if (IdxShouTest <= files.Length)
            {
                try
                {
                    Bitmap imM = new Bitmap(files[IdxShouTest]);
                    textBox3.Text = IdxShouTest.ToString();

                    imOriginalM = imM.ToImage<Bgr, byte>().Resize(64, 64, interpolationType: Inter.Linear).Mat;

                    Stopwatch watch = Stopwatch.StartNew();
                    Image<Bgr, byte>[] ImagesViw = new Image<Bgr, byte>[2];


                        ImagesViw = vision.DetectBlob(imOriginalM, (int)numericUpDown10.Value, (int)numericUpDown11.Value, (int)numericUpDown9.Value);



                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    toolStripStatusLabel5.Text = elapsedMs.ToString();
                    // Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

                    pictureBox1.Image = ImagesViw[0].ToBitmap();
                    pictureBox2.Image = ImagesViw[1].ToBitmap();


                }
                catch { }
            }

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
        }

        private void numericUpDown11_Click(object sender, EventArgs e)
        {
            SetingsValGrayImg();
        }




        short coutTim=0;

        private void timer3_Tick(object sender, EventArgs e)
        {

            //if (coutTim==5) {  DLS. InstCOM_Setings(DLS.Slave);}

            if (coutTim == 5) {
                coutTim++;
                try { DLS = new DLS(); }
            catch { Help.ErrorMesag("Cameras are not connected"); }
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

 


            }



                coutTim++;
        }

        private void GAIN1_Click(object sender, EventArgs e)
        {
            DLS.SetGain((double)GAIN1.Value, DLS.Master);
        }



        private void GAIN2_Click(object sender, EventArgs e)
        {
 DLS.SetGain((double)GAIN2.Value, DLS.Slave);
        }
    }
}
