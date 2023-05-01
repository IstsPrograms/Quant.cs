Example of IQuantCommand
```cs
public class ExampleCommand : IQuantCommand
{
    public string Name { get; set; } = "Example";
    public string Description { get; set; } = "An example command";
    public QuantPermissions permissionLevel { get; } = QuantPermissions.None;
    public void Execute(params string[] arg)
    {
        Console.WriteLine($"I'm an Example Command, args: {arg[0]}, my description: {Description} and i have path format: {arg[1]}");
    }
}
```
How to create Quant object and launch:
```cs
var quant = new QuantCore(new List<IQuantCommand>()
{
    new ExampleCommand()
});
quant.Launch();
```
