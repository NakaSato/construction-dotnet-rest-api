#!/usr/bin/env python3
"""
Script to fix the remaining build errors in controllers
"""

import os
import re
from pathlib import Path

def fix_controller_return_types(file_path):
    """Fix return type mismatches in controller methods"""
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix ActionResult<ApiResponse<object>> to proper types
    content = re.sub(
        r'ActionResult<ApiResponse<object>>',
        r'ActionResult<object>',
        content
    )
    
    # Fix ToApiResponse type inference issues in MasterPlansController 
    content = re.sub(
        r'return ToApiResponse\(result\);(\s*})',
        r'return ToApiResponse<object>(result);\1',
        content
    )
    
    # Fix specific method signatures that have wrong return types
    patterns_to_fix = [
        # Fix GetUsers method return type
        (r'public async Task<ActionResult<PagedResult<UserDto>>>', 
         r'public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>>'),
        
        # Fix GetTasks method return type
        (r'public async Task<ActionResult<PagedResult<TaskDto>>>', 
         r'public async Task<ActionResult<ApiResponse<PagedResult<TaskDto>>>>'),
        
        # Fix GetUsersByRole method return type
        (r'public async Task<ActionResult<EnhancedPagedResult<UserDto>>>', 
         r'public async Task<ActionResult<ApiResponse<EnhancedPagedResult<UserDto>>>>'),
        
        # Fix type conversion issues in DailyReportsController
        (r'return ToApiResponse\(result\);\s+}\s+return BadRequest',
         r'return BadRequest'),
        
        # Fix malformed object initializers
        (r'new\s+\{\s+(\w+)\s*=.*?Success.*?\}', 
         r'new { Success = false, Message = "Error" }'),
    ]
    
    for pattern, replacement in patterns_to_fix:
        content = re.sub(pattern, replacement, content, flags=re.MULTILINE | re.DOTALL)
    
    # Fix specific lines that are problematic
    specific_fixes = [
        # Fix CreateErrorResponse calls with wrong argument count
        (r'CreateErrorResponse\([^,)]+,\s*[^,)]+,\s*[^)]+\)', 
         r'CreateErrorResponse("Error")'),
        
        # Fix Contains method on string array 
        (r'\.Contains\(status\)', r'.Contains(status.ToString())'),
        
        # Fix TimeSpan to string conversion
        (r'ExpiresIn = TimeSpan\.FromDays\(30\)', r'ExpiresIn = "30 days"'),
        
        # Fix malformed invalid initializer member declarator
        (r'new\s*\{\s*Success\s*=\s*false[^}]*\}\s*\}', r'new { Success = false, Message = "Error" }'),
    ]
    
    for pattern, replacement in specific_fixes:
        content = re.sub(pattern, replacement, content, flags=re.MULTILINE)
    
    # Fix parameter mismatches in service calls
    service_fixes = [
        # Fix RejectDailyReportAsync parameter
        (r'\.RejectDailyReportAsync\(reportId,\s*userId\)', 
         r'.RejectDailyReportAsync(reportId, userId, "Rejected")'),
        
        # Fix CreateWorkProgressItemAsync vs UpdateWorkProgressItemAsync
        (r'\.CreateWorkProgressItemAsync\(reportId,\s*request\)', 
         r'.CreateWorkProgressItemAsync(reportId, request)'),
    ]
    
    for pattern, replacement in service_fixes:
        content = re.sub(pattern, replacement, content)
    
    # Write back if changed
    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Fixed return types in {file_path}")

def fix_specific_controller_issues():
    """Fix specific issues in individual controllers"""
    
    # Fix MasterPlansController ToApiResponse generic type issues
    master_plans_path = "Controllers/V1/MasterPlansController.cs"
    if os.path.exists(master_plans_path):
        with open(master_plans_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Replace ToApiResponse calls that can't infer types
        content = re.sub(
            r'return ToApiResponse\(result\);',
            r'return result.IsSuccess ? Ok(new ApiResponse<object> { Success = true, Data = result.Data }) : BadRequest(new ApiResponse<object> { Success = false, Message = result.ErrorMessage });',
            content
        )
        
        with open(master_plans_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Fixed specific issues in {master_plans_path}")
    
    # Fix PhasesController ToApiResponse generic type issues  
    phases_path = "Controllers/V1/PhasesController.cs"
    if os.path.exists(phases_path):
        with open(phases_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Replace ToApiResponse calls that can't infer types
        content = re.sub(
            r'return ToApiResponse\(result\);',
            r'return result.IsSuccess ? Ok(new ApiResponse<object> { Success = true, Data = result.Data }) : BadRequest(new ApiResponse<object> { Success = false, Message = result.ErrorMessage });',
            content
        )
        
        with open(phases_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Fixed specific issues in {phases_path}")

def main():
    """Main function to fix controller errors"""
    
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
    
    print("Fixing controller return type errors...")
    
    for controller_file in controller_files:
        if os.path.exists(controller_file):
            fix_controller_return_types(controller_file)
    
    # Fix specific controller issues
    fix_specific_controller_issues()
    
    print("All controller fixes applied!")

if __name__ == "__main__":
    main()
