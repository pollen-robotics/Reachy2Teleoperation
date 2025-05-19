package com.pollenrobotics.gstreamer;

import android.opengl.GLES20;
import android.os.Bundle;
import android.system.ErrnoException;
import android.system.Os;
import android.util.Log;
import android.view.Surface;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.widget.FrameLayout;
import com.unity3d.player.UnityPlayerActivity;
import org.freedesktop.gstreamer.GStreamer;

public class GstreamerActivity extends UnityPlayerActivity {

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Log.d("GstreamerActivity", "onCreate called!");

        try {
            /*Os.setenv(
                "GST_DEBUG_FILE",
                "/storage/emulated/0/Android/data/com.DefaultCompany.UnityProject/files/gstreamer.log",
                //"/sdcard/Android/data/com.DefaultCompany.UnityProject/files/gstreamer/gstreamer.log",
                true
            );*/
            Os.setenv("GST_DEBUG_NO_COLOR", "1", true);
            Os.setenv("GST_DEBUG", "2", true);
        } catch (ErrnoException ex) {
            Log.d(
                "OverrideActivity",
                "ErrnoException caught: " + ex.getMessage()
            );
        }
        System.loadLibrary("gstreamer_android");
        try {
            GStreamer.init(this);
        } catch (Exception e) {
            e.printStackTrace();
        }

    }
}
