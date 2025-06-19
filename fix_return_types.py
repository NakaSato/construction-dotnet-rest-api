#!/usr/bin/env python3
"""
Script to fix controller return types based on method signatures
"""

import os
import re
import glob

def extract_return_type(method_signature):
    """Extract the data type from ActionResult<ApiResponse<T>>"""
    match = re.search(r'ActionResult<ApiResponse<([^>]+(?:<[^>]+>)?)>>', method_signature)
    if match:
        return match.group(1)
    return "object"

def fix_specific_controller_errors(file_path):
    """Fix specific controller type errors"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Find method signatures and fix CreateErrorResponse/CreateNotFoundResponse within those methods
    method_pattern = re.compile(
        r'(public async Task<ActionResult<ApiResponse<([^>]+(?:<[^>]+>)?)>>>[^{]*{[^}]*?})',
        re.MULTILINE | re.DOTALL
    )
    
    def fix_method_returns(match):
        method_body = match.group(1)
        return_type = match.group(2)
        
        # Fix CreateErrorResponse<object> to CreateErrorResponse<actual_type>
        method_body = re.sub(
            r'CreateErrorResponse<object>',
            f'CreateErrorResponse<{return_type}>',
            method_body
        )
        
        # Fix CreateNotFoundResponse<object> to CreateNotFoundResponse<actual_type>
        method_body = re.sub(
            r'CreateNotFoundResponse<object>',
            f'CreateNotFoundResponse<{return_type}>',
            method_body
        )
        
        return method_body
    
    content = method_pattern.sub(fix_method_returns, content)
    
    # Additional specific fixes for non-generic methods
    non_generic_pattern = re.compile(
        r'(public async Task<ActionResult<([^>]+(?:<[^>]+>)?)>>[^{]*{[^}]*?})',
        re.MULTILINE | re.DOTALL
    )
    
    def fix_non_generic_method_returns(match):
        method_body = match.group(1)
        return_type = match.group(2)
        
        # If it's not wrapped in ApiResponse, it should return ToApiResponse(result)
        if 'ApiResponse' not in return_type:
            # Replace CreateSuccessResponse patterns
            method_body = re.sub(
                r'return CreateSuccessResponse\([^;]+;',
                'return ToApiResponse(result);',
                method_body
            )
        
        return method_body
    
    content = non_generic_pattern.sub(fix_non_generic_method_returns, content)
    
    # Only write if content changed
    if content != original_content:
        with open(file_path, 'w') as f:
            f.write(content)
        print(f"Fixed return types in {file_path}")
        return True
    return False

def main():
    """Main function to fix controller return types"""
    controller_files = glob.glob('/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/*.cs')
    
    fixed_count = 0
    for file_path in controller_files:
        if fix_specific_controller_errors(file_path):
            fixed_count += 1
    
    print(f"Fixed return types in {fixed_count} files")

if __name__ == "__main__":
    main()
