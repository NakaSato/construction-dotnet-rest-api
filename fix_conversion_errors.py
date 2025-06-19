#!/usr/bin/env python3

import os
import re

def fix_conversion_errors():
    """Fix IActionResult to ActionResult<T> conversion issues"""
    
    # Define the files and their specific issues
    fixes = {
        "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/UsersController.cs": [
            ("return HandleException(_logger, ex, \"updating user\");", "return HandleException<UserDto>(_logger, ex, \"updating user\");"),
            ("return HandleException(_logger, ex, \"deleting user\");", "return HandleException<bool>(_logger, ex, \"deleting user\");"),
            ("return HandleException(_logger, ex, \"activating user\");", "return HandleException<bool>(_logger, ex, \"activating user\");"),
            ("return HandleException(_logger, ex, \"deactivating user\");", "return HandleException<bool>(_logger, ex, \"deactivating user\");"),
            ("return HandleException(_logger, ex, \"retrieving users with rich pagination\");", "return HandleException<EnhancedPagedResult<UserDto>>(_logger, ex, \"retrieving users with rich pagination\");"),
        ],
        "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/ResourcesController.cs": [
            ("return HandleException(_logger, ex, \"retrieving resources\");", "return HandleException<EnhancedPagedResult<ResourceDto>>(_logger, ex, \"retrieving resources\");"),
            ("return HandleException(_logger, ex, $\"retrieving resource {id}\");", "return HandleException<ResourceDto>(_logger, ex, $\"retrieving resource {id}\");"),
            ("return HandleException(_logger, ex, \"creating resource\");", "return HandleException<ResourceDto>(_logger, ex, \"creating resource\");"),
            ("return HandleException(_logger, ex, $\"updating resource {id}\");", "return HandleException<ResourceDto>(_logger, ex, $\"updating resource {id}\");"),
            ("return HandleException(_logger, ex, $\"deleting resource {id}\");", "return HandleException<bool>(_logger, ex, $\"deleting resource {id}\");"),
        ],
        "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/DocumentsController.cs": [
            ("return HandleException(_logger, ex, \"retrieving documents\");", "return HandleException<EnhancedPagedResult<DocumentDto>>(_logger, ex, \"retrieving documents\");"),
            ("return HandleException(_logger, ex, $\"retrieving document {id}\");", "return HandleException<DocumentDto>(_logger, ex, $\"retrieving document {id}\");"),
            ("return HandleException(_logger, ex, \"creating document\");", "return HandleException<DocumentDto>(_logger, ex, \"creating document\");"),
            ("return HandleException(_logger, ex, $\"updating document {id}\");", "return HandleException<DocumentDto>(_logger, ex, $\"updating document {id}\");"),
            ("return HandleException(_logger, ex, $\"deleting document {id}\");", "return HandleException<bool>(_logger, ex, $\"deleting document {id}\");"),
        ],
        "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/AuthController.cs": [
            ("return HandleException(_logger, ex, \"user login\");", "return HandleException<LoginResponse>(_logger, ex, \"user login\");"),
            ("return HandleException(_logger, ex, \"user registration\");", "return HandleException<UserDto>(_logger, ex, \"user registration\");"),
            ("return HandleException(_logger, ex, \"user logout\");", "return HandleException<string>(_logger, ex, \"user logout\");"),
        ],
        "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/ImagesController.cs": [
            ("return HandleException(_logger, ex, \"uploading image\");", "return HandleException<ImageMetadataDto>(_logger, ex, \"uploading image\");"),
            ("return HandleException(_logger, ex, \"getting image metadata\");", "return HandleException<ImageMetadataDto>(_logger, ex, \"getting image metadata\");"),
            ("return HandleException(_logger, ex, \"serving image\");", "return HandleException<string>(_logger, ex, \"serving image\");"),
            ("return HandleException(_logger, ex, \"bulk upload\");", "return HandleException<object>(_logger, ex, \"bulk upload\");"),
            ("return HandleException(_logger, ex, \"bulk metadata upload\");", "return HandleException<object>(_logger, ex, \"bulk metadata upload\");"),
            ("return HandleException(_logger, ex, \"retrieving images\");", "return HandleException<EnhancedPagedResult<ImageMetadataDto>>(_logger, ex, \"retrieving images\");"),
            ("return HandleException(_logger, ex, \"retrieving images with rich pagination\");", "return HandleException<ApiResponseWithPagination<ImageMetadataDto>>(_logger, ex, \"retrieving images with rich pagination\");"),
        ],
    }
    
    changes_made = 0
    
    for file_path, file_fixes in fixes.items():
        if not os.path.exists(file_path):
            print(f"File not found: {file_path}")
            continue
            
        print(f"Processing {os.path.basename(file_path)}...")
        
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            original_content = content
            
            for old_text, new_text in file_fixes:
                content = content.replace(old_text, new_text)
            
            if content != original_content:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                changes_made += 1
                print(f"  ✓ Fixed {os.path.basename(file_path)}")
            else:
                print(f"  - No changes needed for {os.path.basename(file_path)}")
                
        except Exception as e:
            print(f"  ✗ Error processing {file_path}: {e}")
    
    print(f"\n=== SUMMARY ===")
    print(f"Files changed: {changes_made}")

if __name__ == "__main__":
    fix_conversion_errors()
