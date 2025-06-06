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
using System;

namespace ToDoList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TodoContext _context;
        private ObservableCollection<Todo> _todos;
        private readonly AppConfig _config;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _context = new TodoContext();
                _todos = new ObservableCollection<Todo>(_context.GetAllTodos());
                TodoListView.ItemsSource = _todos;
                NewTodoTextBox.KeyDown += NewTodoTextBox_KeyDown;
                _config = AppConfig.Instance;
                ApplyFilter(_config.CurrentFilter);
                UpdateRadioButtonState(_config.CurrentFilter);
            }
            catch (TodoDataException ex)
            {
                ShowErrorDialog("Database Error", ex.Message);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An unexpected error occurred: " + ex.Message);
                Application.Current.Shutdown();
            }
        }

        private void UpdateRadioButtonState(TaskFilter filter)
        {
            if (AllFilter == null || PendingFilter == null || CompletedFilter == null)
                return;

            AllFilter.IsChecked = filter == TaskFilter.All;
            PendingFilter.IsChecked = filter == TaskFilter.Pending;
            CompletedFilter.IsChecked = filter == TaskFilter.Completed;
        }

        private void ShowErrorDialog(string title, string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
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

            try
            {
                var todo = new Todo
                {
                    Title = NewTodoTextBox.Text.Trim(),
                    IsCompleted = false,
                    CreatedAt = DateTime.Now
                };

                _context.AddTodo(todo);
                _todos.Insert(0, todo);
                NewTodoTextBox.Clear();
                ApplyFilter(_config.CurrentFilter);
            }
            catch (TodoSaveException ex)
            {
                ShowErrorDialog("Save Error", ex.Message);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Todo todo)
            {
                try
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
                    ApplyFilter(_config.CurrentFilter);
                }
                catch (TodoSaveException ex)
                {
                    ShowErrorDialog("Update Error", ex.Message);
                    // Revert the checkbox state
                    todo.IsCompleted = !todo.IsCompleted;
                    todo.CompletedAt = todo.IsCompleted ? DateTime.Now : null;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Todo todo)
            {
                try
                {
                    _context.DeleteTodo(todo.Id);
                    _todos.Remove(todo);
                    ApplyFilter(_config.CurrentFilter);
                }
                catch (TodoDeleteException ex)
                {
                    ShowErrorDialog("Delete Error", ex.Message);
                }
            }
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton radioButton || string.IsNullOrEmpty(radioButton.Name))
                return;

            try
            {
                TaskFilter filter = radioButton.Name switch
                {
                    "AllFilter" => TaskFilter.All,
                    "PendingFilter" => TaskFilter.Pending,
                    "CompletedFilter" => TaskFilter.Completed,
                    _ => TaskFilter.All
                };

                if (_config != null)
                {
                    _config.CurrentFilter = filter;
                    _config.Save();
                    ApplyFilter(filter);
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Filter Error", "Failed to apply filter: " + ex.Message);
            }
        }

        private void ApplyFilter(TaskFilter filter)
        {
            if (_todos == null)
                return;

            try
            {
                var filteredItems = filter switch
                {
                    TaskFilter.All => _todos,
                    TaskFilter.Pending => _todos.Where(t => !t.IsCompleted),
                    TaskFilter.Completed => _todos.Where(t => t.IsCompleted),
                    _ => throw new FilterException($"Unknown filter: {filter}")
                };

                TodoListView.ItemsSource = filteredItems;
            }
            catch (FilterException fex)
            {
                ShowErrorDialog("Filter Error", fex.Message);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Filter Error", "Failed to apply filter: " + ex.Message);
            }
        }
    }

    public class TodoItem
    {
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}