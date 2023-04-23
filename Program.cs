namespace Quant.Cs {
    class QuantCore
    {
        public List<IQuantCommand> commands;
        public string pathFormat = "\\";
        public QuantCore(List<IQuantCommand> commands)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            { // Check OS
                pathFormat = "/";
                Environment.CurrentDirectory = "/home/";
            }
            else
            {
                Environment.CurrentDirectory = $"C:\\Users\\{Environment.UserName}\\";
            }
            this.commands = commands;
        }

        public void Launch()
        {
            Console.WriteLine("Launched!");
            while (true)
            {
                Console.Write($"{Environment.UserName}@{Environment.CurrentDirectory} > ");
                string argument = Console.ReadLine();
                foreach (var item in commands)
                {
                    if (item.name.ToLower() == argument.Split()[0])
                    {
                        string[] argumentList = { argument.Replace($"{argument.Split()[0]} ", ""), pathFormat};
                        item.Execute(argumentList); // If command name equals first word of input, execute command
                    }
                }
            }
        }


    }
    interface IQuantCommand
    {
        public string name { get; set; }
        public string description { get; set; }
        public void Execute(params string[] arg)
        {

        }
    }
}