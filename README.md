# RaceDay

## 1. Project Overview

RaceDay is a web-based event management system that I am developing for the South African road running, walking, and cycling community.

The purpose of the system is to provide one platform where event organisers can manage sporting events and participants can register for events and view their results.

The system is designed to reduce the problems caused by paper-based registrations, spreadsheets, and disconnected communication.

## 2. Purpose of the System

The main purpose of RaceDay is to make the management of running, walking, and cycling events more organised and easier to manage.

The system will support:

* User registration and login
* Different user roles
* Event management
* Event categories
* Participant enrolments
* Event routes
* Race results
* RESTful API functionality

## 3. User Roles

### Organiser

The Organiser is responsible for managing events on the RaceDay system.

An Organiser can manage event information, event categories, routes, and other information related to an event.

### Participant

The Participant can view available events, select an event category, enrol in an event, and view their race results.

## 4. Project Structure

The project is organised into the following main folders and files:

```text
RaceDayAPI
│
├── Controllers
├── Models
├── Migrations
│
├── database
│   └── RaceDay.sql
│
├── docs
│   ├── RaceDay-ERD.png
│   └── API-Endpoint-Plan.md
│
├── Program.cs
├── RaceDayAPI.csproj
└── README.md
```

## 5. Documentation

The `docs` folder contains the documentation required for Part 1.

### RaceDay-ERD.png

This is the Entity Relationship Diagram for the RaceDay database. It shows the database entities, attributes, primary keys, foreign keys, and relationships between the tables.

### API-Endpoint-Plan.md

This document contains the REST API endpoint plan for the RaceDay system. It includes the HTTP methods, routes, descriptions, required roles, request bodies, and expected response codes.

## 6. Database

The `database` folder contains the SQL Server database script:

`database/RaceDay.sql`

The SQL script creates the RaceDay database, tables, relationships, constraints, and sample data.

### SQL Server Configuration

For my local development environment, I used:

* Server: `localhost\SQLEXPRESS`
* Authentication: Windows Authentication
* Trust Server Certificate: Enabled

### Running the Database Script

To create the database:

1. Open SQL Server Management Studio (SSMS).
2. Connect to the SQL Server instance.
3. Open the `database/RaceDay.sql` script.
4. Execute the complete script.
5. The script creates the `RaceDayDB` database.
6. Check the tables and sample data in Object Explorer.

## 7. Part 1 Documentation

Part 1 of the RaceDay project includes:

* Database design
* Entity Relationship Diagram
* REST API Endpoint Plan
* SQL Server database script
* Database relationships and constraints
* Sample data
* Repository documentation

## 8. Evidence

The following evidence will be included in the project submission:

* ERD: `docs/RaceDay-ERD.png`
* API Endpoint Plan: `docs/API-Endpoint-Plan.md`
* SQL database execution screenshots
* GitHub repository and commit history
* Video presentation link

## 9. Project Status

This repository contains the Part 1 work for the RaceDay system, including the database design, SQL database script, REST API planning, and supporting documentation.
