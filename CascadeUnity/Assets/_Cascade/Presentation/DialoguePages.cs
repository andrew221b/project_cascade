using System;
using System.Collections.Generic;

namespace Cascade.Presentation
{
    // Readable chunks are presentation only; story receipts and authored lines keep their IDs.
    public static class DialoguePages
    {
        // Story pages end at spoken sentence boundaries, so Next never reveals a severed thought.
        public static IReadOnlyList<string> SplitSentences(string text,Func<string,bool> fits)
        {
            var pages=new List<string>();var sentence=new System.Text.StringBuilder();string current="";
            string copy=string.Join(" ",(text??"").Split(new[]{' ','\n','\r'},StringSplitOptions.RemoveEmptyEntries));
            for(int i=0;i<copy.Length;i++){
                sentence.Append(copy[i]);
                bool end=(copy[i]=='.'||copy[i]=='!'||copy[i]=='?'||copy[i]=='…')&&(i+1==copy.Length||copy[i+1]==' ');
                if(!end&&i+1<copy.Length)continue;
                string phrase=sentence.ToString().Trim();sentence.Clear();
                if(phrase.Length==0)continue;
                string joined=current.Length==0?phrase:current+" "+phrase;
                if(current.Length>0&&!fits(joined)){pages.Add(current);current=phrase;}else current=joined;
            }
            if(current.Length>0)pages.Add(current);
            if(pages.Count==0)pages.Add("");
            return pages;
        }
        public static IReadOnlyList<string> Split(string text,Func<string,bool> fits)
        {
            var pages=new List<string>();string current="";
            foreach(string word in (text??"").Split(new[]{' ','\n','\r'},StringSplitOptions.RemoveEmptyEntries)){
                string candidate=current.Length==0?word:current+" "+word;
                if(current.Length>0&&!fits(candidate)){pages.Add(current);current=word;}else current=candidate;
            }
            if(current.Length>0)pages.Add(current);
            if(pages.Count==0)pages.Add("");
            return pages;
        }
    }
}
