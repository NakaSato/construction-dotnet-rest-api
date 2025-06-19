#!/usr/bin/env python3
"""
Fix malformed ApiResponse constructor syntax
"""

import os
import re
import glob

def fix_malformed_constructors(file_path):
    """Fix malformed ApiResponse constructor calls"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Fix pattern: new ApiResponse<Type>> { ... }
    # Should be: new ApiResponse<Type> { ... }
    content = re.sub(
        r'new ApiResponse<([^>]+)>> \{',
        r'new ApiResponse<\1> {',
        content
    )
    
    # Fix patterns where the entire constructor is malformed
    # Pattern: BadRequest(new ApiResponse<Type>> { Success = false, Message = "msg" });
    content = re.sub(
        r'BadRequest\(new ApiResponse<([^>]+)>> \{ Success = false, Message = "([^"]+)" \}\)',
        r'BadRequest(new ApiResponse<\1> { Success = false, Message = "\2" })',
        content
    )
    
    # Fix patterns where NotFound has same issue
    content = re.sub(
        r'NotFound\(new ApiResponse<([^>]+)>> \{ Success = false, Message = "([^"]+)" \}\)',
        r'NotFound(new ApiResponse<\1> { Success = false, Message = "\2" })',
        content
    )
    
    # Fix patterns where StatusCode has same issue
    content = re.sub(
        r'StatusCode\(\d+, new ApiResponse<([^>]+)>> \{ Success = false, Message = "([^"]+)" \}\)',
        r'StatusCode(500, new ApiResponse<\1> { Success = false, Message = "\2" })',
        content
    )
    
    # Only write if content changed
    if content != original_content:
        with open(file_path, 'w') as f:
            f.write(content)
        print(f"Fixed malformed constructors in {file_path}")
        return True
    return False

def main():
    """Main function to fix malformed constructors in all controller files"""
    controller_files = glob.glob('/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/*.cs')
    
    fixed_count = 0
    for file_path in controller_files:
        if fix_malformed_constructors(file_path):
            fixed_count += 1
    
    print(f"Fixed malformed constructors in {fixed_count} files")

if __name__ == "__main__":
    main()
