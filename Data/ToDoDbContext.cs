using Microsoft.EntityFrameworkCore;
using TestBlazor.Models;

namespace TestBlazor.Data
{
    public class ToDoDbContext : DbContext
    {
        public DbSet<ArchivedToDoList> ArchivedLists { get; set; }
        public DbSet<ArchivedToDoItem> ArchivedItems { get; set; }

        public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
        {
        }
    }
}
