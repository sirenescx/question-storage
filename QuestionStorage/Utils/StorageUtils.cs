using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QuestionStorage.Models.QuizzesQuestionsModels;

namespace QuestionStorage.Utils
{
    public static class StorageUtils
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

        internal static QuestionsInfo CreateQuestion(string questionText, int questionId, string typeInfo,
            string questionName, int flags = 0)
        {
            var question = new QuestionsInfo
            {
                QuestId = questionId,
                QuestionName = questionName.Trim(),
                QuestionText = questionText.Trim(),
                TypeId = GetTypeId(typeInfo),
                Flags = flags
            };

            return question;
        }

        internal static QuestionAnswerVariants CreateAnswerVariant(int variantId, int questionId, string answer,
            bool isCorrect, int sortCode = 0)
        {
            var answerVariant = new QuestionAnswerVariants
            {
                VariantId = variantId,
                QuestId = questionId,
                Answer = answer.Trim(),
                IsCorrect = isCorrect
            };

            return answerVariant;
        }

        internal static TagsInfo CreateTag(int tagId, string name, int? parentId = null)
        {
            var tag = new TagsInfo
            {
                TagId = tagId,
                Name = name.Trim()
            };

            return tag;
        }

        internal static int GetTypeId(string typeInfo) =>
            typeInfo.Equals("sc") ? 1 : typeInfo.Equals("mc") ? 2 : 3;

        internal static void EditQuestion(
            QuestionsInfo question, string questionName, string questionText, string typeInfo)
        {
            question.QuestionName = questionName.Trim();
            question.QuestionText = questionText.Trim();
            question.TypeId = GetTypeId(typeInfo);
        }

        internal static bool IsValidTagId(string tagInfo) => tagInfo.ElementAt(0).Equals('ŧ');

        private static string GetHashStringFromByteArray(byte[] hash)
        {
            var hashString = new StringBuilder();
            foreach (var @byte in hash)
            {
                hashString.Append(@byte);
            }

            return hashString.ToString();
        }

        public static string GetPasswordHash(string password) =>
            GetHashStringFromByteArray(
                new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(password)));
    }
}