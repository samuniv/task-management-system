# GitHub Copilot Instructions

## Core Commands

### Build & Run

- **Development**: `docker-compose up` (MySQL + API + Client as per PRD)
- **Client only**: `dotnet run --project BlazorWasm.Client`
- **Server only**: `dotnet run --project BlazorWasm.Server`
- **Build**: `dotnet build`
- **Restore**: `dotnet restore`

### Database

- **Migrations**: `dotnet ef migrations add <name> --project BlazorWasm.Server`
- **Update DB**: `dotnet ef database update --project BlazorWasm.Server`
- **Drop DB**: `dotnet ef database drop --project BlazorWasm.Server`

### Testing

- **Unit Tests**: `dotnet test`
- **Integration Tests**: `dotnet test --filter Category=Integration`
- **E2E Tests**: `npm run cypress:run` (Cypress against Docker Compose)
- **Load Tests**: `k6 run load-test.js` (200+ concurrent users)

### Task Management

- **Initialize**: Use `initialize_project` MCP tool (preferred) or `task-master init`
- **List tasks**: Use `get_tasks` MCP tool or `task-master list`
- **Next task**: Use `next_task` MCP tool or `task-master next`
- **Show task**: Use `get_task` MCP tool or `task-master show <id>`
- **Task status**: Use `set_task_status` MCP tool or `task-master set-status --id=<id> --status=<status>`

## High-Level Architecture

### Technology Stack

- **.NET 9 STS (9.0.6)** - ASP.NET Core API, Blazor runtime
- **Entity Framework Core 9.0.6** - MySQL provider
- **Blazor WebAssembly** - Razor components, CSS isolation
- **MySQL Database** - RDS on AWS for production
- **Docker Compose** - Local development environment
- **AI/LLM Integration** - OpenAI/Azure OpenAI .NET SDK for optional features

### Component Structure

```
BlazorWasm.Client (Blazor WebAssembly)
├── Components/
│   ├── Login.razor - JWT auth, form validation
│   ├── Dashboard.razor - Charts via JS interop
│   ├── TaskList.razor - Server-side pagination, search
│   ├── TaskDetail.razor - CRUD forms, markdown support
│   └── Notification.razor - Toast notifications
├── Services/ - HttpClient wrappers
└── wwwroot/ - Static assets, Service Worker

BlazorWasm.Server (ASP.NET Core API)
├── Controllers/ - REST endpoints
├── Models/ - Entity models
├── Data/ - EF Core context, migrations
├── Services/ - Business logic
└── Program.cs - JWT config, health checks
```

### Key Endpoints

- `POST /api/auth/login` - JWT + refresh token
- `GET /api/tasks` - Paginated with search/filter
- `POST /api/tasks` - Create with validation
- `PUT /api/tasks/{id}` - Update (PATCH semantics)
- `DELETE /api/tasks/{id}` - Soft delete
- `GET /api/tasks/export` - CSV streaming
- `GET /healthz` - Health checks
- `GET /metrics` - Prometheus metrics

### External Dependencies

- **MySQL** - Primary data store
- **JWT tokens** - Authentication/authorization
- **Serilog** - Structured logging
- **AI/LLM APIs** - Optional enhancements
- **AWS Services** - ECS deployment, RDS, Secrets Manager

## Style Rules

### C# & .NET Conventions

- **Async/await**: Always use for I/O operations, suffix methods with `Async`
- **Nullable reference types**: Enable and use appropriately
- **Records**: Use for DTOs and immutable data structures
- **Primary constructors**: Use in .NET 8+ for cleaner syntax
- **Configuration**: Use `IOptions<T>` pattern for settings
- **Dependency injection**: Register services in `Program.cs`, use constructor injection

### Blazor Component Guidelines

- **CSS Isolation**: Use `.razor.css` files for component-specific styles
- **Parameters**: Use `[Parameter]` attributes, validate in `OnParametersSet`
- **State management**: Use `@bind` for two-way binding, avoid direct DOM manipulation
- **JS Interop**: Minimal usage, prefer Blazor native approaches
- **Error boundaries**: Implement `ErrorBoundary` components for graceful failures
- **Accessibility**: Use ARIA roles, keyboard navigation, high contrast support

### Entity Framework Patterns

- **DbContext**: Single context per request, configure in `Program.cs`
- **Migrations**: Descriptive names, apply at startup for development
- **Queries**: Use `Select` projections, avoid N+1 problems
- **Soft deletes**: Use `IsDeleted` flag, filter in global query filters
- **Validation**: Use data annotations and fluent validation

### API Design

- **REST conventions**: Use appropriate HTTP verbs and status codes
- **Error handling**: Return consistent error response format
- **Pagination**: Include `X-Total-Count` header for client-side paging
- **Throttling**: Implement rate limiting for auth endpoints
- **CORS**: Configure appropriately for Blazor WASM client

### Security & Authentication

- **JWT**: Use refresh tokens, validate issuer/audience
- **HTTPS**: Enforce in production, redirect in development
- **Input validation**: Server-side validation always, client-side for UX
- **SQL injection**: Use parameterized queries, EF Core protects by default
- **XSS**: Blazor protects by default, be careful with raw HTML

### Testing Patterns

- **Unit tests**: xUnit, Moq for dependencies, ≥90% coverage target
- **Integration tests**: Testcontainers for MySQL, test full request pipeline
- **Component tests**: bUnit for Blazor components, ≥80% coverage target
- **E2E tests**: Cypress for critical user flows

### Error Handling

- **Global exception handling**: Use middleware for unhandled exceptions
- **Logging**: Structured logging with Serilog, log to console and file
- **Client errors**: Capture via JS interop, send to API logging endpoint
- **User feedback**: Toast notifications for success/error states

### Performance Guidelines

- **Database**: Use indices, optimize queries, consider read replicas
- **Blazor**: Minimize re-renders, use `@key` for list items
- **Caching**: HTTP caching headers, in-memory caching where appropriate
- **Bundle size**: Tree shaking, lazy loading for large components

## Agent Rules Integration

### Taskmaster Workflow

- Use **MCP tools** over CLI commands when available (better performance, structured data)
- Follow the [development workflow](instructions/dev_workflow.md) for task-driven development
- Use tagged task lists for feature branches and team collaboration
- Run complexity analysis before expanding tasks
- Update subtasks with implementation progress and findings

### VS Code Integration

- Follow [VS Code rules structure](instructions/vscode_rules.md) for creating/updating guidelines
- Use [self-improvement guidelines](instructions/self_improve.md) for rule maintenance
- Reference files using `mdc:` syntax for clickable links
- Maintain rule quality through continuous updates

### AI/LLM Configuration

- AI features configured via `.taskmaster/config.json` (not environment variables)
- API keys stored in `.env` (CLI) or `.vscode/mcp.json` (MCP)
- Use research role for informed task creation and updates
- Leverage AI for natural language task creation, summarization, priority recommendations

## Project-Specific Context

### Business Requirements

- Single-page application for small to medium teams
- Offline support with Service Worker
- Role-based authentication and authorization
- Interactive dashboard with charts and CSV export
- Optional AI/LLM enhancements for productivity

### Deployment Strategy

- **Development**: Docker Compose with MySQL
- **Production**: AWS ECS Fargate, RDS MySQL, CloudFront
- **CI/CD**: GitHub Actions, Terraform, automated testing
- **Monitoring**: Health checks, Prometheus metrics, structured logging

### Feature Priorities

1. **High**: Authentication, Task CRUD, Search/Filter, Pagination
2. **Medium**: Dashboard Charts, CSV Export, AI Enhancements
3. **Low**: Dark/Light Mode, Localization

### AI Enhancement Features

- Natural-language task creation
- Task summarization and insights
- Priority and deadline recommendations
- Conversational assistant interface
- Sentiment analysis for comments
- Automated report drafting
- Translation support

Remember: This is a Blazor WebAssembly project focused on task management with modern .NET practices, comprehensive testing, and optional AI features for enhanced productivity.
