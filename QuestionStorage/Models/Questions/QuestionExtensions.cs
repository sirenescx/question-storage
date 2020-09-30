using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Models.Types;
using QuestionStorage.Models.Users;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.Questions
{
    public static class QuestionExtensions
    {
        internal static async Task<QuestionsInfo> CreateQuestion(StorageContext context, string questionText,
            StringValues typeInfo, string questionName, int authorId, StringValues checkTemplate, int courseId, string xml = null)
        {
            var questionId = await DataStorage.GetIdAsync(context.QuestionsInfo,
                questionsInfo => questionsInfo.QuestId);

            return new QuestionsInfo
            {
                QuestId = questionId,
                QuestionName = questionName.Trim(),
                QuestionText = questionText.Trim(),
                TypeId = TypesExtensions.GetTypeId(typeInfo),
                IsTemplate = checkTemplate.Any() && StorageUtils.PreprocessCheckboxValues(checkTemplate).First(),
                VersionId = 1,
                SourceQuestId = questionId,
                AuthorId = authorId,
                CourseId = courseId
            };
        }

        internal static async Task<QuestionsInfo> CreateQuestion(StorageContext context, StringValues questionName,
            StringValues questionText, StringValues typeInfo, int authorId, int courseId, int versionId = 1, int sourceQuestionId = -1)
        {
            var questionId = await DataStorage.GetIdAsync(context.QuestionsInfo,
                questionsInfo => questionsInfo.QuestId);
            
            return new QuestionsInfo
            {
                QuestId = questionId,
                QuestionName = questionName,
                QuestionText = questionText,
                TypeId = TypesExtensions.GetTypeId(typeInfo),
                VersionId = versionId,
                SourceQuestId = sourceQuestionId == -1 ? questionId : sourceQuestionId,
                AuthorId = authorId,
                CourseId = courseId
            };
        }

        internal static async Task<int> GetSourceVersionId(StorageContext context, QuestionsInfo editableQuestion)
        {
            while (editableQuestion.SourceQuestId != editableQuestion.QuestId)
            {
                var question = editableQuestion;
                editableQuestion =
                    await context.QuestionsInfo.Where(
                        q => q.QuestId == question.SourceQuestId).FirstOrDefaultAsync();
            }

            return editableQuestion.QuestId;
        }

        internal static async Task<QuestionAnswerVariants> CreateQuestionAnswerVariant(StorageContext context,
            int questionId, string text, bool isCorrect = true, int sortCode = 0) =>
            new QuestionAnswerVariants
            {
                VariantId = await DataStorage.GetIdAsync(context.QuestionAnswerVariants, 
                    questionAnswerVariants => questionAnswerVariants.VariantId),
                QuestId = questionId,
                Answer = text,
                IsCorrect = isCorrect,
                SortCode = sortCode
            };
        
        internal static async Task<bool> QuestionExists(StorageContext context, int id) =>
            await context.QuestionsInfo.AnyAsync(q => q.QuestId == id);
    }
}