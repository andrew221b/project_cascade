#import <UIKit/UIKit.h>
#import <dispatch/dispatch.h>
#include <math.h>

static void CascadePulseOnMainThread(float strength)
{
    if (@available(iOS 10.0, *)) {
        static UIImpactFeedbackGenerator *light;
        static UIImpactFeedbackGenerator *medium;
        static UIImpactFeedbackGenerator *heavy;
        static UIImpactFeedbackGenerator *soft;
        if (light == nil) {
            light = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            medium = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            heavy = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
            if (@available(iOS 13.0, *))
                soft = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleSoft];
        }
        UIImpactFeedbackGenerator *generator = strength >= .7f ? heavy : strength >= .38f ? medium : strength < .18f && soft != nil ? soft : light;
        if (@available(iOS 13.0, *))
            [generator impactOccurredWithIntensity:strength];
        else {
            generator = light;
            [generator impactOccurred];
        }
        // Keep the next nearby tap prepared; preparing immediately before impact adds no lead time.
        [generator prepare];
    }
}

extern "C" void CascadePulseHaptic(float strength)
{
    if (!isfinite(strength) || strength <= 0) return;
    strength = fminf(strength, 1.f);
    if ([NSThread isMainThread])
        CascadePulseOnMainThread(strength);
    else
        dispatch_async(dispatch_get_main_queue(), ^{ CascadePulseOnMainThread(strength); });
}
