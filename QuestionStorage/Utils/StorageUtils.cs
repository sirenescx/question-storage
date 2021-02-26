using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Models;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Users;
using TextCopy;

namespace QuestionStorage.Utils
{
    public static class StorageUtils
    {
        private static readonly QRandom Random = new QRandom();

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

        private static string GetHashStringFromByteArray(byte[] hash)
        {
            var hashString = new StringBuilder();
            foreach (var @byte in hash)
            {
                hashString.Append(@byte);
            }

            return hashString.ToString();
        }

        internal static string GetPasswordHash(string password, Encoding enc = null)
        {
            enc ??= Encoding.ASCII;

            return GetHashStringFromByteArray(
                new SHA1CryptoServiceProvider().ComputeHash(enc.GetBytes(password)));
        }

        internal static async Task<string> ReadAsStringAsync(this IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(await reader.ReadLineAsync());
                }
            }

            return result.ToString();
        }

        internal static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            try
            {
                Regex.Match(string.Empty, pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private static int GenerateAssemblyNumber() =>
            int.Parse(Random.StringFromRegex(
                "([1-8][0-9]{6}|9[0-8][0-9]{5}|99[0-8][0-9]{4}|999[0-8][0-9]{3}|9999[0-8][0-9]{2}|99999[0-8][0-9]|999999[0-9]|10000000)"));

        internal static string
            CreateStringArrayFromResponseOptions(IEnumerable<QuestionAnswerVariants> responseOptions) =>
            responseOptions.Aggregate(
                "new [] {",
                (current, ro) => current + $"$\"{{{ro.Answer.Trim('$')}}}\", ") + "}";

        internal static bool[] GetResponseOptionsCorrectness(List<QuestionAnswerVariants> answers)
        {
            var correct = new bool[answers.Count];
            for (var i = 0; i < answers.Count; ++i)
            {
                correct[i] = answers[i].IsCorrect;
            }

            return correct;
        }

        internal static string GetInterpolatedString(string text) =>
            Regex.Replace(text, @"\$([\s\S]*)\$", "{" + @"$1" + "}",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private static readonly IEnumerable<string> DefaultNamespaces = new[]
        {
            "System",
            "System.IO",
            "System.Net",
            "System.Linq",
            "System.Text.RegularExpressions",
            "System.Collections.Generic"
        };

        internal static Assembly CompileSourceRoslyn(string sourceCode)
        {
            using (var ms = new MemoryStream())
            {
                var assemblyFileName = $"QStorage{GenerateAssemblyNumber()}.dll";
                var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

                var compilation = CSharpCompilation.Create(assemblyFileName,
                    new[] {CSharpSyntaxTree.ParseText(sourceCode)},
                    new[]
                    {
                        MetadataReference.CreateFromFile(typeof(StorageUtils).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.Extensions.dll"))
                    },
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithUsings(DefaultNamespaces)
                );

                var compilationResult = compilation.Emit(ms);
                if (!compilationResult.Success)
                {
                    foreach (var issue in compilationResult.Diagnostics
                        .Select(codeIssue => $"ID: {codeIssue.Id}, " +
                                             $"Message: {codeIssue.GetMessage()}, " +
                                             $"Location: {codeIssue.Location.GetLineSpan()}, " +
                                             $"Severity: {codeIssue.Severity} "))
                    {
                        Console.WriteLine(issue);
                    }
                }

                var assembly = Assembly.Load(ms.ToArray());

                return assembly;
            }
        }

        internal static void CopyToClipboard(in StringValues email, string password)
        {
            var clipboard = new Clipboard();
            clipboard.SetText($"{email}\n{password}");
        }

        internal static async Task<int> GetUserId(StorageContext context, string email) =>
            (await DataStorage.GetByPredicateAsync(context.Users,
                user => user.Email.Equals(email))).Id;

        internal static async Task<bool> CheckAccess(StorageContext context, int courseId, string userName)
        {
            var userId = await GetUserId(context, userName);

            var userCoursesIdentifiers = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(
                context.UsersCourses,
                usersCourses => usersCourses.UserId == userId,
                usersCourses => usersCourses.CourseId);

            return userCoursesIdentifiers.Contains(courseId);
        }
        
        internal static async Task<int> GetQuestionsCountForCourse(StorageContext context, int courseId)
        {
            var questions = await DataStorage.GetTypedListByPredicateAndSelectorAsync(context.QuestionsInfo,
                questionsInfo => questionsInfo.CourseId == courseId,
                questionsInfo => questionsInfo);

            var lastVersions =
                questions
                    .GroupBy(question => question.SourceQuestId)
                    .Select(versions => versions.OrderBy(version => version.VersionId)
                        .Last()).ToList();

            return lastVersions.Count;
        }

        private static readonly HashSet<char> Delimiters = new HashSet<char>
        {
            '}', '{'
        };

        private static string ReplaceDelimiters(string s)
        {
            return Delimiters.Aggregate(s, (current, delimiter) => current
                .Replace(delimiter.ToString(), $@"\{delimiter}"));
        }

        private static int GetFirstNotWhitespaceIndex(string s)
        {
            var indexOfFirstNotWhitespace = 0;
            for (var i = 0; i < s.Length; ++i)
            {
                if (s[i] == ' ')
                {
                    continue;
                }

                indexOfFirstNotWhitespace = i;
                break;
            }

            return indexOfFirstNotWhitespace;
        }
        
        public static string GenerateToken()
        {
            var time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            var key = Guid.NewGuid().ToByteArray();
            var token = Convert.ToBase64String(time.Concat(key).ToArray());

            return token;
        }

        public static bool DecodeToken(string token)
        {
            var data = Convert.FromBase64String(token);
            var when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            
            return when >= DateTime.UtcNow.AddHours(-24);
        }
        
    }
}