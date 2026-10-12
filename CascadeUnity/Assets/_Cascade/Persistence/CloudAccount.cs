using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cascade.Persistence
{
    public sealed class CloudAccount
    {
        public const int CurrentSchema=1,MaxDevices=8;
        public int Schema=CurrentSchema;
        public long Revision;
        public PlayerProfile Profile=new PlayerProfile();
        public List<DeviceCheckpoint> Devices=new List<DeviceCheckpoint>();

        public void Validate()
        {
            if(Schema!=CurrentSchema||Revision<0||Profile==null||Devices==null||Devices.Count>MaxDevices
                ||Devices.Any(d=>d==null)||Devices.Select(d=>d.DeviceId).Distinct(StringComparer.Ordinal).Count()!=Devices.Count)
                throw new InvalidDataException("Invalid cloud account.");
            Profile.Validate();foreach(var device in Devices)device.Validate();
        }
        public void UpsertDevice(DeviceCheckpoint checkpoint)
        {
            if(checkpoint==null)throw new ArgumentNullException(nameof(checkpoint));checkpoint.Validate();
            Devices.RemoveAll(d=>d.DeviceId==checkpoint.DeviceId);Devices.Add(CloudReconciler.Clone(checkpoint));
            Devices=Devices.OrderByDescending(d=>d.UpdatedUnixSeconds).ThenBy(d=>d.DeviceId,StringComparer.Ordinal).Take(MaxDevices).ToList();
        }
    }

    // Checkpoints carry resumable state, while the shared profile owns earned progress/economy.
    public sealed class DeviceCheckpoint
    {
        public string DeviceId,PreferredMode="Shapes";
        public long UpdatedUnixSeconds;
        public List<ModeJourney> Journeys=new List<ModeJourney>();

        public static DeviceCheckpoint Capture(string deviceId,long updatedUnixSeconds,PlayerProfile profile)
        {
            if(profile==null)throw new ArgumentNullException(nameof(profile));
            var copy=CloudReconciler.Clone(profile);var result=new DeviceCheckpoint{DeviceId=deviceId,UpdatedUnixSeconds=updatedUnixSeconds,PreferredMode=copy.PreferredMode};
            foreach(var journey in new[]{ModeJourney.Capture(copy)}.Concat(copy.ModeJourneys))result.Journeys.Add(new ModeJourney{
                Mode=journey.Mode,JourneyAttempt=journey.JourneyAttempt,StoryRoute=journey.StoryRoute,
                Attempt=journey.Attempt,Descent=journey.Descent,Completion=journey.Completion,Director=journey.Director});
            result.Validate();return result;
        }
        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(DeviceId)||DeviceId.Length>128||UpdatedUnixSeconds<0||!CloudReconciler.ValidMode(PreferredMode)
                ||Journeys==null||Journeys.Count<1||Journeys.Count>3||Journeys.Any(j=>j==null||!CloudReconciler.ValidMode(j.Mode))
                ||Journeys.Select(j=>j.Mode).Distinct(StringComparer.Ordinal).Count()!=Journeys.Count||!Journeys.Any(j=>j.Mode==PreferredMode))
                throw new InvalidDataException("Invalid device checkpoint.");
            foreach(var journey in Journeys){var check=new PlayerProfile();journey.Apply(check);check.PreferredMode=journey.Mode;check.Validate();}
        }
        public void ApplyTo(PlayerProfile profile)
        {
            if(profile==null)throw new ArgumentNullException(nameof(profile));Validate();
            profile.SelectJourney(PreferredMode);
            foreach(var source in CloudReconciler.Clone(Journeys)){
                var destination=source.Mode==profile.PreferredMode?ModeJourney.Capture(profile):profile.ModeJourneys.FirstOrDefault(j=>j.Mode==source.Mode);
                if(destination==null){destination=new ModeJourney{Mode=source.Mode};profile.ModeJourneys.Add(destination);}
                destination.Attempt=source.Attempt;destination.Descent=source.Descent;destination.Completion=source.Completion;
                destination.Director=source.Director;destination.JourneyAttempt=source.JourneyAttempt;destination.StoryRoute=source.StoryRoute;
                if(source.Mode==profile.PreferredMode)destination.Apply(profile);
            }
        }
    }

    public sealed class CloudMergeResult
    {
        public PlayerProfile Profile {get;}
        public bool EconomyConflict {get;}
        internal CloudMergeResult(PlayerProfile profile,bool economyConflict){Profile=profile;EconomyConflict=economyConflict;}
    }
}
