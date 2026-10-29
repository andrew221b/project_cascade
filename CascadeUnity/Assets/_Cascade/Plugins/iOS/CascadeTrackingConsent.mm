#import <Foundation/Foundation.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
extern "C" void CascadeRequestTrackingAuthorization(void (*completion)(int))
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 14.0, *)) {
            ATTrackingManagerAuthorizationStatus status = [ATTrackingManager trackingAuthorizationStatus];
            if (status == ATTrackingManagerAuthorizationStatusNotDetermined) {
                [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus result) {
                    dispatch_async(dispatch_get_main_queue(), ^{ if (completion) completion((int)result); });
                }];
            } else if (completion) completion((int)status);
        } else if (completion) completion(3);
    });
}
