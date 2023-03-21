using Microsoft.Data.Sqlite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class ProceduresForm : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        public PatientInfo parent;
        public int diseaseId;
        private Size normalSize;

        private Button addProcButton;

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

        public void ReloadProcsTable()
        {
            if (this.procsTable.Rows.Count > 0)
            {
                this.procsTable.Rows.Clear();
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

                SqliteCommand realProcCommand = connection.CreateCommand();
                realProcCommand.Connection = connection;
                realProcCommand.CommandText = $"SELECT id, ProcedureId, TotalAmount, AppointmentDate, CancellationDate, State " +
                                             $"FROM RealProcedureTreatment WHERE DiseaseId={diseaseId}";

                using (var realProcReader = realProcCommand.ExecuteReader())
                {
                    if (realProcReader.HasRows)
                    {
                        int index;

                        while (realProcReader.Read())
                        {
                            index = this.procsTable.Rows.Add();

                            this.procsTable.Rows[index].Cells["procRealId"].Value = realProcReader.GetInt32(0);
                            this.procsTable.Rows[index].Cells["procedureId"].Value = realProcReader.GetString(1);
                            this.procsTable.Rows[index].Cells["totalAmount"].Value = realProcReader.GetString(2);
                            this.procsTable.Rows[index].Cells["appointmentDate"].Value = realProcReader.GetString(3);
                            this.procsTable.Rows[index].Cells["state"].Value = realProcReader.GetString(5);
                            if (this.procsTable.Rows[index].Cells["state"].Value.ToString() == "Отменено")
                            {
                                this.procsTable.Rows[index].Cells["state"].ToolTipText = $"Отменено {realProcReader.GetString(4)}";
                            }
                        }
                    }
                }

                SqliteCommand procsCommand = connection.CreateCommand();
                procsCommand.Connection = connection;
                procsCommand.CommandText = $"SELECT id, Name FROM Procedure";

                using (var procsReader = procsCommand.ExecuteReader())
                {
                    if (procsReader.HasRows)
                    {
                        while (procsReader.Read())
                        {
                            int medId = procsReader.GetInt32(0);
                            string medName = procsReader.GetString(1);

                            for (int i = 0; i < this.procsTable.Rows.Count; i++)
                            {
                                if (medId == int.Parse(this.procsTable.Rows[i].Cells["procedureId"].Value.ToString()))
                                {
                                    this.procsTable.Rows[i].Cells["procedureName"].Value = medName;
                                }
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        private void ProceduresForm_Load(object? sender, EventArgs e)
        {
            this.Resize += ProceduresForm_Resize;

            #region DataGridView.ProceduresTable

            this.procsTable = new DataGridView();

            this.procsTable.CellDoubleClick += ProcsTable_CellDoubleClick;

            this.procsTable.Location = new Point(5, 5);
            this.procsTable.AutoSize = true;
            this.procsTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.procsTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.procsTable.AllowUserToAddRows = false;
            this.procsTable.AllowUserToDeleteRows = false;
            this.procsTable.AllowUserToResizeRows = false;
            this.procsTable.AllowUserToResizeColumns = false;
            this.procsTable.AllowUserToOrderColumns = false;
            this.procsTable.AllowDrop = false;
            this.procsTable.ReadOnly = true;
            this.procsTable.BackgroundColor = this.BackColor;
            this.procsTable.BorderStyle = BorderStyle.None;

            this.procsTable.Columns.Add("procRealId", "procRealId");

            this.procsTable.Columns.Add("procedureName", "Название процедуры");

            this.procsTable.Columns.Add("procedureId", "medicineId");
            this.procsTable.Columns["procedureId"].Visible = false;

            this.procsTable.Columns.Add("totalAmount", "Количество приёмов");
            this.procsTable.Columns.Add("appointmentDate", "Дата назначения");
            this.procsTable.Columns.Add("state", "Статус");

            this.ReloadProcsTable();

            #endregion

            this.addProcButton = new Button();
            this.addProcButton.Text = "Назначить процедуру";
            this.addProcButton.BackColor = Color.White;
            this.addProcButton.ForeColor = Color.Black;
            this.addProcButton.AutoSize = true;
            this.addProcButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.addProcButton.HandleCreated += AddProcButton_HandleCreated;
            this.addProcButton.Click += AddProcButton_Click;

            this.Controls.Add(this.procsTable);
            this.Controls.Add(this.addProcButton);
        }

        private void ProcsTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == this.procsTable.Columns["totalAmount"].Index)
            {
                if (this.procsTable.Rows[e.RowIndex].Cells["state"].Value.ToString().Contains("Назначено"))
                {
                    ChangeAmount changeAmount = new ChangeAmount(this, int.Parse(this.procsTable.Rows[e.RowIndex].Cells["procRealId"].Value.ToString()));
                    changeAmount.Show();
                }
            }

            if (e.ColumnIndex == this.procsTable.Columns["state"].Index)
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

                        var procRealCommand = connection.CreateCommand();
                        procRealCommand.Connection = connection;
                        procRealCommand.CommandText = $"UPDATE RealProcedureTreatment SET " +
                                                     $"CancellationDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                     $"State='Отменено' " +
                                                     $"WHERE id={this.procsTable.Rows[e.RowIndex].Cells["procRealId"].Value.ToString()}";

                        procRealCommand.ExecuteNonQuery();

                        connection.Close();
                    }

                    this.ReloadProcsTable();
                    this.parent.ReloadDiseaseTable();
                }
            }
        }

        private void AddProcButton_Click(object? sender, EventArgs e)
        {
            AddProcedure addProcedure = new AddProcedure(this.parent, diseaseId);
            addProcedure.Show();
        }

        private void AddProcButton_HandleCreated(object? sender, EventArgs e)
        {
            this.addProcButton.Location = new Point(this.procsTable.Width + this.procsTable.Location.X + 5, this.procsTable.Location.Y);
        }

        private void ProceduresForm_Resize(object? sender, EventArgs e)
        {
            normalSize = this.Size;
        }

        public ProceduresForm(PatientInfo parent, int diseaseId)
        {
            this.parent = parent;
            this.diseaseId = diseaseId;
            this.Load += ProceduresForm_Load;

            InitializeComponent();
        }
    }
}
