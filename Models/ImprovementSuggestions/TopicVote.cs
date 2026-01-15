using Sammlerplattform.Models.UserSettings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ImprovementSuggestions
{
    public class TopicVote
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public required string UserId { get; set; }
        public UsingIdentityUser User { get; set; } = null!;
        public int TopicId { get; set; }
        public Topic Topic { get; set; } = null!;
        public int VoteTypeInt { get; set; }
        [NotMapped]
        public VoteType VoteType 
        { 
            get => (VoteType)VoteTypeInt; 
            set => VoteTypeInt = (int)value; 
        }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }

    public enum VoteType
    {
        Up = 1,
        Down = -1
    }
}
