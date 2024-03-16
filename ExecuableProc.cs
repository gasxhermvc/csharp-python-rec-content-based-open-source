using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendSystemContentBased
{
    public class ExecuableProc
    {
        private string executableTarget { get; set; }

        public ExecuableProc(string executableTarget)
        {
            this.executableTarget = executableTarget;
        }

        public ProcResult Proccess(string commandLine)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = new ProcResult();

            using (Process proc = new Process())
            {
                try
                {
                    proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                    proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                    proc.StartInfo.FileName = this.executableTarget;
                    //proc.StartInfo.FileName = @"python";

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.Arguments = commandLine;
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                    using (proc)
                    {
                        using (StreamReader reader = proc.StandardOutput)
                        {
                            result.response = reader.ReadToEnd();
                        }

                        proc.Close();
                    }

                    result.success = true;
                }
                catch (Exception e)
                {
                    result.response = string.Empty;
                    result.exception = e;
                    result.success = false;
                }
                finally
                {
                    if (proc != null)
                    {
                        proc.Close();
                        proc.Dispose();
                    }
                }
            }

            return result;
        }

        public class ProcResult
        {
            public string response { get; set; }
            public bool success { get; set; }
            public Exception exception { get; set; }

            public TResult ToObject<TResult>() where TResult : class
            {
                return JsonConvert.DeserializeObject<TResult>(this.response);
            }
        }
    }
}
