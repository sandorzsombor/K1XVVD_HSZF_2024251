using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace K1XVVD_HSZF_2024251.Application
{
    public interface ISimulationService
    {
        void SimulateDay();
        event EventHandler<string> CriticalEventOccurred;
    }

    public class SimulationService : ISimulationService
    {
        private readonly ICharacterService _characterService;
        private readonly IResourceService _resourceService;
        private readonly IActivityService _activityService;

        public event EventHandler<string> CriticalEventOccurred;

        public SimulationService(ICharacterService characterService, IResourceService resourceService, IActivityService activityService)
        {
            _characterService = characterService;
            _resourceService = resourceService;
            _activityService = activityService;
        }
        public void SimulateDay()
        {
            var characters = _characterService.GetAllCharacters().ToList();
            var resources = _resourceService.GetAllResources()
                .GroupBy(r => r.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.Quantity)
                );

            var events = new List<string>();

            foreach (var character in characters)
            {
                character.HungerLevel += 10;
                character.ThirstLevel += 10;

                if (character.HungerLevel > 70 || character.ThirstLevel > 70)
                {
                    CriticalEventOccurred?.Invoke(this, $"Critical need for {character.Name}: Hunger or Thirst level critical!");
                    character.Mood = "Depressed";
                    events.Add($"{character.Name} has reached critical levels of hunger or thirst.");
                }

                if (resources.TryGetValue("Food", out int food) && food > 0)
                {
                    character.HungerLevel = Math.Max(0, character.HungerLevel - 30);
                    resources["Food"]--;
                }

                if (resources.TryGetValue("Water", out int water) && water > 0)
                {
                    character.ThirstLevel = Math.Max(0, character.ThirstLevel - 30);
                    resources["Water"]--;
                }

                if (character.HungerLevel > 70 || character.ThirstLevel > 70)
                {
                    character.HealthStatus = "Deceased";
                    events.Add($"{character.Name} has died due to unmet needs.");
                }

                foreach (var activity in character.Activities.ToList()) 
                {
                    if (activity.HealthImpact < 0 && character.HealthStatus == "Healthy")
                    {
                        character.HealthStatus = "Injured";
                        events.Add($"{character.Name} was injured during {activity.Name}.");
                    }
                }

                _characterService.UpdateCharacter(character);
            }

            GenerateDailyReport(events);
        }


        private void GenerateDailyReport(IEnumerable<string> events)
        {
            var filePath = $"daily_report_{DateTime.Now:yyyyMMdd}.txt";
            File.WriteAllLines(filePath, events);
            Console.WriteLine($"Daily report saved: {filePath}");
        }
        private void GenerateIndividualReports()
        {
            var characters = _characterService.GetAllCharacters();

            foreach (var character in characters)
            {
                var yearFolder = Path.Combine("Reports", DateTime.Now.Year.ToString());
                var dateFolder = Path.Combine(yearFolder, $"{DateTime.Now:MMdd}");
                Directory.CreateDirectory(dateFolder);

                var filePath = Path.Combine(dateFolder, $"{character.Name}_{DateTime.Now:yyyyMMdd}.xml");
                var xmlContent = new XElement("CharacterReport",
                    new XElement("Name", character.Name),
                    new XElement("HealthStatus", character.HealthStatus),
                    new XElement("HungerLevel", character.HungerLevel),
                    new XElement("ThirstLevel", character.ThirstLevel),
                    new XElement("Mood", character.Mood),
                    new XElement("Activities",
                        character.Activities.Select(a => new XElement("Activity",
                            new XElement("Name", a.Name),
                            new XElement("Duration", a.Duration),
                            new XElement("TimeOfDay", a.TimeOfDay),
                            new XElement("HealthImpact", a.HealthImpact),
                            new XElement("MoodImpact", a.MoodImpact)
                        )))
                );

                xmlContent.Save(filePath);
            }
        }

    }

}
