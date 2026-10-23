#import <UIKit/UIKit.h>

extern "C" void WobbleImpact(int strength)
{
    UIImpactFeedbackStyle style = strength == 2 ? UIImpactFeedbackStyleMedium : UIImpactFeedbackStyleLight;
    UIImpactFeedbackGenerator *feedback = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
    [feedback prepare];
    [feedback impactOccurredWithIntensity:strength == 0 ? 0.35 : (strength == 1 ? 0.6 : 0.8)];
}
