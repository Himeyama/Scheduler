# OpenCode Guidelines for Scheduler

## Build, Lint, and Test Commands
- **Build**: `dotnet build Scheduler.sln`
- **Lint**: `dotnet format` (Ensure code adheres to formatting standards)
- **Run Tests**: `dotnet test` to run all tests.
- **Run Single Test**: `dotnet test <TestProject>.dll --filter "FullyQualifiedName~<TestMethodName>"`

## Code Style Guidelines
### Imports
- Use clear and concise namespace imports. Avoid unused imports.

### Formatting
- Use 4 spaces for indentation.
- Place opening braces on the same line as declaration.

### Types
- Prefer using explicit types instead of `var` for clarity.

### Naming Conventions
- Use PascalCase for class names and method names.
- Use camelCase for method parameters and local variables.

### Error Handling
- Use exceptions for handling errors instead of return codes. Make sure to log exceptions appropriately to aid debugging.