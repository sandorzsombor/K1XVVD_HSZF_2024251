using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Model
{
    public class Character
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string HealthStatus { get; set; }
        public int HungerLevel { get; set; }
        public int ThirstLevel { get; set; }
        public string Mood { get; set; }
        public string Skills { get; set; }
        public virtual ICollection<Activity> Activities { get; set; }
        public Character()
        {
            Activities = new HashSet<Activity>();
        }
    }

}
