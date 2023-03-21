using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataBasesProjectWinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(776, 426);
            this.tabControl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Менеджер данных больницы";
            this.ResumeLayout(false);

        }

        #endregion

        private Form hireDoctorForm;
        private Form newPatientForm;

        private TabControl tabControl;

        //
        // MainTab
        //
        private TabPage mainTab;
        private Button badDocsRepButton;
        private Button bestDocsRepButton;
        private Button disFreqRepButton;
        private Button yearHisRepButton;
        private Button allHisRepButton;

        private DateTimePicker freqStartDate;
        private DateTimePicker freqEndDate;

        //
        // DoctorsTab
        //
        private TabPage doctorsTab;
        private DataGridView doctorsTable;
        private Button fireDocButton;
        private Button hireDocButton;

        //
        // PatientTab
        //
        private TabPage patientsTab;
        private DataGridView patientsTable;
        private Button newPatientButton;
        private Button changeStateButton;
        private ComboBox changeStateCombo;
        private Button infoButton;
        private Button changeDepButton;
    }
}

