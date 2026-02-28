# ST Funds API

## Overview

ST Funds API is a .NET Web API application that ingests fund data from external sources and exposes REST endpoints to retrieve and filter fund information.

The system:

- Fetches fund data from configured external URLs
- Stores the data in a In memory database
- Exposes endpoints to retrieve funds with filtering support
- Supports cancellation and proper error handling
- Includes unit tests using NUnit and Moq

---

## Architecture

The project follows a clean layered architecture:

ST.Funds.Api  
→ Controllers  
→ Application (Services, Business Logic & DTO)  
→ Data (Entities + DbContext)  
→ Middleware (Logging)

### Key Layers

- Controller Layer
  - Thin controllers
  - Handles HTTP concerns
  - Proper status codes and exception handling

- Application Layer
  - Business logic
  - Data ingestion
  - Filtering

- Data Layer
  - Entity Framework Core
  - InMemory database (for testing)
  - Proper entity relationships

---

## Technologies Used

- .NET 10.0 Web API
- Entity Framework Core
- InMemory Database (for testing)
- NUnit
- Moq
- Microsoft.Extensions.Logging
- HttpClient (for external API ingestion)

---

## Features

### 1. Ingest External Fund Data

POST `/api/funds/refresh`

- Fetches data from configured sources
- Adds new funds
- Updates existing funds
- Handles invalid responses safely
- Supports cancellation

---

### 2. Retrieve All Funds

GET `/api/funds`

Supports filtering via query parameters:

- `MaxOngoingCharge`
- `AnalystRating`
- `SectorName`

Example:  GET /api/funds?MaxOngoingCharge=0.3&AnalystRating=4

---

### 3. Retrieve Fund by Market Code

GET `/api/funds/{marketCode}`

Returns:

- 200 OK if found
- 404 Not Found if missing
- 499 if request cancelled
- 500 if unexpected error

---

## Filtering Design

Filtering is centralized in:
- BuildFundQuery(FundQueryParameters query)	


This design allows easy extension without modifying other layers.

---

## Error Handling

Controller handles:

- OperationCanceledException → 499
- Unexpected exceptions → 500

Service layer logs and rethrows exceptions.

---

## Unit Testing

Unit tests cover:

### Service Tests
- Filtering logic
- Get by market code
- Refresh ingestion
- Invalid external response handling

### Controller Tests
- Success responses
- Not found scenarios
- Cancellation scenarios
- Exception scenarios
- Correct status codes

Testing tools used:

- NUnit
- Moq
- EF Core InMemory
- Fake HttpMessageHandler for HttpClient mocking

---

## Running the Project

1. Clone repository
2. Restore packages


