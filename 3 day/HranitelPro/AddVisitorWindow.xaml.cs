using System;
using System.Windows;

namespace HranitelPro
{
    public partial class AddVisitorWindow : Window
    {
        public GroupMember? NewVisitor { get; private set; }

        public AddVisitorWindow()
        {
            InitializeComponent();

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                lblError.Text = "Введите фамилию";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                lblError.Text = "Введите имя";
                return;
            }

            // Создаём посетителя
            NewVisitor = new GroupMember
            {
                LastName = txtLastName.Text.Trim(),
                FirstName = txtFirstName.Text.Trim(),
                Patronymic = string.IsNullOrWhiteSpace(txtPatronymic.Text) ? null : txtPatronymic.Text.Trim(),
                Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                BirthDate = dpBirthDate.SelectedDate ?? DateTime.Now.AddYears(-20),
                PassportData = txtPassportSeries.Text.Trim() + txtPassportNumber.Text.Trim()
            };

            this.DialogResult = true;
            this.Close();
        }
    }
}