using System.Windows;
using System.Windows.Controls;

namespace HranitelProGeneralDept
{
    public partial class RejectReasonWindow : Window
    {
        public string RejectionReason { get; private set; } = "";

        public RejectReasonWindow()
        {
            InitializeComponent();

            cmbReason.SelectionChanged += (s, e) =>
            {
                var selected = (cmbReason.SelectedItem as ComboBoxItem)?.Content.ToString();
                txtOtherReason.Visibility = (selected == "Другое") ? Visibility.Visible : Visibility.Collapsed;
            };

            btnOk.Click += (s, e) =>
            {
                var selected = (cmbReason.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (selected == "Другое")
                {
                    if (string.IsNullOrWhiteSpace(txtOtherReason.Text))
                    {
                        lblError.Text = "Введите причину отклонения";
                        return;
                    }
                    RejectionReason = txtOtherReason.Text;
                }
                else
                {
                    RejectionReason = selected ?? "";
                }

                DialogResult = true;
                Close();
            };

            btnCancel.Click += (s, e) => Close();

            cmbReason.SelectedIndex = 0;
        }
    }
}