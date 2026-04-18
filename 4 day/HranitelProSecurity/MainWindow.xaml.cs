using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HranitelProSecurity
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

            dpDate.SelectedDate = DateTime.Now;

            btnFilter.Click += (s, e) => ApplyFilters();
            btnReset.Click += (s, e) => ResetFilters();
            btnSearch.Click += (s, e) => ApplyFilters();
            btnLogout.Click += (s, e) => Logout();
            btnAccess.Click += (s, e) => OpenAccessWindow("entry");
            btnExit.Click += (s, e) => OpenAccessWindow("exit");

            dgvRequests.MouseDoubleClick += (s, e) => OpenAccessWindow("entry");
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
            allRequests = db.GetApprovedRequests();
            dgvRequests.ItemsSource = allRequests;
        }

        private void ApplyFilters()
        {
            var filtered = allRequests.ToList();

            // Фильтр по дате
            if (dpDate.SelectedDate.HasValue)
            {
                DateTime selectedDate = dpDate.SelectedDate.Value.Date;
                filtered = filtered.Where(r => r.StartDate.Date == selectedDate).ToList();
            }

            // Фильтр по типу
            string type = (cmbType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Все";
            if (type != "Все")
                filtered = filtered.Where(r => r.RequestType == type).ToList();

            // Фильтр по подразделению
            string department = cmbDepartment.SelectedItem?.ToString();
            if (department != null && department != "Все")
                filtered = filtered.Where(r => r.TargetDepartment == department).ToList();

            // Поиск
            string search = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(r =>
                    r.VisitorLastName.ToLower().Contains(search) ||
                    r.VisitorFirstName.ToLower().Contains(search) ||
                    (r.VisitorPatronymic?.ToLower().Contains(search) ?? false) ||
                    r.VisitorPassportData.Contains(search)
                ).ToList();
            }

            dgvRequests.ItemsSource = filtered;
        }

        private void ResetFilters()
        {
            dpDate.SelectedDate = DateTime.Now;
            cmbType.SelectedIndex = 0;
            cmbDepartment.SelectedIndex = 0;
            txtSearch.Text = "";
            dgvRequests.ItemsSource = allRequests;
        }

        private void OpenAccessWindow(string type)
        {
            var selected = dgvRequests.SelectedItem as VisitRequest;
            if (selected == null)
            {
                MessageBox.Show("Выберите заявку", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new AccessWindow(currentUser, selected, type);
            win.ShowDialog();

            allRequests = db.GetApprovedRequests();
            dgvRequests.ItemsSource = allRequests;
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}