using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace DataBasesProjectWinForms
{
    public partial class AddNewDoctor : Form
    {
        private string sqlConnectionString = Config.sqlConnectionString;
        private int marginTopText = 30;
        private int marginField = 50;
        private MainForm parent;

        private TextBox docName;
        private TextBox docSurname;

        private Label docNameLabel;
        private Label docSurnameLabel;
        private Label depNameLabel;

        private ComboBox depName;
        
        private Button applyButton;

        public AddNewDoctor(MainForm parent)
        {
            this.parent = parent;
            
            this.Load += Form2_Load;

            InitializeComponent();
        }

        private void Form2_Load(object? sender, EventArgs e)
        {
            this.docName = new TextBox();
            this.docName.Location = new Point(marginField, marginField);
            this.docName.Size = new Size(200, 20);
            this.docName.BorderStyle = BorderStyle.FixedSingle;
            this.docName.BackColor = Color.White;
            this.docName.ForeColor = Color.Black;

            this.docSurname = new TextBox();
            this.docSurname.Location = new Point(marginField + docName.Size.Width + 15, marginField);
            this.docSurname.Size = new Size(200, 20);
            this.docSurname.BorderStyle = BorderStyle.FixedSingle;
            this.docSurname.BackColor = Color.White;
            this.docSurname.ForeColor = Color.Black;

            this.depName = new ComboBox();
            this.depName.Location = new Point(docSurname.Location.X + docSurname.Size.Width + 15, marginField);
            this.depName.Size = new Size(200, 20);
            this.depName.BackColor = Color.White;
            this.depName.ForeColor = Color.Black;
            this.depName.DropDownStyle = ComboBoxStyle.DropDownList;

            this.docNameLabel = new Label();
            this.docNameLabel.Text = "Имя:";
            this.docNameLabel.AutoSize = true;
            this.docNameLabel.HandleCreated += DocNameLabel_HandleCreated;

            this.docSurnameLabel = new Label();
            this.docSurnameLabel.Text = "Фамилия:";
            this.docSurnameLabel.AutoSize = true;
            this.docSurnameLabel.HandleCreated += DocSurnameLabel_HandleCreated;

            this.depNameLabel = new Label();
            this.depNameLabel.Text = "Отделение:";
            this.depNameLabel.AutoSize = true;
            this.depNameLabel.HandleCreated += DepNameLabel_HandleCreated;

            this.applyButton = new Button();
            this.applyButton.Text = "Оформить";
            this.applyButton.BackColor = Color.White;
            this.applyButton.ForeColor = Color.Black;
            this.applyButton.AutoSize = true;
            this.applyButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.applyButton.HandleCreated += ApplyButton_HandleCreated;
            this.applyButton.Click += ApplyButton_Click;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();
                SqliteCommand depCommand = connection.CreateCommand();

                depCommand.Connection = connection;
                depCommand.CommandText = "SELECT Name FROM Department";

                using (SqliteDataReader depReader = depCommand.ExecuteReader())
                {
                    if (depReader.HasRows)
                    {
                        while (depReader.Read())
                        {
                            string depName = depReader.GetString(0);

                            this.depName.Items.Add(depName);
                        }
                    }
                }

                connection.Close();
            }

            this.Controls.Add(docName);
            this.Controls.Add(docSurname);
            this.Controls.Add(depName);
            this.Controls.Add(docNameLabel);
            this.Controls.Add(docSurnameLabel);
            this.Controls.Add(depNameLabel);
            this.Controls.Add(applyButton);
        }

        private async void ApplyButton_Click(object? sender, EventArgs e)
        {
            string name = this.docName.Text;
            string surname = this.docSurname.Text;
            string depName = this.depName.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        this.docName.BackColor = Color.LightPink;
                    }

                    if (string.IsNullOrWhiteSpace(surname))
                    {
                        this.docSurname.BackColor = Color.LightPink;
                    }

                    await Task.Delay(250);


                    if (string.IsNullOrWhiteSpace(name))
                    {
                        this.docName.BackColor = Color.White;
                    }

                    if (string.IsNullOrWhiteSpace(surname))
                    {
                        this.docSurname.BackColor = Color.White;
                    }

                    await Task.Delay(250);
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(depName))
            {
                return;
            }

            InsertNewDoctor(name, surname, depName);
        }

        private void InsertNewDoctor(string name, string surname, string depName)
        {
            int depId = -1;

            using (var connection = new SqliteConnection(sqlConnectionString))
            {
                connection.Open();
                SqliteCommand depCommand = connection.CreateCommand();
                SqliteCommand docCommand = connection.CreateCommand();

                depCommand.Connection = connection;
                depCommand.CommandText = $"SELECT id FROM Department WHERE Name='{depName}'";

                docCommand.Connection = connection;

                using (SqliteDataReader depReader = depCommand.ExecuteReader())
                {
                    if (depReader.HasRows)
                    {
                        depReader.Read();
                        depId = depReader.GetInt32(0);

                        if (depId != -1)
                        {
                            docCommand.CommandText = $"INSERT INTO Doctor (Name, Surname, DepartmentId, HireDate, isFiring)" +
                                                     $"VALUES ('{name}', '{surname}', {depId}, '{DateTime.Today.Date.ToShortDateString()}', '{false}')";

                            docCommand.ExecuteNonQuery();
                        }
                    }
                }

                connection.Close();
            }

            parent.NewDoctorInserted();

            this.Dispose();
        }

        private void ApplyButton_HandleCreated(object? sender, EventArgs e)
        {
            this.applyButton.Location = new Point(this.docSurnameLabel.Location.X - 5, this.ClientSize.Height - this.applyButton.Size.Height - marginField);
        }

        private void DocNameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.docNameLabel.Location = new Point(this.docName.Location.X + this.docName.Size.Width / 2 - this.docNameLabel.Size.Width / 2, marginTopText);
        }

        private void DocSurnameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.docSurnameLabel.Location = new Point(this.docSurname.Location.X + this.docSurname.Size.Width / 2 - this.docSurnameLabel.Size.Width / 2, marginTopText);
        }

        private void DepNameLabel_HandleCreated(object? sender, EventArgs e)
        {
            this.depNameLabel.Location = new Point(this.depName.Location.X + this.depName.Size.Width / 2 - this.depNameLabel.Size.Width / 2, marginTopText);
        }
    }
}
