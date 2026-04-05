# gci409 REST API Design

## Overview
This document defines the target REST API contract for `gci409`. It is the production-facing API design for the ASP.NET Core backend and should guide controller design, application handlers, Swagger/OpenAPI generation, authorization policies, and client integration.

The API serves these product capabilities:

- authentication and session renewal
- user profile and access context
- project and collaborator management
- requirement and constraint capture
- artifact recommendation and generation orchestration
- artifact versioning and UML-specific representations
- exports and download workflows
- templates and generation rule administration
- collaboration and audit review
- platform administration

## API Conventions

### Base Path and Versioning
- Base path: `/api/v1`
- Use URI versioning for public contracts.
- Breaking changes require a new major version path.

### Naming
- Use plural nouns for collections.
- Use nested resources only when child ownership is clear.
- Use action-style subresources only for commands that do not map cleanly to CRUD, such as `/refresh`, `/approve`, or `/downloads/{exportId}`.

### Standard Media Types
- Request and response content type: `application/json`
- Validation and business errors: `application/problem+json`
- File downloads: format-specific content type

### Standard Query Parameters
- `page`: 1-based page number
- `pageSize`: requested page size, default `20`, max `100`
- `sort`: comma-separated sort fields, prefix `-` for descending
- `q`: free-text search where supported
- feature-specific filters such as `status`, `type`, `kind`, `from`, `to`

### Pagination Shape
Paginated endpoints should return:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 125,
  "totalPages": 7
}
```

### Problem Details Shape
Use ASP.NET Core `ProblemDetails` and `ValidationProblemDetails` for all non-success responses.

```json
{
  "type": "https://gci409/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/projects",
  "errors": {
    "name": ["Project name is required."]
  }
}
```

### Audit and Correlation
- Every mutating request should emit an audit event when business-relevant.
- Every request should accept and echo `X-Correlation-Id`.

### Idempotency
Use `Idempotency-Key` on client-triggered command endpoints that may be retried:

- `POST /auth/register`
- `POST /projects`
- `POST /projects/{projectId}/recommendations`
- `POST /projects/{projectId}/generation-requests`
- `POST /artifact-versions/{artifactVersionId}/exports`
- `POST /projects/{projectId}/templates`

Idempotency keys should be stored per authenticated user and route template for a bounded retention period.

## Security Model

### Authentication
- JWT bearer access token for API access
- Rotating refresh token flow for access renewal

### Authorization
Authorization is enforced at two levels:

- platform scope
  - `PlatformAdmin`
  - `SecurityAuditor`
- project scope
  - `Owner`
  - `Contributor`
  - `Reviewer`
  - `Viewer`

Use permission-based policies behind the scenes. Suggested permission codes:

- `profile.read`
- `project.read`
- `project.manage`
- `project.collaborators.manage`
- `requirements.read`
- `requirements.write`
- `recommendations.generate`
- `generation.queue`
- `artifact.read`
- `artifact.review`
- `artifact.export`
- `template.read`
- `template.write`
- `audit.read`
- `admin.manage`

## Endpoint Catalog

## Authentication

### `POST /api/v1/auth/register`
Creates a user account.

Request:
- `fullName`
- `email`
- `password`

Response `201 Created`:
- `user`
- `accessToken`
- `refreshToken`
- `expiresAtUtc`

Validation:
- unique email
- valid email format
- password strength policy

Authorization:
- anonymous

Status codes:
- `201 Created`
- `400 Bad Request`
- `409 Conflict`

Idempotency:
- support `Idempotency-Key`

### `POST /api/v1/auth/login`
Authenticates a user.

Request:
- `email`
- `password`

Response `200 OK`:
- `user`
- `accessToken`
- `refreshToken`
- `expiresAtUtc`

Status codes:
- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `423 Locked` when account is locked or disabled

Authorization:
- anonymous

### `POST /api/v1/auth/refresh`
Rotates refresh token and returns a new access token.

Request:
- `refreshToken`

Response `200 OK`:
- `accessToken`
- `refreshToken`
- `expiresAtUtc`

Status codes:
- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`

Authorization:
- anonymous

### `POST /api/v1/auth/logout`
Revokes the current refresh token or all sessions.

Request:
- `refreshToken`
- optional `revokeAllSessions`

Response `204 No Content`

Status codes:
- `204 No Content`
- `400 Bad Request`
- `401 Unauthorized`

## User Profile

### `GET /api/v1/me`
Returns the current user profile and effective platform roles.

Response `200 OK`:
- `id`
- `fullName`
- `email`
- `status`
- `platformRoles`
- `permissions`

Authorization:
- authenticated user

### `PATCH /api/v1/me`
Updates user display profile.

Request:
- `fullName`

Response `200 OK`

Validation:
- name length

Authorization:
- authenticated user

### `GET /api/v1/me/projects`
Lists projects visible to the current user.

Query:
- `page`, `pageSize`, `sort`, `status`, `q`

Authorization:
- `project.read`

## Projects

### `GET /api/v1/projects`
Lists accessible projects.

Query:
- `page`, `pageSize`
- `sort=name,-createdAtUtc`
- `status`
- `q`

Response `200 OK` paged project summaries

Authorization:
- `project.read`

### `POST /api/v1/projects`
Creates a new project workspace.

Request:
- `name`
- `key`
- `description`

Response `201 Created`

Validation:
- unique `key`
- required `name`
- length constraints

Authorization:
- authenticated user

Idempotency:
- support `Idempotency-Key`

### `GET /api/v1/projects/{projectId}`
Returns project detail.

Authorization:
- project membership with `project.read`

Status codes:
- `200 OK`
- `403 Forbidden`
- `404 Not Found`

### `PATCH /api/v1/projects/{projectId}`
Updates project metadata.

Request:
- `name`
- `description`
- `status`

Authorization:
- `project.manage`

### `DELETE /api/v1/projects/{projectId}`
Archives the project.

Response `204 No Content`

Authorization:
- `project.manage`

Notes:
- use archive semantics instead of hard delete

## Collaborators

### `GET /api/v1/projects/{projectId}/collaborators`
Lists project collaborators and roles.

Query:
- `page`, `pageSize`, `status`, `q`

Authorization:
- `project.read`

### `POST /api/v1/projects/{projectId}/collaborators`
Adds or invites a collaborator.

Request:
- `userId` or `email`
- `role`

Authorization:
- `project.collaborators.manage`

Validation:
- valid role
- user must exist or invitation mode must be supported

Status codes:
- `201 Created`
- `400 Bad Request`
- `409 Conflict`

### `PATCH /api/v1/projects/{projectId}/collaborators/{membershipId}`
Updates collaborator role or membership status.

Authorization:
- `project.collaborators.manage`

### `DELETE /api/v1/projects/{projectId}/collaborators/{membershipId}`
Removes a collaborator.

Response `204 No Content`

Authorization:
- `project.collaborators.manage`

## Requirement Sets

### `GET /api/v1/projects/{projectId}/requirement-sets/current`
Returns the latest requirement set version for a project.

Authorization:
- `requirements.read`

### `GET /api/v1/projects/{projectId}/requirement-sets/versions`
Lists requirement set versions.

Query:
- `page`, `pageSize`, `sort=-versionNumber`

Authorization:
- `requirements.read`

### `GET /api/v1/projects/{projectId}/requirement-sets/versions/{versionId}`
Returns a specific requirement set version.

Authorization:
- `requirements.read`

### `POST /api/v1/projects/{projectId}/requirement-sets/versions`
Creates a new requirement set version from submitted requirements and constraints.

Request:
- `summary`
- `requirements[]`
- `constraints[]`

Requirement item fields:
- `code`
- `title`
- `description`
- `type`
- `priority`
- `rationale`

Constraint item fields:
- `title`
- `description`
- `type`
- `severity`

Response `201 Created`

Validation:
- at least one requirement
- unique requirement `code` within version
- enum validation for `type`, `priority`, `severity`

Authorization:
- `requirements.write`

Idempotency:
- support `Idempotency-Key`

### `PATCH /api/v1/projects/{projectId}/requirement-sets/versions/{versionId}`
Updates a draft requirement set version only.

Authorization:
- `requirements.write`

Status codes:
- `200 OK`
- `409 Conflict` when version is immutable/published

## Constraints
Constraints are modeled as part of a requirement set version, but filtered endpoints are useful for UI workflows.

### `GET /api/v1/projects/{projectId}/constraints`
Lists constraints from the latest or requested version.

Query:
- `versionId`
- `type`
- `severity`
- `page`, `pageSize`

Authorization:
- `requirements.read`

### `GET /api/v1/projects/{projectId}/constraints/{constraintId}`
Returns one constraint entry.

Authorization:
- `requirements.read`

## Recommendations

### `GET /api/v1/projects/{projectId}/recommendations`
Lists recommendation runs for the project.

Query:
- `page`, `pageSize`, `sort=-createdAtUtc`

Authorization:
- `requirements.read`

### `GET /api/v1/projects/{projectId}/recommendations/latest`
Returns the latest recommendation set.

Authorization:
- `requirements.read`

### `GET /api/v1/projects/{projectId}/recommendations/{recommendationSetId}`
Returns a specific recommendation set and scoring detail.

Response:
- recommendation set metadata
- recommended artifact kinds
- confidence scores
- score breakdown
- rationale
- traceability references

Authorization:
- `requirements.read`

### `POST /api/v1/projects/{projectId}/recommendations`
Runs recommendation analysis against a requirement set version.

Request:
- optional `requirementSetVersionId`
- optional `templateIds`

Response `202 Accepted` if async, otherwise `200 OK`

Authorization:
- `recommendations.generate`

Idempotency:
- support `Idempotency-Key`

## Generation Requests

### `GET /api/v1/projects/{projectId}/generation-requests`
Lists generation requests.

Query:
- `page`, `pageSize`
- `status`
- `sort=-createdAtUtc`

Authorization:
- `generation.queue` or `artifact.read`

### `GET /api/v1/projects/{projectId}/generation-requests/{generationRequestId}`
Returns generation request detail, targets, status, and diagnostics.

Authorization:
- `artifact.read`

### `POST /api/v1/projects/{projectId}/generation-requests`
Queues artifact generation.

Request:
- `requirementSetVersionId`
- `artifactKinds[]`
- optional `templateVersionIds[]`
- optional `notes`

Response `202 Accepted`

Authorization:
- `generation.queue`

Validation:
- referenced version must belong to the project
- at least one artifact kind
- duplicate artifact kinds rejected

Idempotency:
- support `Idempotency-Key`

### `POST /api/v1/projects/{projectId}/generation-requests/{generationRequestId}/cancel`
Attempts to cancel a queued or running request.

Response `202 Accepted`

Authorization:
- `generation.queue`

## Artifacts

### `GET /api/v1/projects/{projectId}/artifacts`
Lists artifacts for a project.

Query:
- `page`, `pageSize`
- `kind`
- `status`
- `q`
- `sort=-createdAtUtc`

Authorization:
- `artifact.read`

### `GET /api/v1/projects/{projectId}/artifacts/{artifactId}`
Returns artifact summary and latest approved or latest draft version metadata.

Authorization:
- `artifact.read`

### `PATCH /api/v1/projects/{projectId}/artifacts/{artifactId}`
Updates artifact metadata such as title or review status when allowed.

Authorization:
- `artifact.review`

### `GET /api/v1/projects/{projectId}/artifacts/{artifactId}/versions`
Lists immutable versions for an artifact.

Query:
- `page`, `pageSize`, `sort=-versionNumber`

Authorization:
- `artifact.read`

### `GET /api/v1/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}`
Returns one artifact version with summary, validation findings, and rationale.

Authorization:
- `artifact.read`

## UML Artifacts

### `GET /api/v1/projects/{projectId}/artifacts/{artifactId}/uml`
Returns UML-specific metadata.

Response:
- `diagramType`
- `primaryNotation`
- `availableNotations`
- `validationStatus`

Authorization:
- `artifact.read`

### `GET /api/v1/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}/representations`
Returns all stored representations for a version.

Response:
- canonical model metadata
- `mermaid`
- `plantUml`
- `markdown`
- validation warnings

Authorization:
- `artifact.read`

### `GET /api/v1/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}/preview`
Returns preview-ready representation metadata or signed URLs for rendered assets.

Query:
- `notation=mermaid|plantuml`
- optional `format=svg|png|txt`

Authorization:
- `artifact.read`

## Artifact Review

### `POST /api/v1/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}/approve`
Marks an artifact version as approved.

Request:
- optional `comment`

Authorization:
- `artifact.review`

Status codes:
- `200 OK`
- `409 Conflict` when validation findings block approval

### `POST /api/v1/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}/reject`
Rejects an artifact version.

Request:
- `comment`

Authorization:
- `artifact.review`

## Exports

### `GET /api/v1/projects/{projectId}/exports`
Lists export jobs for the project.

Query:
- `page`, `pageSize`
- `status`
- `format`
- `sort=-createdAtUtc`

Authorization:
- `artifact.export`

### `POST /api/v1/artifact-versions/{artifactVersionId}/exports`
Creates an export job for a specific immutable artifact version.

Request:
- `format`
- optional `options`

Response `202 Accepted`

Authorization:
- `artifact.export`

Validation:
- format supported for the artifact type

Idempotency:
- support `Idempotency-Key`

### `GET /api/v1/exports/{exportId}`
Returns export status and metadata.

Authorization:
- `artifact.export`

### `GET /api/v1/exports/{exportId}/download`
Downloads the exported file or returns a temporary download URL.

Authorization:
- `artifact.export`

Status codes:
- `200 OK`
- `302 Found` for redirect to signed URL
- `404 Not Found`
- `409 Conflict` if export is not complete

## Templates

### `GET /api/v1/templates`
Lists global and visible project templates.

Query:
- `page`, `pageSize`
- `scope=global|project|all`
- `kind`
- `status`
- `q`

Authorization:
- `template.read`

### `GET /api/v1/projects/{projectId}/templates`
Lists project templates.

Authorization:
- `template.read`

### `POST /api/v1/projects/{projectId}/templates`
Creates a project-scoped template.

Request:
- `name`
- `description`
- `artifactKind`
- `content`
- optional `metadata`

Response `201 Created`

Authorization:
- `template.write`

Idempotency:
- support `Idempotency-Key`

### `GET /api/v1/templates/{templateId}`
Returns template detail and current version.

Authorization:
- `template.read`

### `GET /api/v1/templates/{templateId}/versions`
Lists template versions.

Authorization:
- `template.read`

### `POST /api/v1/templates/{templateId}/versions`
Creates a new template version.

Authorization:
- `template.write`

## Comments and Collaboration

### `GET /api/v1/projects/{projectId}/comments/threads`
Lists comment threads for a target resource.

Query:
- `targetType`
- `targetId`
- `status`
- `page`, `pageSize`

Authorization:
- `project.read`

### `POST /api/v1/projects/{projectId}/comments/threads`
Creates a thread attached to an artifact, version, recommendation set, or requirement version.

Request:
- `targetType`
- `targetId`
- `title`

Authorization:
- `project.read`

### `GET /api/v1/projects/{projectId}/comments/threads/{threadId}`
Returns a thread and comments.

Authorization:
- `project.read`

### `POST /api/v1/projects/{projectId}/comments/threads/{threadId}/comments`
Adds a comment.

Request:
- `body`

Authorization:
- `project.read`

Validation:
- non-empty body

### `POST /api/v1/projects/{projectId}/comments/threads/{threadId}/resolve`
Resolves a thread.

Authorization:
- `artifact.review` or `project.manage`

## Audit Logs

### `GET /api/v1/audit-logs`
Lists platform audit records.

Query:
- `page`, `pageSize`
- `actorUserId`
- `projectId`
- `action`
- `entityType`
- `from`
- `to`
- `sort=-createdAtUtc`

Authorization:
- `audit.read`
- platform admin or security auditor

### `GET /api/v1/projects/{projectId}/audit-logs`
Lists project audit records.

Authorization:
- `audit.read`
- project owner, reviewer, or platform auditor

## Admin Operations

### `GET /api/v1/admin/users`
Lists users for administration.

Query:
- `page`, `pageSize`, `status`, `q`

Authorization:
- `admin.manage`

### `PATCH /api/v1/admin/users/{userId}`
Updates user status or profile-adjacent admin fields.

Authorization:
- `admin.manage`

### `GET /api/v1/admin/roles`
Lists roles and permissions.

Authorization:
- `admin.manage`

### `POST /api/v1/admin/roles`
Creates a role.

Authorization:
- `admin.manage`

### `PATCH /api/v1/admin/roles/{roleId}`
Updates a role and attached permissions.

Authorization:
- `admin.manage`

### `GET /api/v1/admin/generation-rules`
Lists active generation rules and versions.

Authorization:
- `admin.manage`

### `POST /api/v1/admin/generation-rules`
Creates a rule definition.

Authorization:
- `admin.manage`

### `POST /api/v1/admin/generation-rules/{ruleId}/versions`
Publishes a new rule version.

Authorization:
- `admin.manage`

## Validation Expectations

### Request Validation
Use FluentValidation for:

- required fields
- enum validation
- string length limits
- collection size rules
- duplicate detection in lists
- foreign key ownership checks in application handlers

### Business Validation
Use application-level guards for:

- project membership and permission checks
- immutable version protections
- cross-resource ownership and scope checks
- version compatibility checks for generation and exports

## Status Code Guidance

### Read Endpoints
- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

### Command Endpoints
- `201 Created` for synchronous resource creation
- `202 Accepted` for queued async work
- `204 No Content` for archive, remove, or revoke
- `400 Bad Request` for malformed requests
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict` for uniqueness, immutable version, or invalid state transitions
- `422 Unprocessable Entity` may be used for complex business rule violations if the team prefers it consistently

## Implementation Guidance for ASP.NET Core

### Controller Design
- Keep controllers thin.
- One controller per aggregate or workflow boundary.
- Use route prefix attributes like `[Route("api/v1/projects/{projectId:guid}/artifacts")]`.
- Do not expose EF Core entities directly.

Suggested controllers:

- `AuthController`
- `MeController`
- `ProjectsController`
- `CollaboratorsController`
- `RequirementSetsController`
- `ConstraintsController`
- `RecommendationsController`
- `GenerationRequestsController`
- `ArtifactsController`
- `ArtifactReviewsController`
- `ExportsController`
- `TemplatesController`
- `CommentsController`
- `AuditLogsController`
- `AdminUsersController`
- `AdminRolesController`
- `AdminGenerationRulesController`

### Request and Response Models
- Separate request DTOs from response DTOs.
- Use explicit paged response DTOs.
- Include ETag or version metadata later if optimistic concurrency is exposed over HTTP.

### Authorization in ASP.NET Core
- Use policy names that map to permission codes.
- Enforce project membership inside handlers and services, not only attributes.
- Use `[Authorize(Policy = "...")]` for endpoint-level coarse checks.

### Validation Pipeline
- Register FluentValidation validators for all command and query DTOs.
- Convert validation failures to `ValidationProblemDetails`.

### OpenAPI
- Group endpoints with Swagger tags by module.
- Document authorization requirements on secured endpoints.
- Document `Idempotency-Key` and `X-Correlation-Id` headers for relevant operations.
- Provide response examples for common success and failure shapes.

### Async Workflows
- Recommendation generation, artifact generation, and export creation should return `202 Accepted` when queued.
- Include status endpoints and stable identifiers for polling.
- Prefer durable background processing over in-memory queues.

### Audit and Logging
- Write audit events in application services or behaviors, not controllers.
- Use structured logging with route, user id, project id, correlation id, and command name.

## Notes on the Current Implementation
The current backend already exposes a subset of this contract for:

- auth
- projects
- requirements
- recommendations
- generation requests
- artifacts

This document defines the fuller enterprise target contract that new controllers and OpenAPI descriptions should converge on.
