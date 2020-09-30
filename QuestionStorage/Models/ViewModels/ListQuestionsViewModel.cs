using System.Collections.Generic;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Tags;

namespace QuestionStorage.Models.ViewModels
{
    public class ListQuestionsViewModel
    {
        public IEnumerable<QuestionsInfo> QuestionsInfo { get; set; }
        
        public HashSet<TagsInfo> TagsInfo { get; set; }
    }
}