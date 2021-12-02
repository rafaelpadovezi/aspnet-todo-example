using Example.Api;
using Example.Api.Infrastructure;
using Example.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Example.Tests
{
    public class TodoControllerTests
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public TodoControllerTests()
        {
            // Creating the WebApplicationFactory means that a different
            // application is created for each test.
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the Application db context from the DI
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType ==
                                 typeof(DbContextOptions<TodoContext>));

                        services.Remove(descriptor);
                        // Create a unique db name for each test. This way data from one test does not
                        // affect the other.
                        var databaseName = $"InMemoryDbForTesting-{Guid.NewGuid()}";
                        // Add the test DB context
                        services.AddDbContext<TodoContext>(options =>
                        {
                            options.UseInMemoryDatabase(databaseName);
                        });
                    });
                });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetTodos_ReturnTodosPaginated()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
                context.Todos.AddRange(
                    new Todo { Description = "Do something 1" },
                    new Todo { Description = "Do something 2" },
                    new Todo { Description = "Do something 3" },
                    new Todo { Description = "Do something 4" },
                    new Todo { Description = "Do something 5" },
                    new Todo { Description = "Do something 6" },
                    new Todo { Description = "Do something 7" });
                await context.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync("todo?page=2&itemsPerPage=3");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var todos = await response.Content.ReadFromJsonAsync<Todo[]>();
            Assert.Collection(todos,
                todo => Assert.Equal("Do something 4", todo.Description),
                todo => Assert.Equal("Do something 5", todo.Description),
                todo => Assert.Equal("Do something 6", todo.Description));
        }

        [Fact]
        public async Task UpdateStatus_ReturnTodosPaginated()
        {
            // Arrange
            int todoId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
                var todo = new Todo { Description = "Do something", Status = TodoStatus.Todo };
                context.Todos.Add(todo);
                await context.SaveChangesAsync();
                todoId = todo.Id;
            }

            // Act
            var response = await _client.PutAsync($"todo/1?status={(int)TodoStatus.Done}", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
                var updatedTodo = await context.Todos.FirstAsync(x => x.Id == todoId);
                Assert.Equal(TodoStatus.Done, updatedTodo.Status);
            }
        }
    }
}