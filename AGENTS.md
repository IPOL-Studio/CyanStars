# AI Guidelines

This document serves as the code of conduct and development context specification for AI programming assistants (Agent / GitHub Copilot / Cursor / Windsurf, etc.). When providing code suggestions, generating code, fixing defects, or refactoring for `IPOL-Studio/CyanStars`, these guidelines must be strictly adhered to.

## 1. Code Architecture

Review code through the perspective of a software architect and engineering manager. **Prioritize software architecture, domain abstraction, maintainability, and extensibility above all else, strictly adhering to SOLID principles.** Completing specific tasks is secondary to architectural integrity.

## 2. Code and Comment Style

### 1. Code Style

Strictly adhere to the `.\Cyan-Stars\.editorconfig` specification.

### 2. Commenting Guidelines

Comments and documentation are an integral part of the codebase's long-term lifecycle and are written for future collaborators; **they must never target the current conversational context**. Strictly adhere to the following rules:

- **Eliminate Conversational and Contextual Residue**: Strictly forbid any traces of dialogue, task metadata, or change logs within the code (e.g., "As requested by the user...", "Fixed the previous error here...", "Optimized existing logic..."). The code must be presented as a pristine, cohesive final deliverable written in a single pass.
- **Eliminate Out-of-Bounds Call-Chain Descriptions**: **Modules must focus solely on their own responsibilities.** Strictly avoid reverse-describing upstream callers, high-level business flows, or indirect downstream dependencies within implementations (e.g., "This method is invoked by GameManager and passes data to UI..."). Callees must not contain any assumptions regarding the caller's business logic.
- **Self-Explanatory Code and Restraint**: Prioritize self-documenting code. Write comments only when dealing with non-obvious domain contracts, complex algorithms, critical performance considerations, or implicit side effects.
- **Purely Objective Written Language**: Use rigorous, concise, and formal Simplified Chinese with standardized punctuation. Colloquialisms, internet slang, coined terms, and arbitrarily introduced unvetted concepts are strictly prohibited.
- **Documentation Comment Completeness**: For interfaces and public contracts, parameter, return value, and exception descriptions in documentation comments must either be entirely omitted (relying on self-explanatory naming) or comprehensively and strictly provided. Incomplete documentation tags are strictly prohibited.

## 3. Code Implementation

Architecture precedes implementation; implementation must serve design.

- **Null Safety**: Enable `#nullable` globally. Rely primarily on the compiler for null-state checks and avoid redundant defensive programming.
- **Error Handling**: Adopt a Fail-Fast design. Never swallow business exceptions silently; exposing issues is preferable to masking errors. Avoid over-engineering and maintain proportional design.
- **Concurrency and Cancellation Mechanisms**: When dealing with asynchronous, race-prone, or long-running operations, consistently pass a `CancellationToken`. Provided that risks are controlled, business degradation strategies (such as disallowing concurrency or rejecting cancellation) are permitted to achieve rapid delivery.
- **Dependency Introduction**: Introducing third-party libraries without explicit permission is strictly prohibited. You must obtain user consent and verify open-source license compatibility prior to adding any dependency.

## 4. Project Testing

- Write unit tests only for core domain models and critical branches.
- Test cases must cover comprehensive business edge cases, rather than merely validating the code modified in the current interaction.
- When unable to execute Unity engine compilation and runtime tests in the current environment, never force tests or fabricate/speculate test results; explicitly hand off verification to the user for compilation and testing.

## 5. Commits and Collaboration

- **Commit Control**: Do not create Git commits autonomously unless explicitly instructed by the user.
- **Unified Textual Style**: Commit messages, branch names, PR descriptions, and user-facing responses must all follow the same formal, concise, and restrained written style required for code comments above.
- **Interaction Boundaries**: When conversing with the user, focus directly on the delivered outcomes and avoid lengthy restatements of implementation steps.
