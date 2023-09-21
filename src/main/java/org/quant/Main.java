package org.quant;


import java.util.ArrayList;

public class Main {
    public static void main(String[] args) {
        var commands = new ArrayList<QuantCommand>();
        var core = new QuantCore(commands, new CustomIO(), null, null);
        core.Launch();
    }
}
