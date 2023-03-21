using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class AddMedicine : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private PatientInfo parent;
        private int diseaseId;

        private ComboBox medicine;
        private TextBox dosePerDay;

        private Label medLabel;
        private Label doseLabel;

        private Button applyButton;

        private void AddMedicine_Load(object? sender, EventArgs e)
        {
            this.medicine = new ComboBox();
            this.medicine.Location = new Point(50, 50);
            this.medicine.Size = new Size(200, 20);
            this.medicine.BackColor = Color.White;
            this.medicine.ForeColor = Color.Black;
            this.medicine.DropDownStyle = ComboBoxStyle.DropDownList;

            this.dosePerDay = new TextBox();
            this.dosePerDay.TextChanged += DosePerDay_TextChanged;
            this.dosePerDay.Location = new Point(this.medicine.Location.X + this.medicine.Size.Width + 25, this.medicine.Location.Y);
            this.dosePerDay.Size = new Size(200, 20);
            this.dosePerDay.BorderStyle = BorderStyle.FixedSingle;
            this.dosePerDay.BackColor = Color.White;
            this.dosePerDay.ForeColor = Color.Black;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var diseaseCommand = connection.CreateCommand();
                diseaseCommand.Connection = connection;
                diseaseCommand.CommandText = $"SELECT DiagnosisId FROM Disease WHERE id={diseaseId}";

                int diagId = -1;

                using (var diseaseReader = diseaseCommand.ExecuteReader())
                {
                    if (diseaseReader.HasRows)
                    {
                        diseaseReader.Read();
                        diagId = diseaseReader.GetInt32(0);
                    }
                }

                if (diagId == -1)
                {
                    connection.Close();
                    return;
                }

                var procTreatCommand = connection.CreateCommand();
                procTreatCommand.Connection = connection;
                procTreatCommand.CommandText = $"SELECT MedicineId FROM MedicineTreatment WHERE DiagnosisId={diagId}";
                List<int> procIds = new List<int>();
                using (var procTreatReader = procTreatCommand.ExecuteReader())
                {
                    if (procTreatReader.HasRows)
                    {
                        while (procTreatReader.Read())
                        {
                            procIds.Add(procTreatReader.GetInt32(0));
                        }
                    }
                }

                var procCommand = connection.CreateCommand();
                procCommand.Connection = connection;
                procCommand.CommandText = $"SELECT id, Name FROM Medicine";

                using (var procReader = procCommand.ExecuteReader())
                {
                    if (procReader.HasRows)
                    {
                        while (procReader.Read())
                        {
                            if (procIds.Contains(procReader.GetInt32(0)))
                            {
                                this.medicine.Items.Add(procReader.GetString(1));
                            }
                        }
                    }
                }

                connection.Close();
            }

            this.medLabel = new Label();
            this.medLabel.Text = "Лекарство:";
            this.medLabel.AutoSize = true;
            this.medLabel.HandleCreated += MedLabel_HandleCreated;

            this.doseLabel = new Label();
            this.doseLabel.Text = "Дневная доза (мг)";
            this.doseLabel.AutoSize = true;
            this.doseLabel.HandleCreated += DoseLabel_HandleCreated;

            this.applyButton = new Button();
            this.applyButton.Text = "Назначить";
            this.applyButton.BackColor = Color.White;
            this.applyButton.ForeColor = Color.Black;
            this.applyButton.AutoSize = true;
            this.applyButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.applyButton.HandleCreated += ApplyButton_HandleCreated;
            this.applyButton.Click += ApplyButton_Click;

            this.Controls.Add(medicine);
            this.Controls.Add(dosePerDay);
            this.Controls.Add(medLabel);
            this.Controls.Add(doseLabel);
            this.Controls.Add(applyButton);
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.dosePerDay.Text))
            {
                return;
            }

            string medName = this.medicine.SelectedItem?.ToString() ?? string.Empty;
            int dosePerDay = int.Parse(this.dosePerDay.Text);

            if (medName == null)
            {
                return;
            }

            int medId = -1;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var medCommand = connection.CreateCommand();
                medCommand.Connection = connection;
                medCommand.CommandText = $"SELECT id FROM Medicine WHERE Name='{medName}'";

                using (var medReader = medCommand.ExecuteReader())
                {
                    if (medReader.HasRows)
                    {
                        medReader.Read();
                        medId = medReader.GetInt32(0);
                    }
                }

                if (medId == -1)
                {
                    connection.Close();
                    return;
                }

                var medRealCommand = connection.CreateCommand();
                medRealCommand.Connection = connection;

                medRealCommand.CommandText = $"SELECT IntakePerDay FROM RealMedicineTreatment " +
                                             $"WHERE State='Назначено' AND MedicineId={medId} AND DiseaseId={diseaseId}";
                int curDose;
                using (var medRealReader = medRealCommand.ExecuteReader())
                {
                    if (medRealReader.HasRows)
                    {
                        medRealReader.Read();
                        curDose = medRealReader.GetInt32(0);
                        MessageBox.Show(
                            $"Не удалось назначить лекарство, так как \n" +
                            $"данному пациенту уже назначено такое лекарство \n" +
                            $"с дозой {curDose} (мг/день)",
                            $"Ошибка!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                            );
                        return;
                    }
                }

                medRealCommand.CommandText = $"INSERT INTO RealMedicineTreatment (DiseaseId, MedicineId, " +
                                              $"IntakePerDay, AppointmentDate, LastChangeDate, State) " +
                                              $"VALUES ({diseaseId}, {medId}, {dosePerDay}, " +
                                              $"'{DateTime.Today.Date.ToShortDateString()}', " +
                                              $"'{DateTime.Today.Date.ToShortDateString()}', 'Назначено')";

                medRealCommand.ExecuteNonQuery();

                connection.Close();
            }

            this.parent.ReloadDiseaseTable();
            this.Dispose();
        }

        private void ApplyButton_HandleCreated(object? sender, EventArgs e)
        {
            this.applyButton.Location = new Point(this.medicine.Location.X + this.medicine.Size.Width / 2, this.medicine.Location.Y + 45);
            this.applyButton.Size = new Size(215, 20);
        }

        private void DoseLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.doseLabel.Location = new Point(this.dosePerDay.Location.X + this.dosePerDay.Size.Width / 2 - this.doseLabel.Size.Width / 2, this.dosePerDay.Location.Y - this.doseLabel.Size.Height);
        }

        private void MedLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.medLabel.Location = new Point(this.medicine.Location.X + this.medicine.Size.Width / 2 - this.medLabel.Size.Width / 2, this.medicine.Location.Y - this.medLabel.Size.Height);
        }

        private void DosePerDay_TextChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.dosePerDay.Text))
            {
                this.dosePerDay.Text = Regex.Replace(this.dosePerDay.Text, @"[\sа-яА-ЯёЁa-zA-z\s]", "");
            }
        }

        public AddMedicine(PatientInfo parent, int diseaseId)
        {
            this.parent = parent;
            this.diseaseId = diseaseId;
            this.Load += AddMedicine_Load;

            InitializeComponent();
        }
    }
}
