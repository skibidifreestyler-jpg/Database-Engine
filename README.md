[project_readme.md](https://github.com/user-attachments/files/33899627/project_readme.md)
# 🚀 SimpleDatabase v1.0

A high-performance, lightweight **hybrid database system** built with a **C++ native storage engine** and orchestrated via a **C# Command-Line Interface (CLI)**. 

Designed for speed, low-level binary manipulation, and zero-bloat data storage! ⚡

---

## 🛠️ Architecture & Tech Stack

* **Core Engine (C++):** Handles low-level binary file I/O, fixed-size record formatting, and direct disk operations using `std::fstream` and pointers.
* **CLI Orchestrator (C#):** Provides an interactive terminal UI, argument parsing, and seamless Native Interoperability (**P/Invoke / `[DllImport]`**).
* **Storage Strategy:** Fixed-size binary blocks (`UserRecord` struct) ensuring predictable data offsets and fast lookups.

---

## 📂 Project Structure

```text
proiectnou/
│
├── engine.cpp          # C++ native binary storage engine and logic
├── libengine.so        # Compiled shared library (generated via g++)
├── Program.cs          # C# CLI orchestrator and P/Invoke bindings
├── [project].csproj    # .NET project configuration with auto-copy setup
└── database.db         # Binary database file (auto-created on first insert)
```

---

## ⚙️ Prerequisites

Make sure you have the following installed on your Linux system:
* **GCC / G++** (with support for C++11 or newer)
* **.NET SDK** (v10.0 or compatible)

---

## 🛠️ Build & Installation Guide

1. **Navigate to the project folder:**
   ```bash
   cd /home/vic/proiectnou/
   ```

2. **Compile the C++ Engine:**
   Compile `engine.cpp` into a shared library (`libengine.so`):
   ```bash
   g++ -shared -fPIC engine.cpp -o libengine.so
   ```

3. **Configure Automatic Library Deployment (Recommended):**
   Ensure your `.csproj` file includes the auto-copy rule so .NET can locate `libengine.so` at runtime:
   ```xml
   <ItemGroup>
     <None Update="libengine.so">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </None>
   </ItemGroup>
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```

---

## 🕹️ Available Commands

Once the application is running, type `ls` to view the available commands in the interactive shell (`db>`):

| Command | Syntax Example | Description |
| :--- | :--- | :--- |
| **`INSERT`** | `INSERT Victor 14 victor@email.com USR001 password123` | Adds a new user directly into the binary database. |
| **`GET`** | `GET USR001` | Searches and retrieves user details by their unique User ID. |
| **`IMPORT`** | `IMPORT users.log` | Bulk imports user records line-by-line from an external text/log file. |
| **`LIST`** | `LIST` | Dumps and lists all user records currently stored in the database. |
| **`DELETE`** | `DELETE USR001` | Safely removes a user using a temporary file-swap rewrite technique. |
| **`LS`** | `ls` | Displays the help menu with all commands. |
| **`EXIT`** | `EXIT` | Closes the database session safely. |

---

## 📝 File Import Format (`users.log`)

When using the `IMPORT` command, your source log or text file should be structured with space-separated fields per line:

```text
Name Age Email UserId Password
Andrei 25 andrei@email.com USR002 securepass
Ioana 22 ioana@email.com USR003 anotherpass
```

---

## 💡 How It Works Under the Hood

* **Fixed-Size Records:** Every user record is packed into a rigid 354-byte binary structure (`UserRecord`), containing fixed character arrays for ID, Name, Email, Password, and a 4-byte integer for Age.
* **Safe Deletions:** Deleting a record bypasses slow in-place modifications by streaming valid records into a temporary `temp.db` file, deleting the old database, and performing an atomic rename.

---

## 👨‍💻 Author

Built as an experimental hybrid systems-programming project. 🚀
