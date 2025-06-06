using System;

namespace ToDoList.Models
{
    public interface ITask
    {
        int Id { get; set; }
        string Title { get; set; }
        bool IsCompleted { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? CompletedAt { get; set; }
    }

    public class Todo : ITask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
    }
} 