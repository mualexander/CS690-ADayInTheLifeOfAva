# CS690-ADayInTheLifeOfAva Travel Planning System

## Overview
This project implements a travel planning application that helps users organize destinations, research materials, travel options, and manage trip budgets.

The system is based on the “A Day in the Life of Ava” planning scenario and is designed using structured functional and nonfunctional requirements.

---

## System Scope

The application supports:

- Managing trips and locations
- Organizing research links by location
- Tracking flight and lodging options
- Computing budget summaries
- Enforcing overall budget constraints

Out of scope (v1):
- Direct booking integrations
- Real-time airline/hotel APIs
- Multi-user collaboration

---

# Use Cases and Associated Issues

## UC1 – Manage Trips

Users can create and manage trips as containers for locations and travel options.

### Functional Requirements
- FR0 – Create and manage trips (Trip CRUD)

### Related GitHub Issues
- #1 FR0 – Create and manage trips
- #9 INF0 – Establish domain/service architecture

---

## UC2 – Manage Locations

Users can create, edit, reorder, and remove locations within a trip.

### Functional Requirements
- FR1 – Create and manage locations (Location CRUD)

### Related GitHub Issues
- #2 FR1 – Location CRUD

---

## UC3 – Save Research Links by Location

Users can save travel guides, blogs, and other URLs and associate them with a specific location.

### Functional Requirements
- FR2 – Bookmark CRUD by location

### Related GitHub Issues
- #3 FR2 – Bookmark CRUD

---

## UC4 – Track Flight Options

Users can store and compare flight options for each location.

### Functional Requirements
- FR3 – FlightOption CRUD + selection

### Related GitHub Issues
- #4 FR3 – FlightOption CRUD

---

## UC5 – Track Lodging Options

Users can store and compare lodging options for each location.

### Functional Requirements
- FR4 – LodgingOption CRUD + selection

### Related GitHub Issues
- #5 FR4 – LodgingOption CRUD

---

## UC6 – Budget Reporting

Users can view total projected trip cost and per-location breakdown.

### Functional Requirements
- FR5a – Cost assumptions per location
- FR5 – Budget summary calculation

### Related GitHub Issues
- #7 FR5a – Cost assumptions
- #6 FR5 – Budget summary

---

## UC7 – Budget Constraints

Users can define a total trip budget and receive warnings when projected costs exceed it.

### Functional Requirements
- FR6 – Budget constraint enforcement

### Related GitHub Issues
- #8 FR6 – Budget constraints

---

# Nonfunctional Requirements

## Reliability
- NFR1 – Persistence reliability
- NFR5 – Data integrity

## Performance
- NFR2 – Budget calculation performance

## Maintainability
- NFR3 – Layered architecture enforcement
- NFR7 – Extensibility of cost categories

## Testability
- NFR4 – Unit-testable services

See corresponding GitHub issues for detailed acceptance criteria.

---

# Scenario

## A Day in the Life of Ava 

### Morning

Ava sits at her kitchen table with her laptop open to a travel website. She wants to plan a trip to Italy but feels overwhelmed by the number of cities she wants to visit. “How do I decide where to go first?” she wonders, scribbling a rough list of locations.


### Late Morning

Ava’s best friend calls and suggests meeting to discuss the trip. “Let’s decide on activities for each city,” they say. Ava agrees but realizes shehasn’torganized her ideas. “I need a better way to map this out,” she mutters.


### Lunch

Over lunch, Ava browses through her bookmarked travel guides and blogs.She’sexcited about trying local foods and visiting historical sites but feels scattered. “I should organize theseideasso I don’t miss anything,” she thinks.


### Afternoon

Later in the day, Ava looks up flight options and hotel deals. She quickly loses track of the prices and availabilityshe’schecked. “If I don’t save this information somewhere, I’ll have to look it all up again,” she sighs.


### Evening

At the end of the day, Ava reflects on how much she loves planning trips but feels frustrated by the lack of a clear system. “If I had a tool to organize my itinerary, track bookings, and manage my budget, this trip would be so much easier to plan,” she resolves.


