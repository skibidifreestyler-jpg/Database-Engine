using System;
using System.Runtime.InteropServices;
using System.IO;
namespace Handler
{   
    class Person
    {
        
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        
        public Person(string name, int age, string email, string userId, string password)
        {
            Name = name;
            Age = age;
            Email = email;
            UserId = userId;
            Password = password;
        }
    }
    class Program
    {
        [DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int db_insert(string userId, string name, int age, string email, string password);

        [DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int db__get(string targetUserId, 
                                 System.Text.StringBuilder outName, 
                                 out int outAge, 
                                 System.Text.StringBuilder outEmail, 
                                 System.Text.StringBuilder outPassword);
        
        [DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int db__list();

        [DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int db__delete(string targetUserId);


        static void Ls()
        {
            Console.WriteLine("Avaliable commands: ");
            Console.WriteLine("1. INSERT - Add a new person to the database. Example: INSERT Name Age email USERID password");
            Console.WriteLine("2. GET - Find a person's information. Example: GET USERID");
            Console.WriteLine("3. IMPORT - Extract data from a file, txt or log. Example: IMPORT <nameoffile.log/.txt>");
            Console.WriteLine("4. LIST - Shows all USERIDs in the database.");
            Console.WriteLine("5. DELETE - Delete a user from the database. Example: DELETE USERID");
            Console.WriteLine("6. EXIT - Exit the program");
        }



        static void Insert(string[] parts)
        {
            if (parts.Length < 6)
            {
                Console.WriteLine("[ERROR]: Too few arguments!");
                Console.WriteLine("Correct way: INSERT <name> <age> <email> <userID> <password>");
                return;
            }
            string name = parts[1];

            if (!int.TryParse(parts[2], out int age))
            {
                Console.WriteLine("[ERROR]: Age has to be a number!");
                return;
            }

            string email = parts[3];
            string userId = parts[4];
            string password = parts[5];
            int result  =  db_insert(userId, name, age, email, password);
            if (result == 1)
            {
                Console.WriteLine("${Success! '{userId}' was registred sucessfully");
            }
            else
            {
                Console.WriteLine("Error! C++  server didn't process the user!");
            }
        }

        static void Get(string[] parts)
        {
           if (parts.Length < 2)
            {
                Console.WriteLine("[ERROR]: Too few arguments in function call!");
                Console.WriteLine("Usage: GET <USERID>");
                return;
            }

            string targetUserId = parts[1];

            System.Text.StringBuilder name = new System.Text.StringBuilder(100);
            System.Text.StringBuilder email = new System.Text.StringBuilder(100);
            System.Text.StringBuilder password = new System.Text.StringBuilder(100);
            int age = 0;

            int found = db__get(targetUserId, name, out age, email, password);

            if (found == 1)
            {
                Console.WriteLine("\n [Found] User details: ");
                Console.WriteLine($"  User ID:  {targetUserId}");
                Console.WriteLine($"  Name:     {name}");
                Console.WriteLine($"  Age:      {age}");
                Console.WriteLine($"  Email:    {email}");
                Console.WriteLine($"  Password: {password}\n");
            }
            else
            {
               
            }
        }

        static void Import(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("[ERROR]: Please specify the file path!");
                Console.WriteLine("Correct way: IMPORT <file path>");
                return;
            }
            string filePath = parts[1];

            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"[ERROR] File '{filePath}' not found.");
                return;
            }

            try
            {
                
                string[] lines = System.IO.File.ReadAllLines(filePath);
                int successCount = 0;
                int failCount = 0;
                
                Console.WriteLine($"[INFO] Se procesează {lines.Length} linii din '{filePath}'...");

                
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] fields = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (fields.Length < 5)
                    {
                        failCount++;
                        continue; 
                    }

                    string name = fields[0];
                    
                    if (!int.TryParse(fields[1], out int age))
                    {
                        failCount++;
                        continue; 
                    }

                    string email = fields[2];
                    string userId = fields[3];
                    string password = fields[4];

                    int result = db_insert(userId, name, age, email, password);

                    if (result == 1)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                } 

                
                Console.WriteLine($"\n[IMPORT FINISHED]");
                Console.WriteLine($"  - Success: {successCount}");
                Console.WriteLine($"  - Failed / Ignored: {failCount}\n");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"[ERROR] Exeption: {ex.Message}");
            }
        }

        

        static void List()
        {
            Console.WriteLine("Reading.");
            int total = db__list();
            if (total == 0)
            {
                Console.WriteLine("[INFO] No file found.");
            }
        }

        static void Delete(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("[ERROR] Please specify the user ID to delete!");
                Console.WriteLine("Correct way: DELETE <USERID>");
                return;
            }

            string targetUserId = parts[1];
            int result = db__delete(targetUserId);

            if (result == 1)
            {
                Console.WriteLine($"[SUCCESS].");
            }
            else
            {
                Console.WriteLine($"[INFO] No user found with:  '{targetUserId}'.");
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to SimpleDatabase v1.0");
            Console.WriteLine("Please type ls to see available commands.");

            bool running = true;
            while (running)
            {
                Console.WriteLine("db> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                string[] parts = input.Split(' ');
                string command = parts[0].ToLower();
                switch (command)
                {
                    case "ls":
                        Ls();
                        break;
                    
                    case "insert":
                        Insert(parts);
                        break;

                    case "get":
                        Get(parts);
                        break;

                    case "import":
                        Import(parts);
                        break;
                    
                    case "list":
                        List();
                        break;

                    case "delete":
                        Delete(parts);
                        break;

                    case "exit":
                        running = false;
                        Console.WriteLine("Exiting program! Goodbye!");
                        break;

                    default:
                        Console.WriteLine("Unkown command. Please type ls to see available commands. ");
                        break;
                }
            }             

        }
    }
}