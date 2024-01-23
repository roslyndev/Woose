using System.Text;
using Woose.Data;

namespace Woose.Builder
{
    public class CodeHelper
    {
        private static readonly Lazy<CSharpCreater> cs = new Lazy<CSharpCreater>(() => new CSharpCreater());
        public static CSharpCreater CSharp { get { return cs.Value; } }

        private static readonly Lazy<MsSqlCreater> ms = new Lazy<MsSqlCreater>(() => new MsSqlCreater());
        public static MsSqlCreater MsSQL { get { return ms.Value; } }

        private static readonly Lazy<TypeScriptCreater> ts = new Lazy<TypeScriptCreater>(() => new TypeScriptCreater());
        public static TypeScriptCreater TypeScript { get { return ts.Value; } }

        private static readonly Lazy<JavaScriptCreater> jsc = new Lazy<JavaScriptCreater>(() => new JavaScriptCreater());
        public static JavaScriptCreater JavaScript { get { return jsc.Value; } }

        private static readonly Lazy<JavaCreater> js = new Lazy<JavaCreater>(() => new JavaCreater());
        public static JavaCreater Java { get { return js.Value; } }

        private static readonly Lazy<VueCreater> vue = new Lazy<VueCreater>(() => new VueCreater());
        public static VueCreater Vue { get { return vue.Value; } }

        private static readonly Lazy<YAMLCreater> yaml = new Lazy<YAMLCreater>(() => new YAMLCreater());
        public static YAMLCreater YAML { get { return yaml.Value; } }

        private static readonly Lazy<HTMLCreater> html = new Lazy<HTMLCreater>(() => new HTMLCreater());
        public static HTMLCreater HTML { get { return html.Value; } }

        private static readonly Lazy<NestJsHelper> nest = new Lazy<NestJsHelper>(() => new NestJsHelper());
        public static NestJsHelper NEST { get { return nest.Value; } }

        public CodeHelper()
        {
        }

        protected BindOption option { get; set; }

        public CodeHelper(BindOption _option)
        {
            this.option = _option;
        }

        public string Serialize(DbContext context)
        {
            StringBuilder builder = new StringBuilder(200);

            if (this.option != null && this.option.target != null)
            {
                if (!string.IsNullOrWhiteSpace(this.option.target.name))
                {
                    using (var rep = new SqlServerRepository(context))
                    {
                        var sps = rep.GetSpEntities();

                        switch (this.option.targetType)
                        {
                            case "TABLE":
                                var tableproperties = rep.GetTableProperties(this.option.target.name);
                                switch (this.option.Language.Trim().ToUpper())
                                {
                                    case "ASP.NET":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append(CodeHelper.CSharp.CreateEntity(this.option, tableproperties));
                                                break;
                                            case "CONTROLLER":
                                                builder.Append(CodeHelper.CSharp.CreateController(this.option, this.option.target, tableproperties, false));
                                                break;
                                            case "ABSTRACT":
                                                builder.Append(CodeHelper.CSharp.CreateAbstract(this.option, this.option.target, tableproperties, false));
                                                break;
                                            case "REPOSITORY":
                                                builder.Append(CodeHelper.CSharp.CreateRepository(this.option, this.option.target, tableproperties, false));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "JAVA":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append(CodeHelper.Java.CreateEntity(this.option, tableproperties));
                                                break;
                                            case "CONTROLLER":
                                                builder.Append("");
                                                break;
                                            case "ABSTRACT":
                                                builder.Append("");
                                                break;
                                            case "REPOSITORY":
                                                builder.Append("");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "DATABASE":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "MSSQL":
                                                builder.Append(CodeHelper.MsSQL.CreateSaveSP(this.option, tableproperties));
                                                break;
                                            case "MYSQL":
                                                builder.Append("");
                                                break;
                                            case "MONGODB":
                                                builder.Append("");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "TYPESCRIPT":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append(CodeHelper.TypeScript.CreateEntity(this.option, tableproperties));
                                                break;
                                            case "CONTROLLER":
                                                builder.Append("");
                                                break;
                                            case "ABSTRACT":
                                                builder.Append("");
                                                break;
                                            case "REPOSITORY":
                                                builder.Append("");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "NODE.JS":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append(CodeHelper.JavaScript.NodeSequelizeEntitiyCreate(this.option, tableproperties));
                                                break;
                                            case "CONTROLLER":
                                                builder.Append("");
                                                break;
                                            case "ABSTRACT":
                                                builder.Append("");
                                                break;
                                            case "REPOSITORY":
                                                builder.Append(CodeHelper.JavaScript.NodeSequelizeEntitiySaveMethod(this.option, tableproperties));
                                                break;
                                            case "YAML":
                                                builder.Append(CodeHelper.YAML.CreateEntity(this.option, tableproperties));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "NEST.JS":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append(CodeHelper.NEST.EntitiyCreate(this.option, tableproperties));
                                                break;
                                            case "CONTROLLER":
                                                builder.Append(CodeHelper.NEST.ControllerCreate(this.option, tableproperties));
                                                break;
                                            case "DTO":
                                                builder.Append(CodeHelper.NEST.DtoCreate(this.option, tableproperties));
                                                break;
                                            case "SERVICE":
                                                builder.Append(CodeHelper.NEST.ServiceCreate(this.option, tableproperties));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "VUE.JS":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "FORM":
                                                builder.Append(CodeHelper.Vue.CreateEntityForm(this.option, tableproperties));
                                                break;
                                            case "COMPONENT":
                                                builder.Append(CodeHelper.Vue.CreateComponent(this.option, tableproperties));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "REACT.JS":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "FORM":
                                                builder.Append("");
                                                break;
                                            case "COMPONENT":
                                                builder.Append("");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "HTML":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "TAILWINDCSSFORM":
                                                builder.Append(CodeHelper.HTML.CreateEntity(this.option, tableproperties));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    default:
                                        builder.Append("언어탭을 선택해 주세요.");
                                        break;
                                }
                                break;
                            case "SP":
                                var sptables = rep.GetSPTables(this.option.target.name);
                                var spentities = rep.GetSpProperties(this.option.target.name);
                                var spoutput = rep.GetSpOutput(this.option.target.name);
                                switch (this.option.Language.Trim().ToUpper())
                                {
                                    case "ASP.NET":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append(CodeHelper.CSharp.CreateSPEntity(this.option, spentities, sptables, spoutput));
                                                break;
                                            case "CONTROLLER":
                                                builder.Append(CodeHelper.CSharp.CreateApiMethod(this.option, spentities, sptables, false));
                                                break;
                                            case "ABSTRACT":
                                                builder.Append(CodeHelper.CSharp.CreateSPInterface(this.option, spentities, sptables, spoutput));
                                                break;
                                            case "REPOSITORY":
                                                builder.Append(CodeHelper.CSharp.CreateSP(this.option, spentities, sptables, spoutput));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "JAVA":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append("");
                                                break;
                                            case "CONTROLLER":
                                                builder.Append("");
                                                break;
                                            case "ABSTRACT":
                                                builder.Append("");
                                                break;
                                            case "REPOSITORY":
                                                builder.Append("");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "DATABASE":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "MSSQL":
                                                builder.Append("해당사항없음.");
                                                break;
                                            case "MYSQL":
                                                builder.Append("해당사항없음.");
                                                break;
                                            case "MONGODB":
                                                builder.Append("해당사항없음.");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "TYPESCRIPT":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append("");
                                                break;
                                            case "CONTROLLER":
                                                builder.Append("");
                                                break;
                                            case "ABSTRACT":
                                                builder.Append("");
                                                break;
                                            case "REPOSITORY":
                                                builder.Append("");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "NODE.JS":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "ENTITY":
                                                builder.Append("");
                                                break;
                                            case "CONTROLLER":
                                                builder.Append(CodeHelper.JavaScript.NodeControllerMethodCreate(this.option, spentities, sptables));
                                                break;
                                            case "ABSTRACT":
                                                builder.Append("");
                                                break;
                                            case "REPOSITORY":
                                                builder.Append("");
                                                break;
                                            case "YAML":
                                                builder.Append(CodeHelper.YAML.CreateSP(this.option, spentities, sptables, spoutput));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "VUE.JS":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "FORM":
                                                builder.Append(CodeHelper.Vue.CreateSP(this.option,spentities,sptables,spoutput));
                                                break;
                                            case "COMPONENT":
                                                builder.Append(CodeHelper.Vue.ComponentSP(this.option, spentities,sptables,spoutput));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "REACT.JS":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "FORM":
                                                builder.Append("");
                                                break;
                                            case "COMPONENT":
                                                builder.Append("");
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    case "HTML":
                                        switch (this.option.Category.Trim().ToUpper())
                                        {
                                            case "TAILWINDCSSFORM":
                                                builder.Append(CodeHelper.HTML.CreateSP(this.option,spentities,sptables,spoutput));
                                                break;
                                            default:
                                                builder.Append("분류 탭을 선택해 주세요.");
                                                break;
                                        }
                                        break;
                                    default:
                                        builder.Append("언어탭을 선택해 주세요.");
                                        break;
                                }
                                break;
                            default:
                                builder.Append("대상을 선택해 주세요.");
                                break;
                        }
                    }
                }
                else
                {
                    builder.Append("대상을 선택해 주세요.");
                }
            }

            return builder.ToString();
        }

        public static string GetNameFromSP(string spname)
        {
            string result = string.Empty;

            if (!string.IsNullOrWhiteSpace(spname))
            {
                if (spname.IndexOf("_") > -1)
                {
                    string[] arr = spname.Split('_');
                    if (arr.Length > 1)
                    {
                        result = "";
                        for (int i = 1; i < arr.Length; i++)
                        {
                            result += arr[i];
                        }
                    }
                    else
                    {
                        result = spname.Replace("_", "");
                    }
                }
            }

            return result;
        }

        public static bool ContainEntitySaveSP(string str, out string entity)
        {
            entity = string.Empty;

            // 문자열이 "USP_"로 시작하고 "_Save"로 끝나는지 확인
            if (str.StartsWith("USP_") && str.EndsWith("_Save"))
            {
                // "USP_"와 "_Save" 사이의 문자열 추출
                int startIndex = "USP_".Length;
                int endIndex = str.Length - "_Save".Length - startIndex;

                entity = str.Substring(startIndex, endIndex);

                return true;
            }

            return false;
        }
    }

    public static class ExtendCodeHelper
    {
        public static void AppendTab(this StringBuilder builder, int tabCount, string str)
        {
            if (tabCount > 0)
            {
                for(int i = 0; i < tabCount; i++)
                {
                    builder.Append('\t');
                }
            }
            builder.Append(str);
        }

        public static void AppendTabLine(this StringBuilder builder, int tabCount, string str)
        {
            if (tabCount > 0)
            {
                for (int i = 0; i < tabCount; i++)
                {
                    builder.Append('\t');
                }
            }
            builder.AppendLine(str);
        }

        public static void AppendSpace(this StringBuilder builder, int tabCount, string str)
        {
            if (tabCount > 0)
            {
                for (int i = 0; i < tabCount; i++)
                {
                    builder.Append("  ");
                }
            }
            builder.Append(str);
        }

        public static void AppendSpaceLine(this StringBuilder builder, int tabCount, string str)
        {
            if (tabCount > 0)
            {
                for (int i = 0; i < tabCount; i++)
                {
                    builder.Append("  ");
                }
            }
            builder.AppendLine(str);
        }

        public static void AppendEmptyLine(this StringBuilder builder)
        {
            builder.AppendLine("");
        }

        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            {
                return input;
            }

            char[] inputArray = input.ToCharArray();
            inputArray[0] = char.ToLower(inputArray[0]);
            return new string(inputArray);
        }
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            {
                return input;
            }

            char[] inputArray = input.ToCharArray();
            inputArray[0] = char.ToUpper(inputArray[0]);
            return new string(inputArray);
        }

    }
}
