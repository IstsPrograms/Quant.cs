package org.quant;

public class QuantCommand {
    public String Name;
    public String Description;
    public QuantCommand() {

    }

    // Override it
    public QuantCommandResult Execute(QuantCore core, String args) {
        return new QuantCommandResult(core, "Placeholder");
    }
}
