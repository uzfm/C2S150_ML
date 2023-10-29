using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Diagnostics;
using System.IO;

namespace C2S150_ML
{
    class XLSX
    {
      


            public void CreateXlsx(DTchart dTchart, string URL_Save)
            {


                var reportExcel = new MarketExcelGenerator().Generate(dTchart);
                string DateNameFile = DateTime.Now.ToString("dd_MM_yyyy  hh_mm_ss");

                DateNameFile = URL_Save + "\\Report " + DateNameFile + ".xlsx";
                File.WriteAllBytes(DateNameFile, reportExcel);



                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = DateNameFile,
                    UseShellExecute = true
                }; Process.Start(psi);



            }










            public class DTchart
            {
                public string PeriodFrom { get; set; }
                public string PeriodTo { get; set; }
                public String[] Name { set; get; }
                public DT[] DT { set; get; }
            }

            public class DT
            {
                public string[] Value { set; get; }
            }









            public class MarketExcelGenerator
            {

                public byte[] Generate(DTchart dTchart)
                {
                // Встановити LicenseContext на початку вашого додатка (наприклад, в методі Main або в конструкторі головної форми)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // або LicenseContext.Commercial

                var package = new ExcelPackage();

                    var sheet = package.Workbook.Worksheets.Add("Report"); //name List


                    sheet.Cells["B3"].Value = "C2 150";
                    sheet.Cells["B3"].Style.Font.Size = 26;
                    sheet.Cells["B3"].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    // Змінити висоту рядка для відображення тексту
                    sheet.Row(3).Height = 38; // Задайте необхідну висоту

                    sheet.Cells["B4"].Value = "Period from : " + dTchart.PeriodFrom;
                    sheet.Cells["B4"].Style.Font.Bold = true;
                    sheet.Cells["B4"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                    sheet.Cells["B5"].Value = "Period to : " + dTchart.PeriodTo;
                    sheet.Cells["B5"].Style.Font.Bold = true;
                    sheet.Cells["B5"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                    DateTime dateOnly = new DateTime();
                    dateOnly = DateTime.Now;
                    sheet.Cells["B6"].Value = "Date of creation : " + dateOnly.ToString();
                    sheet.Cells["B6"].Style.Font.Bold = true;
                    sheet.Cells["B6"].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    sheet.Cells["B6:D6"].Merge = true; // Об'єднати клітинки з трьома справа

                   //Name Chart - Fulling 
                    sheet.Cells[8, 2, 8, dTchart.Name.Length + 2].LoadFromArrays(new object[][] { dTchart.Name });
                    sheet.Cells[8, 2, 8, dTchart.Name.Length + 2].Style.WrapText = true; // перенос по строкам
                    sheet.Cells[8, 2, 8, dTchart.Name.Length + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    //------СТИЛЬ ЗАГОЛОВКА ТАБЛИЦІ
                    //заміна коліру заголовка 11-12
                    sheet.Cells[8, 2, 8, 11].Style.Font.Color.SetColor(System.Drawing.Color.DarkRed);
                    //sheet.Cells[8, 12, 8, dTchart.Name.Length + 2].Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);

                    sheet.Cells[8, 2, 8, dTchart.Name.Length + 2].Style.Font.Bold = true; // select text
                    sheet.Cells[8, 2, 8, dTchart.Name.Length + 2].Style.Font.Size = 14; // шрифт заголовка
                    sheet.Cells[8, 2, 8, 2].Style.Font.UnderLine = true; // виділити підкреслення Select Row

                    sheet.Column(1).Width = 5; //Ширина ID стовпця
                    sheet.Column(2).Width = 22;// Ширина Tate Time стовпця
                    sheet.Row(8).Height = 35; // Задайте необхідну висоту ЗАГОЛОВКА



                //Chart - Fulling
                var row = 9;     // ROW START TABLE 
                    var column = 2;  // COLUMN START TABLE 
                    int ID = 1;      // ID START TABLE 

                //********* " string char " separate write  не стандартні дані (TATE TIME) *******//
                foreach (DT item in dTchart.DT)
                    {
                    
                        sheet.Cells[row, column - 1].Value = ID++;
                        int i = 0;
                        sheet.Cells[row, column + i].Value = item.Value[i];
                        i++;

                        //***********************************************//
                        try
                        {
                            for (; i < item.Value.Length-1; i++)
                            {

                                if (item.Value[i] != "")
                                {
                                    sheet.Cells[row, column + i].Value = Convert.ToDouble(item.Value[i]);
                                 }  
                            }
                            //Останій запис String
                        sheet.Cells[row, column + i].Value = item.Value[i];


                    } catch { Help.Mesag("Input string was not in correct format (XLSX)"); }

                        row++;// Вибір рядка для запису Data
                    }





                sheet.Protection.IsProtected = true;
                    return package.GetAsByteArray();
                }
            }


        }

    }

