using K1XVVD_HSZF_2024251.Model;
using K1XVVD_HSZF_2024251.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace K1XVVD_HSZF_2024251.Application
{
    public interface IDataImportService
    {
        public void ImportFromXml(string filePath);
    }
    public class DataImportService : IDataImportService
    {
        private readonly IActivityDataProvider activityDataProvider;
        private readonly ICharacterDataProvider characterDataProvider;
        private readonly IResourceDataProvider resourceDataProvider;
        public DataImportService(IActivityDataProvider activityDataProvider, ICharacterDataProvider characterDataProvider, IResourceDataProvider resourceDataProvider)
        {
            this.activityDataProvider = activityDataProvider;
            this.characterDataProvider = characterDataProvider;
            this.resourceDataProvider = resourceDataProvider;
        }
        public void ImportFromXml(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file {filePath} does not exist.");

            var xDoc = XDocument.Load(filePath);

            foreach (var character in xDoc.Descendants("Character"))
            {
                var name = character.Element("Name")?.Value;
                var healthStatus = character.Element("HealthStatus")?.Value;
                var hungerLevel = int.Parse(character.Element("HungerLevel")?.Value ?? "0");
                var thirstLevel = int.Parse(character.Element("ThirstLevel")?.Value ?? "0");
                var mood = character.Element("Mood")?.Value;
                var skills = string.Join(",", character.Element("Skills")?.Elements("Skill").Select(s => s.Value) ?? new List<string>());

                characterDataProvider.CreateCharacter(new Character
                {
                    Name = name,
                    HealthStatus = healthStatus,
                    HungerLevel = hungerLevel,
                    ThirstLevel = thirstLevel,
                    Mood = mood,
                    Skills = skills
                });
            }

            foreach (var activity in xDoc.Descendants("Activity"))
            {
                var name = activity.Element("Name")?.Value;
                var duration = TimeSpan.Parse(activity.Element("Duration")?.Value ?? "00:00:00");
                var timeOfDay = activity.Element("TimeOfDay")?.Value;
                var healthImpact = int.Parse(activity.Element("HealthImpact")?.Value ?? "0");
                var moodImpact = int.Parse(activity.Element("MoodImpact")?.Value ?? "0");
                var requiredHealthStatus = activity.Element("RequiredHealthStatus")?.Value;
                var requiredSkills = string.Join(",", activity.Element("RequiredSkills")?.Elements("Skill").Select(s => s.Value) ?? new List<string>());

                activityDataProvider.CreateActivity(new Activity
                {
                    Name = name,
                    Duration = duration,
                    TimeOfDay = timeOfDay,
                    HealthImpact = healthImpact,
                    MoodImpact = moodImpact,
                    RequiredHealthStatus = requiredHealthStatus,
                    RequiredSkills = requiredSkills
                });
            }

            foreach (var resource in xDoc.Descendants("Resource"))
            {
                var name = resource.Element("Name")?.Value;
                var type = resource.Element("Type")?.Value;
                var quantity = int.Parse(resource.Element("Quantity")?.Value ?? "0");

                resourceDataProvider.CreateResource(new Resource
                {
                    Name = name,
                    Type = type,
                    Quantity = quantity
                });
            }
        }


    }
}
