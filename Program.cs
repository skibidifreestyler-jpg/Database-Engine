
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Handler
{
    class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        public Person(string name, int age, string email,
                      string userId, string password)
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
        private const int BufferSize = 256;

        [DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int db_insert(
            string name,
            int age,
            string email,
            string password,
            StringBuilder outUserId,
            int outUserIdCapacity
        );

        
[DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
public static extern int db__get(
    string targetUserId,
    System.Text.StringBuilder outName,
    int nameCapacity,
    out int outAge,
    System.Text.StringBuilder outEmail,
    int emailCapacity,
    System.Text.StringBuilder outPassword,
    int passwordCapacity
);


        [DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int db__list();

        [DllImport("libengine.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int db__delete(string targetUserId);

        static void Ls()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("1. INSERT <name> <age> <email> <password>");
            Console.WriteLine("   Register a person with an automatically generated ID.");
            Console.WriteLine("2. GET <USERID>");
            Console.WriteLine("   Retrieve a person's information.");
            Console.WriteLine("3. IMPORT <file>");
            Console.WriteLine("   Import records from a text file.");
            Console.WriteLine("4. LIST");
            Console.WriteLine("   List database records.");
            Console.WriteLine("5. DELETE <USERID>");
            Console.WriteLine("   Delete a person after confirmation.");
            Console.WriteLine("6. LS");
            Console.WriteLine("   Show this command list.");
            Console.WriteLine("7. EXIT");
            Console.WriteLine("   Close the application.\n");
        }

        static string[] ParseInput(string input)
        {
            return input.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );
        }

        static bool TryInsert(
            string name,
            int age,
            string email,
            string password,
            out string userId)
        {
            StringBuilder generatedId = new StringBuilder(50);

            int result = db_insert(
                name,
                age,
                email,
                password,
                generatedId,
                generatedId.Capacity
            );

            userId = generatedId.ToString();
            return result == 1;
        }

        static void Insert(string[] parts)
        {
            if (parts.Length < 5)
            {
                Console.WriteLine(
                    "[ERROR] Usage: INSERT <name> <age> <email> <password>"
                );
                return;
            }

            string name = parts[1];

            if (!int.TryParse(parts[2], out int age) || age < 0)
            {
                Console.WriteLine("[ERROR] Age must be a non-negative integer.");
                return;
            }

            string email = parts[3];
            string password = parts[4];

            try
            {
                if (TryInsert(name, age, email, password, out string userId))
                {
                    Console.WriteLine("[SUCCESS] User registered!");
                    Console.WriteLine($"Generated UserID: {userId}");
                }
                else
                {
                    Console.WriteLine(
                        "[ERROR] The C++ engine couldn't insert the user."
                    );
                }
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("[ERROR] Could not load libengine.so.");
            }
            catch (EntryPointNotFoundException)
            {
                Console.WriteLine(
                    "[ERROR] db_insert was not found in libengine.so. " +
                    "Check the exported function name and rebuild the library."
                );
            }
        }

        static void Get(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("[ERROR] Usage: GET <USERID>");
                return;
            }

            string targetUserId = parts[1];

            StringBuilder name = new StringBuilder(BufferSize);
            StringBuilder email = new StringBuilder(BufferSize);
            StringBuilder password = new StringBuilder(BufferSize);

            
int age = 0;

int found = db__get(
    targetUserId,
    name, name.Capacity,
    out age,
    email, email.Capacity,
    password, password.Capacity
);


            if (found == 1)
            {
                Console.WriteLine("\n[FOUND] User details:");
                Console.WriteLine($"User ID: {targetUserId}");
                Console.WriteLine($"Name:    {name}");
                Console.WriteLine($"Age:     {age}");
                Console.WriteLine($"Email:   {email}");

                
                Console.WriteLine("Password: [hidden]\n");
            }
            else
            {
                Console.WriteLine(
                    $"[INFO] No user found with ID '{targetUserId}'."
                );
            }
        }

        static void Import(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("[ERROR] Usage: IMPORT <file>");
                return;
            }

            string filePath = string.Join(" ", parts, 1, parts.Length - 1);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[ERROR] File '{filePath}' not found.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                int successCount = 0;
                int failCount = 0;

                Console.WriteLine(
                    $"[INFO] Processing {lines.Length} lines from '{filePath}'..."
                );

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] fields = ParseInput(line);

                    
                    if (fields.Length < 4)
                    {
                        failCount++;
                        continue;
                    }

                    string name = fields[0];

                    if (!int.TryParse(fields[1], out int age) || age < 0)
                    {
                        failCount++;
                        continue;
                    }

                    string email = fields[2];
                    string password = fields[3];

                    if (TryInsert(
                        name, age, email, password, out string userId))
                    {
                        successCount++;
                        Console.WriteLine(
                            $"[IMPORTED] {name} -> {userId}"
                        );
                    }
                    else
                    {
                        failCount++;
                    }
                }

                Console.WriteLine("\n[IMPORT FINISHED]");
                Console.WriteLine($"Success: {successCount}");
                Console.WriteLine($"Failed / Ignored: {failCount}\n");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[ERROR] File operation failed: {ex.Message}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("[ERROR] Permission denied when reading the file.");
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("[ERROR] Could not load libengine.so.");
            }
            catch (EntryPointNotFoundException)
            {
                Console.WriteLine("[ERROR] A required C++ function was not found.");
            }
        }

        static void List()
        {
            Console.WriteLine("\nReading database...");

            int total = db__list();

            if (total == 0)
            {
                Console.WriteLine("[INFO] No records found, or the database could not be opened.");
            }
            else if (total > 0)
            {
                Console.WriteLine($"[INFO] Total records: {total}");
            }
        }

        static void Delete(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("[ERROR] Usage: DELETE <USERID>");
                return;
            }

            string targetUserId = parts[1];

            Console.Write(
                $"Are you sure you want to delete '{targetUserId}'? (y/n): "
            );

            string confirmation = Console.ReadLine()?.Trim() ?? "";

            if (!confirmation.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[CANCELLED] Delete operation aborted.");
                return;
            }

            int result = db__delete(targetUserId);

            if (result == 1)
            {
                Console.WriteLine(
                    $"[SUCCESS] User '{targetUserId}' deleted."
                );
            }
            else
            {
                Console.WriteLine(
                    $"[INFO] No user found with ID '{targetUserId}', " +
                    "or the operation failed."
                );
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("================================");
            Console.WriteLine("     SimpleDatabase v2.0");
            Console.WriteLine("================================");
            Console.WriteLine("Type LS to see available commands.");

            bool running = true;

            while (running)
            {
                Console.Write("db> ");

                string input = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] parts = ParseInput(input);
                string command = parts[0].ToLowerInvariant();

                try
                {
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
                            Console.WriteLine("Exiting SimpleDatabase. Goodbye!");
                            break;

                        default:
                            Console.WriteLine(
                                "[ERROR] Unknown command. Type LS for help."
                            );
                            break;
                    }
                }
                catch (DllNotFoundException)
                {
                    Console.WriteLine("[ERROR] Native library libengine.so is missing.");
                }
                catch (EntryPointNotFoundException ex)
                {
                    Console.WriteLine($"[ERROR] Native function missing: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message}");
                }
            }
        }
    }
}
