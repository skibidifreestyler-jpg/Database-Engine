#include <iostream>
#include <fstream>
#include <cstring>
#include <cstdio>

extern "C" {

    struct UserRecord {
        char userId[50];
        char name[100];
        int age;
        char email[100];
        char password[100];
    };
    
    

int db_insert(
    const char* name,
    int age,
    const char* email,
    const char* password,
    char* outUserId,
    int outUserIdCapacity
) {
    if (outUserId == nullptr || outUserIdCapacity < 11)
        return 0;

    int nextId = 1;

    
    {
        std::ifstream inFile("database.db", std::ios::binary);
        UserRecord existing{};

        while (inFile.read(
            reinterpret_cast<char*>(&existing),
            sizeof(UserRecord))) {

            int id = 0;

            if (std::sscanf(existing.userId, "USR-%d", &id) == 1
                && id >= nextId) {
                nextId = id + 1;
            }
        }
    }

    char generatedId[50];
    std::snprintf(
        generatedId,
        sizeof(generatedId),
        "USR-%06d",
        nextId
    );

    UserRecord record{};
    std::strncpy(record.userId, generatedId,
                 sizeof(record.userId) - 1);
    std::strncpy(record.name, name, sizeof(record.name) - 1);
    record.age = age;
    std::strncpy(record.email, email, sizeof(record.email) - 1);
    std::strncpy(record.password, password,
                 sizeof(record.password) - 1);

    std::ofstream file("database.db",
                       std::ios::binary | std::ios::app);

    if (!file.is_open())
        return 0;

    file.write(reinterpret_cast<const char*>(&record),
               sizeof(UserRecord));

    if (!file)
        return 0;

    std::strncpy(outUserId, generatedId, outUserIdCapacity - 1);
    outUserId[outUserIdCapacity - 1] = '\0';

    return 1;
}

    
int db__get(
    const char* targetUserId,
    char* outName, int nameCapacity,
    int* outAge,
    char* outEmail, int emailCapacity,
    char* outPassword, int passwordCapacity
) {
    if (targetUserId == nullptr ||
        outName == nullptr || nameCapacity <= 0 ||
        outAge == nullptr ||
        outEmail == nullptr || emailCapacity <= 0 ||
        outPassword == nullptr || passwordCapacity <= 0) {
        return 0;
    }

    std::ifstream file("database.db", std::ios::binary);

    if (!file.is_open())
        return 0;

    UserRecord record{};

    while (file.read(
        reinterpret_cast<char*>(&record),
        sizeof(UserRecord))) {

        if (std::strcmp(record.userId, targetUserId) == 0) {
            // Copy only what fits, and always terminate the strings.
            std::strncpy(outName, record.name, nameCapacity - 1);
            outName[nameCapacity - 1] = '\0';

            *outAge = record.age;

            std::strncpy(outEmail, record.email, emailCapacity - 1);
            outEmail[emailCapacity - 1] = '\0';

            std::strncpy(
                outPassword,
                record.password,
                passwordCapacity - 1
            );
            outPassword[passwordCapacity - 1] = '\0';

            return 1;
        }
    }

    return 0;
}


    int db__list(){
        std::ifstream file("database.db", std::ios::binary);
        if (!file.is_open()){
            return 0;
        }
        UserRecord record;
        int count = 0;

       std::cout << "\n--- Conținut Bază de Date (C++) ---" << std::endl;
        while (file.read(reinterpret_cast<char*>(&record), sizeof(UserRecord))) {
            std::cout << "  [ID]: " << record.userId 
                      << " | [Nume]: " << record.name 
                      << " | [Vârstă]: " << record.age 
                      << " | [Email]: " << record.email << std::endl;
            count++;
        }

        file.close();
        std::cout << "-----------------------------------" << std::endl;
        return count;

    }

    int db__delete(const char* targetUserId){
        std::ifstream inFile("database.db", std::ios::binary);
        if (!inFile.is_open()){
            return 0;
        }

        std::ofstream outFile("temp.db", std::ios::binary);
        if (!outFile.is_open()){
            inFile.close();
            return 0;
        }

        UserRecord record;
        bool found = false;
        while (inFile.read(reinterpret_cast<char*>(&record), sizeof(UserRecord))) {
            if (std::strcmp(record.userId, targetUserId) == 0) {
                found = true; 
            } else {
                outFile.write(reinterpret_cast<const char*>(&record), sizeof(UserRecord));
            }
        }

        inFile.close();
        outFile.close();

        
        std::remove("database.db");
        std::rename("temp.db", "database.db");

        return found ? 1 : 0;
    }
    }
    
    

    
