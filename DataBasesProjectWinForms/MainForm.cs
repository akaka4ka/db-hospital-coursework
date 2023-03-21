using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace DataBasesProjectWinForms
{
    public partial class MainForm : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;
        
        private int fireCursor = -1;
        private bool isFireCursorSet = false;

        private int patientCursor = -1;
        private bool isPatientCursorSet = false;

        private bool isChangeAvailable = false;

        List<PatientInfo> infoForms = new List<PatientInfo>();

        public MainForm()
        {
            this.Load += MainForm_Load;
            this.Resize += MainForm_Resize;
            InitializeComponent();
        }

        #region DoctorsTab

        private void HireDocButton_HandleCreated(object? sender, EventArgs e)
        {
            this.fireDocButton.Size = this.hireDocButton.Size;
        }

        private void DoctorsTab_Leave(object? sender, EventArgs e)
        {
            doctorsTable.Rows.Clear();
            isFireCursorSet = false;
            fireCursor = -1;
            this.fireDocButton.Enabled = false;
            this.fireDocButton.BackColor = Color.LightGray;
            this.fireDocButton.ForeColor = Color.DarkGray;
        }

        public void NewDoctorInserted()
        {
            DoctorsTab_Leave(null, null);
            DoctorsTab_Enter(null, null);
        }

        private void DoctorsTab_Enter(object? sender, EventArgs e)
        {
            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();
                SqliteCommand docCommand = connection.CreateCommand();
                SqliteCommand depCommand = connection.CreateCommand();

                docCommand.Connection = connection;
                docCommand.CommandText = "SELECT * FROM Doctor WHERE FireDate IS NULL";

                depCommand.Connection = connection;

                using (SqliteDataReader docReader = docCommand.ExecuteReader())
                {
                    if (docReader.HasRows)
                    {
                        while (docReader.Read())
                        {
                            int id = docReader.GetInt32(0);
                            string name = docReader.GetString(1);
                            string surname = docReader.GetString(2);
                            int depId = docReader.GetInt32(3);
                            string hireDate = docReader.GetString(4);
                            string depName = string.Empty;

                            depCommand.CommandText = $"SELECT Name FROM Department WHERE id={depId}";
                            using (SqliteDataReader depReader = depCommand.ExecuteReader())
                            {
                                if (depReader.HasRows)
                                {
                                    depReader.Read();
                                    depName = depReader.GetString(0);
                                }
                            }

                            int index = this.doctorsTable.Rows.Add();
                            this.doctorsTable.Rows[index].Cells["id"].Value = id;
                            this.doctorsTable.Rows[index].Cells["name"].Value = name;
                            this.doctorsTable.Rows[index].Cells["surname"].Value = surname;
                            this.doctorsTable.Rows[index].Cells["departmentName"].Value = depName ?? "Ошибка";
                            this.doctorsTable.Rows[index].Cells["hireDate"].Value = hireDate;
                        }
                    }
                }

                connection.Close();
            }

            this.fireCursor = this.doctorsTable?.CurrentCell?.RowIndex ?? -1;
        }

        private void DoctorsTable_CurrentCellChanged(object? sender, EventArgs e)
        {
            if (!isFireCursorSet)
            {
                this.isFireCursorSet = true;
            }
            else
            {
                this.fireDocButton.Enabled = true;
                this.fireDocButton.BackColor = Color.White;
                this.fireDocButton.ForeColor = Color.Black;

                if (this.doctorsTable.CurrentCell != null)
                {
                    this.fireCursor = this.doctorsTable.CurrentCell.RowIndex;
                }
            }
        }

        private void FireDocButton_Click(object? sender, EventArgs e)
        {
            if (fireCursor == -1)
            {
                return;
            }

            int id = int.Parse(this.doctorsTable.Rows[fireCursor].Cells["id"].Value?.ToString() ?? "-1");
            if (id == -1)
            {
                throw new Exception("Can't extract info from cell 'id'");
            }

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();
                SqliteCommand docCommand = connection.CreateCommand();
                docCommand.Connection = connection;

                var diseaseCommand = connection.CreateCommand();
                diseaseCommand.Connection = connection;
                diseaseCommand.CommandText = $"SELECT id FROM Disease WHERE DoctorId={id} AND CureDate IS NULL";

                using (var diseaseReader = diseaseCommand.ExecuteReader())
                {
                    if (diseaseReader.HasRows)
                    {
                        var result = MessageBox.Show(
                            "Не удалось уволить врача, так как \n" +
                            "он ещё лечит пациентов! Не назначать ему больше больных?",
                            "Ошибка!",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                            );

                        if (result == DialogResult.No)
                        {
                            connection.Close();
                            return;
                        }
                        else
                        {
                            docCommand.CommandText = $"UPDATE Doctor SET IsFiring=1 WHERE id={id}";
                            docCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        docCommand.CommandText = $"UPDATE Doctor SET FireDate='{DateTime.Today.Date.ToShortDateString()}', IsFiring=1 WHERE id={id}";
                        this.doctorsTable.Rows.Remove(this.doctorsTable.Rows[fireCursor]);

                        if (docCommand.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Can't update row");
                        }
                    }
                }

                connection.Close();
            }

            this.fireDocButton.Enabled = false;
            this.fireDocButton.BackColor = Color.LightGray;
            this.fireDocButton.ForeColor = Color.DarkGray;

            fireCursor = -1;
        }

        private void HireDocButton_Click(object? sender, EventArgs e)
        {
            hireDoctorForm = new AddNewDoctor(this);
            hireDoctorForm.Show();
        }

        private void DoctorsTable_Resize(object? sender, EventArgs e)
        {
            this.fireDocButton.Location = new Point(doctorsTable.Size.Width + doctorsTable.Location.X, 5);
            this.hireDocButton.Location = new Point(doctorsTable.Size.Width + doctorsTable.Location.X, 5 + fireDocButton.Size.Height + 3);
        }

        #endregion

        #region PatientTab

        private void ChangeDepButton_Click(object? sender, EventArgs e)
        {
            if (patientCursor == -1)
            {
                return;
            }

            int patId = int.Parse(this.patientsTable.Rows[patientCursor].Cells["patId"].Value.ToString());

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var patAccCommand = connection.CreateCommand();
                patAccCommand.Connection = connection;
                patAccCommand.CommandText = $"SELECT id, DepartmentId FROM PatientAccounting " +
                                            $"WHERE PatientId={patId}";

                int depId = -1;
                int patAccId = -1;
                using (var patAccReader = patAccCommand.ExecuteReader())
                {
                    if (patAccReader.HasRows)
                    {
                        patAccReader.Read();
                        depId = patAccReader.GetInt32(1);
                        patAccId = patAccReader.GetInt32(0);
                    }
                    else
                    {
                        connection.Close();
                        return;
                    }
                }

                Dictionary<int, int> diagnosis = new Dictionary<int, int>();

                var diseaseCommand = connection.CreateCommand();
                diseaseCommand.Connection = connection;
                diseaseCommand.CommandText = $"SELECT id, DiagnosisId FROM Disease WHERE " +
                                             $"PatientId={patId} AND CureDate IS NULL";

                using (var diseaseReader = diseaseCommand.ExecuteReader())
                {
                    if (diseaseReader.HasRows)
                    {
                        while (diseaseReader.Read())
                        {
                            diagnosis.Add(diseaseReader.GetInt32(0), diseaseReader.GetInt32(1));
                        }
                    }
                    else
                    {
                        connection.Close();
                        return;
                    }
                }

                Dictionary<int, int> diagDep = new Dictionary<int, int>();

                var diagCommand = connection.CreateCommand();
                diagCommand.Connection = connection;
                diagCommand.CommandText = $"SELECT id, DepartmentId FROM Diagnosis";
                bool trueDep = false;
                using (var diagReader = diagCommand.ExecuteReader())
                {
                    if (diagReader.HasRows)
                    {
                        while (diagReader.Read())
                        {
                            diagDep.Add(diagReader.GetInt32(0), diagReader.GetInt32(1));
                            if (diagnosis.ContainsValue(diagReader.GetInt32(0)) && (depId == diagReader.GetInt32(1)))
                            {
                                trueDep = true;
                            }
                        }
                    }
                }

                if (trueDep)
                {
                    MessageBox.Show(
                        $"Не удалось перевести пациента, так как \n" +
                        $"его ещё лечат в текущем отделении \n",
                        $"Ошибка!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                    connection.Close();
                    return;
                }
                else
                {
                    var result = MessageBox.Show(
                                    $"Перевести пациента в другое доступное отделение?",
                                    $"Ошибка!",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question
                                    );

                    if (result == DialogResult.Yes)
                    {
                        List<int> availableDeps = new List<int>();

                        var depCommand = connection.CreateCommand();
                        depCommand.Connection = connection;
                        depCommand.CommandText = $"SELECT id FROM Department WHERE FreeBeds>0";

                        using (var depReader = depCommand.ExecuteReader())
                        {
                            if (depReader.HasRows)
                            {
                                while (depReader.Read())
                                {
                                    availableDeps.Add(depReader.GetInt32(0));
                                }
                            }
                            else
                            {
                                MessageBox.Show(
                                    $"Не удалось перевести пациента, так как \n" +
                                    $"нет доступных отделений \n",
                                    $"Ошибка!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                    );
                                connection.Close();
                                return;
                            }
                        }

                        Dictionary<int, int>.KeyCollection keys = diagDep.Keys;
                        foreach (int item in keys)
                        {
                            if (!availableDeps.Contains(diagDep[item]))
                            {
                                diagDep.Remove(item);
                            }
                        }

                        int newDepId = -1;
                        foreach (KeyValuePair<int, int> item in diagnosis)
                        {
                            newDepId = diagDep[item.Value];
                            break;
                        }

                        patAccCommand.CommandText = $"UPDATE PatientAccounting SET DepartmentId={newDepId} " +
                                                    $"WHERE PatientId={patId} AND DischargeDate IS NULL";
                        patAccCommand.ExecuteNonQuery();

                        int oldFreeBeds = -1;
                        int newFreeBeds = -1;
                        depCommand.CommandText = $"SELECT id, FreeBeds FROM Department WHERE id={depId} OR id={newDepId}";

                        using (var depReader = depCommand.ExecuteReader())
                        {
                            if (depReader.HasRows)
                            {
                                while (depReader.Read())
                                {
                                    if (depReader.GetInt32(0) == depId)
                                    {
                                        oldFreeBeds = depReader.GetInt32(1);
                                    }

                                    if (depReader.GetInt32(0) == newDepId)
                                    {
                                        newFreeBeds = depReader.GetInt32(1);
                                    }
                                }
                            }
                        }

                        depCommand.CommandText = $"UPDATE Department SET FreeBeds={oldFreeBeds + 1} WHERE id={depId}";
                        depCommand.ExecuteNonQuery();

                        depCommand.CommandText = $"UPDATE Department SET FreeBeds={newFreeBeds - 1} WHERE id={newDepId}";
                        depCommand.ExecuteNonQuery();
                    }
                }

                /*Dictionary<int, string> departments = new Dictionary<int, string>();

                var depCommand = connection.CreateCommand();
                depCommand.Connection = connection;
                depCommand.CommandText = $"SELECT id, Name FROM Department";

                using (var depReader = depCommand.ExecuteReader())
                {
                    if (depReader.HasRows)
                    {
                        while (depReader.Read())
                        {
                            departments.Add(depReader.GetInt32(0), depReader.GetString(1));
                        }
                    }
                }

                diagCommand.Connection = connection;
                diagCommand.CommandText = $"SELECT DiagnosisId, DepartmentId FROM Diagnosis";

                bool trueDep = false;
                using (var diagReader = diagCommand.ExecuteReader())
                {
                    if (diagReader.HasRows)
                    {
                        while (diagReader.Read())
                        {
                            if (diagnosis.ContainsValue(diagReader.GetString(1)) && departments.ContainsValue(diagReader.GetString(1)))
                            {

                            }
                        }
                    }
                }*/

                connection.Close();
            }

            this.ReloadPatientTab();
        }

        private void ChangeStateButton_HandleCreated(object? sender, EventArgs e)
        {
            this.newPatientButton.Size = new Size(this.changeStateButton.Size.Width, this.newPatientButton.Size.Height);
        }

        private void ChangeStateButton_Click(object? sender, EventArgs e)
        {
            if (patientCursor == -1)
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
                    int patId = int.Parse(this.patientsTable.Rows[patientCursor].Cells["patId"].Value?.ToString() ?? "-1");
                    if (patId == -1)
                    {
                        throw new Exception("Can't exctract info from cell 'patId'");
                    }

                    string state = (this.changeStateCombo.SelectedItem?.ToString() ?? string.Empty);
                    if (string.IsNullOrWhiteSpace(state))
                    {
                        throw new Exception("Can't exctract info from cell 'state'");
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
                        patDisCommand.CommandText = $"SELECT State, DiagnosisId FROM Disease WHERE PatientId={patId}";
                        
                        patAccCommand.Connection = connection;

                        bool isCureAvailabe = true;
                        bool isDeathAvailable = true;

                        using (SqliteDataReader patDisReader = patDisCommand.ExecuteReader())
                        {
                            if (patDisReader.HasRows)
                            {
                                while (patDisReader.Read())
                                {
                                    string disState = patDisReader.GetString(0);
                                    if (!string.Equals(disState, "Здоров") && string.Equals(state, "Здоров"))
                                    {
                                        isCureAvailabe = false;
                                        break;
                                    }
                                    if (!(string.Equals(disState, "Умер") || string.Equals(disState, "Умер по другой болезни")) && string.Equals(state, "Умер"))
                                    {
                                        isDeathAvailable = false;
                                        break;
                                    }
                                }

                                if (!string.Equals(state, "Умер"))
                                {
                                    isDeathAvailable = false;
                                }
                                
                                if (!string.Equals(state, "Здоров"))
                                {
                                    isCureAvailabe = false;
                                }
                            }
                            else
                            {
                                connection.Close();
                                return;
                            }
                        }

                        if (isCureAvailabe)
                        {
                            int depId;
                            
                            patAccCommand.CommandText = $"SELECT DepartmentId FROM PatientAccounting " +
                                                        $"WHERE PatientId={patId}";

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

                            patAccCommand.CommandText = $"UPDATE PatientAccounting SET State='{state}', " +
                                                        $"DischargeDate='{DateTime.Today.Date.ToShortDateString()}' " +
                                                        $"WHERE id={patId}";

                            patAccCommand.ExecuteNonQuery();

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
                        }
                        else
                        {
                            if (string.Equals(state, "Здоров"))
                            {
                                MessageBox.Show(
                                    "Не удалось изменить состояние, так как \n" +
                                    "данный пациент здоров не по всем заболеваниям",
                                    "Ошибка!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                    );
                            }
                        }

                        if (isDeathAvailable)
                        {
                            int depId;
                            patAccCommand.CommandText = $"SELECT DepartmentId FROM PatientAccounting " +
                                                        $"WHERE PatientId={patId}";

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

                            patAccCommand.CommandText = $"UPDATE PatientAccounting SET State='{state}', " +
                                                        $"DischargeDate='{DateTime.Today.Date.ToShortDateString()}' " +
                                                        $"WHERE id={patId}";

                            patAccCommand.ExecuteNonQuery();

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
                        }
                        else
                        {
                            if (string.Equals(state, "Умер"))
                            {
                                MessageBox.Show(
                                    "Не удалось изменить состояние, так как \n" +
                                    "данный пациент не умер ни по одному из заболеваний",
                                    "Ошибка!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                    );
                            }
                        }

                        if (!string.Equals(state, "Здоров") && !string.Equals(state, "Умер"))
                        {
                            patAccCommand.CommandText = $"UPDATE PatientAccounting SET State='{state}' " +
                                                        $"WHERE Patientid={patId}";

                            patAccCommand.ExecuteNonQuery();
                        }

                        connection.Close();
                    }

                    PatientTab_Leave(null, null);
                    PatientTab_Enter(null, null);
                }
            }
        }

        public void NewPatientInserted()
        {
            PatientTab_Leave(null, null);
            PatientTab_Enter(null, null);
        }

        public void ReloadPatientTab()
        {
            PatientTab_Leave(null, null);
            PatientTab_Enter(null, null);
        }

        private void PatientTab_Leave(object? sender, EventArgs e)
        {
            this.patientsTable.Rows.Clear();
            this.patientCursor = -1;
            this.changeStateButton.Enabled = false;
            this.isPatientCursorSet = false;
            this.changeStateButton.BackColor = Color.LightGray;
            this.changeStateButton.ForeColor = Color.DarkGray;

            this.infoButton.Enabled = false;
            this.infoButton.BackColor = Color.LightGray;
            this.infoButton.ForeColor = Color.DarkGray;

            this.changeDepButton.Enabled = false;
            this.changeDepButton.BackColor = Color.LightGray;
            this.changeDepButton.ForeColor = Color.DarkGray;
        }

        private void PatientTab_Enter(object? sender, EventArgs e)
        {
            int patId = -1;
            string name = string.Empty;
            string surname = string.Empty;
            int age = 0;
            string depName = string.Empty;
            int depId = -1;
            string state = string.Empty;
            string admissionDate = string.Empty;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                SqliteCommand patCommand = connection.CreateCommand();
                SqliteCommand patAccCommand = connection.CreateCommand();
                SqliteCommand depCommand = connection.CreateCommand();

                patCommand.Connection = connection;
                patCommand.CommandText = "SELECT * FROM Patient";

                using (SqliteDataReader patReader = patCommand.ExecuteReader())
                {
                    if (patReader.HasRows)
                    {
                        while (patReader.Read())
                        {
                            patId = patReader.GetInt32(0);
                            name = patReader.GetString(1);
                            surname = patReader.GetString(2);
                            age = patReader.GetInt32(3);

                            if (patId > -1)
                            {
                                patAccCommand.CommandText = $"SELECT AdmissionDate, State, DepartmentId " +
                                                            $"FROM PatientAccounting WHERE PatientId={patId} AND " +
                                                            $"DischargeDate IS NULL";

                                using (SqliteDataReader patAccReader = patAccCommand.ExecuteReader())
                                {
                                    if (patAccReader.HasRows)
                                    {
                                        patAccReader.Read();

                                        admissionDate = patAccReader.GetString(0);
                                        state = patAccReader.GetString(1);
                                        depId = patAccReader.GetInt32(2);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                if (depId > -1)
                                {
                                    depCommand.CommandText = $"SELECT Name FROM Department WHERE id={depId}";

                                    using (SqliteDataReader depReader = depCommand.ExecuteReader())
                                    {
                                        if (depReader.HasRows)
                                        {
                                            depReader.Read();
                                            depName = depReader.GetString(0);
                                        }
                                    }

                                    int index = this.patientsTable.Rows.Add();
                                    this.patientsTable.Rows[index].Cells["patId"].Value = patId;
                                    this.patientsTable.Rows[index].Cells["name"].Value = name;
                                    this.patientsTable.Rows[index].Cells["surname"].Value = surname;
                                    this.patientsTable.Rows[index].Cells["age"].Value = age;
                                    this.patientsTable.Rows[index].Cells["depName"].Value = depName;
                                    this.patientsTable.Rows[index].Cells["state"].Value = state;
                                    this.patientsTable.Rows[index].Cells["admissionDate"].Value = admissionDate;

                                }
                            }
                        }
                    }
                }

                connection.Close();
            }

            patientsTable.Size = new Size(patientsTable.RowHeadersWidth, patientsTable.Height);
            this.newPatientButton.Location = new Point(patientsTable.Width + patientsTable.RowHeadersWidth, 5);
            this.changeStateButton.Location = new Point(this.newPatientButton.Location.X, this.newPatientButton.Location.Y + this.newPatientButton.Size.Height + 3);
            this.changeStateCombo.Location = new Point(this.changeStateButton.Location.X + this.changeStateButton.Size.Width + 15, this.changeStateButton.Location.Y);
            this.infoButton.Location = new Point(this.newPatientButton.Location.X + this.newPatientButton.Size.Width + 15, this.newPatientButton.Location.Y);
            this.changeDepButton.Location = new Point(this.changeStateButton.Location.X, this.changeStateButton.Location.Y + this.changeStateButton.Size.Height + 3);
        }

        private void PatientsTable_CurrentCellChanged(object? sender, EventArgs e)
        {
            if (isChangeAvailable)
            {
                isChangeAvailable = false;
                this.changeStateCombo.Enabled = false;
                this.changeStateCombo.Visible = false;
                this.changeStateCombo.SelectedItem = null;
                this.changeStateButton.Text = "Изменить состояние";

                this.infoButton.Enabled = false;
                this.infoButton.BackColor = Color.LightGray;
                this.infoButton.ForeColor = Color.DarkGray;

                this.changeDepButton.Enabled = false;
                this.changeDepButton.BackColor = Color.LightGray;
                this.changeDepButton.ForeColor = Color.DarkGray;
            }

            if (!isPatientCursorSet)
            {
                this.isPatientCursorSet = true;
            }
            else
            {
                this.changeStateButton.Enabled = true;
                this.changeStateButton.BackColor = Color.White;
                this.changeStateButton.ForeColor = Color.Black;

                this.infoButton.Enabled = true;
                this.infoButton.BackColor = Color.White;
                this.infoButton.ForeColor = Color.Black;

                this.changeDepButton.Enabled = true;
                this.changeDepButton.BackColor = Color.White;
                this.changeDepButton.ForeColor = Color.Black;

                if (this.patientsTable.CurrentCell != null)
                {
                    this.patientCursor = this.patientsTable.CurrentCell.RowIndex;
                }
            }
        }

        private void NewPatient_Click(object? sender, EventArgs e)
        {
            this.newPatientForm = new AddNewPatient(this);
            this.newPatientForm.Show();
        }

        private void InfoButton_HandleCreated(object? sender, EventArgs e)
        {
            this.infoButton.Size = new Size(100, 20);
        }

        private void InfoButton_Click(object? sender, EventArgs e)
        {
            if (!isPatientCursorSet || patientCursor == -1)
            {
                return;
            }

            int patId = int.Parse(this.patientsTable.Rows[patientCursor].Cells["patId"].Value?.ToString() ?? "-1");
            if (patId == -1)
            {
                throw new Exception("Can't exctract info from cell 'patId'");
            }

            foreach (var form in infoForms)
            {
                if (form.patientId == patId)
                {
                    form.SetInFocus();
                    return;
                }
            }

            PatientInfo infoForm = new PatientInfo(this, patId);
            infoForm.Show();
            infoForms.Add(infoForm);
        }

        public void DeleteInfoForm(PatientInfo form)
        {
            infoForms.Remove(form);
        }

        #endregion

        #region MainTab

        private void BadDocsRepButton_Click(object? sender, EventArgs e)
        {
            BadDocsForm badDocsForm = new BadDocsForm();
            badDocsForm.Show();
        }

        private void BestDocsRepButton_Click(object? sender, EventArgs e)
        {
            BestDocsForm bestDocsForm = new BestDocsForm();
            bestDocsForm.Show();
        }

        private void DisFreqRepButton_Click(object? sender, EventArgs e)
        {
            string start = this.freqStartDate.Text;
            string end = this.freqEndDate.Text;

            DiseaseFreqForm diseaseFreqForm = new DiseaseFreqForm(start, end);
            diseaseFreqForm.Show();
        }

        #endregion

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            this.tabControl.Size = new Size(ClientSize.Width - 22, ClientSize.Height - 22);

            if (doctorsTable != null)
            {
                doctorsTable.Size = new Size(doctorsTable.RowHeadersWidth, doctorsTable.Height);
            }

            if (patientsTable != null)
            {
                patientsTable.Size = new Size(patientsTable.RowHeadersWidth, patientsTable.Height);
            }
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            this.mainTab = new TabPage();
            this.mainTab.Text = "Главная";

            #region DoctorsTabConfigure

            this.doctorsTab = new TabPage();
            this.doctorsTab.Text = "Врачи";

            this.doctorsTab.Enter += DoctorsTab_Enter;
            this.doctorsTab.Leave += DoctorsTab_Leave;

            #region DataGridView.DoctorsTable

            this.doctorsTable = new DataGridView();

            this.doctorsTable.CurrentCellChanged += DoctorsTable_CurrentCellChanged;

            this.doctorsTable.Resize += DoctorsTable_Resize;

            this.doctorsTable.Location = new Point(5, 5);
            this.doctorsTable.AutoSize = true;
            this.doctorsTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.doctorsTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.doctorsTable.AllowUserToAddRows = false;
            this.doctorsTable.AllowUserToDeleteRows = false;
            this.doctorsTable.AllowUserToResizeRows = false;
            this.doctorsTable.AllowUserToResizeColumns = false;
            this.doctorsTable.AllowUserToOrderColumns = false;
            this.doctorsTable.AllowDrop = false;
            this.doctorsTable.ReadOnly = true;
            this.doctorsTable.BackgroundColor = this.BackColor;
            this.doctorsTable.BorderStyle = BorderStyle.None;

            this.doctorsTable.Columns.Add("id", "id");
            this.doctorsTable.Columns["id"].Visible = false;
            this.doctorsTable.Columns.Add("name", "Имя");
            this.doctorsTable.Columns.Add("surname", "Фамилия");
            this.doctorsTable.Columns.Add("departmentName", "Отделение");
            this.doctorsTable.Columns.Add("hireDate", "Дата найма");

            #endregion

            this.fireDocButton = new Button();

            this.fireDocButton.Click += FireDocButton_Click;

            this.fireDocButton.BackColor = Color.LightGray;
            this.fireDocButton.ForeColor = Color.DarkGray;
            this.fireDocButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.fireDocButton.AutoSize = true;
            this.fireDocButton.Enabled = false;
            this.fireDocButton.Text = "Уволить";

            this.hireDocButton = new Button();

            this.hireDocButton.Click += HireDocButton_Click;
            this.hireDocButton.HandleCreated += HireDocButton_HandleCreated;

            this.hireDocButton.BackColor = Color.White;
            this.hireDocButton.ForeColor = Color.Black;
            this.hireDocButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.hireDocButton.AutoSize = true;
            this.hireDocButton.Text = "Оформить нового врача";

            #endregion

            #region PatientTabConfigure
            
            this.patientsTab = new TabPage();
            this.patientsTab.Text = "Пациенты";

            this.patientsTab.Enter += PatientTab_Enter;
            this.patientsTab.Leave += PatientTab_Leave;

            #region DataGridView.PatientsTable

            this.patientsTable = new DataGridView();

            this.patientsTable.CurrentCellChanged += PatientsTable_CurrentCellChanged;

            this.patientsTable.Location = new Point(5, 5);
            this.patientsTable.AutoSize = true;
            this.patientsTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.patientsTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.patientsTable.AllowUserToAddRows = false;
            this.patientsTable.AllowUserToDeleteRows = false;
            this.patientsTable.AllowUserToResizeRows = false;
            this.patientsTable.AllowUserToResizeColumns = false;
            this.patientsTable.AllowUserToOrderColumns = false;
            this.patientsTable.AllowDrop = false;
            this.patientsTable.ReadOnly = true;
            this.patientsTable.BackgroundColor = this.BackColor;
            this.patientsTable.BorderStyle = BorderStyle.None;

            this.patientsTable.Columns.Add("patId", "patId");
            this.patientsTable.Columns["patId"].Visible = false;
            this.patientsTable.Columns.Add("name", "Имя");
            this.patientsTable.Columns.Add("surname", "Фамилия");
            this.patientsTable.Columns.Add("age", "Возраст");
            this.patientsTable.Columns.Add("depName", "Отделение");
            this.patientsTable.Columns.Add("state", "Состояние");
            this.patientsTable.Columns.Add("admissionDate", "Дата поступления");

            #endregion
            
            this.newPatientButton = new Button();

            this.newPatientButton.Click += NewPatient_Click;

            this.newPatientButton.BackColor = Color.White;
            this.newPatientButton.ForeColor = Color.Black;
            this.newPatientButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.newPatientButton.AutoSize = true;
            this.newPatientButton.Text = "Новый пациент";

            this.changeStateButton = new Button();

            this.changeStateButton.Click += ChangeStateButton_Click;
            this.changeStateButton.HandleCreated += ChangeStateButton_HandleCreated;

            this.changeStateButton.BackColor = Color.LightGray;
            this.changeStateButton.ForeColor = Color.DarkGray;
            this.changeStateButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.changeStateButton.AutoSize = true;
            this.changeStateButton.Enabled = false;
            this.changeStateButton.Text = "Изменить состояние";

            this.changeStateCombo = new ComboBox();
            this.changeStateCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.changeStateCombo.Enabled = false;
            this.changeStateCombo.Visible = false;
            this.changeStateCombo.Size = new Size(150, 20);
            this.changeStateCombo.BackColor = Color.White;
            this.changeStateCombo.ForeColor = Color.Black;

            this.changeStateCombo.Items.Add("Реанимация");
            this.changeStateCombo.Items.Add("Болен");
            this.changeStateCombo.Items.Add("Здоров");
            this.changeStateCombo.Items.Add("Умер");

            this.infoButton = new Button();

            this.infoButton.Click += InfoButton_Click;
            this.infoButton.HandleCreated += InfoButton_HandleCreated;

            this.infoButton.BackColor = Color.LightGray;
            this.infoButton.ForeColor = Color.DarkGray;
            this.infoButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.infoButton.AutoSize = true;
            this.infoButton.Enabled = false;
            this.infoButton.Text = "Подробнее";

            this.changeDepButton = new Button();

            this.changeDepButton.Click += ChangeDepButton_Click;

            this.changeDepButton.BackColor = Color.LightGray;
            this.changeDepButton.ForeColor = Color.DarkGray;
            this.changeDepButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.changeDepButton.AutoSize = true;
            this.changeDepButton.Enabled = false;
            this.changeDepButton.Text = "Перевести в другое отделение";

            #endregion

            #region MainTabConfigure
            
            this.badDocsRepButton = new Button();

            this.badDocsRepButton.Click += BadDocsRepButton_Click;

            this.badDocsRepButton.BackColor = Color.White;
            this.badDocsRepButton.ForeColor = Color.Black;
            this.badDocsRepButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.badDocsRepButton.AutoSize = true;
            this.badDocsRepButton.Location = new Point(5, 5);
            this.badDocsRepButton.Text = "Список врачей, пациенты которых умирают";

            this.bestDocsRepButton = new Button();

            this.bestDocsRepButton.Click += BestDocsRepButton_Click;

            this.bestDocsRepButton.BackColor = Color.White;
            this.bestDocsRepButton.ForeColor = Color.Black;
            this.bestDocsRepButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.bestDocsRepButton.AutoSize = true;
            this.bestDocsRepButton.Location = new Point(5, 5 + 30 + 5);
            this.bestDocsRepButton.Text = "Список лучших врачей";

            this.disFreqRepButton = new Button();

            this.disFreqRepButton.Click += DisFreqRepButton_Click;

            this.disFreqRepButton.BackColor = Color.White;
            this.disFreqRepButton.ForeColor = Color.Black;
            this.disFreqRepButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.disFreqRepButton.AutoSize = true;
            this.disFreqRepButton.Location = new Point(5, 5 + 30 + 5 + 30 + 5);
            this.disFreqRepButton.Text = "Частота болезней";

            this.yearHisRepButton = new Button();

            this.yearHisRepButton.Click += YearHisRepButton_Click;

            this.yearHisRepButton.BackColor = Color.White;
            this.yearHisRepButton.ForeColor = Color.Black;
            this.yearHisRepButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.yearHisRepButton.AutoSize = true;
            this.yearHisRepButton.Location = new Point(5, 5 + 30 + 5 + 30 + 5 + 30 + 5);
            this.yearHisRepButton.Text = "Годовой отчёт больницы";

            this.allHisRepButton = new Button();

            this.allHisRepButton.Click += AllHisRepButton_Click;

            this.allHisRepButton.BackColor = Color.White;
            this.allHisRepButton.ForeColor = Color.Black;
            this.allHisRepButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.allHisRepButton.AutoSize = true;
            this.allHisRepButton.Location = new Point(5, 5 + 30 + 5 + 30 + 5 + 30 + 5 + 30 + 5);
            this.allHisRepButton.Text = "Отчёт больницы за всё время";

            this.freqStartDate = new DateTimePicker();

            this.freqStartDate.HandleCreated += FreqStartDate_HandleCreated;

            this.freqStartDate.Format = DateTimePickerFormat.Short;
            this.freqStartDate.MaxDate = DateTime.Today.Date;

            this.freqEndDate = new DateTimePicker();

            this.freqEndDate.HandleCreated += FreqEndDate_HandleCreated;

            this.freqEndDate.Format = DateTimePickerFormat.Short;
            this.freqEndDate.MaxDate = DateTime.Today.Date;

            #endregion

            this.doctorsTab.Controls.Add(doctorsTable);
            this.doctorsTab.Controls.Add(fireDocButton);
            this.doctorsTab.Controls.Add(hireDocButton);

            this.patientsTab.Controls.Add(patientsTable);
            this.patientsTab.Controls.Add(newPatientButton);
            this.patientsTab.Controls.Add(changeStateButton);
            this.patientsTab.Controls.Add(changeStateCombo);
            this.patientsTab.Controls.Add(infoButton);
            this.patientsTab.Controls.Add(changeDepButton);

            this.mainTab.Controls.Add(badDocsRepButton);
            this.mainTab.Controls.Add(bestDocsRepButton);
            this.mainTab.Controls.Add(disFreqRepButton);
            this.mainTab.Controls.Add(freqStartDate);
            this.mainTab.Controls.Add(freqEndDate);
            this.mainTab.Controls.Add(yearHisRepButton);
            this.mainTab.Controls.Add(allHisRepButton);

            this.tabControl.TabPages.Add(mainTab);
            this.tabControl.TabPages.Add(doctorsTab);
            this.tabControl.TabPages.Add(patientsTab);
        }

        private void AllHisRepButton_Click(object? sender, EventArgs e)
        {
            AllHistoryForm allHistoryForm = new AllHistoryForm();
            allHistoryForm.Show();
        }

        private void YearHisRepButton_Click(object? sender, EventArgs e)
        {
            YearHistoryForm yearHistoryForm = new YearHistoryForm();
            yearHistoryForm.Show();
        }

        private void FreqEndDate_HandleCreated(object? sender, EventArgs e)
        {
            this.freqEndDate.Location = new Point(freqStartDate.Location.X + freqStartDate.Size.Width + 5, freqStartDate.Location.Y);
        }

        private void FreqStartDate_HandleCreated(object? sender, EventArgs e)
        {
            this.freqStartDate.Location = new Point(disFreqRepButton.Location.X + disFreqRepButton.Size.Width + 5, disFreqRepButton.Location.Y);
        }
    }
}
