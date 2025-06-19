#!/usr/bin/env python3
"""
Fix specific syntax errors in controller files
"""

import os
import re
import glob

def fix_syntax_errors(file_path):
    """Fix specific syntax errors caused by bulk replacements"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Fix malformed ApiResponse constructor calls
    # Pattern: BadRequest(new ApiResponse<Type { Success = false, Message = "msg" ?? "fallback", statusCode });
    pattern1 = re.compile(
        r'BadRequest\(new ApiResponse<([^{]+) \{ Success = false, Message = "([^"]+)" \?\? "[^"]+", \d+ \}\)',
        re.MULTILINE
    )
    
    def fix_badrequest(match):
        return_type = match.group(1).strip()
        message = match.group(2)
        return f'BadRequest(new ApiResponse<{return_type}> {{ Success = false, Message = "{message}" }})'
    
    content = pattern1.sub(fix_badrequest, content)
    
    # Fix malformed StatusCode calls
    # Pattern: StatusCode(500, BadRequest(new ApiResponse<Type { Success = false, Message = "msg" ?? "fallback", statusCode }));
    pattern2 = re.compile(
        r'StatusCode\(\d+, BadRequest\(new ApiResponse<([^{]+) \{ Success = false, Message = "([^"]+)" \?\? "[^"]+", \d+ \}\)\)',
        re.MULTILINE
    )
    
    def fix_statuscode(match):
        return_type = match.group(1).strip()
        message = match.group(2)
        return f'StatusCode(500, new ApiResponse<{return_type}> {{ Success = false, Message = "{message}" }})'
    
    content = pattern2.sub(fix_statuscode, content)
    
    # Fix malformed NotFound calls
    # Pattern: NotFound(new ApiResponse<Type { Success = false, Message = "msg" ?? "fallback", statusCode });
    pattern3 = re.compile(
        r'NotFound\(new ApiResponse<([^{]+) \{ Success = false, Message = "([^"]+)" \?\? "[^"]+", \d+ \}\)',
        re.MULTILINE
    )
    
    def fix_notfound(match):
        return_type = match.group(1).strip()
        message = match.group(2)
        return f'NotFound(new ApiResponse<{return_type}> {{ Success = false, Message = "{message}" }})'
    
    content = pattern3.sub(fix_notfound, content)
    
    # Fix complex generic types that may have been broken
    # Pattern: ApiResponse<PagedResult<SomeDto> where the > is missing
    content = re.sub(
        r'ApiResponse<([^<>]+<[^<>]+) \{',
        r'ApiResponse<\1> {',
        content
    )
    
    # Only write if content changed
    if content != original_content:
        with open(file_path, 'w') as f:
            f.write(content)
        print(f"Fixed syntax errors in {file_path}")
        return True
    return False

def main():
    """Main function to fix syntax errors in all controller files"""
    controller_files = glob.glob('/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/*.cs')
    
    fixed_count = 0
    for file_path in controller_files:
        if fix_syntax_errors(file_path):
            fixed_count += 1
    
    print(f"Fixed syntax errors in {fixed_count} files")

if __name__ == "__main__":
    main()
