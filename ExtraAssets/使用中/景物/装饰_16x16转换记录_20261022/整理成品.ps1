param([string]$Record)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskDrawingDir=Split-Path ([System.Drawing.Bitmap].Assembly.Location)
$taskReferences=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Rectangle].Assembly.Location,(Join-Path $PSHOME 'ref/System.Runtime.dll'),(Join-Path $taskDrawingDir 'System.Private.Windows.GdiPlus.dll'),(Join-Path $taskDrawingDir 'System.Private.Windows.Core.dll'))
$taskCode=@'
using System;
using System.Drawing;
public static class SpritePixelBatch {
 public static Rectangle Bounds(Bitmap b,int threshold) {
  int x0=b.Width,y0=b.Height,x1=-1,y1=-1;
  for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++)if(b.GetPixel(x,y).A>=threshold){x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);}
  if(x1<0)throw new Exception("Empty sprite");
  return Rectangle.FromLTRB(x0,y0,x1+1,y1+1);
 }
 public static Bitmap Normalize(string generated,string original) {
  using(Bitmap g=new Bitmap(generated)) using(Bitmap s=new Bitmap(original)) {
   Rectangle gb=Bounds(g,128),sb=Bounds(s,32);
   double tw=14.0,th=14.0;
   double scale=Math.Min(tw/gb.Width,th/gb.Height);
   int w=Math.Max(1,(int)Math.Round(gb.Width*scale)),h=Math.Max(1,(int)Math.Round(gb.Height*scale));
   int left=(16-w)/2;
   int top=15-h;
   left=Math.Max(0,Math.Min(16-w,left));top=Math.Max(0,Math.Min(16-h,top));
   Bitmap dst=new Bitmap(16,16,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
   for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
    int sx=gb.X+Math.Min(gb.Width-1,(int)((x+0.5)*gb.Width/w));
    int sy=gb.Y+Math.Min(gb.Height-1,(int)((y+0.5)*gb.Height/h));
    Color c=g.GetPixel(sx,sy);
    if(c.A>=128)dst.SetPixel(left+x,top+y,Color.FromArgb(255,c.R,c.G,c.B));
   }
   return dst;
  }
 }
 public static void Palette(Bitmap[] images,int count) {
  long[] hist=new long[32768],rs=new long[32768],gs=new long[32768],bs=new long[32768];
  foreach(Bitmap b in images)for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){Color c=b.GetPixel(x,y);if(c.A==0)continue;int k=((c.R>>3)<<10)|((c.G>>3)<<5)|(c.B>>3);hist[k]++;rs[k]+=c.R;gs[k]+=c.G;bs[k]+=c.B;}
  double[] rr=new double[32768],gg=new double[32768],bb=new double[32768];
  int max=0;
  for(int k=0;k<32768;k++)if(hist[k]>0){rr[k]=(double)rs[k]/hist[k];gg[k]=(double)gs[k]/hist[k];bb[k]=(double)bs[k]/hist[k];if(hist[k]>hist[max])max=k;}
  double[] pr=new double[count],pg=new double[count],pb=new double[count];
  pr[0]=rr[max];pg[0]=gg[max];pb[0]=bb[max];
  for(int j=1;j<count;j++){double score=-1;int chosen=0;for(int k=0;k<32768;k++)if(hist[k]>0){double best=double.MaxValue;for(int i=0;i<j;i++){double dr=rr[k]-pr[i],dg=gg[k]-pg[i],db=bb[k]-pb[i];best=Math.Min(best,dr*dr+dg*dg+db*db);}double v=best*Math.Sqrt(hist[k]);if(v>score){score=v;chosen=k;}}pr[j]=rr[chosen];pg[j]=gg[chosen];pb[j]=bb[chosen];}
  int[] labels=new int[32768];
  for(int iter=0;iter<12;iter++){double[] ar=new double[count],ag=new double[count],ab=new double[count],an=new double[count];for(int k=0;k<32768;k++)if(hist[k]>0){double best=double.MaxValue;int pick=0;for(int j=0;j<count;j++){double dr=rr[k]-pr[j],dg=gg[k]-pg[j],db=bb[k]-pb[j];double d=dr*dr+dg*dg+db*db;if(d<best){best=d;pick=j;}}labels[k]=pick;ar[pick]+=rs[k];ag[pick]+=gs[k];ab[pick]+=bs[k];an[pick]+=hist[k];}for(int j=0;j<count;j++)if(an[j]>0){pr[j]=ar[j]/an[j];pg[j]=ag[j]/an[j];pb[j]=ab[j]/an[j];}}
  Color[] palette=new Color[count];for(int j=0;j<count;j++)palette[j]=Color.FromArgb(255,(int)Math.Round(pr[j]),(int)Math.Round(pg[j]),(int)Math.Round(pb[j]));
  foreach(Bitmap b in images)for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){Color c=b.GetPixel(x,y);if(c.A==0)continue;int k=((c.R>>3)<<10)|((c.G>>3)<<5)|(c.B>>3);b.SetPixel(x,y,palette[labels[k]]);}
 }
}
'@
Add-Type -TypeDefinition $taskCode -ReferencedAssemblies $taskReferences
$taskManifest=@(Get-Content -LiteralPath (Join-Path $Record '生成结果.json') -Raw | ConvertFrom-Json)
New-Item -ItemType Directory -Path (Join-Path $Record '成品'),(Join-Path $Record '生成原图') -Force | Out-Null
$taskContact=[System.Drawing.Bitmap]::new(1000,480)
$taskG=[System.Drawing.Graphics]::FromImage($taskContact)
$taskG.Clear([System.Drawing.Color]::FromArgb(64,67,73))
$taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$taskFont=[System.Drawing.Font]::new('Microsoft YaHei',12)
$taskCheck=@()
for($taskI=0;$taskI -lt $taskManifest.Count;$taskI++){
 $taskF=$taskManifest[$taskI]
 $taskRaw=Join-Path (Join-Path $Record '生成原图') $taskF.Name
 Copy-Item -LiteralPath $taskF.Generated -Destination $taskRaw
 $taskB=[SpritePixelBatch]::Normalize($taskRaw,(Join-Path (Join-Path $Record '原图备份') $taskF.Name))
 [SpritePixelBatch]::Palette([System.Drawing.Bitmap[]]@($taskB),12)
 $taskDst=Join-Path (Join-Path $Record '成品') $taskF.Name
 $taskB.Save($taskDst,[System.Drawing.Imaging.ImageFormat]::Png)
 $taskOpaque=0;$taskTransparent=0;$taskPartial=0;$taskColors=[System.Collections.Generic.HashSet[int]]::new()
 for($taskY=0;$taskY -lt 16;$taskY++){for($taskX=0;$taskX -lt 16;$taskX++){$taskC=$taskB.GetPixel($taskX,$taskY);if($taskC.A -eq 0){$taskTransparent++}elseif($taskC.A -eq 255){$taskOpaque++;[void]$taskColors.Add($taskC.ToArgb())}else{$taskPartial++}}}
 if($taskOpaque -lt 10 -or $taskTransparent -lt 30 -or $taskPartial -gt 0){throw "Invalid sprite $($taskF.Name)"}
 $taskCX=($taskI%5)*200;$taskCY=[Math]::Floor($taskI/5)*240
 $taskG.DrawImage($taskB,[System.Drawing.Rectangle]::new($taskCX+4,$taskCY+4,192,192))
 $taskG.DrawString($taskF.Name,$taskFont,[System.Drawing.Brushes]::White,[single]($taskCX+50),[single]($taskCY+208))
 $taskCheck += [pscustomobject]@{Name=$taskF.Name;Width=16;Height=16;Opaque=$taskOpaque;Transparent=$taskTransparent;PartialAlpha=$taskPartial;Colors=$taskColors.Count;Sha256=(Get-FileHash -LiteralPath $taskDst).Hash}
 $taskB.Dispose()
}
$taskG.Dispose();$taskFont.Dispose()
$taskContact.Save((Join-Path $Record '16x16像素风总览_12倍.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskContact.Dispose()
$taskCheck | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $Record '尺寸透明检查.json') -Encoding utf8
$taskCheck | ConvertTo-Json -Compress

