using System;
using System.Text;
using System.Threading;

namespace SSHTestCode
{
    class Program
    {
        static SshHelper ssh;

        static void Main(string[] args)
        {
            ssh = new SshHelper();

            try
            {
                Console.WriteLine("=== SSH Tool Test Program (Optimized) ===\n");

                // Connect
                if (!Connect("192.168.119.163", 22, "yz", "yz123"))
                {
                    return;
                }

                // Navigate to Downloads
                ExecuteCommand("cd ~/Downloads", "Navigate to Downloads");

                // Test 1: Create text file
                ExecuteCommand("echo 'Hello from SSH Test' > test_ssh.txt", "Create text file");

                // Test 2: Read file content
                ExecuteAndShowOutput("cat test_ssh.txt", "Read file content");

                // Test 3: Create shell script
                Console.WriteLine("\n[Test 3: Create shell script]");
                ExecuteCommand("echo '#!/bin/bash' > test_ssh_script.sh", null);
                ExecuteCommand("echo 'echo Script executed successfully' >> test_ssh_script.sh", null);
                ExecuteCommand("echo 'date' >> test_ssh_script.sh", null);
                ExecuteCommand("chmod +x test_ssh_script.sh", null);
                Console.WriteLine("Script created and made executable");

                // Test 4: Execute script
                ExecuteAndShowOutput("./test_ssh_script.sh", "Execute script");

                // Test 5: List files
                ExecuteAndShowOutput("ls -lh test_ssh*", "List test files");

                // Test 6: Copy file
                ExecuteCommand("cp test_ssh.txt test_ssh_backup.txt", "Copy file");
                ExecuteAndShowOutput("ls -lh test_ssh*", "Verify copy");

                // Test 7: Update file (append)
                ExecuteCommand("echo 'Additional line' >> test_ssh.txt", "Append to file");
                ExecuteAndShowOutput("cat test_ssh.txt", "Show updated content");

                // Test 8: Search files
                ExecuteAndShowOutput("find . -name 'test_ssh*' -type f", "Search for test files");

                // Test 9: File size
                ExecuteAndShowOutput("du -h test_ssh.txt", "Check file size");

                // Test 10: Count lines
                ExecuteAndShowOutput("wc -l test_ssh.txt", "Count lines");

                // Test 11: Cleanup
                ExecuteCommand("rm -f test_ssh.txt test_ssh_backup.txt test_ssh_script.sh", "Delete test files");
                ExecuteAndShowOutput("ls -lh test_ssh* 2>&1", "Verify deletion");

                Console.WriteLine("\n=== All tests completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                Cleanup();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static bool Connect(string host, int port, string username, string password)
        {
            Console.WriteLine("Connecting to " + host + "...");
            ssh.Init(host, port, username, password);

            if (!ssh.OpenSerialPort())
            {
                Console.WriteLine("Failed to connect");
                return false;
            }

            Console.WriteLine("Connected successfully\n");
            Thread.Sleep(2000);
            ssh.ReadMsg(2000);
            return true;
        }

        static void ExecuteCommand(string command, string description)
        {
            if (description != null)
            {
                Console.WriteLine("\n[" + description + "]");
            }

            ssh.SetMsg(command + "\n");
            Thread.Sleep(500);
            string output = ssh.PollingReadMsg(10, 100, "Downloads");

            if (output.Contains("command not found") || output.Contains("No such file"))
            {
                Console.WriteLine("Error: " + output);
            }
        }

        static void ExecuteAndShowOutput(string command, string description)
        {
            Console.WriteLine("\n[" + description + "]");
            ssh.SetMsg(command + "\n");
            Thread.Sleep(500);

            string output = ssh.PollingReadMsg(10, 100, "Downloads");
            string cleanOutput = CleanOutput(output, command);

            Console.WriteLine(cleanOutput);
        }

        static string CleanOutput(string output, string command)
        {
            if (string.IsNullOrEmpty(output))
            {
                return "(no output)";
            }

            // Remove command echo and prompt
            string[] lines = output.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            StringBuilder result = new StringBuilder();

            bool skipFirst = true;
            foreach (string line in lines)
            {
                if (skipFirst && line.Contains(command))
                {
                    skipFirst = false;
                    continue;
                }

                if (line.Contains("yz@ubuntu") || line.Contains("Downloads$"))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    result.AppendLine(line);
                }
            }

            string cleaned = result.ToString().Trim();
            return string.IsNullOrEmpty(cleaned) ? "(no output)" : cleaned;
        }

        static void Cleanup()
        {
            if (ssh != null && ssh.IsOpen())
            {
                ssh.CloseSerialPort();
                Console.WriteLine("\nSSH connection closed");
            }

            if (ssh != null)
            {
                ssh.Dispose();
            }
        }
    }
}
