using K1XVVD_HSZF_2024251.Application;
using K1XVVD_HSZF_2024251.Model;
using K1XVVD_HSZF_2024251.Persistence.MsSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data;

namespace K1XVVD_HSZF_2024251
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<SurvivalDbContext>();
                    services.AddSingleton<IResourceService, ResourceService>();
                    services.AddSingleton<ICharacterService, CharacterService>();
                    services.AddSingleton<IActivityService, ActivityService>();
                    services.AddSingleton<IDataImportService, DataImportService>();
                    services.AddSingleton<ISimulationService, SimulationService>();
                    services.AddSingleton<IResourceDataProvider, ResourceDataProvider>();
                    services.AddSingleton<ICharacterDataProvider, CharacterDataProvider>();
                    services.AddSingleton<IActivityDataProvider, ActivityDataProvider>();
                })
                .Build();

            host.Start();
            ImportDataFromXml(host);
            bool exit = true;
            while (exit)
            {
                MainMenu(ref exit, host);
            }
            host.Dispose();
        }
        static void ImportDataFromXml(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var dataImportService = scope.ServiceProvider.GetRequiredService<IDataImportService>();
                var filePath = "survivor.xml";
                dataImportService.ImportFromXml(filePath);
                Console.WriteLine("Data successfully loaded from XML.");
            }
        }
        static List<Character> team = new List<Character>();
        static List<Activity> activity = new List<Activity>();
        static int daysSurvived = 0;
        static int requiredDays = 10;
        static void MainMenu(ref bool exit, IHost host)
        {
            Console.WriteLine("Survivor Management System");
            Console.WriteLine("1. List Data");
            Console.WriteLine("2. Character Management");
            Console.WriteLine("3. Activity Management");
            Console.WriteLine("4. Resource Management");
            Console.WriteLine("5. List Characters By Skill");
            Console.WriteLine("6. Select Team");
            Console.WriteLine("7. View Team Status");
            Console.WriteLine("8. Assign Activity to Team");
            Console.WriteLine("9. Simulate Day");
            Console.WriteLine("0. Exit");

            int daysSurvived = 0;
            int requiredDays = 10;
            int choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 0:
                    exit = false;
                    break;
                case 1:
                    ListDataMenu(host);
                    break;
                case 2:
                    CrudMenu("Character", host, CreateCharacter, UpdateCharacter, DeleteCharacter);
                    break;
                case 3:
                    CrudMenu("Activity", host, CreateActivity, UpdateActivity, DeleteActivity);
                    break;
                case 4:
                    CrudMenu("Resource", host, CreateResource, UpdateResource, DeleteResource);
                    break;
                case 5:
                    ListCharactersBySkill(host);
                    break;
                case 6:
                    team = SelectTeam(host);
                    break;
                case 7:
                    ViewTeamStatus(team);
                    break;
                case 8:
                    AssignActivity(host, team, activity);
                    break;
                case 9:
                    if (SimulateDay(host, team))
                    {
                        exit = false;
                        return;
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        static void ListDataMenu(IHost host)
        {
            Console.Clear();
            Console.WriteLine("Survivor Management System");
            Console.WriteLine("===========================");
            Console.WriteLine("1. List Characters");
            Console.WriteLine("2. List Activities");
            Console.WriteLine("3. List Resources");
            Console.WriteLine("0. Exit");
            Console.WriteLine("===========================");
            Console.Write("Choose an option: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1:
                        ListCharacters(host);
                        break;
                    case 2:
                        ListActivities(host);
                        break;
                    case 3:
                        ListResources(host);
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }
        }
        static void CrudMenu(string entityType, IHost host, Action<IHost> create, Action<IHost> update, Action<IHost> delete)
        {
            Console.WriteLine($"{entityType} Management");
            Console.WriteLine("1. Create");
            Console.WriteLine("2. Update");
            Console.WriteLine("3. Delete");
            Console.WriteLine("0. Back to Main Menu");

            int choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    create(host);
                    break;
                case 2:
                    update(host);
                    break;
                case 3:
                    delete(host);
                    break;
                case 0:
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        static void ListCharacters(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var characterService = scope.ServiceProvider.GetRequiredService<ICharacterService>();
            var characters = characterService.GetAllCharacters();

            Console.WriteLine("\nList of Characters:");
            Console.WriteLine("====================");
            foreach (var character in characters)
            {
                Console.WriteLine($"Name: {character.Name}");
                Console.WriteLine($"  Health Status: {character.HealthStatus}");
                Console.WriteLine($"  Hunger Level: {character.HungerLevel}");
                Console.WriteLine($"  Thirst Level: {character.ThirstLevel}");
                Console.WriteLine($"  Mood: {character.Mood}");
                Console.WriteLine($"  Skills: {string.Join(", ", character.Skills)}");
                Console.WriteLine();
            }
        }
        static void ListActivities(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var activityService = scope.ServiceProvider.GetRequiredService<IActivityService>();
            var activities = activityService.GetAllActivities();

            Console.WriteLine("\nList of Activities:");
            Console.WriteLine("====================");
            foreach (var activity in activities)
            {
                Console.WriteLine($"Name: {activity.Name}");
                Console.WriteLine($"  Duration: {activity.Duration}");
                Console.WriteLine($"  Time of Day: {activity.TimeOfDay}");
                Console.WriteLine($"  Health Impact: {activity.HealthImpact}");
                Console.WriteLine($"  Mood Impact: {activity.MoodImpact}");
                Console.WriteLine($"  Required Health Status: {activity.RequiredHealthStatus}");
                Console.WriteLine($"  Required Skills: {string.Join(", ", activity.RequiredSkills)}");
                Console.WriteLine("  Resources:");
                foreach (var resource in activity.Resources)
                {
                    Console.WriteLine($"    - {resource.Name} ({resource.Type}), Quantity: {resource.Quantity}");
                }
                Console.WriteLine();
            }
        }
        static void ListResources(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();
            var resources = resourceService.GetAllResources();

            Console.WriteLine("\nList of Resources:");
            Console.WriteLine("===================");
            foreach (var resource in resources)
            {
                Console.WriteLine($"Name: {resource.Name}");
                Console.WriteLine($"  Type: {resource.Type}");
                Console.WriteLine($"  Quantity: {resource.Quantity}");
                Console.WriteLine();
            }
        }
        static void CreateCharacter(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var characterService = scope.ServiceProvider.GetRequiredService<ICharacterService>();

            Console.Write("Enter Character Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Health Status (Healthy/Injured/Sick): ");
            string healthStatus = Console.ReadLine();

            Console.Write("Enter Hunger Level (0-100): ");
            int hungerLevel = int.Parse(Console.ReadLine());

            Console.Write("Enter Thirst Level (0-100): ");
            int thirstLevel = int.Parse(Console.ReadLine());

            Console.Write("Enter Mood (Happy/Sad/Depressed): ");
            string mood = Console.ReadLine();

            Console.Write("Enter Skills (comma-separated): ");
            string skills = Console.ReadLine();

            characterService.CreateCharacter(new Character
            {
                Name = name,
                HealthStatus = healthStatus,
                HungerLevel = hungerLevel,
                ThirstLevel = thirstLevel,
                Mood = mood,
                Skills = skills
            });

            Console.WriteLine("Character created successfully!");
        }
        static void UpdateCharacter(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var characterService = scope.ServiceProvider.GetRequiredService<ICharacterService>();

            Console.Write("Enter Character ID to Update: ");
            int id = int.Parse(Console.ReadLine());

            var character = characterService.GetCharacterById(id);
            if (character == null)
            {
                Console.WriteLine("Character not found!");
                return;
            }

            Console.Write("Enter New Name (leave blank to keep unchanged): ");
            string name = Console.ReadLine();
            character.Name = string.IsNullOrWhiteSpace(name) ? character.Name : name;

            Console.Write("Enter New Health Status (leave blank to keep unchanged): ");
            string healthStatus = Console.ReadLine();
            character.HealthStatus = string.IsNullOrWhiteSpace(healthStatus) ? character.HealthStatus : healthStatus;

            Console.Write("Enter New Hunger Level (leave blank to keep unchanged): ");
            string hungerInput = Console.ReadLine();
            character.HungerLevel = string.IsNullOrWhiteSpace(hungerInput) ? character.HungerLevel : int.Parse(hungerInput);

            Console.Write("Enter New Thirst Level (leave blank to keep unchanged): ");
            string thirstInput = Console.ReadLine();
            character.ThirstLevel = string.IsNullOrWhiteSpace(thirstInput) ? character.ThirstLevel : int.Parse(thirstInput);

            Console.Write("Enter New Mood (leave blank to keep unchanged): ");
            string mood = Console.ReadLine();
            character.Mood = string.IsNullOrWhiteSpace(mood) ? character.Mood : mood;

            Console.Write("Enter New Skills (comma-separated, leave blank to keep unchanged): ");
            string skills = Console.ReadLine();
            character.Skills = string.IsNullOrWhiteSpace(skills) ? character.Skills : skills;

            characterService.UpdateCharacter(character);
            Console.WriteLine("Character updated successfully!");
        }
        static void DeleteCharacter(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var characterService = scope.ServiceProvider.GetRequiredService<ICharacterService>();

            Console.Write("Enter Character ID to Delete: ");
            int id = int.Parse(Console.ReadLine());

            characterService.DeleteCharacter(id);
            Console.WriteLine("Character deleted successfully!");
        }
        static void ListCharactersBySkill(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var characterService = scope.ServiceProvider.GetRequiredService<ICharacterService>();

            Console.Write("Enter a skill to filter characters: ");
            string skill = Console.ReadLine();

            var characters = characterService.GetAllCharacters()
                .Where(c => c.Skills.Split(',').Contains(skill, StringComparer.OrdinalIgnoreCase));

            Console.WriteLine("Characters with skill: " + skill);
            foreach (var character in characters)
            {
                Console.WriteLine($"Name: {character.Name}, Health: {character.HealthStatus}, Mood: {character.Mood}");
            }
        }
        static void CreateActivity(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var activityService = scope.ServiceProvider.GetRequiredService<IActivityService>();

            Console.Write("Enter Activity Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Duration (HH:mm:ss): ");
            TimeSpan duration = TimeSpan.Parse(Console.ReadLine());

            Console.Write("Enter Time of Day (Day/Night): ");
            string timeOfDay = Console.ReadLine();

            Console.Write("Enter Health Impact: ");
            int healthImpact = int.Parse(Console.ReadLine());

            Console.Write("Enter Mood Impact: ");
            int moodImpact = int.Parse(Console.ReadLine());

            Console.Write("Enter Required Health Status: ");
            string requiredHealthStatus = Console.ReadLine();

            Console.Write("Enter Required Skills (comma-separated): ");
            string requiredSkills = Console.ReadLine();

            activityService.CreateActivity(new Activity
            {
                Name = name,
                Duration = duration,
                TimeOfDay = timeOfDay,
                HealthImpact = healthImpact,
                MoodImpact = moodImpact,
                RequiredHealthStatus = requiredHealthStatus,
                RequiredSkills = requiredSkills
            });

            Console.WriteLine("Activity created successfully!");
        }
        static void UpdateActivity(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var activityService = scope.ServiceProvider.GetRequiredService<IActivityService>();

            Console.Write("Enter Activity ID to Update: ");
            int id = int.Parse(Console.ReadLine());

            var activity = activityService.GetActivityById(id);
            if (activity == null)
            {
                Console.WriteLine("Activity not found!");
                return;
            }

            Console.Write("Enter New Name (leave blank to keep unchanged): ");
            string name = Console.ReadLine();
            activity.Name = string.IsNullOrWhiteSpace(name) ? activity.Name : name;

            Console.Write("Enter New Duration (HH:mm:ss, leave blank to keep unchanged): ");
            string durationInput = Console.ReadLine();
            activity.Duration = string.IsNullOrWhiteSpace(durationInput) ? activity.Duration : TimeSpan.Parse(durationInput);

            Console.Write("Enter New Time of Day (leave blank to keep unchanged): ");
            string timeOfDay = Console.ReadLine();
            activity.TimeOfDay = string.IsNullOrWhiteSpace(timeOfDay) ? activity.TimeOfDay : timeOfDay;

            Console.Write("Enter New Health Impact (leave blank to keep unchanged): ");
            string healthImpactInput = Console.ReadLine();
            activity.HealthImpact = string.IsNullOrWhiteSpace(healthImpactInput) ? activity.HealthImpact : int.Parse(healthImpactInput);

            Console.Write("Enter New Mood Impact (leave blank to keep unchanged): ");
            string moodImpactInput = Console.ReadLine();
            activity.MoodImpact = string.IsNullOrWhiteSpace(moodImpactInput) ? activity.MoodImpact : int.Parse(moodImpactInput);

            Console.Write("Enter New Required Health Status (leave blank to keep unchanged): ");
            string requiredHealthStatus = Console.ReadLine();
            activity.RequiredHealthStatus = string.IsNullOrWhiteSpace(requiredHealthStatus) ? activity.RequiredHealthStatus : requiredHealthStatus;

            Console.Write("Enter New Required Skills (comma-separated, leave blank to keep unchanged): ");
            string requiredSkills = Console.ReadLine();
            activity.RequiredSkills = string.IsNullOrWhiteSpace(requiredSkills) ? activity.RequiredSkills : requiredSkills;

            activityService.UpdateActivity(activity);
            Console.WriteLine("Activity updated successfully!");
        }
        static void DeleteActivity(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var activityService = scope.ServiceProvider.GetRequiredService<IActivityService>();

            Console.Write("Enter Activity ID to Delete: ");
            int id = int.Parse(Console.ReadLine());

            activityService.DeleteActivity(id);
            Console.WriteLine("Activity deleted successfully!");
        }
        static void CreateResource(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();

            Console.Write("Enter Resource Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Resource Type: ");
            string type = Console.ReadLine();

            Console.Write("Enter Quantity: ");
            int quantity = int.Parse(Console.ReadLine());

            resourceService.CreateResource(new Resource
            {
                Name = name,
                Type = type,
                Quantity = quantity
            });

            Console.WriteLine("Resource created successfully!");
        }
        static void UpdateResource(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();

            Console.Write("Enter Resource ID to Update: ");
            int id = int.Parse(Console.ReadLine());

            var resource = resourceService.GetResourceById(id);
            if (resource == null)
            {
                Console.WriteLine("Resource not found!");
                return;
            }

            Console.Write("Enter New Name (leave blank to keep unchanged): ");
            string name = Console.ReadLine();
            resource.Name = string.IsNullOrWhiteSpace(name) ? resource.Name : name;

            Console.Write("Enter New Type (leave blank to keep unchanged): ");
            string type = Console.ReadLine();
            resource.Type = string.IsNullOrWhiteSpace(type) ? resource.Type : type;

            Console.Write("Enter New Quantity (leave blank to keep unchanged): ");
            string quantityInput = Console.ReadLine();
            resource.Quantity = string.IsNullOrWhiteSpace(quantityInput) ? resource.Quantity : int.Parse(quantityInput);

            resourceService.UpdateResource(resource);
            Console.WriteLine("Resource updated successfully!");
        }
        static void DeleteResource(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();

            Console.Write("Enter Resource ID to Delete: ");
            int id = int.Parse(Console.ReadLine());

            resourceService.DeleteResource(id);
            Console.WriteLine("Resource deleted successfully!");
        }
        static List<Character> SelectTeam(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var characterService = scope.ServiceProvider.GetRequiredService<ICharacterService>();

            var allCharacters = characterService.GetAllCharacters().ToList();
            var selectedTeam = new List<Character>();

            Console.WriteLine("Select 3 characters for your team:");
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Available characters:");
                foreach (var character in allCharacters)
                {
                    Console.WriteLine($"{character.Id}. {character.Name} (Health: {character.HealthStatus}, Mood: {character.Mood})");
                }

                Console.Write("Enter the ID of the character: ");
                int id = int.Parse(Console.ReadLine());
                var selectedCharacter = allCharacters.FirstOrDefault(c => c.Id == id);

                if (selectedCharacter != null)
                {
                    selectedTeam.Add(selectedCharacter);
                    allCharacters.Remove(selectedCharacter);
                }
                else
                {
                    Console.WriteLine("Invalid selection, try again.");
                    i--;
                }
            }

            Console.WriteLine("Team selection complete.");
            return selectedTeam;
        }
        static void ViewTeamStatus(List<Character> team)
        {
            Console.WriteLine("Team Status:");
            foreach (var character in team)
            {
                Console.WriteLine($"Name: {character.Name}, Health: {character.HealthStatus}, Hunger: {character.HungerLevel}, Thirst: {character.ThirstLevel}, Mood: {character.Mood}");
            }
        }
        static List<Activity> AssignActivity(IHost host, List<Character> team, List<Activity> activity)
        {
            using var scope = host.Services.CreateScope();
            var activityService = scope.ServiceProvider.GetRequiredService<IActivityService>();

            var activities = activityService.GetAllActivities().ToList();
            var selectedActivities = new List<Activity>();


            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Available activities:");

              
                foreach (var actvty in activities)
                {
                    Console.WriteLine($"{actvty.Id}. {actvty.Name} (Time of Day: {actvty.TimeOfDay}, Duration: {actvty.Duration})");
                }

                Console.Write("Enter the ID of the activity: ");
                int id = int.Parse(Console.ReadLine());
                var selectedActivity = activities.FirstOrDefault(c => c.Id == id);

                if (selectedActivity != null)
                {
                    selectedActivities.Add(selectedActivity);
                    activities.Remove(selectedActivity);
                }
                else
                {
                    Console.WriteLine("Invalid selection, try again.");
                    i--;
                }
            }

            Console.WriteLine("ctivity selection complete.");
            return selectedActivities;
        }
        static bool SimulateDay(IHost host, List<Character> team)
        {
            using var scope = host.Services.CreateScope();
            var simulationService = scope.ServiceProvider.GetRequiredService<ISimulationService>();

            simulationService.CriticalEventOccurred += (sender, message) =>
            {
                Console.WriteLine("Critical Event: " + message);
            };

            Console.WriteLine($"Day {daysSurvived + 1} simulation starting...");
            simulationService.SimulateDay();
            daysSurvived++;

            Console.WriteLine("Day simulation complete.");
            foreach (var character in team)
            {
                Console.WriteLine($"Character: {character.Name}, Health: {character.HealthStatus}, Hunger: {character.HungerLevel}, Thirst: {character.ThirstLevel}");
            }

            return IsGameOver(team, daysSurvived, requiredDays);
        }

        static bool IsGameOver(List<Character> team, int daysSurvived, int requiredDays)
        {
            if (team.Count == 0)
            {
                Console.WriteLine("Game Over: All team members are gone.");
                return true;
            }

            if (daysSurvived >= requiredDays)
            {
                Console.WriteLine("Congratulations! You have survived for " + requiredDays + " days.");
                return true;
            }

            return false;
        }

    }
}
