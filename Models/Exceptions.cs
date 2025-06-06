using System;

namespace ToDoList.Models
{
    public class TodoDataException : Exception
    {
        public TodoDataException(string message) : base(message) { }
        public TodoDataException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class TodoFetchException : TodoDataException
    {
        public TodoFetchException(string message) : base(message) { }
        public TodoFetchException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class TodoSaveException : TodoDataException
    {
        public TodoSaveException(string message) : base(message) { }
        public TodoSaveException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class TodoDeleteException : TodoDataException
    {
        public TodoDeleteException(string message) : base(message) { }
        public TodoDeleteException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class FilterException : Exception
    {
        public FilterException(string message) : base(message) { }
        public FilterException(string message, Exception inner) : base(message, inner) { }
    }
} 