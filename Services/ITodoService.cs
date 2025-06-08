using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services
{
    public interface ITodoService
    {
        IEnumerable<TodoItem> GetAllTodos();
        TodoItem GetTodoById(int id);
        void AddTodo(TodoItem todoItem);
        void UpdateTodo(TodoItem todoItem);
        void DeleteTodo(int id);
    }
}