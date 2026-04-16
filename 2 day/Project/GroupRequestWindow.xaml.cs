using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;

namespace HranitelPro
{
    public partial class GroupRequestWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private MainWindow mainWindow;
        private User currentUser;
        private RequestFull? editRequest;
        private string? selectedPassportFile;
        private int? existingPassportFileId;

        // Коллекция для списка посетителей
        private ObservableCollection<GroupMember> groupMembers = new ObservableCollection<GroupMember>();

        public GroupRequestWindow(MainWindow main, User user, RequestFull? requestToEdit = null)
        {
            InitializeComponent();
            mainWindow = main;
            currentUser = user;
            editRequest = requestToEdit;

            dpStartDate.SelectedDate = DateTime.Now.AddDays(1);
            dpStartDate.SelectedDateChanged += (s, e) =>
            {
                if (dpStartDate.SelectedDate.HasValue)
                    dpEndDate.SelectedDate = dpStartDate.SelectedDate.Value.AddDays(1);
            };

            cmbDepartment.ItemsSource = db.GetDepartments().DefaultView;
            cmbDepartment.SelectionChanged += (s, e) =>
            {
                if (cmbDepartment.SelectedItem != null)
                {
                    string dept = ((DataRowView)cmbDepartment.SelectedItem)["department"].ToString() ?? "";
                    if (!string.IsNullOrEmpty(dept))
                        cmbEmployee.ItemsSource = db.GetEmployees(dept).DefaultView;
                }
            };

            // Привязка списка посетителей
            dgvGroupMembers.ItemsSource = groupMembers;

            btnSubmit.Click += (s, e) => SubmitRequest();
            btnCancel.Click += (s, e) => this.Close();
            btnAttachPassport.Click += (s, e) => AttachFile();
            btnRemovePassport.Click += (s, e) => RemoveFile();

            // Обработчики
            btnDownloadTemplate.Click += BtnDownloadTemplate_Click;
            btnUploadList.Click += BtnUploadList_Click;
            btnAddVisitor.Click += BtnAddVisitor_Click;

            if (editRequest != null)
            {
                LoadRequestData();
                btnSubmit.Content = "Сохранить изменения";
                this.Title = "Редактирование групповой заявки";
            }
        }

        // ==================== ЗАГРУЗКА ДАННЫХ ====================
        private void LoadRequestData()
        {
            if (editRequest == null) return;

            dpStartDate.SelectedDate = editRequest.StartDate;
            dpEndDate.SelectedDate = editRequest.EndDate;
            txtPurpose.Text = editRequest.VisitPurpose;
            txtNote.Text = editRequest.Note;

            txtLastName.Text = editRequest.VisitorLastName;
            txtFirstName.Text = editRequest.VisitorFirstName;
            txtPatronymic.Text = editRequest.VisitorPatronymic;
            txtPhone.Text = editRequest.VisitorPhone;
            txtEmail.Text = editRequest.VisitorEmail;
            txtOrganization.Text = editRequest.VisitorOrganization;
            dpBirthDate.SelectedDate = editRequest.VisitorBirthDate;

            if (!string.IsNullOrEmpty(editRequest.VisitorPassportData) && editRequest.VisitorPassportData.Length >= 10)
            {
                txtPassportSeries.Text = editRequest.VisitorPassportData.Substring(0, 4);
                txtPassportNumber.Text = editRequest.VisitorPassportData.Substring(4, 6);
            }

            foreach (DataRowView item in cmbDepartment.Items)
            {
                if (item["department"].ToString() == editRequest.TargetDepartment)
                {
                    cmbDepartment.SelectedItem = item;
                    break;
                }
            }

            // Загрузка файлов
            var files = db.GetAttachedFiles(editRequest.RequestID);
            foreach (var file in files)
            {
                if (file.FileType == "passport_scan")
                {
                    existingPassportFileId = file.FileId;
                    selectedPassportFile = file.FilePath;
                    lblPassportFile.Text = file.FileName;
                    panelPassport.Visibility = Visibility.Visible;
                }
            }

            // Загрузка списка посетителей при редактировании
            var members = db.GetGroupMembersByRequestId(editRequest.RequestID);
            groupMembers.Clear();
            foreach (var m in members)
            {
                groupMembers.Add(m);
            }
        }

        // ==================== ФАЙЛЫ ====================
        private void AttachFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PDF files (*.pdf)|*.pdf";
            if (dialog.ShowDialog() == true)
            {
                selectedPassportFile = dialog.FileName;
                lblPassportFile.Text = System.IO.Path.GetFileName(selectedPassportFile);
                panelPassport.Visibility = Visibility.Visible;
                existingPassportFileId = null;
            }
        }

        private void RemoveFile()
        {
            selectedPassportFile = null;
            lblPassportFile.Text = "";
            panelPassport.Visibility = Visibility.Collapsed;
            existingPassportFileId = null;
        }

        // ==================== РАБОТА СО СПИСКОМ ПОСЕТИТЕЛЕЙ ====================

        private void BtnDownloadTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = "template_group_visitors.csv";
                dialog.Title = "Сохранить шаблон списка посетителей";

                if (dialog.ShowDialog() == true)
                {
                    string template = "№,Фамилия,Имя,Отчество,Телефон,Email,Дата рождения,Серия паспорта,Номер паспорта\n";
                    template += "1,Иванов,Иван,Иванович,89001234567,ivan@example.com,01.01.1990,1234,567890\n";
                    template += "2,Петров,Петр,Петрович,89007654321,peter@example.com,15.05.1985,2345,678901\n";
                    template += "3,Сидоров,Сидор,Сидорович,89001112233,sidor@example.com,20.12.1988,3456,789012\n";

                    System.IO.File.WriteAllText(dialog.FileName, template, System.Text.Encoding.UTF8);
                    MessageBox.Show($"Шаблон сохранён: {dialog.FileName}\n\nОткрыть его можно в Excel или любом текстовом редакторе.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUploadList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx";
                dialog.Title = "Выберите файл со списком посетителей";

                if (dialog.ShowDialog() == true)
                {
                    // Используем UTF-8 вместо 1251
                    string[] lines = System.IO.File.ReadAllLines(dialog.FileName, System.Text.Encoding.UTF8);

                    if (lines.Length < 2)
                    {
                        MessageBox.Show("Файл пуст или не содержит данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int addedCount = 0;
                    // Определяем разделитель (запятая или точка с запятой)
                    char separator = lines[0].Contains(';') ? ';' : ',';

                    // Пропускаем заголовок (первая строка)
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        string[] parts = line.Split(separator);
                        if (parts.Length >= 9)
                        {
                            var member = new GroupMember
                            {
                                LastName = parts[1].Trim(),
                                FirstName = parts[2].Trim(),
                                Patronymic = parts[3].Trim(),
                                Phone = parts[4].Trim(),
                                Email = parts[5].Trim(),
                                PassportData = parts[7].Trim() + parts[8].Trim()
                            };

                            // Парсим дату
                            if (DateTime.TryParse(parts[6].Trim(), out DateTime birthDate))
                                member.BirthDate = birthDate;
                            else
                                member.BirthDate = DateTime.Now.AddYears(-20);

                            groupMembers.Add(member);
                            addedCount++;
                        }
                    }

                    MessageBox.Show($"Загружено {addedCount} посетителей", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddVisitor_Click(object sender, RoutedEventArgs e)
        {
            var addVisitorWindow = new AddVisitorWindow();
            addVisitorWindow.Owner = this;

            if (addVisitorWindow.ShowDialog() == true && addVisitorWindow.NewVisitor != null)
            {
                groupMembers.Add(addVisitorWindow.NewVisitor);
                lblStatus.Text = $"Добавлен посетитель: {addVisitorWindow.NewVisitor.LastName} {addVisitorWindow.NewVisitor.FirstName}";
            }
        }

        // ==================== ОТПРАВКА ЗАЯВКИ ====================
        private void SubmitRequest()
        {
            // Проверка обязательных полей организатора
            if (string.IsNullOrEmpty(txtLastName.Text) || string.IsNullOrEmpty(txtFirstName.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) || cmbDepartment.SelectedItem == null || cmbEmployee.SelectedValue == null)
            {
                lblStatus.Text = "Заполните все обязательные поля организатора!";
                return;
            }

            if (txtPassportSeries.Text.Length != 4 || txtPassportNumber.Text.Length != 6)
            {
                lblStatus.Text = "Серия паспорта — 4 цифры, номер — 6 цифр!";
                return;
            }

            if (!dpBirthDate.SelectedDate.HasValue || dpBirthDate.SelectedDate.Value > DateTime.Now.AddYears(-16))
            {
                lblStatus.Text = "Возраст организатора должен быть не младше 16 лет!";
                return;
            }

            if (groupMembers.Count < 5)
            {
                lblStatus.Text = $"В группе должно быть не менее 5 человек (сейчас {groupMembers.Count})";
                return;
            }

            string dept = ((DataRowView)cmbDepartment.SelectedItem)["department"].ToString() ?? "";
            int empId = Convert.ToInt32(cmbEmployee.SelectedValue);
            string passportData = txtPassportSeries.Text + txtPassportNumber.Text;

            int requestId;
            if (editRequest == null)
            {
                requestId = db.CreateGroupRequestWithMembers(
                    currentUser.UserID, dpStartDate.SelectedDate.Value, dpEndDate.SelectedDate.Value,
                    txtPurpose.Text, dept, empId, txtNote.Text,
                    txtLastName.Text, txtFirstName.Text, txtPatronymic.Text, txtPhone.Text,
                    txtEmail.Text, txtOrganization.Text, dpBirthDate.SelectedDate.Value, passportData,
                    selectedPassportFile, groupMembers);

                if (requestId > 0)
                {
                    MessageBox.Show($"Групповая заявка подана! Количество посетителей: {groupMembers.Count}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    mainWindow.RefreshRequests();
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "Ошибка при сохранении заявки";
                }
            }
            else
            {
                requestId = db.UpdateGroupRequestWithMembers(
                    editRequest.RequestID, dpStartDate.SelectedDate.Value, dpEndDate.SelectedDate.Value,
                    txtPurpose.Text, dept, empId, txtNote.Text,
                    txtLastName.Text, txtFirstName.Text, txtPatronymic.Text, txtPhone.Text,
                    txtEmail.Text, txtOrganization.Text, dpBirthDate.SelectedDate.Value, passportData,
                    selectedPassportFile, groupMembers, existingPassportFileId);

                if (requestId > 0)
                {
                    MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    mainWindow.RefreshRequests();
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "Ошибка при сохранении изменений";
                }
            }
        }
    }
}