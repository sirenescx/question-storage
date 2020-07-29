using System;
using System.ComponentModel.DataAnnotations;
using QuestionStorage.Models.QuizzesQuestionsModels;

namespace QuestionStorage.Models.ViewModels
{
    public class TestViewModel : QuizzesInfo
    {
        [Range(1, int.MaxValue)]
        public string QuestionId { get; set; }
    }
}