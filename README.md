# How to create Quant object:
```java
import java.util.ArrayList;
import org.quant.QuantCore;
import org.quant.QuantCommand;
public class Main {
    public static void main(String[] args) {
        var commands = new ArrayList<QuantCommand>();
        var core = new QuantCore(commands, new CustomIO(), "Your program name", "Your program version");
    }
}
```
# Command example:
```java
import org.quant.QuantCore;
import org.quant.QuantCommand;

class Echo extends QuantCommand {
    public Echo() {
        Name = "echo";
        Description = "Echo";
    }

    @Override
    public QuantCommandResult Execute(QuantCore core, String args) {
        return new QuantCommandResult(core, args);
    }
}
```
# How to make custom I/O:
```java
import org.quant.CustomIO;

public class YourIO extends CustomIO {
    @Override
    public String CustomInput() {
        // Logic of your input. Also you must return string.
    }
    @Override
    public void CustomOutput(String output) {
        // Logic of your output
    }
}
```
