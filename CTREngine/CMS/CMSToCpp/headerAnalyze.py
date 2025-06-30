import re
import sys
import os

def extract_static_members_from_header(header_path):
    with open(header_path, 'r', encoding='utf-8') as f:
        code = f.read()

    namespace_stack = []
    class_stack = []
    output = []

    # Remove comments
    code = re.sub(r'//.*?$|/\*.*?\*/', '', code, flags=re.MULTILINE | re.DOTALL)

    # Simple tokenizer
    tokens = re.split(r'(\s+|{|})', code)

    i = 0
    while i < len(tokens):
        token = tokens[i]

        if token == 'namespace':
            i += 2
            ns = tokens[i].strip()
            if ns:
                namespace_stack.append(ns)

        elif token == 'class':
            i += 2
            cls = tokens[i].strip()
            if cls:
                class_stack.append(cls)

        elif token == '}':
            # Close class or namespace
            if class_stack:
                class_stack.pop()
            elif namespace_stack:
                namespace_stack.pop()

        elif token == 'static':
            lookahead = ''.join(tokens[i+1:i+6])
            # Check for static function
            match_func = re.search(r'\bstatic\s+\w[\w\s:*&<>,]*\s+(\w+)\s*\(', lookahead)
            # Check for static variable
            match_var = re.search(r'\bstatic\s+\w[\w\s:*&<>,]*\s+(\w+)\s*;', lookahead)

            if match_func:
                name = match_func.group(1)
                fq = '::'.join(namespace_stack + class_stack + [name])
                output.append(fq)
            elif match_var:
                name = match_var.group(1)
                fq = '::'.join(namespace_stack + class_stack + [name])
                output.append(fq)

        i += 1

    return output


if __name__ == "__main__":
    if len(sys.argv) != 2:
        sys.exit(1)

    header_file = sys.argv[1]

    if not os.path.isfile(header_file):
        sys.exit(1)

    results = extract_static_members_from_header(header_file)
    for line in results:
        print(line)
