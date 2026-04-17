using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HranitelProGeneralDept
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private User currentUser;
        private List<VisitRequest> allRequests = new List<VisitRequest>();

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            lblUser.Text = currentUser.FullName;

            LoadDepartments();
            LoadRequests();

            btnFilter.Click += (s, e) => FilterRequests();
            btnReset.Click += (s, e) => ResetFilters();
            btnLogout.Click += (s, e) => Logout();
            btnView.Click += (s, e) => ViewRequest();

            dgvRequests.MouseDoubleClick += (s, e) => ViewRequest();
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

        private void LoadRequests()
        {
            allRequests = db.GetAllRequests();
            dgvRequests.ItemsSource = null;
            dgvRequests.ItemsSource = allRequests;
        }

        private void FilterRequests()
        {
            string type = (cmbType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Все";
            string status = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Все";
            string department = cmbDepartment.SelectedItem?.ToString() == "Все" ? null : cmbDepartment.SelectedItem?.ToString();

            var filtered = allRequests;

            if (type != "Все")
                filtered = filtered.FindAll(r => r.RequestType == type);
            if (status != "Все")
                filtered = filtered.FindAll(r => r.Status == status);
            if (!string.IsNullOrEmpty(department))
                filtered = filtered.FindAll(r => r.TargetDepartment == department);

            dgvRequests.ItemsSource = null;
            dgvRequests.ItemsSource = filtered;
        }

        private void ResetFilters()
        {
            cmbType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbDepartment.SelectedIndex = 0;
            dgvRequests.ItemsSource = null;
            dgvRequests.ItemsSource = allRequests;
        }

        private void ViewRequest()
        {
            var selected = dgvRequests.SelectedItem as VisitRequest;
            if (selected == null)
            {
                MessageBox.Show("Выберите заявку", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detailsWindow = new RequestDetailsWindow(currentUser, selected);
            detailsWindow.ShowDialog();

            // Обновляем данные
            LoadRequests();
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}