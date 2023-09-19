namespace Quant.cs;


// Handlers
public delegate string OnCommandExecutionHandler(ref QuantCore core, string args);
public delegate void OnOutputEventHandler(string output);
public delegate string OnInputEventHandler();

public class QuantCore
{
   public readonly string Name; // Your program's name
   public readonly string Version; // Your program's version

   private readonly Stack<List<IQuantCommand>> _snapshots;
   public List<IQuantCommand> Commands;
   private byte _warnings;
   
   // Events
   public OnOutputEventHandler? OnOutputEvent; // Used for custom output
   public OnInputEventHandler? OnInputEvent;   // Used for custom input

   private bool _launched = true;
   
   public QuantCore(List<IQuantCommand> commands, string name = "Quant.cs", string version = "1.0")
   {
      Commands = commands;
      Name = name;
      Version = version;

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
            Environment.CurrentDirectory = $"{Environment.GetLogicalDrives()[0]}:\\"; 
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

   public string InvokeInput() // If there is no custom input, then use default (Console.ReadLine)
   {
      if (OnInputEvent == null)
      {
         return Console.ReadLine() ?? "";
      }
      return OnInputEvent.Invoke();
   }
   
   public void InvokeOutput(string output) // If there is no custom output, then use default (Console.Write)
   {
      if (OnOutputEvent == null)
      {
         Console.Write(output.Contains($"{Environment.UserName}@{Environment.CurrentDirectory}") ?  output : $"{output}\n");
      }
      else
      {
         OnOutputEvent.Invoke(output);
      }
   }

   public void ExecuteCommand(string command)
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
                     Commands = changeCommands;
                     InvokeOutput("List with commands changed to previous one");
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
            if (quant.Commands != Commands)
            {
               _snapshots.Push(new(Commands));
               Commands = quant.Commands; // Synchronization
            }
            return;
         }
      }
   }
   
   public void Launch() 
   {
      while (_launched)
      {
         InvokeOutput($"{Environment.UserName}@{Environment.CurrentDirectory} > ");
         string input = InvokeInput();
         if (input != "")
         {
            ExecuteCommand(input);
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
   public OnCommandExecutionHandler? OnCommandExecution { get; set; }
   public string Execute(ref QuantCore core, string args)
   {
      return OnCommandExecution == null ? "Command is null" : OnCommandExecution.Invoke(ref core, args);
   }
}
