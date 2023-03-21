using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class BestDocsForm : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        DataGridView docsTable;

        private void BestDocsForm_Load(object? sender, EventArgs e)
        {
            this.docsTable = new DataGridView();

            this.docsTable.Location = new Point(5, 5);
            this.docsTable.AutoSize = true;
            this.docsTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.docsTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.docsTable.AllowUserToAddRows = false;
            this.docsTable.AllowUserToDeleteRows = false;
            this.docsTable.AllowUserToResizeRows = false;
            this.docsTable.AllowUserToResizeColumns = false;
            this.docsTable.AllowUserToOrderColumns = false;
            this.docsTable.AllowDrop = false;
            this.docsTable.ReadOnly = true;
            this.docsTable.BackgroundColor = this.BackColor;
            this.docsTable.BorderStyle = BorderStyle.None;

            this.docsTable.Columns.Add("docId", "docId");
            this.docsTable.Columns["docId"].Visible = false;

            this.docsTable.Columns.Add("name", "Имя");
            this.docsTable.Columns.Add("surname", "Фамилия");

            this.docsTable.Columns.Add("dep", "Отделение");

            this.docsTable.Columns.Add("depId", "depId");
            this.docsTable.Columns["depId"].Visible = false;

            this.docsTable.Columns.Add("deaths", "Показатель ценности");
            this.docsTable.Columns["deaths"].ToolTipText = "Ценность = (Вылеченные - Умершие) / Время работы";

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                Dictionary<int, int> deaths = new Dictionary<int, int>();
                Dictionary<int, string> deps = new Dictionary<int, string>();
                Dictionary<int, int> all = new Dictionary<int, int>();
                Dictionary<int, int> days = new Dictionary<int, int>();

                var docCommand = connection.CreateCommand();

                docCommand.Connection = connection;
                docCommand.CommandText = "SELECT id, Name, Surname, DepartmentId, HireDate FROM Doctor WHERE FireDate IS NULL";

                using (var docReader = docCommand.ExecuteReader())
                {
                    int index = 0;

                    if (docReader.HasRows)
                    {
                        while (docReader.Read())
                        {
                            index = this.docsTable.Rows.Add();

                            this.docsTable.Rows[index].Cells["docId"].Value = docReader.GetInt32(0);
                            this.docsTable.Rows[index].Cells["name"].Value = docReader.GetString(1);
                            this.docsTable.Rows[index].Cells["surname"].Value = docReader.GetString(2);
                            this.docsTable.Rows[index].Cells["depId"].Value = docReader.GetInt32(3);
                            deaths.Add(docReader.GetInt32(0), 0);
                            all.Add(docReader.GetInt32(0), 0);
                            if (!deps.ContainsKey(docReader.GetInt32(3)))
                            {
                                deps.Add(docReader.GetInt32(3), "");
                            }

                            if (!days.ContainsKey(docReader.GetInt32(0)))
                            {
                                DateTime hireDate = DateTime.Parse(docReader.GetString(4)).Date;
                                days.Add(docReader.GetInt32(0), (DateTime.Today.Date - hireDate).Days);
                            }
                        }
                    }
                    else
                    {
                        connection.Close();
                        return;
                    }
                }

                var depCommand = connection.CreateCommand();

                depCommand.Connection = connection;
                depCommand.CommandText = $"SELECT id, Name FROM Department";

                using (var depReader = depCommand.ExecuteReader())
                {
                    if (depReader.HasRows)
                    {
                        while (depReader.Read())
                        {
                            if (deps.ContainsKey(depReader.GetInt32(0)))
                            {
                                deps[depReader.GetInt32(0)] = depReader.GetString(1);
                            }
                        }
                    }
                }

                for (int i = 0, id; i < this.docsTable.Rows.Count; ++i)
                {
                    id = int.Parse(this.docsTable.Rows[i].Cells["depId"].Value.ToString());
                    if (deps.ContainsKey(id))
                    {
                        this.docsTable.Rows[i].Cells["dep"].Value = deps[id];
                    }
                }

                var diseaseCommand = connection.CreateCommand();

                diseaseCommand.Connection = connection;
                diseaseCommand.CommandText = "SELECT DoctorId FROM Disease WHERE State='Умер'";

                using (var diseaseReader = diseaseCommand.ExecuteReader())
                {
                    if (diseaseReader.HasRows)
                    {
                        while (diseaseReader.Read())
                        {
                            if (deaths.ContainsKey(diseaseReader.GetInt32(0)))
                            {
                                deaths[diseaseReader.GetInt32(0)]++;
                            }
                        }
                    }
                }

                diseaseCommand.CommandText = $"SELECT DoctorId FROM Disease WHERE State='Здоров' OR State='Умер'";

                using (var diseaseReader = diseaseCommand.ExecuteReader())
                {
                    if (diseaseReader.HasRows)
                    {
                        while (diseaseReader.Read())
                        {
                            if (all.ContainsKey(diseaseReader.GetInt32(0)))
                            {
                                all[diseaseReader.GetInt32(0)]++;
                            }
                        }
                    }
                }

                for (int i = 0, id = 0; i < this.docsTable.Rows.Count; ++i)
                {
                    id = int.Parse(this.docsTable.Rows[i].Cells["docId"].Value.ToString());
                    if (deaths.ContainsKey(id) && all.ContainsKey(id) && days.ContainsKey(id))
                    {
                        this.docsTable.Rows[i].Cells["deaths"].Value = (double)(all[id] - 2 * deaths[id]) / days[id];
                    }
                }

                connection.Close();
            }

            this.Controls.Add(docsTable);
        }

        public BestDocsForm()
        {
            this.Load += BestDocsForm_Load;

            InitializeComponent();
        }
    }
}
