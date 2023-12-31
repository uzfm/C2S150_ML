﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using HidLibrary;
using HidSharp;





namespace C2S150_ML
{
    public class USB_HID
    {


        #region CODE

        //Standart
        ushort DEVICE_VID = 1155;
        ushort DEVICE_PID = 22352;

        //Devices
        ushort V1_PID = 22351;
        ushort C1_PID = 22352;
        ushort CMS_PID = 22353;
        ushort GA_PID = 22354;
        ushort GA_V2 = 22355;

        const byte PAGE = 64;
        byte REPORT_ID_READ = 1;
        byte REPORT_ID_WRITE = 2;

        static byte[] Buffer_USB_RX = new byte[PAGE];
        static byte[] Buffer_USB_TX = new byte[PAGE];

        static Byte OUTPUT0_BIT = 0;
        static Byte OUTPUT1_BIT = 0;
        static Byte OUTPUT2_BIT = 0;
        static Byte OUTPUT3_BIT = 0;


        const byte BIT0_RES = 0xFE;
        const byte BIT1_RES = 0xFD;
        const byte BIT2_RES = 0xFB;
        const byte BIT3_RES = 0xF7;
        const byte BIT4_RES = 0xEF;
        const byte BIT5_RES = 0xDF;
        const byte BIT6_RES = 0xBF;
        const byte BIT7_RES = 0x7F;

        const byte BIT0_SET = 0x01;
        const byte BIT1_SET = 0x02;
        const byte BIT2_SET = 0x04;
        const byte BIT3_SET = 0x08;
        const byte BIT4_SET = 0x10;
        const byte BIT5_SET = 0x20;
        const byte BIT6_SET = 0x40;
        const byte BIT7_SET = 0x80;

        public struct BIT
        {
            public const byte I0 = 0x01;
            public const byte I1 = 0x02;
            public const byte I2 = 0x04;
            public const byte I3 = 0x08;
            public const byte I4 = 0x10;
            public const byte I5 = 0x20;
            public const byte I6 = 0x40;
            public const byte I7 = 0x80;
        }

        const Int16 REG_1 = 1;
        const Int16 REG_2 = 2;
        const Int16 REG_3 = 3; //PWM Channels-1
        const Int16 REG_4 = 4; //PWM Channels-1
        const Int16 REG_5 = 5; //PWM Channels-2
        const Int16 REG_6 = 6; //PWM Channels-2
        const Int16 REG_7 = 7; //PWM Channels-3
        const Int16 REG_8 = 8; //PWM Channels-3
        const Int16 REG_9 = 9; //PWM Channels-4
        const Int16 REG_10 = 10; //PWM Channels-4
        const Int16 REG_11 = 11; //PWM Channels-5
        const Int16 REG_12 = 12; //PWM Channels-5
        const Int16 REG_13 = 13; //PWM Channels-6
        const Int16 REG_14 = 14; //PWM Channels-6
        const Int16 REG_15 = 15; //PWM Channels-7
        const Int16 REG_16 = 16; //
        const Int16 REG_17 = 17; //

        const Int16 REG_30 = 30;

        const Int16 REG_31 = 31;
        const Int16 REG_32 = 32; // LED6
        const Int16 REG_33 = 33; // LED5
        const Int16 REG_34 = 34; // LED4
        const Int16 REG_35 = 35; // LED1
        const Int16 REG_36 = 36; // LED2
        const Int16 REG_37 = 37; // LED3
        const Int16 REG_38 = 38;
        const Int16 REG_39 = 39; //IMPUT1
        const Int16 REG_40 = 40; //IMPUT2
        const Int16 REG_41 = 41; //IMPUT3
        const Int16 REG_42 = 42; //IMPUT4
        const Int16 REG_43 = 43; //IMPUT5
        const Int16 REG_44 = 44; //IMPUT6
        const Int16 REG_45 = 45; //Frequency PWM Channels-1
        const Int16 REG_46 = 46; //Frequency PWM Channels-1
        const Int16 REG_47 = 47; //Frequency PWM Channels-4
        const Int16 REG_48 = 48; //Frequency PWM Channels-4


        ///  RSS 485 Motor.....  //// iNTERFICE fO send COMAND   //////////////////////
        const Int16 REG_49 = 49; //Довжина даних

        //Motor
        const Int16 REG_50 = 50; //
        const Int16 REG_51 = 51;
        const Int16 REG_52 = 55;
        const Int16 REG_53 = 57;
        const Int16 REG_54 = 59;
        //---------------------//
        //const Int16 REG_55 = 55;
        // const Int16 REG_56 = 56;
        // const Int16 REG_57 = 57;
        // const Int16 REG_58 = 58;
        const Int16 REG_59 = 59;
        const Int16 REG_60 = 60;
        const Int16 REG_61 = 61;
        const Int16 REG_62 = 62;
        const Int16 REG_63 = 63;
        const Int16 REG_64 = 64;

        const UInt16 ON = 0xFFFF;
        const UInt16 OFF = 0;

        #endregion CODE



        public static DATA_Save Data = new DATA_Save();
        [Serializable()]
        public class DATA_Save
        {
            //для виривнюваня фону
            public decimal PWM_Table      { get; set; }
            public decimal Hz_Table       { get; set; }
            public decimal Fleps_Time_OFF { get; set; }


            // LIGHT LOCK   -- TOP SPOT BEAK IR---
            public bool Light_Top  { get; set; }
            public bool Light_Bottom { get; set; }
            public bool Light_Back { get; set; }
            public bool Light_IR   { get; set; }

        }






        public class PLC_C2S150
        {

            public class FLAPS
            {
                /// <summary>
                /// Types of on
                /// </summary>
                public enum Typ { Fps1, Fps2, Fps3, Fps4, Fps5, Fps6, Fps7, Fps8, Fps9, Fps10, Fps11, Fps12, Fps13, Fps14, Fps15, Fps16, Fps17 }

                static Button Flaps1 = new Button();
                static Button Flaps2 = new Button();
                static Button Flaps3 = new Button();
                static Button Flaps4 = new Button();
                static Button Flaps5 = new Button();
                static Button Flaps6 = new Button();
                static Button Flaps7 = new Button();
                static Button Flaps8 = new Button();
                static Button Flaps9 = new Button();
                static Button Flaps10 = new Button();
                static Button Flaps11 = new Button();
                static Button Flaps12 = new Button();
                static Button Flaps13 = new Button();
                static Button Flaps14 = new Button();
                static Button Flaps15 = new Button();
                static Button Flaps16 = new Button();
                static Button Flaps17 = new Button();

                static System.Timers.Timer TimerFlepsColour = new System.Timers.Timer();


                static public void FlepsLightInstal(
            Button Flaps_1, Button Flaps_2, Button Flaps_3, Button Flaps_4,
            Button Flaps_5, Button Flaps_6, Button Flaps_7, Button Flaps_8,
            Button Flaps_9, Button Flaps_10, Button Flaps_11, Button Flaps_12,
            Button Flaps_13, Button Flaps_14, Button Flaps_15, Button Flaps_16, Button Flaps_17
            )
                {
                    Flaps1 = Flaps_1;
                    Flaps2 = Flaps_2;
                    Flaps3 = Flaps_3;
                    Flaps4 = Flaps_4;
                    Flaps5 = Flaps_5;
                    Flaps6 = Flaps_6;
                    Flaps7 = Flaps_7;
                    Flaps8 = Flaps_8;
                    Flaps9 = Flaps_9;
                    Flaps10 = Flaps_10;
                    Flaps11 = Flaps_11;
                    Flaps12 = Flaps_12;
                    Flaps13 = Flaps_13;
                    Flaps14 = Flaps_14;
                    Flaps15 = Flaps_15;
                    Flaps16 = Flaps_16;
                    Flaps17 = Flaps_17;

                    // Встановлення інтервалу в мілісекундах (100 мілісекунд)
                    TimerFlepsColour.Interval = 100;
                    // Додавання обробника події Tick
                    TimerFlepsColour.Elapsed += TimerFlepsColourTick;
                    // Активація таймера
                    TimerFlepsColour.Enabled = true;
                    TimerFlepsColour.Start();

                }



                // Обробник події, який буде викликатися при кожному таймауті
                static private void TimerFlepsColourTick(object sender, EventArgs e)
                {
                    FlepsLightRES();

                }


                static public void FlepsLightSET(Typ FlapsSet)
                {
                    switch (FlapsSet)
                    {
                        case Typ.Fps1: Flaps1.BackColor = Color.Salmon; break;
                        case Typ.Fps2: Flaps2.BackColor = Color.Salmon; break;
                        case Typ.Fps3: Flaps3.BackColor = Color.Salmon; break;
                        case Typ.Fps4: Flaps4.BackColor = Color.Salmon; break;
                        case Typ.Fps5: Flaps5.BackColor = Color.Salmon; break;
                        case Typ.Fps6: Flaps6.BackColor = Color.Salmon; break;
                        case Typ.Fps7: Flaps7.BackColor = Color.Salmon; break;
                        case Typ.Fps8: Flaps8.BackColor = Color.Salmon; break;
                        case Typ.Fps9: Flaps9.BackColor = Color.Salmon; break;
                        case Typ.Fps10: Flaps10.BackColor = Color.Salmon; break;
                        case Typ.Fps11: Flaps11.BackColor = Color.Salmon; break;
                        case Typ.Fps12: Flaps12.BackColor = Color.Salmon; break;
                        case Typ.Fps13: Flaps13.BackColor = Color.Salmon; break;
                        case Typ.Fps14: Flaps14.BackColor = Color.Salmon; break;
                        case Typ.Fps15: Flaps15.BackColor = Color.Salmon; break;
                        case Typ.Fps16: Flaps16.BackColor = Color.Salmon; break;
                        case Typ.Fps17: Flaps17.BackColor = Color.Salmon; break;

                    }
                }

                static public void FlepsLightRES()
                {

                    if ((SetFlapsColourDelay) && (SetFlapsColour))
                    {

                        Flaps1.BackColor = Color.ForestGreen;
                        Flaps2.BackColor = Color.ForestGreen;
                        Flaps3.BackColor = Color.ForestGreen;
                        Flaps4.BackColor = Color.ForestGreen;
                        Flaps5.BackColor = Color.ForestGreen;
                        Flaps6.BackColor = Color.ForestGreen;
                        Flaps7.BackColor = Color.ForestGreen;
                        Flaps8.BackColor = Color.ForestGreen;
                        Flaps9.BackColor = Color.ForestGreen;
                        Flaps10.BackColor = Color.ForestGreen;
                        Flaps11.BackColor = Color.ForestGreen;
                        Flaps12.BackColor = Color.ForestGreen;
                        Flaps13.BackColor = Color.ForestGreen;
                        Flaps14.BackColor = Color.ForestGreen;
                        Flaps15.BackColor = Color.ForestGreen;
                        Flaps16.BackColor = Color.ForestGreen;
                        Flaps17.BackColor = Color.ForestGreen;
                        SetFlapsColourDelay = false;
                        SetFlapsColour = false;
                    }
                    else { if (SetFlapsColour) { SetFlapsColourDelay = true; } }

                }



                static bool SetFlaps = false;
                static bool SetFlapsColour = false;
                static bool SetFlapsColourDelay = false;


                /// <summary>
                ///  Selected fleps to on);
                /// </summary>
                /// <param name="Type"></param>
                /// <param name="Data"></param>
                static public void SET(Typ Type)
                {
                    switch (Type)
                    {
                        case Typ.Fps1: OUTPUT0_BIT |= BIT0_SET; break;
                        case Typ.Fps2: OUTPUT0_BIT |= BIT1_SET; break;
                        case Typ.Fps3: OUTPUT0_BIT |= BIT2_SET; break;
                        case Typ.Fps4: OUTPUT0_BIT |= BIT3_SET; break;
                        case Typ.Fps5: OUTPUT0_BIT |= BIT4_SET; break;
                        case Typ.Fps6: OUTPUT0_BIT |= BIT5_SET; break;
                        case Typ.Fps7: OUTPUT0_BIT |= BIT6_SET; break;
                        case Typ.Fps8: OUTPUT0_BIT |= BIT7_SET; break;
                        case Typ.Fps9: OUTPUT1_BIT |= BIT0_SET; break;
                        case Typ.Fps10: OUTPUT1_BIT |= BIT1_SET; break;
                        case Typ.Fps11: OUTPUT1_BIT |= BIT2_SET; break;
                        case Typ.Fps12: OUTPUT1_BIT |= BIT5_SET; break;
                        case Typ.Fps13: OUTPUT1_BIT |= BIT6_SET; break;
                        case Typ.Fps14: OUTPUT1_BIT |= BIT7_SET; break;
                        case Typ.Fps15: OUTPUT2_BIT |= BIT0_SET; break;
                        case Typ.Fps16: OUTPUT2_BIT |= BIT1_SET; break;
                        case Typ.Fps17: OUTPUT2_BIT |= BIT2_SET; break;
                    }
                    FlepsLightSET(Type);
                    TimerFlepsColour.Start();
                    SetFlaps = true;
                    SetFlapsColour = true;
                }

                static public void SET()
                {

                    RUN();
                }

                static public void SET(Typ Type, bool Set)
                {
                    switch (Type)
                    {
                        case Typ.Fps1: OUTPUT0_BIT |= BIT0_SET; break;
                        case Typ.Fps2: OUTPUT0_BIT |= BIT1_SET; break;
                        case Typ.Fps3: OUTPUT0_BIT |= BIT2_SET; break;
                        case Typ.Fps4: OUTPUT0_BIT |= BIT3_SET; break;
                        case Typ.Fps5: OUTPUT0_BIT |= BIT4_SET; break;
                        case Typ.Fps6: OUTPUT0_BIT |= BIT5_SET; break;
                        case Typ.Fps7: OUTPUT0_BIT |= BIT6_SET; break;
                        case Typ.Fps8: OUTPUT0_BIT |= BIT7_SET; break;
                        case Typ.Fps9: OUTPUT1_BIT |= BIT0_SET; break;
                        case Typ.Fps10: OUTPUT1_BIT |= BIT1_SET; break;
                        case Typ.Fps11: OUTPUT1_BIT |= BIT2_SET; break;
                        //case Typ.Fps12: OUTPUT1_BIT |= BIT3_SET; break;
                        //case Typ.Fps13: OUTPUT1_BIT |= BIT4_SET; break;
                        case Typ.Fps12: OUTPUT1_BIT |= BIT5_SET; break;
                        case Typ.Fps13: OUTPUT1_BIT |= BIT6_SET; break;
                        case Typ.Fps14: OUTPUT1_BIT |= BIT7_SET; break;
                        case Typ.Fps15: OUTPUT2_BIT |= BIT0_SET; break;
                        case Typ.Fps16: OUTPUT2_BIT |= BIT1_SET; break;
                        case Typ.Fps17: OUTPUT2_BIT |= BIT2_SET; break;
                    }
                    FlepsLightSET(Type);
                    TimerFlepsColour.Start();
                    SetFlaps = true;
                    SetFlapsColour = true;
                    if (Set) { RUN(); }

                }

                /// <summary>
                /// Send data to "ON" selected FLEPS
                /// </summary>
                static public void RUN()
                {


                    if (SetFlaps == true)
                    {

                        SetFlaps = false;
                        //Buffer_USB_RX[REG_30] = OUTPUT0_BIT;
                        //Buffer_USB_RX[REG_31] = OUTPUT1_BIT;
                        //Buffer_USB_RX[REG_32] = OUTPUT2_BIT; //32

                        HID_Write();
                        OUTPUT0_BIT = 0;
                        OUTPUT1_BIT = 0;
                        OUTPUT2_BIT &= BIT0_RES;
                        OUTPUT2_BIT &= BIT1_RES;
                        OUTPUT2_BIT &= BIT2_RES;
                        //Buffer_USB_RX[REG_30] = OUTPUT0_BIT;
                        //Buffer_USB_RX[REG_31] = OUTPUT1_BIT;
                        //Buffer_USB_RX[REG_32] = OUTPUT2_BIT; //32


                    }
                }

                /// <summary>
                /// Automatic turn-off time of the flaps
                /// </summary>
                /// <param name="Data"></param>
                static public void Time_OFF(decimal Data)
                {
                    if (Data >= 1)
                    {
                        USB_HID.Data.Fleps_Time_OFF = Data;
                        byte[] ConvArray = new byte[4];
                        ConvArray = BitConverter.GetBytes((int)Data);
                        ConvArray = BitConverter.GetBytes((int)Data);
                        Buffer_USB_RX[27] = ConvArray[0];
                        Buffer_USB_RX[28] = ConvArray[1];
                        HID_Write();

                    }
                }

            }

            public class LIGHT
            {


                // ON - OFF  RED
                static public void RED_ERROR(bool ON_OFF)
                {
                    if (ON_OFF) { OUTPUT3_BIT |= BIT7_SET; } else { OUTPUT3_BIT &= BIT7_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT; 
                    HID_Write();
                }

                // ON - OFF  YELLO
                static public void YELLO_ERROR(bool ON_OFF)
                {
                    if (ON_OFF) { OUTPUT3_BIT |= BIT6_SET; } else { OUTPUT3_BIT &= BIT6_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT; 
                    HID_Write();
                }

                // ON - OFF  GREEN
                static public void GREEN_ERRO(bool ON_OFF)
                {
                    if (ON_OFF) { OUTPUT3_BIT |= BIT5_SET; } else { OUTPUT3_BIT &= BIT5_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT; 
                    HID_Write();
                }


                // ON   SOUND на одну секунду
                static public void SOUND_ERRO(bool ON ){
                    if (ON) { OUTPUT3_BIT |= BIT4_SET; } else { OUTPUT3_BIT &= BIT4_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT; //25 OUT
                    HID_Write();
                }


                static public void IR(bool ON)
                {
                    if (ON) { OUTPUT3_BIT |= BIT3_SET; } else { OUTPUT3_BIT &= BIT3_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT; //25 OUT
                    HID_Write();

                }

                static public void Top(bool ON)
                {
                    if (ON) { OUTPUT3_BIT |= BIT2_SET; } else { OUTPUT3_BIT &= BIT2_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT;   //26 OUT
                    HID_Write();
                }

                static public void Bottom(bool ON)
                {
                    if (ON) { OUTPUT3_BIT |= BIT1_SET; } else { OUTPUT3_BIT &= BIT1_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT;   //27 OUT
                    HID_Write();
                }

                static public void Back(bool ON)
                {
                    if (ON) { OUTPUT3_BIT |= BIT0_SET; } else { OUTPUT3_BIT &= BIT0_RES; }
                    Buffer_USB_RX[REG_33] = OUTPUT3_BIT;   //28 OUT
                    HID_Write();
                }



                static public void ON()
                {
                    if (!Data.Light_Back) { OUTPUT3_BIT |= BIT0_SET; }
                    if (!Data.Light_Top) { OUTPUT3_BIT |= BIT1_SET; }
                    if (!Data.Light_Bottom) { OUTPUT3_BIT |= BIT2_SET; }
                    if (!Data.Light_IR) { OUTPUT3_BIT |= BIT3_SET; }


                   // Buffer_USB_RX[REG_33] = OUTPUT3_BIT;   //28 OUT

                    //Buffer_USB_RX[30] = 0;
                    //Buffer_USB_RX[31] = 0;
                    //Buffer_USB_RX[32] &= BIT0_RES;
                    //Buffer_USB_RX[32] &= BIT1_RES;
                    //Buffer_USB_RX[32] &= BIT2_RES;

                    HID_Write();
                }




                static public void OFF()
                {

                    OUTPUT3_BIT &= BIT0_RES;
                    OUTPUT3_BIT &= BIT1_RES;
                    OUTPUT3_BIT &= BIT2_RES;
                    OUTPUT3_BIT &= BIT3_RES;

                    //Buffer_USB_RX[REG_30] = 0;
                    //Buffer_USB_RX[REG_31] = 0;
                    //OUTPUT2_BIT &= BIT0_RES;
                    //OUTPUT2_BIT &= BIT1_RES;
                    //OUTPUT2_BIT &= BIT2_RES;
                    //Buffer_USB_RX[REG_33] = OUTPUT3_BIT;   //28 OUT

                    HID_Write();
                }



            }


            public class CAMERA
            {
                //22 OUT
                static public void OFF()
                {
                    OUTPUT2_BIT &= BIT5_RES;
                    //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();

                }

                static public void ON()
                {
                    OUTPUT2_BIT |= BIT5_SET;
                    //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();
                }



            }


            public class SEPARATOR
            {
                //24 OUT
                static public void OFF()
                {
                    OUTPUT2_BIT &= BIT7_RES;
                    //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();

                }

                static public void ON()
                {
                    OUTPUT2_BIT |= BIT7_SET;
                    //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();
                }

            }


            public class AUTOLOADER
            {  //out 23

                static public void OFF()
                {
                    OUTPUT2_BIT &= BIT6_RES;
                    //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();

                }

                static public void ON()
                {
                    OUTPUT2_BIT |= BIT6_SET;
                    //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();
                }
            }

            public class COOLING
            {  //out 21

                static public void OFF()
                {
                    OUTPUT2_BIT &= BIT4_RES;
                    //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();

                }

                static public void ON()
                {
                    OUTPUT2_BIT |= BIT4_SET;
                   // Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
                    HID_Write();
                }

            }




            public class VIBRATING
            {
                /// <summary>
                /// Types of control
                /// </summary>
                public enum Typ { PWM,/* Table2, Valve, Valve2,*/ Frequency, OFF, ON }

                /// <summary>
                /// Vibrating intensity  0 min - 500 maх ( 0= off ) ( 500= off) ( 255= on);
                /// </summary>
                /// <param name=" VIBRATING"></param>
                /// <returns></returns>
                static public void SET(Typ Type, decimal Data)
                {
                    byte[] ConvArray = new byte[4];
                    ConvArray = BitConverter.GetBytes((Int32)Data);
                    switch (Type)
                    {
                        //** C1 ***//
                        case Typ.PWM: if (Data != 0) { USB_HID.Data.PWM_Table = Data; } ConvArray = BitConverter.GetBytes((Int32)Data); Buffer_USB_RX[9] = ConvArray[0]; Buffer_USB_RX[10] = ConvArray[1]; break;
                        case Typ.Frequency: if (Data != 0) { USB_HID.Data.Hz_Table = Data; } ConvArray = BitConverter.GetBytes((Int32)Data); Buffer_USB_RX[REG_47] = ConvArray[0]; Buffer_USB_RX[REG_48] = ConvArray[1]; break;
                    }
                    HID_Write();
                }



                static public void SET(Typ Type)
                {

                    byte[] ConvArray = new byte[4];
                    Int32 Data = 0;
                    switch (Type)
                    {
                        //** C1 ***//
                        case Typ.OFF: Data = 0; ConvArray = BitConverter.GetBytes((Int32)Data); Buffer_USB_RX[9] = ConvArray[0]; Buffer_USB_RX[10] = ConvArray[1]; break;
                        case Typ.ON:
                            Data = (Int32)USB_HID.Data.PWM_Table; ConvArray = BitConverter.GetBytes((Int32)Data); Buffer_USB_RX[9] = ConvArray[0]; Buffer_USB_RX[10] = ConvArray[1];
                            Data = (Int32)USB_HID.Data.Hz_Table; ConvArray = BitConverter.GetBytes((Int32)Data); Buffer_USB_RX[REG_47] = ConvArray[0]; Buffer_USB_RX[REG_48] = ConvArray[1]; break;
                    }


                    HID_Write();
                }


            }





            void OutputHRD_Set(int OUTPUT_BIT)
            {
                switch (OUTPUT_BIT)
                {
                    //case 15: OUTPUT2_BIT |= BIT0_SET; break; // Flaps 15
                    case 18: OUTPUT2_BIT |= BIT1_SET; break;
                    case 19: OUTPUT2_BIT |= BIT2_SET; break; // Ionizer
                    case 20: OUTPUT2_BIT |= BIT3_SET; break; // 
                    case 21: OUTPUT2_BIT |= BIT4_SET; break; // Cooling
                    case 22: OUTPUT2_BIT |= BIT5_SET; break; // Cameras
                    case 23: OUTPUT2_BIT |= BIT6_SET; break; // Autoloader
                    case 24: OUTPUT2_BIT |= BIT7_SET; break; // Metal separator
                    case 25: OUTPUT3_BIT |= BIT0_SET; break; // Lighte IR
                    case 26: OUTPUT3_BIT |= BIT1_SET; break; // Lighte White
                    case 27: OUTPUT3_BIT |= BIT2_SET; break; // Lighte White
                    case 28: OUTPUT3_BIT |= BIT3_SET; break; // Lighte White
                    case 29: OUTPUT3_BIT |= BIT4_SET; break; // Error sound
                    case 30: OUTPUT3_BIT |= BIT5_SET; break; // Light error green
                    case 31: OUTPUT3_BIT |= BIT6_SET; break; // Light error yellow
                    case 32: OUTPUT3_BIT |= BIT7_SET; break; // Light error red

                }
            }

            void OutputHRD_Res(int OUTPUT_BIT)
            {

                switch (OUTPUT_BIT)
                {
                    //case 15: OUTPUT2_BIT &= BIT0_RES; break;
                    case 18: OUTPUT2_BIT &= BIT1_RES; break; // Lighte
                    case 19: OUTPUT2_BIT &= BIT2_RES; break; // Ionizer
                    case 20: OUTPUT2_BIT &= BIT3_RES; break; // 
                    case 21: OUTPUT2_BIT &= BIT4_RES; break; // Cooling
                    case 22: OUTPUT2_BIT &= BIT5_RES; break; // Cameras
                    case 23: OUTPUT2_BIT &= BIT6_RES; break; // Autoloader
                    case 24: OUTPUT2_BIT &= BIT7_RES; break; // Metal separator
                    case 25: OUTPUT3_BIT &= BIT0_RES; break;
                    case 26: OUTPUT3_BIT &= BIT1_RES; break;
                    case 27: OUTPUT3_BIT &= BIT2_RES; break;
                    case 28: OUTPUT3_BIT &= BIT3_RES; break;
                    case 29: OUTPUT3_BIT &= BIT4_RES; break; // Error sound
                    case 30: OUTPUT3_BIT &= BIT5_RES; break; // Light error green
                    case 31: OUTPUT3_BIT &= BIT6_RES; break; // Light error yellow
                    case 32: OUTPUT3_BIT &= BIT7_RES; break; // Light error red

                }
            }

            //Buffer_USB_RX[REG_30] = OUTPUT0_BIT;
            //Buffer_USB_RX[REG_31] = OUTPUT1_BIT;
            //Buffer_USB_RX[REG_32] = OUTPUT2_BIT;
            //Buffer_USB_RX[REG_33] = OUTPUT3_BIT;




            public class STATUS
            {



                public bool SensorHigh { get; }
                public bool SensorLow { get; }
                public bool Stop { get; }

                public string Door { get; }
                public bool DoorStatus { get; }


              public  STATUS()
                {   
                    SensorHigh = (((Buffer_USB_TX[30] & BIT.I1)) == 0); // Прочитати значення верхнього сенсора
                    SensorLow = (((byte)(Buffer_USB_TX[30] & BIT.I0)) == 0); // Прочитати значення нижнього сенсора
                    Stop = (((byte)(Buffer_USB_TX[31] & BIT.I4)) != 0); // Прочитати значення STOP

                    DoorStatus = false;
                    Door = "";
                    if (((byte)(Buffer_USB_TX[30] & BIT.I2)) != 0) { DoorStatus = true; Door = Door + "- 1"; }
            
                        if (((byte)(Buffer_USB_TX[30] & BIT.I3)) != 0) { DoorStatus = true; Door = Door + "- 2"; }
                     
                            if (((byte)(Buffer_USB_TX[30] & BIT.I4)) != 0) { DoorStatus = true; Door = Door + "- 3"; }
                      
                                if (((byte)(Buffer_USB_TX[30] & BIT.I5)) != 0) { DoorStatus = true; Door = Door + "- 4"; }
                          
                                    if (((byte)(Buffer_USB_TX[30] & BIT.I6)) != 0) { DoorStatus = true; Door = Door + "- 5"; }
                       
                                        if (((byte)(Buffer_USB_TX[30] & BIT.I7)) != 0) { DoorStatus = true; Door = Door + "- 6"; }
                                      
                                            if (((byte)(Buffer_USB_TX[31] & BIT.I0)) != 0) { DoorStatus = true; Door = Door + "- 7"; }
                                        
                                                if (((byte)(Buffer_USB_TX[31] & BIT.I1)) != 0) { DoorStatus = true; Door = Door + "- 8"; }




                    if (DoorStatus == true) { string DoorR = "DOOR IS OPEN  "; Door = DoorR + Door; } else { Door = ""; }

                }



            }
























        }












            //==================================================  USB =========================================================================================================


            /// <summary>
            /// /******************************  NEtwork  *******************************//
            /// </summary>
            public static bool CONECT;
        private static HidDevice device;
        private static HidDevice[] devices;
        private const int ReportLength = 64;

        //public string[] ReadDevice()
        //{
        //    const int LengTyp = 5;
        //    devices = new HidDevice[LengTyp];
        //    int idx = 0;
        //    int LengTypXXX = 0;

        //    devices[0] = HidDevices.Enumerate(DEVICE_VID, V1_PID).FirstOrDefault();
        //    devices[1] = HidDevices.Enumerate(DEVICE_VID, C1_PID).FirstOrDefault();
        //    devices[2] = HidDevices.Enumerate(DEVICE_VID, CMS_PID).FirstOrDefault();
        //    devices[3] = HidDevices.Enumerate(DEVICE_VID, GA_PID).FirstOrDefault();
        //    devices[4] = HidDevices.Enumerate(DEVICE_VID, GA_V2).FirstOrDefault();
        //    while (idx < LengTyp) { if (devices[idx++] != null) { LengTypXXX++; } }
        //    string[] StringDevice = new string[LengTypXXX];
        //    idx = 0;
        //    if (devices[4] != null) { device = devices[4]; StringDevice[idx] = "V2"; idx++; }
        //    if (devices[3] != null) { device = devices[3]; StringDevice[idx] = "GLA"; idx++; }
        //    if (devices[2] != null) { device = devices[2]; StringDevice[idx] = "CMS"; idx++; }
        //    if (devices[1] != null) { device = devices[1]; StringDevice[idx] = "C1"; idx++; }
        //    if (devices[0] != null) { device = devices[0]; StringDevice[idx] = "V1"; idx++; }
        //    return StringDevice;

        //}
        public void SelectedUsbDevice(string NemaDevice)
        {


            if (NemaDevice == "V1") { device = devices[0]; }
            if (NemaDevice == "C1") { device = devices[1]; }
            if (NemaDevice == "CMS") { device = devices[2]; }
            if (NemaDevice == "GLA") { device = devices[3]; }
            if (NemaDevice == "V2") { device = devices[4]; }

            //if ((device == null) || (NemaDevice == ""))
            //{
            //    //MessageBox.Show("No device connecte");
            //    return;
            //}
            //else
            //{
            //    device.OpenDevice();
            //    device.Inserted += DeviceAttachedHandler;
            //    device.Removed += DeviceRemovedHandler;
            //    device.MonitorDeviceEvents = true;
            //    device.ReadReport(OnReport);
            //    // MessageBox.Show(" device connecte");
            //}




        }

        /////////Включити Подію читання USB
        //////private static void DeviceAttachedHandler()
        //////{
        //////    CONECT = true;
        //////    //   Console.WriteLine("Gamepad attached.");
        //////    device.ReadReport(OnReport);
        //////}

        ///////// USB відключено(помилка)
        //////private static void DeviceRemovedHandler()
        //////{
        //////    CONECT = false;
        //////    // Console.WriteLine("Gamepad removed.");




        //////}
        //////void HID_Write(byte[] Bufer)
        //////{
        //////    Bufer[0] = (byte)2;
        //////    // Random.NextBytes(Bufer);
        //////    device.WriteReport(new HidReport(ReportLength, new HidDeviceData(Bufer, HidDeviceData.ReadStatus.Success)));

        //////}

        //////static void StaticHID_Write(byte[] Bufer)
        //////{
        //////    try
        //////    {
        //////        Bufer[0] = (byte)2;
        //////        // Random.NextBytes(Bufer);
        //////        device.WriteReport(new HidReport(ReportLength, new HidDeviceData(Bufer, HidDeviceData.ReadStatus.Success)));
        //////        Bufer[50] = 0;
        //////        Bufer[30] = 0;
        //////        Bufer[31] = 0;
        //////    }
        //////    catch { };

        //////}
        /////////  Читати USB буфер
        //////private static void OnReport(HidReport report)
        //////{
        //////    if (CONECT == false)
        //////    {

        //////        //   if (report.Data.Length >= ReportLength) {}
        //////        //  return;
        //////    }
        //////    else
        //////    {

        //////        for (int i = 0; i < 63; i++) { Buffer_USB_TX[i + 1] = report.Data[i]; }


        //////        if (!RS_485.REFRESH_DATA) { };
        //////        device.ReadReport(OnReport);
        //////    }



        //////}

        /// <summary>
        /// /////////////////////////////////////////////////
        /// </summary>
        /************************  Core 5.0 ******************************************/

        public static bool HidStatus;

        static HidStream HIDstream;

        static readonly HidDeviceLoader HidLoader = new HidDeviceLoader();

        public void HIDinst()
        {
            RefreshConnection();
            DeviceList.Local.Changed += Local_Changed;
        
        }

        void RefreshConnection()
        {
            //var Devise = DeviceList.Local.GetHidDevices();

            if (HidLoader.GetDeviceOrDefault(DEVICE_VID, DEVICE_PID) != null)
            {

                device = HidLoader.GetDeviceOrDefault(DEVICE_VID, DEVICE_PID);
                HIDstream = device.Open();

                //HIDstream.ReadTimeout = 20;
                //byte[] buffer = new byte[65];
                //await HIDstream.ReadAsync(buffer, 0, buffer.Length);
                //byte[] bufferWr = new byte[65];
                //bufferWr[0] = 2;
                //HIDstream.WriteTimeout = 10;
                //await HIDstream.WriteAsync(bufferWr, 0, 65);

                //HIDstream.InterruptRequested += InterruptRequested();
                //HIDstream.Closed += InterruptRequested1();

                HidStatus = true;
            }
            else { HidStatus = false; }

        }

        public string[] InstalDevice(string NemaDevice)
        {
            const int LengTyp = 5;
            devices = new HidDevice[LengTyp];
            int idx = 0;
            int LengTypXXX = 0;

            devices[0] = HidLoader.GetDeviceOrDefault(DEVICE_VID, V1_PID);
            devices[1] = HidLoader.GetDeviceOrDefault(DEVICE_VID, C1_PID);
            devices[2] = HidLoader.GetDeviceOrDefault(DEVICE_VID, CMS_PID);
            devices[3] = HidLoader.GetDeviceOrDefault(DEVICE_VID, GA_PID);
            devices[4] = HidLoader.GetDeviceOrDefault(DEVICE_VID, GA_V2);
            while (idx < LengTyp) { if (devices[idx++] != null) { LengTypXXX++; } }
            string[] StringDevice = new string[LengTypXXX];
            idx = 0;
            if ((devices[4] != null) && (NemaDevice == "V2")) { StringDevice[idx] = "V2"; idx++; device = devices[4]; HIDstream = device.Open(); HidStatus = true; }
            else
            {
                if ((devices[3] != null) && (NemaDevice == "GLA")) { StringDevice[idx] = "GLA"; idx++; device = devices[3]; HIDstream = device.Open(); HidStatus = true; }
                else
                {
                    if ((devices[2] != null) && (NemaDevice == "CMS")) { StringDevice[idx] = "CMS"; idx++; device = devices[2]; HIDstream = device.Open(); HidStatus = true; }
                    else
                    {
                        if ((devices[1] != null) && (NemaDevice == "C1")) { StringDevice[idx] = "C1"; idx++; device = devices[1]; HIDstream = device.Open(); HidStatus = true; }
                        else
                        {
                            if ((devices[0] != null) && (NemaDevice == "V1")) { StringDevice[idx] = "V1"; idx++; device = devices[0]; HIDstream = device.Open(); HidStatus = true; }
                            else
                            {
                                HidStatus = false; return null;
                            }
                        }
                    }
                }
            }

            //DeviceList.Local.Changed += Local_Changed;
            //Buffer_USB_RX = new byte[PAGE];
            //HID_Write();

            return StringDevice;

        }


        private void Local_Changed(object sender, DeviceListChangedEventArgs e)
        {
            InstalDevice("V2");
        }

        private static async void HID_Write()
        {

            Buffer_USB_RX[REG_30] = OUTPUT0_BIT;
            Buffer_USB_RX[REG_31] = OUTPUT1_BIT;
            Buffer_USB_RX[REG_32] = OUTPUT2_BIT;   //32
            Buffer_USB_RX[REG_33] = OUTPUT3_BIT;   //28 OUT

            Buffer_USB_RX[0] = (byte)2;
            try
            {

                if (HidStatus == true)
                {
                    await HIDstream.WriteAsync(Buffer_USB_RX, 0, 64);
                }
              
            }
            catch {// Help.Mesag("USB sending problems");
            }


        }






        public static async void HID_Read()
        {
            while (Flow.PotocStartUSB)
            {

                Buffer_USB_RX[0] = (byte)1;
                // try
                //{

                if (HidStatus == true)
                {



                    await HIDstream.ReadAsync(Buffer_USB_TX, 0, 64);
                    Thread.Sleep(50); // Затримка в 1 секунду

                }
                

            }
        }



    }
}
