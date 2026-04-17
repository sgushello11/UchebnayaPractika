using System.Windows;

namespace HranitelPro
{
    public partial class TypeChoiceWindow : Window
    {
        private MainWindow mainWindow;
        private User currentUser;

        public TypeChoiceWindow(MainWindow main, User user)
        {
            InitializeComponent();
            mainWindow = main;
            currentUser = user;

            btnPersonal.Click += (s, e) => {
                RequestWindow requestWindow = new RequestWindow(mainWindow, currentUser, null);
                requestWindow.ShowDialog();
                this.Close();
            };

            btnGroup.Click += (s, e) => {
                GroupRequestWindow groupWindow = new GroupRequestWindow(mainWindow, currentUser, null);
                groupWindow.ShowDialog();
                this.Close();
            };

            btnCancel.Click += (s, e) => this.Close();
        }
    }
}