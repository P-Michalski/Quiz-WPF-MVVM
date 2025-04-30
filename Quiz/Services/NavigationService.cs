using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Quiz.Services {
    public class NavigationService {
        public void NavigateBackToMainWindow(string currentWindowType) {
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Application.Current.Windows
                .OfType<Window>()
                .SingleOrDefault(w => w.GetType().Name == currentWindowType)?.Close();
        }
    }
}
