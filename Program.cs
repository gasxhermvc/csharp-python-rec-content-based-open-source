using RecommendSystemContentBased.Core;
using RecommendSystemContentBased.Models.TaiplaModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RecommendSystemContentBased
{
    class Program
    {
        static void Main(string[] args)
        {
            SetDateEnv();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            CommandParser.SetArguments(args);
            //=>defined default
            CommandParser.SetArgument("EnvironmentVariable", "Development", false);
            CommandParser.SetArgument("Debug", true, false);

            string Env = CommandParser.GetValue<string>("EnvironmentVariable");

            Ext.ConsoleTraceWriter("Application start...");
            Ext.ConsoleTraceWriter("Environment name : {0}", Env);

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            string workSpaceInput = string.Format("{0}/Input/Foods/",
                AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/")
                .Replace("//", "/").TrimEnd('/'));

            Ext.ConsoleTraceWriter("Input workspace : {0}", workSpaceInput);

            if (!System.IO.Directory.Exists(workSpaceInput))
            {
                System.IO.Directory.CreateDirectory(workSpaceInput);
            }

            string workSpaceOut = string.Format("{0}/Output/Foods/",
                AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/")
                .Replace("//", "/").TrimEnd('/'));

            Ext.ConsoleTraceWriter("Output workspace : {0}", workSpaceOut);

            if (!System.IO.Directory.Exists(workSpaceOut))
            {
                System.IO.Directory.CreateDirectory(workSpaceOut);
            }

            var foodCountries = GetFoodCountry();
            Ext.ConsoleTraceWriter("Country count : {0}", foodCountries.Count);
            var foodCultures = GetFoodCulture();
            Ext.ConsoleTraceWriter("Culture count : {0}", foodCultures.Count);
            var foods = GetFoods();
            Ext.ConsoleTraceWriter("Food count : {0}", foods.Count);

            if (foods.Count < 1)
            {
                Ext.ConsoleTraceWriter("Foods data not found.");
                Environment.Exit(0);
            }

            var dataExtract = extractFoodData(foodCountries, foodCultures, foods);

            string fileName = string.Format("{0}/{1}",
                workSpaceInput.TrimEnd('/'),
                "food_center.csv");

            Ext.ConsoleTraceWriter("Write csv : {0}", fileName);
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }

            System.IO.File.WriteAllLines(fileName, dataExtract, System.Text.Encoding.UTF8);

            Ext.ConsoleTraceWriter("done.");

            var top = ConfigurationManager.AppSettings["TOP"].ToString();
            foods.ForEach(food =>
            {
                try
                {

                    var culture = foodCultures.FirstOrDefault(f => f.country_id == food.country_id &&
                        f.culture_id == food.culture_id);

                    //=>ปัจจุบัน
                    var active = food.food_id;
                    var culture_name = culture.name_th;

                    Ext.ConsoleTraceWriter("Get recommendation FOOD_ID : {0}", active);
                    Ext.ConsoleTraceWriter("Culture name : {0}", culture_name);

                    string readInput = fileName;
                    string writeOutput = string.Format($"{workSpaceOut}/[foodId].json");

                    //=>{1} คือ ตำแหน่งไฟล์ Input ที่จะต้องไปอ่าน
                    //=>{2} คือ รหัสอาหารที่เป็นรายการ Active
                    //=>{3} คือ ประเภทของอาหาร
                    //=>{4} คือ จำนวน Top ของรายการแนะนำ
                    string commandLine = string.Format("{0}/recommendation.py {1} {2} {3} {4}",
                         AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/")
                            .Replace("//", "/").TrimEnd('/'),
                         readInput,
                         active,
                         culture_name,
                         top);

                    Ext.ConsoleTraceWriter("Command: {0}", commandLine);

                    var pythonExe = ConfigurationManager.AppSettings["pythonExeutable"].ToString();
                    ExecuableProc execute = new ExecuableProc(pythonExe);
                    var result = execute.Proccess(commandLine);
                    if (result.success)
                    {
                        string output = writeOutput.Replace("[foodId]", active.ToString());

                        result.response = result.response.Replace(Environment.NewLine, string.Empty);
                        Ext.ConsoleTraceWriter("Rec result : {0}", result.response);
                        Ext.ConsoleTraceWriter("Write result : {0}", result.response);

                        if (System.IO.File.Exists(output))
                        {
                            System.IO.File.Delete(output);
                        }

                        System.IO.File.WriteAllText(output, result.response.Trim().Trim('\n').Trim('\r'), System.Text.Encoding.UTF8);
                        Ext.ConsoleTraceWriter("done.");
                    }
                    else
                    {
                        Ext.ConsoleErrorWriter("Rec.Error : {0}", result.exception.ToString());
                    }
                }
                catch (Exception e)
                {
                    Ext.ConsoleErrorWriter("Exception : {0}", e.ToString());
                }
            });

            Ext.ConsoleErrorWriter("Application stop...");
            Ext.ConsoleErrorWriter("Timmer: {0}ms.", sw.Elapsed.ToString());

        }
        public static List<food_center> GetFoods()
        {
            List<food_center> foods = new List<food_center>();
            using (var context = new taipla_wsEntities())
            {
                try
                {
                    var rows = (from o in context.food_center
                                where o.user_id != 1
                                select o).ToList();

                    if (rows != null && rows.Count > 0)
                    {
                        foods = rows;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetFoods.Exception : {0}", e.ToString());
                }
            }

            return foods;
        }

        public static List<food_country> GetFoodCountry()
        {
            List<food_country> country = new List<food_country>();
            using (var context = new taipla_wsEntities())
            {
                try
                {
                    var rows = (from o in context.food_country
                                select o).ToList();

                    if (rows != null && rows.Count > 0)
                    {
                        country = rows;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetFoodCountry.Exception : {0}", e.ToString());
                }
            }

            return country;
        }

        public static List<food_culture> GetFoodCulture()
        {
            List<food_culture> cultures = new List<food_culture>();
            using (var context = new taipla_wsEntities())
            {
                try
                {
                    var rows = (from o in context.food_culture
                                select o).ToList();

                    if (rows != null && rows.Count > 0)
                    {
                        cultures = rows;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetFoodCulture.Exception : {0}", e.ToString());
                }
            }

            return cultures;
        }

        public static List<string> extractFoodData(List<food_country> countries, List<food_culture> cultures, List<food_center> foods)
        {
            List<string> lines = new List<string>();

            //id,country_id,country_name,culture_id,culture_name,name_th,description,cooking_food,dietetic_food,ingredient
            var foodData = (from food in foods
                            let country = countries.FirstOrDefault(f => f.country_id == food.country_id)
                            let culture = cultures.FirstOrDefault(f => f.country_id == food.country_id &&
                                f.culture_id == food.culture_id)
                            where country != null && culture != null
                            select string.Concat(
                                food.food_id,
                                "|",
                                country.country_id,
                                "|",
                                country.name_th,
                                "|",
                                culture.country_id,
                                "|",
                                culture.name_th,
                                "|",
                                food.name_th,
                                "|",
                                HttpUtility.HtmlDecode(food.description?.Trim() ?? string.Empty).Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty),
                                "|",
                                DeepReplace(HttpUtility.HtmlDecode(food.cooking_food?.Trim() ?? string.Empty).Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty)),
                                "|",
                                DeepReplace(HttpUtility.HtmlDecode(food.dietetic_food?.Trim() ?? string.Empty).Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty)),
                                "|",
                                DeepReplace(HttpUtility.HtmlDecode(food.ingredient?.Trim() ?? string.Empty).Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty)))).ToList();

            lines.Add("id|country_id|country_name|culture_id|culture_name|name|description|cooking_food|dietetic_food|ingredient");
            lines.AddRange(foodData);

            return lines;
        }

        public static string DeepReplace(string value)
        {
            return Regex.Replace(value, @"[0-9]|\!|\@|\#|\$|\%|\^|\&|\*|\(|\)|\.|\/|วิธีทำ|ช้อนโต๊ะ|ช้อนชา|ซต.|ชต.|\t|\t\t|\s+", "", RegexOptions.IgnoreCase);
        }

        public static void SetDateEnv()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            CultureInfo info = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            info.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd HH:mm:ss";
            info.DateTimeFormat.LongTimePattern = "";
            Thread.CurrentThread.CurrentCulture = info;
        }
    }
}
