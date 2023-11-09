using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




using System.Drawing.Imaging;

using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Visitors;

using MigraDoc.DocumentObjectModel;


using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using System.Xml.XPath;
using PdfSharp.Pdf;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using System.Drawing;
using System.IO;
using Image = System.Drawing.Image;
using System.Reflection;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace C2S150_ML
{
    public class PDF
    {

        public static DATA_Save Data = new DATA_Save();


        [Serializable()]
        public class DATA_Save
        {

            //для виривнюваня фону
            public string PathFileSave;
            public string NameReport;
            public string SampleType;
            public string CreatedBy;
            public string Comments;
            public   bool ShowImageInReport;

        }







        //private MigraDoc.DocumentObjectModel.TabAlignment tabAlignment = new MigraDoc.DocumentObjectModel.TabAlignment();
        private Document document = new Document();
        private Table table = new Table();
        private TextFrame textFrame = new TextFrame();
        // private MigraDoc.DocumentObjectModel.Color TableBorder;
        // private MigraDoc.DocumentObjectModel.Color TableGray;
        private TextFrame addressFrame { get; set; }
        private Section section;
        private PDF_DT report;


        // [Obsolete]
        Document CreateDocument(PDF_DT Report)
        {
            report = Report;

            // Create a new MigraDoc document
            DefineStyles();
            CreatePage();
            FillContent();


            DefineCharts(document);
            SetImg();


            Document documentRender = new Document();
            documentRender = document;
            document = new Document();


            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer()
            {
                //    // Передайте документ візуалісту:
                Document = documentRender
            };

            // Нехай візуаліст виконує свою роботу:
            pdfRenderer.RenderDocument();
            DateTime dateOnly = new DateTime();
            dateOnly = DateTime.Now;
            String DataFile = dateOnly.Month.ToString() + ".";
            DataFile = DataFile + dateOnly.Day.ToString() + ".";
            DataFile = DataFile + dateOnly.Year.ToString() + ". ";
            DataFile = DataFile + dateOnly.Hour.ToString() + ".";
            DataFile = DataFile + dateOnly.Minute.ToString() + ".";
            DataFile = DataFile + dateOnly.Second.ToString() + " ";

            try
            {
                //Збережіть PDF у файл:
                string filename = Data.PathFileSave + "\\" + Data.NameReport + " " + DataFile + ".pdf";
        
                pdfRenderer.PdfDocument.Save(filename);
                //  У програмі для ПК може відображатися файл:
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = filename,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }

            catch { Help.Mesag("The path is not correct 'select the folder path' "); }
            //// RtfDocumentRenderer rtf = new RtfDocumentRenderer();
            //// rtf.Render(documentRender, "test.rtf", null);
            ////Process.Start("test.rtf");

            return this.document;
        }









        private void DefineStyles()
        {

            // Get the predefined style Normal.
            Style style = this.document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";

            style = this.document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("6cm", MigraDoc.DocumentObjectModel.TabAlignment.Right);

            style = this.document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", MigraDoc.DocumentObjectModel.TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = this.document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;



            // Create a new style called Reference based on style Normal
            style = this.document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "2mm";  //відступ перед таблицею
            style.ParagraphFormat.SpaceAfter = "2mm";   //відступ після таблиці
            style.Font.Name = "Times New Roman";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", MigraDoc.DocumentObjectModel.TabAlignment.Left);
            style.ParagraphFormat.Shading.Color = Colors.LightGray;

            // Create a new style called Reference based on style Normal
            style = this.document.Styles.AddStyle("StyleType1", "Normal");
            style.ParagraphFormat.SpaceBefore = "2mm";  //відступ перед таблицею
            style.ParagraphFormat.SpaceAfter = "2mm";   //відступ після таблиці
            style.Font.Name = "Times New Roman";
            style.ParagraphFormat.TabStops.AddTabStop("15cm", MigraDoc.DocumentObjectModel.TabAlignment.Left);
            //style.ParagraphFormat.Shading.Color = Colors.LightGray;

            // Create a new style called Reference based on style Normal
            style = this.document.Styles.AddStyle("StyleType2", "Normal");
            style.ParagraphFormat.SpaceBefore = "2mm";  //відступ перед таблицею
            style.ParagraphFormat.SpaceAfter = "2mm";   //відступ після таблиці
            style.Font.Name = "Times New Roman";
            style.ParagraphFormat.TabStops.AddTabStop("15cm", MigraDoc.DocumentObjectModel.TabAlignment.Center);
            //style.ParagraphFormat.Shading.Color = Colors.LightGray;


            // Create a new style called TextBox based on style Normal
            style = document.Styles.AddStyle("TextBox", "Normal");
            style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            style.ParagraphFormat.Borders.Width = 2.5;
            style.ParagraphFormat.Borders.Distance = "3pt";
            style.ParagraphFormat.Shading.Color = Colors.SkyBlue;


        }




        string MigraDocFilenameFromByteArray(byte[] image)
        {
            return "base64:" + Convert.ToBase64String(image);
        }

        byte[] LoadImage(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + name);

                int count = (int)stream.Length;
                byte[] data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }


        private byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }


        private void CreatePage()
        {

            // Each MigraDoc document needs at least one section.
            section = this.document.AddSection();
            MigraDoc.DocumentObjectModel.Shapes.Image image = new MigraDoc.DocumentObjectModel.Shapes.Image();
            section.PageSetup.OddAndEvenPagesHeaderFooter = true;
            section.PageSetup.StartingNumber = 1;

            // Put a logo in the header
            image = section.Headers.Primary.AddImage("MicroOptik.tif");      //                   section.Headers.Primary.AddImage("../../PowerBooks.png");


            image.Height = "2cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Right;
            image.WrapFormat.Style = WrapStyle.TopBottom;



            // Create footer
            Paragraph paragraph = section.Footers.Primary.AddParagraph();
            //paragraph.AddText("Pge 1");





            // Створіть новий документ MigraDoc
            Document document = new Document();

      

            // Встановіть відступи сторінки (в даному випадку, зверху, справа, знизу, зліва - по 1 дюйму)
            section.PageSetup.TopMargin = MigraDoc.DocumentObjectModel.Unit.FromInch(0.9);
            section.PageSetup.RightMargin = MigraDoc.DocumentObjectModel.Unit.FromInch(0.2);
            section.PageSetup.BottomMargin = MigraDoc.DocumentObjectModel.Unit.FromInch(0);
            section.PageSetup.LeftMargin = MigraDoc.DocumentObjectModel.Unit.FromInch(0.9);

            // Встановіть номер сторінки внизу сторінки
            section.PageSetup.StartingNumber = 1;

           
        












            paragraph.Format.Font.Size = 9;
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // Create the text frame for the address
            addressFrame = section.AddTextFrame();
            this.addressFrame.Height = "3.0cm";
            this.addressFrame.Width = "7.0cm";
            this.addressFrame.Left = ShapePosition.Left;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.Top = "5.0cm";
            this.addressFrame.RelativeVertical = RelativeVertical.Page;

            // Put sender in address frame
            paragraph = this.addressFrame.AddParagraph("Sorting machine C2S 150");
            paragraph.Format.Font.Name = "Times New Roman";
            paragraph.Format.Font.Size = 24;
            paragraph.Format.SpaceAfter = 3;

            // Put sender in address frame
            paragraph = this.addressFrame.AddParagraph("Name of report : " + Data.NameReport);
            paragraph.Format.Font.Name = "Times New Roman";
            paragraph.Format.Font.Color = Colors.Blue;
            paragraph.Format.Font.Size = 14;
            paragraph.Format.SpaceAfter = 5;

            // Put sender in address frame
            paragraph = this.addressFrame.AddParagraph("Created by :    " + Data.CreatedBy);
            paragraph.Format.Font.Name = "Times New Roman";
            paragraph.Format.Font.Color = Colors.Black;
            paragraph.Format.Font.Size = 14;
            paragraph.Format.SpaceAfter = 5;
            paragraph = section.AddParagraph();  // Add the print date field

            // Put sender in address frame ///////////////////////////////////
            paragraph.Style = "StyleType1";
            paragraph.Format.SpaceBefore = "3cm";
            paragraph = this.addressFrame.AddParagraph("Comments :    " + Data.Comments);
            paragraph.Format.Font.Name = "Times New Roman";
            paragraph.Format.Font.Color = Colors.Black;
            paragraph.Format.Font.Size = 14;
            paragraph.Format.SpaceAfter = 5;

            // paragraph.AddTab();
            paragraph = section.AddParagraph();  // Add the print date field
            //////////////////////////////////

            paragraph.Format.SpaceBefore = "8cm";
            paragraph.Style = "StyleType1";
            paragraph.Format.Font.Size = 14;
            paragraph.AddFormattedText("Sample type :   " + Data.SampleType, TextFormat.Bold/* жирний шрифт */ );

            paragraph = section.AddParagraph();  // Add the print date field

            paragraph.Format.Font.Size = 12;
            paragraph.AddText("Measure Date: ");
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddDateField("MM/dd/yyyy    hh:mm:ss tt");
           
            paragraph = section.AddParagraph();  // Add the print date field
           
            


            // Create the item table
            this.table = section.AddTable();
            //////////////////////////////
            this.table.Style = "Table";
            this.table.Borders.Color = Colors.Black;           //TableBorder;
            this.table.Borders.Width = 0.5;
            this.table.Borders.Left.Width = 0.5;
            this.table.Borders.Right.Width = 0.5;
            this.table.Rows.LeftIndent = 0;

            // Before you can add a row, you must define the columns
            Column column = this.table.AddColumn("1cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("5cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("6cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            //column = this.table.AddColumn("4cm");
            //column.Format.Alignment = ParagraphAlignment.Center;


            // column = this.table.AddColumn("3cm");
            // column.Format.Alignment = ParagraphAlignment.Center;


            // Create the header of the table
            Row row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = Colors.LightGray;
            row.Format.Font.Size = 12;
            int Caunt_colun = 0;

            row.Cells[Caunt_colun].AddParagraph(" № ");
            row.Cells[Caunt_colun].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[Caunt_colun].VerticalAlignment = VerticalAlignment.Top;
            row.Cells[Caunt_colun++].MergeDown = 0; //обєднати стовці

            row.Cells[Caunt_colun].AddParagraph(" Description");
            row.Cells[Caunt_colun].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[Caunt_colun].VerticalAlignment = VerticalAlignment.Top;
            row.Cells[Caunt_colun++].MergeDown = 0; //обєднати стовці

            row.Cells[Caunt_colun].AddParagraph(" Data ");
           //row.Cells[Caunt_colun].Format.Font.Bold = false;
            row.Cells[Caunt_colun].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[Caunt_colun].VerticalAlignment = VerticalAlignment.Top;
            row.Cells[Caunt_colun++].MergeDown = 0; // обєднати рядки


            //row.Cells[3].AddParagraph(" Quantity ");
            //row.Cells[3].Format.Font.Bold = false;
            //row.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            //row.Cells[3].VerticalAlignment = VerticalAlignment.Top;
            //row.Cells[3].MergeDown = 0;


            //row.Cells[3].AddParagraph("ACTUAL NUMBER OF REJECTS");
            //row.Cells[3].Format.Font.Bold = false;
            //row.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            //row.Cells[3].VerticalAlignment = VerticalAlignment.Top;
            //row.Cells[3].MergeDown = 0;

            this.table.SetEdge(0, 0, Caunt_colun, 1, Edge.Box, MigraDoc.DocumentObjectModel.BorderStyle.Single, 0.80, MigraDoc.DocumentObjectModel.Color.Empty);
        }



        // НАПОВНЕННЯ ТАБЛИЧКИ
        private void FillContent()
        {


            for (int i = 0; i < report.Idx; i++)
            {

                // Create the item table
                Row row = this.table.AddRow();
                //nema
                row.Cells[0].AddParagraph((i + 1).ToString());
                row.Cells[0].Format.Font.Bold = false;
                row.Cells[0].Format.Alignment = ParagraphAlignment.Center;
                row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
                row.Cells[0].MergeRight = 0; // обєднати рядки
                row.Cells[0].Shading.Color = Colors.Honeydew;
                row.Cells[0].Format.Font.Size = 14;


                //Description
                row.Cells[1].AddParagraph(report.Name[i]);
                row.Cells[1].Format.Font.Bold = false;
                row.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                row.Cells[1].VerticalAlignment = VerticalAlignment.Top;
                row.Cells[1].MergeRight = 0; // обєднати рядки
                row.Cells[1].Shading.Color = Colors.Honeydew;
                row.Cells[1].Format.Font.Size = 14;

                //Data
                row.Cells[2].AddParagraph(report.Data[i].ToString());
                row.Cells[2].Format.Font.Bold = false;
                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[2].VerticalAlignment = VerticalAlignment.Top;
                row.Cells[2].MergeRight = 0; // обєднати рядки
                row.Cells[2].Shading.Color = Colors.Honeydew;
                row.Cells[2].Format.Font.Size = 14;



                // Set the borders of the specified cell range
                this.table.SetEdge(1, this.table.Rows.Count - 1, 2, 1, Edge.Box, MigraDoc.DocumentObjectModel.BorderStyle.Single, 0.75);

            }
        }



        //---------------- ГРАФІК  -------------------------
        private void DefineCharts(Document document)
        {

            Section section = document.AddSection();

            // Create footer
            Paragraph paragraph = section.Footers.Primary.AddParagraph();
            //paragraph.AddText("Pge 2");
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            document.LastSection.AddParagraph();
            //document.LastSection.AddParagraph();


            document.LastSection.AddParagraph(" Chart  :" + Data.NameReport, "Heading2");



            Chart chart = new Chart();
            chart.XAxis.Title.Font.Name = "Tahoma";
            chart.XAxis.Title.Font.Color = Colors.DarkBlue;
            chart.XAxis.Title.Font.Size = 14;
            chart.XAxis.Title.Caption = " Contamination diameter  ";
            chart.XAxis.Title.Alignment = HorizontalAlignment.Center;

            chart.YAxis.Title.Font.Name = "Tahoma";
            chart.YAxis.Title.Font.Color = Colors.DarkBlue;
            chart.YAxis.Title.Font.Size = 14;
            chart.YAxis.Title.Caption = " Quantity ";
            chart.YAxis.Title.Orientation = 90;
            chart.YAxis.Title.Alignment = HorizontalAlignment.Center;

            // Set the font properties for XAxis labels
            chart.XAxis.TickLabels.Font.Name = "Arial"; // Set the font name
            chart.XAxis.TickLabels.Font.Color = Colors.Green; // Set the font color
            chart.XAxis.TickLabels.Font.Size = 12; // Set the font size

            // Set the font properties for YAxis labels
            chart.YAxis.TickLabels.Font.Name = "Arial"; // Set the font name
            chart.YAxis.TickLabels.Font.Color = Colors.Blue; // Set the font color
            chart.YAxis.TickLabels.Font.Size = 10; // Set the font size
                // Для цілих чисел
            chart.YAxis.MajorTickMark = TickMarkType.Outside;
            chart.YAxis.HasMajorGridlines = true;
            chart.YAxis.TickLabels.Format = "0"; // Формат для цілих чисел

            //chart.Left = 0;
            document.LastSection.AddParagraph();
            document.LastSection.AddParagraph();
            paragraph.Format.SpaceAfter = "4cm";
            paragraph.Format.SpaceBefore = "6cm";



            chart.Width = Unit.FromCentimeter(17);
            chart.Height = Unit.FromCentimeter(12);
            Series series = chart.SeriesCollection.AddSeries();

            series.ChartType = ChartType.Column2D;

            const int LengColun = 9;
            double[] DataReport = new double[report.Idx - LengColun];

            for (int i = 0; i < report.Idx - LengColun; i++){

                DataReport[i] =  Convert.ToDouble( report.Data[LengColun+i] );

            }

            series.Add(DataReport);

            series.HasDataLabel = false;

            // підпис в середині графіка
            //series.DataLabel.Font.Color = Colors.Red;
            //series.DataLabel.Type = DataLabelType.Value;
            //series.DataLabel.Position = DataLabelPosition.InsideBase;

            //series.
            //.AxisX.Title
            //    series.ti
            // System.IO.MemoryStream stream = new System.IO.MemoryStream();
            //chart.SaveImage(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            // Bitmap bmp = new Bitmap(stream); 
            //початок вибірки ( i )


            string[] NemaReportX = new string[report.Idx - LengColun];
            XSeries xseries = chart.XValues.AddXSeries();

 
            for (int i = 0; i < report.Idx- LengColun; i++){

                NemaReportX[i] = report.Name[LengColun+i] + "μm";

            }

            // After setting up your series
            series.HasDataLabel = true; // Enable data labels

            // Set the font properties for data labels
            series.DataLabel.Font.Name = "Arial"; // Set the font name
            series.DataLabel.Font.Color = Colors.Red; // Set the font color
            series.DataLabel.Font.Size = 10; // Set the font size


            xseries.Add(NemaReportX);
            chart.Format.Font.Size = report.Idx - LengColun;


            chart.XAxis.LineFormat.Color = Colors.Plum;

            //chart.XAxis.MajorTickMark = TickMarkType.Outside;
            //chart.XAxis.Title.Caption = "Y-" + SV.DT_BIN.NameReport;

            chart.YAxis.MajorTickMark = TickMarkType.Outside;
            chart.YAxis.HasMajorGridlines = true;



            chart.PlotArea.LineFormat.Color = Colors.DarkGray;
            chart.PlotArea.LineFormat.Width = 2;
     
            document.LastSection.Add(chart);
        }





        void SetImg()
        {

            // Each MigraDoc document needs at least one section.
           section = this.document.AddSection();
           MigraDoc.DocumentObjectModel.Shapes.Image image = new MigraDoc.DocumentObjectModel.Shapes.Image();




            //////////////////// Put a logo in the header
            image = section.Headers.Primary.AddImage("MicroOptik.tif");      //                   section.Headers.Primary.AddImage("../../PowerBooks.png");
            image.Height = "1.2cm";
            image.Width= "1.9cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
          
            //image.Top = "-1cm"; // Замість від'ємного значення встановіть конкретне положення
            image.Top = ShapePosition.Top; 
            image.Left = ShapePosition.Right;
            image.WrapFormat.Style = WrapStyle.Through;










            if ((report.IMG != null)/*&&(report.IMG[1].Count != 0)*/)
            {
       



                Paragraph paragraph = section.Footers.Primary.AddParagraph();






                        // Add the print date field
                        paragraph = section.AddParagraph();

                //paragraph.Format.SpaceBefore = "-1cm";  відступ в гору
                //paragraph.Format.SpaceAfter= "1cm"; // відступ в низ
                paragraph.Style = "Reference";
                        paragraph.Style.AsQueryable();
                        paragraph.Format.Alignment = ParagraphAlignment.Left;
                        paragraph.AddFormattedText(" Images with contaminations -  "+ report.IMG.Count.ToString() + "  PCS", TextFormat.NotItalic);
                
                paragraph.AddSpace(60);
                paragraph.Format.Font.Size =    16;
                paragraph.Format.Font.Bold = false;
                paragraph.Format.Font.Color = Colors.DarkBlue;
               




                Bitmap Data;
                            string imageFilename = "";
                            MemoryStream strm = new MemoryStream();

                for (int Q = 0; Q < report.IMG.Count ; Q++)
                {
                          
                                //запис Bitmap через стрім
                                strm = new MemoryStream();
                                Data = new Bitmap(report.IMG[Q]);


                                Data.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);
                                imageFilename = MigraDocFilenameFromByteArray(strm.ToArray());

  
                                paragraph.AddImage(imageFilename).Clone();

                                strm.Close();

                             }

                        
                    






             




                //Create footer
                //paragraph = section.Footers.Primary.AddParagraph();
                //paragraph.AddText("www.MicrOptic.com ");
                //paragraph.Format.Font.Size = 10;
                //paragraph.Format.Alignment = ParagraphAlignment.Justify;

                // Create the text frame for the address
                //this.addressFrame = section.AddTextFrame();
                //this.addressFrame.Height = "3.0cm";
                //this.addressFrame.Width = "7.0cm";
                //this.addressFrame.Left = ShapePosition.Left;
                //this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
                //this.addressFrame.RelativeVertical = RelativeVertical.Page;




            }

        }




        public void ReportSet(PDF_DT Report)
        {





            //report = Report;
            //PDF_DT reportDTrw;

            try
            {

            //    if (report == null)
            //    {
            //        //report = new ReportDT(report.IMG.Length);
            //        for (int i = 0; i < report.Name.Length; i++)
            //        {
            //            report.Name[i] = " ";
            //            report.Data[i] = "";
            //            report.DataQunty[i] = 0;
            //            report.REJEST[i] = 0;

            //        }
            //    }




            //    int cot = 0;
            //    for (int i = 0; i < report.IMG.Length; i++) { cot++; }
            //    reportDTrw = new PDF_DT(cot );
            //    cot = 0;

            //    for (int i = 0; i < report.Name.Length; i++) {



            //        reportDTrw.Name[cot] = report.Name[i];
            //        reportDTrw.Data[cot] = report.Data[i];
            //        reportDTrw.DataQunty[cot] = report.DataQunty[i];
            //        reportDTrw.REJEST[cot] = report.REJEST[i];
            //        //reportDTrw.IMG[cot] = report.IMG[i];
            //        cot++;
            //        reportDTrw.Idx = cot;

            
            //    }

            //    report = reportDTrw;

                CreateDocument(Report);


            }
            catch { Help.Mesag(" Experiment cannot be empty ! "); }



        }



    }

















    public class PDF_DT
    {

        public int Idx;


        //CHART
        public PDF_DT(int idx )
        {
            this.Idx = idx;
            Name = new string[Idx];
            Data= new string[Idx];

            // ChartTabl = new double  [Idx];
            //DataQunty = new int[Idx];
            //REJEST = new double[Idx];
            //img = new Bitmap[Idx];
            IMG = new List<Bitmap>();
        }







        public string[] Name;
        public string[] Data;      //200

        public double[] ChartTabl;



       public int[] DataQunty;     //Caunt
       public double[] REJEST;
        //public Bitmap[]  img ;
        public List<Bitmap> IMG;



    }




}
