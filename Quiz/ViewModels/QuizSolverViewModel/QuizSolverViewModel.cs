using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Quiz.Models;
using Quiz.Services;

namespace Quiz.ViewModels
{
    public class QuizSolverViewModel : BaseViewModel
    {
        public QuizModel quiz { get; set; }
        public string Title { get; set; }
        private int _currentQuestionIndex;
        private int[][] savedAnswers;

        public bool IsQuizLoaded => quiz != null;

        public Question CurrentQuestion => quiz != null
            ? quiz.Questions.ElementAtOrDefault(CurrentQuestionIndex)
            : null;

        public ObservableCollection<Answer> CurrentAnswers =>
            CurrentQuestion != null && CurrentQuestion.Answers != null
                ? new ObservableCollection<Answer>(CurrentQuestion.Answers)
                : new ObservableCollection<Answer>();

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                if (_currentQuestionIndex != value)
                {
                    SaveSelectedAnswers();
                    _currentQuestionIndex = value;
                    OnPropertyChanged(nameof(CurrentQuestionIndex));
                    OnPropertyChanged(nameof(CurrentQuestion));
                    OnPropertyChanged(nameof(CurrentAnswers));
                    RestoreAnswers();
                }
            }
        }

        private readonly NavigationService _navigationService;
        private readonly QuizFileService _quizFileService;

        public ICommand NextQuestionCommand { get; }
        public ICommand EndQuizCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand LoadQuizCommand { get; }
        public ICommand SelectQuestionCommand { get; }

        public QuizSolverViewModel()
        {
            _quizFileService = new QuizFileService();
            _navigationService = new NavigationService();

            NextQuestionCommand = new RelayCommand(NextQuestion);
            EndQuizCommand = new RelayCommand(EndQuiz);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            LoadQuizCommand = new RelayCommand(LoadQuizFromComputer);
            SelectQuestionCommand = new RelayCommand<Question>(SelectQuestion);

        }

        private void LoadQuizFromComputer()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Quiz Files (*.quiz)|*.quiz",
                Title = "Wybierz plik quizu"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    quiz = _quizFileService.LoadQuiz(openFileDialog.FileName);
                    Title = quiz.Name;
                    CurrentQuestionIndex = 0;
                    savedAnswers = new int[quiz.Questions.Count][];

                    OnPropertyChanged(nameof(quiz));
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(CurrentQuestion));
                    OnPropertyChanged(nameof(CurrentAnswers));
                    OnPropertyChanged(nameof(IsQuizLoaded));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas ładowania quizu: {ex.Message}");
                }
            }
        }

        private void SelectQuestion(Question selectedQuestion)
        {
            if (selectedQuestion != null)
            {
                SaveSelectedAnswers();
                int newIndex = quiz.Questions.IndexOf(selectedQuestion);
                if (newIndex >= 0)
                {
                    CurrentQuestionIndex = newIndex;
                }
            }
        }

        private void NextQuestion()
        {
            if (CurrentQuestionIndex < quiz.Questions.Count - 1)
            {
                CurrentQuestionIndex++;
            }
        }

        private void EndQuiz()
        {
            SaveSelectedAnswers();
            int score = CheckQuizResults();
            MessageBox.Show($"Quiz zakończony! Twój wynik: {score}/{quiz.Questions.Count}");
        }

        private int CheckQuizResults()
        {
            int correctAnswers = 0;
            for (int i = 0; i < savedAnswers.Length; i++)
            {
                var correctIndices = quiz.Questions[i].Answers
                    .Select((answer, index) => answer.IsCorrect ? index : -1)
                    .Where(index => index != -1)
                    .ToArray();

                int[] selected = savedAnswers[i] ?? new int[0];

                if (selected.OrderBy(x => x).SequenceEqual(correctIndices.OrderBy(x => x)))
                {
                    correctAnswers++;
                }
            }
            return correctAnswers;
        }

        private void RestoreAnswers()
        {
            if (CurrentQuestion == null)
                return;

            if (savedAnswers[CurrentQuestionIndex] == null)
            {
                foreach (var answer in CurrentQuestion.Answers)
                {
                    answer.IsSelected = false;
                }
            }
            else
            {
                foreach (var answer in CurrentQuestion.Answers)
                {
                    int index = CurrentQuestion.Answers.IndexOf(answer);
                    answer.IsSelected = savedAnswers[CurrentQuestionIndex].Contains(index);
                }
            }
        }

        public void SaveSelectedAnswers()
        {
            if (CurrentQuestion == null)
                return;

            int[] selectedIndices = CurrentQuestion.Answers
                .Select((answer, index) => new { answer, index })
                .Where(pair => pair.answer.IsSelected)
                .Select(pair => pair.index)
                .ToArray();

            savedAnswers[CurrentQuestionIndex] = selectedIndices;
        }

        private void NavigateBack()
        {
            _navigationService.NavigateBackToMainWindow("QuizSolverView");
        }
    }
}
