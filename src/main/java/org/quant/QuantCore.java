package org.quant;

import java.util.ArrayList;
import java.util.Stack;

public class QuantCore {
    public String Name;
    public String Version;
    public ArrayList<QuantCommand> commands;
    public CustomIO io;
    private Stack<ArrayList<QuantCommand>> _snapshots;
    private int _warnings = 0;
    private boolean _launched = true;


    public QuantCore(ArrayList<QuantCommand> commands, CustomIO io, String name, String version) {
        _snapshots = new Stack<ArrayList<QuantCommand>>();
        _snapshots.push(commands);
        this.commands = commands;
        this.io = io;
        if(name == null) {
            Name = "Quant.java";
        }
        if(version == null) {
            Version = "1.0";
        }
    }

    public void ExecuteCommand(String command) {
        var splittedString = command.split(" ");
        for (var cmd : commands) {
            if(splittedString[0].toLowerCase().equals(cmd.Name.toLowerCase())) {
                try {
                    var result = cmd.Execute(this, command.replaceAll(splittedString[0] + " ", ""));
                    io.CustomOutput(result.output);
                    commands = result.res.commands;
                }
                catch (Exception exception) {
                    io.CustomOutput("Information:");
                    io.CustomOutput("Command " + cmd.Name.toLowerCase() + " caused Exception");
                    io.CustomOutput("Exception message: " + exception.getMessage());
                    io.CustomOutput("Args: " + command.replaceAll(splittedString[0] + " ", ""));
                    _warnings++;
                    if(_warnings >= 3) {
                        try {
                        var state = _snapshots.pop();
                            if(state != null) {
                                commands = state;
                                io.CustomOutput("List with commands changed to previous one");
                                return;
                            }
                            else {
                                _launched = false;
                                io.CustomOutput("Closing Quant.java...");
                            }
                        }
                        catch (Exception exception1) {
                            _launched = false;
                            io.CustomOutput("Closing Quant.java...");
                        }

                    }
                }
                return;
            }
        }
        io.CustomOutput("Not found");
    }

    public void Launch() {
        while(_launched) {
            io.CustomOutput(System.getProperty("user.name") + "@" + System.getProperty("user.dir") +" > ");
            var input = io.CustomInput();
            ExecuteCommand(input);
        }
    }
}
