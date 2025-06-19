#!/usr/bin/env python3
"""
Bulk fix script for common controller patterns that cause type conversion errors
"""

import os
import re
import glob

def bulk_fix_controller_patterns(file_path):
    """Apply bulk fixes to common patterns causing type conversion errors"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Fix 1: Replace all standalone service result calls with ToApiResponse
    # Pattern: var result = await service.Method(); if (!result.Success) { return Error... } return Success...
    pattern1 = re.compile(
        r'(\s+var result = await [^;]+;)\s*\n\s*if\s*\(\s*!result\.Success\s*\)\s*\n\s*return\s+[^;]+;\s*\n\s*return\s+[^;]+;',
        re.MULTILINE | re.DOTALL
    )
    
    def replace_with_toapi(match):
        service_call = match.group(1)
        return f"{service_call}\n            return ToApiResponse(result);"
    
    content = pattern1.sub(replace_with_toapi, content)
    
    # Fix 2: Replace manual error handling patterns with ToApiResponse
    pattern2 = re.compile(
        r'(\s+var result = await [^;]+;)\s*\n\s*if\s*\(\s*!result\.Success\s*\)\s*\{[^}]*\}\s*\n\s*return\s+[^;]+;',
        re.MULTILINE | re.DOTALL
    )
    
    content = pattern2.sub(replace_with_toapi, content)
    
    # Fix 3: Replace remaining CreateErrorResponse<object> and CreateNotFoundResponse<object> patterns
    # by extracting the correct type from method signature
    
    # Find method signatures and their return types
    method_pattern = re.compile(
        r'public async Task<ActionResult<ApiResponse<([^>]+(?:<[^>]+>)?)>>>[^{]*\{',
        re.MULTILINE
    )
    
    methods = method_pattern.findall(content)
    
    # For each unique return type found, replace the error patterns within the file
    for return_type in set(methods):
        # Replace CreateErrorResponse<object> with correct type
        content = re.sub(
            r'CreateErrorResponse<object>\(([^)]+)\)',
            f'BadRequest(new ApiResponse<{return_type}> {{ Success = false, Message = \\1 }})',
            content
        )
        
        # Replace CreateNotFoundResponse<object> with correct type  
        content = re.sub(
            r'CreateNotFoundResponse<object>\(([^)]+)\)',
            f'NotFound(new ApiResponse<{return_type}> {{ Success = false, Message = \\1 }})',
            content
        )
    
    # Fix 4: Handle non-generic return types (methods that don't return ApiResponse<T>)
    non_generic_pattern = re.compile(
        r'public async Task<ActionResult<([^>]+(?:<[^>]+>)?)>>[^{]*\{',
        re.MULTILINE
    )
    
    non_generic_methods = non_generic_pattern.findall(content)
    
    for return_type in set(non_generic_methods):
        if 'ApiResponse' not in return_type:
            # These methods should use direct service result returns
            content = re.sub(
                rf'return CreateSuccessResponse\([^;]+;',
                'return ToApiResponse(result);',
                content
            )
    
    # Fix 5: Handle HandleException return type mismatches
    content = re.sub(
        r'return HandleException\(_logger, ex, ([^)]+)\);',
        r'return HandleException<object>(_logger, ex, \1);',
        content
    )
    
    # Fix 6: Handle validation result return type mismatches
    content = re.sub(
        r'return validationResult;',
        r'return BadRequest(new { Success = false, Message = validationResult });',
        content
    )
    
    # Only write if content changed
    if content != original_content:
        with open(file_path, 'w') as f:
            f.write(content)
        print(f"Applied bulk fixes to {file_path}")
        return True
    return False

def main():
    """Main function to apply bulk fixes to all controller files"""
    controller_files = glob.glob('/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/*.cs')
    
    fixed_count = 0
    for file_path in controller_files:
        if bulk_fix_controller_patterns(file_path):
            fixed_count += 1
    
    print(f"Applied bulk fixes to {fixed_count} files")

if __name__ == "__main__":
    main()
