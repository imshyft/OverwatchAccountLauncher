﻿using System.IO;
using System.Text;
using Windows.System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Studio.Models;
using Newtonsoft.Json.Linq;
using Studio.Contracts.Services;
using System.Collections.ObjectModel;
using Studio.Services.Files;

namespace Studio.Services.Data;

public class StoredUserProfileDataService : UserProfileDataService
{
    private readonly FileService _fileService;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public StoredUserProfileDataService(FileService fileService, IOptions<AppConfig> appConfig)
    {
        ProfileDirectory = Path.Combine(_localAppData, appConfig.Value.ConfigurationFolderName, appConfig.Value.ProfilesPath);

        _fileService = fileService;

        if (!Directory.Exists(ProfileDirectory))
            Directory.CreateDirectory(ProfileDirectory);
    }

    public override void SaveProfile(Profile profile)
    {
        string fileName = $"{profile.Battletag}.json";
        _fileService.Save(ProfileDirectory, fileName, profile);

        base.SaveProfile(profile);
    }

    public override Profile ReadProfile(BattleTag battletag)
    {
        string fileName = $"{battletag}.json";
        var data = _fileService.Read<Profile>(ProfileDirectory, fileName);
        return data;
    }

    public override void DeleteProfile(Profile profile)
    {
        string fileName = $"{profile.Battletag}.json";

        _fileService.Delete(ProfileDirectory, fileName);

        base.DeleteProfile(profile);
    }

    public override void LoadProfilesFromDisk()
    {
        foreach (string file in Directory.GetFiles(ProfileDirectory))
        {
            Profiles.Add(_fileService.Read<Profile>(file));
        }
    }

}
