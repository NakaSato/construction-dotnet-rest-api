using dotnet_rest_api.Data;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

public class TodoService : ITodoService
{
    private readonly TodoContext _context;

    public TodoService(TodoContext context)
    {
        _context = context;
    }

    public IEnumerable<TodoItem> GetAllTodos()
    {
        return _context.TodoItems.ToList();
    }

    public TodoItem GetTodoById(int id)
    {
        return _context.TodoItems.FirstOrDefault(t => t.Id == id);
    }

    public void AddTodo(TodoItem todoItem)
    {
        _context.TodoItems.Add(todoItem);
        _context.SaveChanges();
    }

    public void UpdateTodo(TodoItem todoItem)
    {
        _context.TodoItems.Update(todoItem);
        _context.SaveChanges();
    }

    public void DeleteTodo(int id)
    {
        var todo = GetTodoById(id);
        if (todo != null)
        {
            _context.TodoItems.Remove(todo);
            _context.SaveChanges();
        }
    }
}
