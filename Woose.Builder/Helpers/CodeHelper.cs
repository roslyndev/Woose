﻿using System;
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
                    switch (this.option.Language.Trim().ToUpper())
                    {
                        case "ASP.NET":
                            switch (this.option.Category.Trim().ToUpper())
                            {
                                case "ENTITY":
                                    switch (this.option.targetType)
                                    {
                                        case "TABLE":
                                            using(var rep = new SqlServerRepository(context))
                                            {
                                                var list = rep.GetTableProperties(this.option.target.name);
                                                builder.Append(CSharpCreater.CreateEntity(this.option, list));
                                            }
                                            break;
                                        case "SP":
                                            builder.Append(this.option.target.name);
                                            break;
                                        default:
                                            builder.Append("대상을 선택해 주세요.");
                                            break;
                                    }
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
                }
                else
                {
                    builder.Append("대상을 선택해 주세요.");
                }
            }

            return builder.ToString();
        }
    }

    public static class ExtendCodeHelper
    {
        public static void AppendTabString(this StringBuilder builder, int tabCount, string str)
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

        public static void AppendTabStringLine(this StringBuilder builder, int tabCount, string str)
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

    }
}
