using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace DataBasesProjectWinForms
{
    public partial class AddNewPatient : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private MainForm parent;

        private TextBox patName;
        private TextBox patSurname;
        private TextBox patAge;

        private Label patNameLabel;
        private Label patSurnameLabel;
        private Label patAgeLabel;
        private Label patStateLabel;
        private Label depNameLabel;
        private Label diseaseLabel;
        private Label doctorLabel;

        private ComboBox depName;
        private ComboBox patState;
        private ComboBox disease;
        private ComboBox doctor;

        private Button applyButton;

        private Dictionary<int, string> depNames;
        private Dictionary<int, string> doctors;

        public AddNewPatient(MainForm parent)
        {
            this.parent = parent;

            this.Load += Form3_Load;

            InitializeComponent();
        }

        private void Form3_Load(object? sender, EventArgs e)
        {
            depNames = new Dictionary<int, string>();
            doctors = new Dictionary<int, string>();

            patName = new TextBox();
            this.patName.Location = new Point(50, 50);
            this.patName.Size = new Size(200, 20);
            this.patName.BorderStyle = BorderStyle.FixedSingle;
            this.patName.BackColor = Color.White;
            this.patName.ForeColor = Color.Black;

            this.patSurname = new TextBox();
            this.patSurname.Location = new Point(patName.Location.X + patName.Size.Width + 15, 50);
            this.patSurname.Size = new Size(200, 20);
            this.patSurname.BorderStyle = BorderStyle.FixedSingle;
            this.patSurname.BackColor = Color.White;
            this.patSurname.ForeColor = Color.Black;

            this.patAge = new TextBox();
            this.patAge.TextChanged += PatAge_TextChanged;
            this.patAge.Location = new Point(patSurname.Location.X + patSurname.Size.Width + 15, 50);
            this.patAge.Size = new Size(80, 20);
            this.patAge.BorderStyle = BorderStyle.FixedSingle;
            this.patAge.BackColor = Color.White;
            this.patAge.ForeColor = Color.Black;

            this.patNameLabel = new Label();
            this.patNameLabel.Text = "Имя:";
            this.patNameLabel.AutoSize = true;
            this.patNameLabel.HandleCreated += PatNameLabel_HandleCreated;

            this.patSurnameLabel = new Label();
            this.patSurnameLabel.Text = "Фамилия:";
            this.patSurnameLabel.AutoSize = true;
            this.patSurnameLabel.HandleCreated += PatSurnameLabel_HandleCreated;

            this.patAgeLabel = new Label();
            this.patAgeLabel.Text = "Возраст:";
            this.patAgeLabel.AutoSize = true;
            this.patAgeLabel.HandleCreated += PatAgeLabel_HandleCreated;

            this.patState = new ComboBox();
            this.patState.Location = new Point(patName.Location.X, 100 + patName.Size.Height);
            this.patState.Size = new Size(200, 20);
            this.patState.BackColor = Color.White;
            this.patState.ForeColor = Color.Black;
            this.patState.DropDownStyle = ComboBoxStyle.DropDownList;

            this.patState.Items.Add("Реанимация");
            this.patState.Items.Add("Болен");
            this.patState.Items.Add("Здоров");
            this.patState.Items.Add("Умер");

            this.depName = new ComboBox();

            this.depName.SelectedIndexChanged += DepName_SelectedIndexChanged;

            this.depName.Location = new Point(patSurname.Location.X, 100 + patSurname.Size.Height);
            this.depName.Size = new Size(200, 20);
            this.depName.BackColor = Color.White;
            this.depName.ForeColor = Color.Black;
            this.depName.DropDownStyle = ComboBoxStyle.DropDownList;

            this.disease = new ComboBox();
            this.disease.Location = new Point(patState.Location.X, 50 + patState.Size.Height + patState.Location.Y);
            this.disease.Size = new Size(200, 20);
            this.disease.BackColor = Color.White;
            this.disease.ForeColor = Color.Black;
            this.disease.DropDownStyle = ComboBoxStyle.DropDownList;

            this.doctor = new ComboBox();
            this.doctor.Location = new Point(depName.Location.X, 50 + depName.Size.Height + depName.Location.Y);
            this.doctor.Size = new Size(200, 20);
            this.doctor.BackColor = Color.White;
            this.doctor.ForeColor = Color.Black;
            this.doctor.DropDownStyle = ComboBoxStyle.DropDownList;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();
                SqliteCommand depCommand = connection.CreateCommand();

                depCommand.Connection = connection;
                depCommand.CommandText = "SELECT id, Name FROM Department WHERE FreeBeds>0";

                using (SqliteDataReader depReader = depCommand.ExecuteReader())
                {
                    if (depReader.HasRows)
                    {
                        while (depReader.Read())
                        {
                            string depName = depReader.GetString(1);

                            if (!string.IsNullOrWhiteSpace(depName))
                            {
                                this.depName.Items.Add(depName);
                                depNames.Add(depReader.GetInt32(0), depName);
                            }
                        }
                    }
                }

                connection.Close();
            }

            this.patStateLabel = new Label();
            this.patStateLabel.Text = "Состояние:";
            this.patStateLabel.AutoSize = true;
            this.patStateLabel.HandleCreated += PatStateLabel_HandleCreated;

            this.depNameLabel = new Label();
            this.depNameLabel.Text = "Отделение:";
            this.depNameLabel.AutoSize = true;
            this.depNameLabel.HandleCreated += DepNameLabel_HandleCreated;

            this.diseaseLabel = new Label();
            this.diseaseLabel.Text = "Диагноз:";
            this.diseaseLabel.AutoSize = true;
            this.diseaseLabel.HandleCreated += DiseaseLabel_HandleCreated;

            this.doctorLabel = new Label();
            this.doctorLabel.Text = "Лечащий врач:";
            this.doctorLabel.AutoSize = true;
            this.doctorLabel.HandleCreated += DoctorLabel_HandleCreated;

            applyButton = new Button();
            this.applyButton.Text = "Оформить";
            this.applyButton.BackColor = Color.White;
            this.applyButton.ForeColor = Color.Black;
            this.applyButton.AutoSize = true;
            this.applyButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.applyButton.HandleCreated += ApplyButton_HandleCreated;
            this.applyButton.Click += ApplyButton_Click;

            this.Controls.Add(patName);
            this.Controls.Add(patSurname);
            this.Controls.Add(patAge);
            this.Controls.Add(patNameLabel);
            this.Controls.Add(patSurnameLabel);
            this.Controls.Add(patAgeLabel);
            this.Controls.Add(patState);
            this.Controls.Add(depName);
            this.Controls.Add(patStateLabel);
            this.Controls.Add(depNameLabel);
            this.Controls.Add(disease);
            this.Controls.Add(doctor);
            this.Controls.Add(diseaseLabel);
            this.Controls.Add(doctorLabel);
            this.Controls.Add(applyButton);
        }

        private void DepName_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string depName = this.depName.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(depName))
            {
                return;
            }
            int depId = -1;
            foreach (KeyValuePair<int, string> item in depNames)
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

            this.disease.Items.Clear();
            this.doctor.Items.Clear();
            this.doctors.Clear();

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                SqliteCommand diagCommand = connection.CreateCommand();
                diagCommand.Connection = connection;
                diagCommand.CommandText = $"SELECT Name FROM Diagnosis WHERE DepartmentId={depId}";

                using (var diagReader = diagCommand.ExecuteReader())
                {
                    if (diagReader.HasRows)
                    {
                        while (diagReader.Read())
                        {
                            this.disease.Items.Add(diagReader.GetString(0));
                        }
                    }
                }

                SqliteCommand docCommand = connection.CreateCommand();
                docCommand.Connection = connection;
                docCommand.CommandText = $"SELECT id, Name, Surname FROM Doctor WHERE DepartmentId={depId} " +
                                         $"AND FireDate IS NULL AND IsFiring=0";

                using (var docReader = docCommand.ExecuteReader())
                {
                    if (docReader.HasRows)
                    {
                        string docName;
                        while (docReader.Read())
                        {
                            docName = $"{docReader.GetString(2)} {docReader.GetString(1)}";
                            this.doctor.Items.Add(docName);
                            doctors.Add(docReader.GetInt32(0), docName);
                        }
                    }
                }

                connection.Close();
            }
        }

        private void DoctorLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.doctorLabel.Location = new Point(this.doctor.Location.X + this.doctor.Size.Width / 2 - this.doctorLabel.Size.Width / 2, this.doctor.Location.Y - this.doctorLabel.Size.Height);
        }

        private void DiseaseLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.diseaseLabel.Location = new Point(this.disease.Location.X + this.disease.Size.Width / 2 - this.diseaseLabel.Size.Width / 2, this.disease.Location.Y - this.diseaseLabel.Size.Height);
        }

        private void PatAge_TextChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.patAge.Text))
            {
                this.patAge.Text = Regex.Replace(this.patAge.Text, @"[\sа-яА-ЯёЁa-zA-z\s]", "");
            }
        }

        private async void ApplyButton_Click(object? sender, EventArgs e)
        {
            string name = this.patName.Text;
            string surname = this.patSurname.Text;
            string age = this.patAge.Text;
            string state = this.patState.SelectedItem?.ToString();
            string depName = this.depName.SelectedItem?.ToString();
            string diagName = this.disease.SelectedItem?.ToString();
            int docId = -1;

            foreach (KeyValuePair<int, string> item in doctors)
            {
                if (string.Equals(item.Value, this.doctor.SelectedItem?.ToString()))
                {
                    docId = item.Key;
                    break;
                }
            }

            if (docId == -1)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(age))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        this.patName.BackColor = Color.LightPink;
                    }

                    if (string.IsNullOrWhiteSpace(surname))
                    {
                        this.patSurname.BackColor = Color.LightPink;
                    }

                    if (string.IsNullOrWhiteSpace(age))
                    {
                        this.patAge.BackColor = Color.LightPink;
                    }

                    await Task.Delay(250);


                    if (string.IsNullOrWhiteSpace(name))
                    {
                        this.patName.BackColor = Color.White;
                    }

                    if (string.IsNullOrWhiteSpace(surname))
                    {
                        this.patSurname.BackColor = Color.White;
                    }

                    if (string.IsNullOrWhiteSpace(age))
                    {
                        this.patAge.BackColor = Color.White;
                    }

                    await Task.Delay(250);
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(depName) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(diagName))
            {
                return;
            }

            InsertNewPatient(name, surname, int.Parse(age), state, depName, diagName, docId);
        }

        private void InsertNewPatient(string name, string surname, int age, string state, string depName,
                                      string diagName, int docId)
        {
            int depId = -1;
            int freeBeds = 0;
            int diagId = -1;

            if (string.Equals(state, "Здоров") || string.Equals(state, "Умер"))
            {
                MessageBox.Show(
                    "Не удалось добавить пациента, так как\n" +
                    "указано состояние \"Здоров\" или \"Умер\"",
                    "Ошибка!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();
                SqliteCommand depCommand = connection.CreateCommand();
                SqliteCommand patCommand = connection.CreateCommand();
                SqliteCommand patAccCommand = connection.CreateCommand();
                SqliteCommand diagCommand = connection.CreateCommand();

                diagCommand.Connection = connection;
                diagCommand.CommandText = $"SELECT id FROM Diagnosis WHERE Name='{diagName}'";

                using (var diagReader = diagCommand.ExecuteReader())
                {
                    if (diagReader.HasRows)
                    {
                        diagReader.Read();
                        diagId = diagReader.GetInt32(0);
                    }
                }

                if (diagId == -1)
                {
                    connection.Close();
                    return;
                }

                depCommand.Connection = connection;
                depCommand.CommandText = $"SELECT id, FreeBeds FROM Department WHERE Name='{depName}' AND FreeBeds>0";

                patCommand.Connection = connection;
                patAccCommand.Connection = connection;

                using (SqliteDataReader depReader = depCommand.ExecuteReader())
                {
                    if (depReader.HasRows)
                    {
                        depReader.Read();

                        depId = depReader.GetInt32(0);
                        freeBeds = depReader.GetInt32(1);
                    }
                }

                if (depId > 0)
                {
                    int patId = 0;

                    depCommand.CommandText = $"UPDATE Department SET FreeBeds={freeBeds - 1} WHERE id={depId}";
                    depCommand.ExecuteNonQuery();

                    patCommand.CommandText = $"INSERT INTO Patient (Name, Surname, Age) " +
                                             $"VALUES ('{name}','{surname}', {age})";
                    patCommand.ExecuteNonQuery();

                    patCommand.CommandText = $"SELECT id FROM Patient WHERE Name='{name}' AND Surname='{surname}' " +
                                             $"AND Age={age}";
                    using (SqliteDataReader patReader = patCommand.ExecuteReader())
                    {
                        if (patReader.HasRows)
                        {
                            patReader.Read();

                            patId = patReader.GetInt32(0);
                        }
                    }

                    patAccCommand.CommandText = $"INSERT INTO PatientAccounting (PatientId, AdmissionDate, " +
                                                $"State, DepartmentId) VALUES ({patId}, " +
                                                $"'{DateTime.Today.Date.ToShortDateString()}', '{state}', " +
                                                $"{depId})";
                    patAccCommand.ExecuteNonQuery();

                    string diseaseState = state switch
                    {
                        "Реанимация" => "Болен",
                        _ => state
                    };

                    SqliteCommand diseaseCommand = connection.CreateCommand();
                    diseaseCommand.Connection = connection;
                    diseaseCommand.CommandText = $"INSERT INTO Disease (PatientId, DoctorId, DiagnosisId, " +
                                                 $"DiagnosisDate, State) VALUES ({patId}, {docId}, {diagId}, " +
                                                 $"'{DateTime.Today.Date.ToShortDateString()}', '{diseaseState}')";
                    diseaseCommand.ExecuteNonQuery();
                }

                connection.Close();
            }

            parent.NewPatientInserted();

            this.Dispose();
        }

        private void ApplyButton_HandleCreated(object? sender, EventArgs e)
        {
            this.applyButton.Location = new Point(this.depName.Location.X + this.depName.Size.Width + 15, this.depName.Location.Y - 1);
            this.applyButton.Size = new Size(this.applyButton.Size.Width, 20);
            this.patAge.Size = new Size(this.applyButton.Size.Width, this.patAge.Size.Height);
        }

        private void DepNameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.depNameLabel.Location = new Point(this.depName.Location.X + this.depName.Size.Width / 2 - this.depNameLabel.Size.Width / 2, 60 + this.patSurname.Size.Height + this.patSurnameLabel.Size.Height);
        }

        private void PatStateLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.patStateLabel.Location = new Point(this.patState.Location.X + this.patState.Size.Width / 2 - this.patStateLabel.Size.Width / 2, 60 + this.patName.Size.Height + this.patNameLabel.Size.Height);
        }

        private void PatAgeLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.patAgeLabel.Location = new Point(this.patAge.Location.X + this.patAge.Size.Width / 2 - this.patAgeLabel.Size.Width / 2, 30);
        }

        private void PatSurnameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.patSurnameLabel.Location = new Point(this.patSurname.Location.X + this.patSurname.Size.Width / 2 - this.patSurnameLabel.Size.Width / 2, 30);
        }

        private void PatNameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.patNameLabel.Location = new Point(this.patName.Location.X + this.patName.Size.Width / 2 - this.patNameLabel.Size.Width / 2, 30);
        }
    }
}
