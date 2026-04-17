using System.Windows;

namespace HranitelProGeneralDept
{
    public partial class LoginWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();

        public LoginWindow()
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Password.Trim();
            var user = db.LoginByEmployeeCode(code);

            if (user != null && user.Department == "Общий отдел")
            {
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
            }
            else if (user != null && user.Department != "Общий отдел")
            {
                lblError.Text = "Доступ только для сотрудников общего отдела";
            }
            else
            {
                lblError.Text = "Неверный код сотрудника";
            }
        }
    }
}