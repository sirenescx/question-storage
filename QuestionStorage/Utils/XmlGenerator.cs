using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using QuestionStorage.Models.Questions;

namespace QuestionStorage.Utils
{
    public static class XmlGenerator
    {
        private const string PathToCorrectAnswerXml = "/resources/correct-answer.xml";
        private const string PathToIncorrectAnswerXml = "/resources/incorrect-answer.xml";
        private const string PathToAnswerXml = "/resources/answer-open.xml";

        private static string GetPath(string filename) =>
            $"/resources/{filename}";
        
        private static string GetDirectPath(string filename, string webRootPath) =>
            $"{webRootPath}/{filename}";
        
        private static string GetTemplatePath(int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return GetPath("single-choice.xml");
                case 2:
                    return GetPath("multiple-choice.xml");
                case 3:
                    return GetPath("open-answer.xml");
                default:
                    return string.Empty;
            }
        }

        private static void AddAnswersToXmlDocument(string template, int count, StringBuilder xmlOptions, string value)
        {
            for (var i = 0; i < count; ++i)
            {
                xmlOptions.Append(template.Replace(value, $"{value}{i + 1}"));
            }
        }

        private static MemoryStream ExpandTemplate(string templatePath, 
            List<QuestionAnswerVariants> responseOptions, int typeId, string webRootPath)
        {
            var (correctOptions, incorrectOptions) = 
                GetCorrectAndIncorrectOptions(responseOptions);
            var xmlOptions = new StringBuilder();

            if (typeId == 3)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(
                    File.ReadAllText(templatePath).Replace(
                        "$ANSWER", File.ReadAllText(GetDirectPath(PathToAnswerXml, webRootPath)))));
            }
            
            AddAnswersToXmlDocument(File.ReadAllText(GetDirectPath(PathToCorrectAnswerXml, webRootPath)), 
                correctOptions.Count, xmlOptions, "$CORRECT");
            AddAnswersToXmlDocument(File.ReadAllText(GetDirectPath(PathToIncorrectAnswerXml, webRootPath)), 
                incorrectOptions.Count, xmlOptions, "$INCORRECT");

            return new MemoryStream(Encoding.UTF8.GetBytes(
                File.ReadAllText(templatePath).Replace("$ANSWER", xmlOptions.ToString())));
        }

        public static string GetCode(string variables, string webRootPath)
        {
            var firstPart = File.ReadAllText(GetDirectPath("/resources/first", webRootPath));
            var secondPart = File.ReadAllText(GetDirectPath("/resources/second", webRootPath));
            return $"{firstPart}{variables}{secondPart}";
        }

        private static void GetTemplate(string templatePath, out XmlDocument template, 
            out XmlNode questionNode, List<QuestionAnswerVariants> responseOptions, int typeId, string webRootPath)
        {
            template = new XmlDocument();
            template.Load(ExpandTemplate(templatePath, responseOptions, typeId, webRootPath));
            questionNode = template.SelectNodes("//question")[0];
        }

        private static void GetResult(XmlDocument template, out XmlDocument result, out XmlNode resultQuiz)
        {
            result = (XmlDocument)template.CloneNode(true);
            var removableNodes = result.SelectNodes("//question");
            foreach (XmlNode node in removableNodes)
            {
                node.ParentNode.RemoveChild(node);
            }
            resultQuiz = result.SelectSingleNode("//quiz");
        }

        private static (List<QuestionAnswerVariants>, List<QuestionAnswerVariants>) 
            GetCorrectAndIncorrectOptions(List<QuestionAnswerVariants> responseOptions)
        {
            PreprocessResponseOptions(responseOptions);
            var correct = new List<QuestionAnswerVariants>();
            var incorrect = new List<QuestionAnswerVariants>();

            foreach (var option in responseOptions)
            {
                if (option.IsCorrect)
                {
                    correct.Add(option);
                }
                else
                {
                    incorrect.Add(option);
                }
            }

            return (correct, incorrect);
        }

        private static void PreprocessResponseOptions(List<QuestionAnswerVariants> responseOptions)
        {
            foreach (var responseOption in responseOptions)
            {
                responseOption.Answer = responseOption.Answer.Trim();
                while (responseOption.Answer.EndsWith("&nbsp;"))
                {
                    responseOption.Answer = responseOption.Answer
                        .Substring(0, responseOption.Answer.LastIndexOf("&nbsp;", StringComparison.Ordinal));
                    responseOption.Answer = responseOption.Answer.Trim();
                }
            }
        }

        private static void FillXml(QuestionsInfo question, XmlDocument result, XmlNode resultQuiz,
            XmlNode questionNode, List<QuestionAnswerVariants> responseOptions)
        {
            var dictionary = new Dictionary<string, string>
            {
                ["$NAME"] = question.QuestionName, 
                ["$TEXT"] = ReplaceHtmlTags(question.QuestionText)
            };
            
            var node = result.ImportNode(questionNode, true);

            if (question.TypeId == 3)
            {
                dictionary["$CORRECT"] = TrimParagraph(ReplaceHtmlTags(responseOptions.First().Answer));
            }
            else
            {
                var correct = 0;
                var incorrect = 0;
                
                foreach (var option in responseOptions)
                {
                    if (option.IsCorrect)
                    {
                        dictionary[$"$CORRECT{++correct}"] = ReplaceHtmlTags(option.Answer);
                    }
                    else
                    {
                        dictionary[$"$INCORRECT{++incorrect}"] = ReplaceHtmlTags(option.Answer);
                    }
                }
            }
            
            foreach(var (key, value) in dictionary)
            {
                node.InnerXml = node.InnerXml.Replace(key,value);
            }
            
            resultQuiz.AppendChild(node);
        }

        public static XmlDocument ExportToXml(QuestionsInfo question, List<QuestionAnswerVariants> responseOptions, 
            IWebHostEnvironment environment)
        {
            var path = environment.WebRootPath + GetTemplatePath(question.TypeId);
            
            GetTemplate(path, out var template, out var questionNode, 
                        responseOptions, question.TypeId, environment.WebRootPath);
            
            GetResult(template, out var result, out var resultQuiz);
            FillXml(question, result, resultQuiz, questionNode, responseOptions);
            
            return result;
        }
        
        public static XmlDocument ExportQuestionsToXml(List<QuestionsInfo> questions, 
            List<List<QuestionAnswerVariants>> responseOptions, IWebHostEnvironment environment)
        {
            var xmlDocuments = questions
                .Select((question, i) => ExportToXml(question, responseOptions[i], environment)).ToList();
            var template = xmlDocuments.First().OuterXml;
            var questionsXml = new StringBuilder();
            for (var i = 1; i < xmlDocuments.Count; ++i)
            {
                questionsXml.Append(TrimXmlTags(xmlDocuments[i].OuterXml));
            }
            var insertIndex = template.IndexOf("</quiz>", StringComparison.Ordinal);
            var questionsXmlDocument = new XmlDocument();
            
            template = template.Insert(insertIndex, questionsXml.ToString());
            questionsXmlDocument.LoadXml(template);

            return questionsXmlDocument;
        }
        
        internal static string FindQuestionType(XmlDocument document)
        {
            var node = document.GetElementsByTagName("question");
            if (node.Count == 0)
            {
                throw new XmlException("Invalid XML file format.");
            }

            var type = node[0].Attributes.GetNamedItem("type");
            if (type is null)
            {
                throw new XmlException("XML file does not contain information about question type.");
            }

            return type.Value;
        }
        
        internal static string GetElementTextFromXml(XmlDocument document, string tagName)
        {
            var element = document.GetElementsByTagName(tagName);
            if (element.Count == 0)
            {
                throw new Exception("Invalid XML file format.");
            }
            
            return (from XmlNode node in element select TrimParagraph(node.InnerText)).FirstOrDefault();
        }

        private static List<string> GetElementsFromXml(XmlDocument document, string tagName)
        {
            var elements = document.GetElementsByTagName(tagName);
            if (elements.Count == 0)
            {
                throw new Exception("Invalid XML file format.");
            }

            return (from XmlNode node in elements select node.InnerXml).ToList();
        }

        private static string[] GetResponseOptionsFromXml(List<string> strings)
        {
            var regex = new Regex("<p>.*?</p>");
            
            return strings.Select
                    (s => regex.Matches(s).Count != 0 ? TrimParagraph(regex.Matches(s)[0].Value) : s).ToArray();
        }
        
        private static bool[] GetResponseOptionsCorrectness(XmlDocument document, string tagName)
        {
            var elements = document.GetElementsByTagName(tagName);
            if (elements.Count == 0)
            {
                throw new XmlException("Invalid XML file format.");
            }

            var values = new List<bool>();

            foreach (XmlNode node in elements)
            {
                var fraction = node.Attributes.GetNamedItem("fraction");
                if (fraction is null)
                {
                    throw new XmlException("XML file does not contain information about correct answer.");
                }
                
                values.Add(fraction.Value == "100");
                
            }
            
            return values.ToArray();
        }

        public static (string[] responseOptions, bool[] correct) GetResponseInfo(XmlDocument document)
        {
            return (GetResponseOptionsFromXml(GetElementsFromXml(document, "answer")), 
                    GetResponseOptionsCorrectness(document, "answer"));
        }

        private static string TrimParagraph(string responseOption) =>
            responseOption.Replace("<p>", string.Empty)
                          .Replace("</p>", string.Empty);

        private static string TrimXmlTags(string xmlDocument)
        {
            return xmlDocument
                .Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?><quiz>", string.Empty)
                .Replace("</quiz>", string.Empty);
        }

        private static string ReplaceHtmlTags(string questionText) =>
            questionText.Replace("<code>", CodeOpeningTag)
                        .Replace("</code>", CodeClosingTag)
                        .Replace("    ", "&nbsp; &nbsp;");

        private const string CodeOpeningTag =
            "<span lang=\"EN-US\" class=\"\" style=\"font-family: &quot;Courier New&quot;, Courier, mono;\">";

        private const string CodeClosingTag = "</span>";
    }
}