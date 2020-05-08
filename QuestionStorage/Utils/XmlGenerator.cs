using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using QuestionStorage.Models.QuizzesQuestionsModels;

namespace QuestionStorage.Utils
{
    public static class XmlGenerator
    {
        private const string PathToCorrectAnswerXml = "../QuestionStorage/wwwroot/resources/correct-answer.xml";
        private const string PathToIncorrectAnswerXml = "../QuestionStorage/wwwroot/resources/incorrect-answer.xml";
        private const string PathToAnswerXml = "../QuestionStorage/wwwroot/resources/answer-open.xml";

        private static string GetPath(string filename) =>
            $"../QuestionStorage/wwwroot/resources/{filename}";
        
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
            List<QuestionAnswerVariants> responseOptions, int typeId)
        {
            var (correctOptions, incorrectOptions) = 
                GetCorrectAndIncorrectOptions(responseOptions);
            var xmlOptions = new StringBuilder();

            if (typeId == 3)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(
                    File.ReadAllText(templatePath).Replace(
                        "$ANSWER", File.ReadAllText(PathToAnswerXml))));
            }
            
            AddAnswersToXmlDocument(File.ReadAllText(PathToCorrectAnswerXml), correctOptions.Count, 
                xmlOptions, "$CORRECT");
            AddAnswersToXmlDocument(File.ReadAllText(PathToIncorrectAnswerXml), incorrectOptions.Count,
                xmlOptions, "$INCORRECT");

            return new MemoryStream(Encoding.UTF8.GetBytes(
                File.ReadAllText(templatePath).Replace("$ANSWER", xmlOptions.ToString())));
        }

        private static void GetTemplate(string templatePath, out XmlDocument template, 
            out XmlNode questionNode, List<QuestionAnswerVariants> responseOptions, int typeId)
        {
            template = new XmlDocument();
            template.Load(ExpandTemplate(templatePath, responseOptions, typeId));
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

        private static void FillXml(QuestionsInfo question, XmlDocument result, XmlNode resultQuiz,
            XmlNode questionNode, List<QuestionAnswerVariants> responseOptions)
        {
            var dictionary = new Dictionary<string, string>
            {
                ["$NAME"] = question.QuestionName, 
                ["$TEXT"] = ReplaceUselessHtmlTags(question.QuestionText)
            };
            
            var node = result.ImportNode(questionNode, true);

            if (question.TypeId == 3)
            {
                dictionary["$CORRECT"] = TrimParagraph(ReplaceUselessHtmlTags(responseOptions.First().Answer));
            }
            else
            {
                var correct = 0;
                var incorrect = 0;
                
                foreach (var option in responseOptions)
                {
                    if (option.IsCorrect)
                    {
                        dictionary[$"$CORRECT{++correct}"] = ReplaceUselessHtmlTags(option.Answer);
                    }
                    else
                    {
                        dictionary[$"$INCORRECT{++incorrect}"] = ReplaceUselessHtmlTags(option.Answer);
                    }
                }
            }
            
            foreach(var (key, value) in dictionary)
            {
                node.InnerXml = node.InnerXml.Replace(key,value);
            }
            
            resultQuiz.AppendChild(node);
        }

        public static XmlDocument ExportToXml(QuestionsInfo question, List<QuestionAnswerVariants> responseOptions)
        {
            GetTemplate(GetTemplatePath(question.TypeId), out var template, out var questionNode, 
                        responseOptions, question.TypeId);
            GetResult(template, out var result, out var resultQuiz);
            FillXml(question, result, resultQuiz, questionNode, responseOptions);
            
            return result;
        }
        
        public static XmlDocument ExportQuestionsToXml(List<QuestionsInfo> questions, 
            List<List<QuestionAnswerVariants>> responseOptions)
        {
            var xmlDocuments = questions
                .Select((question, i) => ExportToXml(question, responseOptions[i])).ToList();
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

        private static string TrimParagraph(string responseOption) =>
            responseOption.Replace("<p>", string.Empty)
                          .Replace("</p>", string.Empty);

        private static string TrimXmlTags(string xmlDocument)
        {
            return xmlDocument
                .Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?><quiz>", string.Empty)
                .Replace("</quiz>", string.Empty);
        }

        private static string ReplaceUselessHtmlTags(string questionText) =>
            questionText.Replace("<code>", CodeOpeningTag)
                        .Replace("</code>", CodeClosingTag)
                        .Replace("    ", "&nbsp; &nbsp;");

        private const string CodeOpeningTag =
            "<span lang=\"EN-US\" class=\"\" style=\"font-family: &quot;Courier New&quot;, Courier, mono;\">";

        private const string CodeClosingTag = "</span>";
    }
}