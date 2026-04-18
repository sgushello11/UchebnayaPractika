using System;
using System.Media;
using System.Windows;

namespace HranitelProSecurity
{
    public partial class AccessWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private User currentUser;
        private VisitRequest request;
        private string actionType;
        private bool isInBlacklist;

        public AccessWindow(User user, VisitRequest req, string type)
        {
            InitializeComponent();
            currentUser = user;
            request = req;
            actionType = type;

            // Проверка черного списка
            isInBlacklist = db.IsInBlacklist(request.VisitorPassportData);

            LoadData();

            btnAction.Click += BtnAction_Click;
            btnClose.Click += (s, e) => this.Close();
        }

        private void LoadData()
        {
            // Если в черном списке - показываем сообщение и блокируем кнопку
            if (isInBlacklist)
            {
                lblFullName.Text = request.FullName;
                lblPassport.Text = request.VisitorPassportData;
                lblPhone.Text = string.IsNullOrEmpty(request.VisitorPhone) ? "—" : request.VisitorPhone;
                lblDepartment.Text = request.TargetDepartment;
                lblType.Text = request.RequestType == "личная" ? "Личное посещение" : "Групповое посещение";
                lblPurpose.Text = request.VisitPurpose;
                lblDate.Text = request.StartDate.ToShortDateString();

                lblTimeInfo.Text = "🚫 ПОСЕТИТЕЛЬ В ЧЕРНОМ СПИСКЕ!\nДоступ запрещен.";
                btnAction.IsEnabled = false;
                btnAction.Content = "❌ ДОСТУП ЗАПРЕЩЕН";
                btnAction.Background = System.Windows.Media.Brushes.DarkGray;
                return;
            }

            lblFullName.Text = request.FullName;
            lblPassport.Text = request.VisitorPassportData;
            lblPhone.Text = string.IsNullOrEmpty(request.VisitorPhone) ? "—" : request.VisitorPhone;
            lblDepartment.Text = request.TargetDepartment;
            lblType.Text = request.RequestType == "личная" ? "Личное посещение" : "Групповое посещение";
            lblPurpose.Text = request.VisitPurpose;
            lblDate.Text = request.StartDate.ToShortDateString();

            if (actionType == "entry")
            {
                this.Title = "Разрешение на доступ";
                btnAction.Content = "🚪 РАЗРЕШИТЬ ДОСТУП";
                btnAction.Background = System.Windows.Media.Brushes.DarkGreen;

                if (request.ActualEntryTime.HasValue)
                {
                    lblTimeInfo.Text = $"✅ Вход уже зафиксирован: {request.ActualEntryTime.Value:HH:mm:ss dd.MM.yyyy}";
                    btnAction.IsEnabled = false;
                }
                else
                {
                    lblTimeInfo.Text = "⚠️ Посетитель ещё не прошёл на территорию";
                }
            }
            else
            {
                this.Title = "Фиксация убытия";
                btnAction.Content = "⏱️ ЗАФИКСИРОВАТЬ ВЫХОД";
                btnAction.Background = System.Windows.Media.Brushes.DarkOrange;

                if (request.ActualExitTime.HasValue)
                {
                    lblTimeInfo.Text = $"✅ Выход уже зафиксирован: {request.ActualExitTime.Value:HH:mm:ss dd.MM.yyyy}";
                    btnAction.IsEnabled = false;
                }
                else if (!request.ActualEntryTime.HasValue)
                {
                    lblTimeInfo.Text = "⚠️ Посетитель ещё не прошёл вход! Сначала зафиксируйте вход.";
                    btnAction.IsEnabled = false;
                }
                else
                {
                    lblTimeInfo.Text = $"Вход зафиксирован: {request.ActualEntryTime.Value:HH:mm:ss dd.MM.yyyy}";
                }
            }
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            // Дополнительная проверка перед действием
            if (isInBlacklist)
            {
                MessageBox.Show("Доступ запрещен! Посетитель находится в черном списке.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime now = DateTime.Now;
            SystemSounds.Beep.Play();

            if (actionType == "entry")
            {
                int result = db.SetEntryTime(request.RequestID, now);
                if (result > 0)
                {
                    MessageBox.Show($"✅ ДОСТУП РАЗРЕШЁН!\n\nВремя входа: {now:HH:mm:ss dd.MM.yyyy}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при сохранении времени входа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                int result = db.SetExitTime(request.RequestID, now);
                if (result > 0)
                {
                    MessageBox.Show($"✅ ВЫХОД ЗАФИКСИРОВАН!\n\nВремя выхода: {now:HH:mm:ss dd.MM.yyyy}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при сохранении времени выхода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}