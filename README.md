# QuantCore object:
```cs
using Quant.cs;
var commands = new List<IQuantCommand>();
bool useMultithreading = false; // Replace with 'true' if you want to use multi threading
var core = new QuantCore(commands, "Name of your program", "Version of your program", useMultithreading);
```
# Command example:
```cs
var exampleCommand = new QuantCommand()
{
    Name = "example",
    Description = "Example Command",
    OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore quantCore, string s) =>
    {
        return "Output";
    }
}
```
**or**
```cs
public class Command : IQuantCommand
{
    public string Name { get; set; } = "example";
    public string Description { get; set; } = "Example Command";
    public string Execute(ref QuantCore core, string args)
    {
        return "Output";
    };
}
```
# Merge QuantCore with other
```cs
core.MergeWith(otherCore);
```
**or**
```cs
core.MergeWith(addressOfQuantServerHere);
```
**or**
``cs
QuantCore.MergeCores(firstCore, secondCore); // returns new QuantCore
```
# You can also make custom I/O
**Input:**
```cs
core.OnInputEvent += new OnInputEventHandler(() =>
{
    // Logic of your input. Also you must return string.
});
```
**Output:**
```cs
core.OnOutputEvent += new OnOutputEventHandler((string output) =>
{
    // Logic of your output
});
```

# Notifications
**Send notification:**
```cs
core.GetNotification(new Notification("Title", "Description", SomeData, Sender)); // SomeData and Sender are not necessary
```

**Custom notification handler:**
```cs
core.OnNotificationEvent += new OnNotificationEventHandler((ref QuantCore core, ref Notification notification) =>
{
    // Logic of your notification handler
});
```

# Quant Server

**How to create Quant Server**
```cs
using Quant.cs;
var commands = new List<IQuantCommand>();
string host = "http://127.0.0.1:8080/"; // Replace 'http://127.0.0.1:8080/' with other if necessary
var core = new QuantCore(commands, "Name of your Quant Server", "Version of your Quant Server", false, host);
core.Launch(); // Start server
```

**Custom request handler:**
```cs
// core is server-side
core.OnRequestEvent = new OnRequestHandler((ref HttpListenerRequest request, ref HttpListenerResponse response, ref HttpListenerContext context, ref HttpListener server, ref ServerQuantCore quantCore) =>
{
    // Logic of your request handler
});
```

**Custom request sender:**
```cs
core.OnRequestSendingEvent = new OnRequestSendingHandler((string cmd, string address, QuantRequestType quantRequest) =>
{
    // Logic here
});
```

**How to send request to Quant Server**
```cs
// core is client-side
core.SendRequest("command", "request uri"); // Returns response
```

