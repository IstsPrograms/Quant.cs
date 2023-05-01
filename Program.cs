using System.Text.Json;

namespace Quant.Cs {
    class AuthenticationModule
    {
        public static void WriteUsers(List<User> users, string normalPath)
        {
            string currentPath = Environment.CurrentDirectory;
            Environment.CurrentDirectory = normalPath;
            File.WriteAllText("userslocale.json", JsonSerializer.Serialize(users, typeof(List<User>), new JsonSerializerOptions()
            {
                IncludeFields = true,
            }));
            Environment.CurrentDirectory = currentPath;
            Console.WriteLine("Users saved!");
        }
        public static List<User> CreateUser(User currentUser, List<User> userslocale)
        {
            if(currentUser.Permissions == QuantPermissions.Root)
            {
                try
                {
                    Console.Write("Enter new user's name > ");
                    string name = Console.ReadLine();
                    Console.Write("Enter new user's password > ");
                    string password = Console.ReadLine();
                    Console.WriteLine("Select new user's permission level:");
                    Console.WriteLine("[0] None\n[1] Guest\n[2] Root");
                    Console.Write("> ");
                    int permissions;
                    int.TryParse(Console.ReadKey().KeyChar.ToString(), out permissions);
                    QuantPermissions givenPermission = (QuantPermissions)permissions;
                    userslocale.Add(new User(name, password, givenPermission));
                    Console.WriteLine("\nNew user created!");
                }
                catch
                {
                    Console.WriteLine($"Error: wrong data given");
                }
            }
            return userslocale;
        }
        public static User? Authenticate()
        {
            if(File.Exists("userslocale.json"))
            {
                List<User> users = new List<User>();
                try
                {
                    users = JsonSerializer.Deserialize(File.ReadAllText("userslocale.json"), typeof(List<User>), new JsonSerializerOptions()
                    {
                        IncludeFields = true,
                    }) as List<User>;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"userslocale.json have no user data: {ex}");
                    return null;
                }
                for(int i = 0; i < users.Count; i++)
                {
                    Console.WriteLine($"[{i}] {users[i].Name}");
                }
                Console.Write($"Select user by ID > ");
                try
                {
                    int selectedUser = Convert.ToInt32(Console.ReadKey().KeyChar.ToString());
                    bool notAutorized = true;
                    while (notAutorized)
                    {
                        Console.Write("$\nWrite your password > ");
                        string password = Console.ReadLine();
                        if (users[selectedUser].Password == password)
                        {
                            Console.WriteLine("Authorized!");
                            return users[selectedUser];
                        }
                        else
                        {
                            Console.WriteLine("\nWrong password!\nWrite your password >");
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                Console.WriteLine("userslocale.json not found. Launching system from root...");
                return null;
            }
            return null;
        }
    }
    class QuantCore
    {
        public List<IQuantCommand> commands;
        public string pathFormat = "\\";
        public User currentUser;
        public QuantCore(List<IQuantCommand> commands, User? currentUser = null)
        {
            
            this.commands = commands;
            var res = AuthenticationModule.Authenticate();
            if (res == null)
            {
                List<User> userslocale = new List<User>();
                userslocale = AuthenticationModule.CreateUser(new User("Root", "Root", QuantPermissions.Root), userslocale);
                AuthenticationModule.WriteUsers(userslocale, Environment.CurrentDirectory);
                res = AuthenticationModule.Authenticate();
            }
            currentUser = res;
            if (currentUser == null)
            {
                this.currentUser = new User($"{Environment.UserName}", "Root", QuantPermissions.Root); // Set current user to root, if not loggined in
            }
            else
            {
                this.currentUser = currentUser;
            }
        }

        public void Launch()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            { // Check OS
                pathFormat = "/";
                Environment.CurrentDirectory = "/home/"; // If system is Linux, set current directory to home directory
            }
            else
            {
                Environment.CurrentDirectory = $"C:\\Users\\{Environment.UserName}\\"; // If system is windows, set current directory to user's directory
            }
            Console.WriteLine("Launched!");
            while (true)
            {
                Console.Write($"{currentUser.Name}@{Environment.CurrentDirectory} > ");
                string argument = Console.ReadLine();
                foreach (var item in commands)
                {
                    if (item.Name.ToLowerInvariant() == argument.Split()[0].ToLowerInvariant())
                    {
                        if (item.permissionLevel <= currentUser.Permissions)
                        {
                            string[] argumentList = { argument.Replace($"{argument.Split()[0]} ", ""), pathFormat };
                            item.Execute(argumentList); // If command name equals first word of input and user has permission - execute command
                        }
                        else
                        {
                            Console.WriteLine($"You have no permission to execute this command. Command required permission '{item.permissionLevel}', you have: '{currentUser.Permissions}'");
                        }
                    }
                }
            }
        }
    }
    class User
    {
        public string Name;
        public string Password;
        public QuantPermissions Permissions;
        public User(string Name, string Password, QuantPermissions Permissions) 
        {
            this.Name = Name;
            this.Password = Password;
            this.Permissions = Permissions;
        }
    }
    public enum QuantPermissions
    {
        Root = 2,
        Guest = 1,
        None = 0
    }
    interface IQuantCommand
    {
        public QuantPermissions permissionLevel { get; }
        public string Name { get; set; }
        public string Description { get; set; }

        public void Execute(params string[] arg) 
        {

        }
    }

}