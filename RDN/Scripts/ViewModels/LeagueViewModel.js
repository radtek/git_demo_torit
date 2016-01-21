﻿var LeagueViewModel = new function () {
    var thisViewModel = this;
    this.Leagues = ko.observableArray([]);
    this.maxId = ko.observable();
    this.pendingRequest = ko.observable(false);
    this.page = ko.observable();
    this.IsFinishedScrolling = ko.observable(false);
    this.SearchText = ko.observable();
    this.ListCountPull = ko.observable();

    this.LeaguesStats = ko.observableArray([]);
    this.LeaguesCount = ko.observable();
    var lastLeagueSearch = "";

    this.LoadLeagueList = function (count) {
        thisViewModel.ListCountPull(count);
        thisViewModel.page(0);
        getItems(thisViewModel.ListCountPull(), false);
        $(window).scroll(function () {
            if ($(window).scrollTop() >= $(document).height() - $(window).height() - 400) {
                getItems(thisViewModel.ListCountPull(), false);
            }
        });
    }

    this.LoadLeagueStats = function () {
        $.ajax({
            url: apiUrl + "League/GetLeagueStats",
            dataType: "json",
            success: function (data) {
                thisViewModel.LeaguesCount(data.nrOfLeagues);
                thisViewModel.LeaguesStats(data.leaguesData);
            },
            error: function () {

            }
        });
    }

    this.LoadTweets = function (userName) {
        userName = userName.substring(userName.lastIndexOf("/") + 1, userName.length);
        $.ajax({
            type: "POST",
            url: apiUrl + "Utilities/GetTweets",
            data: { userName: userName },
            dataType: "json",
            success: function (data) {

            }
        });
    };
    this.SearchLeagues = function (input) {
        thisViewModel.SearchText($(input).val());
        thisViewModel.IsFinishedScrolling(false);
        thisViewModel.pendingRequest(false);
        thisViewModel.page(0);
        getItems(thisViewModel.ListCountPull(), true);
    };

    function getItems(cnt, isSearch) {
        if (thisViewModel.SearchText() != null) {
            var text = thisViewModel.SearchText();
            thisViewModel.SearchText(text.trim());
        }
        if (!thisViewModel.pendingRequest() && !thisViewModel.IsFinishedScrolling()) {
            thisViewModel.pendingRequest(true);
            $.ajax({
                type: "POST",
                url: apiUrl + "League/GetAllLeagues",
                data: { p: thisViewModel.page(), c: cnt, s: thisViewModel.SearchText() },
                dataType: "json",
                success: function (data) {
                    if (data.leagues.length > 0) {
                        if (!isSearch) {
                            ko.utils.arrayForEach(data.leagues, function (entry) {
                                thisViewModel.Leagues.push(entry);
                            });
                        } else {
                            thisViewModel.Leagues.removeAll();
                            thisViewModel.Leagues(data.leagues);
                        }
                        thisViewModel.page(thisViewModel.page() + 1);
                        thisViewModel.pendingRequest(false);
                    } else {
                        thisViewModel.IsFinishedScrolling(true);

                    }
                }
            });

        }
    }
}