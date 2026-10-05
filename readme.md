# Forumish

A comments SPA built for the .NET / Entity Framework assignment, with persistent discussions, nested replies, and asynchronous attachment processing.

## Implemented features

- Post comments and replies with an alphanumeric username, email, optional homepage, and required image CAPTCHA; client and server validation.
- Nested reply threads; sort by username, email, or creation date in either direction. Newest first by default, with 25 comments per page and selectable page sizes.
- Formatting buttons for `a`, `code`, `i`, and `strong`; server HTML sanitization and XHTML serialization, Angular sanitization, and parameterized EF queries.
- One JPEG, PNG, GIF, or TXT attachment per comment. Images are proportionally resized to fit 320 x 240; TXT files are limited to 100 KiB. Uploaded images may be up to 10 MiB. Published files have inline previews and open links.
- Client identification using SHA-256 hashes of IP address and browser fingerprint, plus rate limits for comment submission and CAPTCHA generation.

## Technologies and their roles

**Backend:** .NET 10 / ASP.NET Core Minimal APIs, EF Core 10, SQL Server 2022, Redis 7, Hangfire, Mediator, FluentValidation, AutoMapper, HtmlSanitizer / AngleSharp, ImageSharp, Lazy.Captcha / SkiaSharp, Polly 8, and Serilog. Feature-based layers separate domain, orchestration, infrastructure, and presentation.

- **Persistence:** EF Core maps users, comments, reply relationships, and attachment metadata to SQL Server. Migrations define the schema; file contents are stored on disk.
- **CAPTCHA:** Lazy.Captcha / SkiaSharp generates the images. Redis stores verification codes for 120 seconds; verification atomically retrieves and deletes an entry so each challenge can be used only once.
- **Comment caching:** ASP.NET Core output caching stores comment-list responses in memory for 60 seconds. Cache entries vary by page, page size, sort field, direction, and parent comment, keeping different discussion views separate.
- **Event-driven eviction:** Creating a comment or reply records a `CommentCreatedDeferredEvent`. After EF successfully saves the changes, an interceptor dispatches the event through Mediator. Its handler evicts entries tagged `discussion-comments`, so subsequent reads fetch the updated discussion from the database.
- **Background processing:** Hangfire uses a SQL Server job queue to validate and process attachments outside the submission request. ImageSharp resizes images and removes metadata; text processing validates TXT uploads. A recovery service reschedules stale pending attachments.
- **Validation and mapping:** FluentValidation validates API inputs, AutoMapper maps request/response models, and HtmlSanitizer / AngleSharp removes disallowed markup and serializes messages as XHTML. Serilog records requests and application events.

**Frontend:** Angular 22 and TypeScript 6 provide the SPA and reactive forms; RxJS coordinates requests and UI state. FingerprintJS supplies the browser fingerprint, and CSS styles the discussion and composer. Nginx serves the frontend, proxies API requests on the same origin, and applies security headers.

