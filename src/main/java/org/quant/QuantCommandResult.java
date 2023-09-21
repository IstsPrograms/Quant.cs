package org.quant;

public class QuantCommandResult {
    public QuantCore res;
    public String output;
    public QuantCommandResult(QuantCore res, String output) {
        this.res = res;
        this.output = output;
    }
}
