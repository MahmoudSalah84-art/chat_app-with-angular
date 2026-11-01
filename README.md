ChatMe 💬

A real-time, WhatsApp-style chat application built with Angular 21 and ASP.NET Core (.NET 10), with live messaging over SignalR, installable PWA support, and Web Push notifications.

Live demo: https://chat-me.runasp.net API: https://chatme-api.runasp.net

✨ Features
Authentication – register / login with JWT, route guards on the client
Direct & group chats – start a 1:1 conversation or create a group
Real-time messaging – instant delivery through SignalR (WebSockets)
Message actions – edit, delete, reply to a message, and emoji reactions
Attachments – images, files, video and audio, stored on Cloudinary
Message status – sent / delivered / read receipts
Typing indicators – see when the other person is typing
Profile & settings – update your profile, upload an avatar, switch theme
Web Push notifications – receive new messages even when the app/tab is closed
PWA – installable, service-worker powered, with an in-app "update available" prompt
RTL / Arabic-ready manifest and UI
🧱 Tech Stack
Layer	Technologies
Frontend	Angular 21, TypeScript 5.9, Tailwind CSS 4, RxJS, @microsoft/signalr, Angular Service Worker, Vitest
Backend	ASP.NET Core (.NET 10), SignalR, Entity Framework Core 10, ASP.NET Core Identity, JWT Bearer
Application layer	MediatR (CQRS), FluentValidation, domain events
Database	SQL Server
Integrations	Cloudinary (file storage), Web Push (VAPID)
DevOps	GitHub Actions (CI/CD), Docker + Nginx (frontend image), Swagger / OpenAPI
🏗️ Architecture

The backend follows Clean Architecture with CQRS:

chat_app-with-angular/
├── backend/chatme/
│   ├── chatme.Domain/          # Entities, value objects, domain events, Result type
│   ├── chatme.Application/     # Commands, queries, handlers, validators, DTOs, interfaces
│   ├── chatme.infrastructure/  # EF Core, Identity, JWT, Cloudinary, Web Push, push worker
│   └── chatme.API/             # Controllers, SignalR hub (ChatHub), middleware, Program.cs
├── frontend/                   # Angular app
│   └── src/app/
│       ├── core/               # API services, SignalR, auth, state, facade, guards, interceptors, models
│       ├── features/           # auth, chat-list, chat-window, new-chat, new-group, profile
│       ├── layout/             # main layout
│       └── shared/             # reusable components (avatar, app-update)
├── test/chatme.Application.Tests/   # Unit tests for application handlers
└── .github/workflows/          # CI.yaml, CD.yaml

How real-time works: the Angular client opens a SignalR connection to /hubs/chat (JWT passed via access_token). Domain events raised by the application layer (message sent, edited, deleted, read, reaction changed, …) are pushed to connected clients through IChatNotificationService. For offline users, a background worker dispatches Web Push notifications through a job queue.

🔌 API Overview

All endpoints are under /api and require a JWT unless noted.

Area	Endpoints
Auth	POST /auth/register, POST /auth/login (public)
Users	GET /users/me, PUT /users/me, GET /users/contacts, POST /users/me/avatar
Chats	GET /chats, GET /chats/{chatId}/messages, POST /chats/direct, POST /chats/group, POST /chats/{chatId}/read
Messages	POST /messages, PUT /messages, DELETE /messages, POST /messages/attachments
Push	GET /push/public-key, POST /push/subscribe, POST /push/unsubscribe

SignalR hub: /hubs/chat — SendMessage, EditMessage, DeleteMessage, MarkAsRead, MarkAsDelivered, ReactToMessage, StartTyping, StopTyping, JoinChatGroup.

Swagger UI is available in the Development environment.

🚀 Getting Started
Prerequisites
.NET SDK 10
Node.js 22+ and npm
SQL Server (LocalDB, Docker, or a remote instance)
A Cloudinary account (for attachments & avatars)
VAPID keys for Web Push (optional, only needed for push notifications)
1. Clone
bash
git clone https://github.com/MahmoudSalah84-art/chat_app-with-angular.git
cd chat_app-with-angular
2. Configure the backend

The committed appsettings.json does not contain secrets. Provide them with user secrets or an appsettings.Development.json:

bash
cd backend/chatme/chatme.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ChatMe;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<a-long-random-secret-at-least-32-chars>"
dotnet user-secrets set "Jwt:Issuer" "WhatsAppClone.Api"
dotnet user-secrets set "Jwt:Audience" "WhatsAppClone.Client"
dotnet user-secrets set "Cloudinary:CloudName" "<cloud-name>"
dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"
# Optional – Web Push
dotnet user-secrets set "WebPush:Subject" "mailto:you@example.com"
dotnet user-secrets set "WebPush:PublicKey" "<vapid-public-key>"
dotnet user-secrets set "WebPush:PrivateKey" "<vapid-private-key>"
3. Create the database
bash
dotnet ef database update --project ../chatme.infrastructure --startup-project .

If the repository has no migrations yet, create one first: dotnet ef migrations add InitialCreate --project ../chatme.infrastructure --startup-project .

4. Run the API
bash
dotnet run --launch-profile https

The API starts at https://localhost:7087 (Swagger at /swagger).

5. Run the frontend

Open frontend/src/environments/environment.ts and switch to the local URLs:

ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7087/api',
  hubUrl: 'https://localhost:7087/hubs/chat',
};

Then:

bash
cd frontend
npm ci
npm start

Open http://localhost:4200 (the origin allowed by the backend's CORS policy).

🧪 Tests
bash
# Backend (from the repo root)
dotnet test backend/chatme/chatme.slnx

# Frontend
cd frontend
npm test
🐳 Docker (frontend)
bash
cd frontend
docker build -f dockerfile.yml -t chatme-frontend .
docker run -p 8080:80 chatme-frontend
🔄 CI/CD

GitHub Actions workflows live in .github/workflows/:

CI – runs on pushes and PRs to develop / master: restores, builds and tests the .NET solution, builds the Angular app, and uploads both as artifacts.
CD – runs after a successful CI on master (or manually via workflow_dispatch): generates appsettings.Production.json from repository secrets and deploys the backend and frontend to MonsterASP via Web Deploy.

Required repository secrets include the DB connection string, JWT key, Cloudinary credentials, Web Push keys, and the MonsterASP deployment credentials.

🗺️ Roadmap
 Improve offline push notification delivery and notification actions
 Voice messages
 Message search
 End-to-end encryption
👤 Author

Mahmoud Mohamed Salah GitHub: @MahmoudSalah84-art