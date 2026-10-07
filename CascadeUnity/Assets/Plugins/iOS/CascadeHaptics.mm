#import <UIKit/UIKit.h>
#import <dispatch/dispatch.h>
#include <math.h>

static void CascadePulseOnMainThread(float strength)
{
    if (@available(iOS 10.0, *)) {
        static UIImpactFeedbackGenerator *light;
        static UIImpactFeedbackGenerator *medium;
        if (light == nil) {
            light = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            medium = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
        }
        UIImpactFeedbackGenerator *generator = strength > .55f ? medium : light;
        if (@available(iOS 13.0, *))
            [generator impactOccurredWithIntensity:fminf(strength, .65f)];
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
