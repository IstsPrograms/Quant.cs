# General

**Quant Commands**

There are two ways to create command

First:
```cs
var exampleCommand = new QuantCommand()
{
    Name = "example_name",
    Description = "Example Description",
    OnCommandExecution = (ref QuantCore core, string s) =>
    {
        return "Output";
    }
}
```
Second:
```cs
var exampleCommand = new Command();

public class Command : IQuantCommand
{
    public string Name { get; set; } = "example";
    public string Description { get; set; } = "Example Command";
    public string Execute(ref QuantCore core, string s)
    {
        return "Output";
    };
}
```

You can handle case when command is NOT found:
```cs
core.OnNoCommandFoundEvent = (string cmd) => 
{
    // Logic here
};
```

**Custom I/O**

Input:
```cs
// core is QuantCore
core.OnInputEvent = () =>
{
    // Logic of your input.
    return "result";
};
```

Output:
```cs
core.OnOutputEvent = (string smth) => 
{
    // Logic of your output
};
```

**Merge QuantCores**

```cs
core.MergeWith(otherCore);
```
or
```cs
core.MergeWith("addres of QuantServer here");
```
or
```cs
QuantCore.MergeCores(firstCore, secondCore);
```
ATTENTION! Pins DOESN'T merge.

**Pins**

You can 'pin' other objects to QuantCore and use them in commands

```cs
core.AddPin(AnyObject, "name of this pin");
```

There is how you can use it:

```cs
var exampleCommand = new QuantCommand()
{
    Name = "example_name",
    Description = "Example Description",
    OnCommandExecution = (ref QuantCore core, string s) =>
    {
	// returns value of Pin with a name equal to the value of 's'
        return $"{core.Pins[s].Value}";
    }
}
```

**Notifications**

Send notification:
```cs
core.GetNotification(new Notification("Title", "Description", Data, Sender)); // Data and Sender can be null
```

**Custom notification handler:**
```cs
core.OnNotificationEvent = (ref QuantCore core, ref Notification notification) =>
{
    // Logic of your notification handler
};
```

# QuantCore

**How to create:**

```cs
var commands = new List<IQuantCommand>();
bool useMultithreading = false; // Replace with 'true' if you want to use multi threading
var core = new QuantCore(commands, "Name of your program", "Version of your program", useMultithreading);
```

**How to use**

you can launch it by method `Launch` but also you can execute commands without QuantCore being launched
```cs
core.Launch();
```
or
```cs
core.ExecuteCommand("any command");
```

**Request sending**

You can send request to QuantServer by method `SendRequest`
```cs
core.SendRequest("command (can be empty if request type is GetInfo)", "address of QuantCore", QuantRequestType.GetInfo/ExecuteCommand);
```

And also you can make your own request sender:
```cs
core.OnRequestSendingEvent = (string cmd, string address, QuantRequestType quantRequest) => 
{
    // Logic here
    return "result";
};
```

# Quant Server

**How to create:**

```cs
var commands = new List<IQuantCommand>();
string host = "http://127.0.0.1:8080/"; // Replace 'http://127.0.0.1:8080/' with other if necessary
var core = new QuantCore(commands, "Name of your Quant Server", "Version of your Quant Server", false, host);
```

**How to use**

Start server:
```cs
core.Launch();
```

**Custom request handler**
```cs
core.OnRequestEvent = (ref HttpListenerRequest request, ref HttpListenerResponse response, ref HttpListenerContext context, ref HttpListener server, ref ServerQuantCore quantCore) =>
{
    // Logic of your request handler
};
```