**Blazor WebAssembly Task Management Portal – PRD**

---

## 1. Purpose & Scope

A single-page application built entirely with Blazor WebAssembly for task tracking and collaboration. Targets small to medium teams needing a lightweight, install-free solution with offline support and exportable reports.

**In scope:**

- Core task CRUD and assignment workflows
- Role-based authentication & authorization
- Interactive dashboard with charts and CSV export
- AI/LLM-driven enhancements (optional phase)
- Docker-based local development and AWS ECS deployment

---

## 2. Technology Stack & Versions

- **.NET 9.0** — ASP.NET Core API, Blazor runtime
- **Entity Framework Core 9.0** (MySQL provider)
- **Blazor WebAssembly** (Razor components, CSS isolation)
- **Docker Compose** — local development (MySQL + API)
- **AI/LLM Integration:** OpenAI or Azure OpenAI .NET SDK (for optional features)

---

## 3. High-Level Architecture

```text
┌───────────────────────────┐          ┌──────────┐          ┌──────────┐
│ BlazorWasm.Client        │ ◀───────▶ │ API Server│ ◀──────▶ │ MySQL DB │
│ - Razor Components       │    HTTP  │ - ASP.NET │  MySQL  │(RDS on AWS)
│ - CSS Isolation          │          │   Core 9  │          │          │
└───────────────────────────┘          └──────────┘          └──────────┘
```

---

## 4. Data Models & Schema

### 4.1 Core Entities

- **User:** Id, Email, PasswordHash, FirstName, LastName, Role, CreatedAt, IsActive
- **Task:** Id, Title, Description, Status, Priority, AssigneeId, CreatorId, DueDate, CreatedAt, UpdatedAt, IsDeleted
- **Comment:** Id, TaskId, UserId, Content, CreatedAt, IsDeleted
- **Role:** Admin, User (enum-based for simplicity)
- **TaskStatus:** Pending, InProgress, Review, Done, Cancelled (enum)
- **Priority:** Low, Medium, High, Critical (enum)

### 4.2 Relationships

- User (1) → Tasks (Many) [as Assignee]
- User (1) → Tasks (Many) [as Creator]
- Task (1) → Comments (Many)
- User (1) → Comments (Many)

---

## 5. Component Breakdown

### 5.1 BlazorWasm.Client

- **Core Libraries:** Blazor WebAssembly, CSS isolation
- **Main Components:**

  - **Login.razor** — form validation, error display, redirect on success
  - **Dashboard.razor** — interactive charts via JS interop; refresh on data change
  - **TaskList.razor** — server-side pagination, search by title/assignee, multi-field sorting
  - **TaskDetail.razor** — edit form with client-/server-side validation, markdown support in comments
  - **Notification.razor** — toast notifications for success/error; debounce to avoid spam

- **Additional Features:**

  - **Search & Filter:** live filter box with debounce; filter by status, assignee, date range
  - **Pagination Controls:** page size selector (10/25/50); jump-to-page input
  - **Validation & Error Handling:** FluentValidation on client; display ModelState errors
  - **Accessibility:** ARIA roles; keyboard navigation; high-contrast CSS theme
  - **Localization:** resource files for UI strings (English + placeholder for future en-US, fr-FR)

- **Offline Support:** Service Worker caches assets and last successful API responses; offline indicator in UI
- **Logging:** capture client errors via JavaScript interop to API logging endpoint
- **Testing:** bUnit for component tests (≥90% coverage)

### 5.2 BlazorWasm.Server (API)

- **Framework:** ASP.NET Core 9.0
- **Data Access:** EF Core 9.0 (MySQL provider); migrations applied at startup
- **Authentication:** JWT via ASP.NET Identity; refresh tokens; role policy attributes
- **CORS:** Configured to allow Blazor WASM client origin; credentials enabled for JWT
- **Endpoints & Behaviors:**

  - `POST /api/auth/login` — returns JWT + refresh token; throttle on repeated failures
  - `GET /api/tasks` — supports `page`,`pageSize`,`search`,`sort`,`filterStatus`; returns total count header
  - `POST /api/tasks` — enforces required fields; returns 201 with `Location` header
  - `PUT /api/tasks/{id}` — handles partial updates via PATCH semantics
  - `DELETE /api/tasks/{id}` — soft delete with `IsDeleted` flag
  - `GET /api/tasks/export` — streams CSV with proper escaping; supports large data (>50k rows) via buffering

- **Configuration & Secrets:**

  - Connection strings: environment variables via ASP.NET Core configuration
  - JWT and hashing keys: stored securely in AWS Secrets Manager

- **Logging & Monitoring:**

  - Structured logging with Serilog to console and file
  - Health checks on `/healthz` for database connectivity
  - Metrics endpoint `/metrics` for Prometheus scraping

- **Testing:**

  - Unit: xUnit for services/controllers; Moq for dependencies
  - Integration: Testcontainers MySQL; run migrations and seed in CI

---

## 6. Feature Set & Acceptance Criteria

| Feature                    | Priority | Acceptance Criteria                                                                                                                                                                                                         |
| -------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Authentication & Roles     | High     | Users sign in/up; JWT + refresh token issued; Admin role can manage users and all tasks; User role can only manage assigned tasks; access denied (403) when unauthorized; lockout after 5 failed attempts in 15 min window. |
| Task CRUD                  | High     | Create/Read/Update/Delete tasks via API; UI shows correct states; soft-deleted tasks hidden; rollback on failure.                                                                                                           |
| Search & Filter            | High     | Tasks filter within 300ms; correct results for partial matches; clear filter resets list.                                                                                                                                   |
| Pagination                 | High     | Page navigation works; total pages accurate; changing page size updates display.                                                                                                                                            |
| Dashboard Charts           | Medium   | Chart displays counts by status; updates on data change; readable labels; accessible tooltips.                                                                                                                              |
| CSV Export                 | Medium   | Exports current list view; supports streaming large datasets; download triggers in <5s for 50k rows.                                                                                                                        |
| AI/LLM-Driven Enhancements | Medium   | Features enabled via config; meet individual acceptance criteria per feature below.                                                                                                                                         |
| Dark/Light Mode            | Low      | Toggle persists in localStorage; respects OS theme; accessible color contrast (WCAG AA).                                                                                                                                    |

---

## 7. AI & LLM-Driven Enhancements (Phase 2)

Leverage generative AI and large language models (LLMs) to streamline task management, improve collaboration, and surface insights. These features are optional modules activated per user/team needs:

**7.1 Natural-Language Task Creation**

- **Description:** Allow users to type or speak plain-language requests (e.g. “Create a task to prepare Q3 financial report due next Friday with Alice”) and auto-generate structured tasks.
- **Implementation:** Integrate OpenAI/Azure OpenAI .NET SDK; Blazor client captures input, calls server endpoint that prompts LLM; map response to Task model.
- **Acceptance Criteria:** Parsed tasks have correct title, description, assignee, and due date in ≥90% of tests; <1s latency for LLM call.

**7.2 Task Summarization & Contextual Insights**

- **Description:** Generate concise summaries of long task descriptions or comment threads; highlight key action items.
- **Implementation:** On-demand server API invokes LLM summarization; results cached for offline access.
- **Acceptance Criteria:** Summaries under 100 words accurately reflect content in user evaluation; UI displays “Summarized at” timestamp.

**7.3 Priority & Deadline Recommendations**

- **Description:** Analyze task metadata and historical completion times to suggest realistic due dates and priority levels.
- **Implementation:** Train a lightweight regression model or prompt LLM with contextual data; surface suggestions in TaskDetail.razor.
- **Acceptance Criteria:** Recommendation accepted by users ≥75% of the time in usability testing; suggestions shown in <500ms.

**7.4 Conversational Assistant Interface**

- **Description:** Embed a chat widget powered by an LLM that can answer questions like “What are my overdue tasks?” or “Show me tasks assigned to Bob this week.”
- **Implementation:** Blazor component wraps a chat UI; server routes messages to LLM with system instructions and fetches data from API as needed.
- **Acceptance Criteria:** Chat queries return accurate data in ≥95% of cases; fallback to “I’m not sure” for unsupported queries; UI latency <1.5s.

**7.5 Sentiment Analysis for Comments**

- **Description:** Automatically detect sentiment in user comments to flag potential issues (e.g. frustration, blockers).
- **Implementation:** Use an LLM or sentiment API to score new comments; display sentiment icon next to each comment.
- **Acceptance Criteria:** Sentiment scores agree with human labels ≥80%; icon appears within 200ms of posting.

**7.6 Automated Report & Notification Drafting**

- **Description:** Generate email or in-app notification drafts summarizing daily or weekly progress, including key metrics and action items.
- **Implementation:** Scheduled job calls LLM with task data snapshots; generates markdown or HTML for emails and notifications.
- **Acceptance Criteria:** Generated content requires minimal edits (<10% modification) in internal review; job completes within scheduled window.

**7.7 Localization & Translation**

- **Description:** Offer on-the-fly translation of task descriptions and comments for global teams.
- **Implementation:** Integrate translation capability of LLM; Blazor client toggles translation view.
- **Acceptance Criteria:** Translations maintain meaning with BLEU score ≥0.6 in automated tests; toggle response <300ms.

---

## 8. Data Flow & Sequence

1. **Login** → POST credentials → API validates → JWT + refresh token issued → store in secure storage
2. **Load Tasks** → GET `/api/tasks` with query parameters → API returns paginated list + `X-Total-Count`
3. **Task Operations** → client POST/PUT/DELETE → API persists changes → client updates list (optimistic UI)
4. **Notifications** → client displays toast on success/error; logs error details
5. **CSV Export** → GET `/api/tasks/export?search=&filterStatus=&sort=&pageSize=` → API streams CSV

---

## 9. Local Development & Deployment

### 9.1 Docker Compose (Dev)

```yaml
version: "3.8"
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: tasksdb
    ports: ["3306:3306"]
  server:
    build: ./BlazorWasm.Server
    environment:
      ConnectionStrings__Default: Server=mysql;Database=tasksdb;User=root;Password=root;
      ASPNETCORE_ENVIRONMENT: Development
    ports: ["5001:80"]
    depends_on:
      - mysql
```

### 9.2 AWS ECS (Prod)

- **Cluster:** Fargate
- **Services:**

  - **API:** min 2, max 10 tasks; scale on CPU >60%; environment secrets from Secrets Manager
  - **Client:** single Fargate task behind CloudFront with WAF

- **RDS MySQL:** Multi-AZ, 20 GB gp3; automated backups
- **CI/CD:** GitHub Actions builds & tests; Docker push; Terraform apply; ECS deploy; post-deploy smoke tests

---

## 10. Database Initialization

On first run, if `Tasks` is empty, `DbInitializer` seeds:

- Users: admin and standard user with strong, hashed passwords
- Sample tasks across statuses for e2e testing

---

## 11. Testing Strategy

- **Unit Tests:** bUnit (client), xUnit (server); target ≥90% coverage
- **Integration Tests:** Testcontainers for MySQL in CI; run migrations and seed data
- **E2E Tests:** Cypress against Docker Compose on PR; cover critical flows
- **Load Testing:** k6 script simulating 100+ concurrent users; benchmark API

---

## 12. Documentation & Deliverables

- **README.md:** setup, env variables, run & test steps
- **Mermaid diagrams:** login flow, data flow, component interactions
- **Screenshots & GIFs:** login, task CRUD, charts, CSV export

---

_This PRD outlines a comprehensive Blazor-native task management portal with optional AI/LLM modules, ensuring a clear implementation roadmap and measurable success criteria._
