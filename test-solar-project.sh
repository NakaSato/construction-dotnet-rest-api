#!/bin/bash

# Test create project with solar fields
curl -X POST http://localhost:5002/api/v1/projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmYTVkMmJkOS1kMTVhLTQxOWEtYWE0Ni1lMWMxMWYwMDFkZWIiLCJ1bmlxdWVfbmFtZSI6InRlc3RfYWRtaW4iLCJlbWFpbCI6InRlc3RfYWRtaW5AZXhhbXBsZS5jb20iLCJyb2xlIjoiQWRtaW4iLCJuYmYiOjE3NDk5NDI0MzAsImV4cCI6MTc1MDAyODgzMCwiaWF0IjoxNzQ5OTQyNDMwLCJpc3MiOiJTb2xhclByb2plY3RzQVBJIiwiYXVkIjoiU29sYXJQcm9qZWN0c0NsaWVudCJ9.Xu4_yZH3kv-OsdejX5782NPQ5VNbt27lTJjgHR3gpsc" \
  -d '{
    "projectName": "Test Solar Installation Project",
    "address": "456 Oak Ave, Another City, State 67890",
    "clientInfo": "XYZ Corp - Contact: Sarah Johnson (555-987-6543)",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-09-30T00:00:00Z",
    "projectManagerId": "fa5d2bd9-d15a-419a-aa46-e1c11f001deb",
    "team": "E2",
    "connectionType": "LV",
    "connectionNotes": "Test connection notes",
    "totalCapacityKw": 171.0,
    "pvModuleCount": 300,
    "equipmentDetails": {
      "inverter125kw": 1,
      "inverter80kw": 0,
      "inverter60kw": 1,
      "inverter40kw": 0
    },
    "ftsValue": 6,
    "revenueValue": 1,
    "pqmValue": 0,
    "locationCoordinates": {
      "latitude": 14.72746,
      "longitude": 102.16276
    }
  }'
