﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DALSA.SaperaLT.SapClassGui;
using Emgu.CV;
using Emgu.CV.Structure;

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
        static bool   PotocStartHID;
 public static bool   PotocStartSorting;

        static Thread PotocCameraM;
        static Thread PotocCameraS;
        static Thread PotocPredict;
        static Thread PotocHID;
        static Thread PotocSorting;

        




        static public void StartPotocHID() {
        // if( (PotocHID==null) ||  (PotocHID.ThreadState==System.Threading.ThreadState.Stopped   )) {
        //          // PotocHID = new Thread(PotocHIDFunction,1000000000);
        //          PotocHID = new Thread(PotocHIDFunction);
        //          PotocHID.Priority = ThreadPriority.Highest;
        //          PotocHID.Name = "HID";
        //        PotocStartHID = true;
        //        // запускаем поток
        //        PotocHID.Start();
        //}
        }




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
            StartPotocHID();
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
            StartPotocHID();
            CountProcesingCamera++;
        }





        static public void StartSorting(){
            PotocSorting = new Thread(Potoc_Sorting);
            PotocSorting.Priority = ThreadPriority.Lowest;
            PotocSorting.Name = "ShowSorting";
            PotocStartSorting = true;
            // запускаем поток
            PotocSorting.Start();
        }



        static public void StopPotocAnalisBlobs()  { ANLImg_M.PotocStartAnalisBlobs = false; ANLImg_S.PotocStartAnalisBlobs = false; }
        static public void StopPotocBlobsPredict() { AnalisPredict.PotocStartPredict = false; }

        static public void StopPotocHID() { PotocStartHID = false; }


   



        
        static FlowCamera FlowCamera = new FlowCamera();
        static FlowAnalis FlowAnalis = new FlowAnalis();
        static FlowAnalis FlowShowImage = new FlowAnalis();
        static FlowAnalis FlowSorting = new FlowAnalis();

        static void PotocHIDFunction(){
            while (PotocStartHID)
            {
                //HID.HID_Set();
            }
        }







      public static bool STARTsorting;
        static void Potoc_Sorting()
        {
            while (PotocStartSorting)
            {

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
                    //HID.OutputHRD_Set(24);  // Metal separator
                    //HID.OutputHRD_Set(19);  // ON Ionizer
                    ///Thread.Sleep(200);
                    //Start Cameras
                    DLS.StartStatus = true;
                    DLS. DalsaVal.m_Xfer[Master].Grab();
                    DLS. DalsaVal.m_Xfer[Slave].Grab();
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


                    PotocStartSorting = false;
                }
                else
                {
                    //HID.OutputHRD_Res(30);
                    //HID.OutputHRD_Res(32);
                    //HID.OutputHRD_Set(31);             // "ЖОВТИЙ ПОПЕРЕДЖУВАЛЬНИЙ ДО СТАРТУ ЗУПИНКИ";
                    //HID.OutputHRD_Res(12);             // OFF Sensor Level
                    //HID.OutputHRD_Res(23);             // OFF Scru feeder
                    //HID.HID_Send_Comand(HID.REG_5, 0); //Start Table
                    //HID.OutputHRD_Res(24);  // Metal separator
                    //Thread.Sleep(7000);

                    AbortDlg abortM = new AbortDlg(DLS.DalsaVal.m_Xfer[Master]);
                    if (DLS.DalsaVal.m_Xfer[Master].Freeze()) { DLS.DalsaVal.m_Xfer[Master].Abort(); }
                    //    if (SV.DT.AnalCamer2){
                    AbortDlg abortS = new AbortDlg(DLS.DalsaVal.m_Xfer[Slave]);
                       if (DLS.DalsaVal.m_Xfer[Slave].Freeze()) { DLS.DalsaVal.m_Xfer[Slave].Abort(); }
                //}
                 
                      DLS.StartStatus = false;
                    //    //Thread.Sleep(500);
                    //    //HID.OutputHRD_Res(21);  // "OFF Conveyor";
                    //    //Thread.Sleep(200);
                    //    //HID.OutputHRD_Res(19);  //  OFF Ionizer
                    //    //HID.OutputHRD_Res(20);  //  OFF Cooling
                    //    //HID.HID_Send_Comand(HID.LIGHT, 0);  // "OFF LIGHT TEST";
                    //    //HID.OutputHRD_Res(18);  // "OFF LIGHT";
                    //    //HID.OutputHRD_Res(31);
                    //    //HID.OutputHRD_Set(32);  // "Червоний зупинено або аварія ";
                        PotocStartSorting = false;
                    //}

                }
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
        //static public void ProcessLerningFunction(string DataPath,int ID) {
        //    System.Diagnostics.ProcessStartInfo startInfo = new ProcessStartInfo();
        //     _process = null;
        //    if (ID==1) {
        //        startInfo = new System.Diagnostics.ProcessStartInfo(
        //         @"../../MLNET_GPU_Learn\MLNET_NEW_GPU_Bitmap\bin\Debug\MLfast\MLNET_GPU_Bitmap.exe");
        //    }
        //    if (ID == 0)
        //    {
        //        startInfo = new System.Diagnostics.ProcessStartInfo(
        //         @"../../MLNET_GPU_Learn\MLNET_NEW_GPU_Bitmap\bin\Debug\MLslow\MLNET_GPU_Bitmap.exe");
        //    }

        //    startInfo.Arguments = DataPath;
        //    startInfo.UseShellExecute = false;
        //   _process = System.Diagnostics.Process.Start(startInfo);

        //    while (!_process.WaitForExit(500)) ;
        //}
      
        static public void ProcessLoadImagesFunction(bool Run){
  
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(
                @"../../LoadImage\WindowsFormsApp1\bin\Debug\LoadImage.exe");
            if (Run) { _process = System.Diagnostics.Process.Start(startInfo); } else{
                _process.Kill();
                _process.Close();
            }
        }










    }
}