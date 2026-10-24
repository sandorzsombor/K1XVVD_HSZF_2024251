using K1XVVD_HSZF_2024251.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Application
{
    public interface IActivityService
    {
        void CreateActivity(Model.Activity activity);
        void UpdateActivity(Model.Activity activity);
        void DeleteActivity(int id);
        IEnumerable<Model.Activity> GetAllActivities();
        Model.Activity GetActivityById(int id);
        public event EventHandler<string>? TaskConflictOccurred;
    }

    public class ActivityService : IActivityService
    {
        private readonly IActivityDataProvider _activityDataProvider;
        private readonly ICharacterDataProvider _characterDataProvider;
        public event EventHandler<string>? TaskConflictOccurred;

        public ActivityService(IActivityDataProvider activityDataProvider, ICharacterDataProvider characterDataProvider)
        {
            _activityDataProvider = activityDataProvider;
            _characterDataProvider = characterDataProvider;
        }

        public void CreateActivity(Model.Activity activity)
        {
            if (string.IsNullOrWhiteSpace(activity.Name))
                throw new ArgumentException("Activity name cannot be empty.");

            if (activity.Duration <= TimeSpan.Zero)
                throw new ArgumentException("Activity duration must be greater than zero.");

            _activityDataProvider.CreateActivity(activity);
        }

        public void UpdateActivity(Model.Activity activity)
        {
            var existingActivity = _activityDataProvider.GetActivityById(activity.Id);
            if (existingActivity == null)
                throw new InvalidOperationException($"Activity with ID {activity.Id} does not exist.");

            _activityDataProvider.UpdateActivity(activity);
        }

        public void DeleteActivity(int id)
        {
            var existingActivity = _activityDataProvider.GetActivityById(id);
            if (existingActivity == null)
                throw new InvalidOperationException($"Activity with ID {id} does not exist.");

            _activityDataProvider.DeleteActivity(id);
        }

        public IEnumerable<Model.Activity> GetAllActivities()
        {
            return _activityDataProvider.ReadAllActivities();
        }

        public Model.Activity GetActivityById(int id)
        {
            var activity = _activityDataProvider.GetActivityById(id);
            if (activity == null)
                throw new InvalidOperationException($"Activity with ID {id} does not exist.");

            return activity;
        }
        public void AssignTask(int characterId, int activityId, DateTime startTime)
        {
            var activity = _activityDataProvider.GetActivityById(activityId);
            if (activity == null)
            {
                throw new InvalidOperationException("Activity not found.");
            }

            var character = _characterDataProvider.GetCharacterById(characterId);
            if (character == null)
            {
                throw new InvalidOperationException("Character not found.");
            }

            var conflictingTasks = _characterDataProvider.ReadAllCharacters()
                .SelectMany(c => c.Activities)
                .Where(a => a.Id == activityId && a.TimeOfDay == startTime.ToString("HH:mm"));

            if (conflictingTasks.Any())
            {
                TaskConflictOccurred?.Invoke(this, $"Conflict: Activity '{activity.Name}' is already assigned at {startTime}.");
                return;
            }

            character.Activities.Add(activity);
            _characterDataProvider.UpdateCharacter(character);
        }

    }

}
