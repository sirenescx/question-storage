#pragma checksum "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "ebbc9807d6c669ac2265d58b8cd9048ae180576f"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Questions_Create), @"mvc.1.0.view", @"/Views/Questions/Create.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/_ViewImports.cshtml"
using QuestionStorage;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/_ViewImports.cshtml"
using QuestionStorage.Models;

#line default
#line hidden
#nullable disable
#nullable restore
#line 1 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
using QuestionStorage.Models.QuizzesQuestionsModels;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ebbc9807d6c669ac2265d58b8cd9048ae180576f", @"/Views/Questions/Create.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"de651716945de7d2a5e15f0b428a494276bb77c4", @"/Views/_ViewImports.cshtml")]
    public class Views_Questions_Create : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<QuestionStorage.Models.ViewModels.QuestionViewModel>
    {
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper;
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 4 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
  
    ViewData["Title"] = "Create";

    var qTypes = new Dictionary<string, string>
    {
        {"sc", "Single choice"},
        {"mc", "Multiple choice"},
        {"oa", "Open answer"}
    };

    var selectList = new SelectList(qTypes.Select(
        x => new {Value = x.Key, Text = x.Value}), "Value", "Text");

    HashSet<TagsInfo> tags = ViewBag.Tags;
    var tagList = new SelectList(tags.Select(t => new {Value = $"ŧ{t.TagId}", Text = t.Name}),
        "Value", "Text");

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n<!DOCTYPE html>\r\n<meta charset=\"utf-8\">\r\n<html lang=\"en\">\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ebbc9807d6c669ac2265d58b8cd9048ae180576f4321", async() => {
                WriteLiteral("\r\n    <title>Create</title>\r\n    <link rel=\"stylesheet\" href=\"/lib/bootstrap/dist/css/custom-stylesheet.css\">\r\n    <script src=\"/js/add-to-table.js\"></script>\r\n    <script src=\"/js/display-answer-variants.js\"></script>\r\n");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ebbc9807d6c669ac2265d58b8cd9048ae180576f5511", async() => {
                WriteLiteral("\r\n");
#nullable restore
#line 33 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
 using (Html.BeginForm("Create", "Questions", FormMethod.Post))
{
    

#line default
#line hidden
#nullable disable
#nullable restore
#line 35 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.AntiForgeryToken());

#line default
#line hidden
#nullable disable
#nullable restore
#line 36 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.ValidationSummary("Please provide the details below and then click submit.",
        new {@class = "text-danger"}));

#line default
#line hidden
#nullable disable
                WriteLiteral("    <h1 class=\"page-header\">Create question</h1>\r\n");
                WriteLiteral("    <p class=\"less-bottom-space\">Question title*</p>\r\n");
#nullable restore
#line 42 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.TextAreaFor(model => model.Question.QuestionName, 1, 100,
        new {@class = "textarea", id = "questionName"}));

#line default
#line hidden
#nullable disable
                WriteLiteral("    <p class=\"less-bottom-space\">Question content*</p>\r\n");
#nullable restore
#line 46 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.TextAreaFor(model => model.Question.QuestionText, 15, 100,
        new {@class = "textarea", id = "questionText"}));

#line default
#line hidden
#nullable disable
                WriteLiteral("    <hr/>\r\n");
#nullable restore
#line 50 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.Label("Question type"));

#line default
#line hidden
#nullable disable
                WriteLiteral("    <br/>\r\n");
#nullable restore
#line 52 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.DropDownListFor(model => model.Question.Type.Name, selectList,
        new {@class = "select", onchange = "displayAnswerVariants()", id = "typeOfQuestionSelector"}));

#line default
#line hidden
#nullable disable
                WriteLiteral("    <hr/>\r\n");
                WriteLiteral("    <p id=\"answerInfo\"></p>\r\n    <table id=\"answerTable\">\r\n        <thead>\r\n        <tr>\r\n        <th style=\"font-weight: normal\">Response options*</th>\r\n        <th style=\"font-weight: normal\">Correct</th>\r\n        <th>\r\n        <tbody>\r\n");
#nullable restore
#line 64 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
          
            for (var i = 0; i <= 1; ++i)
            {

#line default
#line hidden
#nullable disable
                WriteLiteral("                <tr>\r\n                    <td>\r\n                        ");
#nullable restore
#line 69 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
                   Write(Html.TextAreaFor(model => model.AnswerOption.Answer, 1, 60,
                            new {@class = "textarea"}));

#line default
#line hidden
#nullable disable
                WriteLiteral(@"
                    </td>
                    <td style=""text-align: center; vertical-align: top"">
                        <input type=""hidden"" value=""off"" name=""Correct"">
                        <input name=""Correct"" type=""radio"" value=""on"">
                    </td>
                    <td>
                    </td>
                </tr>
");
#nullable restore
#line 79 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
            }
        

#line default
#line hidden
#nullable disable
                WriteLiteral("    </table>\r\n");
                WriteLiteral("    <p>\r\n        <button class=\"button\" id=\"addResponseOptions\" type=\"button\" onclick=\"addNewRow(\'answerTable\')\">\r\n            Add new response option\r\n        </button>\r\n    </p>\r\n");
                WriteLiteral("    <hr/>\r\n");
#nullable restore
#line 91 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.Label("Tags"));

#line default
#line hidden
#nullable disable
#nullable restore
#line 92 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.DropDownList("Tags", tagList, new {id = "tagView", multiple = "multiple"}));

#line default
#line hidden
#nullable disable
                WriteLiteral("    <br/>\r\n");
                WriteLiteral("    <br/>\r\n    <input type=\"submit\" class=\"submit-button\" value=\"Create question\"/>\r\n");
#nullable restore
#line 97 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
}

#line default
#line hidden
#nullable disable
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n</html>\r\n\r\n");
            DefineSection("Scripts", async() => {
                WriteLiteral(@"
    <script>
    $(function() {
        $('#tagView').select2({
        width : ""100%"",
        tags: true,
        });
    });
    </script>

    <script>
        let textareas = document.getElementsByTagName('textarea');
        let count = textareas.length;
        for (let i = 0; i < count; ++i) {
            textareas[i].onkeydown = function(e) {
                if(e.keyCode === 9 || e.which === 9){
                    e.preventDefault();
                    let s = this.selectionStart;
                    this.value = this.value.substring(
                        0, this.selectionStart) + ""    "" + this.value.substring(this.selectionEnd);
                    this.selectionEnd = s + 1;
                }
            }
        }
        </script>

");
#nullable restore
#line 127 "/Users/maria/Documents/University/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
       await Html.RenderPartialAsync("_ValidationScriptsPartial"); 

#line default
#line hidden
#nullable disable
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<QuestionStorage.Models.ViewModels.QuestionViewModel> Html { get; private set; }
    }
}
#pragma warning restore 1591
