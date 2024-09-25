using System.ComponentModel;

namespace SimpleTodo
{
    public class TodoModel : INotifyPropertyChanged
    {
        public bool finished
        {
            get => _finished;
            set
            {
                if (_finished != value)
                {
                    _finished = value;
                    OnPropertyChanged(nameof(finished));
                }
            }
        }
        public string title { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _finished;

        public TodoModel(bool finished, string title)
        {
            this.finished = finished;
            this.title = title;
        }

        public object ToJson() {
            return new
            {
                finished,
                title
            };
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
