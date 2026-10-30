param([string]$Record)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskDrawingDir=Split-Path ([System.Drawing.Bitmap].Assembly.Location)
$taskRefs=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Rectangle].Assembly.Location,(Join-Path $PSHOME 'ref/System.Runtime.dll'),(Join-Path $taskDrawingDir 'System.Private.Windows.GdiPlus.dll'),(Join-Path $taskDrawingDir 'System.Private.Windows.Core.dll'))
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
 public static Bitmap Normalize(string generated,int targetW,int targetH) {
  using(Bitmap g=new Bitmap(generated)){
   Rectangle gb=Bounds(g,128);
   double scale=Math.Min((targetW-2.0)/gb.Width,(targetH-2.0)/gb.Height);
   int w=Math.Max(1,(int)Math.Round(gb.Width*scale)),h=Math.Max(1,(int)Math.Round(gb.Height*scale));
   int left=(targetW-w)/2,top=targetH-h-1;
   Bitmap dst=new Bitmap(targetW,targetH,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){
    int sx=gb.X+Math.Min(gb.Width-1,(int)((x+.5)*gb.Width/w));
    int sy=gb.Y+Math.Min(gb.Height-1,(int)((y+.5)*gb.Height/h));
    Color color=g.GetPixel(sx,sy);
    if(color.A>=128)dst.SetPixel(left+x,top+y,Color.FromArgb(255,color.R,color.G,color.B));
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
Add-Type -TypeDefinition $taskCode -ReferencedAssemblies $taskRefs
$taskFiles=@(Get-Content -LiteralPath (Join-Path $Record '生成结果.json') -Raw | ConvertFrom-Json)
New-Item -ItemType Directory -Path (Join-Path $Record '成品'),(Join-Path $Record '生成原图') -Force | Out-Null
$taskItems=@()
$taskOverview=[System.Drawing.Bitmap]::new(1200,1200)
$taskG=[System.Drawing.Graphics]::FromImage($taskOverview)
$taskG.Clear([System.Drawing.Color]::FromArgb(62,66,72))
$taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$taskFont=[System.Drawing.Font]::new('Microsoft YaHei',8)
foreach($taskF in $taskFiles){
 $taskRaw=Join-Path (Join-Path $Record '生成原图') $taskF.Relative
 $taskDst=Join-Path (Join-Path $Record '成品') $taskF.Relative
 New-Item -ItemType Directory -Path (Split-Path $taskRaw),(Split-Path $taskDst) -Force | Out-Null
 Copy-Item -LiteralPath $taskF.Generated -Destination $taskRaw -Force
 $taskB=[SpritePixelBatch]::Normalize($taskRaw,$taskF.Width,$taskF.Height)
 [SpritePixelBatch]::Palette([System.Drawing.Bitmap[]]@($taskB),$taskF.Colors)
 $taskB.Save($taskDst,[System.Drawing.Imaging.ImageFormat]::Png)
 $taskColors=[System.Collections.Generic.HashSet[int]]::new()
 $taskOpaque=0;$taskTransparent=0;$taskPartial=0
 for($taskY=0;$taskY -lt $taskB.Height;$taskY++){for($taskX=0;$taskX -lt $taskB.Width;$taskX++){
  $taskC=$taskB.GetPixel($taskX,$taskY)
  if($taskC.A -eq 0){$taskTransparent++}elseif($taskC.A -eq 255){$taskOpaque++;[void]$taskColors.Add($taskC.ToArgb())}else{$taskPartial++}
 }}
 if($taskB.Width -ne $taskF.Width -or $taskB.Height -ne $taskF.Height -or $taskPartial -ne 0 -or $taskOpaque -lt 10 -or $taskTransparent -lt 10){throw "Invalid output: $($taskF.Name)"}
 $taskItems += [pscustomobject]@{Index=$taskF.Index;Name=$taskF.Name;Relative=$taskF.Relative;Width=$taskB.Width;Height=$taskB.Height;Colors=$taskColors.Count;Opaque=$taskOpaque;Transparent=$taskTransparent;PartialAlpha=$taskPartial;Sha256=(Get-FileHash -LiteralPath $taskDst).Hash}
 $taskI=$taskF.Index-1;$taskCX=($taskI%6)*200;$taskCY=[int][Math]::Floor($taskI/6)*200
 $taskScale=[Math]::Min(4,[Math]::Floor([Math]::Min(180.0/$taskB.Width,158.0/$taskB.Height)))
 $taskPW=$taskB.Width*$taskScale;$taskPH=$taskB.Height*$taskScale
 $taskG.DrawImage($taskB,[System.Drawing.Rectangle]::new([int]($taskCX+(200-$taskPW)/2),[int]($taskCY+(166-$taskPH)/2),$taskPW,$taskPH))
 $taskLabel=$taskF.Name -replace '.png$',''
 $taskG.DrawString($taskLabel,$taskFont,[System.Drawing.Brushes]::White,[single]($taskCX+4),[single]($taskCY+174))
 $taskB.Dispose()
}
$taskG.Dispose();$taskFont.Dispose()
$taskOverview.Save((Join-Path $Record '像素风总览.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskOverview.Dispose()
$taskItems | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $Record '尺寸透明色板检查.json') -Encoding utf8
[pscustomobject]@{Count=$taskItems.Count;AllSizesMatch=$true;AllTransparent=$true;AlphaBinary=$true} | ConvertTo-Json -Compress

