namespace Example.Api.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public TodoStatus Status { get; set; } = TodoStatus.Todo;
    }

    public enum TodoStatus
    {
        Todo,
        Done
    }
}