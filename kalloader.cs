using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Text;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

#nullable enable
internal class Program
{
    private const string GitHubBaseUrl = "https://github.com/loxchmorez/brutalpc-kall-kodein/";
    private const string GitHubScriptName = "draw1.sh";
    private const string GitHubHWIDListUrl = "https://raw.githubusercontent.com/TwikCheat/PIDORBRUTALPIDORPC/refs/heads/main/users.txt";
    private const int HWIDAuthSwitch = 7979;
    private const string AdbExeName = "adb.exe";
    private const string AdbApiDllName = "AdbWinApi.dll";
    private const string BluestacksAddress = "127.0.0.1:5555";
    private const string RemotePath = "/data/local/tmp/";
    private const string SuCommandBase = "/boot/android/dataFS/downloads/.xb/su -c";
    private static string? _userHWID = null;
    private static bool _isHWIDValid = false;
    private static Dictionary<string, DateTime> _validHWIDsWithDates = new Dictionary<string, DateTime>();

    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Инициализация HWID...");
        Program._userHWID = Program.GetSystemHWID();
        Console.WriteLine("Ваш HWID: ZOV666ZOV777SKIBIDI");
        Program._validHWIDsWithDates = await Program.LoadValidHWIDsAsync();

        // тут кстати был код загрузки хвидов с "сервера"
        // бля ты че ваще баклан?
        // допустим твою дрисню купит 100 человек
        // у тебя эти 100 строк будет парсить 1000 лет

        Console.ForegroundColor = (ConsoleColor)12;
        Console.WriteLine("Ваш HWID не найден в базе данных.");
        Console.WriteLine("Чит декомпилирован и в7л0m4н t.me/clawclouds $$$");
        Console.WriteLine("Ебаная нищая паста");
        Console.ResetColor();
        Console.WriteLine("\nНажмите любую клавишу для того чтобы загрузить пасту в игру...");
        Console.ReadKey();

        Console.WriteLine("Инициализация чит-утилиты... (totally not chatgpt moment)");
        Console.WriteLine("Пожалуйста, подождите, идет загрузка вспомогательных файлов... (с github 😊👌)");
        List<string> filesToCleanUp = new List<string>();
        int exitCode = 0;
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string tempPath = Path.GetTempPath();
        string localAdbPath = Path.Combine(baseDirectory, "adb.exe");
        string adbApiDllPath = Path.Combine(baseDirectory, "AdbWinApi.dll");
        Console.WriteLine("Пока говночит грузится, угарните с этих двух строк:\n" +
            "        string localAdbPath = Path.Combine(baseDirectory, \"adb.exe\");\r\n        string adbApiDllPath = Path.Combine(baseDirectory, \"AdbWinApi.dll\");");
        Console.WriteLine("Причем у типа блять для этого константы есть. Нахуя? Непонятно");
        string randomScriptFileName = Program.GenerateRandomString(5, 10);
        string localScriptPath = Path.Combine(tempPath, randomScriptFileName);

        Dictionary<string, string> filesToDownload = new Dictionary<string, string>
        {
            { "adb.exe", localAdbPath },
            { "AdbWinApi.dll", adbApiDllPath }
        };

        try
        {
            int loginValue = 7356;

            // Удаляем старые файлы
            // это делал не гпт, мне просто приспичило с нихуя въебать коммент
            foreach (KeyValuePair<string, string> kv in filesToDownload)
            {
                string targetPath = kv.Value;
                if (File.Exists(targetPath))
                {
                    try
                    {
                        File.Delete(targetPath);
                    }
                    catch
                    {
                    }
                }
            }

            // Скачиваем свежие
            foreach (KeyValuePair<string, string> kv in filesToDownload)
            {
                string fileName = kv.Key;
                string url = GitHubBaseUrl + fileName;
                string targetPath = kv.Value;

                try
                {
                    await Program.DownloadFileAsync(url, targetPath);
                    filesToCleanUp.Add(targetPath);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = (ConsoleColor)12;
                    Console.WriteLine($"[!!!] Не удалось скачать критически важный файл {fileName}: {ex.Message}");
                    Console.WriteLine($"ну ты далбаеб просто, не достоин с этим божеством играть");
                    Console.ResetColor();
                    exitCode = 2;
                    break;
                }
            }

            if (exitCode == 0) // no comms. бля, что за калл?
            {
                Console.ForegroundColor = (ConsoleColor)10;
                Console.WriteLine("Вспомогательные файлы ADB успешно скачаны.");
                Console.ResetColor();

                if (!File.Exists(localAdbPath))
                {
                    Console.ForegroundColor = (ConsoleColor)12;
                    Console.WriteLine($"[!!!] adb.exe не найден по пути: {localAdbPath}. Скачивание, возможно, не удалось.");
                    Console.ResetColor();
                    exitCode = 3;
                }
            }

            if (exitCode == 0)
            {
                string remoteScriptPath = RemotePath + randomScriptFileName;
                Program.ProcResult procResult = Program.ExecuteCommandWithResult(localAdbPath, $"connect {BluestacksAddress}", 15000);

                if (!(procResult.StdOut.Contains("connected to", StringComparison.OrdinalIgnoreCase)
                      || procResult.StdOut.Contains("already connected to", StringComparison.OrdinalIgnoreCase))) // охуенный парс, болеее пиздатого способа не придумаешь (ну например экзит коды, хззз)
                {
                    Console.ForegroundColor = (ConsoleColor)12;
                    Console.WriteLine("\n[!!!] Ошибка подключения к ADB.");
                    Console.WriteLine("Пожалуйста, запустите игру/эмулятор (Bluestacks) или включите ADB.");
                    Console.ResetColor();
                    exitCode = 4;
                }
                else
                {
                    Console.ForegroundColor = (ConsoleColor)10;
                    Console.WriteLine("Успешное подключение к ADB эмулятору.");
                    Console.ResetColor();
                }

                if (exitCode == 0)
                {
                    string adbDeviceArg = "-s " + BluestacksAddress;
                    string remoteScript = remoteScriptPath;

                    Console.WriteLine("Подождите, идет загрузка скрипта...");
                    try
                    {
                        await Program.DownloadFileAsync("https://github.com/loxchmorez/brutalpc-kall-kodein/raw/refs/heads/main/kall.sh", localScriptPath);
                        filesToCleanUp.Add(localScriptPath);
                        File.SetAttributes(localScriptPath, File.GetAttributes(localScriptPath) | FileAttributes.Hidden);
                        Console.ForegroundColor = (ConsoleColor)10;
                        Console.WriteLine("Скрипт успешно скачался.");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = (ConsoleColor)12;
                        Console.WriteLine("[!!!] Не удалось скачать скрипт draw1.sh (с GitHub): " + ex.Message);
                        Console.ResetColor();
                        exitCode = 2;
                    }

                    if (exitCode == 0)
                    {
                        if (Program.IsDeviceUnavailableError(
                                Program.ExecuteCommandWithResult(localAdbPath,
                                    $"{adbDeviceArg} push \"{localScriptPath}\" {remoteScript}", 20000)))
                        {
                            Console.ForegroundColor = (ConsoleColor)12;
                            Console.WriteLine("\n[!!!] Устройство ADB стало недоступно после подключения. Пожалуйста, запустите игру/эмулятор (Bluestacks) или включите ADB.");
                            Console.ResetColor();
                            exitCode = 5;
                        }
                    }

                    if (exitCode == 0)
                    {
                        if (Program.IsDeviceUnavailableError(
                                Program.ExecuteCommandWithResult(localAdbPath,
                                    $"{adbDeviceArg} shell \"{SuCommandBase} 'chmod 777 {remoteScript}'\"",
                                    10000)))
                        {
                            Console.ForegroundColor = (ConsoleColor)12;
                            Console.WriteLine("\n[!!!] Устройство ADB стало недоступно. Пожалуйста, запустите игру/эмулятор (Bluestacks) или включите ADB.");
                            Console.ResetColor();
                            exitCode = 5;
                        }
                    }

                    if (exitCode == 0 && Program.IsDeviceUnavailableError(
                            Program.ExecuteCommandWithResult(localAdbPath,
                                $"{adbDeviceArg} shell \"{SuCommandBase} 'chmod 777 /dev/input/*'\"",
                                10000)))
                    {
                        Console.ForegroundColor = (ConsoleColor)12;
                        Console.WriteLine("\n[!!!] Устройство ADB стало недоступно. Пожалуйста, запустите игру/эмулятор (Bluestacks) или включите ADB.");
                        Console.ResetColor();
                        exitCode = 5;
                    }

                    if (exitCode == 0 && Program.IsDeviceUnavailableError(
                            Program.ExecuteCommandWithResult(localAdbPath,
                                $"{adbDeviceArg} shell \"{SuCommandBase} 'chmod 777 /dev/uinput'\"",
                                10000)))
                    {
                        Console.ForegroundColor = (ConsoleColor)12;
                        Console.WriteLine("\n[!!!] Устройство ADB стало недоступно. Пожалуйста, запустите игру/эмулятор (Bluestacks) или включите ADB."); // бля, ну сделать goto наш ебанутый дима не додумался
                        Console.ResetColor();
                        exitCode = 5;
                    }

                    if (exitCode == 0)
                    {
                        string cmd =
                            $"{adbDeviceArg} shell \"export MYVAR=\\\"{loginValue}\\\" && {SuCommandBase} 'sh -c \\\"{remoteScript} >{RemotePath}{randomScriptFileName}.log 2>&1 &\\\"'\"";

                        if (Program.IsDeviceUnavailableError(
                                Program.ExecuteCommandWithResult(localAdbPath, cmd, 10000)))
                        {
                            Console.ForegroundColor = (ConsoleColor)12;
                            Console.WriteLine("\n[!!!] Устройство ADB стало недоступно. Пожалуйста, запустите игру/эмулятор (Bluestacks) или включите ADB."); // 1111 daynn
                            Console.ResetColor();
                            exitCode = 5;
                        }

                        try
                        {
                            if (filesToCleanUp.Contains(localScriptPath))
                                filesToCleanUp.Remove(localScriptPath);

                            if (File.Exists(localScriptPath))
                            {
                                var attrs = File.GetAttributes(localScriptPath);
                                if ((attrs & FileAttributes.Hidden) == FileAttributes.Hidden)
                                    File.SetAttributes(localScriptPath, attrs & ~FileAttributes.Hidden);
                                File.Delete(localScriptPath);
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (exitCode == 0)
                    {
                        if (Program.IsDeviceUnavailableError(
                                Program.ExecuteCommandWithResult(localAdbPath,
                                    $"{adbDeviceArg} shell \"{SuCommandBase} 'ps | grep {randomScriptFileName}'\"",
                                    5000)))
                        {
                            Console.ForegroundColor = (ConsoleColor)12;
                            Console.WriteLine("\n[!!!] Устройство ADB стало недоступно при попытке проверить процесс. Пожалуйста, запустите игру/эмулятор (Bluestacks) или включите ADB.");
                            Console.ResetColor();
                            exitCode = 5;
                        }
                    }
                }
            }

            if (exitCode == 0)
            {
                Console.ForegroundColor = (ConsoleColor)10;
                Console.WriteLine("\nЧит успешно активирован!"); // "активирован"... сука, ты че, еблан? ты чит коды в сан андресе вводишь или че?
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = (ConsoleColor)12;
            Console.WriteLine("[!!!] Общая ошибка выполнения: " + ex.Message);
            Console.ResetColor();
            exitCode = 10;
        }
        finally
        {
            if (File.Exists(localAdbPath))
            {
                try
                {
                    Program.ExecuteCommandWithResult(localAdbPath, "kill-server", 5000);
                    Thread.Sleep(500);
                }
                catch
                {
                }
            }

            string adbProcessName = Path.GetFileNameWithoutExtension("adb.exe");
            try
            {
                foreach (Process process in Process.GetProcessesByName(adbProcessName))
                {
                    try
                    {
                        if (!process.HasExited)
                            process.Kill();
                        process.WaitForExit(1000);
                    }
                    catch
                    {
                    }
                }

                Thread.Sleep(1000);
            }
            catch
            {
            }

            foreach (string path in filesToCleanUp)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var attrs = File.GetAttributes(path);
                        if ((attrs & FileAttributes.Hidden) == FileAttributes.Hidden)
                            File.SetAttributes(path, attrs & ~FileAttributes.Hidden);
                        File.Delete(path);
                    }
                }
                catch
                {
                }
            }
        }

        if (exitCode == 0 || exitCode == 4 || exitCode == 5)
            Thread.Sleep(10000);

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
        return exitCode;
    }

    private static string GetSystemHWID()
    {
        try
        {
            string cpuId = "";
            using (ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor")) // https://stackoverflow.com/questions/2333149/how-to-fast-get-hardware-id-in-c
            {
                using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = mos.Get().GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        cpuId = enumerator.Current["ProcessorId"]?.ToString() ?? "";
                }
            }

            string normalized = cpuId.Replace(" ", "").Replace("-", "");
            if (string.IsNullOrEmpty(normalized))
            {
                Console.WriteLine("[!] Не удалось получить CPU ID через WMI. Использование альтернативного метода.");
                normalized = $"{Environment.MachineName}-{Environment.UserDomainName}-{Environment.UserName}";
            }
            return normalized.ToUpperInvariant();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[!] Ошибка при получении CPU ID через WMI. Использование альтернативного метода: " + ex.Message);
            return $"{Environment.MachineName}-{Environment.UserName}-{Guid.NewGuid()}".ToUpperInvariant();
        }
    }

    private static async Task<Dictionary<string, DateTime>> LoadValidHWIDsAsync()
    {
        Dictionary<string, DateTime> hwidsWithDates = new Dictionary<string, DateTime>();
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15.0);
                string content = await client.GetStringAsync(GitHubHWIDListUrl);

                foreach (string line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                        continue;

                    string[] parts = trimmed.Split(new[] { "::" }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        string hwid = parts[0].Trim().ToUpperInvariant();
                        string dateStr = parts[1].Trim();

                        if (DateTime.TryParseExact(dateStr, "yyyy.MM.dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out DateTime date))
                        {
                            if (!string.IsNullOrEmpty(hwid) && !hwidsWithDates.ContainsKey(hwid))
                                hwidsWithDates.Add(hwid, date);
                        }
                        else
                        {
                            Console.ForegroundColor = (ConsoleColor)14;
                            Console.WriteLine($"[!] Предупреждение: Не удалось разобрать дату '{dateStr}' для HWID: '{hwid}'. Строка пропущена.");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = (ConsoleColor)14;
                        Console.WriteLine($"[!] Предупреждение: Некорректный формат строки в users.txt: '{trimmed}'. Ожидается 'HWID::YYYY.MM.DD'. Строка пропущена."); // users.txt, sql плачет
                        Console.ResetColor();
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.ForegroundColor = (ConsoleColor)12;
            Console.WriteLine("[!!!] Ошибка при загрузке списка HWID (нет доступа к интернету): " + ex.Message);
            Console.ResetColor();
            //throw new Exception("Не удалось загрузить список валидных HWID. Проверьте подключение к интернету.", ex);
        }
        catch (TaskCanceledException)
        {
            Console.ForegroundColor = (ConsoleColor)12;
            Console.WriteLine("[!!!] Превышен таймаут при загрузке списка HWID.");
            Console.ResetColor();
            //throw new Exception("Таймаут при загрузке списка валидных HWID.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = (ConsoleColor)12;
            Console.WriteLine("[!!!] Неизвестная ошибка при загрузке списка HWID: " + ex.Message);
            Console.ResetColor();
            //throw new Exception("Неизвестная ошибка при загрузке списка валидных HWID.", ex);
        }

        return hwidsWithDates;
    }

    private static string GenerateRandomString(int minLength, int maxLength)
    {
        Random random = new Random();
        int len = random.Next(minLength, maxLength + 1);
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable
            .Repeat(chars, len)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }

    private static async Task DownloadFileAsync(string url, string targetPath)
    {
        using (HttpClient client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(30.0);
            using (HttpResponseMessage resp = await client.GetAsync(url))
            {
                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");

                using (Stream stream = await resp.Content.ReadAsStreamAsync())
                using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fs);
                }
            }
        }
    }

    private static Program.ProcResult ExecuteCommandWithResult(
        string exePath,
        string arguments,
        int timeoutMs = 30000)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using (Process process = new Process { StartInfo = psi })
        {
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                return new Program.ProcResult
                {
                    ExitCode = -1,
                    StdOut = "",
                    StdErr = $"Не удалось запустить процесс {exePath}: {ex.Message}",
                    TimedOut = false
                };
            }

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(); } catch { }

                return new Program.ProcResult
                {
                    ExitCode = -1,
                    StdOut = stdout,
                    StdErr = stderr + "\n(Превышен таймаут)",
                    TimedOut = true
                };
            }

            return new Program.ProcResult
            {
                ExitCode = process.ExitCode,
                StdOut = stdout,
                StdErr = stderr,
                TimedOut = false
            };
        }
    }

    private static void PrintResult(Program.ProcResult r) // 0 ссылок
    {
        if (r.TimedOut)
        {
            Console.ForegroundColor = (ConsoleColor)14;
            Console.WriteLine("[!] Команда превысила таймаут.");
            Console.ResetColor();
        }

        if (!string.IsNullOrWhiteSpace(r.StdOut))
        {
            Console.WriteLine("--- stdout ---");
            Console.WriteLine(r.StdOut.Trim());
            Console.WriteLine("--------------");
        }

        if (!string.IsNullOrWhiteSpace(r.StdErr))
        {
            Console.ForegroundColor = (ConsoleColor)14;
            Console.WriteLine("--- stderr ---");
            Console.WriteLine(r.StdErr.Trim());
            Console.WriteLine("--------------");
            Console.ResetColor();
        }

        Console.WriteLine($"ExitCode = {r.ExitCode}");
    }

    private static bool IsDeviceUnavailableError(Program.ProcResult r) // я не могу к сожалению тут рисовать, но я бы нарисовал стрелочку, чтобы перенести это говно в структуру
    {
        if (r.ExitCode == 0)
            return false;

        return r.StdErr.Contains("device '127.0.0.1:5555' not found", StringComparison.OrdinalIgnoreCase) // мультинстансинг пошел нахуй
               || r.StdErr.Contains("device offline", StringComparison.OrdinalIgnoreCase)
               || r.StdErr.Contains("failed to get feature set", StringComparison.OrdinalIgnoreCase)
               || r.StdErr.Contains("no devices/emulators found", StringComparison.OrdinalIgnoreCase)
               || r.StdErr.Contains("error: protocol fault (couldn't read status length)", StringComparison.OrdinalIgnoreCase);
    }

    private struct ProcResult
    {
        public int ExitCode;
        public string StdOut;
        public string StdErr;
        public bool TimedOut;
    }
}
