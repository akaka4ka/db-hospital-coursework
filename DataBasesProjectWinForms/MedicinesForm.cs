using Microsoft.Data.Sqlite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class MedicinesForm : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        public PatientInfo parent;
        public int diseaseId;
        private Size normalSize;

        private DataGridView medsTable;

        private Button addMedButton;

        public MedicinesForm(PatientInfo parent, int diseaseId)
        {
            this.parent = parent;
            this.diseaseId = diseaseId;
            this.Load += MedicinesForm_Load;

            InitializeComponent();
        }

        public void SetInFocus()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Size = normalSize;
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.Hide();
            }

            this.Show();
        }

        public void ReloadMedsTable()
        {
            if (this.medsTable.Rows.Count > 0)
            {
                this.medsTable.Rows.Clear();
            }

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                SqliteCommand patCommand = connection.CreateCommand();

                patCommand.Connection = connection;
                patCommand.CommandText = $"SELECT Name, Surname FROM Patient WHERE id={this.parent.patientId}";

                using (var patReader = patCommand.ExecuteReader())
                {
                    if (patReader.HasRows)
                    {
                        patReader.Read();
                        if (!this.Text.Contains(patReader.GetString(1)))
                        {
                            this.Text = this.Text + $", {patReader.GetString(1)} {patReader.GetString(0)}";
                        }
                    }
                }

                SqliteCommand realMedCommand = connection.CreateCommand();
                realMedCommand.Connection = connection;
                realMedCommand.CommandText = $"SELECT id, MedicineId, IntakePerDay, AppointmentDate, CancellationDate, State " +
                                             $"FROM RealMedicineTreatment WHERE DiseaseId={diseaseId}";

                using (var realMedReader = realMedCommand.ExecuteReader())
                {
                    if (realMedReader.HasRows)
                    {
                        int index;

                        while (realMedReader.Read())
                        {
                            index = this.medsTable.Rows.Add();

                            this.medsTable.Rows[index].Cells["medRealId"].Value = realMedReader.GetInt32(0);
                            this.medsTable.Rows[index].Cells["medicineId"].Value = realMedReader.GetString(1);
                            this.medsTable.Rows[index].Cells["dailyDose"].Value = realMedReader.GetString(2);
                            this.medsTable.Rows[index].Cells["appointmentDate"].Value = realMedReader.GetString(3);
                            this.medsTable.Rows[index].Cells["state"].Value = realMedReader.GetString(5);
                            if (this.medsTable.Rows[index].Cells["state"].Value.ToString() == "Отменено")
                            {
                                this.medsTable.Rows[index].Cells["state"].ToolTipText = $"Отменено {realMedReader.GetString(4)}";
                            }
                        }
                    }
                }

                SqliteCommand medCommand = connection.CreateCommand();
                medCommand.Connection = connection;
                medCommand.CommandText = $"SELECT id, Name FROM Medicine";

                using (var medReader = medCommand.ExecuteReader())
                {
                    if (medReader.HasRows)
                    {
                        while (medReader.Read())
                        {
                            int medId = medReader.GetInt32(0);
                            string medName = medReader.GetString(1);

                            for (int i = 0; i < this.medsTable.Rows.Count; i++)
                            {
                                if (medId == int.Parse(this.medsTable.Rows[i].Cells["medicineId"].Value.ToString()))
                                {
                                    this.medsTable.Rows[i].Cells["medicineName"].Value = medName;
                                }
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        private void MedicinesForm_Resize(object? sender, EventArgs e)
        {
            normalSize = this.Size;
        }

        private void AddMedButton_Click(object? sender, EventArgs e)
        {
            AddMedicine addMedicine = new AddMedicine(this.parent, diseaseId);
            addMedicine.Show();
        }

        private void AddMedButton_HandleCreated(object? sender, EventArgs e)
        {
            this.addMedButton.Location = new Point(this.medsTable.Width + this.medsTable.Location.X + 5, this.medsTable.Location.Y);
        }

        private void MedicinesForm_Load(object? sender, EventArgs e)
        {
            this.Resize += MedicinesForm_Resize;

            #region DataGridView.MedicinesTable

            this.medsTable = new DataGridView();

            this.medsTable.CellDoubleClick += MedsTable_CellDoubleClick;

            this.medsTable.Location = new Point(5, 5);
            this.medsTable.AutoSize = true;
            this.medsTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.medsTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.medsTable.AllowUserToAddRows = false;
            this.medsTable.AllowUserToDeleteRows = false;
            this.medsTable.AllowUserToResizeRows = false;
            this.medsTable.AllowUserToResizeColumns = false;
            this.medsTable.AllowUserToOrderColumns = false;
            this.medsTable.AllowDrop = false;
            this.medsTable.ReadOnly = true;
            this.medsTable.BackgroundColor = this.BackColor;
            this.medsTable.BorderStyle = BorderStyle.None;

            this.medsTable.Columns.Add("medRealId", "medRealId");
            this.medsTable.Columns["medRealId"].Visible = false;

            this.medsTable.Columns.Add("medicineName", "Название лекарства");

            this.medsTable.Columns.Add("medicineId", "medicineId");
            this.medsTable.Columns["medicineId"].Visible = false;

            this.medsTable.Columns.Add("dailyDose", "Дневная доза (мг)");
            this.medsTable.Columns.Add("appointmentDate", "Дата назначения");
            this.medsTable.Columns.Add("state", "Статус");

            this.ReloadMedsTable();

            #endregion

            this.addMedButton = new Button();
            this.addMedButton.Text = "Назначить лекарство";
            this.addMedButton.BackColor = Color.White;
            this.addMedButton.ForeColor = Color.Black;
            this.addMedButton.AutoSize = true;
            this.addMedButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.addMedButton.HandleCreated += AddMedButton_HandleCreated;
            this.addMedButton.Click += AddMedButton_Click;

            this.Controls.Add(this.medsTable);
            this.Controls.Add(this.addMedButton);
        }

        private void MedsTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == this.medsTable.Columns["dailyDose"].Index)
            {
                if (this.medsTable.Rows[e.RowIndex].Cells["state"].Value.ToString().Contains("Назначено"))
                {
                    ChangeDose changeDose = new ChangeDose(this, int.Parse(this.medsTable.Rows[e.RowIndex].Cells["medRealId"].Value.ToString()));
                    changeDose.Show();
                }
            }
            
            if (e.ColumnIndex == this.medsTable.Columns["state"].Index)
            {
                var result = MessageBox.Show(
                                $"Вы действительно хотите отменить назначение?",
                                $"Ошибка!",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question
                                );

                if (result == DialogResult.Yes)
                {
                    using (var connection = new SqliteConnection(sqlConnectionString))
                    {
                        connection.Open();

                        var medRealCommand = connection.CreateCommand();
                        medRealCommand.Connection = connection;
                        medRealCommand.CommandText = $"UPDATE RealMedicineTreatment SET " +
                                                     $"CancellationDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                     $"State='Отменено' " +
                                                     $"WHERE id={this.medsTable.Rows[e.RowIndex].Cells["medRealId"].Value.ToString()}";

                        medRealCommand.ExecuteNonQuery();

                        connection.Close();
                    }

                    this.ReloadMedsTable();
                    this.parent.ReloadDiseaseTable();
                }
            }
        }
    }
}
