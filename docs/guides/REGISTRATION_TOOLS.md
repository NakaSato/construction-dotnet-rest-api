# Production API User Registration Tools

This document provides information about the available user registration tools for the Solar Projects API.

## Overview

We have created several registration scripts to help you register users in the production API at `https://solar-projects-api.azurewebsites.net`.

## Current Database Status

⚠️ **IMPORTANT**: As of June 21, 2025, there is a database connectivity issue specifically with the registration endpoint. The diagnostic shows "Name or service not known" errors when attempting to register users, while other database operations (login validation, project queries) work correctly.

### Symptoms
- Login endpoint: ✅ Working (validates credentials, returns proper error messages)
- Test endpoints: ✅ Working (returns sample data)
- Registration endpoint: ❌ Failing with database connectivity issues

### Resolution Required
The Azure PostgreSQL database connection for the registration service needs to be fixed. See `scripts/diagnose-azure-db.sh` for detailed troubleshooting steps.

## Available Registration Scripts

### 1. `scripts/register-user.sh` (NEW - Recommended)

A clean, user-friendly registration script with comprehensive validation.

**Features:**
- Interactive mode with guided input
- Command-line mode for batch operations
- Strong password validation (matches production requirements)
- Email format validation
- Clear error messages and help text
- Beautiful CLI interface

**Usage:**
```bash
# Interactive mode
./scripts/register-user.sh

# Command line mode
./scripts/register-user.sh "username" "email@example.com" "Password123!" "Full Name" 1
```

**Password Requirements:**
- At least 8 characters long
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (!@#$%^&*)

### 2. `scripts/register-production-user.sh` (Full Featured)

Comprehensive registration script with advanced features.

**Features:**
- Interactive mode with detailed prompts
- Batch registration for multiple users
- Role discovery and validation
- Extensive error handling
- JSON file import/export
- CLI mode support

**Usage:**
```bash
# Interactive mode
./scripts/register-production-user.sh

# Batch mode
./scripts/register-production-user.sh batch

# CLI mode
./scripts/register-production-user.sh --cli username email password "Full Name" roleId
```

### 3. `scripts/quick-register.sh` (Simple CLI)

Quick command-line registration for simple use cases.

**Features:**
- Fast command-line interface
- Basic validation
- Minimal output
- Good for scripting

**Usage:**
```bash
./scripts/quick-register.sh "username" "email@example.com" "Password123!" "Full Name" 1
```

### 4. Test Runner Integration

The test runner (`scripts/test-runner.sh`) includes registration options in its menu:

```bash
./scripts/test-runner.sh
# Select option 5: User Registration
```

**Registration Options in Test Runner:**
1. Interactive Registration (Full featured)
2. Quick Registration (Command line)
3. Batch Registration (Multiple users)

## Role IDs

When registering users, you need to specify a role ID:

- `1` = Employee (default)
- `2` = Manager  
- `3` = Admin

## Example Valid Registration

```bash
# Using the new register-user.sh script
./scripts/register-user.sh \
  "john.doe" \
  "john.doe@company.com" \
  "MySecure123!" \
  "John Doe" \
  1
```

## Troubleshooting

### Database Connection Issues

If you see "Name or service not known" errors:

1. Run the diagnostic script:
   ```bash
   ./scripts/diagnose-azure-db.sh
   ```

2. Check Azure PostgreSQL server status:
   ```bash
   az postgres flexible-server show --name solar-projects-db-staging --resource-group solar-projects-rg
   ```

3. Check App Service logs:
   ```bash
   az webapp log tail --name solar-projects-api --resource-group solar-projects-rg
   ```

### Common Registration Errors

- **400 Bad Request**: Usually password validation issues or duplicate usernames/emails
- **401 Unauthorized**: API authentication issues
- **500 Server Error**: Database connectivity problems
- **409 Conflict**: Username or email already exists

### Password Validation

If registration fails with password errors, ensure your password meets these requirements:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter  
- At least one digit
- At least one special character

## Testing Registration

Once the database connectivity issue is resolved, you can test registration with:

```bash
# Test with a strong password
./scripts/register-user.sh \
  "testuser001" \
  "testuser001@example.com" \
  "TestPassword123!" \
  "Test User" \
  1
```

## Next Steps

1. **Fix Database Connectivity**: Resolve the PostgreSQL connection issue for the registration endpoint
2. **Test Registration**: Once fixed, test with the provided scripts
3. **User Management**: Consider adding user management endpoints (update, delete, list users)
4. **Authentication Testing**: Test login with newly registered users

## Available Scripts Summary

| Script | Best For | Features |
|--------|----------|----------|
| `register-user.sh` | General use | Clean UI, validation, both modes |
| `register-production-user.sh` | Advanced users | Full features, batch mode |
| `quick-register.sh` | Automation | Simple CLI, scripting |
| `test-runner.sh` | Testing | Integrated menu system |
| `diagnose-azure-db.sh` | Troubleshooting | Database diagnostics |

All scripts are ready to use once the Azure PostgreSQL connectivity issue is resolved.
