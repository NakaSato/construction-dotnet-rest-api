using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

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

    public TodoItem? GetTodoById(int id)
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
        // First, check if entity is already being tracked
        var existingEntity = _context.TodoItems.Local.FirstOrDefault(t => t.Id == todoItem.Id);
        
        if (existingEntity != null)
        {
            // Update the existing tracked entity
            _context.Entry(existingEntity).CurrentValues.SetValues(todoItem);
        }
        else
        {
            // Check if the entity exists in the database without tracking it
            var exists = _context.TodoItems.AsNoTracking().Any(t => t.Id == todoItem.Id);
            if (!exists)
            {
                throw new InvalidOperationException($"Todo item with ID {todoItem.Id} not found");
            }
            
            // Entity is not being tracked, so attach and mark as modified
            _context.TodoItems.Attach(todoItem);
            _context.Entry(todoItem).State = EntityState.Modified;
        }
        
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
