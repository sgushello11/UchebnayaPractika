using System.Windows;

namespace HranitelProDivision
{
    public partial class AddToBlacklistWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private string lastName, firstName, patronymic, passportData;
        private bool isInBlacklist;

        public AddToBlacklistWindow(string last, string first, string patron, string passport)
        {
            InitializeComponent();
            lastName = last;
            firstName = first;
            patronymic = patron;
            passportData = passport;

            lblFullName.Text = $"{lastName} {firstName} {patronymic}".Trim();
            lblPassport.Text = passportData;

            // Проверяем, есть ли уже в черном списке
            isInBlacklist = db.IsInBlacklist(passportData);

            if (isInBlacklist)
            {
                // Показываем блок удаления
                borderAdd.Visibility = Visibility.Collapsed;
                borderRemove.Visibility = Visibility.Visible;
                lblExistingReason.Text = db.GetBlacklistReason(passportData);
            }
            else
            {
                // Показываем блок добавления
                borderAdd.Visibility = Visibility.Visible;
                borderRemove.Visibility = Visibility.Collapsed;
            }

            btnAdd.Click += (s, e) => AddToBlacklist();
            btnRemove.Click += (s, e) => RemoveFromBlacklist();
            btnClose.Click += (s, e) => this.Close();
        }

        private void AddToBlacklist()
        {
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                lblError.Text = "Укажите причину добавления";
                return;
            }

            int result = db.AddToBlacklist(lastName, firstName, patronymic, passportData, txtReason.Text);
            if (result > 0)
            {
                MessageBox.Show("Посетитель добавлен в черный список!\n\nВсе последующие заявки будут автоматически отклоняться.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                lblError.Text = "Ошибка при добавлении";
            }
        }

        private void RemoveFromBlacklist()
        {
            var result = MessageBox.Show("Удалить посетителя из черного списка?\n\nПосле удаления он сможет подавать заявки.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int rows = db.RemoveFromBlacklist(passportData);
                if (rows > 0)
                {
                    MessageBox.Show("Посетитель удален из черного списка!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    lblError.Text = "Ошибка при удалении";
                }
            }
        }
    }
}