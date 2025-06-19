#!/usr/bin/env python3

import os
import re

def fix_specific_errors():
    """Fix specific compilation errors"""
    
    print("Fixing specific controller errors...")
    
    # Fix TasksController missing variables
    tasks_controller = "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/TasksController.cs"
    
    try:
        with open(tasks_controller, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original = content
        
        # Fix missing 'createTaskRequest' variable (line 58)
        content = re.sub(
            r'LogControllerAction\(_logger, "CreateTask", createTaskRequest\);',
            'LogControllerAction(_logger, "CreateTask", request);',
            content
        )
        
        # Fix missing 'request' variable in GetTaskProgressReports
        content = re.sub(
            r'LogControllerAction\(_logger, "GetTaskProgressReports", request\);',
            'LogControllerAction(_logger, "GetTaskProgressReports", new { id, page, pageSize });',
            content
        )
        
        # Fix malformed object initializer in UpdateTaskStatus
        if 'new { id: id, status: status }' in content:
            content = content.replace('new { id: id, status: status }', 'new { id, status }')
        
        # Fix usage of parameters before declaration - need to restructure the method
        # Find the method with parameter issues
        pattern = r'(public async Task<ActionResult<ApiResponse<EnhancedPagedResult<TaskDto>>>>.*?GetTasksWithAdvancedPagination.*?)(\{.*?)(\})'
        
        def fix_method_parameters(match):
            method_signature = match.group(1)
            method_body = match.group(2)
            method_end = match.group(3)
            
            # Add proper method parameters
            if '[FromQuery]' not in method_signature:
                # Replace with proper parameters
                fixed_signature = '''public async Task<ActionResult<ApiResponse<EnhancedPagedResult<TaskDto>>>> GetTasksWithAdvancedPagination(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? assigneeId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")'''
                
                return fixed_signature + method_body + method_end
            
            return match.group(0)
        
        content = re.sub(pattern, fix_method_parameters, content, flags=re.DOTALL)
        
        if content != original:
            with open(tasks_controller, 'w', encoding='utf-8') as f:
                f.write(content)
            print("✓ Fixed TasksController.cs")
        else:
            print("- No changes needed for TasksController.cs")
            
    except Exception as e:
        print(f"✗ Error fixing TasksController.cs: {e}")
    
    # Fix PlaceholderServices TimeSpan issue
    placeholder_services = "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Services/PlaceholderServices.cs"
    
    try:
        with open(placeholder_services, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original = content
        
        # Fix TimeSpan to string conversion (line 356)
        content = re.sub(
            r'TotalTime = timeSpan',
            'TotalTime = timeSpan.ToString()',
            content
        )
        
        if content != original:
            with open(placeholder_services, 'w', encoding='utf-8') as f:
                f.write(content)
            print("✓ Fixed PlaceholderServices.cs TimeSpan issue")
        else:
            print("- No TimeSpan issue found in PlaceholderServices.cs")
            
    except Exception as e:
        print(f"✗ Error fixing PlaceholderServices.cs: {e}")
    
    # Fix DailyReportsController specific issues
    daily_reports_controller = "/Users/chanthawat/Development/dotnet-dev/dotnet-rest-api/Controllers/V1/DailyReportsController.cs"
    
    try:
        with open(daily_reports_controller, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original = content
        
        # Fix return type mismatch (lines 286, 321 - returning ApiResponse<DailyReportDto> instead of ApiResponse<bool>)
        content = re.sub(
            r'return await GetDailyReport\(id\);',
            'var result = await _dailyReportService.ApproveDailyReportAsync(id, userId);\n            return ToApiResponse(result);',
            content
        )
        
        # Fix missing 'reason' parameter in RejectDailyReportAsync (line 350)
        content = re.sub(
            r'var result = await _dailyReportService\.RejectDailyReportAsync\(id, userId\);',
            'var result = await _dailyReportService.RejectDailyReportAsync(id, userId, "Rejected by user");',
            content
        )
        
        # Fix CreateWorkProgressItemRequest vs UpdateWorkProgressItemRequest (line 421)
        content = re.sub(
            r'var result = await _dailyReportService\.CreateWorkProgressItemAsync\(id, request\);',
            'var updateRequest = new UpdateWorkProgressItemRequest { Description = request.Description, HoursWorked = request.HoursWorked, ProgressPercentage = request.ProgressPercentage };\n            var result = await _dailyReportService.UpdateWorkProgressItemAsync(id, updateRequest);',
            content
        )
        
        if content != original:
            with open(daily_reports_controller, 'w', encoding='utf-8') as f:
                f.write(content)
            print("✓ Fixed DailyReportsController.cs")
        else:
            print("- No changes needed for DailyReportsController.cs")
            
    except Exception as e:
        print(f"✗ Error fixing DailyReportsController.cs: {e}")

if __name__ == "__main__":
    fix_specific_errors()
