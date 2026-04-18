using System.Windows;

namespace HranitelProSecurity
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

            if (user != null && user.Department == "Охрана")
            {
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
            }
            else if (user != null && user.Department != "Охрана")
            {
                lblError.Text = "Доступ только для сотрудников охраны";
            }
            else
            {
                lblError.Text = "Неверный код сотрудника";
            }
        }
    }
}