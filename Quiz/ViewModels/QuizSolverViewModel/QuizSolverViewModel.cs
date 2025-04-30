using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Quiz.Models;
using Quiz;
using System.Windows;
using Quiz.Services;

namespace Quiz.ViewModels {
    public class QuizSolverViewModel : BaseViewModel {
        public ObservableCollection<Question> Questions { get; set; }
        public int CurrentQuestionIndex { get; set; }
        private readonly NavigationService _navigationService;
        public ICommand StartQuizCommand { get; }
        public ICommand SubmitAnswerCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public QuizSolverViewModel() {
            StartQuizCommand = new RelayCommand(StartQuiz);
            SubmitAnswerCommand = new RelayCommand(SubmitAnswer);
            _navigationService = new NavigationService();
            NavigateBackCommand = new RelayCommand(NavigateBack);
        }

        private void StartQuiz() {
            // Logika wczytania quizu z pliku
        }

        private void SubmitAnswer() {
            // Logika sprawdzania odpowiedzi
        }
        private void NavigateBack() {
            _navigationService.NavigateBackToMainWindow("QuizSolverView");
        }
    }
}

// Update any references to QuizModel.Questions to ensure compatibility with ObservableCollection
// For example, if binding to the collection, ensure the binding works with ObservableCollection.
