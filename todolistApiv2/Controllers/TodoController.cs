using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todolistApiv2.Data;
using todolistApiv2.Models;

namespace todolistApiv2.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize] // Require authorization for all actions in this controller
    public class TodoController : Controller
    {
        private readonly DataContext _context;

        public TodoController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Todo
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); // Get authenticated user's UserId
            var todos = await _context.Todos.Where(t => t.UserId == userId).ToListAsync(); // Filter todos by UserId
            return todos;
        }

        // GET: api/Todo/5
        [HttpGet("{id}", Name = "Get")]
        private ActionResult<Todo> Get(int id)
        {
            var todo = _context.Todos.FirstOrDefault(t => t.TodoId == id && t.UserId == int.Parse(User.FindFirst("UserId").Value)); // Filter based on user's identifier from JWT token
            if (todo == null)
            {
                return NotFound();
            }
            return todo;
        }

        // POST: api/Todo
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTodo(Todo todo)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); // Get authenticated user's UserId

                // Set UserId for the todo
                todo.UserId = userId;

                // Set CreatedDate to current date and time
                todo.CreatedDate = DateTime.Now;

                _context.Todos.Add(todo);
                await _context.SaveChangesAsync();

                return Ok(todo);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500); // Internal Server Error
            }
        }
        // PUT: api/Todo/5
        [HttpPut("{id}")]
        public IActionResult UpdateTodo(int id, [FromBody] Todo updatedTodo)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                // User ID claim not found or invalid
                return BadRequest("Invalid user ID");
            }
            var todo = _context.Todos.FirstOrDefault(t => t.TodoId == id && t.UserId == userId); // Filter based on user's identifier from JWT token
            if (todo == null)
            {
                return NotFound();
            }

            // Update only the properties that are provided
            if (!string.IsNullOrEmpty(updatedTodo.Title))
            {
                todo.Title = updatedTodo.Title;
            }
            if (!string.IsNullOrEmpty(updatedTodo.Description))
            {
                todo.Description = updatedTodo.Description;
            }
            if (!string.IsNullOrEmpty(updatedTodo.Status))
            {
                todo.Status = updatedTodo.Status;
            }

            // Update updated date to current date and time
            todo.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            return Ok(todo);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult DeleteTodo(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                // User ID claim not found or invalid
                return BadRequest("Invalid user ID");
            }
            var todo = _context.Todos.FirstOrDefault(t => t.TodoId == id && t.UserId == userId); // Filter based on user's identifier from JWT token
            if (todo == null)
            {
                return NotFound();
            }

            _context.Todos.Remove(todo);
            _context.SaveChanges(); // Save changes to the database

            return Ok();
        }
    }
}