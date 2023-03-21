using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class DiseaseFreqForm : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private DateTime start;
        private DateTime end;

        private DataGridView disTable;

        private void DiseaseFreqForm_Load(object? sender, EventArgs e)
        {
            this.disTable = new DataGridView();

            this.disTable.Location = new Point(5, 5);
            this.disTable.AutoSize = true;
            this.disTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.disTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.disTable.AllowUserToAddRows = false;
            this.disTable.AllowUserToDeleteRows = false;
            this.disTable.AllowUserToResizeRows = false;
            this.disTable.AllowUserToResizeColumns = false;
            this.disTable.AllowUserToOrderColumns = false;
            this.disTable.AllowDrop = false;
            this.disTable.ReadOnly = true;
            this.disTable.BackgroundColor = this.BackColor;
            this.disTable.BorderStyle = BorderStyle.None;

            this.disTable.Columns.Add("diagId", "diagId");
            this.disTable.Columns["diagId"].Visible = false;

            this.disTable.Columns.Add("diagName", "Диагноз");

            this.disTable.Columns.Add("sick", "Заболело");
            this.disTable.Columns.Add("well", "Выздоровело");
            this.disTable.Columns.Add("die", "Умерло");

            Dictionary<int, string> diag = new Dictionary<int, string>();
            Dictionary<int, int[]> stat = new Dictionary<int, int[]>();

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var diseaseCommand = connection.CreateCommand();

                diseaseCommand.Connection = connection;
                diseaseCommand.CommandText = $"SELECT DiagnosisId, DiagnosisDate, State FROM Disease";

                using (var diseaseReader = diseaseCommand.ExecuteReader())
                {
                    if (diseaseReader.HasRows)
                    {
                        int diagId;
                        DateTime diagDate;
                        string state;
                        while (diseaseReader.Read())
                        {
                            diagId = diseaseReader.GetInt32(0);
                            diagDate = DateTime.Parse(diseaseReader.GetString(1));
                            state = diseaseReader.GetString(2);
                            if ((diagDate >= this.start) && (diagDate <= this.end))
                            {
                                if (!stat.ContainsKey(diagId))
                                {
                                    stat.Add(diagId, new int[3] { 1, 0, 0 });
                                    diag.Add(diagId, "");
                                }
                                else
                                {
                                    stat[diagId][0]++;
                                }

                                if (state == "Умер")
                                {
                                    stat[diagId][2]++;
                                }

                                if (state == "Здоров")
                                {
                                    stat[diagId][1]++;
                                }
                            }
                        }
                    }
                }

                var diagCommand = connection.CreateCommand();

                diagCommand.Connection = connection;
                diagCommand.CommandText = $"SELECT id, Name FROM Diagnosis";

                using (var diagReader = diagCommand.ExecuteReader())
                {
                    if (diagReader.HasRows)
                    {
                        while (diagReader.Read())
                        {
                            if (diag.ContainsKey(diagReader.GetInt32(0)))
                            {
                                diag[diagReader.GetInt32(0)] = diagReader.GetString(1);
                            }
                        }
                    }
                }

                connection.Close();
            }

            int index = 0;
            foreach (KeyValuePair<int, int[]> item in stat)
            {
                index = this.disTable.Rows.Add();

                this.disTable.Rows[index].Cells["diagId"].Value = item.Key;
                this.disTable.Rows[index].Cells["sick"].Value = item.Value[0];
                this.disTable.Rows[index].Cells["well"].Value = item.Value[1];
                this.disTable.Rows[index].Cells["die"].Value = item.Value[2];
            }

            for (int i = 0, id; i < this.disTable.Rows.Count; ++i)
            {
                id = int.Parse(this.disTable.Rows[i].Cells["diagId"].Value.ToString());
                if (diag.ContainsKey(id))
                {
                    this.disTable.Rows[i].Cells["diagName"].Value = diag[id];
                }
            }

            this.Controls.Add(disTable);
        }

        public DiseaseFreqForm(string start, string end)
        {
            this.Load += DiseaseFreqForm_Load;
            this.start = DateTime.Parse(start);
            this.end = DateTime.Parse(end);

            InitializeComponent();
        }
    }
}
