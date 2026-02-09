using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using TestBlazor.Data;
using TestBlazor.Models;

namespace TestBlazor.Services
{
    public class ToDoService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _filePath;

        public ToDoService(IWebHostEnvironment environment, IServiceScopeFactory scopeFactory)
        {
            _environment = environment;
            _scopeFactory = scopeFactory;
            _filePath = Path.Combine(_environment.WebRootPath, "data", "todolists.xml");
            EnsureDirectoryExists();
            
            if (!File.Exists(_filePath))
            {
                GenerateSampleData();
            }
        }

        public async Task ArchiveListAsync(ToDoTemplate activeList)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();

            var archivedList = new ArchivedToDoList
            {
                Id = Guid.NewGuid(),
                OriginalTemplateName = activeList.Name,
                DateArchived = DateTime.Now,
                Items = activeList.Items.Select(i => new ArchivedToDoItem
                {
                    Id = Guid.NewGuid(),
                    Description = i.Description,
                    IsChecked = i.IsChecked
                }).ToList()
            };

            dbContext.ArchivedLists.Add(archivedList);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<ArchivedToDoList>> GetArchivedListsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
            
            return await dbContext.ArchivedLists
                .Include(l => l.Items)
                .OrderByDescending(l => l.DateArchived)
                .ToListAsync();
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
        }

        public List<ToDoTemplate> LoadTemplates()
        {
            if (!File.Exists(_filePath)) return new List<ToDoTemplate>();

            var serializer = new XmlSerializer(typeof(List<ToDoTemplate>));
            using var stream = new FileStream(_filePath, FileMode.Open);
            return (List<ToDoTemplate>)serializer.Deserialize(stream)!;
        }

        public void SaveTemplates(List<ToDoTemplate> templates)
        {
            var serializer = new XmlSerializer(typeof(List<ToDoTemplate>));
            using var stream = new FileStream(_filePath, FileMode.Create);
            serializer.Serialize(stream, templates);
        }

        private void GenerateSampleData()
        {
            var templates = new List<ToDoTemplate>
            {
                new ToDoTemplate
                {
                    Name = "Daily Inspection",
                    Description = "Routine checks for the facility.",
                    Items = new List<ToDoItem>
                    {
                        new ToDoItem { Description = "Check water levels" },
                        new ToDoItem { Description = "Inspect turbine casing" },
                        new ToDoItem { Description = "Verify valve positions" },
                        new ToDoItem { Description = "Record outgoing pressure" }
                    }
                },
                new ToDoTemplate
                {
                    Name = "Emergency Shutdown",
                    Description = "Steps to follow in case of emergency.",
                    Items = new List<ToDoItem>
                    {
                        new ToDoItem { Description = "Activate emergency siren" },
                        new ToDoItem { Description = "Close main intake valve" },
                        new ToDoItem { Description = "Notify control center" },
                        new ToDoItem { Description = "Evacuate personnel" }
                    }
                }
            };

            // Add some random tasks
            var randomTasks = new[] { "Clean filters", "Oil bearings", "Test backup generator", "Calibrate sensors" };
            var random = new Random();
            var randomTemplate = new ToDoTemplate
            {
                Name = "Maintenance Random",
                Description = "Randomly generated maintenance tasks.",
                Items = new List<ToDoItem>()
            };

            foreach (var task in randomTasks)
            {
                if (random.Next(2) == 0) // 50% chance
                {
                    randomTemplate.Items.Add(new ToDoItem { Description = task });
                }
            }
            templates.Add(randomTemplate);

            SaveTemplates(templates);
        }
    }
}
