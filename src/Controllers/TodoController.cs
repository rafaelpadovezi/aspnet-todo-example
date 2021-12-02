using Example.Api.Infrastructure;
using Example.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Example.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoController(TodoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos(int page = 1, int itemsPerPage = 5, TodoStatus? status = null)
        {
            var itemsQuery = _context.Todos
                .OrderBy(x => x.Id)
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage);

            if (status is not null)
                itemsQuery = itemsQuery.Where(x => x.Status == status.Value);

            return Ok(await itemsQuery.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddTodo(Todo todo)
        {
            _context.Add(todo);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, TodoStatus status)
        {
            var todo = await _context.Todos.SingleOrDefaultAsync(x => x.Id == id);
            if (todo is null)
                return NotFound("Todo not found");

            todo.Status = status;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}