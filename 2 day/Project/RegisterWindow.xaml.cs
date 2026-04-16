using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace HranitelPro
{
    public partial class RegisterWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private MainWindow mainWindow;
        private LoginWindow loginWindow;

        public RegisterWindow(MainWindow main, LoginWindow login)
        {
            InitializeComponent();
            mainWindow = main;
            loginWindow = login;

            btnRegister.Click += BtnRegister_Click;
            btnBack.Click += (s, e) => { loginWindow.Show(); this.Close(); };
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                lblError.Text = "Заполните обязательные поля";
                return;
            }

            // Проверка пароля
            string pwd = txtPassword.Password;
            if (!Regex.IsMatch(pwd, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{8,}$"))
            {
                lblError.Text = "Пароль: 8+ символов, заглавная, строчная, цифра, спецсимвол";
                return;
            }

            // Проверка совпадения паролей
            if (pwd != txtConfirmPassword.Password)
            {
                lblError.Text = "Пароли не совпадают";
                return;
            }

            // Проверка возраста
            if (!dpBirthDate.SelectedDate.HasValue)
            {
                lblError.Text = "Выберите дату рождения";
                return;
            }

            DateTime birthDate = dpBirthDate.SelectedDate.Value;
            if (birthDate > DateTime.Now.AddYears(-16))
            {
                lblError.Text = "Возраст не младше 16 лет";
                return;
            }

            // Проверка паспорта
            if (txtPassportSeries.Text.Length != 4 || txtPassportNumber.Text.Length != 6)
            {
                lblError.Text = "Серия 4 цифры, номер 6 цифр";
                return;
            }

            string passport = txtPassportSeries.Text + txtPassportNumber.Text;
            string hash = db.HashMD5(pwd);

            // Регистрация
            bool success = db.RegisterSQL(
                txtLastName.Text,
                txtFirstName.Text,
                string.IsNullOrEmpty(txtPatronymic.Text) ? null : txtPatronymic.Text,
                string.IsNullOrEmpty(txtPhone.Text) ? null : txtPhone.Text,
                txtEmail.Text,
                birthDate,
                passport,
                txtLogin.Text,
                hash);

            if (success)
            {
                MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                loginWindow.Show();
                this.Close();
            }
            else
            {
                lblError.Text = "Логин или Email уже заняты";
            }
        }
    }
}