using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace C2S150_ML
{
    class SQL
    {

        #region Includ
        static private SqlConnection sqlConnection = null;
        static private SqlCommandBuilder sqlBilder = null;
        static private SqlDataAdapter sqlDataAdapter = null;
        static private DataSet dataSet = null;
        static private SqlCommand sqlCommand = null;
        static private String sqlconect = @$"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={SETS.Data.PachDB }\Databases.mdf;Integrated Security=True";
        string databaseName = "Databases"; // Замініть на бажане ім'я для бази даних

        DataGridViewLinkCell linkCell;
        DataGridViewButtonCell buttonCell;


        class GridData
        {
            static public string[] Name;
            static public int[] Valua;
            static public int StartDataFemeli = 11;
            static public string GOOD = "Good";
        };

        static public class TimSQL {

            public static string Now;
            public static string AddDayst;
        }


        static string TablNema = "USER";// назва таблиці 
        //static string DataTimes = "Date Time Star"; //виборка по даті. В таблиці має бути такий стовбець для сортування по даті
        public static string[] СolumnNames = {
                 "Sample name",
                 "Date Time Start",
                 "Time Stop",
                 "Good %",
                 "Bad %",
                 "Speed Kg/h",
                 "Good Kg",
                 "Bad Kg",
                 "Total Kg",
                 "Size <100",
                 "Size 100-500",
                 "Size 500-1000"
        };

        static string DataTimes = СolumnNames[1]; //виборка по даті. В таблиці має бути такий стовбець для сортування по даті

        #endregion Includ 


        static public void Updat(bool SelectData, DataGridView dataGridView1, string DataA, string DataB)
        {

            Font F = new Font("Arial", 8, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Red;
            dataGridView1.RowsDefaultCellStyle.ForeColor = Color.Blue;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = F;
            dataGridView1.ReadOnly = true;

            if (!SelectData)

            {
                DataA = DateTime.Now.Date.ToString("yyy-MM-dd");
                DataB = DateTime.Now.Date.AddDays(1).ToString("yyy-MM-dd");

            }
            
            sqlDataAdapter = new SqlDataAdapter($"SELECT * FROM [{TablNema}] WHERE [{DataTimes}] >='" + DataA + $"' AND [{DataTimes}] <='" + DataB + "'", sqlConnection);
            
            try
            {
                sqlBilder = new SqlCommandBuilder(sqlDataAdapter);
                sqlBilder.GetInsertCommand();
                sqlBilder.GetUpdateCommand();
                sqlBilder.GetDeleteCommand();
                dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet, TablNema);
                dataGridView1.DataSource = dataSet.Tables[TablNema];


                //dataGridView1.Columns.w

                GridData.Valua = new int[dataSet.Tables[TablNema].Columns.Count];
                GridData.Name = new string[dataSet.Tables[TablNema].Columns.Count];
                //comboBox2.Items.Clear();
                //comboBox3.Items.Clear();
                //визнвчаєм назви заголовків
                for (int i = 0; i < dataSet.Tables[$"{TablNema}"].Columns.Count; i++)
                {
                    GridData.Name[i] = dataSet.Tables[$"{TablNema}"].Columns[i].ColumnName.ToString();
                    if (i >= GridData.StartDataFemeli)
                    {
                        ///comboBox3.Items.Add(GridData.Name[i]);
                        //comboBox2.Items.Add(GridData.Name[i]);
                    }
                    dataGridView1.Columns[i].Width = 63;
                }

                dataGridView1.Columns[0].Width = 52;
                dataGridView1.Columns[1].Width = 85;
                dataGridView1.Columns[2].Width = 123;
                dataGridView1.Columns[3].Width = 60;
            } catch { Help.Mesag("Database error! Check database."); }


        }


      static  public bool Conect()
        {
            sqlConnection = new SqlConnection(sqlconect);
            
                try
                {
                    sqlConnection.Open();
                    //Help.WriteLine("Connected to SQL Server.");

                // Перевірка наявності таблиці value -"TablNema"
                if (!TableExists(sqlConnection, TablNema)) {
                        // Якщо таблиці не існує, створюємо її
                        CreateUsersTable(sqlConnection);
                    }

              if (!ColumnExists(sqlConnection, TablNema, СolumnNames[0]))
                {
                    // Якщо стовпця "DataTime" не існує, додаємо його
                    AddColumn(sqlConnection, TablNema, СolumnNames[0], "NVARCHAR(MAX)");
                }

                    // Перевірка наявності стовпців
                    if (!ColumnExists(sqlConnection, TablNema, СolumnNames[1]))
                    {
                        // Якщо стовпця "DataTime" не існує, додаємо його
                        AddColumn(sqlConnection, TablNema, СolumnNames[1], "DATETIME");
                    }
       


                // Пройтися по списку імен стовців і додати їх до "TablNema"
                foreach (string columnName in СolumnNames.Skip(2))
                    {
                        if (!ColumnExists(sqlConnection, TablNema, columnName))
                        {
                            // Якщо стовпця "Data" не існує, додаємо його
                            AddColumn(sqlConnection, TablNema, columnName, "NVARCHAR(MAX)");
                        }
                    }



                    var bfvdfb = DateTime.Now;
                    TimSQL.Now = DateTime.Now.ToString("yyy-MM-dd");
                    TimSQL.AddDayst = DateTime.Now.Date.AddDays(1).ToString("yyy-MM-dd");

                    return true;
                }
                catch (SqlException)
                {
                    Help.Mesag("Error connecting to SQL Server.");
                }
            
            return false;
        }



    
      static  public void SaveRow(DataGridView dataGridView1) {
           
            try
            {   DataRow Row;
                // Створення SQL-запиту, який вибирає дані з таблиці  value -"TablNema"
                string selectQuery = $"SELECT * FROM [{TablNema}]";

                // Створення SqlDataAdapter та встановлення SelectCommand
                SqlDataAdapter dataAdapter = new SqlDataAdapter(selectQuery, sqlConnection);

                // Визначення структури таблиці у DataSet
                DataSet dataSet = new DataSet();

                sqlBilder = new SqlCommandBuilder(dataAdapter);
                sqlBilder.GetInsertCommand();
                sqlBilder.GetUpdateCommand();
                sqlBilder.GetDeleteCommand();
                //
                //dataAdapter.FillSchema(dataSet, SchemaType.Source);
                dataAdapter.Fill(dataSet, TablNema);

                // Отримання таблиці "USER" з DataSet
                DataTable userTable = dataSet.Tables[TablNema];

                // Отримання останнього рядка з DataGridView
                DataGridViewRow lastRow = dataGridView1.Rows[dataGridView1.Rows.Count - 2];
            
              // Створення рядка для таблиці "USER"
                Row = userTable.NewRow();

                // Операція foreach для кожного рядка в DataGridView
                foreach (var dataRow in СolumnNames) {
                 Row[$"{dataRow}"] = lastRow.Cells[$"{dataRow}"].Value;
                }

                // Додавання рядка до таблиці value -"TablNema" в DataSet
                userTable.Rows.Add(Row);

                // Оновлення бази даних за допомогою SqlDataAdapter
                dataAdapter.Update(dataSet, TablNema);
            }
            catch (Exception ex)
            {
                // Обробка помилок, наприклад, виведення їх у консоль або журнал
                Console.WriteLine("Error: " + ex.Message);
            }

            //UpdateGridSet(false);

        }




        static public void DeleteRow(DataGridView dataGridView1) {

        var result = MessageBox.Show("Do you want delete select 'Row' ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes) {
                try{
                    int Idx = dataGridView1.SelectedRows[0].Index;
        dataGridView1.Rows.RemoveAt(Idx);
                    dataSet.Tables[TablNema].Rows[Idx].Delete();//  .RemoveAt(Idx);
        sqlDataAdapter.Update(dataSet, TablNema);
                }catch { Help.Mesag("You need to select a 'Row' to delete"); }
            }

 }
        ///----------------------------------------------------------------------------------------





        static  public  void DataGridNames(DataGridView dataGridView)
        {
            // Очистити всі існуючі стовці в dataGridView
            dataGridView.Columns.Clear();
            dataGridView.ReadOnly = true;
            // Пройтися по списку імен стовців і додати їх до dataGridView
            foreach (string columnName in СolumnNames)
            {
                // Додати стовець
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.Name = columnName;
                column.HeaderText = columnName;
                dataGridView.Columns.Add(column);
            }

            // Встановити ширину першої колонки вдвічі більше
            if (dataGridView.Columns.Count > 0)
            {   
                dataGridView.Columns[0].Width = (int)((double)dataGridView.Columns[2].Width / 1.2);
                dataGridView.Columns[1].Width =   (int)( (double) dataGridView.Columns[0].Width * 1.4 );
                dataGridView.Columns[2].Width = (int)((double)dataGridView.Columns[1].Width / 1.7);

            }

            // визначаєм заголовкі таблиці
            for (int i = 3; i < dataGridView.Columns.Count; i++)
            {
                dataGridView.Columns[i].Width = (int)((double)dataGridView.Columns[i].Width / 1.7);
            }


        }

        static  private bool TableExists(SqlConnection connection, string tableName)
        {
            string query = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                return cmd.ExecuteScalar() != null;
            }

            // Ваша функція для перевірки наявності таблиці
        }

        static private void CreateUsersTable(SqlConnection connection)
        {
            // SQL-запит для створення таблиці "USER"
            // SQL-запит для створення таблиці "USER" зі списком стовпців
            string createTableQuery = $"CREATE TABLE [{TablNema}] (" +
                "ID INT PRIMARY KEY IDENTITY(1,1)," + // Приклад стовпця ID з автоінкрементом
                ")";

            using (SqlCommand cmd = new SqlCommand(createTableQuery, connection))
            {
                cmd.ExecuteNonQuery();
            }

            // Ваш код для створення таблиці
        }

        static private bool ColumnExists(SqlConnection connection, string tableName, string columnName)
        {
            string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                return cmd.ExecuteScalar() != null;
            }

            // Ваша функція для перевірки наявності стовпця
        }

        static private void AddColumn(SqlConnection connection, string tableName, string columnName, string columnType)
        {
            // SQL-запит для додавання стовпця
            string addColumnQuery = $"ALTER TABLE [{tableName}] ADD [{columnName}] {columnType}";

            using (SqlCommand cmd = new SqlCommand(addColumnQuery, connection))
            {
                cmd.ExecuteNonQuery();
            }

            // Ваш код для додавання стовпця
        }


        ///-------------------------------   XLS     ---------------------------------------------------------

      static public void XLSX_Save (RichTextBox richTextBox2 )
        {
            XLSX xLSX = new XLSX();
            XLSX.DTchart Report = new XLSX.DTchart();
            Report.DT = new XLSX.DT[dataSet.Tables[TablNema].Rows.Count];
            Report.Name = new string[GridData.Name.Length - 1];
            int idx = 0;
            int ix = 0;
            Report.PeriodFrom = TimSQL.Now;
            Report.PeriodTo = TimSQL.AddDayst;

            // визначаєм заголовкі таблиці
            for (int i = 1; i <= GridData.Name.Length - 1; i++)
            {
                Report.Name[idx] = GridData.Name[i].ToString();
                idx++;
            }

            idx = 0;
            ix = 0;

            // заповняєм даними таблиці
            foreach (DataRow item in dataSet.Tables[TablNema].Rows)
            {
                Report.DT[idx] = new XLSX.DT();
                Report.DT[idx].Value = new string[item.ItemArray.Length - 1];
                ix = 0;

                for (int i = 1; i <= item.ItemArray.Length - 1; i++)
                {
                    Report.DT[idx].Value[ix] = "";
                    Report.DT[idx].Value[ix] = item.ItemArray[i].ToString();
                    ix++;
                }
                idx++;
            }
            xLSX.CreateXlsx(Report, richTextBox2.Text);
        }




    }
}
