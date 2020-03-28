#pragma checksum "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0acff16a3bbcd59e921096bc3a951bb8b2ad1e4f"
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
#line 1 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/_ViewImports.cshtml"
using QuestionStorage;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/_ViewImports.cshtml"
using QuestionStorage.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0acff16a3bbcd59e921096bc3a951bb8b2ad1e4f", @"/Views/Questions/Create.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"de651716945de7d2a5e15f0b428a494276bb77c4", @"/Views/_ViewImports.cshtml")]
    public class Views_Questions_Create : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<QuestionsInfo>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("onload", new global::Microsoft.AspNetCore.Html.HtmlString("displayAnswerVariants()"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
#line 3 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
  
    ViewData["Title"] = "Create";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "0acff16a3bbcd59e921096bc3a951bb8b2ad1e4f3883", async() => {
                WriteLiteral("\r\n    <title>Create new question</title>\r\n    <link rel=\"stylesheet\" href=\"/lib/bootstrap/dist/css/custom-stylesheet.css\">\r\n    <script src=\"/js/add-to-table.js\"></script>\r\n    <script src=\"/js/display-answer-variants.js\"></script>\r\n");
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
#nullable restore
#line 14 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
 using (Html.BeginForm("Create", "Questions", FormMethod.Post))
{
    

#line default
#line hidden
#nullable disable
#nullable restore
#line 16 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.AntiForgeryToken());

#line default
#line hidden
#nullable disable
            WriteLiteral("    ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "0acff16a3bbcd59e921096bc3a951bb8b2ad1e4f5589", async() => {
                WriteLiteral("\r\n    <h1 class=\"page-header\">Edit question</h1>\r\n\r\n    <label for=\"questionText\">Question content</label>\r\n    <br/>\r\n    ");
#nullable restore
#line 22 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.TextAreaFor(model => model.QuestionText, 8, 100,
        new {@class = "textarea", id = "questionText"}));

#line default
#line hidden
#nullable disable
                WriteLiteral("\r\n");
                WriteLiteral("    <hr/>\r\n\r\n");
                WriteLiteral("\r\n");
#nullable restore
#line 38 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
      
        var qTypes = new Dictionary<string, string>
        {
            {"sc", "Single choice"},
            {"mc", "Multiple choice"},
            {"oa", "Open answer"}
        };

        var selectList = new SelectList(qTypes.Select(
            x => new {Value = x.Key, Text = x.Value}), "Value", "Text");
    

#line default
#line hidden
#nullable disable
                WriteLiteral("    \r\n    ");
#nullable restore
#line 50 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
Write(Html.DropDownListFor(model => model.Type.Name, selectList,
        new {@class = "select", onchange = "displayAnswerVariants()", id = "typeOfQuestionSelector"}));

#line default
#line hidden
#nullable disable
                WriteLiteral(@"
    <hr/>

    <p id=""answerInfo""></p>
    <hr/>

    <label for=""tagTable"">Tags</label>
    <table id=""tagTable"">
        <tr>
            <th></th>
            <th></th>
        </tr>
    </table>

    <p>
        <button class=""button"" id=""addTags""
                type=""button"" onclick=""addNewRow('tagTable')"">
            Add new tag
        </button>
    </p>
    <br/>
    
    <input type=""submit"" class=""submit-button"" value=""Create""/>

    ");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n");
#nullable restore
#line 76 "/Users/maria/Documents/UNIVERSITY/coursework/QuestionStorage/QuestionStorage/Views/Questions/Create.cshtml"
}

#line default
#line hidden
#nullable disable
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<QuestionsInfo> Html { get; private set; }
    }
}
#pragma warning restore 1591
