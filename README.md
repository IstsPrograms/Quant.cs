# How to create Quant object:
```cs
using Quant.cs;
var commands = new List<IQuantCommand>();
var core = new QuantCore(commands, "Name of your program", "Version of your program");
```
# Command example:
```cs
using Quant.cs;
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
