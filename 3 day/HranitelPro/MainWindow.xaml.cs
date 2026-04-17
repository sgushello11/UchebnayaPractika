using System.Windows;

namespace HranitelPro
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private User? currentUser;
        private RequestItem? selectedRequest;

        public MainWindow()
        {
            InitializeComponent();
            ShowLoginWindow();

            btnLogout.Click += (s, e) => Logout();
            btnAdd.Click += (s, e) => ShowRequestTypeChoice();
            btnEdit.Click += (s, e) => EditRequest();
            btnDelete.Click += (s, e) => DeleteRequest();
            btnRefresh.Click += (s, e) => LoadRequests();

            dgvRequests.SelectionChanged += (s, e) =>
            {
                selectedRequest = dgvRequests.SelectedItem as RequestItem;
            };
        }

        public void ShowLoginWindow()
        {
            LoginWindow loginWindow = new LoginWindow(this);
            loginWindow.Show();
            this.Hide();
        }

        public void ShowMainMenu(User user)
        {
            currentUser = user;
            lblUser.Text = $"{user.FirstName} {user.LastName}";
            LoadRequests();
            this.Show();
        }

        private void LoadRequests()
        {
            if (currentUser != null)
            {
                var requests = db.GetUserRequests(currentUser.UserID);
                dgvRequests.ItemsSource = requests;
            }
        }

        private void ShowRequestTypeChoice()
        {
            if (currentUser != null)
            {
                TypeChoiceWindow typeChoice = new TypeChoiceWindow(this, currentUser);
                typeChoice.ShowDialog();
                LoadRequests();
            }
        }

        private void EditRequest()
        {
            if (selectedRequest == null)
            {
                MessageBox.Show("Выберите заявку для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fullRequest = db.GetRequestById(selectedRequest.Id);
            if (fullRequest != null)
            {
                if (fullRequest.RequestType == "личная")
                {
                    RequestWindow requestWindow = new RequestWindow(this, currentUser!, fullRequest);
                    requestWindow.ShowDialog();
                }
                else
                {
                    GroupRequestWindow groupWindow = new GroupRequestWindow(this, currentUser!, fullRequest);
                    groupWindow.ShowDialog();
                }
                LoadRequests();
            }
        }

        private void DeleteRequest()
        {
            if (selectedRequest == null)
            {
                MessageBox.Show("Выберите заявку для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить заявку №{selectedRequest.Id}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                db.DeleteRequest(selectedRequest.Id);
                LoadRequests();
            }
        }

        private void Logout()
        {
            currentUser = null;
            ShowLoginWindow();
        }

        public void RefreshRequests()
        {
            LoadRequests();
        }
    }
}