# RaceDay RESTful API Endpoint Plan

## Section B – API Endpoint Plan

This document defines the planned RESTful API endpoints for the RaceDay event management system. The endpoint plan is designed before implementation and covers authentication, user profiles, events, categories, event enrolments, and participant results.

The API will support two system roles:

- **Organiser** – creates and manages events and categories, views enrolments, and records and manages participant results.
- **Participant** – creates an account, browses events, enrols in events, views personal enrolments, and tracks personal results.
- **Public** – unauthenticated users can register, log in, and browse available events and event categories.

## Authentication

| HTTP MethodRouteDescriptionRole RequiredRequest BodyExpected Response |                      |                                                              |        |                                                           |                                                                                                                           |
| --------------------------------------------------------------------- | -------------------- | ------------------------------------------------------------ | ------ | --------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| POST                                                                  | `/api/auth/register` | Creates a new Participant or Organiser account.              | Public | `firstName, lastName, email, password, phoneNumber, role` | **201 Created** – User created successfully. **400 Bad Request** – Invalid data. **409 Conflict** – Email already exists. |
| POST                                                                  | `/api/auth/login`    | Authenticates a registered user and returns an access token. | Public | `email, password`                                         | **200 OK** – Authentication successful with token and user information. **401 Unauthorized** – Invalid credentials.       |

## User Profile

| HTTP MethodRouteDescriptionRole RequiredRequest BodyExpected Response |                      |                                                                        |                        |                                    |                                                                                                                                |
| --------------------------------------------------------------------- | -------------------- | ---------------------------------------------------------------------- | ---------------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| GET                                                                   | `/api/users/profile` | Retrieves the profile information of the currently authenticated user. | Any authenticated user | None                               | **200 OK** – User profile returned. **401 Unauthorized** – Authentication required.                                            |
| PUT                                                                   | `/api/users/profile` | Updates the profile information of the currently authenticated user.   | Any authenticated user | `firstName, lastName, phoneNumber` | **200 OK** – Profile updated successfully. **400 Bad Request** – Invalid data. **401 Unauthorized** – Authentication required. |

## Events

| HTTP MethodRouteDescriptionRole RequiredRequest BodyExpected Response |                    |                                                                                   |           |                                                               |                                                                                                                                                  |
| --------------------------------------------------------------------- | ------------------ | --------------------------------------------------------------------------------- | --------- | ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| GET                                                                   | `/api/events`      | Retrieves a list of upcoming RaceDay events available for participants to browse. | Public    | None                                                          | **200 OK** – List of upcoming events returned.                                                                                                   |
| GET                                                                   | `/api/events/{id}` | Retrieves the details of a specific event.                                        | Public    | None                                                          | **200 OK** – Event details returned. **404 Not Found** – Event does not exist.                                                                   |
| POST                                                                  | `/api/events`      | Creates a new RaceDay event.                                                      | Organiser | `name, description, eventDate, location, distance, eventType` | **201 Created** – Event created successfully. **400 Bad Request** – Invalid data. **401 Unauthorized / 403 Forbidden** – User is not authorised. |
| PUT                                                                   | `/api/events/{id}` | Updates an existing event managed by the organiser.                               | Organiser | `name, description, eventDate, location, distance, eventType` | **200 OK** – Event updated successfully. **404 Not Found** – Event does not exist. **403 Forbidden** – User does not have permission.            |
| DELETE                                                                | `/api/events/{id}` | Deletes an existing event.                                                        | Organiser | None                                                          | **204 No Content** – Event deleted successfully. **404 Not Found** – Event does not exist. **403 Forbidden** – User does not have permission.    |

## Categories

| HTTP MethodRouteDescriptionRole RequiredRequest BodyExpected Response |                                    |                                                         |           |                                                    |                                                                                                                                                     |
| --------------------------------------------------------------------- | ---------------------------------- | ------------------------------------------------------- | --------- | -------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| GET                                                                   | `/api/events/{eventId}/categories` | Retrieves all categories belonging to a specific event. | Public    | None                                               | **200 OK** – List of event categories returned. **404 Not Found** – Event does not exist.                                                           |
| POST                                                                  | `/api/events/{eventId}/categories` | Creates a new category for a specific event.            | Organiser | `name, description, entryFee, maximumParticipants` | **201 Created** – Category created successfully. **400 Bad Request** – Invalid data. **404 Not Found** – Event does not exist.                      |
| PUT                                                                   | `/api/categories/{id}`             | Updates an existing event category.                     | Organiser | `name, description, entryFee, maximumParticipants` | **200 OK** – Category updated successfully. **404 Not Found** – Category does not exist. **403 Forbidden** – User does not have permission.         |
| DELETE                                                                | `/api/categories/{id}`             | Deletes an existing event category.                     | Organiser | None                                               | **204 No Content** – Category deleted successfully. **404 Not Found** – Category does not exist. **403 Forbidden** – User does not have permission. |

## Event Enrolments

| HTTP MethodRouteDescriptionRole RequiredRequest BodyExpected Response |                                    |                                                                                      |                         |              |                                                                                                                                                                                                          |
| --------------------------------------------------------------------- | ---------------------------------- | ------------------------------------------------------------------------------------ | ----------------------- | ------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| POST                                                                  | `/api/events/{eventId}/enrolments` | Enrols the currently authenticated participant in a selected event category.         | Participant             | `categoryId` | **201 Created** – Enrolment created successfully. **400 Bad Request** – Invalid category or data. **404 Not Found** – Event/category does not exist. **409 Conflict** – Participant is already enrolled. |
| GET                                                                   | `/api/users/enrolments`            | Retrieves all event enrolments belonging to the currently authenticated participant. | Participant             | None         | **200 OK** – Participant's enrolments returned. **401 Unauthorized** – Authentication required.                                                                                                          |
| GET                                                                   | `/api/events/{eventId}/enrolments` | Retrieves all participant enrolments for a specific event.                           | Organiser               | None         | **200 OK** – Event enrolments returned. **404 Not Found** – Event does not exist. **403 Forbidden** – User does not have permission.                                                                     |
| GET                                                                   | `/api/enrolments/{id}`             | Retrieves the details of a specific event enrolment.                                 | Participant / Organiser | None         | **200 OK** – Enrolment details returned. **404 Not Found** – Enrolment does not exist. **403 Forbidden** – User does not have permission.                                                                |

## Results

| HTTP MethodRouteDescriptionRole RequiredRequest BodyExpected Response |                                        |                                                                                    |             |                                |                                                                                                                                                                             |
| --------------------------------------------------------------------- | -------------------------------------- | ---------------------------------------------------------------------------------- | ----------- | ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| POST                                                                  | `/api/enrolments/{enrolmentId}/result` | Records the result of a participant who completed an event.                        | Organiser   | `finishTime, position, status` | **201 Created** – Result recorded successfully. **400 Bad Request** – Invalid result data. **404 Not Found** – Enrolment does not exist.                                    |
| PUT                                                                   | `/api/results/{id}`                    | Updates an existing participant result.                                            | Organiser   | `finishTime, position, status` | **200 OK** – Result updated successfully. **400 Bad Request** – Invalid data. **404 Not Found** – Result does not exist. **403 Forbidden** – User does not have permission. |
| GET                                                                   | `/api/users/results`                   | Retrieves the personal results history of the currently authenticated participant. | Participant | None                           | **200 OK** – Participant's results returned. **401 Unauthorized** – Authentication required.                                                                                |
| GET                                                                   | `/api/events/{eventId}/results`        | Retrieves the results recorded for a specific event.                               | Organiser   | None                           | **200 OK** – Event results returned. **404 Not Found** – Event does not exist. **403 Forbidden** – User does not have permission.                                           |

## Summary

The planned RaceDay API contains **21 endpoints** covering the core functionality required for the system.

| Functional AreaNumber of Endpoints |        |
| ---------------------------------- | ------ |
| Authentication                     | 2      |
| User Profile                       | 2      |
| Events                             | 5      |
| Categories                         | 4      |
| Event Enrolments                   | 4      |
| Results                            | 4      |
| **Total**                          | **21** |

This endpoint plan will be used as the reference for the RESTful API implementation in Part 2. Any changes made during implementation should be documented and justified so that the implemented API remains consistent with the approved system design. 