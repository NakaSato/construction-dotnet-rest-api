#!/usr/bin/env python3
"""
Script to fix common controller error patterns in the .NET REST API project
"""

import os
import re
import glob

def fix_controller_patterns(file_path):
    """Fix common error patterns in controller files"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Pattern 1: Replace service result checking with ToApiResponse
    # Look for pattern: if (!result.Success) { return CreateErrorResponse/CreateNotFoundResponse... } return CreateSuccessResponse...
    pattern1 = re.compile(
        r'(\s+)(var result = await [^;]+;)\s*\n\s*if\s*\(!result\.Success\)\s*\{\s*\n\s*return\s+[^}]+;\s*\}\s*\n\s*return\s+CreateSuccessResponse\([^)]+\);',
        re.MULTILINE | re.DOTALL
    )
    
    def replace_pattern1(match):
        indent = match.group(1)
        service_call = match.group(2)
        return f"{indent}{service_call}\n{indent}return ToApiResponse(result);"
    
    content = pattern1.sub(replace_pattern1, content)
    
    # Pattern 2: Replace simple service result checking with ToApiResponse
    pattern2 = re.compile(
        r'(\s+)(var result = await [^;]+;)\s*\n\s*if\s*\(!result\.Success\)\s*\{\s*\n\s*return\s+[^}]+;\s*\}\s*\n\s*([^r][^e][^t][^u][^r][^n])',
        re.MULTILINE | re.DOTALL
    )
    
    def replace_pattern2(match):
        indent = match.group(1)
        service_call = match.group(2)
        next_line = match.group(3)
        return f"{indent}{service_call}\n{indent}return ToApiResponse(result);\n{indent}{next_line}"
    
    content = pattern2.sub(replace_pattern2, content)
    
    # Pattern 3: Fix CreateNotFoundResponse calls that need generic type parameter
    content = re.sub(
        r'CreateNotFoundResponse\(([^)]*)\)',
        lambda m: f'CreateNotFoundResponse<object>({m.group(1)} ?? "Resource not found")',
        content
    )
    
    # Pattern 4: Fix CreateErrorResponse calls that need generic type parameter
    content = re.sub(
        r'CreateErrorResponse\(([^,)]*),?\s*(\d+)?\)',
        lambda m: f'CreateErrorResponse<object>({m.group(1)} ?? "Operation failed", {m.group(2) if m.group(2) else "400"})',
        content
    )
    
    # Only write if content changed
    if content != original_content:
        with open(file_path, 'w') as f:
            f.write(content)
        print(f"Fixed patterns in {file_path}")
        return True
    return False

def main():
    """Main function to fix all controller files"""
    controller_files = glob.glob('/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/*.cs')
    
    fixed_count = 0
    for file_path in controller_files:
        if fix_controller_patterns(file_path):
            fixed_count += 1
    
    print(f"Fixed patterns in {fixed_count} files")

if __name__ == "__main__":
    main()
