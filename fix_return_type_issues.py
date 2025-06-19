#!/usr/bin/env python3

import os
import re
import glob

def fix_controller_return_types():
    """Fix common return type conversion issues in controllers"""
    
    # Find all controller files
    controller_files = glob.glob("/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/*.cs")
    
    changes_made = 0
    
    for file_path in controller_files:
        print(f"\nProcessing {os.path.basename(file_path)}...")
        
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            original_content = content
            
            # Pattern 1: Fix IActionResult return type issues
            # Fix validation returns
            content = re.sub(
                r'return BadRequest\(new \{ Success = false, Message = validationResult \}\);',
                'return BadRequest(CreateErrorResponse(validationResult));',
                content
            )
            
            # Fix other BadRequest patterns
            content = re.sub(
                r'return BadRequest\(new \{ Success = false, Message = "([^"]+)" \}\);',
                r'return BadRequest(CreateErrorResponse("\1"));',
                content
            )
            
            # Fix Ok patterns with ApiResponse
            content = re.sub(
                r'return Ok\(new ApiResponse<([^>]+)>\s*\{\s*Success\s*=\s*true,\s*Data\s*=\s*([^}]+)\s*\}\);',
                r'return ToApiResponse(ServiceResult<\1>.Success(\2));',
                content
            )
            
            # Write back if changes were made
            if content != original_content:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                changes_made += 1
                print(f"  ✓ Fixed return type issues in {os.path.basename(file_path)}")
            else:
                print(f"  - No changes needed for {os.path.basename(file_path)}")
                
        except Exception as e:
            print(f"  ✗ Error processing {file_path}: {e}")
    
    print(f"\n=== SUMMARY ===")
    print(f"Files processed: {len(controller_files)}")
    print(f"Files changed: {changes_made}")

if __name__ == "__main__":
    fix_controller_return_types()
