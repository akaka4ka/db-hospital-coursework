using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace DataBasesProjectWinForms
{
    public partial class PatientInfo : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private MainForm parent;
        public int patientId;
        private int departmentId;
        private Size normalSize;

        private int stateCursor;
        private bool isChangeAvailable;
        private bool isStateCursorSet;

        private Label patInfo;

        private DataGridView diagTable;

        private Button changeStateButton;
        private ComboBox changeStateCombo;

        private Button addNewDiagnosis;

        private Button changeDepButton;
        //private ComboBox changeDepCombo;

        private List<MedicinesForm> medForms = new List<MedicinesForm>();
        private List<ProceduresForm> procsForms = new List<ProceduresForm>();

        public PatientInfo(MainForm parent, int patientId)
        {
            this.parent = parent;
            this.patientId = patientId;
            
            this.Load += Form4_Load;
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

        private void Form4_Resize(object? sender, EventArgs e)
        {
            normalSize = this.Size;
        }

        public void ReloadDiseaseTable()
        {
            if (this.diagTable.Columns.Count == 0)
            {
                return;
            }

            this.diagTable.Rows?.Clear();

            string patName = string.Empty;
            string patSurname = string.Empty;
            int patAge = 0;
            string patState = string.Empty;
            string depName = string.Empty;

            using (SqliteConnection connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                int diseaseCount = 0;

                SqliteCommand patAccCommand = connection.CreateCommand();
                SqliteCommand patCommand = connection.CreateCommand();

                patCommand.Connection = connection;
                patCommand.CommandText = $"SELECT Name, Surname, Age FROM Patient WHERE id={patientId}";

                using (SqliteDataReader patReader = patCommand.ExecuteReader())
                {
                    if (patReader.HasRows)
                    {
                        patReader.Read();

                        patName = patReader.GetString(0);
                        patSurname = patReader.GetString(1);
                        patAge = patReader.GetInt32(2);
                    }
                    else
                    {
                        throw new Exception("Form: patInfo; SqlQuerry: Patient Table; Error: Reader");
                    }
                }

                patAccCommand.Connection = connection;
                patAccCommand.CommandText = $"SELECT DepartmentId FROM PatientAccounting WHERE id={patientId}";

                using (SqliteDataReader patAccReader = patAccCommand.ExecuteReader())
                {
                    if (patAccReader.HasRows)
                    {
                        patAccReader.Read();

                        departmentId = patAccReader.GetInt32(0);
                    }
                }

                if (departmentId > 0f)
                {
                    SqliteCommand depCommand = connection.CreateCommand();

                    depCommand.Connection = connection;
                    depCommand.CommandText = $"SELECT Name FROM Department WHERE id={departmentId}";

                    using (SqliteDataReader depReader = depCommand.ExecuteReader())
                    {
                        if (depReader.HasRows)
                        {
                            depReader.Read();

                            depName = depReader.GetString(0);

                            if (string.IsNullOrWhiteSpace(depName))
                            {
                                throw new Exception("Form: patInfo; SqlQuerry: Department Table; Error: Reader");
                            }
                        }
                    }

                    var ageWord = (patAge % 10) switch
                    {
                        1 or 2 or 3 or 4 => "год",
                        _ => "лет"
                    };

                    this.patInfo.Text = patSurname + " " + patName + ", " + patAge + " " + ageWord + ", " + depName;

                    SqliteCommand diseaseCommand = connection.CreateCommand();

                    diseaseCommand.Connection = connection;
                    diseaseCommand.CommandText = $"SELECT id, DoctorId, DiagnosisId, DiagnosisDate, State FROM Disease " +
                                                 $"WHERE PatientId={patientId}";

                    using (SqliteDataReader diseaseReader = diseaseCommand.ExecuteReader())
                    {
                        if (diseaseReader.HasRows)
                        {
                            int diseaseId;
                            int doctorId;
                            int diagId;
                            string diagDate;
                            string diagState;
                            int index;

                            while (diseaseReader.Read())
                            {
                                diseaseCount++;

                                diseaseId = diseaseReader.GetInt32(0);
                                doctorId = diseaseReader.GetInt32(1);
                                diagId = diseaseReader.GetInt32(2);
                                diagDate = diseaseReader.GetString(3);
                                diagState = diseaseReader.GetString(4);

                                index = this.diagTable.Rows.Add();

                                this.diagTable.Rows[index].Cells["diseaseId"].Value = diseaseId;
                                this.diagTable.Rows[index].Cells["patId"].Value = patientId;
                                this.diagTable.Rows[index].Cells["diagId"].Value = diagId;

                                this.diagTable.Rows[index].Cells["diagState"].Value = diagState;
                                this.diagTable.Rows[index].Cells["diagDate"].Value = diagDate;

                                this.diagTable.Rows[index].Cells["doctorId"].Value = doctorId;
                            }
                        }
                        else
                        {
                            throw new Exception("Form: patInfo; SqlQuerry: Disease Table; Error: Reader");
                        }
                    }

                    SqliteCommand docCommand = connection.CreateCommand();
                    SqliteCommand diagCommand = connection.CreateCommand();
                    SqliteCommand medCommand = connection.CreateCommand();
                    SqliteCommand proCommand = connection.CreateCommand();

                    docCommand.Connection = connection;
                    diagCommand.Connection = connection;
                    medCommand.Connection = connection;
                    proCommand.Connection = connection;

                    for (int i = 0; i < diseaseCount; i++)
                    {
                        docCommand.CommandText = $"SELECT Name, Surname FROM Doctor WHERE id={this.diagTable.Rows[i].Cells["doctorId"].Value}";
                        diagCommand.CommandText = $"SELECT Name FROM Diagnosis WHERE id={this.diagTable.Rows[i].Cells["diagId"].Value}";
                        medCommand.CommandText = $"SELECT id, State FROM RealMedicineTreatment WHERE DiseaseId={this.diagTable.Rows[i].Cells["diseaseId"].Value}";
                        proCommand.CommandText = $"SELECT id, State FROM RealProcedureTreatment WHERE DiseaseId={this.diagTable.Rows[i].Cells["diseaseId"].Value}";

                        using (var docReader = docCommand.ExecuteReader())
                        {
                            if (docReader.HasRows)
                            {
                                docReader.Read();

                                this.diagTable.Rows[i].Cells["doctorName"].Value = docReader.GetString(0) + " " + docReader.GetString(1);
                            }
                            else
                            {
                                this.diagTable.Rows[i].Cells["doctorName"].Value = "Доктор не найден";
                            }
                        }

                        using (var diagReader = diagCommand.ExecuteReader())
                        {
                            if (diagReader.HasRows)
                            {
                                diagReader.Read();

                                this.diagTable.Rows[i].Cells["diagName"].Value = diagReader.GetString(0);
                            }
                            else
                            {
                                this.diagTable.Rows[i].Cells["diagName"].Value = "Доктор не найден";
                            }
                        }

                        using (var medReader = medCommand.ExecuteReader())
                        {
                            if (medReader.HasRows)
                            {
                                int count = 0;

                                while (medReader.Read())
                                {
                                    if (!medReader.GetString(1).Contains("Отменено") && !medReader.GetString(1).Contains("Завершено"))
                                    {
                                        count++;
                                    }
                                }

                                if (count == 0)
                                {
                                    this.diagTable.Rows[i].Cells["medicines"].Value = "Лекарства не назначены";
                                }
                                else
                                {
                                    this.diagTable.Rows[i].Cells["medicines"].Value = count;
                                }
                            }
                            else
                            {
                                this.diagTable.Rows[i].Cells["medicines"].Value = "Лекарства не назначены";
                            }
                        }

                        using (var proReader = proCommand.ExecuteReader())
                        {
                            if (proReader.HasRows)
                            {
                                int count = 0;

                                while (proReader.Read())
                                {
                                    if (!proReader.GetString(1).Contains("Отменено") && !proReader.GetString(1).Contains("Завершено"))
                                    {
                                        count++;
                                    }
                                }

                                if (count == 0)
                                {
                                    this.diagTable.Rows[i].Cells["procedures"].Value = "Процедуры не назначены";
                                }
                                else
                                {
                                    this.diagTable.Rows[i].Cells["procedures"].Value = count;
                                }
                            }
                            else
                            {
                                this.diagTable.Rows[i].Cells["procedures"].Value = "Процедуры не назначены";
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        private void Form4_Load(object? sender, EventArgs e)
        {
            this.Resize += Form4_Resize;
            
            ///
            /// Variables
            ///
            this.patInfo = new Label();
            this.patInfo.AutoSize = true;
            this.patInfo.Location = new Point(5, 5);

            #region DataGridView.DiseaseTable

            this.diagTable = new DataGridView();

            this.diagTable.CellDoubleClick += DiagTable_CellDoubleClick;
            this.diagTable.CurrentCellChanged += DiagTable_CurrentCellChanged;
            this.diagTable.SizeChanged += DiagTable_SizeChanged;

            this.diagTable.Location = new Point(5, 25);
            this.diagTable.AutoSize = true;
            this.diagTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.diagTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.diagTable.AllowUserToAddRows = false;
            this.diagTable.AllowUserToDeleteRows = false;
            this.diagTable.AllowUserToResizeRows = false;
            this.diagTable.AllowUserToResizeColumns = false;
            this.diagTable.AllowUserToOrderColumns = false;
            this.diagTable.AllowDrop = false;
            this.diagTable.ReadOnly = true;
            this.diagTable.BackgroundColor = this.BackColor;
            this.diagTable.BorderStyle = BorderStyle.None;

            this.diagTable.Columns.Add("diseaseId", "diseaseId");
            this.diagTable.Columns["diseaseId"].Visible = false;

            this.diagTable.Columns.Add("patId", "patId");
            this.diagTable.Columns["patId"].Visible = false;

            this.diagTable.Columns.Add("diagId", "diagId");
            this.diagTable.Columns["diagId"].Visible = false;

            this.diagTable.Columns.Add("diagName", "Диагноз");
            this.diagTable.Columns.Add("diagState", "Состояние");
            this.diagTable.Columns.Add("diagDate", "Дата диагностики");
            this.diagTable.Columns.Add("medicines", "Назначено лекарств");
            this.diagTable.Columns.Add("procedures", "Назначено процедур");
            this.diagTable.Columns.Add("doctorName", "Лечащий врач");

            this.diagTable.Columns.Add("doctorId", "doctorId");
            this.diagTable.Columns["doctorId"].Visible = false;

            #endregion

            this.changeStateButton = new Button();

            this.changeStateButton.Click += ChangeStateButton_Click;
            this.changeStateButton.HandleCreated += ChangeStateButton_HandleCreated;

            this.changeStateButton.BackColor = Color.LightGray;
            this.changeStateButton.ForeColor = Color.DarkGray;
            this.changeStateButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.changeStateButton.AutoSize = true;
            this.changeStateButton.Enabled = false;
            this.changeStateButton.Text = "Изменить состояние";

            this.addNewDiagnosis = new Button();

            this.addNewDiagnosis.Click += AddNewDiagnosis_Click;
            this.addNewDiagnosis.HandleCreated += AddNewDiagnosis_HandleCreated;

            this.addNewDiagnosis.BackColor = Color.White;
            this.addNewDiagnosis.ForeColor = Color.Black;
            this.addNewDiagnosis.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.addNewDiagnosis.AutoSize = true;
            this.addNewDiagnosis.Text = "Добавить диагноз";

            this.changeStateCombo = new ComboBox();

            this.changeStateCombo.HandleCreated += ChangeStateCombo_HandleCreated;

            this.changeStateCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.changeStateCombo.Enabled = false;
            this.changeStateCombo.Visible = false;
            this.changeStateCombo.Size = new Size(150, 20);
            this.changeStateCombo.BackColor = Color.White;
            this.changeStateCombo.ForeColor = Color.Black;

            this.changeStateCombo.Items.Add("Болен");
            this.changeStateCombo.Items.Add("Здоров");
            this.changeStateCombo.Items.Add("Умер");

            this.ReloadDiseaseTable();

            this.Controls.Add(this.patInfo);
            this.Controls.Add(this.diagTable);
            this.Controls.Add(changeStateButton);
            this.Controls.Add(changeStateCombo);
            this.Controls.Add(addNewDiagnosis);
        }

        private void DiagTable_SizeChanged(object? sender, EventArgs e)
        {
            this.changeStateCombo.Location = new Point(this.changeStateButton.Location.X, this.changeStateButton.Location.Y + changeStateButton.Size.Height + 5);
            this.changeStateButton.Location = new Point(this.diagTable.Location.X + this.diagTable.Width + 5, this.diagTable.Location.Y);
            this.addNewDiagnosis.Location = new Point(this.changeStateButton.Location.X, this.changeStateButton.Location.Y + this.changeStateButton.Size.Height + 5 + this.changeStateCombo.Size.Height + 5);
        }

        private void AddNewDiagnosis_HandleCreated(object? sender, EventArgs e)
        {
            this.addNewDiagnosis.Location = new Point(this.changeStateButton.Location.X, this.changeStateButton.Location.Y + this.changeStateButton.Size.Height + 5 + this.changeStateCombo.Size.Height + 5);
        }

        private void AddNewDiagnosis_Click(object? sender, EventArgs e)
        {
            AddDiagnosis addDiagnosis = new AddDiagnosis(this, patientId);
            addDiagnosis.Show();
        }

        private void ChangeStateCombo_HandleCreated(object? sender, EventArgs e)
        {
            this.changeStateCombo.Location = new Point(this.changeStateButton.Location.X, this.changeStateButton.Location.Y + changeStateButton.Size.Height + 5);
        }

        private void DiagTable_CurrentCellChanged(object? sender, EventArgs e)
        {
            if (isChangeAvailable)
            {
                isChangeAvailable = false;
                this.changeStateCombo.Enabled = false;
                this.changeStateCombo.Visible = false;
                this.changeStateCombo.SelectedItem = null;
                this.changeStateButton.Text = "Изменить состояние";
            }

            if (!isStateCursorSet)
            {
                this.isStateCursorSet = true;
            }
            else
            {
                this.changeStateButton.Enabled = true;
                this.changeStateButton.BackColor = Color.White;
                this.changeStateButton.ForeColor = Color.Black;

                if (this.diagTable.CurrentCell != null)
                {
                    this.stateCursor = this.diagTable.CurrentCell.RowIndex;
                }
            }
        }

        private void ChangeStateButton_HandleCreated(object? sender, EventArgs e)
        {
            this.changeStateButton.Location = new Point(this.diagTable.Location.X + this.diagTable.Width + 5, this.diagTable.Location.Y);
        }

        private void ChangeStateButton_Click(object? sender, EventArgs e)
        {
            if (this.stateCursor == -1)
            {
                return;
            }

            if (!isChangeAvailable)
            {
                isChangeAvailable = true;
                this.changeStateButton.Text = "Принять";
                this.changeStateCombo.Enabled = true;
                this.changeStateCombo.Visible = true;
            }
            else
            {
                if (this.changeStateCombo?.SelectedItem == null)
                {
                    return;
                }
                else
                {
                    string state = (this.changeStateCombo.SelectedItem?.ToString() ?? string.Empty);
                    if (string.IsNullOrWhiteSpace(state))
                    {
                        throw new Exception("Can't exctract info from cell 'state'");
                    }

                    if (this.diagTable.Rows[stateCursor].Cells["diagState"].Value.ToString().Contains("Умер"))
                    {
                        return;
                    }

                    this.changeStateCombo.SelectedItem = null;
                    this.changeStateCombo.Enabled = false;
                    this.changeStateCombo.Visible = false;
                    this.changeStateButton.Text = "Изменить состояние";
                    isChangeAvailable = false;

                    using (var connection = new SqliteConnection(sqlConnectionString))
                    {
                        connection.Open();

                        SqliteCommand patAccCommand = connection.CreateCommand();
                        SqliteCommand patDisCommand = connection.CreateCommand();

                        patDisCommand.Connection = connection;
                        patDisCommand.CommandText = $"SELECT State, DiagnosisId FROM Disease WHERE PatientId={patientId}";

                        patAccCommand.Connection = connection;

                        int sickCount = 0;
                        bool isCureAvailabe = true;
                        using (SqliteDataReader patDisReader = patDisCommand.ExecuteReader())
                        {
                            if (patDisReader.HasRows)
                            {
                                while (patDisReader.Read())
                                {
                                    string disState = patDisReader.GetString(0);
                                    if (!string.Equals(disState, "Здоров") && string.Equals(state, "Здоров"))
                                    {
                                        sickCount++;
                                        if (sickCount > 1)
                                        {
                                            isCureAvailabe = false;
                                            break;
                                        }
                                    }
                                }

                                if (!string.Equals(state, "Здоров"))
                                {
                                    isCureAvailabe = false;
                                }

                                int depId;
                                patAccCommand.CommandText = $"SELECT DepartmentId FROM PatientAccounting " +
                                                            $"WHERE PatientId={patientId}";

                                using (var patAccReader = patAccCommand.ExecuteReader())
                                {
                                    if (patAccReader.HasRows)
                                    {
                                        patAccReader.Read();
                                        depId = patAccReader.GetInt32(0);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }

                                int diseaseId = int.Parse(this.diagTable.Rows[stateCursor].Cells["diseaseId"].Value.ToString());

                                if (string.Equals(state, "Здоров"))
                                {
                                    var diseaseCommand = connection.CreateCommand();
                                    diseaseCommand.Connection = connection;

                                    diseaseCommand.CommandText = $"UPDATE Disease SET CureDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                                 $"State='Здоров' " +
                                                                 $"WHERE id={diseaseId}";

                                    diseaseCommand.ExecuteNonQuery();

                                    var medRealCommand = connection.CreateCommand();
                                    medRealCommand.Connection = connection;
                                    medRealCommand.CommandText = $"UPDATE RealMedicineTreatment SET " +
                                                                 $"CancellationDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                                 $"State='Завершено' " +
                                                                 $"WHERE DiseaseId={diseaseId}";

                                    medRealCommand.ExecuteNonQuery();

                                    var procRealCommand = connection.CreateCommand();
                                    procRealCommand.Connection = connection;
                                    procRealCommand.CommandText = $"UPDATE RealProcedureTreatment SET " +
                                                                 $"CancellationDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                                 $"State='Завершено' " +
                                                                 $"WHERE DiseaseId={diseaseId}";

                                    procRealCommand.ExecuteNonQuery();

                                    if (isCureAvailabe)
                                    {
                                        int freeBeds;
                                        SqliteCommand depCommand = connection.CreateCommand();
                                        depCommand.Connection = connection;
                                        depCommand.CommandText = $"SELECT FreeBeds FROM Department WHERE id={depId}";

                                        using (var depReader = depCommand.ExecuteReader())
                                        {
                                            if (depReader.HasRows)
                                            {
                                                depReader.Read();
                                                freeBeds = depReader.GetInt32(0);
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }

                                        depCommand.CommandText = $"UPDATE Department SET FreeBeds={freeBeds + 1} " +
                                                                 $"WHERE id={depId}";
                                        depCommand.ExecuteNonQuery();

                                        patAccCommand.CommandText = $"UPDATE PatientAccounting SET State='{state}', " +
                                                                    $"DischargeDate='{DateTime.Today.Date.ToShortDateString()}' " +
                                                                    $"WHERE id={patientId} AND DischargeDate IS NULL";

                                        patAccCommand.ExecuteNonQuery();

                                        connection.Close();
                                        this.parent.ReloadPatientTab();
                                        this.Dispose();
                                    }
                                }
                                
                                if (string.Equals(state, "Умер"))
                                {
                                    var diseaseCommand = connection.CreateCommand();
                                    diseaseCommand.Connection = connection;

                                    diseaseCommand.CommandText = $"UPDATE Disease SET CureDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                                 $"State='Умер' " +
                                                                 $"WHERE id={diseaseId}";

                                    diseaseCommand.ExecuteNonQuery();

                                    diseaseCommand.CommandText = $"UPDATE Disease SET CureDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                                 $"State='Умер по другой болезни' " +
                                                                 $"WHERE PatientId={patientId} " +
                                                                 $"AND id<>{diseaseId}";

                                    diseaseCommand.ExecuteNonQuery();

                                    var medRealCommand = connection.CreateCommand();
                                    medRealCommand.Connection = connection;
                                    medRealCommand.CommandText = $"UPDATE RealMedicineTreatment SET " +
                                                                 $"CancellationDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                                 $"State='Завершено' " +
                                                                 $"WHERE DiseaseId={diseaseId}";

                                    medRealCommand.ExecuteNonQuery();

                                    var procRealCommand = connection.CreateCommand();
                                    procRealCommand.Connection = connection;
                                    procRealCommand.CommandText = $"UPDATE RealProcedureTreatment SET " +
                                                                 $"CancellationDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                                 $"State='Завершено' " +
                                                                 $"WHERE DiseaseId={diseaseId}";

                                    procRealCommand.ExecuteNonQuery();

                                    patAccCommand.CommandText = $"UPDATE PatientAccounting SET State='{state}', " +
                                                                $"DischargeDate='{DateTime.Today.Date.ToShortDateString()}' " +
                                                                $"WHERE id={patientId} AND DischargeDate IS NULL";

                                    patAccCommand.ExecuteNonQuery();

                                    this.parent.ReloadPatientTab();
                                    this.Dispose();
                                }
                            }
                        }

                        connection.Close();
                    }
                }

                this.ReloadDiseaseTable();
            }
        }

        private void DiagTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!(this.diagTable.Rows[e.RowIndex].Cells["diagState"].Value?.ToString().Contains("Болен") ?? false))
            {
                return;
            }

            if (this.diagTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString()?.Contains("не назначены") ?? false)
            {
                int diseaseId = int.Parse(this.diagTable.Rows[e.RowIndex].Cells["diseaseId"].Value.ToString());

                if (this.diagTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString()?.Contains("Процедуры") ?? false)
                {
                    AddProcedure addProcedure = new AddProcedure(this, diseaseId);
                    addProcedure.Show();
                }
                else
                {
                    AddMedicine addMedicine = new AddMedicine(this, diseaseId);
                    addMedicine.Show();
                }

                return;
            }

            if (e.ColumnIndex == this.diagTable.Columns["medicines"].Index)
            {
                int diseaseId = int.Parse(this.diagTable.Rows[e.RowIndex].Cells["diseaseId"].Value.ToString());

                foreach (var form in medForms)
                {
                    if (form.diseaseId == diseaseId)
                    {
                        form.SetInFocus();
                        return;
                    }
                }

                MedicinesForm medForm = new MedicinesForm(this, diseaseId);
                medForm.Show();
                medForms.Add(medForm);
            }
            if (e.ColumnIndex == this.diagTable.Columns["procedures"].Index)
            {
                int diseaseId = int.Parse(this.diagTable.Rows[e.RowIndex].Cells["diseaseId"].Value.ToString());

                foreach (var form in procsForms)
                {
                    if (form.diseaseId == diseaseId)
                    {
                        form.SetInFocus();
                        return;
                    }
                }

                ProceduresForm procsForm = new ProceduresForm(this, diseaseId);
                procsForm.Show();
                procsForms.Add(procsForm);
            }
        }

        public void DeleteMedsForm(MedicinesForm form)
        {
            medForms.Remove(form);
        }

        public void DeleteProcsForm(ProceduresForm form)
        {
            procsForms.Remove(form);
        }
    }
}
