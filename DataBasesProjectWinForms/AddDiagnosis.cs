using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class AddDiagnosis : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private PatientInfo parent;
        private int patientId;

        private ComboBox docName;
        private ComboBox depName;
        private ComboBox diagName;
        private ComboBox state;

        private Button applyButton;

        private Label docNameLabel;
        private Label depNameLabel;
        private Label diagNameLabel;
        private Label stateLabel;

        private Dictionary<int, string> departments;
        private Dictionary<int, string> doctors;
        private Dictionary<int, string> diagnosis;

        private void AddDiagnosis_Load(object? sender, EventArgs e)
        {
            this.departments = new Dictionary<int, string>();
            this.doctors = new Dictionary<int, string>();
            this.diagnosis = new Dictionary<int, string>();

            this.depName = new ComboBox();

            this.depName.SelectedIndexChanged += DepName_SelectedIndexChanged;

            this.depName.Location = new Point(50, 50);
            this.depName.Size = new Size(200, 20);
            this.depName.BackColor = Color.White;
            this.depName.ForeColor = Color.Black;
            this.depName.DropDownStyle = ComboBoxStyle.DropDownList;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var depCommand = connection.CreateCommand();
                depCommand.Connection = connection;
                depCommand.CommandText = "SELECT id, Name FROM Department WHERE FreeBeds>0";

                using (var depReader = depCommand.ExecuteReader())
                {
                    if (depReader.HasRows)
                    {
                        while (depReader.Read())
                        {
                            this.depName.Items.Add(depReader.GetString(1));
                            departments.Add(depReader.GetInt32(0), depReader.GetString(1));
                        }
                    }
                    else
                    {
                        connection.Close();
                        return;
                    }
                }

                connection.Close();
            }

            this.docName = new ComboBox();
            this.docName.Location = new Point(50 + 200 + 15, 50);
            this.docName.Size = new Size(200, 20);
            this.docName.BackColor = Color.White;
            this.docName.ForeColor = Color.Black;
            this.docName.DropDownStyle = ComboBoxStyle.DropDownList;

            this.diagName = new ComboBox();
            this.diagName.Location = new Point(50 + 200 + 15 + 200 + 15, 50);
            this.diagName.Size = new Size(200, 20);
            this.diagName.BackColor = Color.White;
            this.diagName.ForeColor = Color.Black;
            this.diagName.DropDownStyle = ComboBoxStyle.DropDownList;

            /*this.state = new ComboBox();
            this.state.Location = new Point(50, 50 + 20 + 50);
            this.state.Size = new Size(200, 20);
            this.state.BackColor = Color.White;
            this.state.ForeColor = Color.Black;
            this.state.DropDownStyle = ComboBoxStyle.DropDownList;

            this.state.Items.Add("Болен");
            this.state.Items.Add("Умер по другой болезни");
            this.state*/

            this.docNameLabel = new Label();
            this.docNameLabel.Text = "Лечащий врач:";
            this.docNameLabel.AutoSize = true;
            this.docNameLabel.HandleCreated += DocNameLabel_HandleCreated;

            this.depNameLabel = new Label();
            this.depNameLabel.Text = "Отделение";
            this.depNameLabel.AutoSize = true;
            this.depNameLabel.HandleCreated += DepNameLabel_HandleCreated;

            this.diagNameLabel = new Label();
            this.diagNameLabel.Text = "Диагноз";
            this.diagNameLabel.AutoSize = true;
            this.diagNameLabel.HandleCreated += DiagNameLabel_HandleCreated;

            /*this.stateLabel = new Label();
            this.stateLabel.Text = "Состояние";
            this.stateLabel.AutoSize = true;
            this.stateLabel.HandleCreated += StateLabel_HandleCreated;*/

            this.applyButton = new Button();
            this.applyButton.Text = "Принять";
            this.applyButton.BackColor = Color.White;
            this.applyButton.ForeColor = Color.Black;
            this.applyButton.AutoSize = true;
            this.applyButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.applyButton.HandleCreated += ApplyButton_HandleCreated;
            this.applyButton.Click += ApplyButton_Click;

            this.Controls.Add(depName);
            this.Controls.Add(docName);
            this.Controls.Add(diagName);
            this.Controls.Add(state);
            this.Controls.Add(depNameLabel);
            this.Controls.Add(docNameLabel);
            this.Controls.Add(diagNameLabel);
            //this.Controls.Add(stateLabel);
            this.Controls.Add(applyButton);
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            string depName = this.depName.SelectedItem?.ToString() ?? string.Empty;
            string docName = this.docName.SelectedItem?.ToString() ?? string.Empty;
            string diagName = this.diagName.SelectedItem?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(depName) || string.IsNullOrWhiteSpace(docName) || string.IsNullOrWhiteSpace(diagName))
            {
                return;
            }

            int depId = -1;
            int docId = -1;
            int diagId = -1;

            foreach (KeyValuePair<int, string> item in departments)
            {
                if (string.Equals(item.Value, depName))
                {
                    depId = item.Key;
                }
            }

            if (depId == -1)
            {
                return;
            }

            foreach (KeyValuePair<int, string> item in doctors)
            {
                if (string.Equals(item.Value, docName))
                {
                    docId = item.Key;
                }
            }

            if (docId == -1)
            {
                return;
            }

            foreach (KeyValuePair<int, string> item in diagnosis)
            {
                if (string.Equals(item.Value, diagName))
                {
                    diagId = item.Key;
                }
            }

            if (diagId == -1)
            {
                return;
            }

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var diseaseCommand = connection.CreateCommand();
                diseaseCommand.Connection = connection;
                diseaseCommand.CommandText = $"SELECT id FROM Disease WHERE PatientId={patientId} AND " +
                                             $"DiagnosisId={diagId} AND " +
                                             $"(State='Болен' OR State='Умер' OR State='Умер по другой болезни')";

                using (var diseaseReader = diseaseCommand.ExecuteReader())
                {
                    if (diseaseReader.HasRows)
                    {
                        MessageBox.Show(
                            $"Не удалось добавить диагноз, так как \n" +
                            $"данный пациент уже болеет по нему \n" +
                            $"или умер",
                            $"Ошибка!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                            );

                        connection.Close();
                        return;
                    }
                }

                diseaseCommand.CommandText = $"INSERT INTO Disease (PatientId, DoctorId, DiagnosisId, DiagnosisDate, " +
                                             $"State) VALUES ({patientId}, {docId}, {diagId}, '{DateTime.Today.Date.ToShortDateString()}', " +
                                             $"'Болен')";

                diseaseCommand.ExecuteNonQuery();

                this.parent.ReloadDiseaseTable();
                this.Dispose();

                connection.Close();
            }
        }

        private void ApplyButton_HandleCreated(object? sender, EventArgs e)
        {
            this.applyButton.Location = new Point(50, 50 + 20 + 50);
        }

        private void StateLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.stateLabel.Location = new Point(this.state.Location.X + this.state.Size.Width / 2 - this.stateLabel.Width / 2, this.state.Location.Y - this.stateLabel.Size.Height);
        }

        private void DiagNameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.diagNameLabel.Location = new Point(this.diagName.Location.X + this.diagName.Size.Width / 2 - this.diagNameLabel.Width / 2, this.diagName.Location.Y - this.diagNameLabel.Size.Height);
        }

        private void DepNameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.depNameLabel.Location = new Point(this.depName.Location.X + this.depName.Size.Width / 2 - this.depNameLabel.Width / 2, this.depName.Location.Y - this.depNameLabel.Size.Height);
        }

        private void DocNameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.docNameLabel.Location = new Point(this.docName.Location.X + this.docName.Size.Width / 2 - this.docNameLabel.Width / 2, this.docName.Location.Y - this.docNameLabel.Size.Height);
        }

        private void DepName_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string depName = this.depName.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(depName))
            {
                return;
            }

            int depId = -1;
            foreach (KeyValuePair<int, string> item in departments)
            {
                if (string.Equals(item.Value, depName))
                {
                    depId = item.Key;
                }
            }

            if (depId == -1)
            {
                return;
            }

            docName.Items.Clear();
            diagName.Items.Clear();

            doctors.Clear();
            diagnosis.Clear();

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var docCommand = connection.CreateCommand();
                docCommand.Connection = connection;
                docCommand.CommandText = $"SELECT id, Name, Surname FROM Doctor WHERE DepartmentId={depId} AND " +
                                         $"FireDate IS NULL AND IsFiring=0";

                using (var docReader = docCommand.ExecuteReader())
                {
                    if (docReader.HasRows)
                    {
                        while (docReader.Read())
                        {
                            docName.Items.Add(docReader.GetString(2) + " " + docReader.GetString(1));
                            doctors.Add(docReader.GetInt32(0), docReader.GetString(2) + " " + docReader.GetString(1));
                        }
                    }
                }

                var diagCommand = connection.CreateCommand();
                diagCommand.Connection = connection;
                diagCommand.CommandText = $"SELECT id, Name FROM Diagnosis WHERE DepartmentId={depId}";

                using (var diagReader = diagCommand.ExecuteReader())
                {
                    if (diagReader.HasRows)
                    {
                        while (diagReader.Read())
                        {
                            diagName.Items.Add(diagReader.GetString(1));
                            diagnosis.Add(diagReader.GetInt32(0), diagReader.GetString(1));
                        }
                    }
                }

                connection.Close();
            }
        }

        public AddDiagnosis(PatientInfo parent, int patientId)
        {
            this.parent = parent;
            this.patientId = patientId;
            this.Load += AddDiagnosis_Load;

            InitializeComponent();
        }
    }
}
