using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToDoList.Converters;
using ToDoList.Data;
using ToDoList.Models;
using System.Linq;

namespace ToDoList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TodoContext _context;
        private ObservableCollection<Todo> _todos;

        public MainWindow()
        {
            InitializeComponent();
            _context = new TodoContext();
            _todos = new ObservableCollection<Todo>(_context.GetAllTodos());
            TodoListView.ItemsSource = _todos;
            NewTodoTextBox.KeyDown += NewTodoTextBox_KeyDown;
        }

        private void NewTodoTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddButton_Click(sender, e);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewTodoTextBox.Text))
                return;

            var todo = new Todo
            {
                Title = NewTodoTextBox.Text.Trim(),
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };

            _context.AddTodo(todo);
            _todos.Insert(0, todo);
            NewTodoTextBox.Clear();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Todo todo)
            {
                if (todo.IsCompleted)
                {
                    todo.CompletedAt = DateTime.Now;
                }
                else
                {
                    todo.CompletedAt = null;
                }
                
                _context.UpdateTodo(todo);
                
                // Reorder the list using LINQ
                var sortedTodos = _todos.OrderBy(t => t.IsCompleted)
                                     .ThenByDescending(t => t.IsCompleted ? t.CompletedAt : t.CreatedAt)
                                     .ToList();
                
                _todos.Clear();
                foreach (var item in sortedTodos)
                {
                    _todos.Add(item);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Todo todo)
            {
                _context.DeleteTodo(todo.Id);
                _todos.Remove(todo);
            }
        }
    }
}