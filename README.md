# 📱 Phone Bill Manager — Full Stack Application

## Overview
A full-stack mobile-friendly web application that allows users to:
- Register & log in securely
- Upload carrier-generated phone bill PDFs
- Auto-parse plan, equipment, and service charges
- Split costs per line on the account
- Assign names to each phone line
- Send individualised bill summaries via WhatsApp or SMS (Twilio)

---

## 🏗️ Tech Stack

| Layer        | Technology                              |
|--------------|-----------------------------------------|
| Frontend     | Next.js 14 (App Router) + TypeScript    |
| UI Library   | Tailwind CSS + shadcn/ui                |
| Backend API  | C# .NET 8 Web API                       |
| ORM          | Entity Framework Core 8                 |
| Database     | SQL Server (SSMS)                       |
| PDF Parsing  | PdfPig (C# NuGet)                       |
| Auth         | JWT Bearer Tokens                       |
| Messaging    | Twilio (WhatsApp + SMS)                 |
| File Storage | Local disk / Azure Blob (configurable)  |

---

## 📁 Full Project Folder Structure

```
PhoneBillManager/
│
├── README.md                          ← You are here
├── database/
│   └── schema.sql                     ← All SQL Server tables, views, stored procedures
│
├── backend/
│   └── PhoneBillManager.Api/
│       ├── PhoneBillManager.Api.csproj
│       ├── Program.cs                 ← App entry point, DI, middleware
│       ├── appsettings.json           ← DB connection, Twilio, JWT config
│       │
│       ├── Controllers/
│       │   ├── AuthController.cs      ← Register / Login
│       │   ├── BillsController.cs     ← Upload, parse, list, get bill
│       │   ├── LinesController.cs     ← Assign names/contacts to lines
│       │   └── NotificationsController.cs ← Send WhatsApp/SMS
│       │
│       ├── Models/                    ← EF Core entity classes (map to DB tables)
│       │   ├── AppUser.cs
│       │   ├── Bill.cs
│       │   ├── AccountLine.cs
│       │   ├── PlanCharge.cs
│       │   ├── EquipmentCharge.cs
│       │   ├── ServiceCharge.cs
│       │   └── Notification.cs
│       │
│       ├── DTOs/                      ← Request/Response data shapes
│       │   ├── Auth/
│       │   │   ├── RegisterRequest.cs
│       │   │   ├── LoginRequest.cs
│       │   │   └── AuthResponse.cs
│       │   ├── Bills/
│       │   │   ├── BillSummaryDto.cs
│       │   │   ├── BillDetailDto.cs
│       │   │   └── UploadBillResponse.cs
│       │   ├── Lines/
│       │   │   ├── LineDto.cs
│       │   │   └── AssignLineRequest.cs
│       │   └── Notifications/
│       │       └── SendNotificationRequest.cs
│       │
│       ├── Services/
│       │   ├── Interfaces/
│       │   │   ├── IAuthService.cs
│       │   │   ├── IBillParserService.cs
│       │   │   ├── IBillService.cs
│       │   │   ├── ILineService.cs
│       │   │   └── INotificationService.cs
│       │   ├── AuthService.cs         ← JWT creation, password hashing
│       │   ├── BillParserService.cs   ← PdfPig PDF parsing logic
│       │   ├── BillService.cs         ← CRUD + cost recalculation
│       │   ├── LineService.cs         ← Line assignment logic
│       │   └── NotificationService.cs ← Twilio WhatsApp / SMS
│       │
│       └── Data/
│           └── AppDbContext.cs        ← EF Core DbContext
│
└── frontend/
    └── phone-bill-app/                ← Next.js 14 App
        ├── package.json
        ├── tailwind.config.ts
        ├── tsconfig.json
        │
        └── src/
            ├── app/                   ← Next.js App Router pages
            │   ├── layout.tsx         ← Root layout
            │   ├── page.tsx           ← Redirect to /login or /dashboard
            │   ├── (auth)/
            │   │   ├── login/
            │   │   │   └── page.tsx   ← Login page
            │   │   └── register/
            │   │       └── page.tsx   ← Register page
            │   └── (dashboard)/
            │       ├── dashboard/
            │       │   └── page.tsx   ← Bills list / home
            │       ├── bills/
            │       │   ├── upload/
            │       │   │   └── page.tsx   ← Upload PDF
            │       │   └── [id]/
            │       │       ├── page.tsx   ← Bill breakdown view
            │       │       └── assign/
            │       │           └── page.tsx ← Assign names to lines
            │       └── notifications/
            │           └── [billId]/
            │               └── page.tsx   ← Send WhatsApp/SMS
            │
            ├── components/
            │   ├── ui/                ← shadcn/ui base components
            │   ├── layout/
            │   │   ├── Navbar.tsx
            │   │   └── Sidebar.tsx
            │   ├── auth/
            │   │   ├── LoginForm.tsx
            │   │   └── RegisterForm.tsx
            │   ├── bills/
            │   │   ├── BillCard.tsx
            │   │   ├── BillUpload.tsx
            │   │   ├── BillBreakdown.tsx
            │   │   ├── PlanChargesTable.tsx
            │   │   ├── EquipmentChargesTable.tsx
            │   │   ├── ServiceChargesTable.tsx
            │   │   └── LineCostSummaryCard.tsx
            │   └── notifications/
            │       └── SendBillForm.tsx
            │
            ├── services/              ← API call wrappers (axios)
            │   ├── api.ts             ← Axios base instance + interceptors
            │   ├── authService.ts
            │   ├── billService.ts
            │   ├── lineService.ts
            │   └── notificationService.ts
            │
            ├── types/                 ← Shared TypeScript interfaces
            │   ├── auth.types.ts
            │   ├── bill.types.ts
            │   ├── line.types.ts
            │   └── notification.types.ts
            │
            ├── context/
            │   └── AuthContext.tsx    ← Global auth state
            │
            └── lib/
                └── utils.ts           ← Helpers (cn, formatCurrency, etc.)
```

---

## 🗄️ Database Schema Summary

> All scripts are in `database/schema.sql` — run this in SSMS first.

| Table                | Purpose                                                      |
|----------------------|--------------------------------------------------------------|
| `AppUsers`           | Registered app users (name, email, mobile, hashed password)  |
| `Bills`              | Uploaded bill metadata + parsed totals                        |
| `AccountLines`       | Each phone line on the bill with computed cost breakdown      |
| `PlanCharges`        | Charges from the PLANS section (shared, split equally)        |
| `EquipmentCharges`   | Per-line handset / device payment charges                     |
| `ServiceCharges`     | Per-line additional service charges                           |
| `Notifications`      | WhatsApp / SMS message log per line                           |

### Key Business Logic (DB level)
- `sp_RecalculateLineCosts` — Stored procedure that recalculates all line totals:
  - `PlanCostShare = TotalPlanAmount / NumberOfLines`
  - `EquipmentCost = SUM of EquipmentCharges for that line`
  - `ServicesCost = SUM of ServiceCharges for that line`
  - `TotalLineCost = PlanCostShare + EquipmentCost + ServicesCost`
- `vw_LineCostSummary` — View for quick full breakdown per line

---

## ⚙️ Setup Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) + SSMS
- [Twilio Account](https://www.twilio.com/) (free trial works)
- Git

---

## 🗃️ Step 1 — Database Setup

1. Open **SSMS**
2. Open `database/schema.sql`
3. Execute the full script
4. Verify the following are created:
   - 7 Tables: `AppUsers`, `Bills`, `AccountLines`, `PlanCharges`, `EquipmentCharges`, `ServiceCharges`, `Notifications`
   - 1 View: `vw_LineCostSummary`
   - 2 Stored Procedures: `sp_RecalculateLineCosts`, `sp_GetBillBreakdown`

---

## 🔧 Step 2 — Backend (.NET 8 API) Setup

### 2.1 Create the project
```bash
# From PhoneBillManager/backend/
dotnet new webapi -n PhoneBillManager.Api --framework net8.0
cd PhoneBillManager.Api
```

### 2.2 Install NuGet packages
```bash
# Entity Framework Core + SQL Server
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Authentication / JWT
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next

# PDF Parsing
dotnet add package PdfPig

# Twilio (WhatsApp + SMS)
dotnet add package Twilio

# Swagger
dotnet add package Swashbuckle.AspNetCore

# File handling
dotnet add package Microsoft.AspNetCore.Http.Features
```

### 2.3 Configure appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PhoneBillManager;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS_LONG",
    "Issuer": "PhoneBillManagerApi",
    "Audience": "PhoneBillManagerApp",
    "ExpiryHours": 24
  },
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
    "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
    "FromPhoneNumber": "+1XXXXXXXXXX",
    "WhatsAppFrom": "whatsapp:+14155238886"
  },
  "FileStorage": {
    "UploadPath": "uploads/bills"
  }
}
```

### 2.4 Run the API
```bash
dotnet run
# API will be available at: https://localhost:7001
# Swagger UI: https://localhost:7001/swagger
```

---

## 💻 Step 3 — Frontend (Next.js 14) Setup

### 3.1 Create the Next.js app
```bash
# From PhoneBillManager/frontend/
npx create-next-app@latest phone-bill-app --typescript --tailwind --eslint --app --src-dir --import-alias "@/*"
cd phone-bill-app
```

### 3.2 Install dependencies
```bash
# HTTP client
npm install axios

# UI components
npx shadcn-ui@latest init
npx shadcn-ui@latest add button card input label table badge dialog toast

# Form handling
npm install react-hook-form @hookform/resolvers zod

# File upload
npm install react-dropzone

# State management
npm install zustand

# JWT decode (client side)
npm install jwt-decode

# Icons
npm install lucide-react
```

### 3.3 Configure environment variables
Create `.env.local` in `frontend/phone-bill-app/`:
```env
NEXT_PUBLIC_API_BASE_URL=https://localhost:7001/api
```

### 3.4 Run the frontend
```bash
npm run dev
# App will be available at: http://localhost:3000
```

---

## 🔄 Step 4 — Application Flow (End to End)

```
User Registers / Logs In
        │
        ▼
JWT Token issued by .NET API
        │
        ▼
User uploads Phone Bill PDF
        │
        ▼
.NET API receives PDF → PdfPig parses content
        │
        ├──► Extracts PLANS section     → saved to PlanCharges table
        ├──► Extracts EQUIPMENT section → saved to EquipmentCharges table
        │    (Handsets sub-section per line)
        └──► Extracts SERVICES section  → saved to ServiceCharges table
                                           (per individual line)
        │
        ▼
sp_RecalculateLineCosts stored procedure runs:
  PlanCostShare   = TotalPlans / NumberOfLines
  EquipmentCost   = Sum of Equipment charges per line
  ServicesCost    = Sum of Service charges per line
  TotalLineCost   = All three combined
        │
        ▼
Frontend displays Bill Breakdown:
  ┌─────────────────────────────────────────────┐
  │  Line: (555) 123-4567  │  Name: John         │
  │  Plan Share:   $30.00  │                     │
  │  Equipment:    $25.00  │                     │
  │  Services:     $15.00  │                     │
  │  TOTAL:        $70.00  │                     │
  └─────────────────────────────────────────────┘
        │
        ▼
User assigns names + WhatsApp/SMS numbers to each line
        │
        ▼
User clicks "Send Bills"
        │
        ▼
.NET API calls Twilio → sends personalised message to each line holder
```

---

## 📡 API Endpoints Reference

### Auth
| Method | Endpoint               | Description          |
|--------|------------------------|----------------------|
| POST   | `/api/auth/register`   | Register new user    |
| POST   | `/api/auth/login`      | Login, returns JWT   |

### Bills
| Method | Endpoint                      | Description                      |
|--------|-------------------------------|----------------------------------|
| GET    | `/api/bills`                  | List all bills for logged-in user|
| POST   | `/api/bills/upload`           | Upload + parse a PDF bill        |
| GET    | `/api/bills/{id}`             | Get full bill breakdown          |
| DELETE | `/api/bills/{id}`             | Delete a bill                    |

### Lines
| Method | Endpoint                          | Description                      |
|--------|-----------------------------------|----------------------------------|
| GET    | `/api/bills/{billId}/lines`       | Get all lines for a bill         |
| PUT    | `/api/lines/{lineId}/assign`      | Assign name + contact to a line  |

### Notifications
| Method | Endpoint                               | Description                    |
|--------|----------------------------------------|--------------------------------|
| POST   | `/api/notifications/send/{billId}`     | Send messages to all lines     |
| POST   | `/api/notifications/send-line/{lineId}`| Send message to one line       |
| GET    | `/api/notifications/{billId}`          | Get notification history       |

---

## 📄 PDF Parsing Strategy (BillParserService.cs)

The parser uses **PdfPig** to extract raw text from the PDF and then uses
section-based parsing logic to identify charges:

### Parsing Sections:
```
1. PLANS
   └── Any line items with a dollar amount → PlanCharges
       (split equally across all lines on the account)

2. EQUIPMENT
   └── Handsets sub-section
       └── Grouped by phone number → EquipmentCharges per LineId

3. SERVICES
   └── Listed per individual phone number → ServiceCharges per LineId
```

### Phone Number Detection:
- Regex: `\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{4}` matches `(555) 123-4567` style numbers
- Each detected number creates an `AccountLine` record

### Section Header Detection:
- Looks for uppercase keywords: `PLANS`, `EQUIPMENT`, `SERVICES`, `HANDSETS`
- Tracks current section as it reads each line of the PDF

---

## 💬 WhatsApp / SMS Message Format

Each recipient receives a personalised message like:

```
Hi John! 👋

Here is your phone bill summary for the period: October 2024

📱 Line: (555) 123-4567

💰 Cost Breakdown:
   Plan Share:     $30.00
   Equipment:      $25.00
   Services:       $15.00
   ─────────────────────
   YOUR TOTAL:     $70.00

Please arrange payment at your earliest convenience.
Thank you!
```

---

## 🔐 Security Notes

- Passwords are hashed using **BCrypt** (never stored in plain text)
- All API routes (except `/auth/login` and `/auth/register`) require a valid **JWT Bearer Token**
- JWT tokens expire after **24 hours** (configurable)
- PDFs are stored server-side with a UUID filename (not the original filename)
- CORS is locked to the frontend origin only

---

## 🚀 Deployment (Optional)

| Component  | Option                                    |
|------------|-------------------------------------------|
| Backend    | Azure App Service / AWS Elastic Beanstalk |
| Frontend   | Vercel (recommended for Next.js)          |
| Database   | Azure SQL / AWS RDS SQL Server            |
| Files      | Azure Blob Storage / AWS S3               |

---

## 🧱 Build Order (Step-by-Step)

Follow this exact order to build and run the app:

```
[1]  database/schema.sql           → Run in SSMS ✅ (already done)
[2]  backend/ — dotnet new webapi  → Scaffold .NET project
[3]  backend/ — NuGet packages     → Install dependencies
[4]  backend/ — Models             → AppUser, Bill, AccountLine, etc.
[5]  backend/ — AppDbContext.cs    → EF Core context
[6]  backend/ — DTOs               → Request/Response shapes
[7]  backend/ — Services           → Auth, Parser, Bill, Line, Notification
[8]  backend/ — Controllers        → Auth, Bills, Lines, Notifications
[9]  backend/ — Program.cs         → Wire everything up
[10] frontend/ — create-next-app   → Scaffold Next.js project
[11] frontend/ — npm installs      → Dependencies
[12] frontend/ — types/            → TypeScript interfaces
[13] frontend/ — services/         → API call wrappers
[14] frontend/ — context/          → AuthContext
[15] frontend/ — components/       → UI components
[16] frontend/ — app/pages         → Route pages
[17] Test end-to-end               → Upload a bill, see breakdown, send SMS
```

---

## 📦 All Files To Be Created

### Backend Files
```
Program.cs
appsettings.json
Data/AppDbContext.cs
Models/AppUser.cs
Models/Bill.cs
Models/AccountLine.cs
Models/PlanCharge.cs
Models/EquipmentCharge.cs
Models/ServiceCharge.cs
Models/Notification.cs
DTOs/Auth/RegisterRequest.cs
DTOs/Auth/LoginRequest.cs
DTOs/Auth/AuthResponse.cs
DTOs/Bills/BillSummaryDto.cs
DTOs/Bills/BillDetailDto.cs
DTOs/Bills/UploadBillResponse.cs
DTOs/Lines/LineDto.cs
DTOs/Lines/AssignLineRequest.cs
DTOs/Notifications/SendNotificationRequest.cs
Services/Interfaces/IAuthService.cs
Services/Interfaces/IBillParserService.cs
Services/Interfaces/IBillService.cs
Services/Interfaces/ILineService.cs
Services/Interfaces/INotificationService.cs
Services/AuthService.cs
Services/BillParserService.cs
Services/BillService.cs
Services/LineService.cs
Services/NotificationService.cs
Controllers/AuthController.cs
Controllers/BillsController.cs
Controllers/LinesController.cs
Controllers/NotificationsController.cs
```

### Frontend Files
```
src/app/layout.tsx
src/app/page.tsx
src/app/(auth)/login/page.tsx
src/app/(auth)/register/page.tsx
src/app/(dashboard)/dashboard/page.tsx
src/app/(dashboard)/bills/upload/page.tsx
src/app/(dashboard)/bills/[id]/page.tsx
src/app/(dashboard)/bills/[id]/assign/page.tsx
src/app/(dashboard)/notifications/[billId]/page.tsx
src/components/layout/Navbar.tsx
src/components/layout/Sidebar.tsx
src/components/auth/LoginForm.tsx
src/components/auth/RegisterForm.tsx
src/components/bills/BillCard.tsx
src/components/bills/BillUpload.tsx
src/components/bills/BillBreakdown.tsx
src/components/bills/PlanChargesTable.tsx
src/components/bills/EquipmentChargesTable.tsx
src/components/bills/ServiceChargesTable.tsx
src/components/bills/LineCostSummaryCard.tsx
src/components/notifications/SendBillForm.tsx
src/services/api.ts
src/services/authService.ts
src/services/billService.ts
src/services/lineService.ts
src/services/notificationService.ts
src/types/auth.types.ts
src/types/bill.types.ts
src/types/line.types.ts
src/types/notification.types.ts
src/context/AuthContext.tsx
src/lib/utils.ts
.env.local
```

---

## ❓ FAQ

**Q: Which PDF formats does the parser support?**
> Any text-based PDF from major US carriers (Verizon, AT&T, T-Mobile). Scanned image PDFs are not supported without adding OCR (e.g. Tesseract).

**Q: Can I customise the message template?**
> Yes — the message template is in `NotificationService.cs` and can be freely edited.

**Q: What if a charge doesn't match a phone number?**
> Unmatched charges are still saved to the DB with `LineId = NULL` and shown as "Unassigned" in the UI, where the user can manually link them.

**Q: Can I use email instead of WhatsApp/SMS?**
> Yes — you can add a `SendGrid` or `SMTP` option in `NotificationService.cs` and add an email field to the `AssignLineRequest`.

---

*README last updated: 2025*
