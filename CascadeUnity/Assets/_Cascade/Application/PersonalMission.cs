using System;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Application
{
    public sealed class PersonalMissionOffer
    {
        public int Serial,Hero;
        public string SourceHash,Locale;
        public DescentRun Run;
    }
    // Separate progress; no mission can grant a campaign first-clear receipt.
    public sealed class PersonalMissionMemory
    {
        public static int ChallengeFor(DirectorMemory director)
            =>director!=null&&director.Struggle>=2?0:director!=null&&director.CleanWins>=2?2:1;
        public int Serial,NextAfterClears=3,Completed;
        public string LastFingerprint;
        public PersonalMissionOffer Offer;
        public bool Active,Won;
        public AttemptRecord CampaignAttempt;
        public DescentRun CampaignDescent;
        public LevelCompletion CampaignCompletion;
        public bool CampaignJourney;
        public bool? CampaignStory;
        public bool Due(int clears)=>!Active&&Offer==null&&clears>=NextAfterClears;
        public void Present(PersonalMissionOffer offer,int clears)
        {
            Offer=offer;Serial=offer.Serial;NextAfterClears=clears+3+(Serial%3);
            LastFingerprint=offer.Run.ContentHash;
        }
        public void Decline(){if(Active)return;Offer=null;}
        public void ClearActive(){Offer=null;Active=Won=false;CampaignAttempt=null;CampaignDescent=null;CampaignCompletion=null;CampaignJourney=false;CampaignStory=null;}
        public bool IsValid()=>Serial>=0&&Serial<=10000&&NextAfterClears>=3&&NextAfterClears<=20000&&Completed>=0&&Completed<=Serial
            &&(!Active||Offer!=null&&CampaignAttempt!=null)&&(!Won||Active)
            &&(Active||CampaignAttempt==null&&CampaignDescent==null&&CampaignCompletion==null)
            &&(Offer==null||Offer.Serial==Serial&&Offer.Hero>=0&&Offer.Hero<9&&Offer.SourceHash!=null&&Offer.SourceHash.Length==64
                &&(Offer.Locale=="en"||Offer.Locale=="ru"||Offer.Locale=="uk")&&Offer.Run!=null&&Offer.Run.Version==6&&Offer.Run.SourceHash==Offer.SourceHash&&Offer.Run.Stages?.Length==3
                &&Offer.Run.Stages.All(s=>s!=null&&s.Triad!=null&&s.Witness!=null&&s.Minimum>0));
    }
}
