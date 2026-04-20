using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace HranitelProGeneralDept
{
    public partial class RequestDetailsWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private User currentUser;
        private VisitRequest request;
        private bool isInBlackList = false;
        private List<AttachedFile> attachedFiles;

        public RequestDetailsWindow(User user, VisitRequest req)
        {
            InitializeComponent();
            currentUser = user;
            request = req;

            LoadRequestData();
            LoadAttachedFiles();
            CheckBlackList();

            btnApprove.Click += (s, e) => ApproveRequest();
            btnReject.Click += (s, e) => RejectRequest();
            btnClose.Click += (s, e) => this.Close();
            btnCloseOnly.Click += (s, e) => this.Close();
            btnOpenFile.Click += (s, e) => OpenSelectedFile();

            lstFiles.MouseDoubleClick += (s, e) => OpenSelectedFile();
        }

        private void LoadRequestData()
        {
            lblRequestId.Text = request.RequestID.ToString();
            lblRequestType.Text = request.RequestType == "личная" ? "Личное посещение" : "Групповое посещение";
            lblStatus.Text = GetStatusText(request.Status);
            lblPurpose.Text = request.VisitPurpose;
            lblDepartment.Text = request.TargetDepartment;
            lblEmployee.Text = request.TargetEmployeeName;
            lblNote.Text = string.IsNullOrEmpty(request.Note) ? "—" : request.Note;

            // Показываем причину отказа, если заявка отклонена
            if (request.Status == "не одобрена" && !string.IsNullOrEmpty(request.RejectionReason))
            {
                borderRejectionReason.Visibility = Visibility.Visible;
                lblRejectionReason.Text = request.RejectionReason;
            }
            else
            {
                borderRejectionReason.Visibility = Visibility.Collapsed;
            }

            string fullName = $"{request.VisitorLastName} {request.VisitorFirstName} {request.VisitorPatronymic}".Trim();
            lblVisitorFullName.Text = string.IsNullOrEmpty(fullName) ? "—" : fullName;
            lblVisitorPhone.Text = string.IsNullOrEmpty(request.VisitorPhone) ? "—" : request.VisitorPhone;
            lblVisitorEmail.Text = string.IsNullOrEmpty(request.VisitorEmail) ? "—" : request.VisitorEmail;
            lblVisitorOrganization.Text = string.IsNullOrEmpty(request.VisitorOrganization) ? "—" : request.VisitorOrganization;
            lblVisitorBirthDate.Text = request.VisitorBirthDate.ToShortDateString();
            lblVisitorPassport.Text = string.IsNullOrEmpty(request.VisitorPassportData) ? "—" : request.VisitorPassportData;

            dpVisitDate.SelectedDate = request.StartDate;

            // Настройка видимости элементов в зависимости от статуса
            if (request.Status == "проверка")
            {
                borderApproval.Visibility = Visibility.Visible;
                panelActions.Visibility = Visibility.Visible;
                btnCloseOnly.Visibility = Visibility.Collapsed;
                btnApprove.IsEnabled = true;
                btnReject.IsEnabled = true;
            }
            else
            {
                borderApproval.Visibility = Visibility.Collapsed;
                panelActions.Visibility = Visibility.Collapsed;
                btnCloseOnly.Visibility = Visibility.Visible;
            }
        }

        private void LoadAttachedFiles()
        {
            attachedFiles = db.GetAttachedFiles(request.RequestID);
            lstFiles.ItemsSource = attachedFiles;

            if (attachedFiles.Count == 0)
            {
                lstFiles.ItemsSource = new List<AttachedFile> { new AttachedFile { FileName = "Нет прикреплённых файлов" } };
                btnOpenFile.IsEnabled = false;
            }
        }

        private void OpenSelectedFile()
        {
            if (lstFiles.SelectedItem == null)
            {
                MessageBox.Show("Выберите файл", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var file = lstFiles.SelectedItem as AttachedFile;
            if (file == null || string.IsNullOrEmpty(file.FilePath))
            {
                MessageBox.Show("Не удалось открыть файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (File.Exists(file.FilePath))
                {
                    Process.Start(new ProcessStartInfo(file.FilePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show($"Файл не найден: {file.FilePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetStatusText(string status)
        {
            switch (status)
            {
                case "проверка": return "⏳ На проверке";
                case "одобрена": return "✅ Одобрена";
                case "не одобрена": return "❌ Отклонена";
                default: return status;
            }
        }

        private void CheckBlackList()
        {
            if (!string.IsNullOrEmpty(request.VisitorPassportData))
            {
                isInBlackList = db.IsInBlackList(request.VisitorPassportData);

                if (isInBlackList)
                {
                    borderBlackList.Visibility = Visibility.Visible;
                    lblBlackList.Text = "⚠️ ВНИМАНИЕ! Посетитель находится в ЧЁРНОМ СПИСКЕ!\nЗаявка автоматически отклонена.";

                    string message = "Заявка на посещение объекта КИИ отклонена в связи с нарушением Федерального закона от 26.07.2017 № 187-ФЗ";
                    db.UpdateRequestStatus(request.RequestID, "не одобрена", message);
                    request.Status = "не одобрена";
                    request.RejectionReason = message;
                    lblStatus.Text = GetStatusText("не одобрена");

                    // Показываем причину отказа
                    borderRejectionReason.Visibility = Visibility.Visible;
                    lblRejectionReason.Text = message;

                    // Скрываем кнопки одобрения
                    borderApproval.Visibility = Visibility.Collapsed;
                    panelActions.Visibility = Visibility.Collapsed;
                    btnCloseOnly.Visibility = Visibility.Visible;

                    lblMessage.Text = message;
                }
                else
                {
                    borderBlackList.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool ValidateTime(string time, out TimeSpan parsedTime)
        {
            parsedTime = TimeSpan.Zero;
            if (TimeSpan.TryParse(time, out parsedTime))
                return true;

            if (time.Contains(":"))
            {
                string[] parts = time.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int hour) && int.TryParse(parts[1], out int minute))
                {
                    if (hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59)
                    {
                        parsedTime = new TimeSpan(hour, minute, 0);
                        return true;
                    }
                }
            }
            return false;
        }

        private void ApproveRequest()
        {
            if (!dpVisitDate.SelectedDate.HasValue)
            {
                lblMessage.Text = "Укажите дату посещения";
                return;
            }

            if (!ValidateTime(txtVisitTime.Text, out TimeSpan visitTime))
            {
                lblMessage.Text = "Укажите корректное время в формате ЧЧ:ММ (например, 14:30)";
                return;
            }

            DateTime visitDate = dpVisitDate.SelectedDate.Value;
            string message = $"Заявка на посещение объекта КИИ одобрена, дата посещения: {visitDate:dd.MM.yyyy}, время посещения: {visitTime:hh\\:mm}";

            db.UpdateRequestStatus(request.RequestID, "одобрена", null, visitDate, visitTime);
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void RejectRequest()
        {
            var dialog = new RejectReasonWindow();
            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.RejectionReason))
            {
                string message = $"Заявка на посещение объекта КИИ отклонена. Причина: {dialog.RejectionReason}";
                db.UpdateRequestStatus(request.RequestID, "не одобрена", message);

                if (dialog.RejectionReason.Contains("недостоверных") || dialog.RejectionReason.Contains("недостоверные"))
                {
                    int rejections = db.GetRejectionCountByPassport(request.VisitorPassportData);
                    if (rejections >= 1)
                    {
                        db.AddToBlackList(request.VisitorLastName, request.VisitorFirstName, request.VisitorPatronymic, request.VisitorPassportData);
                        MessageBox.Show("Посетитель добавлен в ЧЁРНЫЙ СПИСК!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show(message, "Заявка отклонена", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}