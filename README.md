# World Cup Predictor API

World Cup Predictor API is the backend platform that powers the World Cup Predictor ecosystem, including authentication, prediction management, tournament simulations, scoring systems, standings calculations, notifications, cloud integrations, and data persistence.

Frontend Application:

https://goal2026.net/login

---

## Overview

This API provides all business logic and backend services required to run a World Cup prediction competition platform.

Users can create prediction pools, invite friends and family, generate predictions, simulate tournaments, compete in private groups, receive notifications, and follow rankings throughout the competition.

---

## Core Features

### Authentication

* Firebase Authentication integration
* Passwordless Magic Link sign-in
* Email-based authentication flow
* User account management

### Prediction Pools

* Create public or private prediction groups
* Group membership management
* Invitations via email
* User permissions and roles
* Administrator controls

### Tournament Simulations

* Simulate group stage results
* Generate qualification scenarios
* Predict standings before tournament kickoff
* Analyze alternative outcomes

### Scoring Engine

* Configurable scoring systems
* Multiple competition rule sets
* Automatic score calculation
* Ranking generation

### Standings Calculation

* FIFA group stage ranking rules
* Tie-breaker calculations
* Qualification determination
* Automatic leaderboard updates

### Snapshot Engine

To improve performance, the API generates and stores standings snapshots, avoiding expensive recalculations across large prediction groups.

Features include:

* Cached standings
* Historical rankings
* Fast leaderboard rendering
* Performance optimization

### Notifications

#### Email Notifications

Powered by SendGrid.

Supported events include:

* Magic Link login
* Group invitations
* Competition updates
* Ranking changes
* System notifications

#### Push Notifications

Powered by Firebase Cloud Messaging (FCM).

Supported events include:

* Match reminders
* Group activity
* Ranking updates
* Competition events

---

## Cloud Integrations

### AWS S3

Secure image upload workflow.

Features:

* Signed upload URLs
* Time-limited upload permissions
* File size validation
* Content type validation
* Secure file access

### Firebase

* Authentication
* Magic Link Sign-In
* Push Notifications
* User Management

### SendGrid

* Transactional emails
* Authentication emails
* Group invitations
* Multilingual templates

---

## Data Storage

### SQL Server

Primary persistence layer for:

* Users
* Groups
* Predictions
* Matches
* Standings
* Rankings
* Notifications
* Snapshots

---

## Security

* reCAPTCHA protection
* Firebase Authentication
* Role-based authorization
* Signed AWS upload URLs
* Input validation
* Permission enforcement

---

## Technology Stack

### Backend

* ASP.NET Core
* C#
* Entity Framework Core
* SQL Server

### Cloud Services

* AWS S3
* Firebase Authentication
* Firebase Cloud Messaging
* SendGrid

### Security

* Google reCAPTCHA
* Role-based authorization

---

## Related Project

Frontend repository:

https://github.com/juliobriceno/world-cup-predictor-web

---

## License

MIT License
