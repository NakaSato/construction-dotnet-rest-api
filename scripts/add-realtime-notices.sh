#!/bin/bash

# Script to add real-time update notices to all API documentation files
# Usage: ./add-realtime-notices.sh

API_DOCS_DIR="/Users/chanthawat/Development/dotnet-rest-api/docs/api"

# Array of API documentation files that should have real-time notices
API_FILES=(
    "03_PROJECTS.md"
    "04_MASTER_PLANS.md" 
    "05_TASKS.md"
    "06_DAILY_REPORTS.md"
    "07_WORK_REQUESTS.md"
    "10_WEEKLY_PLANNING.md"
    "16_WBS_WORK_BREAKDOWN_STRUCTURE.md"
)

REALTIME_NOTICE="**⚡ Real-Time Live Updates**: This API supports real-time data synchronization. All changes are instantly broadcast to connected users via WebSocket."

echo "Adding real-time notices to API documentation files..."

for file in "${API_FILES[@]}"; do
    filepath="$API_DOCS_DIR/$file"
    
    if [[ -f "$filepath" ]]; then
        echo "Processing $file..."
        
        # Check if file already has real-time notice
        if grep -q "Real-Time Live Updates" "$filepath"; then
            echo "  ✅ $file already has real-time notice"
        else
            echo "  📝 Adding real-time notice to $file"
            
            # Create temporary file with real-time notice added after first heading
            awk '
                /^# / && !added { 
                    print $0; 
                    print ""; 
                    print "**⚡ Real-Time Live Updates**: This API supports real-time data synchronization. All changes are instantly broadcast to connected users via WebSocket.";
                    print "";
                    added=1;
                    next;
                }
                { print }
            ' "$filepath" > "$filepath.tmp"
            
            # Replace original file
            mv "$filepath.tmp" "$filepath"
            echo "  ✅ Added real-time notice to $file"
        fi
    else
        echo "  ❌ File not found: $file"
    fi
done

echo ""
echo "✅ Real-time notices updated in all API documentation files!"
echo ""
echo "📖 For comprehensive real-time implementation details, see:"
echo "   docs/api/00_REAL_TIME_LIVE_UPDATES.md"
