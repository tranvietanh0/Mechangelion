---
origin: theonekit-core
repository: The1Studio/theonekit-core
module: null
protected: true
---
# Code Conventions (Universal)

Applies to ALL languages and frameworks. Kit-specific rules extend this in `code-conventions-{kit}.md`.

## SOLID Principles
- **Single Responsibility:** one class/function = one reason to change
- **Open/Closed:** extend via composition, not modification
- **Liskov Substitution:** subtypes must be substitutable for base types
- **Interface Segregation:** prefer small, focused interfaces
- **Dependency Inversion:** depend on abstractions, not concretions

## Naming
- Names must be self-documenting — if a name needs a comment, rename it
- Booleans: use `is`, `has`, `can`, `should` prefixes
- Functions: use verbs — `getUser`, `calculateTotal`, `validateInput`
- Avoid abbreviations except widely known ones (`id`, `url`, `api`)

## Structure
- One class/component per file (small related types may share)
- Max 200 lines per file — split if larger
- Guard clauses over nested if/else — return early
- Prefer composition over inheritance
- Prefer immutability (`const`, `readonly`, `final`) by default

## Code Quality
- No magic numbers — extract to named constants or config
- No empty catch blocks — handle or rethrow with context
- No `TODO` in merged code — track in issues instead
- Import order: stdlib → external packages → internal modules
- Comments only where logic isn't self-evident — code should be readable without them

## Testing
- Test public behavior, not implementation details
- Each test should be independent — no shared mutable state
- Name tests descriptively: `should_returnError_when_inputIsNull`

## Living Document
If unsure about a convention not covered here, ask the user for their preference and update this file with the answer. Conventions grow from real decisions.
