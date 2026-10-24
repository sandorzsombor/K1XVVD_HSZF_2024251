using K1XVVD_HSZF_2024251.Application;
using K1XVVD_HSZF_2024251.Model;
using K1XVVD_HSZF_2024251.Persistence.MsSql;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Test
{
    public class SurvivalTester
    {
        private Mock<ICharacterDataProvider> mockCharacterDataProvider;
        private Mock<IActivityDataProvider> mockActivityDataProvider;
        private Mock<IResourceDataProvider> mockResourceDataProvider;

        private ICharacterService characterService;
        private IActivityService activityService;
        private IResourceService resourceService;

        public SurvivalTester()
        {
            mockCharacterDataProvider = new Mock<ICharacterDataProvider>();
            mockActivityDataProvider = new Mock<IActivityDataProvider>();
            mockResourceDataProvider = new Mock<IResourceDataProvider>();

            characterService = new CharacterService(mockCharacterDataProvider.Object);
            activityService = new ActivityService(mockActivityDataProvider.Object, mockCharacterDataProvider.Object);
            resourceService = new ResourceService(mockResourceDataProvider.Object, mockActivityDataProvider.Object);

            var characters = new List<Character>
            {
                new Character { Id = 1, Name = "Bill Bob", HealthStatus = "Healthy", HungerLevel = 20, ThirstLevel = 30, Mood = "Happy", Skills = "Strength,Speed" },
                new Character { Id = 2, Name = "Alice Bob", HealthStatus = "Injured", HungerLevel = 50, ThirstLevel = 40, Mood = "Sad", Skills = "Medicine,Engineering" }
            };

            var activities = new List<Model.Activity>
            {
                new Model.Activity { Id = 1, Name = "Foraging", Duration = TimeSpan.FromHours(2), TimeOfDay = "Day", HealthImpact = -5, MoodImpact = 10, RequiredHealthStatus = "Healthy", RequiredSkills = "Strength,Speed" },
                new Model.Activity { Id = 2, Name = "Repairing", Duration = TimeSpan.FromHours(3.5), TimeOfDay = "Day", HealthImpact = -10, MoodImpact = 20, RequiredHealthStatus = "Healthy", RequiredSkills = "Engineering" }
            };

            var resources = new List<Resource>
            {
                new Resource { Id = 1, Name = "Water", Type = "Drinkable", Quantity = 20 },
                new Resource { Id = 2, Name = "Medicine", Type = "Medical", Quantity = 5 }
            };

            mockCharacterDataProvider.Setup(repo => repo.ReadAllCharacters()).Returns(characters.AsQueryable());
            mockActivityDataProvider.Setup(repo => repo.ReadAllActivities()).Returns(activities.AsQueryable());
            mockResourceDataProvider.Setup(repo => repo.ReadAllResources()).Returns(resources.AsQueryable());
        }

        [Test]
        public void CreateCharacterTest()
        {
            var newCharacter = new Character { Name = "Tom Survivor", HealthStatus = "Healthy", HungerLevel = 10, ThirstLevel = 15, Mood = "Happy", Skills = "Strength,Agility" };

            characterService.CreateCharacter(newCharacter);

            mockCharacterDataProvider.Verify(repo => repo.CreateCharacter(It.Is<Character>(c => c.Name == "Tom Survivor")), Times.Once);
        }

        [Test]
        public void ListCharactersTest()
        {
            var characters = characterService.GetAllCharacters();

            Assert.That(characters.Count(), Is.EqualTo(2));
            Assert.That(characters.First().Name, Is.EqualTo("Bill Bob"));
        }

        [Test]
        public void CreateActivityTest()
        {
            var newActivity = new Activity { Name = "Hunting", Duration = TimeSpan.FromHours(4), TimeOfDay = "Night", HealthImpact = -15, MoodImpact = 25, RequiredHealthStatus = "Healthy", RequiredSkills = "Strength" };

            activityService.CreateActivity(newActivity);

            mockActivityDataProvider.Verify(repo => repo.CreateActivity(It.Is<Activity>(a => a.Name == "Hunting")), Times.Once);
        }

        [Test]
        public void ListActivitiesTest()
        {
            var activities = activityService.GetAllActivities();

            Assert.That(activities.Count(), Is.EqualTo(2));
            Assert.That(activities.First().Name, Is.EqualTo("Foraging"));
        }

        [Test]
        public void CreateResourceTest()
        {
            var newResource = new Resource { Name = "Food", Type = "Edible", Quantity = 10 };

            resourceService.CreateResource(newResource);

            mockResourceDataProvider.Verify(repo => repo.CreateResource(It.Is<Resource>(r => r.Name == "Food")), Times.Once);
        }

        [Test]
        public void ListResourcesTest()
        {
            var resources = resourceService.GetAllResources();

            Assert.That(resources.Count(), Is.EqualTo(2));
            Assert.That(resources.First().Name, Is.EqualTo("Water"));
        }
        [Test]
        public void FilterCharactersBySkillTest()
        {
            var filteredCharacters = characterService.GetAllCharacters()
                .Where(c => c.Skills.Contains("Medicine"))
                .ToList();

            Assert.That(filteredCharacters.Count, Is.EqualTo(1));
            Assert.That(filteredCharacters.First().Name, Is.EqualTo("Alice Bob"));
        }
        [Test]
        public void VerifyActivityDurationTest()
        {
            var activity = activityService.GetAllActivities().First(a => a.Name == "Repairing");

            Assert.That(activity.Duration, Is.EqualTo(TimeSpan.FromHours(3.5)));
        }
        [Test]
        public void VerifyActivityRequirementsTest()
        {
            var activity = activityService.GetAllActivities().First(a => a.Name == "Foraging");

            Assert.That(activity.RequiredHealthStatus, Is.EqualTo("Healthy"));
            Assert.That(activity.RequiredSkills, Does.Contain("Strength"));
            Assert.That(activity.RequiredSkills, Does.Contain("Speed"));
        }
        [Test]
        public void ListResourcesByTypeTest()
        {
            var edibleResources = resourceService.GetAllResources()
                .Where(r => r.Type == "Drinkable")
                .ToList();

            Assert.That(edibleResources.Count, Is.EqualTo(1));
            Assert.That(edibleResources.First().Name, Is.EqualTo("Water"));
        }

    }

}
