# gci409 PostgreSQL Schema Design

## Overview
`gci409` should use a normalized PostgreSQL schema with schema-per-module boundaries:

- `iam`
- `projects`
- `requirements`
- `generation`
- `artifacts`
- `collaboration`
- `audit`

This keeps transactional boundaries clear, improves ownership inside the modular monolith, and makes future extraction easier if any module is later split into a service.

## Shared Design Rules
- Use `uuid` primary keys for all business tables.
- Use `timestamp with time zone` for all UTC timestamps.
- Keep transactional metadata separate from large generated payloads.
- Use append-only or immutable version tables for requirements, rules, templates, and artifacts.
- Use `jsonb` only for variable machine-oriented structures, not as a substitute for normalized relational tables.

## Common Audit Columns
Apply these columns to all aggregate roots and versioned entities unless the table is a pure join table:

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | Application-generated identifier |
| `created_at_utc` | `timestamptz not null` | Record creation time |
| `created_by_user_id` | `uuid null` | Actor that created the record |
| `last_modified_at_utc` | `timestamptz null` | Last update time |
| `last_modified_by_user_id` | `uuid null` | Actor that last updated the record |

## Table Design

### `iam.users`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `full_name` | `varchar(200)` | |
| `email` | `varchar(320)` | Unique, lowercase |
| `password_hash` | `varchar(4000)` | |
| `status` | `smallint` | Pending, Active, Suspended, Disabled |
| audit columns | mixed | |

Indexes:
- unique (`email`)
- index (`status`)

### `iam.refresh_tokens`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `user_id` | `uuid` FK -> `iam.users.id` | |
| `token_hash` | `varchar(512)` | Unique |
| `expires_at_utc` | `timestamptz` | |
| `revoked_at_utc` | `timestamptz null` | |
| audit columns | mixed | |

Indexes:
- unique (`token_hash`)
- index (`user_id`, `expires_at_utc`, `revoked_at_utc`)

### `iam.roles`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `name` | `varchar(128)` | |
| `description` | `varchar(1000)` | |
| `scope` | `smallint` | Platform or Project |
| audit columns | mixed | |

Indexes:
- unique (`name`, `scope`)

### `iam.permissions`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `code` | `varchar(128)` | Unique capability code |
| `description` | `varchar(1000)` | |
| audit columns | mixed | |

Indexes:
- unique (`code`)

### `iam.role_permissions`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | Optional surrogate key for EF simplicity |
| `role_id` | `uuid` FK -> `iam.roles.id` | |
| `permission_id` | `uuid` FK -> `iam.permissions.id` | |

Indexes:
- unique (`role_id`, `permission_id`)
- index (`permission_id`)

### `iam.platform_role_assignments`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `user_id` | `uuid` FK -> `iam.users.id` | |
| `role_id` | `uuid` FK -> `iam.roles.id` | |
| audit columns | mixed | |

Indexes:
- unique (`user_id`, `role_id`)
- index (`role_id`)

### `projects.projects`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `key` | `varchar(32)` | Unique project/workspace key |
| `name` | `varchar(200)` | |
| `description` | `varchar(4000)` | |
| `status` | `smallint` | Draft, Active, Archived |
| `owner_user_id` | `uuid` FK -> `iam.users.id` | |
| audit columns | mixed | |

Indexes:
- unique (`key`)
- index (`owner_user_id`, `status`)

### `projects.project_memberships`
This table is the collaborator store for project-scoped access.

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid` FK -> `projects.projects.id` | |
| `user_id` | `uuid` FK -> `iam.users.id` | |
| `role` | `smallint` | Owner, Contributor, Reviewer, Viewer |
| `status` | `smallint` | Invited, Active, Removed |
| audit columns | mixed | |

Indexes:
- unique (`project_id`, `user_id`)
- index (`user_id`, `status`)

### `requirements.requirement_sets`
One logical requirement container per project in the current model.

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid` FK -> `projects.projects.id` | Unique |
| `name` | `varchar(200)` | |
| `overview` | `varchar(4000)` | Latest overview |
| `current_version_number` | `integer` | |
| audit columns | mixed | |

Indexes:
- unique (`project_id`)

### `requirements.requirement_set_versions`
Immutable snapshots used by recommendation and generation pipelines.

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `requirement_set_id` | `uuid` FK -> `requirements.requirement_sets.id` | |
| `version_number` | `integer` | |
| `summary` | `varchar(4000)` | |
| audit columns | mixed | |

Indexes:
- unique (`requirement_set_id`, `version_number`)
- index (`created_at_utc`)

### `requirements.requirement_items`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `requirement_set_version_id` | `uuid` FK -> `requirements.requirement_set_versions.id` | |
| `code` | `varchar(64)` | Version-local unique requirement code |
| `title` | `varchar(300)` | |
| `description` | `varchar(4000)` | |
| `type` | `smallint` | Functional, NonFunctional, Integration, Security, Data, Reporting |
| `priority` | `smallint` | Low, Medium, High, Critical |

Indexes:
- unique (`requirement_set_version_id`, `code`)
- index (`requirement_set_version_id`, `type`, `priority`)

### `requirements.constraint_items`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `requirement_set_version_id` | `uuid` FK -> `requirements.requirement_set_versions.id` | |
| `title` | `varchar(300)` | |
| `description` | `varchar(4000)` | |
| `type` | `smallint` | Business, Technical, Regulatory, Cost, Timeline, Platform |
| `severity` | `smallint` | Advisory, Important, Mandatory |

Indexes:
- index (`requirement_set_version_id`, `type`, `severity`)

### `generation.recommendation_sets`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid` FK -> `projects.projects.id` | |
| `requirement_set_version_id` | `uuid` FK -> `requirements.requirement_set_versions.id` | |
| audit columns | mixed | |

Indexes:
- index (`project_id`, `created_at_utc`)
- index (`requirement_set_version_id`)

### `generation.recommendations`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `recommendation_set_id` | `uuid` FK -> `generation.recommendation_sets.id` | |
| `artifact_kind` | `smallint` | Target artifact family |
| `title` | `varchar(256)` | |
| `rationale` | `varchar(4000)` | Explainable recommendation text |
| `confidence_score` | `numeric(5,4)` | Score in 0.0000-1.0000 range |
| `strength` | `smallint` | Low, Medium, High |

Indexes:
- unique (`recommendation_set_id`, `artifact_kind`)

### `generation.generation_requests`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid` FK -> `projects.projects.id` | |
| `requirement_set_version_id` | `uuid` FK -> `requirements.requirement_set_versions.id` | |
| `status` | `smallint` | Queued, Processing, Completed, Failed |
| `started_at_utc` | `timestamptz null` | |
| `completed_at_utc` | `timestamptz null` | |
| `failure_reason` | `varchar(4000) null` | |
| audit columns | mixed | |

Indexes:
- index (`project_id`, `status`, `created_at_utc`)
- index (`requirement_set_version_id`)

### `generation.generation_request_targets`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `generation_request_id` | `uuid` FK -> `generation.generation_requests.id` | |
| `artifact_kind` | `smallint` | |
| `preferred_format` | `smallint` | Markdown, Mermaid, PlantUML, PDF, PNG |

Indexes:
- unique (`generation_request_id`, `artifact_kind`)

### `artifacts.generated_artifacts`
Logical artifact record, independent of individual versions.

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid` FK -> `projects.projects.id` | |
| `artifact_kind` | `smallint` | |
| `title` | `varchar(300)` | |
| `status` | `smallint` | Draft, Reviewed, Approved, Superseded |
| `current_version_number` | `integer` | |
| audit columns | mixed | |

Indexes:
- index (`project_id`, `status`)
- index (`project_id`, `artifact_kind`, `created_at_utc`)

### `artifacts.uml_profiles`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `generated_artifact_id` | `uuid` FK -> `artifacts.generated_artifacts.id` | Unique one-to-one |
| `diagram_type` | `smallint` | UseCase, Class, Sequence, Activity, Component, Deployment |
| `supports_mermaid` | `boolean` | |
| `supports_plant_uml` | `boolean` | |

Indexes:
- unique (`generated_artifact_id`)
- index (`diagram_type`)

### `artifacts.artifact_versions`
Recommended as metadata-only in the long-term production schema.

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `generated_artifact_id` | `uuid` FK -> `artifacts.generated_artifacts.id` | |
| `version_number` | `integer` | |
| `generation_request_id` | `uuid null` FK -> `generation.generation_requests.id` | |
| `primary_format` | `smallint` | |
| `summary` | `varchar(4000)` | |
| `representations_jsonb` | `jsonb null` | Lightweight map of alternate textual renderings |
| audit columns | mixed | |

Indexes:
- unique (`generated_artifact_id`, `version_number`)
- index (`generation_request_id`)

### `artifacts.artifact_version_payloads`
Recommended production split for large generated content.

| Column | Type | Notes |
| --- | --- | --- |
| `artifact_version_id` | `uuid` PK/FK -> `artifacts.artifact_versions.id` | One-to-one payload row |
| `primary_content` | `text` | Large generated source body |
| `primary_content_sha256` | `varchar(64)` | Optional integrity check |
| `storage_mode` | `smallint` | Inline, ExternalObjectStorage |
| `external_storage_key` | `varchar(512) null` | For future object storage |

Indexes:
- primary key (`artifact_version_id`)

### `artifacts.artifact_exports`
Recommended as export job/result metadata.

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `artifact_version_id` | `uuid` FK -> `artifacts.artifact_versions.id` | |
| `format` | `smallint` | |
| `status` | `smallint` | Queued, Completed, Failed |
| `file_name` | `varchar(256)` | |
| audit columns | mixed | |

Indexes:
- index (`artifact_version_id`, `format`, `created_at_utc`)

### `artifacts.artifact_export_payloads`
Recommended production split for exported file bodies.

| Column | Type | Notes |
| --- | --- | --- |
| `artifact_export_id` | `uuid` PK/FK -> `artifacts.artifact_exports.id` | |
| `content` | `text null` | Inline for textual formats |
| `content_sha256` | `varchar(64) null` | |
| `storage_mode` | `smallint` | Inline, ExternalObjectStorage |
| `external_storage_key` | `varchar(512) null` | For PDF/PNG or large bundles |

Indexes:
- primary key (`artifact_export_id`)

### `generation.templates`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid null` FK -> `projects.projects.id` | Null means global template |
| `name` | `varchar(200)` | |
| `description` | `varchar(2000)` | |
| `status` | `smallint` | Draft, Active, Retired |
| `current_version_number` | `integer` | |
| audit columns | mixed | |

Indexes:
- index (`project_id`, `name`)
- index (`status`)

### `generation.template_versions`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `template_id` | `uuid` FK -> `generation.templates.id` | |
| `version_number` | `integer` | |
| `content` | `text` | |
| `supported_artifact_kinds_csv` | `text` | Candidate for later normalization if querying becomes important |
| audit columns | mixed | |

Indexes:
- unique (`template_id`, `version_number`)

### `generation.generation_rules`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid null` FK -> `projects.projects.id` | Null means global rule |
| `name` | `varchar(200)` | |
| `description` | `varchar(2000)` | |
| `scope` | `smallint` | Global or Project |
| `current_version_number` | `integer` | |
| audit columns | mixed | |

Indexes:
- index (`project_id`, `name`, `scope`)

### `generation.generation_rule_versions`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `generation_rule_id` | `uuid` FK -> `generation.generation_rules.id` | |
| `version_number` | `integer` | |
| `rule_definition_jsonb` | `jsonb` | Structured rule payload |
| audit columns | mixed | |

Indexes:
- unique (`generation_rule_id`, `version_number`)

### `collaboration.comment_threads`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `project_id` | `uuid` FK -> `projects.projects.id` | |
| `target_type` | `smallint` | Project, RequirementSetVersion, RecommendationSet, GeneratedArtifact, ArtifactVersion |
| `target_id` | `uuid` | Polymorphic target identifier |
| `status` | `smallint` | Open, Resolved |
| audit columns | mixed | |

Indexes:
- unique (`project_id`, `target_type`, `target_id`)
- index (`project_id`, `status`)

### `collaboration.comments`
| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `comment_thread_id` | `uuid` FK -> `collaboration.comment_threads.id` | |
| `body` | `text` | |
| audit columns | mixed | |

Indexes:
- index (`comment_thread_id`, `created_at_utc`)

### `audit.audit_logs`
Append-only audit trail.

| Column | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` PK | |
| `actor_user_id` | `uuid null` FK -> `iam.users.id` | |
| `project_id` | `uuid null` FK -> `projects.projects.id` | |
| `action` | `varchar(128)` | e.g. `artifact.exported` |
| `entity_type` | `varchar(128)` | |
| `entity_id` | `varchar(128)` | Keep string to support non-uuid references if needed |
| `description` | `varchar(4000)` | Human-readable summary |
| `correlation_id` | `varchar(128) null` | |
| `metadata_jsonb` | `jsonb null` | Structured audit context |
| `created_at_utc` | `timestamptz` | Event time |
| `created_by_user_id` | `uuid null` | Usually equals `actor_user_id` |
| `last_modified_at_utc` | `timestamptz null` | Should remain null in practice |
| `last_modified_by_user_id` | `uuid null` | Should remain null in practice |

Indexes:
- index (`project_id`, `created_at_utc`)
- index (`actor_user_id`, `created_at_utc`)
- index (`entity_type`, `entity_id`, `created_at_utc`)
- index (`action`, `created_at_utc`)

## Relationship Notes
- `projects.projects` is the collaboration root.
- `projects.project_memberships` is the normalized collaborator table.
- `requirements.requirement_set_versions`, `generation.generation_rule_versions`, `generation.template_versions`, and `artifacts.artifact_versions` are immutable version tables.
- `generation.recommendation_sets` and `generation.generation_requests` must always point to a frozen `requirements.requirement_set_versions` row for reproducibility.
- `artifacts.generated_artifacts` is the logical artifact identity; `artifacts.artifact_versions` is the immutable history.
- `collaboration.comment_threads` uses `(target_type, target_id)` to attach discussion to multiple aggregate types without duplicating comment tables.

## JSONB Guidance
Use `jsonb` when the payload is:
- machine-structured
- variable in shape
- secondary to the main relational model
- not the primary filter dimension for core transactional workflows

Good `jsonb` candidates:
- `generation_rule_versions.rule_definition_jsonb`
- `artifacts.artifact_versions.representations_jsonb`
- `audit.audit_logs.metadata_jsonb`

Avoid `jsonb` for:
- users
- collaborators
- requirements
- constraints
- comments
- generation request targets
- core foreign key relationships

## Transactional Data vs Generated Payloads
The production schema should split large payloads away from hot transactional rows:

- keep lifecycle, status, provenance, and timestamps in `artifact_versions` and `artifact_exports`
- keep large source bodies or rendered files in `artifact_version_payloads` and `artifact_export_payloads`

Rationale:
- smaller hot rows improve index efficiency
- large text/blob access becomes opt-in
- future object storage is easier to introduce
- write amplification is reduced for status updates and approval workflows

## Performance Notes
- Favor composite indexes aligned to actual read paths rather than indexing every foreign key blindly.
- Keep recommendation and generation queries fast with indexes on project, status, and created timestamp.
- Use `text` for large payload columns so PostgreSQL can TOAST them efficiently.
- If audit volume grows significantly, consider monthly partitioning on `audit.audit_logs.created_at_utc`.
- If artifact payloads become large or binary, move the payload tables to object storage-backed indirection rather than expanding the main transactional database indefinitely.

## EF Core Mapping Notes
- Keep table ownership aligned to module boundaries, even inside one DbContext.
- Use explicit `ToTable(name, schema)` for every entity.
- Use `DeleteBehavior.Cascade` only within aggregates.
- Use `DeleteBehavior.Restrict` across aggregate boundaries such as user/project references from audit and collaboration.
- Map JSON columns explicitly with `.HasColumnType("jsonb")`.
- Keep large textual bodies as `text`.
- Prefer immutable version rows instead of updating historical content.
- Add migrations for all schema evolution work; do not rely on `EnsureCreated()` outside local bootstrap scenarios.
- Consider extracting inline artifact/export content into dedicated payload entities in the domain model before the first serious production rollout.
