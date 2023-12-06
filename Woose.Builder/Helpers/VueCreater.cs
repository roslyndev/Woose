using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woose.Builder
{
    public class VueCreater
    {
        public VueCreater()
        {
        }

        public string CreateEntityForm(BindOption options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string mainTable = info[0].TableName;

                builder.AppendTabStringLine(0, $"<template>");
                builder.AppendTabStringLine(0, $"<Layout>");
                builder.AppendTabStringLine(0, $"<div class=\"bg-gray-100 flex items-center justify-center\">");
                builder.AppendTabStringLine(0, $"<div class=\"bg-white p-8 rounded-md shadow-md w-full max-w-3xl\">");
                builder.AppendTabStringLine(0, $"<h2 class=\"text-2xl font-semibold mb-4\">{mainTable}</h2>");
                builder.AppendTabStringLine(0, $"<form @submit.prevent=\"fnSave\">");
                foreach (var item in info)
                {
                    switch (item.ScriptType)
                    {
                        case "number":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"number\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(2, "<p class=\"text-red-500 text-sm\">{{ errors." + item.Name.FirstCharToLower() + " }}</p>");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                        case "Date":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"date\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(2, "<p class=\"text-red-500 text-sm\">{{ errors." + item.Name.FirstCharToLower() + " }}</p>");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                        case "string":
                            if (item.max_length == -1 || item.max_length >= 4000)
                            {
                                builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                                builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                                builder.AppendTabStringLine(2, $"<textarea id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")}></textarea>");
                                builder.AppendTabStringLine(2, "<p class=\"text-red-500 text-sm\">{{ errors." + item.Name.FirstCharToLower() + " }}</p>");
                                builder.AppendTabStringLine(1, $"</div>");
                            }
                            else
                            {
                                builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                                builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                                builder.AppendTabStringLine(2, $"<input type=\"text\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" maxlength=\"{item.max_length}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                                builder.AppendTabStringLine(2, "<p class=\"text-red-500 text-sm\">{{ errors." + item.Name.FirstCharToLower() + " }}</p>");
                                builder.AppendTabStringLine(1, $"</div>");
                            }
                            break;
                        case "boolean":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">");
                            builder.AppendTabStringLine(2, $"<input type=\"checkbox\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"border border-gray-300 rounded-md py-2 px-3\" />");
                            builder.AppendTabStringLine(2, $"{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(2, "<p class=\"text-red-500 text-sm\">{{ errors." + item.Name.FirstCharToLower() + " }}</p>");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                        case "object":
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"file\" id=\"{item.Name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(2, "<p class=\"text-red-500 text-sm\">{{ errors." + item.Name.FirstCharToLower() + " }}</p>");
                            builder.AppendTabStringLine(1, $"</div>");
                            break;
                    }
                }

                builder.AppendTabStringLine(1, $"<div class=\"w-full text-center\">");
                builder.AppendTabStringLine(2, $"<button type=\"submit\" class=\"bg-blue-500 text-white rounded-md py-2 px-4 hover:bg-blue-600 transition\">Submit</button>");
                builder.AppendTabStringLine(1, $"</div>");

                builder.AppendTabStringLine(0, "</form>");
                builder.AppendTabStringLine(0, $"</div>");
                builder.AppendTabStringLine(0, $"</div>");
                builder.AppendTabStringLine(0, $"</Layout>");
                builder.AppendTabStringLine(0, $"</template>");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(0, $"<script setup lang=\"ts\">");
                builder.AppendLine("import { onMounted,ref,computed,reactive } from 'vue';");
                if (options.Usei18n)
                {
                    builder.AppendLine("import { useI18n } from 'vue-i18n';");
                    builder.AppendLine("import { ApiHelper,MessageBox,DbMsg } from '../../models';");
                }
                else
                {
                    builder.AppendLine("import { ApiHelper,MessageBox } from '../../models';");
                }
                builder.Append("import { " + ((options.BindModel == OptionData.BindModelType.ExecuteResult.ToString()) ? "ApiResult,Member" : "ReturnValues,Member"));
                builder.AppendLine(((options.UsingCustomModel) ? "" : $",{mainTable}") + " } from '../../entities';");
                builder.AppendLine("import { Layout } from '../../components';");
                builder.AppendLine("import { useStore } from 'vuex';");
                builder.AppendLine("import config from '../../Config';");
                builder.AppendEmptyLine();
                if (options.Usei18n)
                {
                    builder.AppendLine("const { t, locale } = useI18n();");
                }
                builder.AppendLine("const store = useStore();");
                builder.AppendLine($"const userinfo = computed(() => store.getters.userinfo as Member);");
                builder.AppendEmptyLine();
                if (options.UsingCustomModel)
                {
                    builder.Append($"interface {mainTable} ");
                    builder.AppendLine("{");
                    int num1 = 0;
                    foreach (var item in info)
                    {
                        if (num1 > 0)
                        {
                            builder.Append(",");
                        }
                        builder.AppendTabString(1, "");
                        builder.AppendLine($"{item.Name.FirstCharToLower()}:{item.ScriptType}");
                        num1++;
                    }
                    builder.AppendLine("}");
                    builder.AppendEmptyLine();
                }

                if (options.UsingCustomModel)
                {
                    builder.AppendLine($"var {mainTable.FirstCharToLower()} = " + "ref({} as " + $"{mainTable});");
                }
                else
                {
                    builder.AppendLine($"var {mainTable.FirstCharToLower()} = ref(new {mainTable}());");
                }
                builder.AppendEmptyLine();

                builder.AppendLine("var errors = reactive({");
                int num2 = 0;
                foreach (var item in info.Where(x => x.is_nullable == false))
                {
                    if (num2 > 0)
                    {
                        builder.Append(",");
                    }
                    builder.AppendTabString(1, "");
                    builder.AppendLine($"{item.Name.FirstCharToLower()}:''");
                    num2++;
                }
                builder.AppendLine("});");
                builder.AppendEmptyLine();

                builder.AppendLine("const validateForm = () => {");
                foreach (var item in info.Where(x => x.is_nullable == false))
                {
                    builder.AppendTabStringLine(1, $"errors.{item.Name.FirstCharToLower()} = {mainTable.FirstCharToLower()}.value.{item.Name.FirstCharToLower()} ? '' : 'Requird {item.Name}'");
                }
                builder.AppendLine("};");
                builder.AppendEmptyLine();

                builder.AppendLine("const isFormValid = () => {");
                builder.AppendTabStringLine(1, "return !Object.values(errors).some((error) => error !== '');");
                builder.AppendLine("};");
                builder.AppendEmptyLine();

                builder.AppendLine("onMounted(async () => {");
                if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
                {
                    builder.AppendTabStringLine(1, $"let rst:ApiResult = await ApiHelper.Get({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/View\");");
                    builder.AppendTabStringLine(1, "if (rst.isSuccess) {");
                }
                else
                {
                    builder.AppendTabStringLine(1, $"let rst:ReturnValues = await ApiHelper.Get({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/View\");");
                    builder.AppendTabStringLine(1, "if (rst.check) {");
                }
                
                builder.AppendTabStringLine(2, $"{mainTable.FirstCharToLower()}.value = rst.data as {mainTable};");
                builder.AppendTabStringLine(1, "} else {");
                if (options.Usei18n)
                {
                    builder.AppendTabStringLine(2, "MessageBox.Alert(t(DbMsg(rst.message)));");
                }
                else
                {
                    builder.AppendTabStringLine(2, "MessageBox.Alert(t(DbMsg(rst.message)));");
                }
                builder.AppendTabStringLine(1, "}");
                builder.AppendLine("});");
                builder.AppendEmptyLine();

                builder.Append($"const fnSave = async () => ");
                builder.AppendLine("{");
                builder.AppendTabStringLine(1, "validateForm();");
                builder.AppendTabStringLine(1, "if (isFormValid()) {");
                builder.AppendTabStringLine(2, $"let jsonData:{mainTable} = " + "Object.assign({}, " + $"{mainTable.FirstCharToLower()}.value);");
                if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
                {
                    builder.AppendTabStringLine(2, $"let rst:ApiResult = await ApiHelper.Post({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/Save\", jsonData);");
                    builder.AppendTabStringLine(2, "if (rst.isSuccess) {");
                }
                else
                {
                    builder.AppendTabStringLine(2, $"let rst:ReturnValues = await ApiHelper.Post({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/Save\", jsonData);");
                    builder.AppendTabStringLine(2, "if (rst.check) {");
                }
                if (options.Usei18n)
                {
                    builder.AppendTabStringLine(3, "MessageBox.Success(t(DbMsg('Save')));");
                    builder.AppendTabStringLine(2, "} else {");
                    builder.AppendTabStringLine(3, "MessageBox.Alert(t(DbMsg(rst.message)));");
                }
                else
                {
                    builder.AppendTabStringLine(3, "MessageBox.Success('저장했습니다.');");
                    builder.AppendTabStringLine(2, "} else {");
                    builder.AppendTabStringLine(3, "MessageBox.Alert(rst.message);");
                }
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(1, "}");
                builder.AppendLine("};");
                builder.AppendTabStringLine(0, $"</script>");
            }

            return builder.ToString();
        }

        public string ComponentSP(BindOption options, List<SPEntity> sPEntities, List<SpTable> spTables, List<SpOutput> spOutputs)
        {
            return "";
        }

        public string CreateComponent(BindOption options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string mainTable = info[0].TableName;

                builder.AppendTabStringLine(0, "<template>");
                builder.AppendTabStringLine(0, $"<Layout title=\"{mainTable}\" :init=\"Initialize\">");
                builder.AppendTabStringLine(1, "<DataTable");
                builder.AppendTabStringLine(2, ":key=\"data.key\"");
                builder.AppendTabStringLine(2, ":data=\"{");
                builder.AppendTabStringLine(3, "items : data.items,");
                builder.AppendTabStringLine(3, "totalCount : data.count,");
                builder.AppendTabStringLine(3, "pageSize:10,");
                builder.AppendTabStringLine(3, "curPage:1");
                builder.AppendTabStringLine(2, "}\"");
                builder.AppendTabStringLine(2, ":option=\"{");
                builder.AppendTabStringLine(3, "isCheck : false,");
                builder.AppendTabStringLine(3, "columns : [");

                int num = 0;
                foreach (var item in info)
                {
                    builder.AppendTabString(4, "{ ");
                    builder.Append($"displayText : '{(string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description)}'");
                    builder.Append($", columnName : '{item.Name.FirstCharToLower()}'");
                    switch (item.ScriptType)
                    {
                        case "Date":
                            builder.Append(", dataType : 'date'");
                            break;
                        case "boolean":
                            builder.Append(", dataType : 'boolean'");
                            break;
                    }
                    //builder.Append(", foreign : { key:'code', display:'name', items : [{ code : 'input', name : '입고' },{ code : 'output', name : '출고' }] }");
                    builder.Append(" }");
                    if (num < info.Count() - 1)
                    {
                        builder.Append(",");
                    }
                    builder.AppendEmptyLine();
                    num++;
                }
                builder.AppendTabStringLine(3, "]");
                builder.AppendTabStringLine(2, "}\"");
                builder.AppendTabStringLine(2, ":events=\"{");
                builder.AppendTabStringLine(3, "getCheckList:(arr:[]) => { console.log(arr); },");
                builder.AppendTabStringLine(3, "rowClicked:(target:any) => { console.log(target); },");
                builder.AppendTabStringLine(3, "onsearch:(search:any) => { console.log(search); }");
                builder.AppendTabStringLine(2, "}\"");
                builder.AppendTabStringLine(1, ">");
                builder.AppendTabStringLine(1, "</DataTable>");
                builder.AppendTabStringLine(0, "</Layout>");
                builder.AppendTabStringLine(0, "</template>");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(0, "<script setup lang=\"ts\">");
                builder.AppendTabStringLine(0, "import { onMounted, ref } from 'vue';");
                builder.AppendTabStringLine(0, "import { Layout,DataTable } from '@/components';");
                builder.AppendTabStringLine(0, "import { ApiHelper,MessageBox } from '@/models';");
                builder.AppendTabStringLine(0, "import { ApiResult, " + mainTable + " } from '@/entities';");
                builder.AppendTabStringLine(0, "import config from '@/Config';");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(0, "var data = ref({");
                builder.AppendTabStringLine(1, $"items:[] as {mainTable}[],");
                builder.AppendTabStringLine(1, "count : 0,");
                builder.AppendTabStringLine(1, "key: 0");
                builder.AppendTabStringLine(0, "});");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(0, "const Initialize = async () => {");
                builder.AppendTabStringLine(1, "await fnDatabind();");
                builder.AppendTabStringLine(1, "data.value.key += 1;");
                builder.AppendTabStringLine(0, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(0, "const fnDatabind = async () => {");
                builder.AppendTabStringLine(1, $"let rst:{options.BindModelResult} = await ApiHelper.Get({((options.UseMultiApi) ? "config.apis.url + " : "")}`/api/{mainTable}/List`);");
                builder.AppendTabStringLine(1, $"if (rst.{options.BindModelIsBoolean})" + " {");
                builder.AppendTabStringLine(2, $"data.value.items = rst.data as {mainTable}[];");
                builder.AppendTabStringLine(2, $"data.value.count = rst.{options.BindModelCount};");
                builder.AppendTabStringLine(1, "} else {");
                builder.AppendTabStringLine(2, "MessageBox.Alert(rst.message);");
                builder.AppendTabStringLine(1, "}");
                builder.AppendTabStringLine(0, "}");
                builder.AppendTabStringLine(0, "</script>");
            }

            return builder.ToString();
        }

        public string CreateSP(BindOption options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
        {
            StringBuilder builder = new StringBuilder(200);

            string mainTable = tables[0].TableName;

            builder.AppendTabStringLine(0, $"<template>");
            builder.AppendTabStringLine(0, $"<Layout>");
            builder.AppendTabStringLine(0, $"<div class=\"bg-gray-100 flex items-center justify-center\">");
            builder.AppendTabStringLine(0, $"<div class=\"bg-white p-8 rounded-md shadow-md w-full max-w-3xl\">");
            builder.AppendTabStringLine(0, $"<h2 class=\"text-2xl font-semibold mb-4\">{mainTable}</h2>");
            builder.AppendTabStringLine(0, $"<form @submit.prevent=\"fnSave\">");
            foreach (var item in properties.Where(x => !x.is_output))
            {
                switch (item.ScriptType)
                {
                    case "number":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{item.Name}</label>");
                        builder.AppendTabStringLine(2, $"<input type=\"number\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                    case "Date":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{item.Name}</label>");
                        builder.AppendTabStringLine(2, $"<input type=\"date\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                    case "string":
                        if (item.max_length == -1 || item.max_length >= 4000)
                        {
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{item.Name}</label>");
                            builder.AppendTabStringLine(2, $"<textarea id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")}></textarea>");
                            builder.AppendTabStringLine(1, $"</div>");
                        }
                        else
                        {
                            builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                            builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{item.Name}</label>");
                            builder.AppendTabStringLine(2, $"<input type=\"text\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" maxlength=\"{item.max_length}\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                            builder.AppendTabStringLine(1, $"</div>");
                        }
                        break;
                    case "boolean":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">");
                        builder.AppendTabStringLine(2, $"<input type=\"checkbox\" id=\"{item.Name}\" v-model=\"{mainTable.FirstCharToLower()}.{item.Name.FirstCharToLower()}\" class=\"border border-gray-300 rounded-md py-2 px-3\" />");
                        builder.AppendTabStringLine(2, $"{item.Name}</label>");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                    case "object":
                        builder.AppendTabStringLine(1, $"<div class=\"mb-4\">");
                        builder.AppendTabStringLine(2, $"<label for=\"{item.Name}\" class=\"block text-gray-700\">{item.Name}</label>");
                        builder.AppendTabStringLine(2, $"<input type=\"file\" id=\"{item.Name}\" value=\"\" class=\"w-full border border-gray-300 rounded-md py-2 px-3\" {((item.is_nullable) ? "" : "required")} />");
                        builder.AppendTabStringLine(1, $"</div>");
                        break;
                }
            }

            builder.AppendTabStringLine(1, $"<div class=\"w-full text-center\">");
            builder.AppendTabStringLine(2, $"<button type=\"submit\" class=\"bg-blue-500 text-white rounded-md py-2 px-4 hover:bg-blue-600 transition\">Submit</button>");
            builder.AppendTabStringLine(1, $"</div>");

            builder.AppendTabStringLine(0, "</form>");
            builder.AppendTabStringLine(0, $"</div>");
            builder.AppendTabStringLine(0, $"</div>");
            builder.AppendTabStringLine(0, $"</Layout>");
            builder.AppendTabStringLine(0, $"</template>");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine(0, $"<script setup lang=\"ts\">");
            builder.AppendLine("import { onMounted,ref,computed,reactive } from 'vue';");
            if (options.Usei18n)
            { 
                builder.AppendLine("import { useI18n } from 'vue-i18n';");
                builder.AppendLine("import { ApiHelper,MessageBox,DbMsg } from '../../models';");
            }
            else
            {
                builder.AppendLine("import { ApiHelper,MessageBox } from '../../models';");
            }
            builder.Append("import { " + ((options.BindModel == OptionData.BindModelType.ExecuteResult.ToString()) ? "ApiResult,Member" : "ReturnValues,Member"));
            builder.AppendLine(((options.UsingCustomModel) ? "" : $",{mainTable}") + " } from '../../entities';");
            builder.AppendLine("import { Layout } from '../../components';");
            builder.AppendLine("import { useStore } from 'vuex';");
            builder.AppendLine("import config from '../../Config';");
            builder.AppendEmptyLine();
            if (options.Usei18n)
            {
                builder.AppendLine("const { t, locale } = useI18n();");
            }
            builder.AppendLine("const store = useStore();");
            builder.AppendLine($"const userinfo = computed(() => store.getters.userinfo as {((options.BindModel == OptionData.BindModelType.ExecuteResult.ToString()) ? "MemberInfo" : "Member")});");
            builder.AppendEmptyLine();
            if (options.UsingCustomModel)
            {
                builder.Append($"interface {mainTable} ");
                builder.AppendLine("{");
                int num1 = 0;
                foreach (var item in properties.Where(x => !x.is_output))
                {
                    if (num1 > 0)
                    {
                        builder.Append(",");
                    }
                    builder.AppendTabString(1, "");
                    builder.AppendLine($"{item.Name.FirstCharToLower()}:{item.ScriptType}");
                    num1++;
                }
                builder.AppendLine("}");
                builder.AppendEmptyLine();
            }

            if (options.UsingCustomModel)
            {
                builder.AppendLine($"var {mainTable.FirstCharToLower()} = " + "ref({} as " + $"{mainTable});");
            }
            else
            {
                builder.AppendLine($"var {mainTable.FirstCharToLower()} = ref(new {mainTable}());");
            }
            builder.AppendEmptyLine();

            builder.AppendLine("var errors = reactive({");
            int num2 = 0;
            foreach (var item in properties.Where(x => x.is_nullable == false && x.is_output == false))
            {
                if (num2 > 0)
                {
                    builder.Append(",");
                }
                builder.AppendTabString(1, "");
                builder.AppendLine($"{item.Name.FirstCharToLower()}:''");
                num2++;
            }
            builder.AppendLine("});");
            builder.AppendEmptyLine();

            builder.AppendLine("const validateForm = () => {");
            foreach (var item in properties.Where(x => x.is_nullable == false && x.is_output == false))
            {
                builder.AppendTabStringLine(1, $"errors.{item.Name.FirstCharToLower()} = {mainTable.FirstCharToLower()}.value.{item.Name.FirstCharToLower()} ? '' : 'Requird {item.Name}'");
            }
            builder.AppendLine("};");
            builder.AppendEmptyLine();

            builder.AppendLine("const isFormValid = () => {");
            builder.AppendTabString(1, "return !Object.values(errors).some((error) => error !== '');");
            builder.AppendLine("};");
            builder.AppendEmptyLine();

            builder.AppendLine("onMounted(async () => {");
            if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
            {
                builder.AppendTabStringLine(1, $"let rst:ApiResult = await ApiHelper.Get({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/View\");");
                builder.AppendTabStringLine(1, "if (rst.isSuccess) {");
            }
            else
            {
                builder.AppendTabStringLine(1, $"let rst:ReturnValues = await ApiHelper.Get({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/View\");");
                builder.AppendTabStringLine(1, "if (rst.check) {");
            }
            
            
            builder.AppendTabStringLine(2, $"{mainTable.FirstCharToLower()}.value = rst.data as {mainTable};");
            builder.AppendTabStringLine(1, "} else {");
            if (options.Usei18n)
            {
                builder.AppendTabStringLine(2, "MessageBox.Alert(t(DbMsg(rst.message)));");
            }
            else
            {
                builder.AppendTabStringLine(2, "MessageBox.Alert(rst.message);");
            }
            builder.AppendTabStringLine(1, "}");
            builder.AppendLine("});");
            builder.AppendEmptyLine();

            builder.Append($"const fnSave = async () => ");
            builder.AppendLine("{");
            builder.AppendTabStringLine(1, "validateForm();");
            builder.AppendTabStringLine(1, "if (isFormValid()) {");
            builder.AppendTabStringLine(2, $"let jsonData:{mainTable} = " + "Object.assign({}, " + $"{mainTable.FirstCharToLower()}.value);");
            if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
            {
                builder.AppendTabStringLine(2, $"let rst:ApiResult = await ApiHelper.Post({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/Save\", jsonData);");
                builder.AppendTabStringLine(2, "if (rst.isSuccess) {");
            }
            else
            {
                builder.AppendTabStringLine(2, $"let rst:ReturnValues = await ApiHelper.Post({((options.UseMultiApi) ? "config.apis.url + " : "")}\"/api/{mainTable}/Save\", jsonData);");
                builder.AppendTabStringLine(2, "if (rst.check) {");
            }
            if (options.Usei18n)
            {
                builder.AppendTabStringLine(3, "MessageBox.Success(t(DbMsg('Save')));");
                builder.AppendTabStringLine(2, "} else {");
                builder.AppendTabStringLine(3, "MessageBox.Alert(t(DbMsg(rst.message)));");
            }
            else
            {
                builder.AppendTabStringLine(3, "MessageBox.Success('저장했습니다.');");
                builder.AppendTabStringLine(2, "} else {");
                builder.AppendTabStringLine(3, "MessageBox.Alert(rst.message);");
            }
            builder.AppendTabStringLine(2, "}");
            builder.AppendTabStringLine(1, "}");
            builder.AppendLine("};");
            builder.AppendTabStringLine(0, $"</script>");
            

            return builder.ToString();
        }
    }
}
