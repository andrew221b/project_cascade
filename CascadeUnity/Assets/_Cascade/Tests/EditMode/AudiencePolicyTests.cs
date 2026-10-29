using Cascade.Presentation;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class AudiencePolicyTests
    {
        [TestCase(AudienceAgeBand.Unknown,false,false,false)]
        [TestCase(AudienceAgeBand.NineToTwelve,true,true,false)]
        [TestCase(AudienceAgeBand.ThirteenToFifteen,true,true,false)]
        [TestCase(AudienceAgeBand.SixteenPlus,false,false,true)]
        public void AgeBandSelectsPrivacyBeforeServices(AudienceAgeBand band,bool child,bool underAge,bool account)
        {
            Assert.That(AudiencePolicy.IsChild(band),Is.EqualTo(child));
            Assert.That(AudiencePolicy.IsUnderAgeOfConsent(band),Is.EqualTo(underAge));
            Assert.That(AudiencePolicy.AllowsLinkedAccount(band),Is.EqualTo(account));
            Assert.That(AudiencePolicy.IsKnown(band),Is.EqualTo(band!=AudienceAgeBand.Unknown));
        }
    }
}
