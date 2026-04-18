using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HranitelProDivision
{
    public partial class RequestDetailsWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private User currentUser;
        private VisitRequest request;

        public RequestDetailsWindow(User user, VisitRequest req)
        {
            InitializeComponent();
            currentUser = user;
            request = req;

            LoadData();

            btnEntry.Click += (s, e) => SetEntryTime();
            btnExit.Click += (s, e) => SetExitTime();
            btnClose.Click += (s, e) => this.Close();
            btnAddToBlacklist.Click += (s, e) => AddToBlacklist();

            // Контекстное меню для ФИО (правый клик)
            lblVisitorFullName.MouseRightButtonDown += LblVisitorFullName_MouseRightButtonDown;
            lblVisitorFullName.Cursor = Cursors.Hand;
            lblVisitorFullName.TextDecorations = TextDecorations.Underline;
            lblVisitorFullName.Foreground = System.Windows.Media.Brushes.Blue;
        }


        private void LoadData()
        {
            // Информация о заявке
            lblRequestId.Text = request.RequestID.ToString();
            lblRequestType.Text = request.RequestType == "личная" ? "Личное посещение" : "Групповое посещение";
            lblStatus.Text = request.Status;
            lblPurpose.Text = request.VisitPurpose;
            lblDepartment.Text = request.TargetDepartment;
            lblNote.Text = string.IsNullOrEmpty(request.Note) ? "—" : request.Note;

            // Информация о посетителе
            lblVisitorFullName.Text = request.FullName;
            lblVisitorPhone.Text = string.IsNullOrEmpty(request.VisitorPhone) ? "—" : request.VisitorPhone;
            lblVisitorEmail.Text = string.IsNullOrEmpty(request.VisitorEmail) ? "—" : request.VisitorEmail;
            lblVisitorOrganization.Text = string.IsNullOrEmpty(request.VisitorOrganization) ? "—" : request.VisitorOrganization;
            lblVisitorBirthDate.Text = request.VisitorBirthDate.ToShortDateString();
            lblVisitorPassport.Text = string.IsNullOrEmpty(request.VisitorPassportData) ? "—" : request.VisitorPassportData;

            // Время от охраны (разрешение доступа)
            if (request.ActualEntryTime.HasValue)
            {
                lblEntryTime.Text = request.ActualEntryTime.Value.ToString("HH:mm:ss dd.MM.yyyy");
            }
            else
            {
                lblEntryTime.Text = "Не получено";
            }

            // Время прихода (фиксация подразделением)
            if (request.DivisionEntryTime.HasValue)
            {
                lblDivisionEntryTime.Text = request.DivisionEntryTime.Value.ToString("HH:mm:ss dd.MM.yyyy");
                lblDivisionEntryTime.Visibility = Visibility.Visible;
                (lblDivisionEntryTime.Parent as StackPanel).Children[0].Visibility = Visibility.Visible;
            }
            else
            {
                lblDivisionEntryTime.Text = "Не зафиксирован";
                lblDivisionEntryTime.Visibility = Visibility.Visible;
                (lblDivisionEntryTime.Parent as StackPanel).Children[0].Visibility = Visibility.Visible;
            }

            // Время выхода
            if (request.ActualExitTime.HasValue)
            {
                lblExitTime.Text = request.ActualExitTime.Value.ToString("HH:mm:ss dd.MM.yyyy");
            }
            else
            {
                lblExitTime.Text = "Не зафиксирован";
            }

            // Логика кнопок
            // Если охрана разрешила доступ (есть ActualEntryTime)
            if (request.ActualEntryTime.HasValue)
            {
                // Если сотрудник подразделения ещё не зафиксировал приход
                if (!request.DivisionEntryTime.HasValue)
                {
                    btnEntry.IsEnabled = true;
                    btnExit.IsEnabled = false;
                    lblMessage.Text = "✅ Охрана разрешила доступ. Нажмите 'Зафиксировать вход' для отметки прихода посетителя.";
                }
                else
                {
                    // Приход уже зафиксирован
                    btnEntry.IsEnabled = false;

                    // Если выход ещё не зафиксирован
                    if (!request.ActualExitTime.HasValue)
                    {
                        btnExit.IsEnabled = true;
                        lblMessage.Text = "✅ Приход зафиксирован. Нажмите 'Зафиксировать выход' для завершения посещения.";
                    }
                    else
                    {
                        btnExit.IsEnabled = false;
                        lblMessage.Text = "✅ Посещение завершено. Выход зафиксирован.";
                    }
                }
            }
            else
            {
                btnEntry.IsEnabled = false;
                btnExit.IsEnabled = false;
                lblMessage.Text = "⏳ Ожидание разрешения от охраны... Посетитель ещё не прошёл турникет.";
            }
        }

        private void SetEntryTime()
        {
            if (!request.ActualEntryTime.HasValue)
            {
                MessageBox.Show("Охрана ещё не разрешила доступ!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (request.DivisionEntryTime.HasValue)
            {
                MessageBox.Show("Вход уже зафиксирован!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime now = DateTime.Now;
            SystemSounds.Beep.Play();

            int result = db.SetDivisionEntryTime(request.RequestID, now);
            if (result > 0)
            {
                MessageBox.Show($"✅ ПРИХОД ПОСЕТИТЕЛЯ ЗАФИКСИРОВАН!\n\nВремя прихода: {now:HH:mm:ss dd.MM.yyyy}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении времени прихода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetExitTime()
        {
            if (!request.DivisionEntryTime.HasValue)
            {
                MessageBox.Show("Сначала зафиксируйте вход посетителя!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (request.ActualExitTime.HasValue)
            {
                MessageBox.Show("Выход уже зафиксирован!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime now = DateTime.Now;

            int result = db.SetExitTime(request.RequestID, now);
            if (result > 0)
            {
                MessageBox.Show($"✅ ВЫХОД ПОСЕТИТЕЛЯ ЗАФИКСИРОВАН!\n\nВремя выхода: {now:HH:mm:ss dd.MM.yyyy}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении времени выхода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LblVisitorFullName_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowBlacklistMenu();
        }

        private void AddToBlacklist()
        {
            ShowBlacklistMenu();
        }

        private void ShowBlacklistMenu()
        {
            var menu = new ContextMenu();
            var item = new MenuItem { Header = "🚫 Добавить в черный список" };
            item.Click += (s, ev) => OpenAddToBlacklistWindow();
            menu.Items.Add(item);
            lblVisitorFullName.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void OpenAddToBlacklistWindow()
        {
            var win = new AddToBlacklistWindow(request.VisitorLastName, request.VisitorFirstName,
                request.VisitorPatronymic, request.VisitorPassportData);
            win.ShowDialog();
        }
    }
}