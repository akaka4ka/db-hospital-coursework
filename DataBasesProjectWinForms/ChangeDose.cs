using Microsoft.Data.Sqlite;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class ChangeDose : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private MedicinesForm parent;
        private int medRealId;

        private Label info;
        private TextBox dailyDose;
        private Button applyButton;

        private void ChangeDose_Load(object? sender, EventArgs e)
        {
            this.dailyDose = new TextBox();
            this.dailyDose.TextChanged += DailyDose_TextChanged;
            this.dailyDose.Location = new Point(80, 80);
            this.dailyDose.Size = new Size(200, 20);
            this.dailyDose.BorderStyle = BorderStyle.FixedSingle;
            this.dailyDose.BackColor = Color.White;
            this.dailyDose.ForeColor = Color.Black;

            this.info = new Label();
            this.info.Text = "Дневную дозу можно изменить лишь раз в сутки.\n" +
                             "Если вы уверены, то введите новую дозу (мг/день)\n" +
                             "и нажмите Принять";
            this.info.AutoSize = true;
            this.info.Location = new Point(5, 5);

            this.applyButton = new Button();
            this.applyButton.Text = "Применить";
            this.applyButton.BackColor = Color.White;
            this.applyButton.ForeColor = Color.Black;
            this.applyButton.AutoSize = true;
            this.applyButton.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.applyButton.HandleCreated += ApplyButton_HandleCreated;
            this.applyButton.Click += ApplyButton_Click;

            this.Controls.Add(dailyDose);
            this.Controls.Add(info);
            this.Controls.Add(applyButton);
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.dailyDose.Text))
            {
                int newDose = int.Parse(this.dailyDose.Text);

                using (var connection = new SqliteConnection(sqlConnectionString))
                {
                    connection.Open();

                    var medRealCommand = connection.CreateCommand();
                    medRealCommand.Connection = connection;
                    medRealCommand.CommandText = $"SELECT LastChangeDate FROM RealMedicineTreatment " +
                                                 $"WHERE id={medRealId}";

                    bool isAvailable = true;

                    using (var medRealReader = medRealCommand.ExecuteReader())
                    {
                        if (medRealReader.HasRows)
                        {
                            medRealReader.Read();
                            DateTime lastChange = DateTime.Parse(medRealReader.GetString(0));
                            if (lastChange.Date >= DateTime.Today.Date)
                            {
                                isAvailable = false;
                            }
                            
                        }
                    }

                    if (isAvailable)
                    {
                        medRealCommand.CommandText = $"UPDATE RealMedicineTreatment SET " +
                                                             $"LastChangeDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                             $"IntakePerDay={newDose} " +
                                                             $"WHERE id={medRealId}";

                        medRealCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Не удалось изменить дозу, так как \n" +
                            $"данному пациенту уже изменяли её сегодня",
                            $"Ошибка!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                            );
                    }

                    connection.Close();
                }
            }

            this.parent.ReloadMedsTable();
            this.Dispose();
        }

        private void ApplyButton_HandleCreated(object? sender, EventArgs e)
        {
            this.applyButton.Location = new Point(this.dailyDose.Location.X + this.dailyDose.Size.Width / 2 - this.applyButton.Size.Width / 2, this.dailyDose.Location.Y + this.dailyDose.Size.Height + 5);
        }

        private void DailyDose_TextChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.dailyDose.Text))
            {
                this.dailyDose.Text = Regex.Replace(this.dailyDose.Text, @"[\sа-яА-ЯёЁa-zA-z\s]", "");
            }
        }

        public ChangeDose(MedicinesForm parent, int medRealId)
        {
            this.parent = parent;
            this.medRealId = medRealId;
            this.Load += ChangeDose_Load;

            InitializeComponent();
        }
    }
}
