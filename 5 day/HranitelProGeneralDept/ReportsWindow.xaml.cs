using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HranitelProGeneralDept
{
    public partial class ReportsWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private User currentUser;

        public ReportsWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            LoadDepartments();

            cmbPeriod.SelectedIndex = 0;
            cmbDepartment.SelectedIndex = 0;

            btnShowReport.Click += (s, e) => LoadVisitsReport();
            btnManualReport.Click += (s, e) => CreateManualReport();
            // Используем SelectionChanged у TabControl
            tabControl.SelectionChanged += TabControl_SelectionChanged;
        }

        private void LoadDepartments()
        {
            var depts = db.GetDepartments();
            cmbDepartment.Items.Clear();
            cmbDepartment.Items.Add("Все");
            foreach (var dept in depts)
                cmbDepartment.Items.Add(dept);
            cmbDepartment.SelectedIndex = 0;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem == tabVisits)
            {
                gridVisits.Visibility = Visibility.Visible;
                gridCurrent.Visibility = Visibility.Collapsed;
            }
            else if (tabControl.SelectedItem == tabCurrent)
            {
                gridVisits.Visibility = Visibility.Collapsed;
                gridCurrent.Visibility = Visibility.Visible;
                LoadCurrentVisitors();
            }
        }

        private void LoadVisitsReport()
        {
            // Получаем текст из ComboBox (теперь на русском)
            var periodItem = cmbPeriod.SelectedItem as ComboBoxItem;
            string period = periodItem?.Content.ToString() ?? "день";

            string department = cmbDepartment.SelectedItem?.ToString();
            if (department == "Все")
                department = null;

            var dt = db.GetVisitsReport(period, department);
            dgvReport.ItemsSource = dt.DefaultView;
        }

        private void LoadCurrentVisitors()
        {
            var dt = db.GetCurrentVisitors();
            dgvCurrent.ItemsSource = dt.DefaultView;
        }

        private void CreateManualReport()
        {
            DateTime endTime = DateTime.Now;
            DateTime startTime = endTime.AddHours(-3);

            var dt = db.GetVisitorsByDepartmentReport(startTime, endTime);

            // Папка в директории проекта
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string reportsPath = Path.Combine(projectPath, "Отчеты ТБ");
            string todayFolder = Path.Combine(reportsPath, DateTime.Now.ToString("dd_MM_yyyy"));

            if (!Directory.Exists(todayFolder))
                Directory.CreateDirectory(todayFolder);

            string fileName = $"Отчет_ручной_{DateTime.Now:HH_mm}_за_3_часа.csv";
            string filePath = Path.Combine(todayFolder, fileName);

            SaveReportToCsv(dt, filePath);

            MessageBox.Show($"Отчет сохранен:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveReportToCsv(DataTable dt, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    writer.Write(dt.Columns[i].ColumnName);
                    if (i < dt.Columns.Count - 1)
                        writer.Write(";");
                }
                writer.WriteLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        writer.Write(row[i].ToString());
                        if (i < dt.Columns.Count - 1)
                            writer.Write(";");
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}