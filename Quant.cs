using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Quant.cs;


// Handlers
public delegate string OnCommandExecutionHandler(ref QuantCore core, string args);
public delegate void OnOutputEventHandler(string output);
public delegate string OnInputEventHandler();
public delegate void OnNotificationEventHandler(ref QuantCore core, ref Notification notification);

public class QuantCore
{
    public readonly string Name; // Your program's name
    public readonly string Version; // Your program's version

    protected readonly Stack<List<IQuantCommand>> _snapshots;
    public List<IQuantCommand> Commands;
    protected readonly List<Thread>? _threads;
    protected byte _warnings;
    protected readonly bool _useThreading;

    // Events
    public OnOutputEventHandler? OnOutputEvent; // Used for custom output
    public OnInputEventHandler? OnInputEvent;   // Used for custom input
    public OnNotificationEventHandler? OnNotificationEvent; // Used for handling notifications

    protected bool _launched = true;
    
    public virtual void GetNotification(Notification notification)
    {
        if (OnNotificationEvent != null)
        {
            var copy = this;
            OnNotificationEvent.Invoke(ref copy, ref notification);
            if (copy.Commands != Commands)
            {
                _snapshots.Push(Commands);
                Commands = copy.Commands; // Synchronization
            }
        }
        else
        {
            string senderName = notification.Sender != null ? notification.Sender.GetType().Name : "No Info";
            InvokeOutput($"Got notification!\nTitle: {notification.Title}\nDescription: {notification.Description}\nSender: {senderName}");
        }
    }
   
    public QuantCore(List<IQuantCommand> commands, string name = "Quant.cs", string version = "1.0", bool useThreading = false)
    {
        Commands = commands;
        Name = name;
        Version = version;
        _useThreading = useThreading;
        if (useThreading)
        {
            _threads = new List<Thread>();
        }
        // Init script (Executes all commands in file init.sh)
        if (File.Exists("init.sh"))
        {
            foreach (var cmd in File.ReadAllText("init.sh").Split("\n"))
            {
                ExecuteCommand(cmd);
            }
        }
      
        Environment.SetEnvironmentVariable("QUANT_PATH", Environment.CurrentDirectory);
        // Setting current directory
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                Environment.CurrentDirectory = $"{Environment.GetLogicalDrives()[0]}"; 
                break;
            case PlatformID.Unix:
                Environment.CurrentDirectory = Environment.UserName != "root" ? $"/home/{Environment.UserName}" : "/"; // If root, path will be '/', if not path will be '/home/{user}'
                break;
            default:
                Environment.CurrentDirectory = "/";
                break;
        }

        _snapshots = new();
        _snapshots.Push(Commands);
    }

    public virtual string InvokeInput() // If there is no custom input, then use default (Console.ReadLine)
    {
        if (OnInputEvent == null)
        {
            return Console.ReadLine() ?? "";
        }
        return OnInputEvent.Invoke();
    }
   
    public virtual void InvokeOutput(string output) // If there is no custom output, then use default (Console.Write)
    {
        if (output != "")
        {
            if (OnOutputEvent == null)
            {
                Console.Write(output.Contains($"{Environment.UserName}@{Environment.CurrentDirectory}")
                ? output
                : $"{output}\n");
            }
            else
            {
                OnOutputEvent.Invoke(output);
            }
        }
    }

    public virtual void ExecuteCommand(string command)
    {
        foreach (var cmd in Commands)
        {
            if (cmd.Name.ToLower().Equals(command.Split()[0]))
            {

                QuantCore quant = this;
                var args = command.Replace($"{command.Split()[0]} ", ""); // Remove command name from input
                try
                {
                    string result = cmd.Execute(ref quant, args);
                    InvokeOutput(result);
                }
                catch (Exception exception)
                {
                    InvokeOutput("Information:");
                    InvokeOutput($"Command {cmd.Name.ToLowerInvariant()} caused Exception\nMessage: {exception.Message}\nArgs: {command.Replace($"{command.Split()[0]} ", "")}");
                    _warnings += 1;
                    // If there is 3 or more warnings, then change Commands to previous state (If there is one)
                    if (_warnings >= 3)
                    {
                        if (_snapshots.TryPop(out List<IQuantCommand>? changeCommands))
                        {
                            lock (Commands)
                            {
                                Commands = changeCommands;
                                InvokeOutput("List with commands changed to previous one");
                            }
                            return;
                        }
                        else
                        {
                            // If there is no any previous state of Commands, then close Quant.cs
                            _launched = false;
                            Console.WriteLine("Closing Quant.cs");
                        }
                    }
                }
                lock (Commands)
                {
                    if (quant.Commands != Commands)
                    {
                        _snapshots.Push(new(Commands));
                        Commands = quant.Commands; // Synchronization
                    }
                }
                return;
            }
        }
    }


    public virtual void SendRequest(string cmd, string address)
    {
        using(HttpClient client = new HttpClient())
        {
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Content = new StringContent(
                    new QuantRequest()
                    {
                        CommandToExecute = cmd
                    }.GetJson()
                ),
                RequestUri = new Uri(address),
                Method = HttpMethod.Post
            }).GetAwaiter().GetResult();
            string result;
            using(StreamReader reader = new StreamReader(response.Content.ReadAsStream()))
            {
                result = reader.ReadToEnd();
            }
            InvokeOutput(result);
        }
    }

   
    public virtual void Launch() 
    {
        while (_launched)
        {
            InvokeOutput($"{Environment.UserName}@{Environment.CurrentDirectory} > ");
            string input = InvokeInput();
            if (input != "")
            {
                if (_useThreading && _threads != null)
                {
                    var thread = new Thread(ignoreIt => ExecuteCommand(input));
                    _threads.Add(thread);
                    _threads[^1].Start();
                }
                else
                {
                    ExecuteCommand(input);
                }
            }
        }
    }
}

public interface IQuantCommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Execute(ref QuantCore core, string args);
}

// You can use it if you don't want to create many classes that inherit IQuantCommand
public class QuantCommand : IQuantCommand
{
    public string Name { get; set; } = "default-command";
    public string Description { get; set; } = "";
    public OnCommandExecutionHandler? OnCommandExecution { get; init; }
    public string Execute(ref QuantCore core, string args)
    {
        return OnCommandExecution == null ? "Command is null" : OnCommandExecution.Invoke(ref core, args);
    }
}

public class Notification
{
    public string Title { get; set; }
    public string Description { get; set; }
    public object? Data { get; set; }
    public object? Sender { get; set; }

    public Notification(string title, string description, object? data, object? sender)
    {
        Title = title;
        Description = description;
        Data = data;
        Sender = sender;
    }
}

// TESTING FEATURE!!! IT CAN NOT WORK PROPERLY!!!

public delegate void OnRequestHandler(ref HttpListenerRequest request, ref HttpListenerResponse response, ref HttpListenerContext context, ref HttpListener server, ref ServerQuantCore quantCore);

public class ServerQuantCore : QuantCore
{
    public string Host { get; set; }
    public OnRequestHandler? OnRequestEvent;


    public ServerQuantCore(List<IQuantCommand> commands, string name = "Quant.cs", string version = "1.0", bool useThreading = false, string host = "http://127.0.0.1:8080/") : base(commands, name, version, useThreading)
    {
        Host = host;
    }


    public new string ExecuteCommand(string command)
    {
        foreach (var cmd in Commands)
        {
            if (cmd.Name.ToLower().Equals(command.Split()[0]))
            {

                QuantCore quant = this;
                var args = command.Replace($"{command.Split()[0]} ", ""); // Remove command name from input
                try
                {
                    string result = cmd.Execute(ref quant, args);
                    return result;
                }
                catch (Exception exception)
                {
                    InvokeOutput("Information:");
                    InvokeOutput($"Command {cmd.Name.ToLowerInvariant()} caused Exception\nMessage: {exception.Message}\nArgs: {command.Replace($"{command.Split()[0]} ", "")}");
                    _warnings += 1;
                    // If there is 3 or more warnings, then change Commands to previous state (If there is one)
                    if (_warnings >= 3)
                    {
                        if (_snapshots.TryPop(out List<IQuantCommand>? changeCommands))
                        {
                            Commands = changeCommands;
                            InvokeOutput("List with commands changed to previous one");
                            return "EXCEPTION";
                        }
                        else
                        {
                            // If there is no any previous state of Commands, then close Quant.cs
                            _launched = false;
                            Console.WriteLine("Closing Quant.cs");
                        }
                    }
                }
            }
        }
        return "NO COMMAND FOUND";
    }



    public void InvokeHandleRequest(ref HttpListenerRequest request, ref HttpListenerResponse response, ref HttpListenerContext context, ref HttpListener server, ref ServerQuantCore quantCore)
    {
        InvokeOutput("Processing: Got request");
        if(OnRequestEvent == null)
        {
            string content;
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                content = reader.ReadToEnd();
            }
            try
            {
                var req = QuantRequest.GetFromJson(content);
                var result = ExecuteCommand(req.CommandToExecute);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(result);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
                InvokeOutput("Done: Responded");
            }
            catch
            {
                InvokeOutput("Error: Non-Quant request");
            }
        }
        else
        {
            InvokeOutput("Processing: Invoking custom request handler");
            OnRequestEvent.Invoke(ref request, ref response, ref context, ref server, ref quantCore);
        }
    }


    public override void Launch()
    {
        var server = new HttpListener();
        server.Prefixes.Add(Host);
        server.Start();
        InvokeOutput("Done: Launched");
        while(_launched)
        {
            var context = server.GetContextAsync().GetAwaiter().GetResult();
            ServerQuantCore copy = this;
            var request = context.Request;
            var response = context.Response;

            InvokeHandleRequest(ref request, ref response, ref context, ref server, ref copy);
        }
    }
}

public class QuantRequest
{
    public string CommandToExecute { get; set; }
    public object? Data { get; set; }
    public string GetJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions()
        {
            IncludeFields = true
        });
    }
    public static QuantRequest GetFromJson(string json)
    {
        return JsonSerializer.Deserialize(json, typeof(QuantRequest), new JsonSerializerOptions()
        {
            IncludeFields = true
        }) as QuantRequest;
    }
}