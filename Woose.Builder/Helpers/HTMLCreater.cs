using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Woose.Builder
{
    public class HTMLCreater
    {
        public HTMLCreater()
        {
        }

        public string CreateEntity(OptionData options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string mainTable = info[0].TableName.Trim().FirstCharToLower();

                builder.AppendTabStringLine(0, $"<form>");
                foreach (var item in info)
                {
                    switch (item.ScriptType)
                    {
                        case "number":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"number\" id=\"{item.Name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                        case "Date":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"date\" id=\"{item.Name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                        case "string":
                            if (item.ColumnMaxLength == -1 || item.ColumnMaxLength > 4000)
                            {
                                builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                                builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                                builder.AppendTabStringLine(2, $"<textarea id=\"{item.Name}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                                builder.AppendTabStringLine(1, $"</div>");
                            }
                            else
                            {
                                builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                                builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                                builder.AppendTabStringLine(2, $"<input type=\"text\" id=\"{item.Name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                                builder.AppendTabStringLine(1, $"</div>");
                            }
                            break;
                        case "boolean":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">");
                            builder.AppendTabStringLine(2, $"<input type=\"checkbox\" id=\"{item.Name}\" value=\"true\" class=\"border border-gray-300 rounded-md py-2 px-3\" />");
                            builder.AppendTabStringLine(2, $"{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                        case "object":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"file\" id=\"{item.Name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                    }
                    builder.AppendTabStringLine(1, $"<button type=\"submit\" class=\"bg-blue-500 text-white rounded-md py-2 px-4 hover:bg-blue-600 transition\">Submit</button>");
                }
                builder.AppendTabStringLine(0, "</form>");
            }

            return builder.ToString();
        }

        public string CreateSP(OptionData options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
        {
            StringBuilder builder = new StringBuilder(200);
            string mainTable = string.Empty;

            if (tables != null && tables.Count > 0)
            {
                mainTable = tables[0].TableName.FirstCharToLower();
            }

            builder.AppendTabStringLine(0, $"<form>");
            foreach (var item in properties)
            {
                switch (item.ScriptType)
                {
                    case "number":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.name}\" class=\"block text-gray-700\">{item.name}</label>");
                        builder.AppendTabStringLine(2, $"<input type=\"number\" id=\"{item.name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                    case "Date":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.name}\" class=\"block text-gray-700\">{item.name}</label>");
                        builder.AppendTabStringLine(2, $"<input type=\"date\" id=\"{item.name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                    case "string":
                        if (item.max_length == -1 || item.max_length > 4000)
                        {
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.name}\" class=\"block text-gray-700\">{item.name}</label>");
                            builder.AppendTabStringLine(2, $"<textarea id=\"{item.name}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(1, $"</div>");
                        }
                        else
                        {
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.name}\" class=\"block text-gray-700\">{item.name}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"text\" id=\"{item.name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(1, $"</div>");
                        }
                        break;
                    case "boolean":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.name}\" class=\"block text-gray-700\">");
                        builder.AppendTabStringLine(2, $"<input type=\"checkbox\" id=\"{item.name}\" value=\"true\" class=\"border border-gray-300 rounded-md py-2 px-3\" />");
                        builder.AppendTabStringLine(2, $"{item.name}</label>");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                    case "object":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.name}\" class=\"block text-gray-700\">{item.name}</label>");
                        builder.AppendTabStringLine(2, $"<input type=\"file\" id=\"{item.name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                }
                builder.AppendTabStringLine(1, $"<button type=\"submit\" class=\"bg-blue-500 text-white rounded-md py-2 px-4 hover:bg-blue-600 transition\">Submit</button>");
            }
            builder.AppendTabStringLine(0, "</form>");
        

            return builder.ToString();
        }
    }
}
