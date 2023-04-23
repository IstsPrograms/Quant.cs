Example of IQuantCommand
```cs
public class ExampleCommand : IQuantCommand
{
    public string name { get; set; } = "Example";
    public string description { get; set; } = "An example command";
    public void Execute(params string[] arg)
    {
        Console.WriteLine($"I'm an Example Command, args: {arg[0]}, my description: {description}"); 
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
