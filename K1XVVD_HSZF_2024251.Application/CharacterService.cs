using K1XVVD_HSZF_2024251.Model;
using K1XVVD_HSZF_2024251.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Application
{
    public interface ICharacterService
    {
        void CreateCharacter(Character character);
        void UpdateCharacter(Character character);
        void DeleteCharacter(int id);
        IEnumerable<Character> GetAllCharacters();
        Character GetCharacterById(int id);
    }

    public class CharacterService : ICharacterService
    {
        private readonly ICharacterDataProvider _characterDataProvider;
        public event EventHandler<string>? CriticalHungerLevelReached;
        public event EventHandler<string>? CriticalThirstLevelReached;

        public CharacterService(ICharacterDataProvider characterDataProvider)
        {
            _characterDataProvider = characterDataProvider;
        }

        public void CreateCharacter(Character character)
        {
            if (string.IsNullOrWhiteSpace(character.Name))
                throw new ArgumentException("Character name cannot be empty.");

            _characterDataProvider.CreateCharacter(character);
        }

        public void UpdateCharacter(Character character)
        {
            var existingCharacter = _characterDataProvider.GetCharacterById(character.Id);
            if (existingCharacter == null)
                throw new InvalidOperationException($"Character with ID {character.Id} does not exist.");

            _characterDataProvider.UpdateCharacter(character);
        }

        public void DeleteCharacter(int id)
        {
            var existingCharacter = _characterDataProvider.GetCharacterById(id);
            if (existingCharacter == null)
                throw new InvalidOperationException($"Character with ID {id} does not exist.");

            _characterDataProvider.DeleteCharacter(id);
        }

        public IEnumerable<Character> GetAllCharacters()
        {
            return _characterDataProvider.ReadAllCharacters();
        }

        public Character GetCharacterById(int id)
        {
            var character = _characterDataProvider.GetCharacterById(id);
            if (character == null)
                throw new InvalidOperationException($"Character with ID {id} does not exist.");

            return character;
        }
        public void CheckNeeds(int characterId)
        {
            var character = _characterDataProvider.GetCharacterById(characterId);
            if (character == null)
            {
                throw new InvalidOperationException("Character not found.");
            }

            if (character.HungerLevel >= 100)
            {
                CriticalHungerLevelReached?.Invoke(this, $"Warning: {character.Name} has reached critical hunger level.");
                character.HungerLevel -= 30;
            }

            if (character.ThirstLevel >= 100)
            {
                CriticalThirstLevelReached?.Invoke(this, $"Warning: {character.Name} has reached critical thirst level.");
                character.ThirstLevel -= 30;
            }

            _characterDataProvider.UpdateCharacter(character);
        }

    }

}
