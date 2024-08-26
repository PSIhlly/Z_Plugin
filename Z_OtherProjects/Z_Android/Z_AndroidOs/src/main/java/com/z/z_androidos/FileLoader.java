package com.z.z_androidos;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.provider.MediaStore;

import com.unity3d.player.UnityPlayer;


public class FileLoader extends Activity {

    public static final int NONE = 0;
    public static String UNSPECIFIED = "image/*";//划重点，这里指定视频或图片

    private int callbackId;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Bundle extras = getIntent().getExtras();

        callbackId=extras.getInt("callbackId");

        Intent intent = new Intent(Intent.ACTION_PICK, null);
        intent.setDataAndType(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, UNSPECIFIED);
        startActivityForResult(intent, callbackId);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        //获取失败
        if (data == null) {
            this.finish();
            UnityPlayer.UnitySendMessage("AndroidDllActivityListener", "OnActivityResult", requestCode+"^");
            return;
        } else if (resultCode == NONE) {
            if (this.isFinishing() == false) {
                this.finish();
            }
            UnityPlayer.UnitySendMessage("AndroidDllActivityListener", "OnActivityResult", requestCode+"^");
            return;
        }
        ContentResolver resolver = getContentResolver();

        Uri originalUri = data.getData();

        String[] proj = {MediaStore.Images.Media.DATA};

        Cursor cursor = getContentResolver().query(originalUri, proj, null, null, null);
        int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
        cursor.moveToFirst();
        String _path = cursor.getString(column_index);
        UnityPlayer.UnitySendMessage("AndroidDllActivityListener", "OnActivityResult", requestCode+"^"+_path);
        super.onActivityResult(requestCode, resultCode, data);
        this.finish();
    }
}
