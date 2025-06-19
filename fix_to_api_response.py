#!/usr/bin/env python3
"""
Script to fix ToApiResponse type conversion errors
"""

import os
import re
from pathlib import Path

def fix_to_api_response_calls(file_path):
    """Fix ToApiResponse<object> calls to just ToApiResponse"""
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Replace ToApiResponse<object> with ToApiResponse
    content = re.sub(r'ToApiResponse<object>\(', r'ToApiResponse(', content)
    
    # Fix HandleException calls that are returning wrong types
    content = re.sub(r'HandleException<object>\(', r'HandleException(', content)
    
    # Write back if changed
    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Fixed ToApiResponse calls in {file_path}")

def main():
    """Main function to fix ToApiResponse errors"""
    
    # List of controller files to fix
    controller_files = [
        "Controllers/V1/AuthController.cs",
        "Controllers/V1/CalendarController.cs", 
        "Controllers/V1/DailyReportsController.cs",
        "Controllers/V1/DocumentsController.cs",
        "Controllers/V1/ImagesController.cs",
        "Controllers/V1/MasterPlansController.cs",
        "Controllers/V1/PhasesController.cs",
        "Controllers/V1/ProjectsController.cs",
        "Controllers/V1/ResourcesController.cs",
        "Controllers/V1/TasksController.cs",
        "Controllers/V1/UsersController.cs",
        "Controllers/V1/WorkRequestsController.cs",
    ]
    
    print("Fixing ToApiResponse type conversion errors...")
    
    for controller_file in controller_files:
        if os.path.exists(controller_file):
            fix_to_api_response_calls(controller_file)
    
    print("All ToApiResponse fixes applied!")

if __name__ == "__main__":
    main()
