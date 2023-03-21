using Microsoft.Data.Sqlite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class YearHistoryForm : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        int year;

        DataGridView table;

        private void YearHistoryForm_Load(object? sender, EventArgs e)
        {
            year = DateTime.Today.Year;

            this.table = new DataGridView();

            this.table.Location = new Point(5, 5);
            this.table.AutoSize = true;
            this.table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.table.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.table.AllowUserToAddRows = false;
            this.table.AllowUserToDeleteRows = false;
            this.table.AllowUserToResizeRows = false;
            this.table.AllowUserToResizeColumns = false;
            this.table.AllowUserToOrderColumns = false;
            this.table.AllowDrop = false;
            this.table.ReadOnly = true;
            this.table.BackgroundColor = this.BackColor;
            this.table.BorderStyle = BorderStyle.None;

            this.table.Columns.Add("month", "Месяц");
            this.table.Columns.Add("hire", "Нанято врачей");
            this.table.Columns.Add("fire", "Уволено врачей");
            this.table.Columns.Add("admiss", "Поступило пациентов");
            this.table.Columns.Add("cure", "Вылечено пациентов");
            this.table.Columns.Add("die", "Умерло пациентов");

            this.table.Rows.Add(12);

            this.table.Rows[0].Cells["month"].Value = "Январь";
            this.table.Rows[1].Cells["month"].Value = "Февраль";
            this.table.Rows[2].Cells["month"].Value = "Март";
            this.table.Rows[3].Cells["month"].Value = "Апрель";
            this.table.Rows[4].Cells["month"].Value = "Май";
            this.table.Rows[5].Cells["month"].Value = "Июнь";
            this.table.Rows[6].Cells["month"].Value = "Июль";
            this.table.Rows[7].Cells["month"].Value = "Август";
            this.table.Rows[8].Cells["month"].Value = "Сентябрь";
            this.table.Rows[9].Cells["month"].Value = "Октябрь";
            this.table.Rows[10].Cells["month"].Value = "Ноябрь";
            this.table.Rows[11].Cells["month"].Value = "Декабрь";

            for (int i = 0; i < 12; ++i)
            {
                this.table.Rows[i].Cells["hire"].Value = 0;
                this.table.Rows[i].Cells["fire"].Value = 0;
                this.table.Rows[i].Cells["admiss"].Value = 0;
                this.table.Rows[i].Cells["cure"].Value = 0;
                this.table.Rows[i].Cells["die"].Value = 0;
            }

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var docCommand = connection.CreateCommand();

                docCommand.Connection = connection;
                docCommand.CommandText = $"SELECT HireDate, FireDate FROM Doctor";

                using (var docReader = docCommand.ExecuteReader())
                {
                    if (docReader.HasRows)
                    {
                        string hire;
                        string fire;
                        int index;
                        int value;
                        while (docReader.Read())
                        {
                            hire = docReader.GetString(0);
                            
                            if (DateTime.Parse(hire).Year == year)
                            {
                                index = DateTime.Parse(hire).Month - 1;
                                value = int.Parse(this.table.Rows[index].Cells["hire"].Value.ToString());
                                this.table.Rows[index].Cells["hire"].Value = value + 1;
                            }

                            if (!docReader.IsDBNull(1))
                            {
                                fire = docReader.GetString(1);
                                if (DateTime.Parse(fire).Year == year)
                                {
                                    index = DateTime.Parse(fire).Month - 1;
                                    value = int.Parse(this.table.Rows[index].Cells["fire"].Value.ToString());
                                    this.table.Rows[index].Cells["fire"].Value = value + 1;
                                }
                            }
                        }
                    }
                }

                var patAccCommnad = connection.CreateCommand();

                patAccCommnad.Connection = connection;
                patAccCommnad.CommandText = $"SELECT AdmissionDate, DischargeDate, State FROM PatientAccounting";

                using (var patAccReader = patAccCommnad.ExecuteReader())
                {
                    if (patAccReader.HasRows)
                    {
                        string admiss;
                        string discharge;
                        string state = string.Empty;
                        int index;
                        int value;
                        while (patAccReader.Read())
                        {
                            admiss = patAccReader.GetString(0);
                            state = patAccReader.GetString(2);

                            if (DateTime.Parse(admiss).Year == year)
                            {
                                index = DateTime.Parse(admiss).Month - 1;
                                value = int.Parse(this.table.Rows[index].Cells["admiss"].Value.ToString());
                                this.table.Rows[index].Cells["admiss"].Value = value + 1;
                            }

                            if (string.Equals(state, "Здоров"))
                            {
                                if (!patAccReader.IsDBNull(1))
                                {
                                    discharge = patAccReader.GetString(1);

                                    if (DateTime.Parse(discharge).Year == year)
                                    {
                                        index = DateTime.Parse(discharge).Month - 1;
                                        value = int.Parse(this.table.Rows[index].Cells["cure"].Value.ToString());
                                        this.table.Rows[index].Cells["cure"].Value = value + 1;
                                    }
                                }
                            }

                            if (string.Equals(state, "Умер"))
                            {
                                if (!patAccReader.IsDBNull(1))
                                {
                                    discharge = patAccReader.GetString(1);

                                    if (DateTime.Parse(discharge).Year == year)
                                    {
                                        index = DateTime.Parse(discharge).Month - 1;
                                        value = int.Parse(this.table.Rows[index].Cells["die"].Value.ToString());
                                        this.table.Rows[index].Cells["die"].Value = value + 1;
                                    }
                                }
                            }
                        }
                    }
                }

                connection.Close();
            }

            this.Controls.Add(table);
        }

        public YearHistoryForm()
        {
            this.Load += YearHistoryForm_Load;

            InitializeComponent();
        }
    }
}
