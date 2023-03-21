using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class AddProcedure : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private PatientInfo parent;
        private int diseaseId;

        private ComboBox procedure;
        private TextBox totalAmount;

        private Label procLabel;
        private Label amountLabel;

        private Button applyButton;

        private void AddProcedure_Load(object? sender, EventArgs e)
        {
            this.procedure = new ComboBox();
            this.procedure.Location = new Point(50, 50);
            this.procedure.Size = new Size(200, 20);
            this.procedure.BackColor = Color.White;
            this.procedure.ForeColor = Color.Black;
            this.procedure.DropDownStyle = ComboBoxStyle.DropDownList;

            this.totalAmount = new TextBox();
            this.totalAmount.TextChanged += TotalAmout_TextChanged;
            this.totalAmount.Location = new Point(this.procedure.Location.X + this.procedure.Size.Width + 25, this.procedure.Location.Y);
            this.totalAmount.Size = new Size(200, 20);
            this.totalAmount.BorderStyle = BorderStyle.FixedSingle;
            this.totalAmount.BackColor = Color.White;
            this.totalAmount.ForeColor = Color.Black;

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
                procTreatCommand.CommandText = $"SELECT ProcedureId FROM ProcedureTreatment WHERE DiagnosisId={diagId}";
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
                procCommand.CommandText = $"SELECT id, Name FROM Procedure";

                using (var procReader = procCommand.ExecuteReader())
                {
                    if (procReader.HasRows)
                    {
                        while (procReader.Read())
                        {
                            if (procIds.Contains(procReader.GetInt32(0)))
                            {
                                this.procedure.Items.Add(procReader.GetString(1));
                            }
                        }
                    }
                }

                connection.Close();
            }

            this.procLabel = new Label();
            this.procLabel.Text = "Процедура:";
            this.procLabel.AutoSize = true;
            this.procLabel.HandleCreated += ProcLabel_HandleCreated;

            this.amountLabel = new Label();
            this.amountLabel.Text = "Количество приёмов:";
            this.amountLabel.AutoSize = true;
            this.amountLabel.HandleCreated += AmountLabel_HandleCreated;

            this.applyButton = new Button();
            this.applyButton.Text = "Назначить";
            this.applyButton.BackColor = Color.White;
            this.applyButton.ForeColor = Color.Black;
            this.applyButton.AutoSize = true;
            this.applyButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.applyButton.HandleCreated += ApplyButton_HandleCreated;
            this.applyButton.Click += ApplyButton_Click;

            this.Controls.Add(procedure);
            this.Controls.Add(totalAmount);
            this.Controls.Add(procLabel);
            this.Controls.Add(amountLabel);
            this.Controls.Add(applyButton);
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.totalAmount.Text))
            {
                return;
            }

            string procName = this.procedure.SelectedItem?.ToString() ?? string.Empty;
            int totalAmount = int.Parse(this.totalAmount.Text);

            if (procName == null)
            {
                return;
            }

            int procId = -1;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();

                var procCommand = connection.CreateCommand();
                procCommand.Connection = connection;
                procCommand.CommandText = $"SELECT id FROM Procedure WHERE Name='{procName}'";

                using (var procReader = procCommand.ExecuteReader())
                {
                    if (procReader.HasRows)
                    {
                        procReader.Read();
                        procId = procReader.GetInt32(0);
                    }
                }

                if (procId == -1)
                {
                    connection.Close();
                    return;
                }

                var procRealCommand = connection.CreateCommand();
                procRealCommand.Connection = connection;

                procRealCommand.CommandText = $"SELECT TotalAmount FROM RealProcedureTreatment " +
                                              $"WHERE State='Назначено' AND ProcedureId={procId} AND DiseaseId={diseaseId}";

                int curAmount;
                using (var procRealReader = procRealCommand.ExecuteReader())
                {
                    if (procRealReader.HasRows)
                    {
                        procRealReader.Read();
                        curAmount = procRealReader.GetInt32(0);
                        MessageBox.Show(
                            $"Не удалось назначить процедуру, так как \n" +
                            $"данному пациенту уже назначено такая процедура \n" +
                            $"с общим количеством приёмов {curAmount}",
                            $"Ошибка!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                            );
                        return;
                    }
                }

                procRealCommand.CommandText = $"INSERT INTO RealProcedureTreatment (DiseaseId, ProcedureId, " +
                                              $"TotalAmount, AppointmentDate, LastChangeDate, State) " +
                                              $"VALUES ({diseaseId}, {procId}, {totalAmount}, " +
                                              $"'{DateTime.Today.Date.ToShortDateString()}', " +
                                              $"'{DateTime.Today.Date.ToShortDateString()}', 'Назначено')";

                procRealCommand.ExecuteNonQuery();

                connection.Close();
            }

            this.parent.ReloadDiseaseTable();
            this.Dispose();
        }

        private void ApplyButton_HandleCreated(object? sender, EventArgs e)
        {
            this.applyButton.Location = new Point(this.procedure.Location.X + this.procedure.Size.Width / 2, this.procedure.Location.Y + 45);
            this.applyButton.Size = new Size(215, 20);
        }

        private void TotalAmout_TextChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.totalAmount.Text))
            {
                this.totalAmount.Text = Regex.Replace(this.totalAmount.Text, @"[\sа-яА-ЯёЁa-zA-z\s]", "");
            }
        }

        private void AmountLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.amountLabel.Location = new Point(this.totalAmount.Location.X + this.totalAmount.Size.Width / 2 - this.amountLabel.Size.Width / 2, this.totalAmount.Location.Y - this.amountLabel.Size.Height);
        }

        private void ProcLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.procLabel.Location = new Point(this.procedure.Location.X + this.procedure.Size.Width / 2 - this.procLabel.Size.Width / 2, this.procedure.Location.Y - this.procLabel.Size.Height);
        }

        public AddProcedure(PatientInfo parent, int diseaseId)
        {
            this.parent = parent;
            this.diseaseId = diseaseId;
            this.Load += AddProcedure_Load;

            InitializeComponent();
        }
    }
}
