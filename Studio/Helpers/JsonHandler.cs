﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Studio.Models;

namespace Studio.Helpers
{
    class JsonHandler
    {
        private static readonly string[] _divs = { "bronze", "silver", "gold", "platinum", "diamond", "master", "grandmaster", "champion" };
        private static readonly Dictionary<string, int> _divisions = new Dictionary<string, int> { { "bronze", 1000 }, { "silver", 1500 }, { "gold", 2000 }, { "platinum", 2500 }, { "diamond", 3000 }, { "master", 3500 }, { "grandmaster", 4000 }, { "champion", 4500 } };
        public static ApiResponse DeserializeApiResponseJson(string json)
        {
            return JsonSerializer.Deserialize<ApiResponse>(json)!;
        }

        public static UserData DeserializeUserDataJson(string json)
        {
            return JsonSerializer.Deserialize<UserData>(json)!;
        }

        public static string LoadJsonFromFile(string filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                string json = reader.ReadToEnd();
                return json;
            }
        }

        public static void WriteUserDataToFile(UserData data, string filepath)
        {
            File.WriteAllText(filepath, JsonSerializer.Serialize(data));
        }




        //obsolete
        public static UserData CreateUserData(string username, string tag, string? email)
        {

            ApiResponse response = Api.ApiRequest($"{username}-{tag}").Result;
            Battletag battletag = new Battletag(username, tag);

            UserData userData = new UserData
            {
                Battletag = battletag,
                CustomId = battletag?.ToString(),
                //Username = username,
                //Tag = tag,

                TimesLaunched = 0,
                TimesSwitched = 0,

                RankedCareer = new RankedCareer(),

            };

            if (email != null)
                userData.Email = email;


            if (response == null)
            {
                return userData;
            }

            userData.LastUpdate = response.last_updated_at;
            userData.Avatar = response.avatar;

            if (response.competitive.pc.tank != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.tank.division,
                    response.competitive.pc.tank.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.LastUpdate
                };

                userData.RankedCareer.Tank = new Tank()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };

            }
            if (response.competitive.pc.damage != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.damage.division,
                    response.competitive.pc.damage.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.LastUpdate
                };

                userData.RankedCareer.Damage = new Damage()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };

            }
            if (response.competitive.pc.support != null)
            {
                Rank rank = Rank.RankFromDivision(
                    response.competitive.pc.support.division,
                    response.competitive.pc.support.tier);
                RankMoment peakRankMoment = new RankMoment()
                {
                    Rank = rank,
                    Date = userData.LastUpdate
                };

                userData.RankedCareer.Support = new Support()
                {
                    CurrentRank = rank,
                    PeakRank = peakRankMoment,
                    RankMoments = new List<RankMoment> { peakRankMoment }
                };
            }
            return userData;
        }

    }



}
