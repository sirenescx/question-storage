using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models;

namespace QuestionStorage.Helpers
{
    public static class Common
    {
        internal static bool[] PreprocessCheckboxValues(string checkboxValues)
        {
            var values = checkboxValues.Split(',');
            var compressedValues = new List<bool>();
            for (var i = 0; i < values.Length; ++i)
            {
                if (i + 1 < values.Length && values[i + 1] == "on")
                {
                    compressedValues.Add(true);
                    ++i;
                }
                else
                {
                    compressedValues.Add(false);
                }
            }

            return compressedValues.ToArray();
        }

        internal static async Task SaveToDatabase<T>(StorageContext context, T item)
        {
            await context.AddAsync(item);
            await context.SaveChangesAsync();
        }

        internal static async Task<int> GetUserId(StorageContext context, string email) =>
            (await context.Users.FirstOrDefaultAsync(user => user.Email.Equals(email))).Id;

        internal static async Task<bool> CheckAccess(StorageContext context, int courseId, string userName)
        {
            var userId = await GetUserId(context, userName);

            var userCoursesIdentifiers = new HashSet<int>(await context.UsersCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync());

            return userCoursesIdentifiers.Contains(courseId);
        }

        internal static async Task<int> GetQuestionsCountForCourse(StorageContext context, int courseId)
        {
            var questions = await context.Questions
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            var lastVersions =
                questions
                    .GroupBy(question => question.SourceId)
                    .Select(versions => versions.OrderBy(version => version.VersionId)
                        .Last()).ToList();

            return lastVersions.Count;
        }

        internal static void ValidateField(string field, ModelStateDictionary modelState,
            (string key, string errorMessage) modelError)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                modelState.AddModelError(modelError.key, modelError.errorMessage);
            }
        }
    }
}