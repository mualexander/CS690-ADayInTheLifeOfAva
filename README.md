# CS690-ADayInTheLifeOfAva Travel Planning System

## Overview
This project implements a travel planning application that helps users organize destinations, research materials, travel options, and manage trip budgets.

The system is based on the “A Day in the Life of Ava” planning scenario and is designed using structured functional and nonfunctional requirements.

---

[Project Wiki](https://github.com/mualexander/CS690-ADayInTheLifeOfAva/wiki)
* [User Document](https://github.com/mualexander/CS690-ADayInTheLifeOfAva/wiki/User-document)

---

## TravelPlanner CLI

TravelPlanner CLI is a console-based application for planning trips, tracking expenses, and evaluating travel options including:
* Trips and stays
* Expenses
* Bookmarks (things to do)
* Flight options
* Lodging options

It supports budgeting by combining:
* actual expenses
* selected travel options (flights + lodging)

## Getting Started
### Prerequisites
* .NET 10 SDK
* Windows / macOS / Linux

### Check install:
```
dotnet --version
```
### Build the project
From the solution root:
```
dotnet build
```

### Run the application
```
dotnet run --project TravelPlanner.Cli
```

### Data storage

The app stores data in:
```
data/trips.json
```

Created automatically on first run. It is safe to delete if you want a fresh start

### How to Use

The app is menu-driven.

Main Menu
→ Select or create a Trip
→ Manage Stays
→ Within a Stay:
   - Expenses
   - Bookmarks
   - Flight Options
   - Lodging Options

### Flight Options

Add flight options with:

* route
* times
* optional price
* URL

Mark a flight as selected to include it in the plan

Update price → automatically updates "last checked"

Note that the intent was to have flights associated with the stay that is the flights destination. So flights from SFO to Narita would be kept under a stay for Tokyo.

### Lodging Options

Add lodging options with:
* property name
* dates
* optional price
* URL

Mark selected lodging to include in budget

### Budgeting

The app calculates:
* Expenses → actual spend
* Selected travel options → planned cost
* Total planned cost
* Remaining budget

Displayed at:
* Stay level
* Trip level

### Notes on Selection

“Select” in menus = navigate to details

“Mark selected” = include in trip plan and budget

Multiple options can be selected:
* multiple flights (multi-leg trips)
* multiple lodgings (split stays)

### Running Tests
```
dotnet test
```

### Future Enhancements
* Airline / alliance tracking
* Advanced routing
* Reporting / export
* UI improvements
