// ============================
// Comprehensive C++ Example
// ============================

#include <iostream>
#include <string>
#include <vector>
#include <map>
#include <fstream>
#include <stdexcept>

using namespace std;

// Namespace
namespace MathTools {
    int Add(int a, int b) {
        return a + b;
    }
}

// Base Class
class Animal {
public:
    string name;
    Animal(const string& name) : name(name) {}
    virtual void Speak() const {
        cout << name << " makes a noise." << endl;
    }
};

// Derived Class (Inheritance)
class Dog : public Animal {
public:
    Dog(const string& name) : Animal(name) {}
    void Speak() const override {
        cout << name << " barks." << endl;
    }
};

// Template Function
template<typename T>
T Multiply(T a, T b) {
    return a * b;
}

int main() {
    // Variables
    int age = 25;
    double height = 5.9;
    string name = "Alice";
    bool isActive = true;

    // Pointers and References
    int* agePtr = &age;
    int& ageRef = age;

    // Arrays & Vectors
    int numbers[3] = {1, 2, 3};
    vector<string> names = {"Alice", "Bob", "Charlie"};

    // Maps
    map<string, int> nameToAge = { {"Alice", 25}, {"Bob", 30} };

    // Using Namespace
    int sum = MathTools::Add(3, 4);
    cout << "Sum using namespace function: " << sum << endl;

    // Classes & Polymorphism
    Animal* a = new Animal("Some Animal");
    Animal* d = new Dog("Rex");

    a->Speak();
    d->Speak();

    delete a;
    delete d;

    // Template Function Usage
    cout << "Multiplication of integers: " << Multiply(5, 6) << endl;
    cout << "Multiplication of doubles: " << Multiply(3.14, 2.0) << endl;

    // Control Structures
    if (age > 18) {
        cout << name << " is an adult." << endl;
    } else {
        cout << name << " is not an adult." << endl;
    }

    for (int num : numbers) {
        cout << "Number: " << num << endl;
    }

    for (auto& entry : nameToAge) {
        cout << entry.first << " is " << entry.second << " years old." << endl;
    }

    // File I/O
    ofstream file("example.txt");
    file << "Writing to a file." << endl;
    file.close();

    ifstream readFile("example.txt");
    string line;
    if (getline(readFile, line)) {
        cout << "Read from file: " << line << endl;
    }
    readFile.close();

    // Exception Handling
    try {
        if (name.empty()) {
            throw runtime_error("Name cannot be empty.");
        }
    } catch (const exception& e) {
        cout << "Exception caught: " << e.what() << endl;
    }

    return 0;
}
