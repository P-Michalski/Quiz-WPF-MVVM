using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Quiz.Models {
    public class Question {
        public string Text { get; set; }
        public ObservableCollection<Answer> Answers { get; set; } = new ObservableCollection<Answer>();
    }
}
