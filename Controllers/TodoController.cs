using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Models;
using dotnet_rest_api.Services;
using System.Collections.Generic;
using System.Linq;

namespace dotnet_rest_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetTodos()
        {
            var todos = _todoService.GetAllTodos();
            return Ok(todos);
        }

        [HttpGet("{id}")]
        public ActionResult<TodoItem> GetTodoById(int id)
        {
            var todo = _todoService.GetTodoById(id);
            if (todo == null)
            {
                return NotFound();
            }
            return Ok(todo);
        }

        [HttpPost]
        public ActionResult<TodoItem> CreateTodo([FromBody] TodoItem todoItem)
        {
            if (todoItem == null)
            {
                return BadRequest();
            }

            _todoService.AddTodo(todoItem);
            return CreatedAtAction(nameof(GetTodoById), new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTodo(int id, [FromBody] TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            var existingTodo = _todoService.GetTodoById(id);
            if (existingTodo == null)
            {
                return NotFound();
            }

            _todoService.UpdateTodo(todoItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTodo(int id)
        {
            var todo = _todoService.GetTodoById(id);
            if (todo == null)
            {
                return NotFound();
            }

            _todoService.DeleteTodo(id);
            return NoContent();
        }
    }
}