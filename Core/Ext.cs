using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RecommendSystemContentBased.Core
{
    public static class Ext
    {
        public static string BasePath(params string[] paths)
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/"), string.Join("/", paths).Trim('/')).Trim('/');
        }

        public static Dictionary<string, string> ObjectToDictionary<T>(this T obj)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            var props = obj.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                var value = prop.GetValue(obj, null);

                if (value != null)
                {
                    try
                    {
                        dict.Add(prop.Name, value.ToString());
                    }
                    catch (Exception e)
                    {
                        Type type = value.GetType();
                        var defaultValue = type.GetMethod("GetDefaultGeneric")
                            .Invoke(value, null);
                        dict.Add(prop.Name, defaultValue.ToString());
                    }
                }
                else
                {
                    dict.Add(prop.Name, string.Empty);
                }
            }

            return dict;
        }

        public static double ConvertDouble(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return 0;
            }

            try
            {
                return double.Parse(txt);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public static string FormatNumber(string txt, string format)
        {
            return Ext.ConvertDouble(txt).ToString(format);
        }

        public static void ConsoleTraceWriter(string format, params object[] obj)
        {
            bool debug = CommandParser.GetValue("Debug");
            if (debug)
            {
                Console.Write($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} TRACE ");
                Console.WriteLine(format, obj);
            }
        }

        public static void ConsoleErrorWriter(string format, params object[] obj)
        {
            bool debug = CommandParser.GetValue("Debug");
            if (debug)
            {
                Console.Write($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ERROR ");
                Console.WriteLine(format, obj);
            }
        }
    }

}
