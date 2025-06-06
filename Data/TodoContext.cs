using Microsoft.Data.Sqlite;
using ToDoList.Models;
using System;

namespace ToDoList.Data
{
    public class TodoContext
    {
        private readonly string _connectionString;

        public TodoContext()
        {
            _connectionString = "Data Source=todos.db";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Todos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        IsCompleted INTEGER NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        CompletedAt TEXT
                    );";
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new TodoDataException("Failed to initialize database", ex);
            }
        }

        public List<Todo> GetAllTodos()
        {
            try
            {
                var todos = new List<Todo>();
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Todos 
                    ORDER BY IsCompleted ASC, 
                    CASE 
                        WHEN IsCompleted = 1 THEN CompletedAt 
                        ELSE CreatedAt 
                    END DESC";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    todos.Add(new Todo
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        IsCompleted = reader.GetInt32(2) == 1,
                        CreatedAt = DateTime.Parse(reader.GetString(3)),
                        CompletedAt = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4))
                    });
                }

                return todos;
            }
            catch (Exception ex)
            {
                throw new TodoFetchException("Failed to fetch todos from database", ex);
            }
        }

        public void AddTodo(Todo todo)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Todos (Title, IsCompleted, CreatedAt, CompletedAt)
                    VALUES (@Title, @IsCompleted, @CreatedAt, @CompletedAt);";

                command.Parameters.AddWithValue("@Title", todo.Title);
                command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted ? 1 : 0);
                command.Parameters.AddWithValue("@CreatedAt", todo.CreatedAt.ToString("o"));
                command.Parameters.AddWithValue("@CompletedAt", todo.CompletedAt?.ToString("o") ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new TodoSaveException($"Failed to add todo: {todo.Title}", ex);
            }
        }

        public void UpdateTodo(Todo todo)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Todos 
                    SET Title = @Title, 
                        IsCompleted = @IsCompleted,
                        CompletedAt = @CompletedAt
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", todo.Id);
                command.Parameters.AddWithValue("@Title", todo.Title);
                command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted ? 1 : 0);
                command.Parameters.AddWithValue("@CompletedAt", todo.CompletedAt?.ToString("o") ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new TodoSaveException($"Failed to update todo: {todo.Title}", ex);
            }
        }

        public void DeleteTodo(int id)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Todos WHERE Id = @Id;";
                command.Parameters.AddWithValue("@Id", id);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new TodoDeleteException($"Failed to delete todo with ID: {id}", ex);
            }
        }
    }
} 