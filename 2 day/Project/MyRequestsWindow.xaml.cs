using System.Windows;

namespace HranitelPro
{
    public partial class MyRequestsWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private MainWindow mainWindow;
        private User currentUser;

        public MyRequestsWindow(MainWindow main, User user)
        {
            InitializeComponent();
            mainWindow = main;
            currentUser = user;

            var list = db.GetUserRequests(currentUser.UserID);
            dgvRequests.ItemsSource = list;

            btnBack.Click += (s, e) => { mainWindow.Show(); this.Close(); };
        }
    }
}