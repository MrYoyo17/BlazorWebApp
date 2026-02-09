using System.ComponentModel.DataAnnotations;

namespace TestBlazor.Models
{
    public class ArchivedToDoList
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OriginalTemplateName { get; set; } = "";
        public DateTime DateArchived { get; set; } = DateTime.Now;
        public List<ArchivedToDoItem> Items { get; set; } = new();
    }

    public class ArchivedToDoItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; } = "";
        public bool IsChecked { get; set; }
        
        // Foreign Key
        public Guid ArchivedListId { get; set; }
        public ArchivedToDoList ArchivedList { get; set; } = null!;
    }
}
