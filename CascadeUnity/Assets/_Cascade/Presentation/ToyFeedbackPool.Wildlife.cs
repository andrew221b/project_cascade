using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyFeedbackPool
    {
        sealed class WildlifeVoice
        {
            public AudioSource Source;public WildlifeAudioEmitter Actor;public WildlifeCall Call;public float Gain,Pitch;
        }
        readonly List<WildlifeAudioEmitter> wildlifeActors=new List<WildlifeAudioEmitter>(24);
        readonly WildlifeVoice[] wildlifeVoices=new WildlifeVoice[2];
        AudioClip[] birdContacts,birdSongs,robinSongs,wrenSongs,seaCalls,owlCalls,cricketCalls;
        AudioClip bee;
        WildlifeAudioEmitter singingCricket;
        float wildlifeClock,wildlifeNextPhrase;
        int wildlifePhrase;
        public int WildlifeCallsStarted{get;private set;}
        public int RegisteredWildlifeCount=>wildlifeActors.Count;
        public int ActiveWildlifeVoices
        {
            get{int n=cricketSource!=null&&cricketSource.isPlaying?1:0;foreach(var voice in wildlifeVoices)if(voice?.Source!=null&&voice.Source.isPlaying)n++;return n;}
        }
        void InitializeWildlifeAudio()
        {
            const string path="CascadeArt/Audio/Wildlife/";
            AudioClip Clip(string name)=>Resources.Load<AudioClip>(path+name);
            birdContacts=new[]{Clip("SongbirdContactA"),Clip("SongbirdContactB")};
            birdSongs=new[]{Clip("SongbirdA"),Clip("SongbirdB")};
            robinSongs=new[]{Clip("SongbirdRobinA"),Clip("SongbirdRobinB")};
            wrenSongs=new[]{Clip("SongbirdWrenA"),Clip("SongbirdWrenB")};
            seaCalls=new[]{Clip("Seabird"),Clip("SeabirdContact")};owlCalls=new[]{Clip("Owl"),Clip("OwlAnswer")};
            cricketCalls=new[]{Clip("CricketA"),Clip("CricketB"),Clip("CricketC")};bee=Clip("Bumblebee");
            for(int i=0;i<wildlifeVoices.Length;i++){
                var source=LoopSource("Positioned wildlife voice "+i,null);source.loop=false;source.priority=224;
                source.bypassReverbZones=true;source.outputAudioMixerGroup=windSource.outputAudioMixerGroup;
                wildlifeVoices[i]=new WildlifeVoice{Source=source};
            }
        }
        internal void RegisterWildlife(WildlifeAudioEmitter actor){if(!wildlifeActors.Contains(actor))wildlifeActors.Add(actor);}
        internal void UnregisterWildlife(WildlifeAudioEmitter actor)
        {
            wildlifeActors.Remove(actor);
            foreach(var voice in wildlifeVoices)if(voice?.Source!=null&&voice.Actor==actor){voice.Source.Stop();voice.Source.volume=0;voice.Actor=null;}
            if(singingCricket==actor){StopNightCrickets();singingCricket=null;}
        }
        void StopWildlifeAudio()
        {
            foreach(var voice in wildlifeVoices)if(voice?.Source!=null){voice.Source.Stop();voice.Source.volume=0;voice.Actor=null;}
            foreach(var actor in wildlifeActors)if(actor!=null)actor.VisibleSince=-1;
            wildlifeNextPhrase=wildlifeClock+.7f;
        }
        AudioClip WildlifeClip(WildlifeAudioEmitter actor)
        {
            if(actor.Call==WildlifeCall.Buzz)return bee;
            AudioClip[] bank;
            if(actor.Call==WildlifeCall.Seabird)bank=seaCalls;
            else if(actor.Call==WildlifeCall.Owl)bank=owlCalls;
            else if(!actor.IsResting)bank=birdContacts;
            else if(actor.Species==WildlifeSpecies.Robin)bank=robinSongs;
            else if(actor.Species==WildlifeSpecies.WinterWren||actor.Species==WildlifeSpecies.Goldfinch)bank=wrenSongs;
            else bank=birdSongs;
            // Phrase history belongs to the individual, so another bird cannot make it repeat.
            int identity=(int)(WildlifeMotion.Hash(actor.GetInstanceID()+149)*997);
            int choice=(identity+actor.PhraseIndex)%bank.Length;
            if(bank[choice]==actor.LastPhrase)choice=(choice+1)%bank.Length;
            foreach(var voice in wildlifeVoices)
                if(voice?.Actor!=null&&voice.Source.isPlaying&&voice.Source.clip==bank[choice]&&bank[(choice+1)%bank.Length]!=actor.LastPhrase)
                    choice=(choice+1)%bank.Length;
            return bank[choice];
        }
        void TickWildlifeAudio(float dt,bool allowed)
        {
            allowed=allowed&&game!=null&&!game.ConversationOpen&&game.CurrentScreen!=GameScreen.Prologue;
            if(!allowed){StopWildlifeAudio();TickNightCrickets(dt,false);return;}
            wildlifeClock+=dt;
            foreach(var voice in wildlifeVoices){
                if(voice==null)continue;
                if(voice.Actor!=null&&voice.Actor.Call==voice.Call&&voice.Actor.TrySample(out var position,out float gain)){
                    voice.Gain=gain*(duck>0?.28f:1);voice.Source.panStereo=Mathf.MoveTowards(voice.Source.panStereo,WildlifeAcoustics.Pan(position.x),dt*2.8f);
                    if(voice.Call==WildlifeCall.Buzz)voice.Source.pitch=Mathf.Lerp(voice.Source.pitch,voice.Pitch*Mathf.Lerp(.94f,1.1f,Mathf.Clamp01(voice.Actor.FlightSpeed/.18f)),1-Mathf.Exp(-dt/.25f));
                }else voice.Gain=0;
                voice.Source.volume=Mathf.MoveTowards(voice.Source.volume,voice.Gain,dt*.8f);
                if(!voice.Source.isPlaying||voice.Gain==0&&voice.Source.volume<.001f){voice.Source.Stop();voice.Source.volume=0;voice.Actor=null;}
            }
            WildlifeAudioEmitter candidate=null,cricket=null;float best=-1,cricketGain=0;Vector2 cricketPosition=default;
            for(int i=wildlifeActors.Count-1;i>=0;i--){
                var actor=wildlifeActors[i];if(actor==null){wildlifeActors.RemoveAt(i);continue;}
                if(!actor.TrySample(out var position,out float gain)){actor.VisibleSince=-1;continue;}
                actor.ObserveMotion(position,dt);
                if(actor.VisibleSince<0)actor.VisibleSince=wildlifeClock;
                if(actor.Call==WildlifeCall.Cricket){if(gain>cricketGain){cricket=actor;cricketGain=gain;cricketPosition=position;}continue;}
                bool playing=false,buzzBusy=false;
                foreach(var voice in wildlifeVoices){if(voice?.Actor==actor)playing=true;if(voice?.Actor!=null&&voice.Source.loop)buzzBusy=true;}
                float delay=actor.Call==WildlifeCall.Buzz?.12f:.45f+WildlifeMotion.Hash(actor.GetInstanceID())*.55f;
                if(playing||actor.Call==WildlifeCall.Buzz&&buzzBusy||wildlifeClock<actor.NextCallAt||wildlifeClock-actor.VisibleSince<delay)continue;
                // A nearby eligible voice speaks first, but its own cooldown lets a neighbour answer.
                if(gain>best){best=gain;candidate=actor;}
            }
            if(cricketSource.isPlaying&&singingCricket!=null&&singingCricket.TrySample(out var heldPosition,out float heldGain)){
                cricket=singingCricket;cricketPosition=heldPosition;cricketGain=heldGain;
            }else if(!cricketSource.isPlaying)singingCricket=cricket;
            TickNightCrickets(dt,cricket!=null);
            if(cricket!=null){cricketSource.panStereo=Mathf.MoveTowards(cricketSource.panStereo,WildlifeAcoustics.Pan(cricketPosition.x),dt*2.8f);cricketSource.volume=Mathf.Min(cricketSource.volume,cricketGain*(duck>0?.28f:1));}
            if(candidate==null||wildlifeClock<wildlifeNextPhrase||duck>0)return;
            WildlifeVoice available=null;foreach(var voice in wildlifeVoices)if(voice!=null&&voice.Actor==null&&!voice.Source.isPlaying){available=voice;break;}
            if(available==null)return;var clip=WildlifeClip(candidate);if(clip==null)return;
            if(clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();if(clip.loadState!=AudioDataLoadState.Loaded)return;
            candidate.TrySample(out var point,out float volume);available.Actor=candidate;available.Call=candidate.Call;available.Gain=volume;
            var sound=available.Source;sound.clip=clip;sound.loop=candidate.Call==WildlifeCall.Buzz;
            available.Pitch=WildlifeAcoustics.VoicePitch(candidate.Species,candidate.GetInstanceID());
            sound.panStereo=WildlifeAcoustics.Pan(point.x);sound.pitch=available.Pitch*(.99f+WildlifeMotion.Hash(candidate.PhraseIndex+31)*.02f);
            sound.volume=0;sound.Play();WildlifeCallsStarted++;candidate.PhraseIndex++;candidate.LastPhrase=clip;
            float gap=candidate.Call==WildlifeCall.Owl?16:candidate.Call==WildlifeCall.Seabird?10:candidate.IsResting?8:5;
            candidate.NextCallAt=wildlifeClock+gap+WildlifeMotion.Hash(candidate.PhraseIndex*37+candidate.GetInstanceID())*5;
            wildlifeNextPhrase=wildlifeClock+(candidate.Call==WildlifeCall.Buzz?.3f:.7f+WildlifeMotion.Hash(wildlifePhrase+71)*.6f);wildlifePhrase++;
        }
    }
}
