#include <iostream>
#include <fstream>
#include <cstring>


extern "C" {

    struct UserRecord {
        char userId[50];
        char name[100];
        int age;
        char email[100];
        char password[100];
    };
    
    int db_insert(const char* userId, const char* name, int age, const char* email, const char* password) {
        std::ofstream file("database.db", std::ios::binary | std::ios::app);
        
        if (!file.is_open()) {
            return 0; 
        }

        UserRecord record;

        std::memset(&record, 0, sizeof(UserRecord));

        std::strncpy(record.userId, userId, sizeof(record.userId) - 1);
        std::strncpy(record.name, name, sizeof(record.name) -1);
        record.age = age;
        std::strncpy(record.email, email, sizeof(record.email) - 1);
        std::strncpy(record.password, password, sizeof(record.password)- 1);
        file.write(reinterpret_cast<const char*>(&record), sizeof(UserRecord));

        file.close();
        return 1; 
    }

    int db__get(const char* targetUserId,  char* outName, int* outAge,  char* outEmail, char* outPassword){
        std::ifstream file("database.db", std::ios::binary);
        
        if (!file.is_open()){
            return 0;
        }

        UserRecord record;

        while (file.read(reinterpret_cast<char*>(&record), sizeof(UserRecord))){
            if (std::strcmp(record.userId, targetUserId) == 0){
                std::strcpy(outName, record.name);
                *outAge = record.age;
                std::strcpy(outEmail, record.email);
                std::strcpy(outPassword, record.password);
                file.close();
                return 1;
                
            }

        }
        file.close();

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
    
    
