#ifndef MYCLASS_H
#define MYCLASS_H

// Includes
#include <string>
#include <vector>
#include <iostream>

// Macros
#define PI 3.14159
#define MAX(a, b) ((a) > (b) ? (a) : (b))

// Forward declaration
class AnotherClass;

namespace MyNamespace {

// Enum
enum class Status {
    Success,
    Failure,
    Unknown
};

// Struct
struct Point {
    int x;
    int y;
};

// Class
class MyClass {
public:
    // Constructors
    MyClass();
    MyClass(int value);

    // Destructor
    ~MyClass();

    // Member functions
    void Print() const;
    int GetValue() const;

    // Static member function
    static std::string GetClassName();

    // Inline function
    inline bool IsPositive() const { return value > 0; }

    // Getter and Setter
    void SetValue(int v);
    int GetValueSquared() const;

private:
    // Private member variable
    int value;

    // Static member
    static int instanceCount;

    // Private function
    void LogInternalState() const;
};

} // namespace MyNamespace

#endif // MYCLASS_H
