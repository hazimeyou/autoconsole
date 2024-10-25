using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

public class Program
{
    static StreamWriter inputWriter;
    static string serverDirectory;

    static async Task Main(string[] args)
    {
        // config.txtからディレクトリを読み込む
        StreamReader sr = new StreamReader(@"config.txt", Encoding.UTF8);
        string content = sr.ReadToEnd();
        sr.Close();
        Console.WriteLine(content);
        serverDirectory = @content.Trim(); // サーバーディレクトリを設定
        Directory.SetCurrentDirectory(serverDirectory);
        string dirPath = @"plugins";
        string dirPath2 = @"commands";
        Directory.CreateDirectory(dirPath);
        Directory.CreateDirectory(dirPath2);
        string pluginDirectory = Path.Combine(serverDirectory, "plugins");
        FileSystemWatcher watcher = new FileSystemWatcher(pluginDirectory, "*.dll");
        watcher.Created += async (sender, e) => await OnPluginAddedAsync(e.FullPath);
        watcher.EnableRaisingEvents = true;

        // 初回読み込み
        await LoadPluginsAsync(pluginDirectory);
        Console.WriteLine("プラグインの起動・読み込みが終わりました。メインアプリを起動します。");

        // プロセスの設定と開始
        Process process = new Process();
        process.StartInfo.FileName = Path.Combine(serverDirectory, "are.bat");
        process.StartInfo.Arguments = "";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        inputWriter = process.StandardInput;
        StreamReader outputReader = process.StandardOutput;

        // 出力を非同期で読み取るタスクを開始
        Task.Run(async () =>
        {
            while (!process.HasExited)
            {
                string output = await outputReader.ReadLineAsync();
                if (output != null)
                {
                    Console.WriteLine(output);
                }
            }
        });

        // ユーザーの入力を待ち続ける
        while (true)
        {
            string command = Console.ReadLine();
            if (command.StartsWith("!"))
            {
                string[] commandParts = command.Split(' ', 2);
                string pluginFileName = commandParts[0].Substring(1) + ".dll";
                string pluginFilePath = Path.Combine(pluginDirectory, pluginFileName);
                string pluginCommand = commandParts.Length > 1 ? commandParts[1] : string.Empty;

                if (File.Exists(pluginFilePath))
                {
                    await LoadPluginAsync(pluginFilePath, pluginCommand);
                }
                else
                {
                    Console.WriteLine($"プラグインファイル {pluginFilePath} が見つかりませんでした。");
                }
            }

            await inputWriter.WriteLineAsync(command);
            await inputWriter.FlushAsync();
        }
        process.WaitForExit();
    }

    static void ExecuteMacro(string macroFileName)
    {
        Console.WriteLine($"{macroFileName}のマクロが実行されました！");
        if (File.Exists(macroFileName))
        {
            string[] commands = File.ReadAllLines(macroFileName);
            foreach (string command in commands)
            {
                inputWriter.WriteLine(command);
                inputWriter.Flush();
            }
        }
        else
        {
            Console.WriteLine($"マクロファイル {macroFileName} が見つかりませんでした。");
        }
    }

    private static async Task OnPluginAddedAsync(string pluginFile)
    {
        await LoadPluginAsync(pluginFile, pluginFile);
    }

    private static async Task LoadPluginsAsync(string pluginDirectory)
    {
        var pluginFiles = Directory.GetFiles(pluginDirectory, "*.dll");
        foreach (var pluginFile in pluginFiles)
        {
            await LoadPluginAsync(pluginFile, pluginFile);
        }
    }

    private static async Task LoadPluginAsync(string pluginFile, string command)
    {
        await Task.Run(async () =>
        {
            try
            {
                Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);
                foreach (Type type in pluginAssembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(BasePlugin)))
                    {
                        BasePlugin pluginInstance = (BasePlugin)Activator.CreateInstance(type);
                        string dlloutput = await pluginInstance.GetOutputAsync(command, inputWriter);
                        Console.WriteLine($"Plugin Output: {dlloutput}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"予期せぬエラーが発生しました: {ex.Message}");
            }
        });
    }


    public static string GetServerDirectory()
    {
        return serverDirectory;
    }
}
