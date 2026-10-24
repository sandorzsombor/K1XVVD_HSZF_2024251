using K1XVVD_HSZF_2024251.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Persistence.MsSql
{
    public interface ICharacterDataProvider
    {
        void CreateCharacter(Character character);
        void UpdateCharacter(Character character);
        void DeleteCharacter(int id);
        IEnumerable<Character> ReadAllCharacters();
        Character GetCharacterById(int id);
    }

    public class CharacterDataProvider : ICharacterDataProvider
    {
        private readonly SurvivalDbContext _dbContext;

        public CharacterDataProvider(SurvivalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void CreateCharacter(Character character)
        {
            _dbContext.Characters.Add(character);
            _dbContext.SaveChanges();
        }

        public void UpdateCharacter(Character character)
        {
            var existingCharacter = _dbContext.Characters
                .Include(c => c.Activities) 
                .FirstOrDefault(c => c.Id == character.Id);

            if (existingCharacter == null)
                throw new InvalidOperationException($"Character with ID {character.Id} not found.");

            existingCharacter.Name = character.Name;
            existingCharacter.HealthStatus = character.HealthStatus;
            existingCharacter.HungerLevel = character.HungerLevel;
            existingCharacter.ThirstLevel = character.ThirstLevel;
            existingCharacter.Mood = character.Mood;

            UpdateCharacterActivities(existingCharacter, character.Activities.ToList());

            _dbContext.SaveChanges();
        }

        private void UpdateCharacterActivities(Character existingCharacter, ICollection<Activity> newActivities)
        {
            var activitiesToRemove = existingCharacter.Activities
                .Where(a => !newActivities.Any(na => na.Id == a.Id))
                .ToList();

            foreach (var activity in activitiesToRemove)
            {
                existingCharacter.Activities.Remove(activity);
            }

            var activitiesToAdd = newActivities
                .Where(na => !existingCharacter.Activities.Any(a => a.Id == na.Id))
                .ToList();

            foreach (var activity in activitiesToAdd)
            {
                var existingActivity = _dbContext.Activities.FirstOrDefault(a => a.Id == activity.Id);
                if (existingActivity != null)
                {
                    existingCharacter.Activities.Add(existingActivity);
                }
            }
        }

        public void DeleteCharacter(int id)
        {
            var character = _dbContext.Characters.FirstOrDefault(c => c.Id == id);
            if (character == null)
                throw new InvalidOperationException($"Character with ID {id} not found.");

            _dbContext.Characters.Remove(character);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Character> ReadAllCharacters()
        {
            return _dbContext.Characters;
        }

        public Character GetCharacterById(int id)
        {
            return _dbContext.Characters.FirstOrDefault(c => c.Id == id);
        }
    }

}
