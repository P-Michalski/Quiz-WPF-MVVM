using Microsoft.Win32;
using Quiz.Models;
using Quiz.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;

public class QuizSolverViewModel : BaseViewModel
{
    public QuizModel quiz { get; set; }
    public string Title { get; set; }
    private int _currentQuestionIndex;
    private int[][] savedAnswers;
    private bool _isQuizLocked;
    private DispatcherTimer _quizTimer;
    private DateTime _quizStartTime;
    private string _elapsedTimeString;  
    public string ElapsedTimeString
    {
        get => _elapsedTimeString;
        set
        {
            if (_elapsedTimeString != value)
            {
                _elapsedTimeString = value;
                OnPropertyChanged(nameof(ElapsedTimeString));
            }
        }
    }
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

    public bool IsQuizLocked
    {
        get => _isQuizLocked;
        set
        {
            if (_isQuizLocked != value)
            {
                _isQuizLocked = value;
                OnPropertyChanged(nameof(IsQuizLocked));
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

                // reset blokady, gdy ładujesz nowy quiz
                IsQuizLocked = false;
                StartQuizTimer();
                        
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
    private void StartQuizTimer()
    {
        _quizStartTime = DateTime.Now;
        ElapsedTimeString = "00:00:00"; // początkowa wartość

        _quizTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _quizTimer.Tick += Timer_Tick;
        _quizTimer.Start();
    }
    private void Timer_Tick(object sender, EventArgs e)
    {
        var elapsed = DateTime.Now - _quizStartTime;
        // Format hh:mm:ss
        ElapsedTimeString = elapsed.ToString(@"hh\:mm\:ss");
    }
    private TimeSpan GetElapsedTime()
    {
        return DateTime.Now - _quizStartTime;
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
        _quizTimer?.Stop();
        TimeSpan elapsed = DateTime.Now - _quizStartTime;
        MessageBox.Show($"Quiz zakończony! Twój wynik: {score}/{quiz.Questions.Count}\nCzas rozwiązywania: {elapsed.ToString(@"hh\:mm\:ss")}");
        ShowDetailedResults();
        LockQuiz();
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

    private void ShowDetailedResults()
    {
        string resultsMessage = "Szczegóły wyników:\n\n";

        for (int i = 0; i < savedAnswers.Length; i++)
        {
            var question = quiz.Questions[i];
            var correctIndices = question.Answers
                .Select((answer, index) => answer.IsCorrect ? index : -1)
                .Where(index => index != -1)
                .ToArray();

            int[] selected = savedAnswers[i] ?? new int[0];

            bool isCorrect = selected.OrderBy(x => x).SequenceEqual(correctIndices.OrderBy(x => x));
            string status = isCorrect ? "Poprawnie" : "Błędnie";

            resultsMessage += $"{i + 1}. {question.Text}\n{status}\n\n";
        }

        MessageBox.Show(resultsMessage, "Podsumowanie wyników");
    }

    private void LockQuiz()
    {
        IsQuizLocked = true;
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

    private void NavigateBack()
    {
        _navigationService.NavigateBackToMainWindow("QuizSolverView");
    }
}
