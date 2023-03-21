using Microsoft.Data.Sqlite;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    public partial class ChangeAmount : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;

        private ProceduresForm parent;
        private int procRealId;

        private Label info;
        private TextBox totalAmount;
        private Button applyButton;

        private void ChangeAmount_Load(object? sender, EventArgs e)
        {
            this.totalAmount = new TextBox();
            this.totalAmount.TextChanged += TotalAmount_TextChanged;
            this.totalAmount.Location = new Point(80, 80);
            this.totalAmount.Size = new Size(200, 20);
            this.totalAmount.BorderStyle = BorderStyle.FixedSingle;
            this.totalAmount.BackColor = Color.White;
            this.totalAmount.ForeColor = Color.Black;

            this.info = new Label();
            this.info.Text = "Количество приёмов можно изменить лишь раз в сутки.\n" +
                             "Если вы уверены, то введите новое количество\n" +
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

            this.Controls.Add(totalAmount);
            this.Controls.Add(info);
            this.Controls.Add(applyButton);
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.totalAmount.Text))
            {
                int newAmount = int.Parse(this.totalAmount.Text);

                using (var connection = new SqliteConnection(sqlConnectionString))
                {
                    connection.Open();

                    var procRealCommand = connection.CreateCommand();
                    procRealCommand.Connection = connection;
                    procRealCommand.CommandText = $"SELECT LastChangeDate FROM RealProcedureTreatment " +
                                                 $"WHERE id={procRealId}";

                    bool isAvailable = true;

                    using (var procRealReader = procRealCommand.ExecuteReader())
                    {
                        if (procRealReader.HasRows)
                        {
                            procRealReader.Read();
                            DateTime lastChange = DateTime.Parse(procRealReader.GetString(0));
                            if (lastChange.Date >= DateTime.Today.Date)
                            {
                                isAvailable = false;
                            }

                        }
                    }

                    if (isAvailable)
                    {
                        procRealCommand.CommandText = $"UPDATE RealProcedureTreatment SET " +
                                                      $"LastChangeDate='{DateTime.Today.Date.ToShortDateString()}', " +
                                                      $"TotalAmount={newAmount} " +
                                                      $"WHERE id={procRealId}";

                        procRealCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Не удалось изменить количество приёмов, так как \n" +
                            $"данному пациенту уже изменяли его сегодня",
                            $"Ошибка!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                            );
                    }

                    connection.Close();
                }
            }

            this.parent.ReloadProcsTable();
            this.Dispose();
        }

        private void ApplyButton_HandleCreated(object? sender, EventArgs e)
        {
            this.applyButton.Location = new Point(this.totalAmount.Location.X + this.totalAmount.Size.Width / 2 - this.applyButton.Size.Width / 2, this.totalAmount.Location.Y + this.totalAmount.Size.Height + 5);
        }

        private void TotalAmount_TextChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.totalAmount.Text))
            {
                this.totalAmount.Text = Regex.Replace(this.totalAmount.Text, @"[\sа-яА-ЯёЁa-zA-z\s]", "");
            }
        }

        public ChangeAmount(ProceduresForm parent, int procRealId)
        {
            this.parent = parent;
            this.procRealId = procRealId;
            this.Load += ChangeAmount_Load;

            InitializeComponent();
        }
    }
}
