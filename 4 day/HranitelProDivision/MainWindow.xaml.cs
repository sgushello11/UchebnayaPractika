using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HranitelProDivision
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

            LoadRequests();

            btnFilter.Click += (s, e) => ApplyFilters();
            btnReset.Click += (s, e) => ResetFilters();
            btnLogout.Click += (s, e) => Logout();
            btnView.Click += (s, e) => OpenRequestDetails();

            dgvRequests.MouseDoubleClick += (s, e) => OpenRequestDetails();
        }

        private void LoadRequests()
        {
            allRequests = db.GetApprovedRequestsByDepartment(currentUser.Department);
            dgvRequests.ItemsSource = allRequests;
        }

        private void ApplyFilters()
        {
            var filtered = allRequests.ToList();

            if (dpDate.SelectedDate.HasValue)
            {
                DateTime selectedDate = dpDate.SelectedDate.Value.Date;
                filtered = filtered.Where(r => r.StartDate.Date == selectedDate).ToList();
            }

            dgvRequests.ItemsSource = filtered;
        }

        private void ResetFilters()
        {
            dpDate.SelectedDate = null;
            dgvRequests.ItemsSource = allRequests;
        }

        private void OpenRequestDetails()
        {
            var selected = dgvRequests.SelectedItem as VisitRequest;
            if (selected == null)
            {
                MessageBox.Show("Выберите заявку", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new RequestDetailsWindow(currentUser, selected);
            win.ShowDialog();
            LoadRequests();
            ApplyFilters();
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}