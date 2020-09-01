using System;
using System.Threading.Tasks;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.Quizzes
{
    public static class QuizExtensions
    {
        internal static async Task<QuizzesInfo> CreateQuiz(HSE_QuestContext context, string name, string date) =>
            new QuizzesInfo
            {
                QuizId = await DataStorage.GetIdAsync(context.QuizzesInfo, quizInfo => quizInfo.QuizId),
                Name = name,
                Date = DateTime.Parse(date)
            };
    }
}