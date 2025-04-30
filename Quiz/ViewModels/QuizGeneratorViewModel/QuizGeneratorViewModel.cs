using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Quiz.Models;
using System.Windows;
using Quiz.Services;

namespace Quiz.ViewModels {
    public class QuizGeneratorViewModel : BaseViewModel {
        private string _quizName;
        public string QuizName { 
            get => _quizName; 
            set {
                _quizName = value;
                OnPropertyChanged(nameof(QuizName));
            }
        }
        
        private ObservableCollection<Question> _questions = new ObservableCollection<Question>();
        public ObservableCollection<Question> Questions { 
            get => _questions; 
            set {
                _questions = value;
                OnPropertyChanged(nameof(Questions));
            }
        }
        
        private readonly NavigationService _navigationService;
        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }
        public ICommand AddAnswerCommand { get; }
        public ICommand RemoveAnswerCommand { get; }
        public ICommand SaveQuizCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand ToggleCorrectAnswerCommand { get; }
        public ICommand LoadQuizCommand { get; }

        public QuizGeneratorViewModel() {
            _navigationService = new NavigationService();
            AddQuestionCommand = new RelayCommand(AddQuestion);
            RemoveQuestionCommand = new RelayCommand<Question>(RemoveQuestion);
            AddAnswerCommand = new RelayCommand<Question>(AddAnswer);
            RemoveAnswerCommand = new RelayCommand<Answer>(RemoveAnswerByAnswer);
            SaveQuizCommand = new RelayCommand(SaveQuiz);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            ToggleCorrectAnswerCommand = new RelayCommand<Answer>(ToggleCorrectAnswer);
            LoadQuizCommand = new RelayCommand(LoadQuiz);
        }

        private void AddQuestion() {
            Questions.Add(new Question { Text = "Nowe pytanie" });
        }

        private void RemoveQuestion(Question question) {
            if (question != null) {
                Questions.Remove(question);
            }
        }

        private void AddAnswer(Question question) {
            if (question != null) {
                question.Answers.Add(new Answer { Text = "Nowa odpowiedź" });
            }
        }

        private void RemoveAnswerByAnswer(Answer answer) {
            if (answer != null) {
                foreach (var question in Questions) {
                    if (question.Answers.Contains(answer)) {
                        question.Answers.Remove(answer);
                        break;
                    }
                }
            }
        }

        private void ToggleCorrectAnswer(Answer answer) {
            if (answer != null) {
                foreach (var question in Questions) {
                    if (question.Answers.Contains(answer)) {
                        answer.IsCorrect = !answer.IsCorrect;
                        OnPropertyChanged(nameof(Questions));
                        break;
                    }
                }
            }
        }

        private void SaveQuiz() {
            try {
                var quiz = new QuizModel {
                    Name = QuizName,
                    Questions = Questions
                };

                var fileService = new QuizFileService();
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                    Filter = "Quiz Files (*.quiz)|*.quiz",
                    Title = "Zapisz Quiz"
                };

                if (saveFileDialog.ShowDialog() == true) {
                    fileService.SaveQuiz(quiz, saveFileDialog.FileName);
                    MessageBox.Show("Quiz został zapisany pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas zapisywania quizu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadQuiz() {
            try {
                var fileService = new QuizFileService();
                var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                    Filter = "Quiz Files (*.quiz)|*.quiz",
                    Title = "Wczytaj Quiz"
                };

                if (openFileDialog.ShowDialog() == true) {
                    var quiz = fileService.LoadQuiz(openFileDialog.FileName);
                    QuizName = quiz.Name;
                    Questions.Clear();
                    foreach (var question in quiz.Questions) {
                        Questions.Add(question);
                    }
                    MessageBox.Show("Quiz został wczytany pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas wczytywania quizu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateBack() {
            _navigationService.NavigateBackToMainWindow("QuizGeneratorView");
        }
    }
}

