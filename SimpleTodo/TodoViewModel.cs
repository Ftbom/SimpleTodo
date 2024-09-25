using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SimpleTodo
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?> _canExecute;


        public RelayCommand(Action<object?> execute)
            : this(execute, DefaultCanExecute)
        {
        }

        public RelayCommand(Action<object?> execute, Predicate<object?> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        //默认的CanExecute
        private static bool DefaultCanExecute(object? parameter)
        {
            return true;
        }


        //接口实现
        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }

    public class BooleanToTextDecorationConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return ((bool)value) ? TextDecorations.Strikethrough : null;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TodoViewModel
    {
        public ObservableCollection<TodoModel> Todos { get; set; }
        public ICommand AddNewTodoCommand { get; private set; }
        public ICommand DeleteTodoCommand { get; private set; }

        public TodoViewModel()
        {
            Todos = new ObservableCollection<TodoModel>();
            if (File.Exists("todo.json"))
            {
                using (var fileStream = new FileStream("todo.json", FileMode.Open, FileAccess.Read))
                {
                    List<TodoModel>? todos = JsonSerializer.Deserialize<List<TodoModel>>(fileStream);
                    if (todos != null)
                    {
                        foreach (TodoModel todo in todos)
                        {
                            todo.PropertyChanged += OnTodoChanged;
                            Todos.Add(todo);
                        }
                    }
                }
            }
            Todos.CollectionChanged += OnTodosChanged;
            AddNewTodoCommand = new RelayCommand(AddNewTodo);
            DeleteTodoCommand = new RelayCommand(DeleteTodo);
        }

        private void OnTodosChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveTodos();
        }

        private void OnTodoChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveTodos();
        }

        private void SaveTodos()
        {
            List<object> todo_json = new List<object>();
            foreach (TodoModel todo in Todos)
            {
                todo_json.Add(todo.ToJson());
            }
            File.WriteAllText("todo.json", JsonSerializer.Serialize(todo_json));
        }

        private void AddNewTodo(object? parameter)
        {
            if (parameter != null)
            {
                string title = ((TextBox)parameter).Text;
                if ((title != null) && (title.Length > 0))
                {
                    try {
                        TodoModel todo = new TodoModel(false, title);
                        todo.PropertyChanged += OnTodoChanged;
                        Todos.Add(todo);
                        ((TextBox)parameter).Text = "";
                    } catch { }
                }
            }
        }

        private void DeleteTodo(object? parameter)
        {
            if (parameter != null)
            {
                TodoModel todo = ((TodoModel)parameter);
                Todos.Remove(todo);
            }
        }
    }
}