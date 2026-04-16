using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace HranitelPro
{
    public partial class RequestWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private MainWindow mainWindow;
        private User currentUser;
        private RequestFull? editRequest;
        private string? selectedPassportFile;
        private string? selectedPhotoFile;
        private int? existingPassportFileId;
        private int? existingPhotoFileId;

        public RequestWindow(MainWindow main, User user, RequestFull? requestToEdit = null)
        {
            InitializeComponent();
            mainWindow = main;
            currentUser = user;
            editRequest = requestToEdit;

            dpStartDate.SelectedDate = DateTime.Now.AddDays(1);
            dpStartDate.DisplayDateStart = DateTime.Now.AddDays(1);
            dpStartDate.DisplayDateEnd = DateTime.Now.AddDays(15);

            dpStartDate.SelectedDateChanged += (s, e) =>
            {
                if (dpStartDate.SelectedDate.HasValue)
                {
                    dpEndDate.DisplayDateStart = dpStartDate.SelectedDate.Value;
                    dpEndDate.DisplayDateEnd = dpStartDate.SelectedDate.Value.AddDays(15);
                    dpEndDate.SelectedDate = dpStartDate.SelectedDate.Value.AddDays(1);
                }
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

            btnSubmit.Click += (s, e) => SubmitRequest();
            btnClear.Click += (s, e) => ClearForm();
            btnCancel.Click += (s, e) => this.Close();
            btnAttachPassport.Click += (s, e) => AttachFile("passport_scan");
            btnAttachPhoto.Click += (s, e) => AttachFile("photo");
            btnRemovePassport.Click += (s, e) => RemoveFile("passport_scan");
            btnRemovePhoto.Click += (s, e) => RemoveFile("photo");

            if (editRequest != null)
            {
                LoadRequestData();
                btnSubmit.Content = "Сохранить изменения";
                this.Title = "Редактирование заявки";
            }
        }

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

            // Загрузка прикреплённых файлов
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
                else if (file.FileType == "photo")
                {
                    existingPhotoFileId = file.FileId;
                    selectedPhotoFile = file.FilePath;
                    lblPhotoFile.Text = file.FileName;
                    panelPhoto.Visibility = Visibility.Visible;
                }
            }
        }

        private void AttachFile(string type)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (type == "passport_scan")
            {
                dialog.Filter = "PDF files (*.pdf)|*.pdf";
                dialog.Title = "Выберите скан паспорта (PDF)";
            }
            else
            {
                dialog.Filter = "JPG files (*.jpg)|*.jpg";
                dialog.Title = "Выберите фото (JPG, 3x4)";
            }

            if (dialog.ShowDialog() == true)
            {
                if (type == "passport_scan")
                {
                    selectedPassportFile = dialog.FileName;
                    lblPassportFile.Text = System.IO.Path.GetFileName(selectedPassportFile);
                    panelPassport.Visibility = Visibility.Visible;
                    existingPassportFileId = null; // Новый файл, старый удалим
                }
                else
                {
                    selectedPhotoFile = dialog.FileName;
                    lblPhotoFile.Text = System.IO.Path.GetFileName(selectedPhotoFile);
                    panelPhoto.Visibility = Visibility.Visible;
                    existingPhotoFileId = null; // Новый файл, старый удалим
                }
            }
        }

        private void RemoveFile(string type)
        {
            if (type == "passport_scan")
            {
                selectedPassportFile = null;
                lblPassportFile.Text = "";
                panelPassport.Visibility = Visibility.Collapsed;
                existingPassportFileId = null;
            }
            else
            {
                selectedPhotoFile = null;
                lblPhotoFile.Text = "";
                panelPhoto.Visibility = Visibility.Collapsed;
                existingPhotoFileId = null;
            }
        }

        private void ClearForm()
        {
            txtLastName.Text = txtFirstName.Text = txtPatronymic.Text = txtPhone.Text =
            txtEmail.Text = txtOrganization.Text = txtPurpose.Text = txtNote.Text = "";
            txtPassportSeries.Text = txtPassportNumber.Text = "";
            dpBirthDate.SelectedDate = DateTime.Now.AddYears(-20);
            dpStartDate.SelectedDate = DateTime.Now.AddDays(1);
            cmbDepartment.SelectedItem = null;
            cmbEmployee.ItemsSource = null;
            lblStatus.Text = "";
            RemoveFile("passport_scan");
            RemoveFile("photo");
        }

        private void SubmitRequest()
        {
            // Проверка обязательных полей
            if (string.IsNullOrEmpty(txtLastName.Text) || string.IsNullOrEmpty(txtFirstName.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) || cmbDepartment.SelectedItem == null || cmbEmployee.SelectedValue == null)
            {
                lblStatus.Text = "Заполните все обязательные поля!";
                return;
            }

            if (txtPassportSeries.Text.Length != 4 || txtPassportNumber.Text.Length != 6)
            {
                lblStatus.Text = "Серия — 4 цифры, номер — 6 цифр!";
                return;
            }

            if (!dpBirthDate.SelectedDate.HasValue || dpBirthDate.SelectedDate.Value > DateTime.Now.AddYears(-16))
            {
                lblStatus.Text = "Возраст должен быть не младше 16 лет!";
                return;
            }

            if (string.IsNullOrEmpty(selectedPassportFile) && editRequest == null)
            {
                lblStatus.Text = "Прикрепите скан паспорта!";
                return;
            }

            string dept = ((DataRowView)cmbDepartment.SelectedItem)["department"].ToString() ?? "";
            int empId = Convert.ToInt32(cmbEmployee.SelectedValue);
            string passportData = txtPassportSeries.Text + txtPassportNumber.Text;

            int requestId;
            if (editRequest == null)
            {
                // НОВАЯ ЗАЯВКА
                requestId = db.CreateRequest(
                    currentUser.UserID,
                    dpStartDate.SelectedDate.Value,
                    dpEndDate.SelectedDate.Value,
                    txtPurpose.Text,
                    dept,
                    empId,
                    txtNote.Text,
                    txtLastName.Text,
                    txtFirstName.Text,
                    txtPatronymic.Text,
                    txtPhone.Text,
                    txtEmail.Text,
                    txtOrganization.Text,
                    dpBirthDate.SelectedDate.Value,
                    passportData);

                if (requestId > 0)
                {
                    // Сохраняем скан паспорта
                    if (!string.IsNullOrEmpty(selectedPassportFile) && System.IO.File.Exists(selectedPassportFile))
                    {
                        string fileName = System.IO.Path.GetFileName(selectedPassportFile);
                        db.AddAttachedFile(requestId, "passport_scan", selectedPassportFile, fileName);
                    }

                    // Сохраняем фото
                    if (!string.IsNullOrEmpty(selectedPhotoFile) && System.IO.File.Exists(selectedPhotoFile))
                    {
                        string fileName = System.IO.Path.GetFileName(selectedPhotoFile);
                        db.AddAttachedFile(requestId, "photo", selectedPhotoFile, fileName);
                    }

                    MessageBox.Show("Заявка подана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                // РЕДАКТИРОВАНИЕ ЗАЯВКИ
                requestId = db.UpdateRequest(
                    editRequest.RequestID,
                    dpStartDate.SelectedDate.Value,
                    dpEndDate.SelectedDate.Value,
                    txtPurpose.Text,
                    dept,
                    empId,
                    txtNote.Text,
                    txtLastName.Text,
                    txtFirstName.Text,
                    txtPatronymic.Text,
                    txtPhone.Text,
                    txtEmail.Text,
                    txtOrganization.Text,
                    dpBirthDate.SelectedDate.Value,
                    passportData);

                if (requestId > 0)
                {
                    // Обновляем скан паспорта
                    if (!string.IsNullOrEmpty(selectedPassportFile) && System.IO.File.Exists(selectedPassportFile))
                    {
                        if (existingPassportFileId.HasValue)
                            db.DeleteAttachedFile(existingPassportFileId.Value);
                        string fileName = System.IO.Path.GetFileName(selectedPassportFile);
                        db.AddAttachedFile(editRequest.RequestID, "passport_scan", selectedPassportFile, fileName);
                    }
                    else if (existingPassportFileId.HasValue && string.IsNullOrEmpty(selectedPassportFile))
                    {
                        db.DeleteAttachedFile(existingPassportFileId.Value);
                    }

                    // Обновляем фото
                    if (!string.IsNullOrEmpty(selectedPhotoFile) && System.IO.File.Exists(selectedPhotoFile))
                    {
                        if (existingPhotoFileId.HasValue)
                            db.DeleteAttachedFile(existingPhotoFileId.Value);
                        string fileName = System.IO.Path.GetFileName(selectedPhotoFile);
                        db.AddAttachedFile(editRequest.RequestID, "photo", selectedPhotoFile, fileName);
                    }
                    else if (existingPhotoFileId.HasValue && string.IsNullOrEmpty(selectedPhotoFile))
                    {
                        db.DeleteAttachedFile(existingPhotoFileId.Value);
                    }

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