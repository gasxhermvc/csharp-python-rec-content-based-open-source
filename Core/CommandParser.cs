using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendSystemContentBased.Core
{
    public class CommandParser
    {
        private static Dictionary<string, object> arguments = new Dictionary<string, object>();

        public static void SetArguments(params string[] args)
        {
            CommandParser.arguments = CommandParser.Parser(args);
        }

        public static void SetArgument(string key, object value, bool notExists = true)
        {
            if (CommandParser.arguments.ContainsKey(key))
            {
                if (notExists)
                {
                    CommandParser.arguments[key] = value;
                }
            }
            else
            {
                CommandParser.arguments.Add(key, value);
            }
        }

        public static T GetValue<T>(string argumentName) where T : class
        {
            object value;

            CommandParser.arguments.TryGetValue(argumentName, out value);

            if (value == null)
            {
                try
                {
                    return default(T);
                }
                catch
                {
                    return null;
                }
            }

            try
            {
                return (T)value;

            }
            catch
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }


        public static bool GetValue(string argumentName)
        {
            object value;

            CommandParser.arguments.TryGetValue(argumentName, out value);

            if (value == null)
            {
                return false;
            }

            try
            {
                return (bool)value;

            }
            catch
            {
                return (bool)Convert.ChangeType(value, typeof(bool));
            }
        }

        private static Dictionary<string, object> Parser(string[] args)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();


            var argumentsSplits = string.Join("|", args).Split('|').Where(w => !string.IsNullOrEmpty(w)).ToList();

            if (argumentsSplits == null || argumentsSplits.Count < 1)
            {
                return arguments;
            }


            argumentsSplits.ForEach(argument =>
            {
                var padding = 2;
                var index = 1;
                string key = string.Empty;

                var argumentSplit = argument.Split('=');

                foreach (var arg in argumentSplit)
                {
                    if (index == padding)
                    {
                        arguments.Add(key, (object)arg);
                        key = string.Empty;
                        index = 1;
                        continue;
                    }

                    key = arg.Replace("--", string.Empty).Replace("=", string.Empty).Replace(" ", string.Empty).Trim();

                    index++;
                }
            });

            return arguments;
        }
    }
}
