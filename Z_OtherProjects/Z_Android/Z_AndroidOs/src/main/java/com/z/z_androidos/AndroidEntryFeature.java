package com.z.z_androidos;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;

public class AndroidEntryFeature {

    public void LoadImage(Activity activity,int callbackId) {
        Intent intent=new Intent(activity,FileLoader.class);

        intent.putExtra("callbackId", callbackId);
        activity.startActivity(intent);

    }
    public void LoadVideo(Activity activity,int callbackId) {
        Intent intent=new Intent(activity,FileLoader.class);

        intent.putExtra("callbackId", callbackId);
        activity.startActivity(intent);
    }

    public void OpenCamera(Activity activity,int callbackId)
    {

    }
    public void CloseCamera(Activity activity,int callbackId)
    {

    }
    public static void Log(String content)
    {
        Log.i("Z_AndroidOs",content);
    }


}
