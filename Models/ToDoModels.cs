namespace TestBlazor.Models
{
    public class ToDoTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "New List";
        public string Description { get; set; } = "";
        public List<ToDoItem> Items { get; set; } = new();
    }

    public class ToDoItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; } = "";
        public bool IsChecked { get; set; }
    }
}
