using System.Linq;
using Cascade.Presentation;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class DialoguePagesTests
    {
        [Test] public void PagesKeepEveryWordInOrder()
        {
            const string text="Мика несёт огонёк. Брам чинит мост, а Сора ждёт на другом острове.";
            var pages=DialoguePages.Split(text,t=>t.Length<=32);
            Assert.That(pages.Count,Is.GreaterThan(1));
            Assert.That(string.Join(" ",pages),Is.EqualTo(text));
            Assert.That(pages.All(p=>p.Length<=32),Is.True);
        }
        [Test] public void AnOversizedWordRemainsReadableAndDoesNotLoop()
        {
            var pages=DialoguePages.Split("constellations light",t=>t.Length<=6);
            Assert.That(pages,Is.EqualTo(new[]{"constellations","light"}));
            Assert.That(DialoguePages.Split("",_=>true),Is.EqualTo(new[]{""}));
        }
        [Test] public void StoryPagesNeverStopInsideASentence()
        {
            const string copy="Стик тримається. А мені в кожному скрипі досі вчувається буря.";
            var pages=DialoguePages.SplitSentences(copy,t=>t.Length<=28);
            Assert.That(pages,Is.EqualTo(new[]{"Стик тримається.","А мені в кожному скрипі досі вчувається буря."}));
            Assert.That(string.Join(" ",pages),Is.EqualTo(copy));
        }
    }
}
