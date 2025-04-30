using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Quiz.Models {
    public class QuizModel {
        public string Name { get; set; }
        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
    }
}
