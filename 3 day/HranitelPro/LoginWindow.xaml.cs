using System;
using System.Windows;

namespace HranitelPro
{
    public partial class LoginWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private MainWindow mainWindow;

        public LoginWindow(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;

            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;
            string hash = db.HashMD5(password);

            if (db.LoginSQL(login, hash))
            {
                User user = db.LoginORM(login, hash);
                mainWindow.ShowMainMenu(user);
                this.Close();
            }
            else
            {
                lblError.Text = "Неверный логин или пароль";
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(mainWindow, this);
            registerWindow.Show();
            this.Hide();
        }
    }
}