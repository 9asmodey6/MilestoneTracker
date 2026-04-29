# Milestone Tracker Bot 🍼✨

A modern Telegram bot designed to help parents track and preserve their children's important milestones. Built with a focus on clean code, performance, and maintainability.

## Architecture & Design Patterns

The project follows a hybrid architectural approach to combine the best of both worlds:

*   **Clean Architecture**: Separation of concerns across Domain, Application, and Infrastructure layers.
*   **Vertical Slice Architecture (VSA)**: Inside the features folder, each use case (like "Add Milestone" or "Get Children") is encapsulated in its own slice, making it easier to navigate and modify specific logic without affecting the rest of the system.
*   **State Machine Flow**: A robust state-based engine handles complex multi-step interactions (e.g., a wizard-like flow for adding a milestone with photos, dates, and descriptions).
*   **Handler Factory**: A dynamic factory pattern is used to resolve the correct state handler for every incoming update, ensuring the main logic remains clean and scalable.

## Performance Features

*   **Instant Response (Non-blocking)**: The bot utilizes an **internal update queue** and a **Background Worker**. This allows the Webhook to respond to Telegram servers in approximately **1.5 ms**, while the actual processing happens asynchronously.
*   **MediatR**: Used to decouple request handling from the API/Infrastructure, promoting a "thin controller" approach.
*   **Stateless Navigation**: Milestone retrieval uses DTO-based state management, minimizing unnecessary database roundtrips.


## Key Features

### Secure Shared Access
The system allows parents to share access to a child's profile with other family members using a secure, token-based mechanism:
*   **Unique Tokens**: Generate a one-time secure HEX token (e.g., `a1b2c3d4e5f6`) valid for 24 hours.
*   **Many-to-Many Linking**: A child can be linked to multiple parent accounts, allowing everyone to view and contribute to the same timeline.
*   **Validation**: The system ensures tokens are used only once, haven't expired, and don't grant duplicate access.

## Tech Stack

- **Backend**: .NET 8
- **Database**: PostgreSQL with Entity Framework Core
- **Bot API**: Telegram.Bot library
- **Messaging**: MediatR
- **Logging**: Serilog
- **Locking**: AsyncKeyedLock (for thread-safe chat processing)


## Roadmap

- [ ] Edit/Delete milestones
- [ ] Multi-language support (English/Ukrainian)
- [ ] Analytics for child growth 

## Project Status: Work in Progress

> [!NOTE]
> This project is currently under active development. Some flows are incomplete or simplified.

*   **Language**: The bot's UI is currently in **Russian**, as it's optimized for personal/local use.
*   **Flows**: While the core "Add" and "View" flows are functional, advanced features like editing/deleting milestones are still in the roadmap.
*   **Goal**: The primary goal is to create a reliable and fast personal tool for capturing memories that last forever.

---
*Created with ❤️ for personal use and learning.*
