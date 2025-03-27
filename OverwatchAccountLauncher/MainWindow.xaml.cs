﻿using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OverwatchAccountLauncher.Classes;
using Studio.Models;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;



namespace OverwatchAccountLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }


        // Create New User And Save Data To File
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BattleTag battleTag = new BattleTag("FstAsFoxG4irl#1143");

            BlizzardProfileFetchResult result = await FetchProfileAsync(battleTag);
            if (result.Outcome  == BlizzardProfileFetchOutcome.Success)
            {

            }
            
        }

        private async Task<BlizzardProfileFetchResult> FetchProfileAsync(BattleTag battleTag)
        {
            Profile profile = new Profile();
            profile.Battletag = battleTag;


            string errorMessage = "";

            string battleTagUrl = battleTag.ToString().Replace("#", "-");
            string url = $"https://overwatch.blizzard.com/en-us/careers/{battleTagUrl}";


            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                errorMessage = response.ReasonPhrase ?? "unexplained error.";

                return new BlizzardProfileFetchResult()
                {
                    Profile = profile,
                    Outcome = BlizzardProfileFetchOutcome.Error,
                    ErrorMessage = errorMessage
                };
            }

            string htmlContent = await response.Content.ReadAsStringAsync();

            // initialising angle-sharp document
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(htmlContent));


            

            if (document.QuerySelector(".error-contain") is not null)
            {
                return new BlizzardProfileFetchResult()
                {
                    Profile = profile,
                    Outcome = BlizzardProfileFetchOutcome.NotFound,
                };
            }
                

            

            int unixNow = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            profile.Avatar = document.QuerySelector<IHtmlImageElement>(".Profile-player--portrait")?.Source ??
                "https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png";
            profile.TimesLaunched = 0;
            profile.CustomId = battleTag.Username;
            profile.LastUpdate = unixNow;
            profile.RankedCareer = new RankedCareer();
            profile.Battletag = battleTag;

            // getting roles for each rank
            foreach (IElement roleElement in document.QuerySelectorAll(".Profile-playerSummary--roleWrapper"))
            {
                string? role_img = roleElement.QuerySelector(".Profile-playerSummary--role")?.QuerySelector<IHtmlImageElement>("img")?.Source;
                if (role_img == null)
                    continue;

                string? roleString = Regex.Match(role_img.Split("/").Last(), @"([^.]+)-")?.Groups[1].Value;
                if (string.IsNullOrEmpty(roleString))
                    continue;


                Role role;
                switch (roleString)
                {
                    case "offense":
                        profile.RankedCareer.Damage = new Damage();
                        role = profile.RankedCareer.Damage;
                        break;
                    case "tank":
                        profile.RankedCareer.Tank = new Tank();
                        role = profile.RankedCareer.Tank;
                        break;
                    case "support":
                        profile.RankedCareer.Support = new Support();
                        role = profile.RankedCareer.Support;
                        break;
                    default:
                        continue;
                }



                var imageElements = roleElement.QuerySelector(".Profile-playerSummary--rankImageWrapper")?.QuerySelectorAll<IHtmlImageElement>("img").ToList();
                string? divisionSource = imageElements[0]?.Source;
                string? tierSource = imageElements[1]?.Source;

                if (string.IsNullOrEmpty(tierSource) || string.IsNullOrEmpty(divisionSource))
                    continue;

                var divisionMatch = Regex.Match(divisionSource, @"_([^_-]+)-");
                var tierMatch = Regex.Match(tierSource, @"_(\d+)-");

                if (!tierMatch.Success || divisionMatch.Success)
                    continue;

                string tierString = tierMatch.Groups[1].Value;
                string divisionString = divisionMatch.Groups[1].Value;

                if (string.IsNullOrEmpty(tierString) || string.IsNullOrEmpty(divisionString))
                    continue;

                if (!int.TryParse(tierString, out int tier))
                    continue;

                // remove 'Tier' from ending of division string
                Rank currentRank = Rank.RankFromDivision(divisionString.Remove(divisionMatch.Length - 4), tier);
                RankMoment rankMoment = new()
                {
                    Rank = currentRank,
                    Date = unixNow
                };

                role.PeakRank = rankMoment;
                role.CurrentRank = currentRank;
                role.RankMoments = [rankMoment];


            }
            return new BlizzardProfileFetchResult()
            {
                Profile = profile,
                Outcome = BlizzardProfileFetchOutcome.Success,
            };
        }
        // Switch First Account
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Mem.Test();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }
    }

    public class BlizzardProfileFetchResult
    {
        public Profile Profile { get; set; }
        public BlizzardProfileFetchOutcome Outcome { get; set; }
        public string? ErrorMessage { get; set; }

    }

    public enum BlizzardProfileFetchOutcome
    {
        Success,
        NotFound,
        Error
    }
}