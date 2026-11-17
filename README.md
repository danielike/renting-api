# RentingApi

A .NET 10 vehicle rental API following Clean Architecture (Hexagonal + Vertical Slices) with CQRS.

## Features

- Vehicle management (create, list)
- Rental management (create, return)
- In-memory persistence (swap for real DB in production)
- API versioning via `Asp.Versioning`
- Scalar API reference UI

## Running with Docker

### Prerequisites

- [Docker](https://www.docker.com/) installed

### Quick Start

1. Copy the environment config (optional — uses defaults if missing):

```bash
cp .env.example .env
```

2. Start the API:

```bash
docker compose up --build -d
```

3. Access the API:
   - API Reference: http://localhost:5000/ (redirects to Scalar UI)
   - API Endpoint: http://localhost:5000/vehicles

### Configuration

The port is configurable via `.env`:

```env
# API port exposed on the host (default: 5000)
API_PORT=5000
```

Change the port:

```bash
API_PORT=3000 docker compose up --build -d
```

### Stop the API

```bash
docker compose down
```

## Running Tests

All tests run inside the Docker build:

```bash
docker compose build
```

Or locally:

```bash
dotnet test
```

## Project Structure

```
src/
  RentingApi.Api/          # Controllers, routes, response/request models
  RentingApi.Application/  # CQRS commands/queries, ports, DTOs
  RentingApi.Domain/       # Entities, domain exceptions
  RentingApi.Infrastructure/ # In-memory repositories, DI setup
tests/
  RentingApi.Api.Tests/          # Functional tests (WebApplicationFactory)
  RentingApi.Application.Tests/  # Unit tests (mocked ports)
  RentingApi.Domain.Tests/       # Domain entity tests
```
