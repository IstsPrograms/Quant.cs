package org.quant;

import java.util.Scanner;

public class CustomIO {
    // Override it
    public String CustomInput() {
        return new Scanner(System.in).nextLine();
    }
    // Override it
    public void CustomOutput(String output) {
        if(!output.contains(System.getProperty("user.name") + "@" + System.getProperty("user.dir") +" > ")) {
            System.out.print(output + "\n");
        }
        else {
            System.out.print(output);
        }
    }
}
