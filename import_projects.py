#!/usr/bin/env python3
"""
Import solar projects from project.json file to the API database
"""
import json
import requests
import time
from datetime import datetime, timezone
import sys

# API Configuration
API_BASE_URL = "http://localhost:5002/api/v1"
LOGIN_ENDPOINT = f"{API_BASE_URL}/auth/login"
PROJECTS_ENDPOINT = f"{API_BASE_URL}/projects"

# Test credentials (using the test_admin user for project creation permissions)
LOGIN_CREDENTIALS = {
    "username": "test_admin",
    "password": "Admin123!"  # Admin credentials for project creation
}

# Valid project manager ID from the database (test_manager user)
VALID_PROJECT_MANAGER_ID = "61cf3206-7f93-46e8-bf45-3841f63dc655"

def login_and_get_token():
    """Login to the API and get JWT token"""
    try:
        response = requests.post(LOGIN_ENDPOINT, json=LOGIN_CREDENTIALS)
        if response.status_code == 200:
            data = response.json()
            if data.get('success'):
                token = data['data']['token']
                print(f"✅ Successfully logged in as {LOGIN_CREDENTIALS['username']}")
                return token
            else:
                print(f"❌ Login failed: {data.get('message', 'Unknown error')}")
                return None
        else:
            print(f"❌ Login request failed with status {response.status_code}")
            print(f"Response: {response.text}")
            return None
    except Exception as e:
        print(f"❌ Login error: {str(e)}")
        return None

def load_projects_from_json(filename):
    """Load projects from JSON file"""
    try:
        with open(filename, 'r', encoding='utf-8') as f:
            projects = json.load(f)
        print(f"✅ Loaded {len(projects)} projects from {filename}")
        return projects
    except Exception as e:
        print(f"❌ Error loading projects: {str(e)}")
        return []

def transform_project_data(project_data):
    """Transform project data from JSON format to API format"""
    # Set default dates if null - use simple date format without time/timezone
    start_date = project_data.get('startDate')
    if not start_date:
        # Use a simple date format: YYYY-MM-DD
        start_date = "2024-01-01T00:00:00Z"
    
    estimated_end_date = project_data.get('estimatedEndDate')
    if estimated_end_date:
        # Keep the original format if it exists
        estimated_end_date = estimated_end_date
    else:
        # Set to null for the API
        estimated_end_date = None
    
    # Transform equipment details
    equipment_details = project_data.get('equipmentDetails', {})
    transformed_equipment = {
        "inverter125Kw": equipment_details.get('inverter125kw', 0),
        "inverter80Kw": equipment_details.get('inverter80kw', 0),
        "inverter60Kw": equipment_details.get('inverter60kw', 0),
        "inverter40Kw": equipment_details.get('inverter40kw', 0)
    }
    
    # Transform location coordinates
    location_coords = project_data.get('locationCoordinates')
    transformed_location = None
    if location_coords:
        transformed_location = {
            "latitude": location_coords.get('latitude'),
            "longitude": location_coords.get('longitude')
        }
    
    # Create the API request payload
    api_payload = {
        "projectName": project_data.get('projectName', ''),
        "address": project_data.get('address', ''),
        "clientInfo": project_data.get('clientInfo', ''),
        "startDate": start_date,
        "estimatedEndDate": estimated_end_date,
        "projectManagerId": VALID_PROJECT_MANAGER_ID,  # Use valid manager ID instead of invalid one
        "team": project_data.get('team'),
        "connectionType": project_data.get('connectionType'),
        "connectionNotes": project_data.get('connectionNotes'),
        "totalCapacityKw": project_data.get('totalCapacityKw'),
        "pvModuleCount": project_data.get('pvModuleCount'),
        "equipmentDetails": transformed_equipment,
        "ftsValue": project_data.get('ftsValue'),
        "revenueValue": project_data.get('revenueValue'),
        "pqmValue": project_data.get('pqmValue'),
        "locationCoordinates": transformed_location
    }
    
    return api_payload

def clear_rate_limits(token):
    """Clear rate limits to allow bulk import"""
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        # Clear all rate limits
        response = requests.delete(f"{API_BASE_URL}/rate-limit/all", headers=headers)
        if response.status_code in [200, 204]:
            print("✅ Rate limits cleared successfully")
            return True
        else:
            print(f"⚠️  Could not clear rate limits (status {response.status_code}), proceeding with delays")
            return False
    except Exception as e:
        print(f"⚠️  Error clearing rate limits: {str(e)}, proceeding with delays")
        return False

def create_project(token, project_data):
    """Create a single project via API"""
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        response = requests.post(PROJECTS_ENDPOINT, json=project_data, headers=headers)
        if response.status_code == 201:
            # HTTP 201 Created - project was created successfully
            # The API returns empty body with Location header
            location = response.headers.get('Location', '')
            project_name = project_data['projectName'][:50] + "..." if len(project_data['projectName']) > 50 else project_data['projectName']
            print(f"✅ Created project: {project_name}")
            if location:
                # Extract project ID from location header if needed
                project_id = location.split('/')[-1] if '/' in location else None
                return True, project_id
            return True, None
        elif response.status_code == 200:
            # HTTP 200 with JSON response
            data = response.json()
            if data.get('success'):
                project_id = data['data']['projectId']
                project_name = project_data['projectName'][:50] + "..." if len(project_data['projectName']) > 50 else project_data['projectName']
                print(f"✅ Created project: {project_name}")
                return True, project_id
            else:
                print(f"❌ API error: {data.get('message', 'Unknown error')}")
                return False, None
        else:
            # Try to parse error response as JSON, fallback to text
            try:
                error_data = response.json()
                error_msg = error_data.get('message', response.text)
            except:
                error_msg = response.text
            print(f"❌ HTTP error {response.status_code}: {error_msg}")
            return False, None
    except Exception as e:
        print(f"❌ Request error: {str(e)}")
        return False, None

def main():
    """Main import function"""
    print("🚀 Starting solar projects import...")
    
    # Step 1: Login and get token
    token = login_and_get_token()
    if not token:
        print("❌ Cannot proceed without authentication token")
        sys.exit(1)
    
    # Step 2: Clear rate limits for bulk import
    print(f"\n🔧 Clearing rate limits for bulk import...")
    rate_limits_cleared = clear_rate_limits(token)
    
    # Step 3: Load projects from JSON
    projects = load_projects_from_json('project.json')
    if not projects:
        print("❌ No projects to import")
        sys.exit(1)
    
    # Step 4: Import projects
    successful_imports = 0
    failed_imports = 0
    
    print(f"\n📊 Importing {len(projects)} projects...")
    print("-" * 60)
    
    # Determine delay based on whether rate limits were cleared
    delay_between_requests = 0.5 if rate_limits_cleared else 15.0  # 0.5 seconds if cleared, 15 seconds if not
    
    for i, project in enumerate(projects, 1):
        print(f"[{i}/{len(projects)}] ", end="")
        
        # Add delay to respect rate limiting 
        if i > 1:  # Skip delay for the first request
            if rate_limits_cleared:
                print(f"⏳ Brief pause...")
            else:
                print(f"⏳ Waiting {delay_between_requests} seconds to respect rate limits...")
            time.sleep(delay_between_requests)
        
        # Transform the project data
        api_project = transform_project_data(project)
        
        # Create the project
        success, project_id = create_project(token, api_project)
        
        if success:
            successful_imports += 1
        else:
            failed_imports += 1
            print(f"    Failed project: {project.get('projectName', 'Unknown')}")
    
    # Step 4: Show summary
    print("\n" + "=" * 60)
    print("📋 IMPORT SUMMARY")
    print("=" * 60)
    print(f"✅ Successful imports: {successful_imports}")
    print(f"❌ Failed imports: {failed_imports}")
    print(f"📊 Total projects processed: {len(projects)}")
    print(f"📈 Success rate: {(successful_imports/len(projects)*100):.1f}%")
    
    if failed_imports > 0:
        print(f"\n⚠️  {failed_imports} projects failed to import. Check the output above for details.")
    else:
        print(f"\n🎉 All projects imported successfully!")

if __name__ == "__main__":
    main()
