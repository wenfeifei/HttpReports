﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace HttpReports.Dashboard.Views
{
    using System;
    
    #line 3 "..\..\Views\HttpReports\UserLogin.cshtml"
    using System.Collections.Generic;
    
    #line default
    #line hidden
    using System.Linq;
    using System.Text;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    internal partial class UserLogin : HttpReports.Dashboard.Views.RazorPage
    {
#line hidden

        public override void Execute()
        {


WriteLiteral("\r\n");




            
            #line 4 "..\..\Views\HttpReports\UserLogin.cshtml"
  
    ViewData["Title"] = "UserLogin";

    var lang = ViewData["Language"] as HttpReports.Dashboard.Services.Localize;

    Layout = null;



            
            #line default
            #line hidden
WriteLiteral(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>HttpReports</title>

    <link class=""theme"" id=""theme_light"" href=""/HttpReportsStaticFiles/Content/css/theme/light.css?ver=1.0.5"" rel=""stylesheet"" />
    <link class=""theme"" id=""theme_dark"" href=""/HttpReportsStaticFiles/Content/css/theme/dark.css?ver=1.0.5"" rel=""stylesheet"" />

    <link href=""/HttpReportsStaticFiles/Content/login/bootstrap/css/bootstrap.css"" rel=""stylesheet"" />
    <link rel=""stylesheet"" type=""text/css"" href=""/HttpReportsStaticFiles/Content/login/css/my-login.css"">
    <link href=""/HttpReportsStaticFiles/Content/login/css/style.css"" rel=""stylesheet"" />
    <link href=""/HttpReportsStaticFiles/Content/css/animate/_3._5._2/animate.min.css"" rel=""stylesheet"" />


    <script src=""/HttpReportsStaticFiles/Content/login/js/jquery.min.js""></script>
    <script src=""/HttpReportsStaticFiles/Content/login/js/jquery.particleground.js""></script>

    <script>  var lang = JSON.parse('");


            
            #line 33 "..\..\Views\HttpReports\UserLogin.cshtml"
                                 Write(Raw(Newtonsoft.Json.JsonConvert.SerializeObject(lang)));

            
            #line default
            #line hidden
WriteLiteral(@"');   </script>

    <script src=""/HttpReportsStaticFiles/Content/page/user_login.js?v=1.0.3""></script>
    <script src=""/HttpReportsStaticFiles/Content/page/main.js?ver=1.0.5""></script>
    <link href=""/HttpReportsStaticFiles/Content/alert/alert.css?ver=1.0.5"" rel=""stylesheet"" />
    <script src=""/HttpReportsStaticFiles/Content/alert/alert.js?ver=1.0.5""></script>

    <link href=""/HttpReportsStaticFiles/Content/message/message.css"" rel=""stylesheet"" />
    <script src=""/HttpReportsStaticFiles/Content/message/message.js""></script>
    <script src=""/HttpReportsStaticFiles/Content/common/basic.js?ver=1.0.5""></script>
    <link rel=""icon"" href=""/HttpReportsStaticFiles/Content/assets/img/img.ico"" type=""image/x-icon"" />


</head>
<body class=""my-login-page"">

    <div id=""particles"">

        <div class=""intro"">

            <div class=""card-wrapper"" style=""margin:0 auto;"">

                <div class=""card fat"">
                    <div class=""card-body"">

                        <div class=""brand"">
                            <img src=""/HttpReportsStaticFiles/Content/login/img/logo3.png"">
                        </div>

                        <div style=""text-align:center"">
                            <h3 class=""hr_title animated slideInDown"">HttpReports</h3>
                        </div>

                        <div class=""form-group"">
                            <label for=""email"">");


            
            #line 67 "..\..\Views\HttpReports\UserLogin.cshtml"
                                          Write(lang.Login_UserName);

            
            #line default
            #line hidden
WriteLiteral(@"</label>

                            <input type=""text"" class=""form-control username"" name=""email"" value="""" required autofocus>
                        </div>

                        <div class=""form-group"">
                            <label for=""password"">
                                ");


            
            #line 74 "..\..\Views\HttpReports\UserLogin.cshtml"
                           Write(lang.Login_Password);

            
            #line default
            #line hidden
WriteLiteral(@"

                            </label>
                            <input type=""password"" class=""form-control password"" name=""password"" required data-eye>
                        </div>

                        <div class=""form-group"">
                            <button onclick=""login()"" type=""submit"" class=""btn btn-primary btn-block"">
                                ");


            
            #line 82 "..\..\Views\HttpReports\UserLogin.cshtml"
                           Write(lang.Login_Button);

            
            #line default
            #line hidden
WriteLiteral("\r\n                            </button>\r\n                        </div>\r\n        " +
"            </div>\r\n                </div>\r\n\r\n            </div>\r\n\r\n        </di" +
"v>\r\n\r\n\r\n    </div>\r\n\r\n\r\n</body>\r\n</html>  ");


        }
    }
}
#pragma warning restore 1591
