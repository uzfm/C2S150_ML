﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

using VIBR_TABLE = C2S150_ML.USB_HID.PLC_C2S150.VIBRATING;

using AUTOLOADER = C2S150_ML.USB_HID.PLC_C2S150.AUTOLOADER;
using SEPARATOR = C2S150_ML.USB_HID.PLC_C2S150.SEPARATOR;
using System.Runtime.InteropServices;

namespace C2S150_ML
{
    class Flow
    {

        public enum DialogResult
        {
            //
            // Summary:
            //     Nothing is returned from the dialog box. This means that the modal dialog continues
            //     running.
            None = 0,
            //
            // Summary:
            //     The dialog box return value is OK (usually sent from a button labeled OK).
            OK = 1,
            //
            // Summary:
            //     The dialog box return value is Cancel (usually sent from a button labeled Cancel).
            Cancel = 2,
            //
            // Summary:
            //     The dialog box return value is Abort (usually sent from a button labeled Abort).
            Abort = 3,
            //
            // Summary:
            //     The dialog box return value is Retry (usually sent from a button labeled Retry).
            Retry = 4,
            //
            // Summary:
            //     The dialog box return value is Ignore (usually sent from a button labeled Ignore).
            Ignore = 5,
            //
            // Summary:
            //     The dialog box return value is Yes (usually sent from a button labeled Yes).
            Yes = 6,
            //
            // Summary:
            //     The dialog box return value is No (usually sent from a button labeled No).
            No = 7
        }


        const int Master = 0;
        const int Slave = 1;

       // public static bool   PotocStartSorting;
        public static bool   PotocStartUSB;

        static Thread PotocCameraM;
        static Thread PotocCameraS;
        static Thread PotocPredict;
        static Thread PotocHID;
        static Thread PotocSorting;
        static Thread PotocUSB;









        private void MyThreadFunction(object obj){
            Image<Gray, byte> image = (Image<Gray, byte>)obj;
            // ваш код обробки зображення
            FlowAnalis flowAnalis = new FlowAnalis();
            //flowAnalis.FindBlobs(image);

        }

        public void PredictFlow(Image<Gray, byte> image) {

            Thread thread = new Thread(new ParameterizedThreadStart(MyThreadFunction));
            thread.Start(image);
        }


        public static Int16 CountProcesingCamera = 0;



        static public void AnalisBlobs(){

            ANLImg_M analisImg_M = new ANLImg_M();
            PotocCameraM = new Thread(analisImg_M.AnalisBlobs);
            PotocCameraM.Priority = ThreadPriority.Lowest;
            PotocCameraM.Name = "AnalisBlobsM";
            ANLImg_M.PotocStartAnalisBlobs = true;


            ANLImg_S analisImg_S = new ANLImg_S();
            PotocCameraS = new Thread(analisImg_S.AnalisBlobs);
            PotocCameraS.Priority = ThreadPriority.Lowest;
            PotocCameraS.Name = "AnalisBlobsS";
            ANLImg_S.PotocStartAnalisBlobs = true;

            // запускаем поток
            PotocCameraM.Start();
            PotocCameraS.Start();
           // CountProcesingCamera++;
        }



        static  public void BlobsPredict(){
            AnalisPredict analisPredict = new AnalisPredict();
            PotocPredict = new Thread(analisPredict.Predict);
            PotocPredict.Priority = ThreadPriority.Lowest;
            PotocPredict.Name = "Predict" + CountProcesingCamera.ToString();
            AnalisPredict.PotocStartPredict = true;
            // запускаем поток
            PotocPredict.Start();
            CountProcesingCamera++;
        }





        static public void StartSorting(bool start_stop){
    
                 START_Flow = start_stop;
                STARTsorting = start_stop;
            PotocSorting = new Thread(Potoc_Sorting);
            PotocSorting.Priority = ThreadPriority.Lowest;
            PotocSorting.Name = "ShowSorting";
            // запускаем поток
            PotocSorting.Start();
           
        }


        //HID_USB READ
        USB_HID USB = new USB_HID();
         public void USB_Hid()
        {

            if ((PotocUSB == null) || (PotocHID.ThreadState == System.Threading.ThreadState.Stopped))
            {
                PotocUSB = new Thread(USB_HID. HID_Read);

                PotocUSB.Priority = ThreadPriority.Lowest;
                PotocUSB.Name = "USB";
                // запускаем поток
                PotocUSB.Start();
                PotocStartUSB = true;
            }
        }







        static public void StopPotoc()
        {
            ANLImg_M.PotocStartAnalisBlobs = false;
            ANLImg_S.PotocStartAnalisBlobs = false;
            AnalisPredict.PotocStartPredict = false;
            PotocStartUSB = false;
        }



   



        
        static FlowCamera FlowCamera = new FlowCamera();
        static FlowAnalis FlowAnalis = new FlowAnalis();
        static FlowAnalis FlowShowImage = new FlowAnalis();
        static FlowAnalis FlowSorting = new FlowAnalis();



        static bool LivW=false;

      public  static void LiweVive(bool Liv)
        {
             LivW = Liv;

                if (LivW)
                {

                    if (!SETS.Data.CameraAnalis_1) { DLS.StartCAMERA(Master); }
                    if (!SETS.Data.CameraAnalis_2) { DLS.StartCAMERA(Slave); }

                }
                else
                {

                    DLS.StopCAMERA(Master);
                    DLS.StopCAMERA(Slave);

                }
            

            }
        




        public static bool STARTsorting;
              static bool START_Flow =false;
        static void Potoc_Sorting() {
 
       

                if (STARTsorting)
                {
                   
                    //HID.OutputHRD_Res(30);
                    //HID.OutputHRD_Res(32);
                    //HID.OutputHRD_Set(29);  // "Звуковий сигна ЗПУСКУ";
                    //HID.OutputHRD_Set(31);  // "ЖОВТИЙ ПОПЕРЕДЖУВАЛЬНИЙ ДО СТАРТУ ЗУПИНКИ";
                    //Thread.Sleep(1000);
                    //HID.OutputHRD_Res(29);  // "Звуковий сигна ЗПУСКУ";
                    //
                    //HID.OutputHRD_Set(18);  // "ON LIGHT";
                    //HID.HID_Send_Comand(HID.LIGHT, 500);  // "ON LIGHT TEST";
                    //
                    //HID.OutputHRD_Set(20);  // ON Cooling

                    //HID.OutputHRD_Set(19);  // ON Ionizer
                    ///Thread.Sleep(200);
                    //Start Cameras


                    USB_HID.PLC_C2S150.LIGHT.YELLO_ERROR(true);
                    USB_HID.PLC_C2S150.LIGHT.RED_ERROR(false);

                    if (!SETS.Data.CameraAnalis_1) {  DLS.StartCAMERA(Master); }
                    if (!SETS.Data.CameraAnalis_2) {  DLS.StartCAMERA(Slave); }
                    USB_HID.PLC_C2S150.LIGHT.SOUND_ERRO(true);
                    //if (SV.DT.AnalCamer2) { DLS.DalsaVal.m_Xfer[Slave].Grab(); }
                    Thread.Sleep(100);
                    //HID.OutputHRD_Set(21); // "ON Conveyor";
                    //Thread.Sleep(2000);
                    //HID.OutputHRD_Set(12);  // ON Sensor Level
                    //HID.OutputHRD_Set(23);  // ON Screw feeder
                    //Thread.Sleep(4000);
                    //HID.HID_Send_Comand(HID.REG_5, Convert.ToUInt16(SV.SG.PWM_Table));
                    //HID.HID_Send_Comand(HID.REG_17, Convert.ToUInt16(SV.SG.Hz_Table));
                    //HID.OutputHRD_Res(31);  
                    //HID.OutputHRD_Set(30);  // "Зелений ПРАЦЮЄ ";

                    Thread.Sleep(2000);
                    VIBR_TABLE.SET(VIBR_TABLE.Typ.ON);
                    USB_HID.PLC_C2S150.LIGHT.SOUND_ERRO(false);
                    USB_HID.PLC_C2S150.LIGHT.YELLO_ERROR(false);
                    USB_HID.PLC_C2S150.LIGHT.GREEN_ERRO(true);
                    if (!STARTsorting) { USB_HID.PLC_C2S150.LIGHT.GREEN_ERRO(false); VIBR_TABLE.SET(VIBR_TABLE.Typ.OFF); }
              
      
                }
                else
                {
                    //HID.OutputHRD_Res(30);
                    //HID.OutputHRD_Res(32);
                    //HID.OutputHRD_Set(31);             // "ЖОВТИЙ ПОПЕРЕДЖУВАЛЬНИЙ ДО СТАРТУ ЗУПИНКИ";
                    //HID.OutputHRD_Res(12);             // OFF Sensor Level

                    USB_HID.PLC_C2S150.LIGHT.YELLO_ERROR(true);
                    SEPARATOR.OFF();  // Metal separator
                    AUTOLOADER.OFF();  // Autoloder

                    //HID.HID_Send_Comand(HID.REG_5, 0); //Start Table
                    //HID.OutputHRD_Res(24);  // Metal separator


                    VIBR_TABLE.SET(VIBR_TABLE.Typ.OFF);
                    DLS.StopCAMERA(Master);
                    DLS.StopCAMERA(Slave);
                    Thread.Sleep(2000);
                    VIBR_TABLE.SET(VIBR_TABLE.Typ.OFF);
                    USB_HID.PLC_C2S150.LIGHT.YELLO_ERROR(false);
                    USB_HID.PLC_C2S150.LIGHT.GREEN_ERRO(false);
                    USB_HID.PLC_C2S150.LIGHT.RED_ERROR(true);


                    //}
                    //    //Thread.Sleep(500);
                    //    //HID.OutputHRD_Res(21);  // "OFF Conveyor";
                    //    //Thread.Sleep(200);
                    //    //HID.OutputHRD_Res(19);  //  OFF Ionizer
                    //    //HID.OutputHRD_Res(20);  //  OFF Cooling
                    //    //HID.HID_Send_Comand(HID.LIGHT, 0);  // "OFF LIGHT TEST";
                    //    //HID.OutputHRD_Res(18);  // "OFF LIGHT";
                    //    //HID.OutputHRD_Res(31);
                    //    //HID.OutputHRD_Set(32);  // "Червоний зупинено або аварія ";
                    //}

                
            }
        }













 static Stopwatch watch ;
       
      static  public void TimeWriteConsole(bool SartReset)
        { 

            if (SartReset){
                watch = Stopwatch.StartNew();
     

            }
            else {

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("First Prediction took: " + elapsedMs + " ms");

            }
        }



        static System.Diagnostics.Process _process = null;


        static public void ProcessLerningFunction(string DataPath, string ModelName)
        {
            try{

                System.Diagnostics.ProcessStartInfo startInfo = new ProcessStartInfo();
                _process = null;

                startInfo = new System.Diagnostics.ProcessStartInfo(@"../../../../MachineLearning\bin\Debug\net5.0-windows\TenserflowKeras.exe");

                startInfo.ArgumentList.Add(ModelName);
                startInfo.ArgumentList.Add(DataPath);

                //startInfo.Arguments = DataPath;
                _process = System.Diagnostics.Process.Start(startInfo);

            }
            catch { }
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
   
        static public void ProcessLoadImagesFunction(bool Run)
        {

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(
                @"../../../../LoadImage\WindowsFormsApp1\bin\Debug\LoadImage.exe");
            if (Run) { _process = System.Diagnostics.Process.Start(startInfo); }
            else
            {
                _process.Kill();
                _process.Close();
            }


        }

        static public void ProcessLoadImagesFunction()
        {   // Зачекайте, щоб впевнитися, що вікно вже створене
            _process.WaitForInputIdle();

            // Зробіть вікно попереду усіх інших вікон
            SetForegroundWindow(_process.MainWindowHandle);
        }







        }
}
