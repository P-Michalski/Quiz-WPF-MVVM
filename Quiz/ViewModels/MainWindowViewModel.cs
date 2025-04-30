using Quiz.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Quiz.ViewModels {
    public class MainWindowViewModel : BaseViewModel {
        public ICommand NavigateToQuizGeneratorCommand { get; }
        public ICommand NavigateToQuizSolverCommand { get; }

        public MainWindowViewModel() {
            NavigateToQuizGeneratorCommand = new RelayCommand(NavigateToQuizGenerator);
            NavigateToQuizSolverCommand = new RelayCommand(NavigateToQuizSolver);
        }

        private void NavigateToQuizGenerator() {
            var quizGeneratorWindow = new QuizGeneratorView();
            quizGeneratorWindow.Show();
            CloseMainWindow();
        }

        private void NavigateToQuizSolver() {
            var quizSolverWindow = new QuizSolverView();
            quizSolverWindow.Show();
            CloseMainWindow();
        }
        private void CloseMainWindow() {
            Application.Current.MainWindow.Close();
        }
    }
}
