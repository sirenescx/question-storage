using System.Collections.Generic;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Tags;

namespace QuestionStorage.Models.ViewModels
{
    public class ListingViewModel
    {
        public IEnumerable<Question> Questions { get; set; }
        
        public HashSet<Tag> Tags { get; set; }
    }
}