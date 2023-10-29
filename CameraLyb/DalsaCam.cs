

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using DALSA.SaperaLT.SapClassBasic;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
//using ZedGraph;
using DALSA.SaperaLT.SapClassGui;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.Util.TypeEnum;
using Emgu.Util;
using System.Collections.Concurrent;
using System.Collections;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace C2S150_ML
{





    static public class DLSWiew {


        static public void PictBox(Image img , int ID_Cam)
        {


          //  BeginInvoke((MethodInvoker)(() => Imgs[ID_Cam] = null));  // добавляєм нормальні семпли 

        }

    }

    public partial class DLS
    {
       // public static bool StartStatus = false;
        public const int Master = 0;
        public const int Slave = 1;

        public static DalsaVal DalsaVal = new DalsaVal();



    


     public static  class Devis {
    public static bool[] Status = new bool[2];
     public static double[] Gain = new double[2];

        }


        public DLS() {
            // Note:
            //  The code to initialize m_ImageBox was originally in the InitializeComponent function
            //  called above. However, it has been moved to the dialog constructor as a workaround
            //  to a Visual Studio Designer error when loading the DALSA.SaperaLT.SapClassBasic
            //  assembly under 64-bit Windows.
            //  As a consequence, it is not possible to adjust the m_ImageBox properties
            //  automatically using the Designer anymore, this has to be done manually.
            // 




            Devis.Status[Master] = InstCOM_Setings(Master);
            Devis.Status[Slave] = InstCOM_Setings(Slave);


            DalsaVal.m_Acquisition[Master] = null;
            DalsaVal.m_Buffers[Master] = null;
            DalsaVal.m_Xfer[Master] = null;
            DalsaVal.m_View[Master] = null;
            DalsaVal.m_bRecordOn[Master] = false;
            DalsaVal.m_bPlayOn[Master] = false;
            DalsaVal.m_bPauseOn[Master] = false;
            DalsaVal.m_nFramesPerCallback[Master] = 1;
            DalsaVal.m_nFramesOnBoard[Master] = 4096;
            DalsaVal.m_AboutID[Master] = 256;
            DalsaVal.Default_Nbr_buffers[Master] = 15;

            DalsaVal.m_Acquisition[Slave] = null;
            DalsaVal.m_Buffers[Slave] = null;
            DalsaVal.m_Xfer[Slave] = null;
            DalsaVal.m_View[Slave] = null;
            DalsaVal.m_bRecordOn[Slave] = false;
            DalsaVal.m_bPlayOn[Slave] = false;
            DalsaVal.m_bPauseOn[Slave] = false;
            DalsaVal.m_nFramesPerCallback[Slave] = 1;
            DalsaVal.m_nFramesOnBoard[Slave] = 4096;
            DalsaVal.m_AboutID[Slave] = 256;
            DalsaVal.Default_Nbr_buffers[Slave] = 15;



            AcqConfigDlg acConfigDlg = new AcqConfigDlg(null, "", AcqConfigDlg.ServerCategory.ServerAcq, Master);

            if (SETS.Data.SetingsCameraStart) {

                // для того щоб в ручну вибрати тип камери це розкоментувати а нижню умову закоментувати
                if (acConfigDlg.ShowDialog() == DialogResult.OK)
                {
                    //// Потрібно вибрати тип камери Color RGB or Mono
                    if (acConfigDlg.ClousedDialogOK_Mono() == true)
                    {
                        //acConfigDlg.Show();
                        DalsaVal.m_online[Master] = true;
                        acConfigDlg.Close();
                    } else { DalsaVal.m_online[Master] = false; }

                }

            } else {
                acConfigDlg.Show();
                DalsaVal.m_online[Master] = true;
                acConfigDlg.Close();

            }

            if (!CreateNewObjects(DalsaVal.Default_Nbr_buffers[Master], acConfigDlg, Master)) ;
            acConfigDlg.Close();


            AcqConfigDlg acConfigDlg2 = new AcqConfigDlg(null, "", AcqConfigDlg.ServerCategory.ServerAcq, Slave);

            if (SETS.Data.SetingsCameraStart)
            {            //acConfigDlg2.ClousedDialogOK(); //colour RGB
                         // acConfigDlg2.ClousedDialogOK_Mono(); //colour MONO GRAY
                         // if (acConfigDlg2.ShowDialog() == DialogResult.OK)
                         //acConfigDlg2.Show();
                         //FOR GRAY CAMERA
                         //  if (acConfigDlg2.ClousedDialogOK_Mono() == true)  {
                         //FOR COLOUR CAMERA

                //acConfigDlg2.ClousedDialogOK(); //colour RGB
                // acConfigDlg2.ClousedDialogOK_Mono(); //colour MONO GRAY

                if (acConfigDlg2.ShowDialog() == DialogResult.OK)
                    if (acConfigDlg2.ClousedDialogOK_Mono() == true)
                    {

                        DalsaVal.m_online[Slave] = true;
                        acConfigDlg2.Close();
                    }
                    else { DalsaVal.m_online[Slave] = false; }
            }
            else {

                acConfigDlg2.Show();
                DalsaVal.m_online[Slave] = true;
                acConfigDlg2.Close();
            }

            if (!CreateNewObjects(DalsaVal.Default_Nbr_buffers[Slave], acConfigDlg2, Slave)) ;
            acConfigDlg2.Close();


        }

        public bool Setings_Camera1() {
            AcqConfigDlg acConfigDlg = new AcqConfigDlg(null, "", AcqConfigDlg.ServerCategory.ServerAcq, Master);
            if (acConfigDlg.ShowDialog() == DialogResult.OK){
                acConfigDlg.Show();
                DalsaVal.m_online[Master] = true;
                acConfigDlg.Close();
            } else {DalsaVal.m_online[Master] = false;
            } if (!CreateNewObjects(DalsaVal.Default_Nbr_buffers[Master], acConfigDlg, Master)) ;
            Help.Mesag("Restart the program for update the settings !!!");
            return DalsaVal.m_online[Master];
        }

        public bool Setings_Camera2() {
            AcqConfigDlg acConfigDlg2 = new AcqConfigDlg(null, "", AcqConfigDlg.ServerCategory.ServerAcq, Slave);
            if (acConfigDlg2.ShowDialog() == DialogResult.OK) {
                acConfigDlg2.Show();
                DalsaVal.m_online[Slave] = true;
                acConfigDlg2.Close();
            } else { DalsaVal.m_online[Slave] = false; }
            //if (!CreateNewObjects(DalsaVal.Default_Nbr_buffers[Slave], acConfigDlg2, Slave)) ;
            Help.Mesag("Restart the program for update the settings !!!");
            return DalsaVal.m_online[Slave];



        }

        //*****************************************************************************************
        //
        //					Create and Destroy Object
        //
        //*****************************************************************************************


        ImageBox[] imageBox1 = new ImageBox[2];

        // Create new objects with acquisition information
        public bool CreateNewObjects(int Nbr_Buffers, AcqConfigDlg acConfigDlg, int ID_Cam)
        {



            if (DalsaVal.m_online[ID_Cam])
            {
                if (acConfigDlg != null)
                {
                    DalsaVal.m_ServerLocation[ID_Cam] = acConfigDlg.ServerLocation;
                    DalsaVal.m_ConfigFileName[ID_Cam] = acConfigDlg.ConfigFile;

                }



                // define on-line object
                DalsaVal.m_Acquisition[ID_Cam] = new SapAcquisition(DalsaVal.m_ServerLocation[ID_Cam], DalsaVal.m_ConfigFileName[ID_Cam]);


                if (SapBuffer.IsBufferTypeSupported(DalsaVal.m_ServerLocation[ID_Cam], SapBuffer.MemoryType.ScatterGather))
                    DalsaVal.m_Buffers[ID_Cam] = new SapBuffer(Nbr_Buffers, DalsaVal.m_Acquisition[ID_Cam], SapBuffer.MemoryType.ScatterGather);
                else
                    DalsaVal.m_Buffers[ID_Cam] = new SapBuffer(Nbr_Buffers, DalsaVal.m_Acquisition[ID_Cam], SapBuffer.MemoryType.ScatterGatherPhysical);
                DalsaVal.m_Xfer[ID_Cam] = new SapAcqToBuf(DalsaVal.m_Acquisition[ID_Cam], DalsaVal.m_Buffers[ID_Cam]);
                // DalsaVal.m_View[ID_Cam] = new SapView(DalsaVal.m_Buffers[ID_Cam]);

                //event for view  ===== M A S T E R ======
                if (ID_Cam == Master)
                {
                    DalsaVal.m_FlatField[Master] = new SapFlatField(DalsaVal.m_Acquisition[Master]);
                    DalsaVal.m_Xfer[ID_Cam].Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
                    DalsaVal.m_Xfer[ID_Cam].XferNotify += new SapXferNotifyHandler(xfer_XferNotifyMast);
                    DalsaVal.m_Xfer[ID_Cam].XferNotifyContext = this;
                }

                //event for view  ===== S L A V E ======
                if (ID_Cam == Slave)
                {
                    DalsaVal.m_FlatField[Slave] = new SapFlatField(DalsaVal.m_Acquisition[Slave]);
                    DalsaVal.m_Xfer[ID_Cam].Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
                    DalsaVal.m_Xfer[ID_Cam].XferNotify += new SapXferNotifyHandler(xfer_XferNotifySlav);
                    DalsaVal.m_Xfer[ID_Cam].XferNotifyContext = this;
                }


                //SDalsaVal.m_FlatField[ID_Cam] = new SapFlatField(DalsaVal.m_Acquisition[ID_Cam]);
                
         

                /////////////////////////////////////













                //  SapAcqDevice acqDevice = new SapAcqDevice(new SapLocation("Genie_M640_1", 0));

                // Задайте нове значення гейна
                // double newGainValue = 2.0; // Нове значення гейна, яке ви хочете встановити
                // gainFeature =     SapFeature.Type.Float ;

                // Тепер ви можете встановити нове значення гейна для функції "Gain"
                // DalsaVal.Camera[Master].SetFeatureValue(featureName, gainFeature);


                //}

                // event fot Acqcallback (Frame lost)
                // DalsaVal.m_Acquisition[ID_Cam].EventType = SapAcquisition.AcqEventType.FrameLost;
                //  DalsaVal.m_Acquisition[ID_Cam].AcqNotify += new SapAcqNotifyHandler(AcqCallback);
                // DalsaVal.m_Acquisition[ID_Cam].AcqNotifyContext = this;


                // event for signal status
                //DalsaVal.m_Acquisition[ID_Cam].SignalNotify += new SapSignalNotifyHandler(GetSignalStatusMast);
                //DalsaVal.m_Acquisition[ID_Cam].SignalNotifyContext = this;
            }
            else
            {
                //define off-line object
                //m_Buffers = new SapBuffer();
                //m_Buffers.Count = Nbr_Buffers;
                //m_View = new SapView(m_Buffers);
            }



            //imageBox1[ID_Cam] = new ImageBox();
            //imageBox1[ID_Cam].View = DalsaVal.m_View[ID_Cam];
            //imageBox1[ID_Cam].OnSize();
            //imageBox1[ID_Cam].ViewImg();
            //imageBox1[ID_Cam].Show();

            if (!CreateObjects(ID_Cam))
            {
                DisposeObjects(ID_Cam);
                return false;
            }

            /// DLSWiew.imageBox1.OnSize();
            UpdateControls(ID_Cam);
            EnableSignalStatus(ID_Cam);
            return true;
        }


     static   SapFeature [] feature = new SapFeature[2];
   public  bool InstCOM_Setings(int ID_Cam)
        {



           

            int serverCount = SapManager.GetServerCount();




            string[] ServerName = new string[serverCount];
            int[] ResourceIndex = new int[serverCount];

            if (serverCount == 0)
            {
                Console.WriteLine("No device found!\n");
                return false;
            }
            int GrabberIndex = 0;
            for (int serverIndex = 1; serverIndex < serverCount ; serverIndex++)
            {
               

                //if (SapManager.GetResourceCount(serverIndex, SapManager.ResourceType.AcqDevice) != 0)  {
              
                string serverName = SapManager.GetServerName(serverIndex);
                if ((SapManager.GetResourceCount(serverIndex, SapManager.ResourceType.Acq) == 0))
                {
                    ServerName[GrabberIndex] = serverName;
                    ResourceIndex[GrabberIndex] = GrabberIndex;
                    GrabberIndex++;

                }
                //}

            }


            /////////////////////
             if(ID_Cam != ResourceIndex[ID_Cam]) { return false; }

            SapLocation location = new SapLocation(ServerName[ID_Cam], 0  ); //"CameraLink_1" //0

           


            //DalsaVal.m

            // Create a camera object and assign delegate
            DalsaVal.device[ID_Cam] = new SapAcqDevice(location, "");

            DalsaVal.device[ID_Cam].AcqDeviceNotify += new SapAcqDeviceNotifyHandler(AcqDeviceCallback);
            if (!DalsaVal.device[ID_Cam].Create())
            {
                DestroyObjects(ID_Cam);
                return false;
            }


            // Create an empty feature object (to receive information)


            feature[ID_Cam] = new SapFeature(location);
            if (!feature[ID_Cam].Create())
            {
                DestroyObjects(ID_Cam);
                return false;
            }


            SetGain(0,  ID_Cam);


            return true;

        }



   public   bool  SetGain(double  gainValue, int ID_Cam)
        {


            Boolean isGenie = false, isAvailable = false, isSFNCDeprecated = false, status = false;
            string modelName;




            status = DalsaVal.device[ID_Cam].IsFeatureAvailable("DeviceModelName");
            if (status)
            {
                status = DalsaVal.device[ID_Cam].IsFeatureAvailable("FrameRate");
                DalsaVal.device[ID_Cam].GetFeatureValue("DeviceModelName", out modelName);
                if (status && modelName.Contains("Genie"))
                {
                    isGenie = true;
                }
            }



            status = false;
            isAvailable = DalsaVal.device[ID_Cam].IsFeatureAvailable("Gain");
            if (isAvailable)
                DalsaVal.device[ID_Cam].GetFeatureInfo("Gain", feature[ID_Cam]);
            else if (isAvailable = DalsaVal.device[ID_Cam].IsFeatureAvailable("GainRaw"))
            {
                DalsaVal.device[ID_Cam].GetFeatureInfo("GainRaw", feature[ID_Cam]);
                isSFNCDeprecated = true;
            }

            int currentGainInt = 0;
            double currentGainDouble = 0;

            if (isGenie || isSFNCDeprecated)
            {
                int gainMin = 0, gainExponent = 0, gainMax = 0;
                double powValue = 0;
                feature[ID_Cam].GetValueMax(out gainMax);
                feature[ID_Cam].GetValueMin(out gainMin);
                gainExponent = feature[ID_Cam].SiToNativeExp10;
                powValue = Convert.ToDouble(Math.Pow((float)10, -gainExponent));

                // Get current Gain value in camera
                if (isSFNCDeprecated)
                    DalsaVal.device[ID_Cam].GetFeatureValue("GainRaw", out currentGainInt);
                else
                    DalsaVal.device[ID_Cam].GetFeatureValue("Gain", out currentGainInt);
                Devis.Gain[ID_Cam] = currentGainInt;
                Console.WriteLine("\nCurrent gain value is " + Convert.ToString(currentGainInt) + "\n");

                gainValue = Convert.ToInt32(gainMax * powValue);
                if (currentGainInt == gainValue)
                    gainValue = Convert.ToInt32(gainMin * powValue);

                if (isSFNCDeprecated)
                    DalsaVal.device[ID_Cam].SetFeatureValue("GainRaw", gainMax);
                else
                    DalsaVal.device[ID_Cam].SetFeatureValue("Gain", gainMax);
                   Console.WriteLine("\nSet Gain value to " + Convert.ToString(gainValue) + "\n");

            }
            else
            {
                double gainMin = 0, gainMax = 0,  powValue = 0;
                int gainExponent = 0;
                feature[ID_Cam].GetValueMax(out gainMax);
                feature[ID_Cam].GetValueMin(out gainMin);
                gainExponent = feature[ID_Cam].SiToNativeExp10;
                powValue = Convert.ToDouble(Math.Pow((float)10, -gainExponent));

                // Get current Gain value in camera
                DalsaVal.device[ID_Cam].GetFeatureValue("Gain", out currentGainDouble);
                Console.WriteLine("\nCurrent gain value is " + String.Format("{0:0.00}", currentGainDouble) + "\n");

                Devis.Gain[ID_Cam] = currentGainDouble;
                //Set gain to max. if it's already at max, set it to min.
        
                if ((gainValue>=gainMin)&&(gainValue <= gainMax)) { 
                DalsaVal.device[ID_Cam].SetFeatureValue("Gain", gainValue);
                Console.WriteLine("\nSet Gain value to " + String.Format("{0:0.00}", gainValue) + "\n");}
            }


            return true;
        }





        static void AcqDeviceCallback(Object sender, SapAcqDeviceNotifyEventArgs args)
        {
            SapAcqDevice acqDevice = sender as SapAcqDevice;
            Console.WriteLine("AcqDeviceNotify event \"{0}\", Feature = \"{1}\"",
                acqDevice.EventNames[args.EventIndex], acqDevice.FeatureNames[args.FeatureIndex]);




        }




        public ImageBox TEST_VI(  ) {


            return imageBox1[0];
        }


        public void UpdateControls(int ID_Cam)
        {

            bool bAcqNoGrab = DalsaVal.m_online    [ID_Cam] && !DalsaVal.m_bRecordOn[ID_Cam] && !DalsaVal.m_bPlayOn[ID_Cam];
            bool bNoGrab    = !DalsaVal.m_bRecordOn[ID_Cam] && !DalsaVal.m_bPlayOn[ID_Cam];

            //// Record Control
            //button_Record.Enabled = bAcqNoGrab;
            //button_Play.Enabled = bNoGrab;
            //button_Pause.Enabled = !bNoGrab;
            //button_Stop.Enabled = !bNoGrab;
            //button_Pause.Text = m_bPauseOn ? "Continue" : "Pause";

            //// General Options
            //button_Buffers.Enabled = bNoGrab;
            //button_Load_Config.Enabled = bNoGrab;
            //button_High_Frame_Rate.Enabled = bNoGrab;

            //// File Options
            //button_Load_Current.Enabled = bNoGrab;
            //button_Load_Sequence.Enabled = bNoGrab;
            //button_Save_Current.Enabled = bNoGrab;
            //button_Save_Sequence.Enabled = bNoGrab;

            //// Recording statistics
            //textBox_Frame_Rate.Enabled = bNoGrab;

            // Slider
            //DLSWiew.imageBox1.SliderEnable = bNoGrab || (DalsaVal.m_bPlayOn[ID_Cam] && DalsaVal.m_bPauseOn[ID_Cam]);
            //DLSWiew.imageBox1.SliderMinimum = 0;
            //DLSWiew.imageBox1.SliderMaximum = DalsaVal.m_Buffers[ID_Cam].Count - 1;
        }


        //public PictureBox PictBox() {
        //    return DLSWiew .imgBox;
        //}


        private void EnableSignalStatus(int ID_Cam)
        {
            if (DalsaVal.m_Acquisition[ID_Cam] != null)
            {
                DalsaVal.m_IsSignalDetected[ID_Cam] = (DalsaVal.m_Acquisition[ID_Cam].SignalStatus != SapAcquisition.AcqSignalStatus.None);
                if (DalsaVal.m_IsSignalDetected[ID_Cam] == false)
                { }  //StatusLabelInfo.Text = "Online... No camera signal detected";
                else
                    //StatusLabelInfo.Text = "Online... Camera signal detected";
                    DalsaVal.m_Acquisition[ID_Cam].SignalNotifyEnable = true;
            }
        }

        private bool CreateObjects(int ID_Cam)
        {




            // Create acquisition object
            //Xtium-CL_MX4_1 [ CameraLink Medium Color RGB ]
            if (DalsaVal.m_Acquisition[ID_Cam] != null && !DalsaVal.m_Acquisition[ID_Cam].Initialized)
            {
                if (DalsaVal.m_Acquisition[ID_Cam].Create() == false)
                {
                    DestroyObjects(ID_Cam);
                    return false;
                }
            }
            // Create buffer object
            if (DalsaVal.m_Buffers[ID_Cam] != null && !DalsaVal.m_Buffers[ID_Cam].Initialized)
            {
                if (DalsaVal.m_Buffers[ID_Cam].Create() == false)
                {
                    DestroyObjects(ID_Cam);
                    return false;
                }
                DalsaVal.m_Buffers[ID_Cam].Clear();
            }
            // Create view object
            if (DalsaVal.m_View[ID_Cam] != null && !DalsaVal.m_View[ID_Cam].Initialized)
            {
                if (DalsaVal.m_View[ID_Cam].Create() == false)
                {
                    DestroyObjects(ID_Cam);
                    return false;
                }
            }
            // Create Xfer object
            if (DalsaVal.m_Xfer[ID_Cam] != null && !DalsaVal.m_Xfer[ID_Cam].Initialized)
            {

                //Number of frames per callback retreived
                int nFramesPerCallback;

                // Set number of onboard buffers
                DalsaVal.m_Xfer[ID_Cam].Pairs[0].FramesOnBoard = DalsaVal.m_nFramesOnBoard[ID_Cam];

                // Set number of frames per callback
                DalsaVal.m_Xfer[ID_Cam].Pairs[0].FramesPerCallback = DalsaVal.m_nFramesPerCallback[ID_Cam];

                // If there is a large number of buffers, temporarily boost the command timeout value,
                // since the call to Create may take a long time to complete.
                // As a safe rule of thumb, use 100 milliseconds per buffer.
                int oldCommandTimeout = SapManager.CommandTimeout;
                int newCommandTimeout = 100 * DalsaVal.m_Buffers[ID_Cam].Count;

                if (newCommandTimeout < oldCommandTimeout)
                    newCommandTimeout = oldCommandTimeout;

                SapManager.CommandTimeout = newCommandTimeout;

                // Create transfer object
                if (!DalsaVal.m_Xfer[ID_Cam].Create())
                {
                    DestroyObjects(ID_Cam);
                    return false;
                }

                // Restore original command timeout value
                SapManager.CommandTimeout = oldCommandTimeout;
                // initialize tranfer object and reset source/destination index

                DalsaVal.m_Xfer[ID_Cam].Init(true);

                // Retrieve number of frames per callback
                // It may be less than what we have asked for.
                nFramesPerCallback = DalsaVal.m_Xfer[ID_Cam].Pairs[0].FramesPerCallback;
                if (DalsaVal.m_nFramesPerCallback[ID_Cam] > nFramesPerCallback)
                {
                    DalsaVal.m_nFramesPerCallback[ID_Cam] = nFramesPerCallback;
                    MessageBox.Show("No memory");
                }

                DalsaVal.m_nFramesOnBoard[ID_Cam] = DalsaVal.m_Xfer[ID_Cam].Pairs[0].FramesOnBoard;
            }


            // Create flat field object
            if (DalsaVal.m_FlatField[ID_Cam] != null && !DalsaVal.m_FlatField[ID_Cam].Initialized)
            {
                if (!DalsaVal.m_FlatField[ID_Cam].Create())
                {
                    DestroyObjects(ID_Cam);
                    return false;
                }
            }



            return true;
        }

        private void DestroyObjects(int ID_Cam)
        {
            if (DalsaVal.m_Xfer[ID_Cam] != null && DalsaVal.m_Xfer[ID_Cam].Initialized)
                DalsaVal.m_Xfer[ID_Cam].Destroy();
            if (DalsaVal.m_View[ID_Cam] != null && DalsaVal.m_View[ID_Cam].Initialized)
                DalsaVal.m_View[ID_Cam].Destroy();
            if (DalsaVal.m_Buffers[ID_Cam] != null && DalsaVal.m_Buffers[ID_Cam].Initialized)
                DalsaVal.m_Buffers[ID_Cam].Destroy();
            if (DalsaVal.m_Acquisition[ID_Cam] != null && DalsaVal.m_Acquisition[ID_Cam].Initialized)
                DalsaVal.m_Acquisition[ID_Cam].Destroy();
            if (DalsaVal.m_FlatField != null && DalsaVal.m_FlatField[ID_Cam].Initialized)
            {DalsaVal.m_FlatField[ID_Cam].Destroy();}

        }
        private void DisposeObjects(int ID_Cam)
        {
            if (DalsaVal.m_Xfer[ID_Cam] != null)
            { DalsaVal.m_Xfer[ID_Cam].Dispose(); DalsaVal.m_Xfer[ID_Cam] = null; }
            if (DalsaVal.m_View[ID_Cam] != null)
            { DalsaVal.m_View[ID_Cam].Dispose(); DalsaVal.m_View[ID_Cam] = null; /*DLSWiew.imageBox1.View = null; */}
            if (DalsaVal.m_Buffers[ID_Cam] != null)
            { DalsaVal.m_Buffers[ID_Cam].Dispose(); DalsaVal.m_Buffers[ID_Cam] = null; }
            if (DalsaVal.m_Acquisition[ID_Cam] != null)
            { DalsaVal.m_Acquisition[ID_Cam].Dispose(); DalsaVal.m_Acquisition[ID_Cam] = null; }

            if (DalsaVal.m_FlatField[ID_Cam] != null)
            { DalsaVal.m_FlatField[ID_Cam].Dispose(); DalsaVal.m_FlatField[ID_Cam] = null; }
        }




        // Delegate to display number of frame acquired 
        // Delegate is needed because .NEt framework does not support cross thread control modification
        private delegate void RefreshControlDelegateMaster();
        private delegate void RefreshControlDelegateSlave();
        //private delegate void CheckForLastFrameDelegate();

        // Delegate to display number of frame acquired 
        // Delegate is needed because .NEt framework does not support  cross thread control modification
        //private delegate void DisplayFrameAcquired(int number, bool trash);

        private delegate void CheckForLastFrameDelegateMaster(int number, bool trash);
        private delegate void CheckForLastFrameDelegateSlave(int number, bool trash);


       void xfer_XferNotifyMast(object sender, SapXferNotifyEventArgs argsNotify){
           
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead
            if (argsNotify.Trash)
                CheckForLastFrameMast( argsNotify.EventCount, true);
            // Refresh view
            else
            {

                CheckForLastFrameMast(argsNotify.EventCount, false);
            }

        }

        static void xfer_XferNotifySlav(object sender, SapXferNotifyEventArgs argsNotify)
        {



   
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead
            if (argsNotify.Trash)
            CheckForLastFrameSlav( argsNotify.EventCount, true);
            // Refresh view
            else
            {
             CheckForLastFrameSlav( argsNotify.EventCount, false);

            }



        }


        static void AcqCallback(object sender, SapAcqNotifyEventArgs argsSignal)
        {
            // Form1 SeqGDDlg = argsSignal.Context as Form1;
            // SeqGDDlg.FrameLostCount.Text = "Frame Lost : " + argsSignal.EventCount.ToString();

        }



        static  PictureBox  LiveViewS = new PictureBox();


      public  void InstImageBox(PictureBox imgBox ) {

     //       LiveViewS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
     //| System.Windows.Forms.AnchorStyles.Right)));
     //       LiveViewS.Location = new System.Drawing.Point(0, 4);
     //       LiveViewS.Name = "LiveView";
     //       LiveViewS.Size = new System.Drawing.Size(384, 513);
     //       LiveViewS.TabIndex = 2;
     //       LiveViewS.TabStop = false;
      //      LiveViewS = imgBox;

        }

        /// <summary>
        ////
        /// </summary>


        /////////////////////


       public static long elapsedMs=0;
        static    IntPtr DataM = new IntPtr(); // image pointer
     static   public void CheckForLastFrameMast(int number, bool trash){
   
            if (trash){

            }else{

                /******************* Convert to Image ***********************************/
                
                DalsaVal.m_Buffers[Master].GetAddress(out DataM);


                Emgu.CV.Mat imOriginal = new Emgu.CV.Mat(DalsaVal.m_Buffers[Master].Height,
                                         DalsaVal.m_Buffers[Master].Width,
                                         Emgu.CV.CvEnum.DepthType. Cv8U, 1,      //Emgu.CV.CvEnum.DepthType.Cv8U  
                                         DataM,
                                         DalsaVal.m_Buffers[Master].Width );
                IProducerConsumerCollection<Image<Gray, byte>> CollecTemp = FlowCamera.BoxImgM;
                CollecTemp.TryAdd(imOriginal.ToImage<Gray, byte>());

            }
        }


               static       IntPtr DataS = new IntPtr(); // image pointer


        static public void CheckForLastFrameSlav(int number, bool trash) {


            if (trash) {
                //str = String.Format("Frames acquired in trash buffer: {0}", number);
                //FrameLostCount.Text = "EROOR";
            }
            else {





                /******************* Convert to Image ***********************************/
               
                DalsaVal.m_Buffers[Slave].GetAddress(out DataS);


                Emgu.CV.Mat imOriginal = new Emgu.CV.Mat(DalsaVal.m_Buffers[Slave].Height,
                                                  DalsaVal.m_Buffers[Slave].Width,
                                                  Emgu.CV.CvEnum.DepthType.Cv8U, 1,      //Emgu.CV.CvEnum.DepthType.Cv8U  
                                                  DataS,
                                                  DalsaVal.m_Buffers[Slave].Width);



                IProducerConsumerCollection<Image<Gray, byte>> CollecTemp = FlowCamera.BoxImgS;
                CollecTemp.TryAdd(imOriginal.ToImage<Gray, byte>());
    


            }
        }




































        /// <summary>
        /// /////////////////////////////////   FFC     //////////////////////////
        /// </summary>

        private const String DEFAULT_FFC_FILENAME = "FFC.tif";
        private const String STANDARD_FILTER = "TIFF Files (*.tif)|*.tif||";

        public void button_Load_FF_Click (int ID_Cam)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Open Flat Field Correction";
            dlg.FileName = DEFAULT_FFC_FILENAME;
            dlg.Filter = STANDARD_FILTER;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                String PathName = dlg.FileName;
                // Load flat field correction file
                if (!DalsaVal.m_FlatField[ID_Cam].Load(PathName))
                    return;
            }

            //UpdateControls();
        }

        public bool button_Load_FF_Click(string PathName)
        {

            if (PathName != "")
            { 
               string PathNameMaster= PathName+"\\"+  "CAM_" + Master + DEFAULT_FFC_FILENAME;
                string PathNameSlave = PathName + "\\" + "CAM_" + Slave + DEFAULT_FFC_FILENAME;
                // Load flat field correction file
                if (!DalsaVal.m_FlatField[Master].Load(PathNameMaster))
                if (!DalsaVal.m_FlatField[Slave].Load(PathNameSlave))
                        return true;
            }
            else { Help.Mesag("Camera Background Alignment (FFC) file not found!!!"); }
            return false;
            //UpdateControls();
        }


        public void checkBox_FaltField_Click(int ID_Cam, bool FaltField_Checked) {



            if (DalsaVal.m_FlatField[ID_Cam] != null && DalsaVal.m_FlatField[ID_Cam].Initialized){
                // To enable/disable flat field correction, the transfer object must first be disconnected from the hardware
                if (DalsaVal.m_Xfer[ID_Cam] != null && DalsaVal.m_Xfer[ID_Cam].Initialized)
                    DalsaVal.m_Xfer[ID_Cam].Destroy();

                bool success = true;

                // Check for invalid pixel format!!!!!!!!

                // Enable/disable flat field correction - useHardware
                if (success)
                    DalsaVal.m_FlatField[ID_Cam].Enable(FaltField_Checked, true);

                if (DalsaVal.m_Xfer[ID_Cam] != null && !DalsaVal.m_Xfer[ID_Cam].Initialized)
                {
                    // Recreate the transfer object to reconnect it to the hardware
                    DalsaVal.m_Xfer[ID_Cam].Create();
                    DalsaVal.m_Xfer[ID_Cam].Init(true);
           
                }

               /// UpdateControls();
            }
            
        }







       int textBox_Frame_Avg = 20;                  // Визначається кількість зображень, які використовуються для розрахунку Flat Field 
        int textBox_Line_Avg = 64;                 // Кількість ліній для усереднення
        int textBox_Max_Dev = 100;                   // Встановіть максимальне відхилення від середнього значення пікселя для темного зображення
        int textBox_Vert_Offset = 0;                // вертикальний зсув
        bool ClippedCoefsDefects_checkbox = false;   // вказує, чи слід вважати пікселі з обрізаними коефіцієнтами як дефектні.

        //
        // Step 1: Snap a Dark image to calculate the gain coefficients
        //


        public void Acq_Dark_Click(int ID_Cam) {

     




                if (ID_Cam==Master) {SetGain((double)SETS.Data.ACQGEIN1, Master); }else {
                                     SetGain((double)SETS.Data.ACQGEIN2, Slave);  }


             System.Threading.Thread.Sleep(500);


            int nbImagesUsed = DalsaVal.m_FlatField[ID_Cam].CorrectionType == SapFlatField.ScanCorrectionType.Field ? textBox_Frame_Avg : 1;

            // Set correction type
            //DalsaVal.m_FlatField[ID_Cam].CorrectionType = DalsaVal.m_CorrectionType[ID_Cam];

            // Set video type
            //DalsaVal.m_FlatField[ID_Cam].SetVideoType(DalsaVal.m_VideoType[ID_Cam], SapBayer.AlignMode.BGGR);

            // Встановіть максимальне відхилення від середнього значення пікселя для темного зображення
            DalsaVal.m_FlatField[ID_Cam].DeviationMaxBlack = textBox_Max_Dev;

            // Set number of lines to average and vertical offset
            DalsaVal.m_FlatField[ID_Cam].NumLinesAverage = textBox_Line_Avg;
            DalsaVal.m_FlatField[ID_Cam].VerticalOffset = textBox_Vert_Offset;

            // Set wether to declare pixels with clipped coefficient as defective
            DalsaVal.m_FlatField[ID_Cam].ClippedGainOffsetDefects = ClippedCoefsDefects_checkbox;

            //Multi flat-field not implemented in .NET
            // Set calibration index
            //m_FlatField->SetIndex(m_CalibrationIndex);

            /////////LogMessageBox.ResetText();
          
            if (DalsaVal.m_Xfer[ID_Cam] != null && DalsaVal.m_Xfer[ID_Cam].Initialized)
            {
                DalsaVal.m_pLocalBuffer[ID_Cam] = new SapBuffer(nbImagesUsed, DalsaVal.m_Buffers[ID_Cam], SapBuffer.MemoryType.Default);
                DalsaVal.m_pLocalBuffer[ID_Cam].Create();

                // Acquire an image
                if (!Snap(ID_Cam))
                {
                    LogMessage(LogTypes.Error, "Unable to acquire an image");
                    if (DalsaVal.m_pLocalBuffer[ID_Cam] != null)
                    {
                        DalsaVal.m_pLocalBuffer[ID_Cam].Destroy();
                        DalsaVal.m_pLocalBuffer[ID_Cam].Dispose();
                        DalsaVal.m_pLocalBuffer[ID_Cam] = null;
                    }
                    return;
                }
            }
            else
            {
                // Load an image

                DalsaVal.m_pLocalBuffer[ID_Cam] = new SapBuffer(1, DalsaVal.m_Buffers[ID_Cam], SapBuffer.MemoryType.Default);
                DalsaVal.m_pLocalBuffer[ID_Cam].Create();

                LoadSaveDlg dlg = new LoadSaveDlg(null, true, false);
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    if (DalsaVal.m_pLocalBuffer[ID_Cam] != null)
                    {
                        DalsaVal.m_pLocalBuffer[ID_Cam].Destroy();
                        DalsaVal.m_pLocalBuffer[ID_Cam].Dispose();
                        DalsaVal.m_pLocalBuffer[ID_Cam] = null;
                    }
                    return;
                }

                String path = dlg.PathName;

                // Create a temporary buffer in order to know the selected file's native format and pixel depth

                SapBuffer loadBuffer = new SapBuffer(path, SapBuffer.MemoryType.Default);
                loadBuffer.Create();

                if (loadBuffer.Format != DalsaVal.m_Buffers[ID_Cam].Format || loadBuffer.PixelDepth != DalsaVal.m_Buffers[ID_Cam].PixelDepth)
                {
                    LogMessage(LogTypes.Warning, "Image file has a different format than expected.  Pixel values may get shifted.");
                }

                if (loadBuffer.Width != DalsaVal.m_Buffers[ID_Cam].Width || loadBuffer.Height != DalsaVal.m_Buffers[ID_Cam].Height)
                {
                    LogMessage(LogTypes.Error, "Image file selected doesn't have same dimensions as buffer.");
                    if (DalsaVal.m_pLocalBuffer[ID_Cam] != null)
                    {
                        DalsaVal.m_pLocalBuffer[ID_Cam].Destroy();
                        DalsaVal.m_pLocalBuffer[ID_Cam].Dispose();
                        DalsaVal.m_pLocalBuffer[ID_Cam] = null;
                    }
                    return;
                }

                //loadBuffer.Load(path,1);
                DalsaVal.m_pLocalBuffer[ID_Cam].Copy(loadBuffer);

                String str;
                str = String.Format("Loaded dark image: {}", path.ToString());
                LogMessage(LogTypes.Info, str);
            }
            DarkImage(ID_Cam);
        }



        //
        // Step 2: Snap a bright image to calculate the gain coefficients
        //
 
        public void Acq_Bright_Click(int ID_Cam)
        {
         

            System.Threading.Thread.Sleep(100);

            if (ID_Cam == Master) { SetGain((double)SETS.Data.ACQGEIN1, Master); }
            else
            {
                SetGain((double)SETS.Data.ACQGEIN2, Slave);
            }

            int nbImagesUsed = DalsaVal.m_FlatField[ID_Cam].CorrectionType == SapFlatField.ScanCorrectionType.Field ? textBox_Frame_Avg : 1;

            // Set maximum deviation from average pixel value for bright image
            DalsaVal.m_FlatField[ID_Cam].DeviationMaxWhite = textBox_Max_Dev;

            // Set number of lines to average and vertical offset
            DalsaVal.m_FlatField[ID_Cam].NumLinesAverage = textBox_Line_Avg;
            DalsaVal.m_FlatField[ID_Cam].VerticalOffset = textBox_Vert_Offset;

            // Set wether to declare pixels with clipped coefficient as defective
            DalsaVal.m_FlatField[ID_Cam].ClippedGainOffsetDefects = ClippedCoefsDefects_checkbox;
            
            if (DalsaVal.m_Xfer[ID_Cam] != null && DalsaVal.m_Xfer[ID_Cam].Initialized)
            {
                DalsaVal.m_pLocalBuffer[ID_Cam] = new SapBuffer(nbImagesUsed, DalsaVal.m_Buffers[ID_Cam], SapBuffer.MemoryType.Default); ///<- SapBuffer m_pBuffer;
                DalsaVal.m_pLocalBuffer[ID_Cam].Create();

                // Acquire an image
                if (!Snap(ID_Cam))
                {
                    Console.WriteLine( "Unable to acquire an image");
                    if (DalsaVal.m_pLocalBuffer[ID_Cam] != null)
                    {
                        DalsaVal.m_pLocalBuffer[ID_Cam].Destroy();
                        DalsaVal.m_pLocalBuffer[ID_Cam].Dispose();
                        DalsaVal.m_pLocalBuffer[ID_Cam] = null;
                    }
                    return;
                }
            }
            else
            {
                // Load an image
                DalsaVal.m_pLocalBuffer[ID_Cam] = new SapBuffer(1, DalsaVal.m_Buffers[ID_Cam], SapBuffer.MemoryType.Default); ///<- SapBuffer m_pBuffer;
                DalsaVal.m_pLocalBuffer[ID_Cam].Create();


                LoadSaveDlg dlg = new LoadSaveDlg(null, true, false);
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    if (DalsaVal.m_pLocalBuffer[ID_Cam] != null)
                    {
                        DalsaVal.m_pLocalBuffer[ID_Cam].Destroy();
                        DalsaVal.m_pLocalBuffer[ID_Cam].Dispose();
                        DalsaVal.m_pLocalBuffer[ID_Cam] = null;
                    }
                    return;
                }

                String path = dlg.PathName;

                // Create a temporary buffer in order to know the selected file's native format and pixel depth

                SapBuffer loadBuffer = new SapBuffer(path, SapBuffer.MemoryType.Default);
                loadBuffer.Create();

                if (loadBuffer.Format != DalsaVal.m_Buffers[ID_Cam].Format || loadBuffer.PixelDepth != DalsaVal.m_Buffers[ID_Cam].PixelDepth) ///<- SapBuffer m_pBuffer;
                {
                    Console.WriteLine(  "Image file has a different format than expected.  Pixel values may get shifted.");
                }

                if (loadBuffer.Width != DalsaVal.m_Buffers[ID_Cam].Width || loadBuffer.Height != DalsaVal.m_Buffers[ID_Cam].Height)         ///<- SapBuffer m_pBuffer;
                {
                    Console.WriteLine( "Image file selected doesn't have same dimensions as buffer.");
                    if (DalsaVal.m_pLocalBuffer[ID_Cam] != null)
                    {
                        DalsaVal.m_pLocalBuffer[ID_Cam].Destroy();
                        DalsaVal.m_pLocalBuffer[ID_Cam].Dispose();
                        DalsaVal.m_pLocalBuffer[ID_Cam] = null;
                    }
                    return;
                }

                loadBuffer.Load(path, 0);
                DalsaVal.m_pLocalBuffer[ID_Cam].Copy(loadBuffer);

                String str;
                str = String.Format("Loaded bright image: '{0}'", path);
                Console.WriteLine( str);
            }

            BrightImage(ID_Cam);
        }

        //
        // Step 3: SAVE image to calculate the gain coefficients
        //

        public void Save_Acq_File(int ID_Cam, string PachSave )
        {
  

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save Flat Field Correction";
            dlg.FileName ="CAM_"+ ID_Cam.ToString()+ DEFAULT_FFC_FILENAME;
            dlg.Filter = STANDARD_FILTER;

            if (PachSave == "") {

                if (dlg.ShowDialog() == DialogResult.OK) {

                    // Save flat field offset correction file
                    DalsaVal.m_FlatField[ID_Cam].Save(dlg.FileName);
                    LogMessage(LogTypes.Info, "File saved successfully.");
                }
            } else {
                PachSave = PachSave + "\\"+ "CAM_" + ID_Cam.ToString() + DEFAULT_FFC_FILENAME;
                DalsaVal.m_FlatField[ID_Cam].Save(PachSave); }
        }

        private void DarkImage(int ID_Cam ){


            String str;
            SapFlatFieldStats stats = new SapFlatFieldStats();

            str = String.Format("Correction type: {0}", DalsaVal.m_CorrectionType[ID_Cam]);
            LogMessage(LogTypes.Info, str);

            str = String.Format("Video type: {0}", DalsaVal.m_VideoType[ID_Cam]);
            LogMessage(LogTypes.Info, str);

            if (DalsaVal.m_Xfer[ID_Cam] != null)
            {
                str = String.Format("Number of frames to average: {0}", textBox_Frame_Avg.ToString());
                LogMessage(LogTypes.Info, str);
            }

            if (DalsaVal.m_CorrectionType[ID_Cam] == SapFlatField.ScanCorrectionType.Line)
            {
                str = String.Format("Number of lines to average: {0}", textBox_Line_Avg.ToString());
                LogMessage(LogTypes.Info, str);

                str = String.Format("Vertical offset from top: {0}", textBox_Vert_Offset.ToString());
                LogMessage(LogTypes.Info, str);
            }

            LogMessage(LogTypes.Info, "Dark image calibration");

            if (!DalsaVal.m_FlatField[ID_Cam].GetStats(DalsaVal.m_pLocalBuffer[ID_Cam], stats))
            {
                LogMessage(LogTypes.Error, "   Unable to get image statistics");
                return;
            }

            bool tooManyBadPixels = false;
            int numComponents = stats.NumComponents;

            for (int i = 0; i < numComponents; i++)                                      //7,10,5,1,7388,7,/90,18555/1
            {
                if (stats.get_Average(i) > m_RecommendedDark)
                {
                    tooManyBadPixels = true;
                    break;
                }
            }

            if (tooManyBadPixels && DalsaVal.m_FlatField[ID_Cam].ClippedGainOffsetDefects)
            {                                                                                              
                //false
                str = "The following statistics have been computed on the dark image: \n";
                str += String.Format("The average pixel value is {0}\n", stats.Average.ToString());
                str += String.Format("\nThis yields too many bad pixels above the hardware limit of {0}\n", m_RecommendedDark);
                str += String.Format("\nTo disable bad pixels, uncheck the \'Consider as defective ...\'\n");
                str += String.Format("checkbox, then acquire the dark image again\n");

                MessageBox.Show(str, "", MessageBoxButtons.OK);
                return;
            }
            else
            {
                str = "The following statistics have been computed on the dark image: \n";
                str += String.Format("The average pixel value is {0}\n", stats.Average.ToString());
                str += String.Format("\nWe recommend an average pixel value of less than {0}\n", m_RecommendedDark);
                str += String.Format("\nDo you want to use this image?");

                if (MessageBox.Show(str, "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
            }

            // Log pixel statistics
            str = String.Format("    Average pixel value: {0}", stats.Average.ToString());
            LogMessage(LogTypes.Info, str);

            str = String.Format("    Maximum deviation allowed from average pixel value: {0}", textBox_Max_Dev);
            LogMessage(LogTypes.Info, str);

            // Compute offset coefficients using last acquired image
                DalsaVal.m_FlatField[ID_Cam].NumFramesAverage = DalsaVal.m_pLocalBuffer[ID_Cam].Count;
            if (DalsaVal.m_FlatField[ID_Cam].ComputeOffset(DalsaVal.m_pLocalBuffer[ID_Cam]))
            {
                //button_Acq_Dark.Enabled = false;
                //button_Acq_Bright.Enabled = true;

                //comboBox_Correction_Type.Enabled = false;
                //comboBox_Video_Type.Enabled = false;
                //comboBox_Calibration_Index.Enabled = false;

                LogMessage(LogTypes.Info, "Calibration with a dark image has been done successfully");
               /// textBox_Max_Dev.Text = m_pFlatField.DeviationMaxWhite.ToString();

            }
        }

        private void BrightImage(int ID_Cam )
        {
  
            String str;
            SapFlatFieldStats stats = new SapFlatFieldStats();

            if (DalsaVal.m_Xfer[ID_Cam] != null)
            {
                str = String.Format("Number of frames to average: {0}", textBox_Frame_Avg);
                LogMessage(LogTypes.Info, str);
            }

        
                str = String.Format("Number of lines to average: {0}", textBox_Line_Avg);
                LogMessage(LogTypes.Info, str);

                str = String.Format("Vertical offset from top: {0}", textBox_Vert_Offset);
                LogMessage(LogTypes.Info, str);
            

            LogMessage(LogTypes.Info, "Bright image calibration");

            // Get statistics on the (bright - dark) image
            if (!DalsaVal.m_FlatField[ID_Cam].GetStats(DalsaVal.m_pLocalBuffer[ID_Cam], stats))
            {
                LogMessage(LogTypes.Error, "Unable to get image statistic");
                return;
            }

            str = "The following statistics have been computed on the bright image\n";
            str += "after the substraction of the dark image:\n\n";
            str += String.Format("    The average pixel value is {0}levels.\n", GetAverageStr(stats));
           // str += String.Format("    The highest peak has been detected at {0}.\n", GetPeakPositionStr(stats));
           // str += String.Format("    {0} pixels {1} have a luminance value between {2} to {3}\n", GetNumPixelsStr(stats), GetPixelRatioStr(stats), GetLowStr(stats), GetHighStr(stats));
            str += String.Format("\nWe recommend at least {0} levels for the highest peak value\n", m_RecommendedBright);
           // str += String.Format("with {0}% pixels lying between the lower and the higher bound.\n", SapDefFlatFieldPixelRatio);
            str += "\nDo you want to use this image?";

            if (MessageBox.Show(str, "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            // Log average pixel value, lower and higher bounds and pixel ratio
            str = String.Format("    Average pixel value: {0}", GetAverageStr(stats));
            LogMessage(LogTypes.Info, str);
            str = String.Format("    Maximum deviation allowed from average pixel value: {0}", textBox_Max_Dev);
            LogMessage(LogTypes.Info, str);
            //str = String.Format("    Highest peak position: {0}", GetPeakPositionStr(stats));
            LogMessage(LogTypes.Info, str);
            //str = String.Format("    Lower bound: {0}", GetLowStr(stats));
            LogMessage(LogTypes.Info, str);
            //str = String.Format("    Upper bound: {0}", GetHighStr(stats));
            //LogMessage(LogTypes.Info, str);
            //str = String.Format("    Number of pixels inside bounds: {0} ({1})", GetNumPixelsStr(stats), GetPixelRatioStr(stats));
            LogMessage(LogTypes.Info, str);

            SapFlatFieldDefects defects = new SapFlatFieldDefects();

            // Compute gain coefficient using last acquired image

            DalsaVal.m_FlatField[ID_Cam].NumFramesAverage = DalsaVal.m_pLocalBuffer[ID_Cam].Count;
            if (DalsaVal.m_FlatField[ID_Cam].ComputeGain(DalsaVal.m_pLocalBuffer[ID_Cam], defects, true))
            {
                // Check for the presence of cluster (adjacent defective pixels)
                if (defects.NumClusters != 0)
                {
                    str = String.Format("{0} pixels ({1} %) have been identified as being defective\n", defects.NumDefects, defects.DefectRatio);
                    str += String.Format("with {0} clusters.\n", defects.NumClusters);
                    //str += String.Format("\nWe recommend less than {0}% of defective pixels with no cluster.\n", SapDefFlatFieldDefectRatio);
                    str += String.Format("\nDo you still want to use this image?\n");

                    if (MessageBox.Show(str, "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                        return;
                }

                // Log number of defective pixels detected
                str = String.Format("    Number of defective pixels detected: {0} ({1})", defects.NumDefects, defects.DefectRatio);
                LogMessage(LogTypes.Info, str);

                // Log number of cluster detected
                str = String.Format("    Number of clusters detected: {0}", defects.NumClusters);
                LogMessage(LogTypes.Info, str);

                //button_Acq_Dark.Enabled = true;
                //button_Acq_Bright.Enabled = false;
                //button_OK.Enabled = true;
                bool isOnline = (DalsaVal.m_Xfer[ID_Cam] != null && DalsaVal.m_Xfer[ID_Cam].Initialized);
                //comboBox_Correction_Type.Enabled = !isOnline && m_VideoType == SapAcquisition.VideoType.Mono;
                //comboBox_Video_Type.Enabled = m_pXfer == null;
                //textBox_Frame_Avg.Enabled = m_pXfer != null;
                //textBox_Line_Avg.Enabled = m_CorrectionType == SapFlatField.ScanCorrectionType.Line;
                //textBox_Vert_Offset.Enabled = m_CorrectionType == SapFlatField.ScanCorrectionType.Line;
                //textBox_Max_Dev.Enabled = true;
                //button_Save_and_Upload.Enabled = true;
                //comboBox_FlatField_Selector.Enabled = true;

                //Multi flat-field not implemented in .NET
                //m_CalibrationIndexCtrl.EnableWindow( m_pFlatField->GetNumFlatField() > 1);

                LogMessage(LogTypes.Info, "Calibration with a bright image has been done successfully");

                //textBox_Max_Dev.Text = m_pFlatField.DeviationMaxBlack.ToString();
            }
        }


        String GetAverageStr(SapFlatFieldStats stats)
        {
            String str = "";

            if (stats.NumComponents > 1)
            {
                str += "[ ";
                for (int iComponent = 0; iComponent < stats.NumComponents; iComponent++)
                {
                    String szComponent;
                    szComponent = String.Format("{0}", stats.get_Average(iComponent));
                    str += szComponent;

                    if (iComponent != stats.NumComponents - 1)
                        str += ", ";
                }
                str += " ]";
            }
            else
            {
                str = String.Format("{0}", stats.Average);
            }

            return str;
        }

        public bool Snap(int ID_Cam)
        {
            // Check if the transfer object is available
            if (DalsaVal.m_Xfer[ID_Cam] == null || !DalsaVal.m_Xfer[ID_Cam].Initialized)
                return false;

            for (int iFrame = 0; iFrame < DalsaVal.m_pLocalBuffer[ID_Cam].Count; iFrame++)
            {
                // Acquire one image
                DalsaVal.m_Xfer[ID_Cam].Snap();

                // Wait until the acquired image has been transferred into system memory
                AbortDlg abort = new AbortDlg(DalsaVal.m_Xfer[ID_Cam]);
                if (abort.ShowDialog() != DialogResult.OK)
                {
                    DalsaVal.m_Xfer[ID_Cam].Abort();
                    return false;
                }

                //Add a short delay to ensure the transfer callback has time to arrive
                System.Threading.Thread.Sleep(200);

                if (DalsaVal.m_pLocalBuffer[ID_Cam] != null)
                {
           
                    DalsaVal.m_pLocalBuffer[ID_Cam].Copy(DalsaVal.m_Buffers[ID_Cam]);  ///<- SapBuffer m_pBuffer;
                }
            }
            return true;
        }
        int m_RecommendedDark=64;
        int m_RecommendedBright;
        enum LogTypes
        {
            Error,
            Warning,
            Info
        };


        private void LogMessage(LogTypes messageType, String str)
        {
            String message = "";

            // Message header
            switch (messageType)
            {
                case LogTypes.Error:
                    message = "[Err] ";
                    break;
                case LogTypes.Warning:
                    message = "[Wrn] ";
                    break;
                case LogTypes.Info:
                    message = "[Msg] ";
                    break;
            }

            message += str;
            //LogMessageBox.BeginUpdate();
            //LogMessageBox.Items.Add(message);
            //LogMessageBox.EndUpdate();
            Console.WriteLine(message);

        }


        //DLS EXIT
    }
























    public class DalsaVal
    {


         public SapFlatField  [] m_FlatField = new SapFlatField [2];
        //public SapTransfer [] m_pXfer = new SapTransfer[2];    ///
       public SapBuffer [] m_pLocalBuffer = new SapBuffer[2] ; /// <summary>



      //  SapBuffer m_pBuffer;
        /// </summary>


        static  public  SapAcqDevice [] device = new SapAcqDevice[2];

        static public SapAcquisition[] m_Acquisition = new SapAcquisition[2];

        public SapBuffer[] m_Buffers = new SapBuffer[2];
        public SapAcqToBuf[] m_Xfer = new SapAcqToBuf[2];
        public SapView[] m_View = new SapView[2];
        public bool[] m_IsSignalDetected = new bool[2];
        public bool[] m_online = new bool[2];
        public SapFlatField.ScanCorrectionType[] m_CorrectionType = new SapFlatField.ScanCorrectionType[2];
        public SapAcquisition.VideoType [] m_VideoType = new SapAcquisition.VideoType[2];


        static public SapLocation[] m_ServerLocation = new SapLocation[2];

        public string[] m_ConfigFileName = new string[2];
        public bool[] m_bRecordOn = new bool[2];
        public bool[] m_bPlayOn = new bool[2];
        public bool[] m_bPauseOn = new bool[2];
        public int[] m_nFramesPerCallback = new int[2];
        public int[] m_nFramesOnBoard = new int[2];
        public float[] m_MinTime = new float[2];
        public float[] m_MaxTime = new float[2];

        //System menu
        // public SystemMenu m_SystemMenu;
        //index for "about this.." item im system menu
        public int[] m_AboutID = new int[2];
        public int[] Default_Nbr_buffers = new int[2];



        //public int[] m_AboutID = new int[2] ={ 100; 100 };  // 0x100;
        //static public int[] Default_Nbr_buffers = 15;

    };

}