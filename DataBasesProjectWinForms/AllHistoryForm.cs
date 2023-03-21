using Microsoft.Data.Sqlite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class AllHistoryForm : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private int year;

        private TabControl tabControl;
        private TabPage[] pages;

        private DataGridView[] table;

        private void Tab_Leave(object? sender, EventArgs e)
        {
            int i = this.tabControl.SelectedIndex;

            this.table[i].Rows.Clear();
        }

        private void Tab_Enter(object? sender, EventArgs e)
        {
            int i = this.tabControl.SelectedIndex;

            this.table[i].Rows.Add(12);

            this.table[i].Rows[0].Cells["month"].Value = "Январь";
            this.table[i].Rows[1].Cells["month"].Value = "Февраль";
            this.table[i].Rows[2].Cells["month"].Value = "Март";
            this.table[i].Rows[3].Cells["month"].Value = "Апрель";
            this.table[i].Rows[4].Cells["month"].Value = "Май";
            this.table[i].Rows[5].Cells["month"].Value = "Июнь";
            this.table[i].Rows[6].Cells["month"].Value = "Июль";
            this.table[i].Rows[7].Cells["month"].Value = "Август";
            this.table[i].Rows[8].Cells["month"].Value = "Сентябрь";
            this.table[i].Rows[9].Cells["month"].Value = "Октябрь";
            this.table[i].Rows[10].Cells["month"].Value = "Ноябрь";
            this.table[i].Rows[11].Cells["month"].Value = "Декабрь";

            for (int j = 0; j < 12; ++j)
            {
                this.table[i].Rows[j].Cells["hire"].Value = 0;
                this.table[i].Rows[j].Cells["fire"].Value = 0;
                this.table[i].Rows[j].Cells["admiss"].Value = 0;
                this.table[i].Rows[j].Cells["cure"].Value = 0;
                this.table[i].Rows[j].Cells["die"].Value = 0;
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

                            index = DateTime.Parse(hire).Month - 1;
                            value = int.Parse(this.table[i].Rows[index].Cells["hire"].Value.ToString());
                            this.table[i].Rows[index].Cells["hire"].Value = value + 1;

                            if (!docReader.IsDBNull(1))
                            {
                                fire = docReader.GetString(1);

                                index = DateTime.Parse(fire).Month - 1;
                                value = int.Parse(this.table[i].Rows[index].Cells["fire"].Value.ToString());
                                this.table[i].Rows[index].Cells["fire"].Value = value + 1;
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

                            index = DateTime.Parse(admiss).Month - 1;
                            value = int.Parse(this.table[i].Rows[index].Cells["admiss"].Value.ToString());
                            this.table[i].Rows[index].Cells["admiss"].Value = value + 1;

                            if (string.Equals(state, "Здоров"))
                            {
                                if (!patAccReader.IsDBNull(1))
                                {
                                    discharge = patAccReader.GetString(1);

                                    index = DateTime.Parse(discharge).Month - 1;
                                    value = int.Parse(this.table[i].Rows[index].Cells["cure"].Value.ToString());
                                    this.table[i].Rows[index].Cells["cure"].Value = value + 1;
                                }
                            }

                            if (string.Equals(state, "Умер"))
                            {
                                if (!patAccReader.IsDBNull(1))
                                {
                                    discharge = patAccReader.GetString(1);

                                    index = DateTime.Parse(discharge).Month - 1;
                                    value = int.Parse(this.table[i].Rows[index].Cells["die"].Value.ToString());
                                    this.table[i].Rows[index].Cells["die"].Value = value + 1;
                                }
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        private void AllHistoryForm_Load(object? sender, EventArgs e)
        {
            year = 2021;

            int thisYear = DateTime.Today.Year;
            table = new DataGridView[thisYear - year + 1];
            pages = new TabPage[thisYear - year + 1];

            for (int i = 0; i < thisYear - year + 1; i++)
            {
                this.pages[i] = new TabPage();
                this.pages[i].Text = (year + i).ToString();

                this.pages[i].Enter += Tab_Enter;
                this.pages[i].Leave += Tab_Leave;

                this.table[i] = new DataGridView();

                this.table[i].Location = new Point(5, 5);
                this.table[i].AutoSize = true;
                this.table[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                this.table[i].AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
                this.table[i].AllowUserToAddRows = false;
                this.table[i].AllowUserToDeleteRows = false;
                this.table[i].AllowUserToResizeRows = false;
                this.table[i].AllowUserToResizeColumns = false;
                this.table[i].AllowUserToOrderColumns = false;
                this.table[i].AllowDrop = false;
                this.table[i].ReadOnly = true;
                this.table[i].BackgroundColor = this.BackColor;
                this.table[i].BorderStyle = BorderStyle.None;

                this.table[i].Columns.Add("month", "Месяц");
                this.table[i].Columns.Add("hire", "Нанято врачей");
                this.table[i].Columns.Add("fire", "Уволено врачей");
                this.table[i].Columns.Add("admiss", "Поступило пациентов");
                this.table[i].Columns.Add("cure", "Вылечено пациентов");
                this.table[i].Columns.Add("die", "Умерло пациентов");
            }

            for (int i = 0; i < thisYear - year + 1; ++i)
            {
                this.pages[i].Controls.Add(this.table[i]);
                this.tabControl.TabPages.Add(this.pages[i]);
            }
        }

        public AllHistoryForm()
        {
            this.Load += AllHistoryForm_Load;

            InitializeComponent();
        }
    }
}
