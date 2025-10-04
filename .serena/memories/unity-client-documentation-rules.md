# Unity Client Documentation Rules

## Overview
This project maintains Unity client API documentation that must be updated whenever APIs or DTOs are added or modified.

## Documentation Files Location
**IMPORTANT**: All Unity documentation is stored in the **IdleRPGClient** project folder (sibling to IdleRPGServer):

```
../IdleRPGClient/Docs/
├── Unity-API-Reference.md    ← Full API documentation
├── Unity-DTOs.cs              ← Unity C# DTO classes
└── Unity-Quick-Reference.md   ← Quick reference tables
```

**Reason**: Documentation is kept with the Unity client project to keep all client-related files together.

## Update Rules

### When Creating a New API Endpoint
1. Add endpoint details to `../IdleRPGClient/Docs/Unity-API-Reference.md`:
   - Endpoint path and HTTP method
   - Request/Response JSON examples
   - Authentication requirements
   - Unity C# usage example
2. Add corresponding DTO classes to `../IdleRPGClient/Docs/Unity-DTOs.cs` as `[Serializable]` classes
3. Update endpoint summary table in `../IdleRPGClient/Docs/Unity-Quick-Reference.md`

### When Adding a New DTO
1. Create the server DTO in `IdleRPG.Application/DTOs/`
2. Add Unity-compatible `[Serializable]` version to `../IdleRPGClient/Docs/Unity-DTOs.cs`
3. Document all properties with XML comments
4. Add usage example in code comments

### When Modifying Existing Endpoints or DTOs
1. Update all three documentation files simultaneously
2. Mark breaking changes with **⚠️ BREAKING CHANGE** in docs
3. Update version date at bottom of markdown files

## Checklist Before Completing API Tasks
- [ ] API endpoint documented in `Unity-API-Reference.md`
- [ ] DTO class added/updated in `Unity-DTOs.cs`
- [ ] Quick reference table updated in `Unity-Quick-Reference.md`
- [ ] Example code provided for new features
- [ ] Error responses documented

## Current API Endpoints (as of 2025-10-04)

### Authentication APIs
- POST `/api/auth/register` - Register new player
- POST `/api/auth/login` - Login
- POST `/api/auth/refresh` - Refresh access token
- POST `/api/auth/logout` - Logout (requires auth)
- GET `/api/auth/profile` - Get profile (requires auth)

### Character APIs
- POST `/api/character/Create` - Create character (requires auth)
- GET `/api/character/{id}` - Get character by ID (requires auth)
- GET `/api/character/GetCharacters` - Get all player characters (requires auth)
- DELETE `/api/character/{id}` - Delete character (requires auth)
- POST `/api/character/{id}/experience` - Add experience (requires auth)
- PUT `/api/character/{id}/stats` - Allocate stat points (requires auth)

## Important Notes
- All authenticated endpoints require `Authorization: Bearer {accessToken}` header
- Access tokens expire in 15 minutes
- Refresh tokens expire in 7 days
- All IDs are GUIDs (UUID format)
- All dates are ISO 8601 format in UTC

## File Paths for Updates
When updating documentation, use these paths from the IdleRPGServer root:
- `../IdleRPGClient/Docs/Unity-API-Reference.md`
- `../IdleRPGClient/Docs/Unity-DTOs.cs`
- `../IdleRPGClient/Docs/Unity-Quick-Reference.md`
