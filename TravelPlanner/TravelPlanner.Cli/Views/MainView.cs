using Spectre.Console;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class MainView
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;

    public MainView(TripService svc, InMemoryTripContext ctx)
    {
        _svc = svc;
        _ctx = ctx;
    }

    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Render();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Action:[/]")
                    .AddChoices("New Trip", "Open Trip", "Delete Trip", "Find by Tag", "Seed Demo", "Quit"));

            try
            {
                switch (choice)
                {
                    case "New Trip":    OnNew();       break;
                    case "Open Trip":   OnOpen();      break;
                    case "Delete Trip": OnDelete();    break;
                    case "Find by Tag": OnFindByTag(); break;
                    case "Seed Demo":   OnSeed();      break;
                    case "Quit":        return;
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    private void Render()
    {
        AnsiConsole.Write(new Rule("[bold deepskyblue1]✈  TravelPlanner[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var trips = _svc.GetTrips();
        if (trips.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No trips yet. Select \"New Trip\" to create one.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Name[/]")
            .AddColumn(new TableColumn("[bold]Budget[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Cost[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Remaining[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Stays[/]").RightAligned());

        foreach (var t in trips)
        {
            var remainColor = t.RemainingBudget >= 0 ? "green" : "red";
            table.AddRow(
                Markup.Escape(t.Name),
                $"[yellow]${t.TotalBudget:0.00}[/]",
                $"${t.TotalPlannedCost:0.00}",
                $"[{remainColor}]${t.RemainingBudget:0.00}[/]",
                t.StayCount.ToString()
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private void OnNew()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]New Trip[/] [grey](Esc to cancel)[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var name = ConsoleInput.AskOrEscape("Trip [bold]name[/]:");
        if (string.IsNullOrWhiteSpace(name)) return;

        decimal budget;
        while (true)
        {
            var budgetStr = ConsoleInput.AskOrEscape("Budget [grey](e.g. 5000)[/]:");
            if (string.IsNullOrWhiteSpace(budgetStr)) return;
            if (decimal.TryParse(budgetStr, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out budget) && budget > 0) break;
            AnsiConsole.MarkupLine("[red]Invalid budget.[/]");
        }

        var homeAirport  = ConsoleInput.AskOrEscape("Home airport code [grey](e.g. SFO, blank to skip)[/]:");
        if (homeAirport == null) return;

        var currencyInput = ConsoleInput.AskOrEscape("Default currency [grey](blank = USD)[/]:");
        if (currencyInput == null) return;
        var currency = string.IsNullOrWhiteSpace(currencyInput) ? "USD" : currencyInput;

        int? travelerCount = null;
        var travelerStr = ConsoleInput.AskOrEscape("Traveler count [grey](blank to skip)[/]:");
        if (travelerStr == null) return;
        if (!string.IsNullOrWhiteSpace(travelerStr))
        {
            if (!int.TryParse(travelerStr, out var tc) || tc < 1)
            { AnsiConsole.MarkupLine("[red]Traveler count must be a positive whole number.[/]"); Pause(); return; }
            travelerCount = tc;
        }

        try
        {
            var trip = _svc.CreateTrip(name, budget,
                string.IsNullOrWhiteSpace(homeAirport) ? null : homeAirport,
                currency,
                travelerCount);
            _svc.SelectTrip(trip.Id);
            new TripView(_svc, _ctx).Run();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
            Pause();
        }
    }

    private void OnOpen()
    {
        var trips = _svc.GetTrips();
        if (trips.Count == 0) { AnsiConsole.MarkupLine("[yellow]No trips to open.[/]"); Pause(); return; }

        var trip = AnsiConsole.Prompt(
            new SelectionPrompt<TripSummary>()
                .Title("Select trip to [bold]open[/]:")
                .UseConverter(t => $"{Markup.Escape(t.Name)}  [grey][[{t.StayCount} stays · ${t.TotalBudget:0}]][/]")
                .AddChoices(trips));

        _svc.SelectTrip(trip.Id);
        new TripView(_svc, _ctx).Run();
    }

    private void OnDelete()
    {
        var trips = _svc.GetTrips();
        if (trips.Count == 0) { AnsiConsole.MarkupLine("[yellow]No trips to delete.[/]"); Pause(); return; }

        var trip = AnsiConsole.Prompt(
            new SelectionPrompt<TripSummary>()
                .Title("Select trip to [bold red]delete[/]:")
                .UseConverter(t => Markup.Escape(t.Name))
                .AddChoices(trips));

        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(trip.Name)}[/]? This cannot be undone."))
            return;

        try { _svc.DeleteTrip(trip.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnFindByTag()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Find by Tag[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var allTags = _svc.GetAllTagsAcrossAllTrips();
        if (allTags.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No tags found across any trips.[/]");
            Pause();
            return;
        }

        var tag = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a tag:")
                .AddChoices(allTags));

        var results = _svc.FindBookmarksByTag(tag);
        AnsiConsole.WriteLine();

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No bookmarks found with tag [bold]#{Markup.Escape(tag)}[/].[/]");
            Pause();
            return;
        }

        AnsiConsole.MarkupLine($"  Bookmarks tagged [deepskyblue1]#{Markup.Escape(tag)}[/]:");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Trip[/]")
            .AddColumn("[bold]Stay[/]")
            .AddColumn("[bold]Title[/]")
            .AddColumn("[bold]URL[/]")
            .AddColumn("[bold]Tags[/]");

        foreach (var r in results)
        {
            var tagsDisplay = string.Join(" ", r.Tags.Select(t => $"[deepskyblue1]#{Markup.Escape(t)}[/]"));
            table.AddRow(
                Markup.Escape(r.TripName),
                Markup.Escape(r.StayDisplayKey),
                Markup.Escape(r.Title),
                $"[link={r.Url}]{Markup.Escape(r.Url)}[/]",
                tagsDisplay);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        Pause();
    }

    private void OnSeed()
    {
        try
        {
            SeedNewZealand2026();
            SeedJapan2027();
            AnsiConsole.MarkupLine("[green]Demo data seeded.[/]");
            Pause();
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void SeedNewZealand2026()
    {
        var trip = _svc.CreateTrip("New Zealand 2026", 6000m);
        _svc.SelectTrip(trip.Id);

        // ── Auckland ─────────────────────────────────────────────────────────
        _svc.AddStay("Auckland", "New Zealand",
            new DateTime(2026, 10, 30), new DateTime(2026, 11, 3), StayStatus.Locked);
        var stays = _svc.GetStays();
        var aklId = stays.Single(s => s.City == "Auckland" && s.StartDate == new DateTime(2026, 10, 30)).Id;

        _svc.AddExpenseToStay(aklId, "Food",               500m,  ExpenseCategory.Food,       "About a hundred a day");
        _svc.AddExpenseToStay(aklId, "Halloween",          100m,  ExpenseCategory.Activities, "Spookers and Cassette Nine");
        _svc.AddExpenseToStay(aklId, "Auckland Marathon Day", 50m, ExpenseCategory.Activities, "Treats and stuff after the marathon");
        _svc.AddExpenseToStay(aklId, "Scuba Diving",       300m,  ExpenseCategory.Activities, "Dive! Tutukaka trip to Poor Knights Islands");

        _svc.AddBookmarkToStay(aklId, "Ding Dong Lounge",  "https://www.dingdongloungenz.com/",       "Late night rock/metal bar that will be done up for Halloween", new[] { "cocktails", "nightlife" });
        _svc.AddBookmarkToStay(aklId, "Sky Tower",         "https://skycityauckland.co.nz/sky-tower/", "Tallest structure in the Southern Hemisphere",                 new[] { "landmarks" });
        _svc.AddBookmarkToStay(aklId, "Auckland Marathon", "https://aucklandmarathon.co.nz/",          "Registration complete!",                                       new[] { "exercise" });

        _svc.AddFlightOptionToStay(aklId, "https://www.google.com/travel/flights/s/wEpGCGqtDgS18NT87",
            "SFO", "AKL", new DateTime(2026, 10, 28, 23, 5, 0), new DateTime(2026, 10, 30, 13, 35, 0), 344m);
        var sfoFlight = _svc.GetFlightOptionsForStay(aklId).Single(f => f.FromAirportCode == "SFO");
        _svc.SelectFlightOption(aklId, sfoFlight.Id);

        _svc.AddFlightOptionToStay(aklId, "https://www.google.com/travel/flights/s/wEpGCGqtDgS18NT87",
            "OAK", "AKL", new DateTime(2026, 10, 28, 12, 5, 0), new DateTime(2026, 10, 30, 13, 35, 0), 542m);

        _svc.AddLodgingOptionToStay(aklId, "https://www.marriott.com/en-us/hotels/akljw-jw-marriott-auckland/overview/",
            "JW Marriott Auckland", new DateTime(2026, 10, 30), new DateTime(2026, 11, 3), 1032.84m);
        var jwMarriott = _svc.GetLodgingOptionsForStay(aklId).Single(l => l.PropertyName == "JW Marriott Auckland");
        _svc.SelectLodgingOption(aklId, jwMarriott.Id);

        // ── Matamata ─────────────────────────────────────────────────────────
        _svc.AddStay("Matamata", "New Zealand",
            new DateTime(2026, 11, 3), new DateTime(2026, 11, 4), StayStatus.Locked);
        var mataId = _svc.GetStays().Single(s => s.City == "Matamata").Id;

        _svc.AddExpenseToStay(mataId, "Hobbiton Evening Banquet", 139.20m, ExpenseCategory.Activities, "Can't believe we got this reservation!");

        _svc.AddBookmarkToStay(mataId, "Wai-O-Tapu Thermal Wonderland", "https://www.waiotapu.co.nz/plan-your-visit/", "Arrive by 9:45am for Lady Knox geyser", new[] { "relaxation" });
        _svc.AddBookmarkToStay(mataId, "Hobbiton",                      "https://www.hobbitontours.com/",              "Lord of the Rings!!!",                    new[] { "landmarks" });

        _svc.AddLodgingOptionToStay(mataId, "https://www.booking.com/hotel/nz/broadway-motel-matamata.html",
            "Broadway Motel & Miro Court Villas", new DateTime(2026, 11, 3), new DateTime(2026, 11, 4), 157m);
        var broadway = _svc.GetLodgingOptionsForStay(mataId).Single();
        _svc.SelectLodgingOption(mataId, broadway.Id);

        // ── Tongariro ─────────────────────────────────────────────────────────
        _svc.AddStay("Tongariro", "New Zealand",
            new DateTime(2026, 11, 4), new DateTime(2026, 11, 5), StayStatus.Shortlist);
        var tonId = _svc.GetStays().Single(s => s.City == "Tongariro").Id;

        _svc.AddExpenseToStay(tonId, "The Hell's Gate Experience", 115m, ExpenseCategory.Activities, "Geothermal walk, mud bath, and pool");

        _svc.AddBookmarkToStay(tonId, "Hell's Gate",          "https://www.hellsgate.co.nz/",      "Thermal mud baths",       new[] { "exercise", "relaxation" });
        _svc.AddBookmarkToStay(tonId, "Tongariro National Park", "https://www.nationalpark.co.nz/", "Hiking and sightseeing",  new[] { "exercise", "landmarks" });

        _svc.AddLodgingOptionToStay(tonId, "https://www.booking.com/hotel/nz/mountain-heights-lodge.html",
            "Mountain Heights Lodge", new DateTime(2026, 11, 4), new DateTime(2026, 11, 5), 155m);
        _svc.SelectLodgingOption(tonId, _svc.GetLodgingOptionsForStay(tonId).Single().Id);

        // ── Wellington ────────────────────────────────────────────────────────
        _svc.AddStay("Wellington", "New Zealand",
            new DateTime(2026, 11, 5), new DateTime(2026, 11, 6), StayStatus.Locked);
        var wlgId = _svc.GetStays().Single(s => s.City == "Wellington").Id;

        _svc.AddExpenseToStay(wlgId, "Zealandia", 31m,  ExpenseCategory.Activities, "Animal sanctuary");
        _svc.AddExpenseToStay(wlgId, "Drinks",    100m, ExpenseCategory.Activities, "Dee's Place and Club K");
        _svc.AddExpenseToStay(wlgId, "Dinner",    100m, ExpenseCategory.Food,       null);

        _svc.AddBookmarkToStay(wlgId, "Zealandia",              "https://www.visitzealandia.com/",                                     "Huge animal sanctuary",                          new[] { "animals" });
        _svc.AddBookmarkToStay(wlgId, "Te Papa National Museum", "https://www.tepapa.govt.nz/",                                        "One of TripAdvisor's top NZ tourist attractions", new[] { "landmarks", "museum" });
        _svc.AddBookmarkToStay(wlgId, "Club K",                  "https://www.instagram.com/explore/locations/11502808/club-k/",       "Dancing and karaoke",                            new[] { "cocktails", "nightlife" });
        _svc.AddBookmarkToStay(wlgId, "S&M's Cocktail & Lounge Bar", "https://www.scottyandmals.co.nz/",                               "Dancing and cocktails",                          new[] { "cocktails", "nightlife" });

        _svc.AddLodgingOptionToStay(wlgId, "https://www.booking.com/hotel/nz/the-cobbler.html",
            "The Cobbler Hotel", new DateTime(2026, 11, 5), new DateTime(2026, 11, 6), 244m);
        _svc.SelectLodgingOption(wlgId, _svc.GetLodgingOptionsForStay(wlgId).Single().Id);

        // ── Punakaiki ─────────────────────────────────────────────────────────
        _svc.AddStay("Punakaiki", "New Zealand",
            new DateTime(2026, 11, 6), new DateTime(2026, 11, 7), StayStatus.Shortlist);
        var pknId = _svc.GetStays().Single(s => s.City == "Punakaiki").Id;

        _svc.AddExpenseToStay(pknId, "Cook Strait Ferry", 382m, ExpenseCategory.Transportation, "Four people with food and drinks included");

        _svc.AddBookmarkToStay(pknId, "Interislander Ferry",        "https://www.interislander.co.nz",              "Primary ferry to cross the Cook Straight");
        _svc.AddBookmarkToStay(pknId, "Bluebridge Ferry",           "https://www.bluebridge.co.nz/",                "Alternative ferry across the Cook Straight");
        _svc.AddBookmarkToStay(pknId, "Pancake rocks and blowholes", "https://maps.app.goo.gl/beoRVbRS51DhwVjy8",   "Also near the Punakaiki cavern");

        _svc.AddLodgingOptionToStay(pknId, "https://www.booking.com/hotel/nz/punakaiki-beach-camp.html",
            "Punakaiki Beach Camp", new DateTime(2026, 11, 6), new DateTime(2026, 11, 7), 70m);
        _svc.SelectLodgingOption(pknId, _svc.GetLodgingOptionsForStay(pknId).Single().Id);

        // ── Franz Josef ───────────────────────────────────────────────────────
        _svc.AddStay("Franz Josef", "New Zealand",
            new DateTime(2026, 11, 7), new DateTime(2026, 11, 8), StayStatus.Locked);
        var fjId = _svc.GetStays().Single(s => s.City == "Franz Josef").Id;

        _svc.AddExpenseToStay(fjId, "Helicopter and glacier hike", 520m, ExpenseCategory.Activities, "Hope weather complies");
        _svc.AddExpenseToStay(fjId, "Food",                         75m, ExpenseCategory.Food,       null);

        _svc.AddBookmarkToStay(fjId, "Snakebite Brewery", "https://www.snakebite.co.nz/", "Should have vegetarian options");

        _svc.AddLodgingOptionToStay(fjId, "https://www.booking.com/hotel/nz/rainforest-retreat.html",
            "Rainforest Retreat", new DateTime(2026, 11, 7), new DateTime(2026, 11, 8), 322.90m);
        _svc.SelectLodgingOption(fjId, _svc.GetLodgingOptionsForStay(fjId).Single(l => l.PropertyName == "Rainforest Retreat").Id);

        _svc.AddLodgingOptionToStay(fjId, "https://www.scenichotelgroup.co.nz/franz-josef/legacy-te-waonui-hotel/",
            "Legacy Te Waonui Hotel", new DateTime(2026, 11, 7), new DateTime(2026, 11, 8), 890m);

        // ── Queenstown ────────────────────────────────────────────────────────
        _svc.AddStay("Queenstown", "New Zealand",
            new DateTime(2026, 11, 8), new DateTime(2026, 11, 10), StayStatus.Locked);
        var qtnId = _svc.GetStays().Single(s => s.City == "Queenstown").Id;

        _svc.AddExpenseToStay(qtnId, "Canyon Explorers",   145m, ExpenseCategory.Activities, "Half day excursion");
        _svc.AddExpenseToStay(qtnId, "Drinks and karaoke", 100m, ExpenseCategory.Activities, "Birdy and Little Blackwood");
        _svc.AddExpenseToStay(qtnId, "Food",               100m, ExpenseCategory.Food,       null);

        _svc.AddBookmarkToStay(qtnId, "Birdy",            "https://www.birdyqt.co.nz/",         "Cocktails and karaoke");
        _svc.AddBookmarkToStay(qtnId, "Little Blackwood", "https://littleblackwood.com/",        "Queensland's best cocktail bar");
        _svc.AddBookmarkToStay(qtnId, "Ziptrek Ecotours", "https://ziptrek.com/",                "Ziplines");
        _svc.AddBookmarkToStay(qtnId, "Canyon Explorers", "https://www.canyonexplorers.nz/",     "Half day adventures of rivers and waterfalls");

        _svc.AddLodgingOptionToStay(qtnId, "https://www.booking.com/hotel/nz/st-james-apartments.html",
            "St James Apartments", new DateTime(2026, 11, 8), new DateTime(2026, 11, 10), 534m);
        _svc.SelectLodgingOption(qtnId, _svc.GetLodgingOptionsForStay(qtnId).Single().Id);

        // ── Auckland (return) ─────────────────────────────────────────────────
        _svc.AddStay("Auckland", "New Zealand",
            new DateTime(2026, 11, 10), new DateTime(2026, 11, 11), StayStatus.Shortlist);
        var aklReturnId = _svc.GetStays().Single(s => s.City == "Auckland" && s.StartDate == new DateTime(2026, 11, 10)).Id;

        _svc.AddFlightOptionToStay(aklReturnId, "https://www.google.com/travel/flights/s/kf3doqh3q5zH1udk8",
            "AKL", "SFO", new DateTime(2026, 11, 11, 14, 50, 0), new DateTime(2026, 11, 11, 14, 50, 0), 436m);

        _svc.AddFlightOptionToStay(aklReturnId, "https://www.google.com/travel/flights/s/fb4Ju8huaBnCKMFm7",
            "ZQN", "AKL", new DateTime(2026, 11, 10, 12, 50, 0), new DateTime(2026, 11, 10, 14, 40, 0), 100m);
        _svc.SelectFlightOption(aklReturnId, _svc.GetFlightOptionsForStay(aklReturnId).Single(f => f.FromAirportCode == "ZQN").Id);

        _svc.AddLodgingOptionToStay(aklReturnId, "https://www.marriott.com/en-us/hotels/aklfp-four-points-auckland/overview/",
            "Four Points by Sheraton Auckland", new DateTime(2026, 11, 10), new DateTime(2026, 11, 11), 265.77m);
        _svc.SelectLodgingOption(aklReturnId, _svc.GetLodgingOptionsForStay(aklReturnId).Single().Id);

        // ── Momi, Fiji ────────────────────────────────────────────────────────
        _svc.AddStay("Momi", "Fiji",
            new DateTime(2026, 11, 11), new DateTime(2026, 11, 14), StayStatus.Idea);
        var momiId = _svc.GetStays().Single(s => s.City == "Momi").Id;

        _svc.AddLodgingOptionToStay(momiId, "https://www.marriott.com/en-us/hotels/nanmc-fiji-marriott-resort-momi-bay/overview/",
            "Fiji Marriott Momi Bay",              new DateTime(2026, 11, 11), new DateTime(2026, 11, 14), 754m,  rating: 4.5m);
        _svc.AddLodgingOptionToStay(momiId, "https://www.marriott.com/en-us/hotels/nanmc-fiji-marriott-resort-momi-bay/overview/",
            "Fiji Marriott Momi Bay All Inclusive", new DateTime(2026, 11, 11), new DateTime(2026, 11, 14), 1543m, rating: 4.5m);
    }

    private void SeedJapan2027()
    {
        var trip = _svc.CreateTrip("Japan 2027", 7000m);
        _svc.SelectTrip(trip.Id);

        // ── Tokyo ─────────────────────────────────────────────────────────────
        _svc.AddStay("Tokyo", "Japan",
            new DateTime(2027, 1, 1), new DateTime(2027, 1, 5), StayStatus.Idea);
        var tokyoId = _svc.GetStays().Single(s => s.City == "Tokyo").Id;

        _svc.AddBookmarkToStay(tokyoId, "Vegan Ramen",
            "https://www.itravelforveganfood.com/articles/where-to-find-the-best-vegan-ramen-in-japan",
            "Covers Tokyo and Osaka");
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
