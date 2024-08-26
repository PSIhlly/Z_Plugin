package com.z.z_androidos;

import android.hardware.Camera;
import android.media.MediaRecorder;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.SurfaceHolder;
import android.view.SurfaceView;

import java.io.IOException;

import androidx.appcompat.app.AppCompatActivity;


public class CameraController extends AppCompatActivity {
    private int REQUEST_CODE_PERMISSIONS = 101;
    private final String [] REQUIRED_PERMISSIONS =new String[] {"android.permission.CAMERA","android.permission.WRITE_EXTERNAL_STORAGE"};
    TextureView textureView;
    ImageView cameraFlip;
    private int backlensfacing = 0;
    private int flashLamp = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        //setContentView(R.layout.activity_camerax_demo);

        //去掉导航栏
        getSupportActionBar().hide();

        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            //透明状态栏
            getWindow().addFlags(WindowManager.LayoutParams.FLAG_TRANSLUCENT_STATUS);
            //透明导航栏
            getWindow().addFlags(WindowManager.LayoutParams.FLAG_TRANSLUCENT_NAVIGATION);
        }


        //textureView = findViewById(R.id.view_camera);
        //cameraFlip = findViewById(R.id.btn_switch_camera);


        cameraFlip.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View view)
            {
                if(backlensfacing == 0)
                {
                    startCamera(CameraX.LensFacing.FRONT);
                    backlensfacing = 1;
                }
                else if(backlensfacing == 1)
                {
                    startCamera(CameraX.LensFacing.BACK);
                    backlensfacing = 0;
                }
            }

        });

        if(allPermissionsGranted())
        {
            startCamera(CameraX.LensFacing.BACK);
        }
        else {
            ActivityCompat.requestPermissions(this, REQUIRED_PERMISSIONS, REQUEST_CODE_PERMISSIONS);
        }


    }


    private void startCamera(CameraX.LensFacing CAMERA_ID)
    {
        unbindAll();
        Rational aspectRatio = new Rational(textureView.getWidth(), textureView.getHeight());
        Size screen = new Size(textureView.getWidth(),textureView.getHeight());
        PreviewConfig pConfig;
        Preview preview;
        pConfig = new PreviewConfig.Builder().setLensFacing(CAMERA_ID).setTargetAspectRatio(aspectRatio).setTargetResolution(screen).build();
        preview = new Preview(pConfig);

        preview.setOnPreviewOutputUpdateListener(new Preview.OnPreviewOutputUpdateListener() {
            @Override
            public void onUpdated(Preview.PreviewOutput output)
            {
                ViewGroup parent = (ViewGroup)textureView.getParent();
                parent.removeView(textureView);
                parent.addView(textureView,0);
                textureView.setSurfaceTexture(output.getSurfaceTexture());
                updateTransform();
            }
        });
        final ImageCaptureConfig imageCaptureConfig ;
        imageCaptureConfig= new ImageCaptureConfig.Builder().setCaptureMode(ImageCapture.CaptureMode.MAX_QUALITY).setTargetRotation(getWindowManager().getDefaultDisplay().getRotation()).setLensFacing(CAMERA_ID).build();
        final ImageCapture imgCap = new ImageCapture(imageCaptureConfig);

        findViewById(R.id.btn_flash).setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View view)
            {
                if (flashLamp == 0)
                {
                    flashLamp = 1;
                    imgCap.setFlashMode(FlashMode.OFF);
                    Toast.makeText(getBaseContext(), "Flash Disable", Toast.LENGTH_SHORT).show();
                }
                else if(flashLamp == 1)
                {
                    flashLamp = 0;
                    imgCap.setFlashMode(FlashMode.ON);
                    Toast.makeText(getBaseContext(), "Flash Enable", Toast.LENGTH_SHORT).show();
                }
            }
        });

        findViewById(R.id.btn_takePict).setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View view)
            {
                File image = null;
                String timeStamp = new SimpleDateFormat("yyyMMdd_HHmmss").format(new Date());
                String imageFileName = "JPEG_"+ timeStamp + "_";
                File storageDir = getExternalStoragePublicDirectory(Environment.DIRECTORY_PICTURES);
                try {
                    image = File.createTempFile(
                            imageFileName,
                            ".jpeg",
                            storageDir);
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
                File file = new File(image.getAbsolutePath());
                imgCap.takePicture(file, new ImageCapture.OnImageSavedListener()
                {
                    @Override
                    public void onImageSaved(@NonNull File file)
                    {
                        String msg = "Pic saved at "+ file.getAbsolutePath();
                        galleryAddPic(file.getAbsolutePath());
                        Toast.makeText(getBaseContext(), msg,Toast.LENGTH_LONG).show();
                    }

                    @Override
                    public void onError(@NonNull ImageCapture.UseCaseError useCaseError, @NonNull String message, @Nullable Throwable cause) {
                        String msg = "Pic saved at "+ message;
                        Toast.makeText(getBaseContext(), msg,Toast.LENGTH_LONG).show();
                        if (cause !=null){
                            cause.printStackTrace();

                            Toast.makeText(getBaseContext(), cause.toString(),Toast.LENGTH_LONG).show();
                        }
                    }
                });
            }
        });

        bindToLifecycle(this,preview, imgCap);

    }

    private void galleryAddPic(String  currentFilePath){
        Intent mediaScanIntent = new Intent(Intent.ACTION_MEDIA_SCANNER_SCAN_FILE);
        File file = new File (currentFilePath);
        Uri contentUri = Uri.fromFile(file);
        mediaScanIntent.setData(contentUri);
        this.sendBroadcast(mediaScanIntent);
        //Toast.makeText(getBaseContext(), "saved to gallery",Toast.LENGTH_LONG).show();
    }

    private void updateTransform(){
        Matrix mx = new Matrix();
        float w = textureView.getMeasuredWidth();
        float h = textureView.getMeasuredHeight();
        float cX = w / 2f;
        float cY = h / 2f;
        int rotationDgr;
        int rotation = (int)textureView.getRotation();
        switch (rotation){
            case Surface.ROTATION_0:
                rotationDgr = 0;
                break;
            case Surface.ROTATION_90:
                rotationDgr = 90;
                break;
            case Surface.ROTATION_180:
                rotationDgr = 180;
                break;
            case Surface.ROTATION_270:
                rotationDgr = 270;
                break;
            default: return;
        }
        mx.postRotate((float)rotationDgr, cX,cY);
        textureView.setTransform(mx);
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if(requestCode == REQUEST_CODE_PERMISSIONS){
            if (allPermissionsGranted()) {

                startCamera(CameraX.LensFacing.BACK);
            }
            else{
                Toast.makeText(this, "Permissions not granted by the user.", Toast.LENGTH_SHORT).show();
                finish();
            }
        }
    }

    private boolean allPermissionsGranted()
    {
        for(String permission : REQUIRED_PERMISSIONS)
        {
            if(ContextCompat.checkSelfPermission(this, permission)!= PackageManager.PERMISSION_GRANTED)
            {
                return false;
            }
        }
        return  true;
    }

    private boolean checkCameraHardware(Context context)
    {

        return context.getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA);
    }
    private void toggleFrontBackCamera()
    {

    }
}