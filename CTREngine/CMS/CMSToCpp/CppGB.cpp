#include <iostream>
#include <memory>  // For std::unique_ptr

int main() {
    for (int i = 0; i < 5; ++i) {
        std::unique_ptr<int> num = std::make_unique<int>(i);  // Allocating memory on the heap
        std::cout << "Number: " << *num << std::endl;
        // Memory will be automatically cleaned up when 'num' goes out of scope at the end of each loop iteration.
    }

    std::cout << "All memory cleaned up successfully!" << std::endl;
    return 0;
}
