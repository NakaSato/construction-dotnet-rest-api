#!/usr/bin/env python3
"""
Fix remaining malformed generic types in ApiResponse constructors
"""

import os
import re
import glob

def fix_remaining_syntax_errors(file_path):
    """Fix remaining malformed generic types"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Fix patterns like: new ApiResponse<Type<SubType> { ... }
    # Should be: new ApiResponse<Type<SubType>> { ... }
    content = re.sub(
        r'new ApiResponse<([^<>]+<[^<>]+) \{',
        r'new ApiResponse<\1> {',
        content
    )
    
    # Also fix patterns where we have complex nested generics incorrectly formatted
    # Pattern: new ApiResponse<PagedResult<SomeDto> { ... }
    # Should be: new ApiResponse<PagedResult<SomeDto>> { ... }
    content = re.sub(
        r'new ApiResponse<PagedResult<([^<>]+) \{',
        r'new ApiResponse<PagedResult<\1>> {',
        content
    )
    
    # Pattern: new ApiResponse<EnhancedPagedResult<SomeDto> { ... }
    # Should be: new ApiResponse<EnhancedPagedResult<SomeDto>> { ... }
    content = re.sub(
        r'new ApiResponse<EnhancedPagedResult<([^<>]+) \{',
        r'new ApiResponse<EnhancedPagedResult<\1>> {',
        content
    )
    
    # Pattern: new ApiResponse<ApiResponse<SomeDto> { ... }
    # Should be: new ApiResponse<ApiResponse<SomeDto>> { ... }
    content = re.sub(
        r'new ApiResponse<ApiResponse<([^<>]+) \{',
        r'new ApiResponse<ApiResponse<\1>> {',
        content
    )
    
    # Fix HandleException calls to use correct generic types
    content = re.sub(
        r'HandleException<object>\(_logger, ex, ([^)]+)\)',
        r'HandleException(_logger, ex, \1)',
        content
    )
    
    # Only write if content changed
    if content != original_content:
        with open(file_path, 'w') as f:
            f.write(content)
        print(f"Fixed remaining syntax errors in {file_path}")
        return True
    return False

def main():
    """Main function to fix remaining syntax errors in all controller files"""
    controller_files = glob.glob('/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/*.cs')
    
    fixed_count = 0
    for file_path in controller_files:
        if fix_remaining_syntax_errors(file_path):
            fixed_count += 1
    
    print(f"Fixed remaining syntax errors in {fixed_count} files")

if __name__ == "__main__":
    main()
