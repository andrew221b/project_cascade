#import <AVFoundation/AVFoundation.h>

extern "C" int CascadeConfigurePlaybackAudio()
{
    @autoreleasepool {
        AVAudioSession *session = [AVAudioSession sharedInstance];
        AVAudioSessionCategoryOptions options = AVAudioSessionCategoryOptionMixWithOthers;
        if ([session.category isEqualToString:AVAudioSessionCategoryPlayback] &&
            [session.mode isEqualToString:AVAudioSessionModeDefault] &&
            session.categoryOptions == options)
            return 1;

        NSError *error = nil;
        BOOL configured = [session setCategory:AVAudioSessionCategoryPlayback
                                         mode:AVAudioSessionModeDefault
                                      options:options
                                        error:&error];
        // Unity owns activation, focus and interruptions. Do not activate an ad's
        // session, force the speaker route, change volume or enable background audio.
        if (!configured)
            NSLog(@"CASCADE playback audio category failed (%ld)", (long)error.code);
        return configured ? 1 : 0;
    }
}
