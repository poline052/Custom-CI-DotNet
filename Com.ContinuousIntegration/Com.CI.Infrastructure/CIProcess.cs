using System;
using System.Diagnostics;

namespace Com.CI.Infrastructure
{
    public class CIProcess : ICIProcess
    {

        private readonly ICILogger ciLogger;
        private readonly ISignalRClient signalRClient;
        public CIProcess(ICILogger ciLogger, ISignalRClient signalRClient)
        {
            this.ciLogger = ciLogger;
            this.signalRClient = signalRClient;
        }

        public int Execute(string executablePath, string arguments, string repositoryId, string branchId)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = arguments
            };

            try
            {
                using (Process process = new Process() { StartInfo = startInfo })
                {
                    if (!process.Start())
                    {
                        ciLogger.WriteError($"Unable to start {executablePath}", new InvalidProgramException($"Program {executablePath} is unable to start"));
                    }
                    else
                    {
                        process.OutputDataReceived += async (object sender, DataReceivedEventArgs e) =>
                        {
                            var ciMessage = CIMessage.Create(repositoryId, branchId, e.Data);

                            await signalRClient.PublishAsync(ciMessage);

                        };

                        process.BeginOutputReadLine();

                        process.ErrorDataReceived += async (object sender, DataReceivedEventArgs e) =>
                        {
                            var ciMessage = CIMessage.Create(repositoryId, branchId, e.Data, true);

                            await signalRClient.PublishAsync(ciMessage);
                        };

                        process.BeginErrorReadLine();

                        process.WaitForExit();

                        Console.WriteLine("ExitCode: {0}", process.ExitCode);

                        return process.ExitCode;
                    }
                }


            }
            catch (Exception e)
            {
                ciLogger.WriteError(e.Message, e);
            }

            return 1;
        }
    }
}
