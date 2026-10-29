package com.vickand.flyingladders.platform;
import android.app.Activity;
import android.os.Build;
import android.window.OnBackInvokedCallback;
import java.lang.ref.WeakReference;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;

/** Back commit only. No Unity calls, file I/O or waits on Android's UI thread. */
public final class CascadeAndroidBack {
    private static final AtomicBoolean pending = new AtomicBoolean();
    private static final AtomicInteger generation = new AtomicInteger();
    private CascadeAndroidBack() {}
    public static boolean consume() { return pending.getAndSet(false); }
    public static void install(final Activity activity) {
        if (Build.VERSION.SDK_INT < 33 || activity == null) return;
        final int ticket = generation.incrementAndGet();
        activity.runOnUiThread(() -> { if (ticket == generation.get() && !activity.isDestroyed() && !activity.isFinishing()) Api33.install(activity); });
    }
    public static void uninstall() {
        pending.set(false);
        int ticket = generation.incrementAndGet();
        if (Build.VERSION.SDK_INT >= 33) Api33.uninstall(ticket);
    }
    public static void background(final Activity activity) {
        if (activity == null) return;
        activity.runOnUiThread(() -> { if (!activity.isDestroyed() && !activity.isFinishing()) activity.moveTaskToBack(true); });
    }
    private static final class Api33 {
        private static volatile WeakReference<Activity> owner = new WeakReference<>(null);
        private static final OnBackInvokedCallback callback = () -> pending.set(true);
        static void install(Activity activity) {
            Activity previous = owner.get();
            if (previous != null && !previous.isDestroyed()) previous.getOnBackInvokedDispatcher().unregisterOnBackInvokedCallback(callback);
            owner = new WeakReference<>(activity);
            activity.getOnBackInvokedDispatcher().registerOnBackInvokedCallback(0, callback);
        }
        static void uninstall(final int ticket) {
            final Activity activity = owner.get();
            if (activity != null) activity.runOnUiThread(() -> {
                if (ticket == generation.get()) {
                    if (!activity.isDestroyed()) activity.getOnBackInvokedDispatcher().unregisterOnBackInvokedCallback(callback);
                    owner = new WeakReference<>(null);
                }
            });
        }
    }
}
